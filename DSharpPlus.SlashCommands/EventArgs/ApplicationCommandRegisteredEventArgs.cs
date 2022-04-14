using System;
using System.Collections.Generic;
using DSharpPlus.Entities;
using Emzi0767.Utilities;

namespace DSharpPlus.SlashCommands.EventArgs
{
	public class ApplicationCommandRegisteredEventArgs : AsyncEventArgs
	{
		/// <summary>
		/// ID of the guild that registered	. This is null for global commands.
		/// </summary>
		public ulong? GuildId { get; internal set; }

		/// <summary>
		/// List of the commands that have been registered.
		/// </summary>
		public IEnumerable<DiscordApplicationCommand> Commands { get; set; }
	}
}