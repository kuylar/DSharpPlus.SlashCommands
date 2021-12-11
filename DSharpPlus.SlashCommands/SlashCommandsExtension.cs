using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands.Entities;

namespace DSharpPlus.SlashCommands
{
	public class SlashCommandsExtension : BaseExtension
	{
		private DiscordClient _client;

		private Dictionary<ulong, ApplicationCommand> _commands;

		private List<(ApplicationCommandBuilder Command, ulong GuildId)> _rawCommands;
		// todo: delete this shit i literally wrote this entire thing because the old extension was using this shit

		protected internal override void Setup(DiscordClient client)
		{
			if (_client != null)
				throw new InvalidOperationException("DONT RUN SETUP MORE THAN ONCE");

			_client = client;
			_rawCommands = new List<(ApplicationCommandBuilder, ulong)>();
			_commands = new Dictionary<ulong, ApplicationCommand>();

			_client.Ready += OnReady;
			_client.GuildAvailable += OnGuildAvailable;

			_client.InteractionCreated += HandleSlashCommand;
			_client.ContextMenuInteractionCreated += HandleContextMenu;
			_client.InteractionCreated += HandleAutocomplete;
		}

		public void RegisterCommand(ApplicationCommandBuilder command, ulong? guildId) =>
			_rawCommands.Add((command, guildId ?? 0));

		public void RegisterCommands<T>(ulong? guildId)
		{
			// normal commands
			foreach (MethodInfo method in typeof(T).GetMethods()
				.Where(x => x.GetCustomAttribute<SlashCommandAttribute>() != null))
			{
				SlashCommandAttribute attr = method.GetCustomAttribute<SlashCommandAttribute>();

				if (attr == null) return; // shut up rider

				ApplicationCommandBuilder command = new ApplicationCommandBuilder(ApplicationCommandType.SlashCommand)
					.WithName(attr.Name)
					.WithDescription(attr.Description)
					.WithDefaultPermission(attr.DefaultPermission)
					.WithMethod(method);

				if (method.GetParameters()[0].ParameterType != typeof(InteractionContext))
					throw new ArgumentException("The first argument on slash commands must be InteractionContext");

				foreach (ParameterInfo parameterInfo in method.GetParameters().Skip(1))
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
					else if (parameterInfo.ParameterType == typeof(Enum))
						throw new ArgumentException("Enums are not supported yet"); // todo
					else
						throw new ArgumentOutOfRangeException(nameof(parameterInfo.ParameterType), parameterInfo.ParameterType,
							"Slash command option types can be one of string, long, bool, double, DiscordUser, DiscordChannel, DiscordRole, SnowflakeObject, Enum");

					ApplicationCommandOptionBuilder option = new ApplicationCommandOptionBuilder(type)
						.WithName(optionAttr?.Name)
						.WithDescription(optionAttr?.Description)
						.IsRequired(!parameterInfo.IsOptional);

					foreach (Attribute attribute in parameterInfo.GetCustomAttributes())
					{
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
					}
					command.AddOption(option);
				}

				RegisterCommand(command, guildId);
			}
			
			// context menus
			foreach (MethodInfo method in typeof(T).GetMethods()
				.Where(x => x.GetCustomAttribute<ContextMenuAttribute>() != null))
			{
				ContextMenuAttribute attr = method.GetCustomAttribute<ContextMenuAttribute>();

				if (attr == null) return; // shut up rider

				ApplicationCommandBuilder command = new ApplicationCommandBuilder(attr.Type)
					.WithName(attr.Name)
					.WithDefaultPermission(attr.DefaultPermission)
					.WithMethod(method);

				if (method.GetParameters().Length == 1 && method.GetParameters()[0].ParameterType != typeof(ContextMenuContext))
					throw new ArgumentException("The only argument on context menus must be a ContextMenuContext");

				RegisterCommand(command, guildId);
			}
		}

		private async Task OnReady(DiscordClient sender, ReadyEventArgs args)
		{
			ApplicationCommandBuilder[] commands =
				_rawCommands.Where(x => x.GuildId == 0).Select(x => x.Command).ToArray();
			IEnumerable<DiscordApplicationCommand> dcCommands =
				await _client.BulkOverwriteGlobalApplicationCommandsAsync(commands.Select(x => x.Build()));

			foreach (DiscordApplicationCommand dac in dcCommands)
				_commands.Add(dac.Id, new ApplicationCommand(commands.First(x => x.Name == dac.Name), 0));
		}

