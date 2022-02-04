using System.Threading.Tasks;

namespace DSharpPlus.SlashCommands.TestBot
{
	public class WrappedGroup : ApplicationCommandModule
	{
		[SlashCommandGroup("wrapped-one-level-group", "Single level group")]
		public class OneLevelGroup : ApplicationCommandModule
		{
			[SlashCommand("command1", "First command")]
			public async Task Command1(InteractionContext ctx) =>
				await ctx.CreateResponseAsync("yes this is a command");

			[SlashCommand("command2", "Second command")]
			public async Task Command2(InteractionContext ctx) =>
				await ctx.CreateResponseAsync("yes this is also a command");
		}

		[SlashCommandGroup("wrapped-two-level-group", "Double level group")]
		public class TwoLevelGroup : ApplicationCommandModule
		{
			[SlashCommandGroup("subgroup1", "Single level group")]
			public class Subgroup1 : ApplicationCommandModule
			{
				[SlashCommand("command1", "First command")]
				public async Task Command1(InteractionContext ctx) =>
					await ctx.CreateResponseAsync("yes this is a command");

				[SlashCommand("command2", "Second command")]
				public async Task Command2(InteractionContext ctx) =>
					await ctx.CreateResponseAsync("yes this is also a command");
			}

			[SlashCommandGroup("subgroup2", "Single level group")]
			public class Subgroup2 : ApplicationCommandModule
			{
				[SlashCommand("command1", "Third command")]
				public async Task Command1(InteractionContext ctx) =>
					await ctx.CreateResponseAsync("yes this is a command");

				[SlashCommand("command2", "Fourth command")]
				public async Task Command2(InteractionContext ctx) =>
					await ctx.CreateResponseAsync("yes this is also a command");
			}
		}
	}
}