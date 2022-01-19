using System.Threading.Tasks;

namespace DSharpPlus.SlashCommands.TestBot
{
	public class GlobalCommands : ApplicationCommandModule
	{
		[SlashCommand("ping", "A global command")]
		public async Task Ping(InteractionContext context) => await context.CreateResponseAsync("Pong!");
	}
}