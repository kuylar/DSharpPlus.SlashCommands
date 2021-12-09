using System;
using System.Reflection.Emit;
using System.Threading.Tasks;
using DSharpPlus.Exceptions;
using DSharpPlus.SlashCommands.Entities;
using DSharpPlus.SlashCommands.Extensions;
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
			slash.RegisterCommand(
				new ApplicationCommandBuilder(ApplicationCommandType.SlashCommand)
					.WithName("test")
					.WithDescription("TESTAAAAAAA")
				, 917263628846108683);
			
			slash.RegisterCommand(
				new ApplicationCommandBuilder(ApplicationCommandType.SlashCommand)
					.WithName("one-level-group")
					.WithDescription("yes")
					.AddOption(
						new ApplicationCommandOptionBuilder(ApplicationCommandOptionType.SubCommand)
							.WithName("cool-command")
							.WithDescription("this is a cool command")
					)
					.AddOption(
						new ApplicationCommandOptionBuilder(ApplicationCommandOptionType.SubCommand)
							.WithName("cooler-command")
							.WithDescription("this command is cooler")
					)
				, 917263628846108683);
			
			slash.RegisterCommand(
				new ApplicationCommandBuilder(ApplicationCommandType.SlashCommand)
					.WithName("two-level-group")
					.WithDescription("YES")
					.AddOption(
						new ApplicationCommandOptionBuilder(ApplicationCommandOptionType.SubCommandGroup)
							.WithName("cool-group")
							.WithDescription("this is a cool group")
							.AddOption(
								new ApplicationCommandOptionBuilder(ApplicationCommandOptionType.SubCommand)
									.WithName("cool-command")
									.WithDescription("this is a cool command")
							)
					)
					.AddOption(
						new ApplicationCommandOptionBuilder(ApplicationCommandOptionType.SubCommandGroup)
							.WithName("cooler-group")
							.WithDescription("this group is cooler")
							.AddOption(
								new ApplicationCommandOptionBuilder(ApplicationCommandOptionType.SubCommand)
									.WithName("coolest-command")
									.WithDescription("this is the coolest command")
							)
					)
					.AddOption(
						new ApplicationCommandOptionBuilder(ApplicationCommandOptionType.SubCommandGroup)
							.WithName("shit-group")
							.WithDescription("this group fucking sucks")
							.AddOption(
								new ApplicationCommandOptionBuilder(ApplicationCommandOptionType.SubCommand)
									.WithName("haha-poop")
									.WithDescription("funny joke lol")
							)
					)
				, 917263628846108683);
			
			slash.RegisterCommand(
				new ApplicationCommandBuilder(ApplicationCommandType.MessageContextMenu)
					.WithName("Message")
				, 917263628846108683);

			slash.RegisterCommand(
				new ApplicationCommandBuilder(ApplicationCommandType.UserContextMenu)
					.WithName("User")
				, 917263628846108683);

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