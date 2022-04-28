using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands.Entities;
using DSharpPlus.SlashCommands.EventArgs;
using Emzi0767.Utilities;

namespace DSharpPlus.SlashCommands
{
	public class SlashCommandsExtension : BaseExtension
	{
		private DiscordClient _client;

		private Dictionary<ulong, ApplicationCommand> _commands = new();
		private List<(ApplicationCommandBuilder Command, ulong GuildId)> _unsubmittedCommands = new();
		private SlashCommandsConfiguration _config;
		
		public Dictionary<ulong, ApplicationCommand> RegisteredCommands => _commands;

		public SlashCommandsExtension(SlashCommandsConfiguration config = null)
		{
			_config = config ?? new SlashCommandsConfiguration();
		}

		protected internal override void Setup(DiscordClient client)
		{
			if (_client != null)
				throw new InvalidOperationException("DONT RUN SETUP MORE THAN ONCE");

			_client = client;

			_client.Ready += OnReady;
			_client.GuildAvailable += OnGuildAvailable;

			_client.InteractionCreated += HandleSlashCommand;
			_client.ContextMenuInteractionCreated += HandleContextMenu;
			_client.InteractionCreated += HandleAutocomplete;
			
			_slashError = new AsyncEvent<SlashCommandsExtension, SlashCommandErrorEventArgs>("SLASHCOMMAND_ERRORED", TimeSpan.Zero, client.EventErrorHandler);
			_slashInvoked = new AsyncEvent<SlashCommandsExtension, SlashCommandInvokedEventArgs>("SLASHCOMMAND_RECEIVED", TimeSpan.Zero, client.EventErrorHandler);
			_slashExecuted = new AsyncEvent<SlashCommandsExtension, SlashCommandExecutedEventArgs>("SLASHCOMMAND_EXECUTED", TimeSpan.Zero, client.EventErrorHandler);
			_contextMenuErrored = new AsyncEvent<SlashCommandsExtension, ContextMenuErrorEventArgs>("CONTEXTMENU_ERRORED", TimeSpan.Zero, client.EventErrorHandler);
			_contextMenuExecuted = new AsyncEvent<SlashCommandsExtension, ContextMenuExecutedEventArgs>("CONTEXTMENU_EXECUTED", TimeSpan.Zero, client.EventErrorHandler);
			_contextMenuInvoked = new AsyncEvent<SlashCommandsExtension, ContextMenuInvokedEventArgs>("CONTEXTMENU_RECEIVED", TimeSpan.Zero, client.EventErrorHandler);
			_autocompleteErrored = new AsyncEvent<SlashCommandsExtension, AutocompleteErrorEventArgs>("AUTOCOMPLETE_ERRORED", TimeSpan.Zero, client.EventErrorHandler);
			_autocompleteExecuted = new AsyncEvent<SlashCommandsExtension, AutocompleteExecutedEventArgs>("AUTOCOMPLETE_EXECUTED", TimeSpan.Zero, client.EventErrorHandler);
			_appComRegFailed = new AsyncEvent<SlashCommandsExtension, ApplicationCommandRegisterFailedEventArgs>("APPLICATION_COMMAND_REGISTER_FAILED", TimeSpan.Zero, client.EventErrorHandler);
			_appComRegSuccess = new AsyncEvent<SlashCommandsExtension, ApplicationCommandRegisteredEventArgs>("APPLICATION_COMMAND_REGISTER_SUCCESS", TimeSpan.Zero, client.EventErrorHandler);
		}

		#region Reflection (the best part!)
		
