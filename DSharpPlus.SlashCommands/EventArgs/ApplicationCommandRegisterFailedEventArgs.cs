using System;
using Emzi0767.Utilities;

namespace DSharpPlus.SlashCommands.EventArgs
{
	public class ApplicationCommandRegisterFailedEventArgs : AsyncEventArgs
	{
		/// <summary>
		/// The exception thrown.
		/// </summary>
		public Exception Exception { get; internal set; }

		/// <summary>
		/// ID of the guild that failed to register. This is null for global commands.
		/// </summary>
		public ulong? GuildId { get; internal set; }
	}
}