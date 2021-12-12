using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DSharpPlus.Entities;

namespace DSharpPlus.SlashCommands.Entities
{
	public class ApplicationCommandBuilder
	{
		public string Name { get; private set; }
		public string Description { get; private set; }
		public bool DefaultPermission { get; private set; } = true;
		public ApplicationCommandType Type { get; private set; }
		public List<ApplicationCommandOptionBuilder> Options { get; private set; }
		public MethodInfo Method { get; private set; }

		public ApplicationCommandBuilder(ApplicationCommandType type)
		{
			if (type == ApplicationCommandType.AutoCompleteRequest)
				throw new ArgumentException("ApplicationCommand type cannot be AutoCompleteRequest");
			Type = type;
			if (type == ApplicationCommandType.SlashCommand) Options = new List<ApplicationCommandOptionBuilder>();
		}

		/// <summary>
		/// Sets the name of this ApplicationCommand.
		/// </summary>
		/// <param name="name">Name of the ApplicationCommand.</param>
		/// <returns>The ApplicationCommandBuilder to be chained</returns>
		public ApplicationCommandBuilder WithName(string name)
		{
			Name = name;
			return this;
		}

		/// <summary>
		/// Sets the description of this ApplicationCommand.
		/// </summary>
		/// <param name="description">Description of the ApplicationCommand.</param>
		/// <returns>The ApplicationCommandBuilder to be chained</returns>
		public ApplicationCommandBuilder WithDescription(string description)
		{
			Description = description;
			return this;
		}

		/// <summary>
		/// Sets if this command should be enabled by default
		/// </summary>
		/// <param name="enabledByDefault">If this command should be enabled by default.</param>
		/// <returns>The ApplicationCommandBuilder to be chained</returns>
		public ApplicationCommandBuilder WithDefaultPermission(bool enabledByDefault)
		{
			DefaultPermission = enabledByDefault;
			return this;
		}

		/// <summary>
		/// Adds an option to this ApplicationCommand
		/// </summary>
		/// <param name="builder">The option to add.</param>
		/// <returns>The ApplicationCommandBuilder to be chained</returns>
		public ApplicationCommandBuilder AddOption(ApplicationCommandOptionBuilder builder)
		{
			Options.Add(builder);
			return this;
		}

		/// <summary>
		/// Adds multiple options to this ApplicationCommand
		/// </summary>
		/// <param name="builders">Array of options to add.</param>
		/// <returns>The ApplicationCommandBuilder to be chained</returns>
		public ApplicationCommandBuilder AddOptions(params ApplicationCommandOptionBuilder[] builders)
		{
			Options.AddRange(builders);
			return this;
		}

		/// <summary>
		/// Sets the method that should be called when this slash command is executed
		/// </summary>
		/// <param name="methodInfo">The method to run.</param>
		/// <returns>The ApplicationCommandBuilder to be chained</returns>
		public ApplicationCommandBuilder WithMethod(MethodInfo methodInfo)
		{
			Method = methodInfo;
			return this;
		}

		/// <summary>
		/// Builds this ApplicationCommandBuilder
		/// </summary>
		/// <returns>A DiscordApplicationCommand to be sent to Discord</returns>
		public DiscordApplicationCommand Build() =>
			new(Name, Description, Options?.Select(x => x.Build()), DefaultPermission, Type);
	}
}