		/// <summary>
		/// Register a command module.
		/// You have to run RefreshCommands if you add any commands after the Ready event
		/// </summary>
		/// <param name="module">The ApplicationCommandModule to add</param>
		/// <param name="guildId">The ID of the guild to add this command module to</param>
		public void RegisterCommands(Type module, ulong? guildId = null)
		{
			BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic;

			if (module.GetCustomAttribute<SlashCommandGroupAttribute>() is not null)
			{
				SlashCommandGroupAttribute gAttr = module.GetCustomAttribute<SlashCommandGroupAttribute>();
				if (gAttr == null) return; // shut up rider

				ApplicationCommandBuilder command = new ApplicationCommandBuilder(ApplicationCommandType.SlashCommand)
					.WithName(gAttr.Name)
					.WithDescription(gAttr.Description);

				if (gAttr.ApplyLocalization)
				{
					foreach (Localization language in _config.LocalizationWhitelist)
					{
						string name =
							_config.LocalizationProvider.GetLocalizedString(LocalizationContext.SlashCommandName,
								language, gAttr.Name);
						string description =
							_config.LocalizationProvider.GetLocalizedString(LocalizationContext.SlashCommandDescription,
								language, gAttr.Description);

						if (name != null) command.WithNameLocalization(language, name);
						if (description != null) command.WithDescriptionLocalization(language, description);
					}
				}

				if (module.GetNestedTypes(bindingFlags).Any(x => x.GetCustomAttribute<SlashCommandGroupAttribute>() is not null))
				{
					// two-level groups
					foreach (Type g in module.GetNestedTypes(bindingFlags))
					{
						SlashCommandGroupAttribute sGAttr = g.GetCustomAttribute<SlashCommandGroupAttribute>();
						if (sGAttr == null) continue; // shut up rider

						ApplicationCommandOptionBuilder group =
							new ApplicationCommandOptionBuilder(ApplicationCommandOptionType.SubCommandGroup)
								.WithName(sGAttr.Name)
								.WithDescription(sGAttr.Description);

						if (sGAttr.ApplyLocalization)
						{
							foreach (Localization language in _config.LocalizationWhitelist)
							{
								string name =
									_config.LocalizationProvider.GetLocalizedString(LocalizationContext.SlashCommandName,
										language, sGAttr.Name);
								string description =
									_config.LocalizationProvider.GetLocalizedString(LocalizationContext.SlashCommandDescription,
										language, sGAttr.Description);

								if (name != null) group.WithNameLocalization(language, name);
								if (description != null) group.WithDescriptionLocalization(language, description);
							}
						}

						foreach (MethodInfo method in g.GetMethods()
							.Where(x => x.GetCustomAttribute<SlashCommandAttribute>() != null))
						{
							SlashCommandAttribute attr = method.GetCustomAttribute<SlashCommandAttribute>();
							if (attr == null) continue; // shut up rider

							ApplicationCommandOptionBuilder subcommand =
								new ApplicationCommandOptionBuilder(ApplicationCommandOptionType.SubCommand)
									.WithName(attr.Name)
									.WithDescription(attr.Description)
									.WithMethod(method);

							if (attr.ApplyLocalization)
							{
								foreach (Localization language in _config.LocalizationWhitelist)
								{
									string name =
										_config.LocalizationProvider.GetLocalizedString(LocalizationContext.SlashCommandName,
											language, attr.Name);
									string description =
										_config.LocalizationProvider.GetLocalizedString(LocalizationContext.SlashCommandDescription,
											language, attr.Description);

									if (name != null) subcommand.WithNameLocalization(language, name);
									if (description != null) subcommand.WithDescriptionLocalization(language, description);
								}
							}

							if (method.GetParameters()[0].ParameterType != typeof(InteractionContext))
								throw new ArgumentException(
									"The first argument on slash commands must be InteractionContext");

							subcommand.AddOptions(ParseParameters(method.GetParameters().Skip(1)));

							group.AddOption(subcommand);
						}

						command.AddOption(group);
					}
				}

				// one-level commands
				foreach (MethodInfo method in module.GetMethods()
					.Where(x => x.GetCustomAttribute<SlashCommandAttribute>() != null)) 
					command.AddOption(ParseSubcommandMethod(method));

				RegisterCommand(command, guildId);
			}
			else if (module.GetNestedTypes(bindingFlags).Any(x => x.GetCustomAttribute<SlashCommandGroupAttribute>() is not null))
			{
				foreach (Type groupType in module.GetNestedTypes(bindingFlags)
					.Where(x => x.GetCustomAttribute<SlashCommandGroupAttribute>() is not null))
				{
					SlashCommandGroupAttribute gAttr = groupType.GetCustomAttribute<SlashCommandGroupAttribute>();
					if (gAttr == null) continue; // shut up rider

					ApplicationCommandBuilder command =
						new ApplicationCommandBuilder(ApplicationCommandType.SlashCommand)
							.WithName(gAttr.Name)
							.WithDescription(gAttr.Description);

					if (gAttr.ApplyLocalization)
					{
						foreach (Localization language in _config.LocalizationWhitelist)
						{
							string name =
								_config.LocalizationProvider.GetLocalizedString(LocalizationContext.SlashCommandName,
									language, gAttr.Name);
							string description =
								_config.LocalizationProvider.GetLocalizedString(LocalizationContext.SlashCommandDescription,
									language, gAttr.Description);

							if (name != null) command.WithNameLocalization(language, name);
							if (description != null) command.WithDescriptionLocalization(language, description);
						}
					}

					if (groupType.GetNestedTypes(bindingFlags)
							.Any(x => x.GetCustomAttribute<SlashCommandGroupAttribute>() is not null))
						// two-level groups
						foreach (Type g in groupType.GetNestedTypes(bindingFlags))
						{
							SlashCommandGroupAttribute sGAttr = g.GetCustomAttribute<SlashCommandGroupAttribute>();
							if (sGAttr == null) continue; // shut up rider

							ApplicationCommandOptionBuilder group =
								new ApplicationCommandOptionBuilder(ApplicationCommandOptionType.SubCommandGroup)
									.WithName(sGAttr.Name)
									.WithDescription(sGAttr.Description);

							if (sGAttr.ApplyLocalization)
							{
								foreach (Localization language in _config.LocalizationWhitelist)
								{
									string name =
										_config.LocalizationProvider.GetLocalizedString(LocalizationContext.SlashCommandName,
											language, sGAttr.Name);
									string description =
										_config.LocalizationProvider.GetLocalizedString(LocalizationContext.SlashCommandDescription,
											language, sGAttr.Description);

									if (name != null) group.WithNameLocalization(language, name);
									if (description != null) group.WithDescriptionLocalization(language, description);
								}
							}

							foreach (MethodInfo method in g.GetMethods()
								.Where(x => x.GetCustomAttribute<SlashCommandAttribute>() != null))
								group.AddOption(ParseSubcommandMethod(method));

							command.AddOption(group);
						}
					else
						// one-level groups
						foreach (MethodInfo method in groupType.GetMethods()
							.Where(x => x.GetCustomAttribute<SlashCommandAttribute>() != null))
							command.AddOption(ParseSubcommandMethod(method));

					RegisterCommand(command, guildId);
				}
			}
			else
				// normal commands
				foreach (MethodInfo method in module.GetMethods()
					.Where(x => x.GetCustomAttribute<SlashCommandAttribute>() != null))
					ParseCommandMethod(method, guildId ?? 0);

			// context menus
			foreach (MethodInfo method in module.GetMethods()
				.Where(x => x.GetCustomAttribute<ContextMenuAttribute>() != null))
			{
				ContextMenuAttribute attr = method.GetCustomAttribute<ContextMenuAttribute>();

				if (attr == null) continue; // shut up rider

				ApplicationCommandBuilder command = new ApplicationCommandBuilder(attr.Type)
					.WithName(attr.Name)
					.WithDefaultPermission(attr.DefaultPermission)
					.WithDefaultPermissions(attr.DefaultPermissions)
					.WithMethod(method);

				if (attr.ApplyLocalization)
				{
					foreach (Localization language in _config.LocalizationWhitelist)
					{
						string name =
							_config.LocalizationProvider.GetLocalizedString(LocalizationContext.ContextMenuName,
								language, attr.Name);

						if (name != null) command.WithNameLocalization(language, name);
					}
				}

				if (method.GetParameters().Length == 1 &&
				    method.GetParameters()[0].ParameterType != typeof(ContextMenuContext))
					throw new ArgumentException("The only argument on context menus must be a ContextMenuContext");

				RegisterCommand(command, guildId);
			}
		}

