using System;
using System.Threading.Tasks;
using DSharpPlus.Exceptions;
using Microsoft.Extensions.Logging;

namespace DSharpPlus.SlashCommands.TestBot
{
	internal static class Program
	{
		private static DiscordClient _client;
		
		private static async Task Main()
		{
			string token = Environment.GetEnvironmentVariable("TOKEN");
			if (token == null)
			{
				Console.WriteLine(
					"Please add a valid Discord bot token in the TOKEN environment variable, and then run this executable again");
				Environment.Exit(1);
			}

			_client = new DiscordClient(new DiscordConfiguration
			{
				Token = token
			});

			SlashCommandsExtension slash = _client.UseSlashCommands();

			slash.RegisterCommands<SlashCommands>(917263628846108683);
			slash.RegisterCommands<OneLevelGroup>(917263628846108683);
			slash.RegisterCommands<TwoLevelGroup>(917263628846108683);
			slash.RegisterCommands<ContextMenus>(917263628846108683);

			_client.ClientErrored += (_, args) =>
			{
				if (args.Exception is BadRequestException exception)
					_client.Logger.LogCritical(exception.Errors);
				return Task.CompletedTask;
			};

			await _client.ConnectAsync();
			await Task.Delay(-1);
		}
	}
}