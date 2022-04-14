using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

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

		public static async Task<IReadOnlyDictionary<int, SlashCommandsExtension>> UseSlashCommandsAsync(this DiscordShardedClient client, SlashCommandsConfiguration cfg)
		{
			Dictionary<int, SlashCommandsExtension> modules = new();
			await client.InitializeShardsAsync();

			foreach (DiscordClient shard in client.ShardClients.Select(x => x.Value))
				modules[shard.ShardId] = shard.GetExtension<SlashCommandsExtension>() ?? shard.UseSlashCommands(cfg);

			return new ReadOnlyDictionary<int, SlashCommandsExtension>(modules);
		}

		public static SlashCommandsExtension GetSlashCommands(this DiscordClient client) =>
			client.GetExtension<SlashCommandsExtension>();
	}
}