using System;
using System.Linq;
using System.Runtime.Loader;
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
				Token = token,
				MinimumLogLevel = LogLevel.Trace
			});

			SlashCommandsExtension slash = _client.UseSlashCommands(new SlashCommandsConfiguration
			{
				LocalizationProvider = new LocalizationProvider()
			});

			const ulong testGuildId = 917263628846108683;
			slash.RegisterCommands<SlashCommands>(testGuildId);
			slash.RegisterCommands<OneLevelGroup>(testGuildId);
			slash.RegisterCommands<TwoLevelGroup>(testGuildId);
			slash.RegisterCommands<WrappedGroup>(testGuildId);
			slash.RegisterCommands<PreExecutionChecks>(testGuildId);
			slash.RegisterCommands<ContextMenus>(testGuildId);
			slash.RegisterCommands<MixedGroups>(testGuildId);
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

			slash.ApplicationCommandRegistered += (sender, args) =>
			{
				_client.Logger.LogInformation($"Registered {args.Commands.Count()} commands for {args.GuildId} ({(args.GuildId is null or 0 ? "global" : _client.Guilds[args.GuildId.Value].Name)})");
				return Task.CompletedTask;
			};

			slash.ApplicationCommandRegisterFailed += (sender, args) =>
			{
				_client.Logger.LogError(args.Exception, $"Failed to register application commands for {args.GuildId} ({(args.GuildId is null or 0 ? "global" : _client.Guilds[args.GuildId.Value].Name)})");
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