		private void ParseCommandMethod(MethodInfo method, ulong guildId)
		{
			SlashCommandAttribute attr = method.GetCustomAttribute<SlashCommandAttribute>();

			if (attr == null) return; // shut up rider

			ApplicationCommandBuilder command =
				new ApplicationCommandBuilder(ApplicationCommandType.SlashCommand)
					.WithName(attr.Name)
					.WithDescription(attr.Description)
					.WithMethod(method)
					.WithDefaultPermission(attr.DefaultPermission)
					.WithDefaultPermissions(attr.DefaultPermissions)
					.WithGuildOnly(attr.GuildOnly);

			if (attr.ApplyLocalization)
			{
				foreach (Localization language in _config.LocalizationWhitelist)
				{
					string name =
						_config.LocalizationProvider.GetLocalizedString(LocalizationContext.SlashCommandName,
							language, attr.Name);
					string description =
						_config.LocalizationProvider.GetLocalizedString(LocalizationContext.SlashCommandDescription,
							language, attr.Description);

					if (name != null) command.WithNameLocalization(language, name);
					if (description != null) command.WithDescriptionLocalization(language, description);
				}
			}

			if (method.GetParameters()[0].ParameterType != typeof(InteractionContext))
				throw new ArgumentException("The first argument on slash commands must be InteractionContext");

			command.AddOptions(ParseParameters(method.GetParameters().Skip(1)));

			RegisterCommand(command, guildId);
		}

		private ApplicationCommandOptionBuilder ParseSubcommandMethod(MethodInfo method)
		{
			SlashCommandAttribute attr = method.GetCustomAttribute<SlashCommandAttribute>();

			if (attr == null) return null; // shut up rider (this should never happen so its fine :thumbs_up:)

			ApplicationCommandOptionBuilder subcommand =
				new ApplicationCommandOptionBuilder(ApplicationCommandOptionType.SubCommand)
					.WithName(attr.Name)
					.WithDescription(attr.Description)
					.WithMethod(method);

			if (attr.ApplyLocalization)
			{
				foreach (Localization language in _config.LocalizationWhitelist)
				{
					string name =
						_config.LocalizationProvider.GetLocalizedString(LocalizationContext.SlashCommandName,
							language, attr.Name);
					string description =
						_config.LocalizationProvider.GetLocalizedString(LocalizationContext.SlashCommandDescription,
							language, attr.Description);

					if (name != null) subcommand.WithNameLocalization(language, name);
					if (description != null) subcommand.WithDescriptionLocalization(language, description);
				}
			}

			if (method.GetParameters()[0].ParameterType != typeof(InteractionContext))
				throw new ArgumentException(
					"The first argument on slash commands must be InteractionContext");

			subcommand.AddOptions(ParseParameters(method.GetParameters().Skip(1)));

			return subcommand;
		}

