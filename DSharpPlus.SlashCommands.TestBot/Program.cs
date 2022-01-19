using System;
using System.Linq;
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
			slash.RegisterCommands<WrappedGroup>(917263628846108683);
			slash.RegisterCommands<PreExecutionChecks>(917263628846108683);
			slash.RegisterCommands<ContextMenus>(917263628846108683);
			slash.RegisterCommands<MixedGroups>(917263628846108683);
			slash.RegisterCommands<GlobalCommands>();

			slash.SlashCommandErrored += (sender, args) =>
			{
				if (args.Exception is SlashExecutionChecksFailedException fail)
					args.Context.CreateResponseAsync(string.Join("\n", fail.FailedChecks.Select(x => x.GetType().Name)));
				else
					args.Context.CreateResponseAsync(Formatter.Sanitize(args.Exception.ToString()));
				return Task.CompletedTask;
			};

			slash.ContextMenuErrored += (sender, args) =>
			{
				if (args.Exception is SlashExecutionChecksFailedException fail)
					args.Context.CreateResponseAsync(string.Join("\n", fail.FailedChecks.Select(x => x.GetType().Name)));
				else
					args.Context.CreateResponseAsync(Formatter.Sanitize(args.Exception.ToString()));
				return Task.CompletedTask;
			};

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