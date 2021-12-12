using System;
using System.Threading.Tasks;

namespace DSharpPlus.SlashCommands.TestBot
{
	public class YearAttribute : SlashCheckBaseAttribute
	{
		public override Task<bool> ExecuteChecksAsync(InteractionContext ctx) =>
			Task.FromResult(DateTime.Now.Year == 2023);
	}
}