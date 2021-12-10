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

			_client.GuildAvailable += OnGuildAvailable;

			_client.InteractionCreated += HandleSlashCommand;
			_client.ContextMenuInteractionCreated += HandleContextMenu;
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
					// heres the part that everyone hates
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
			// TODO: DO ***NOT*** FORGET BEFORE/AFTEREXECUTIONASYNC
			if (e.Interaction.Type != InteractionType.ApplicationCommand) return Task.CompletedTask;
			Task.Run(async () =>
			{
				string method = e.Interaction.Data.Options?.First().Type switch
				{
					ApplicationCommandOptionType.SubCommand => e.Interaction.Data.Options?.First().Name,
					ApplicationCommandOptionType.SubCommandGroup =>
						$"{e.Interaction.Data.Options?.First().Name} {e.Interaction.Data.Options?.First().Options.First().Name}",
					_ => string.Empty
				};

				IEnumerable<object> options =
					await ParseOptions(_commands[e.Interaction.Data.Id].Methods[method],
						e.Interaction.Data.Options?.First().Type switch
						{
							ApplicationCommandOptionType.SubCommand => e.Interaction.Data.Options?.First().Options,
							ApplicationCommandOptionType.SubCommandGroup => e.Interaction.Data.Options?.First().Options
								.First().Options,
							_ => e.Interaction.Data.Options
						} ?? Array.Empty<DiscordInteractionDataOption>());
				// SORRY

				string opts = string.Join("\n", options);
				await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
					.WithContent($"[{e.Interaction.Id}]: {method}\n{opts}"));
			});
			return Task.CompletedTask;
		}

		private Task HandleContextMenu(DiscordClient sender, ContextMenuInteractionCreateEventArgs e)
		{
			if (e.Interaction.Type != InteractionType.ApplicationCommand) return Task.CompletedTask;
			Task.Run(async () =>
			{
				string method = string.Empty;

				await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
					.WithContent($"[{e.Interaction.Id}]: {method}\nUser: {e.TargetUser}\nMessage: {e.TargetMessage}"));
			});
			return Task.CompletedTask;
		}

		private async Task<IEnumerable<object>> ParseOptions(MethodInfo info, IEnumerable<DiscordInteractionDataOption> options)
		{
			List<object> objects = new();
			
			
			foreach (DiscordInteractionDataOption option in options)
			{
				objects.Add($"{option.Type}: {option.Value}");
			}
			
			return objects;
		}
	}
}

// i take no responsibility for this
// if anything goes wrong, blame velvet lmao