		#endregion
		
		#region Registering

		/// <summary>
		/// Register a command with a ApplicationCommandBuilder.
		/// You have to run RefreshCommands if you add any commands after the Ready event
		/// </summary>
		/// <param name="command">ApplicationCommandBuilder to add</param>
		/// <param name="guildId">The ID of the guild to add this command to</param>
		public void RegisterCommand(ApplicationCommandBuilder command, ulong? guildId = null) =>
			_unsubmittedCommands.Add((command, guildId ?? 0));

		/// <summary>
		/// Register a command module.
		/// You have to run RefreshCommands if you add any commands after the Ready event
		/// </summary>
		/// <param name="guildId">The ID of the guild to add this command module to</param>
		public void RegisterCommands<T>(ulong? guildId = null) => RegisterCommands(typeof(T), guildId);

		/// <summary>
		/// Automatically find and register all command modules from an assembly.
		/// You have to run RefreshCommands if you add any commands after the Ready event
		/// </summary>
		/// <param name="assembly">The assembly to find and add the modules from</param>
		/// <param name="guildId">The ID of the guild to add this command modules to</param>
		public void RegisterCommands(Assembly assembly, ulong? guildId = null)
		{
			IEnumerable<Type> types = assembly.ExportedTypes.Where(xt =>
				typeof(ApplicationCommandModule).IsAssignableFrom(xt) &&
				!xt.GetTypeInfo().IsNested);

			foreach (Type xt in types)
				RegisterCommands(xt, guildId);
		}

		/// <summary>
		/// Resubmit all slash commands to Discord.
		/// </summary>
		public async Task RefreshCommands()
		{
			await PushCommands();
			foreach (ulong guildId in _client.Guilds.Keys) await PushCommands(guildId);
		}

		/// <summary>
		/// Resubmit all slash commands for a specific guild to Discord.
		/// </summary>
		/// <param name="guildId">The ID of the guild. Use <code>0</code> to only resubmit global commands</param>
		public async Task RefreshCommands(ulong guildId) => await PushCommands(guildId);

		private async Task OnReady(DiscordClient sender, ReadyEventArgs args) => await PushCommands();

		private async Task OnGuildAvailable(DiscordClient sender, GuildCreateEventArgs args) => await PushCommands(args.Guild.Id);

#pragma warning disable 1998
#pragma warning disable 4014
		private async Task PushCommands(ulong? guildId = 0)
		{
			guildId ??= 0;
			Task.Run(async () =>
			{
				try
				{
					ApplicationCommandBuilder[] commands =
						_unsubmittedCommands.Where(x => x.GuildId == guildId).Select(x => x.Command).ToArray();
					if (commands.Length == 0)
					{
						_appComRegSuccess.InvokeAsync(this, new ApplicationCommandRegisteredEventArgs
						{
							GuildId = guildId == 0 ? null : guildId,
							Commands = Array.Empty<DiscordApplicationCommand>()
						});

						return;
					}
					IEnumerable<DiscordApplicationCommand> dcCommands;
					if (guildId == 0)
						dcCommands =
							await _client.BulkOverwriteGlobalApplicationCommandsAsync(commands.Select(x => x.Build()));
					else
						dcCommands = await _client.BulkOverwriteGuildApplicationCommandsAsync(guildId ?? 0,
							commands.Select(x => x.Build()));

					foreach ((ulong key, ApplicationCommand _) in _commands.Where(x => x.Value.GuildId == guildId)
						.ToArray())
						_commands.Remove(key);

					foreach (DiscordApplicationCommand dac in dcCommands)
						_commands.Add(dac.Id,
							new ApplicationCommand(commands.First(x => x.Name == dac.Name), guildId ?? 0));

					_appComRegSuccess.InvokeAsync(this, new ApplicationCommandRegisteredEventArgs
					{
						GuildId = guildId == 0 ? null : guildId,
						Commands = dcCommands
					});
				}
				catch (Exception e)
				{
					_appComRegFailed.InvokeAsync(this, new ApplicationCommandRegisterFailedEventArgs
					{
						Exception = e,
						GuildId = guildId == 0 ? null : guildId
					});
				}
			});
		}
#pragma warning restore 1998
#pragma warning restore 4014

		#endregion

		#region Handling