		private async Task OnGuildAvailable(DiscordClient sender, GuildCreateEventArgs args)
		{
			ApplicationCommandBuilder[] commands =
				_rawCommands.Where(x => x.GuildId == args.Guild.Id).Select(x => x.Command).ToArray();
			IEnumerable<DiscordApplicationCommand> dcCommands =
				await _client.BulkOverwriteGuildApplicationCommandsAsync(args.Guild.Id,
					commands.Select(x => x.Build()));

			foreach (DiscordApplicationCommand dac in dcCommands)
				_commands.Add(dac.Id, new ApplicationCommand(commands.First(x => x.Name == dac.Name), args.Guild.Id));
		}
		
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
					// todo: services
					Token = e.Interaction.Token,
					Type = e.Interaction.Data.Type,
					User = e.Interaction.User,
					CommandName = e.Interaction.Data.Name,
					InteractionId = e.Interaction.Id,
					ResolvedChannelMentions = e.Interaction.Data.Resolved?.Channels.Values.ToList(),
					ResolvedUserMentions = e.Interaction.Data.Resolved?.Users.Values.ToList(),
					ResolvedRoleMentions = e.Interaction.Data.Resolved?.Roles.Values.ToList(),
					SlashCommandsExtension = this
				};

				List<object> argumentsList = new()
				{
					ctx
				};
				
				argumentsList.AddRange(options);


				MethodInfo method = _commands[e.Interaction.Data.Id].Methods[methodName];
				ApplicationCommandModule instance =
					(ApplicationCommandModule)Activator.CreateInstance(method.DeclaringType);

				bool shouldRun = await instance.BeforeSlashExecutionAsync(ctx);

				if (shouldRun)
				{
					await (Task)method.Invoke(instance, argumentsList.ToArray());
					await instance.AfterSlashExecutionAsync(ctx);
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
					// todo: services
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
					(ApplicationCommandModule)Activator.CreateInstance(method.DeclaringType);

				bool shouldRun = await instance.BeforeContextMenuExecutionAsync(ctx);

				if (shouldRun)
				{
					await (Task)method.Invoke(instance, new object[]{ctx});
					await instance.AfterContextMenuExecutionAsync(ctx);
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
					// todo: services
					Options = options.ToList(),
					User = e.Interaction.User,
					FocusedOption = focusedOption,
					SlashCommandsExtension = this
				};


				MethodInfo method = _commands[e.Interaction.Data.Id].AutocompleteMethods[focusedOption.Name];
				IAutocompleteProvider instance =
					(IAutocompleteProvider)Activator.CreateInstance(method.DeclaringType);

				IEnumerable<DiscordAutoCompleteChoice> choices = await instance.Provider(ctx);

				await e.Interaction.CreateResponseAsync(InteractionResponseType.AutoCompleteResult,
					new DiscordInteractionResponseBuilder().AddAutoCompleteChoices(choices));
			});
			return Task.CompletedTask;
			
		}

		private async Task<IEnumerable<object>> ParseOptions(MethodInfo info, DiscordInteractionDataOption[] options, DiscordInteractionResolvedCollection resolved)
		{
			List<object> objects = new();
			
			foreach (ParameterInfo param in info.GetParameters().Skip(1))
			{
				string paramName = param.GetCustomAttribute<OptionAttribute>()?.Name;
				DiscordInteractionDataOption option = options.FirstOrDefault(x => x.Name == paramName);

				objects.Add(await ConvertOptionToType(option, param.ParameterType, resolved));
			}
			
			return objects;
		}

		private async Task<object> ConvertOptionToType(DiscordInteractionDataOption option, Type type, DiscordInteractionResolvedCollection resolved)
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
			if (type == typeof(Enum))
				throw new ArgumentException("Enums are not supported yet");
			throw new ArgumentOutOfRangeException(nameof(type), type,
				"Slash command option types can be one of string, long, bool, double, DiscordUser, DiscordChannel, DiscordRole, SnowflakeObject, Enum");
		}
	}
}

// i take no responsibility for this
// if anything goes wrong, blame velvet lmao