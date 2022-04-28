using System;
using System.Threading.Tasks;

namespace DSharpPlus.SlashCommands.TestBot
{
	public class ContextMenus : ApplicationCommandModule
	{
		#region Before / After

		public override Task<bool> BeforeContextMenuExecutionAsync(ContextMenuContext ctx)
		{
			Console.WriteLine($"Before executing the context menu '{ctx.CommandName}'");
			return Task.FromResult(true);
		}

		public override Task AfterContextMenuExecutionAsync(ContextMenuContext ctx)
		{
			Console.WriteLine($"After executing the context menu '{ctx.CommandName}'");
			return Task.FromResult(true);
		}

		#endregion
		
		[ContextMenu(ApplicationCommandType.MessageContextMenu, "Message")]
		public async Task MessageCommand(ContextMenuContext ctx) =>
			await ctx.CreateResponseAsync($"{ctx.TargetMessage.Author.Mention}: {ctx.TargetMessage.Content}");
		
		[ContextMenu(ApplicationCommandType.UserContextMenu, "User")]
		public async Task UserCommand(ContextMenuContext ctx) =>
			await ctx.CreateResponseAsync(ctx.TargetUser.Mention);
		
		[ContextMenu(ApplicationCommandType.UserContextMenu, "Ban User (permtest)", Permissions.BanMembers)]
		public async Task BanCommand(ContextMenuContext ctx) =>
			await ctx.CreateResponseAsync("nah :sleeping:");
	}
}