		// ReSharper disable AssignNullToNotNullAttribute
		// ReSharper disable PossibleNullReferenceException
		private Task HandleSlashCommand(DiscordClient sender, InteractionCreateEventArgs e)
		{
			if (e.Interaction.Type != InteractionType.ApplicationCommand) return Task.CompletedTask;
			Task.Run(async () =>
			{
				string methodName = e.Interaction.Data.Options?.First().Type switch
				{
					ApplicationCommandOptionType.SubCommand => e.Interaction.Data.Options?.First().Name,
					ApplicationCommandOptionType.SubCommandGroup =>
						$"{e.Interaction.Data.Options?.First().Name} {e.Interaction.Data.Options?.First().Options.First().Name}",
					_ => string.Empty
				};

				IEnumerable<object> options = await ParseOptions(_commands[e.Interaction.Data.Id].Methods[methodName],
					(e.Interaction.Data.Options?.First().Type switch
					{
						ApplicationCommandOptionType.SubCommand => e.Interaction.Data.Options?.First().Options,
						ApplicationCommandOptionType.SubCommandGroup => e.Interaction.Data.Options?.First().Options
							.First().Options,
						_ => e.Interaction.Data.Options
					} ?? Array.Empty<DiscordInteractionDataOption>()).ToArray(), e.Interaction.Data.Resolved);

				// SORRY

				InteractionContext ctx = new()
				{
					Channel = e.Interaction.Channel,
					Client = sender,
					Guild = e.Interaction.Guild,
					Interaction = e.Interaction,
					Services = _config.Services,
					Token = e.Interaction.Token,
					Type = e.Interaction.Data.Type,
					User = e.Interaction.User,
					CommandName = e.Interaction.Data.Name,
					InteractionId = e.Interaction.Id,
					ResolvedChannelMentions = e.Interaction.Data.Resolved?.Channels?.Values.ToList(),
					ResolvedUserMentions = e.Interaction.Data.Resolved?.Users?.Values.ToList(),
					ResolvedRoleMentions = e.Interaction.Data.Resolved?.Roles?.Values.ToList(),
					SlashCommandsExtension = this
				};

				List<object> argumentsList = new()
				{
					ctx
				};

				argumentsList.AddRange(options);

				try
				{
					await _slashInvoked.InvokeAsync(this, new SlashCommandInvokedEventArgs()
					{
						Context = ctx
					});

					MethodInfo method = _commands[e.Interaction.Data.Id].Methods[methodName];
					ApplicationCommandModule instance =
						(ApplicationCommandModule)InstanceCreator.CreateInstance(method.DeclaringType, _config.Services);

					await PreExecutionChecks(method, ctx);
					
					bool shouldRun = await instance.BeforeSlashExecutionAsync(ctx);

					if (shouldRun)
					{
						await (Task)method.Invoke(instance, argumentsList.ToArray());
						await instance.AfterSlashExecutionAsync(ctx);
						await _slashExecuted.InvokeAsync(this, new SlashCommandExecutedEventArgs()
						{
							Context = ctx
						});
					}
				}
				catch (Exception exception)
				{
					await _slashError.InvokeAsync(this, new SlashCommandErrorEventArgs()
					{
						Context = ctx,
						Exception = exception
					});
				}
			});
			return Task.CompletedTask;
		}

		private Task HandleContextMenu(DiscordClient sender, ContextMenuInteractionCreateEventArgs e)
		{
			if (e.Interaction.Type != InteractionType.ApplicationCommand) return Task.CompletedTask;
			Task.Run(async () =>
			{
				ContextMenuContext ctx = new()
				{
					Channel = e.Interaction.Channel,
					Client = sender,
					Guild = e.Interaction.Guild,
					Interaction = e.Interaction,
					Services = _config.Services,
					Token = e.Interaction.Token,
					Type = e.Interaction.Data.Type,
					User = e.Interaction.User,
					CommandName = e.Interaction.Data.Name,
					InteractionId = e.Interaction.Id,
					TargetMessage = e.TargetMessage,
					TargetUser = e.TargetUser,
					SlashCommandsExtension = this
				};

				MethodInfo method = _commands[e.Interaction.Data.Id].Methods[string.Empty];
				ApplicationCommandModule instance =
					(ApplicationCommandModule)InstanceCreator.CreateInstance(method.DeclaringType, _config.Services);

				try
				{
					await _contextMenuInvoked.InvokeAsync(this, new ContextMenuInvokedEventArgs()
					{
						Context = ctx
					});

					await PreExecutionChecks(method, ctx);
					
					bool shouldRun = await instance.BeforeContextMenuExecutionAsync(ctx);

					if (shouldRun)
					{
						await (Task)method.Invoke(instance, new object[] { ctx });
						await instance.AfterContextMenuExecutionAsync(ctx);
						await _contextMenuExecuted.InvokeAsync(this, new ContextMenuExecutedEventArgs()
						{
							Context = ctx
						});
					}
				}
				catch (Exception exception)
				{
					await _contextMenuErrored.InvokeAsync(this, new ContextMenuErrorEventArgs()
					{
						Context = ctx,
						Exception = exception
					});
				}
			});
			return Task.CompletedTask;
		}

