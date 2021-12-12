using System.Threading.Tasks;

namespace DSharpPlus.SlashCommands.TestBot
{
	public class PreExecutionChecks : ApplicationCommandModule
	{
		[SlashCommand("yearlock", "This command can only be used in 2023")]
		[Year]
		public async Task YearCommand(InteractionContext context) =>
			await context.CreateResponseAsync("oh wow its 2023?");
	}
}