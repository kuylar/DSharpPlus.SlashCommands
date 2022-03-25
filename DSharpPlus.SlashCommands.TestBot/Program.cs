using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Exceptions;
using DSharpPlus.SlashCommands.Entities;
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

			const ulong testGuildId = 917263628846108683;

			slash.RegisterCommands<SlashCommands>(testGuildId);
			slash.RegisterCommands<OneLevelGroup>(testGuildId);
			slash.RegisterCommands<TwoLevelGroup>(testGuildId);
			slash.RegisterCommands<WrappedGroup>(testGuildId);
			slash.RegisterCommands<PreExecutionChecks>(testGuildId);
			slash.RegisterCommands<ContextMenus>(testGuildId);
			slash.RegisterCommands<MixedGroups>(testGuildId);
			slash.RegisterCommands<GlobalCommands>();

			ApplicationCommandBuilder localTestCommand = new ApplicationCommandBuilder(ApplicationCommandType.SlashCommand)
				.WithName("localetest")
				.WithDescription("Localization Test (it failed)")
				.WithMethod(typeof(SlashCommands).GetMethod("LocalTestCommand"));
			
			foreach (Localization l in Enum.GetValues(typeof(Localization)))
			{
				localTestCommand
					.WithNameLocalization(l, l.ToString())
					.WithDescriptionLocalization(l, "Localized to: " + l.GetNativeName());
			}
			
			slash.RegisterCommand(localTestCommand, testGuildId);

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