		private Task HandleAutocomplete(DiscordClient sender, InteractionCreateEventArgs e)
		{
			if (e.Interaction.Type != InteractionType.AutoComplete) return Task.CompletedTask;
			Task.Run(async () =>
			{
				DiscordInteractionDataOption[] options = (e.Interaction.Data.Options?.First().Type switch
				{
					ApplicationCommandOptionType.SubCommand => e.Interaction.Data.Options?.First().Options,
					ApplicationCommandOptionType.SubCommandGroup => e.Interaction.Data.Options?.First().Options
						.First().Options,
					_ => e.Interaction.Data.Options
				})?.ToArray() ?? Array.Empty<DiscordInteractionDataOption>();

				DiscordInteractionDataOption focusedOption = options.First(x => x.Focused);

				AutocompleteContext ctx = new()
				{
					Channel = e.Interaction.Channel,
					Client = sender,
					Guild = e.Interaction.Guild,
					Interaction = e.Interaction,
					Services = _config.Services,
					Options = options.ToList(),
					User = e.Interaction.User,
					FocusedOption = focusedOption,
					SlashCommandsExtension = this
				};

				MethodInfo method = (e.Interaction.Data.Options?.First().Type switch
				{
				    ApplicationCommandOptionType.SubCommand => _commands[e.Interaction.Data.Id].AutocompleteMethods[$"{e.Interaction.Data.Options?.First().Name} {focusedOption.Name}"],
				    ApplicationCommandOptionType.SubCommandGroup => _commands[e.Interaction.Data.Id].AutocompleteMethods[$"{e.Interaction.Data.Options?.First().Name} {e.Interaction.Data.Options?.First().Options?.First().Name} {focusedOption.Name}"],
				    _ => _commands[e.Interaction.Data.Id].AutocompleteMethods[focusedOption.Name],
				});
				IAutocompleteProvider instance =
					(IAutocompleteProvider)InstanceCreator.CreateInstance(method.DeclaringType, _config.Services);

				try
				{
					IEnumerable<DiscordAutoCompleteChoice> choices = await instance.Provider(ctx);

					await e.Interaction.CreateResponseAsync(InteractionResponseType.AutoCompleteResult,
						new DiscordInteractionResponseBuilder().AddAutoCompleteChoices(choices));
					await _autocompleteExecuted.InvokeAsync(this, new AutocompleteExecutedEventArgs()
					{
						Context = ctx
					});	
				}
				catch (Exception exception)
				{
					await _autocompleteErrored.InvokeAsync(this, new AutocompleteErrorEventArgs()
					{
						Context = ctx,
						Exception = exception
					});
				}
			});
			return Task.CompletedTask;
		}

		private async Task PreExecutionChecks(MethodInfo method, BaseContext context)
		{
			//Gets all attributes from parent classes as well and stuff
			List<SlashCheckBaseAttribute> attributes = new List<SlashCheckBaseAttribute>();
			attributes.AddRange(method.GetCustomAttributes<SlashCheckBaseAttribute>(true));
			attributes.AddRange(method.DeclaringType.GetCustomAttributes<SlashCheckBaseAttribute>());
			if (method.DeclaringType.DeclaringType != null)
			{
				attributes.AddRange(method.DeclaringType.DeclaringType.GetCustomAttributes<SlashCheckBaseAttribute>());
				if (method.DeclaringType.DeclaringType.DeclaringType != null)
					attributes.AddRange(method.DeclaringType.DeclaringType.DeclaringType
						.GetCustomAttributes<SlashCheckBaseAttribute>());
			}

			Dictionary<SlashCheckBaseAttribute, bool> dict = new Dictionary<SlashCheckBaseAttribute, bool>();
			foreach (SlashCheckBaseAttribute att in attributes)
			{
				//Runs the check and adds the result to a list
				bool result = await att.ExecuteChecksAsync(context);
				dict.Add(att, result);
			}

			//Checks if any failed, and throws an exception
			if (dict.Any(x => x.Value == false))
				throw new SlashExecutionChecksFailedException
					{ FailedChecks = dict.Where(x => x.Value == false).Select(x => x.Key).ToList() };
		}
		// ReSharper restore AssignNullToNotNullAttribute
		// ReSharper restore PossibleNullReferenceException

