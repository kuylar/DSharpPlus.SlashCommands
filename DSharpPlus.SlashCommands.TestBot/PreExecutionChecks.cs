using System.Threading.Tasks;
using DSharpPlus.SlashCommands.Attributes;

namespace DSharpPlus.SlashCommands.TestBot
{
	public class PreExecutionChecks : ApplicationCommandModule
	{
		[SlashCommand("yearlock", "This command can only be used in 2023")]
		[Year]
		public async Task YearCommand(InteractionContext context) =>
			await context.CreateResponseAsync("oh wow its 2023?");

		[SlashCommand("permissioncheck", "make sure to take my permissions of banning people")]
		[SlashRequireBotPermissions(Permissions.BanMembers)]
		public async Task BanPermCommand(InteractionContext context) =>
			await context.CreateResponseAsync("poggers, i can!!!");
		
		[ContextMenu(ApplicationCommandType.MessageContextMenu, "Year Lock (2023 only)")]
		[Year]
		public async Task YearCommand(ContextMenuContext context) =>
			await context.CreateResponseAsync("oh wow its 2023?");
	}
}