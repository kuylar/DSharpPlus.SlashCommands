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
					.WithName("string")
					.WithDescription("Strings")
					.AddOption(new ApplicationCommandOptionBuilder(ApplicationCommandOptionType.String)
						.WithName("string")
						.WithDescription("Required string"))
					.AddOption(new ApplicationCommandOptionBuilder(ApplicationCommandOptionType.String)
						.WithName("opt-string")
						.WithDescription("Optional string")
						.IsRequired(false))
				, 917263628846108683);
			
			slash.RegisterCommand(
				new ApplicationCommandBuilder(ApplicationCommandType.SlashCommand)
					.WithName("int")
					.WithDescription("Integers")
					.AddOption(new ApplicationCommandOptionBuilder(ApplicationCommandOptionType.Integer)
						.WithName("int")
						.WithDescription("Required int"))
					.AddOption(new ApplicationCommandOptionBuilder(ApplicationCommandOptionType.Integer)
						.WithName("opt-int")
						.WithDescription("Optional int")
						.IsRequired(false))
				, 917263628846108683);
			
			slash.RegisterCommand(
				new ApplicationCommandBuilder(ApplicationCommandType.SlashCommand)
					.WithName("bool")
					.WithDescription("Booleans")
					.AddOption(new ApplicationCommandOptionBuilder(ApplicationCommandOptionType.Boolean)
						.WithName("bool")
						.WithDescription("Required bool"))
					.AddOption(new ApplicationCommandOptionBuilder(ApplicationCommandOptionType.Boolean)
						.WithName("opt-bool")
						.WithDescription("Optional bool")
						.IsRequired(false))
				, 917263628846108683);
			
			slash.RegisterCommand(
				new ApplicationCommandBuilder(ApplicationCommandType.SlashCommand)
					.WithName("user")
					.WithDescription("Users")
					.AddOption(new ApplicationCommandOptionBuilder(ApplicationCommandOptionType.User)
						.WithName("user")
						.WithDescription("Required user"))
					.AddOption(new ApplicationCommandOptionBuilder(ApplicationCommandOptionType.User)
						.WithName("opt-user")
						.WithDescription("Optional user")
						.IsRequired(false))
				, 917263628846108683);
			
			slash.RegisterCommand(
				new ApplicationCommandBuilder(ApplicationCommandType.SlashCommand)
					.WithName("channel")
					.WithDescription("Channels")
					.AddOption(new ApplicationCommandOptionBuilder(ApplicationCommandOptionType.Channel)
						.WithName("channel")
						.WithDescription("Required channel"))
					.AddOption(new ApplicationCommandOptionBuilder(ApplicationCommandOptionType.Channel)
						.WithName("opt-channel")
						.WithDescription("Optional channel")
						.IsRequired(false))
				, 917263628846108683);
			
			slash.RegisterCommand(
				new ApplicationCommandBuilder(ApplicationCommandType.SlashCommand)
					.WithName("role")
					.WithDescription("Roles")
					.AddOption(new ApplicationCommandOptionBuilder(ApplicationCommandOptionType.Role)
						.WithName("role")
						.WithDescription("Required role"))
					.AddOption(new ApplicationCommandOptionBuilder(ApplicationCommandOptionType.Role)
						.WithName("opt-role")
						.WithDescription("Optional role")
						.IsRequired(false))
				, 917263628846108683);
			
			slash.RegisterCommand(
				new ApplicationCommandBuilder(ApplicationCommandType.SlashCommand)
					.WithName("mentionable")
					.WithDescription("Mentionables")
					.AddOption(new ApplicationCommandOptionBuilder(ApplicationCommandOptionType.Mentionable)
						.WithName("mentionable")
						.WithDescription("Required mentionable"))
					.AddOption(new ApplicationCommandOptionBuilder(ApplicationCommandOptionType.Mentionable)
						.WithName("opt-mentionable")
						.WithDescription("Optional mentionable")
						.IsRequired(false))
				, 917263628846108683);
			
			slash.RegisterCommand(
				new ApplicationCommandBuilder(ApplicationCommandType.SlashCommand)
					.WithName("number")
					.WithDescription("Numbers")
					.AddOption(new ApplicationCommandOptionBuilder(ApplicationCommandOptionType.Number)
						.WithName("number")
						.WithDescription("Required number"))
					.AddOption(new ApplicationCommandOptionBuilder(ApplicationCommandOptionType.Number)
						.WithName("opt-number")
						.WithDescription("Optional number")
						.IsRequired(false))
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