		private ApplicationCommandOptionBuilder[] ParseParameters(IEnumerable<ParameterInfo> parameters)
		{
			List<ApplicationCommandOptionBuilder> res = new();
			foreach (ParameterInfo parameterInfo in parameters)
			{
				ApplicationCommandOptionType type;
				OptionAttribute optionAttr = parameterInfo.GetCustomAttribute<OptionAttribute>();
				// here's the part that everyone hates
				if (parameterInfo.ParameterType == typeof(string))
					type = ApplicationCommandOptionType.String;
				else if (parameterInfo.ParameterType == typeof(long))
					type = ApplicationCommandOptionType.Integer;
				else if (parameterInfo.ParameterType == typeof(bool))
					type = ApplicationCommandOptionType.Boolean;
				else if (parameterInfo.ParameterType == typeof(double))
					type = ApplicationCommandOptionType.Number;
				else if (parameterInfo.ParameterType == typeof(DiscordUser))
					type = ApplicationCommandOptionType.User;
				else if (parameterInfo.ParameterType == typeof(DiscordChannel))
					type = ApplicationCommandOptionType.Channel;
				else if (parameterInfo.ParameterType == typeof(DiscordRole))
					type = ApplicationCommandOptionType.Role;
				else if (parameterInfo.ParameterType == typeof(SnowflakeObject))
					type = ApplicationCommandOptionType.Mentionable;
				else if (parameterInfo.ParameterType.IsEnum)
					type = ApplicationCommandOptionType.String;
				else if (parameterInfo.ParameterType == typeof(DiscordAttachment))
					type = (ApplicationCommandOptionType)11;
				else
					throw new ArgumentOutOfRangeException(nameof(parameterInfo.ParameterType),
						parameterInfo.ParameterType,
						"Slash command option types can be one of string, long, bool, double, DiscordUser, DiscordChannel, DiscordRole, DiscordAttachment, SnowflakeObject, Enum");

				ApplicationCommandOptionBuilder option = new ApplicationCommandOptionBuilder(type)
					.WithName(optionAttr?.Name)
					.WithDescription(optionAttr?.Description)
					.IsRequired(!parameterInfo.IsOptional);

				if (optionAttr.ApplyLocalization)
				{
					foreach (Localization language in _config.LocalizationWhitelist)
					{
						string name =
							_config.LocalizationProvider.GetLocalizedString(LocalizationContext.SlashCommandOptionName,
								language, optionAttr.Name);
						string description =
							_config.LocalizationProvider.GetLocalizedString(LocalizationContext.SlashCommandOptionDescription,
								language, optionAttr.Description);

						if (name != null) option.WithNameLocalization(language, name);
						if (description != null) option.WithDescriptionLocalization(language, description);
					}
				}

				Type enumType = parameterInfo.ParameterType;
				if (enumType.IsEnum)
				{
					string[] names = enumType.GetEnumNames();
					Array values = enumType.GetEnumValues();

					for (int i = 0; i < names.Length; i++)
					{
						string enumName = names[i];
						object value = values.GetValue(i);
						
						MemberInfo memberInfo =    
							enumType.GetMember(value.ToString()).First();    
						
						ChoiceNameAttribute nameAttr =    
							memberInfo.GetCustomAttribute<ChoiceNameAttribute>();

						option.AddChoice(nameAttr is not null ? nameAttr.Name : enumName, enumName);
					}
				}
				
				foreach (Attribute attribute in parameterInfo.GetCustomAttributes())
					switch (attribute)
					{
						case AutocompleteAttribute autocomplete:
							option.WithAutocomplete(autocomplete.Provider.GetMethod("Provider"));
							// this is stupid, fix later
							break;
						case ChannelTypesAttribute cType:
							option.WithChannelTypes(cType.ChannelTypes.ToArray());
							break;
						case ChoiceAttribute choice:
							option.AddChoice(choice.Name, choice.Value);
							break;
						case MinimumAttribute min:
							option.WithMinMaxValue((long)min.Value, option.MaxValue);
							break;
						case MaximumAttribute max:
							option.WithMinMaxValue(option.MinValue, (long)max.Value);
							break;
					}

				res.Add(option);
			}

			return res.ToArray();
		}

		private async Task<IEnumerable<object>> ParseOptions(MethodInfo info, DiscordInteractionDataOption[] options,
			DiscordInteractionResolvedCollection resolved)
		{
			List<object> objects = new();

			foreach (ParameterInfo param in info.GetParameters().Skip(1))
			{
				string paramName = param.GetCustomAttribute<OptionAttribute>()?.Name;
				DiscordInteractionDataOption option = options.FirstOrDefault(x => x.Name == paramName);

				if (option is null && param.HasDefaultValue)
					objects.Add(param.DefaultValue);
				else if (param.ParameterType.IsEnum)
					objects.Add(Enum.Parse(param.ParameterType, option?.Value as string ?? string.Empty));
				else
					objects.Add(await ConvertOptionToType(option, param.ParameterType, resolved));
			}

			return objects;
		}

