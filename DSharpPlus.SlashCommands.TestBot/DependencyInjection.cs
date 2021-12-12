using System;
using System.Threading.Tasks;

namespace DSharpPlus.SlashCommands.TestBot
{
	public class DependencyInjection
	{
		public Random Rng { private get; set; } // Implied public setter.
		
		[SlashCommand("di", "DependencyInjection")]
		public async Task DiceCommand(InteractionContext ctx) =>
			await ctx.CreateResponseAsync($"You rolled: {Rng.Next(0, 7)}");	
	}
}