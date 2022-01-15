using System.Threading.Tasks;

namespace DSharpPlus.SlashCommands.TestBot
{
	[SlashCommandGroup("mixed-group", "Slash command with a subgroup & a subcommand")]
	public class MixedGroups : ApplicationCommandModule
	{
		[SlashCommandGroup("subgroup", "Subcommand group")]
		public class Subgroup : ApplicationCommandModule
		{
			[SlashCommand("subcommand1", "Subcommand inside a subcommand group")]
			public async Task SubgroupCommand1(InteractionContext ctx)
			{
				await ctx.CreateResponseAsync("MixedGroups > Subgroup > SubgroupCommand1");
			}

			[SlashCommand("subcommand2", "Another subcommand inside a subcommand group")]
			public async Task SubgroupCommand2(InteractionContext ctx)
			{
				await ctx.CreateResponseAsync("MixedGroups > Subgroup > SubgroupCommand2");
			}
		}
		
		[SlashCommand("subcommand1", "Subcommand right underneath a slash command")]
		public async Task SubgroupCommand1(InteractionContext ctx)
		{
			await ctx.CreateResponseAsync("MixedGroups > SubgroupCommand1");
		}

		[SlashCommand("subcommand2", "Another subcommand right underneath a slash command")]
		public async Task SubgroupCommand2(InteractionContext ctx)
		{
			await ctx.CreateResponseAsync("MixedGroups > SubgroupCommand2");
		}
	}
}