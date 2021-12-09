namespace DSharpPlus.SlashCommands.Extensions
{
	public static class DiscordClientExtensions
	{
		//TODO: config
		public static SlashCommandsExtension UseSlashCommands(this DiscordClient client)
		{
			SlashCommandsExtension ext = new();
			client.AddExtension(ext);
			return ext;
		}

		public static SlashCommandsExtension GetSlashCommands(this DiscordClient client) =>
			client.GetExtension<SlashCommandsExtension>();
	}
}