namespace DSharpPlus.SlashCommands
{
	public static class DiscordClientExtensions
	{
		public static SlashCommandsExtension UseSlashCommands(this DiscordClient client,
			SlashCommandsConfiguration config = null)
		{
			SlashCommandsExtension ext = new(config);
			client.AddExtension(ext);
			return ext;
		}

		public static SlashCommandsExtension GetSlashCommands(this DiscordClient client) =>
			client.GetExtension<SlashCommandsExtension>();
	}
}