		private async Task<object> ConvertOptionToType(DiscordInteractionDataOption option, Type type,
			DiscordInteractionResolvedCollection resolved)
		{
			// here's another part that everyone hates
			if (option == null)
				return null;
			if (type == typeof(string))
				return (string)option.Value;
			if (type == typeof(long))
				return (long)option.Value;
			if (type == typeof(bool))
				return (bool)option.Value;
			if (type == typeof(double))
				return (double)option.Value;
			if (type == typeof(DiscordUser))
			{
				ulong id = (ulong)option.Value;
				if (resolved.Members.TryGetValue(id, out DiscordMember m))
					return m;
				if (resolved.Users.TryGetValue(id, out DiscordUser u))
					return u;

				return await _client.GetUserAsync(id);
			}

			if (type == typeof(DiscordChannel))
			{
				ulong id = (ulong)option.Value;
				if (resolved.Channels.TryGetValue(id, out DiscordChannel c))
					return c;

				return await _client.GetChannelAsync(id);
			}

			if (type == typeof(DiscordRole))
			{
				ulong id = (ulong)option.Value;
				return resolved.Roles.TryGetValue(id, out DiscordRole r) ? r : null;
			}

			if (type == typeof(SnowflakeObject))
			{
				ulong id = (ulong)option.Value;
				//Checks through resolved
				if (resolved.Roles != null && resolved.Roles.TryGetValue(id, out DiscordRole role))
					return role;
				if (resolved.Members != null && resolved.Members.TryGetValue(id, out DiscordMember member))
					return member;
				if (resolved.Users != null && resolved.Users.TryGetValue(id, out DiscordUser user))
					return user;
				throw new ArgumentException("Error resolving mentionable option.");
			}

			if (type == typeof(DiscordAttachment))
			{
				ulong id = (ulong)option.Value;
				//Checks through resolved
				return resolved.Attachments.TryGetValue(id, out DiscordAttachment a) ? a : null;
			}

			throw new ArgumentOutOfRangeException(nameof(type), type,
				"Slash command option types can be one of string, long, bool, double, DiscordUser, DiscordChannel, DiscordRole, SnowflakeObject, Enum");
		}

		#endregion

		#region Events
		
		public event AsyncEventHandler<SlashCommandsExtension, SlashCommandErrorEventArgs> SlashCommandErrored
		{
			add => _slashError.Register(value);
			remove { _slashError.Unregister(value); }
		}

		private AsyncEvent<SlashCommandsExtension, SlashCommandErrorEventArgs> _slashError;

		public event AsyncEventHandler<SlashCommandsExtension, SlashCommandInvokedEventArgs> SlashCommandInvoked
		{
			add => _slashInvoked.Register(value);
			remove => _slashInvoked.Unregister(value);
		}

		private AsyncEvent<SlashCommandsExtension, SlashCommandInvokedEventArgs> _slashInvoked;

		public event AsyncEventHandler<SlashCommandsExtension, SlashCommandExecutedEventArgs> SlashCommandExecuted
		{
			add => _slashExecuted.Register(value);
			remove => _slashExecuted.Unregister(value);
		}

		private AsyncEvent<SlashCommandsExtension, SlashCommandExecutedEventArgs> _slashExecuted;

		public event AsyncEventHandler<SlashCommandsExtension, ContextMenuErrorEventArgs> ContextMenuErrored
		{
			add => _contextMenuErrored.Register(value);
			remove => _contextMenuErrored.Unregister(value);
		}

		private AsyncEvent<SlashCommandsExtension, ContextMenuErrorEventArgs> _contextMenuErrored;

		public event AsyncEventHandler<SlashCommandsExtension, ContextMenuInvokedEventArgs> ContextMenuInvoked
		{
			add => _contextMenuInvoked.Register(value);
			remove => _contextMenuInvoked.Unregister(value);
		}

		private AsyncEvent<SlashCommandsExtension, ContextMenuInvokedEventArgs> _contextMenuInvoked;

		public event AsyncEventHandler<SlashCommandsExtension, ContextMenuExecutedEventArgs> ContextMenuExecuted
		{
			add => _contextMenuExecuted.Register(value);
			remove => _contextMenuExecuted.Unregister(value);
		}

		private AsyncEvent<SlashCommandsExtension, ContextMenuExecutedEventArgs> _contextMenuExecuted;

		public event AsyncEventHandler<SlashCommandsExtension, AutocompleteErrorEventArgs> AutocompleteErrored
		{
			add => _autocompleteErrored.Register(value);
			remove => _autocompleteErrored.Register(value);
		}

		private AsyncEvent<SlashCommandsExtension, AutocompleteErrorEventArgs> _autocompleteErrored;

		public event AsyncEventHandler<SlashCommandsExtension, AutocompleteExecutedEventArgs> AutocompleteExecuted
		{
			add => _autocompleteExecuted.Register(value);
			remove => _autocompleteExecuted.Register(value);
		}

		private AsyncEvent<SlashCommandsExtension, AutocompleteExecutedEventArgs> _autocompleteExecuted;
		
		public event AsyncEventHandler<SlashCommandsExtension, ApplicationCommandRegisterFailedEventArgs> ApplicationCommandRegisterFailed
		{
			add => _appComRegFailed.Register(value);
			remove { _appComRegFailed.Unregister(value); }
		}

		private AsyncEvent<SlashCommandsExtension, ApplicationCommandRegisterFailedEventArgs> _appComRegFailed;

		public event AsyncEventHandler<SlashCommandsExtension, ApplicationCommandRegisteredEventArgs> ApplicationCommandRegistered
		{
			add => _appComRegSuccess.Register(value);
			remove => _appComRegSuccess.Unregister(value);
		}

		private AsyncEvent<SlashCommandsExtension, ApplicationCommandRegisteredEventArgs> _appComRegSuccess;

		#endregion
	}
}

// i take no responsibility for this
// if anything goes wrong, blame velvet lmao
