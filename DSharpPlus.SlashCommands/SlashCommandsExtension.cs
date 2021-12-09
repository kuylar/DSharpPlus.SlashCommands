using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands.Entities;
using Microsoft.Extensions.Logging;

namespace DSharpPlus.SlashCommands
{
	public class SlashCommandsExtension : BaseExtension
	{
		private DiscordClient _client;

		private Dictionary<ulong, ApplicationCommand> _commands;

		private List<(ApplicationCommandBuilder, ulong)> _rawCommands;
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
		
		public void RegisterCommand(ApplicationCommandBuilder command, ulong guildId = 0)
		{
			// todo: ACTUAL registering, not this ApplicationCommandBuilder thing
			// keep an overload for it tho, might be useful for some people
			_rawCommands.Add((command, guildId));
		}

		private async Task OnGuildAvailable(DiscordClient sender, GuildCreateEventArgs guildCreateEventArgs)
		{
			foreach ((ApplicationCommandBuilder command, ulong guildId) in _rawCommands.Where(x =>
				x.Item2 == guildCreateEventArgs.Guild.Id))
			{
				DiscordApplicationCommand dac;
				if (guildId != 0) dac = await _client.CreateGuildApplicationCommandAsync(guildId, command.Build());
				else dac = await _client.CreateGlobalApplicationCommandAsync(command.Build());
				ApplicationCommand ac = new(command, guildId);
				_commands.Add(dac.Id, ac);
			}
		}
		
		private Task HandleSlashCommand(DiscordClient sender, InteractionCreateEventArgs e)
		{
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
				
				_client.Logger.LogInformation($"[{e.Interaction.Data.Id}] ({_commands[e.Interaction.Data.Id]}): {method}");
				await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
					.WithContent($"[{e.Interaction.Id}] ({_commands[e.Interaction.Id]}): {method}"));
			});
			return Task.CompletedTask;
		}

		private Task HandleContextMenu(DiscordClient sender, ContextMenuInteractionCreateEventArgs e)
		{
			if (e.Interaction.Type != InteractionType.ApplicationCommand) return Task.CompletedTask;
			Task.Run(async () =>
			{
				string method = string.Empty;

				_client.Logger.LogInformation($"[{e.Interaction.Data.Id}] ({_commands[e.Interaction.Data.Id]}): {method}\nUser: {e.TargetUser}\nMessage: {e.TargetMessage}");
				await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
					.WithContent($"[{e.Interaction.Id}] ({_commands[e.Interaction.Id]}): {method}"));
			});
			return Task.CompletedTask;
		}
	}
}

// i take no responsibility for this
// if anything goes wrong, blame velvet lmao