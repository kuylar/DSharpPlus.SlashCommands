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

		public ApplicationCommandBuilder WithName(string name)
		{
			Name = name;
			return this;
		}

		public ApplicationCommandBuilder WithDescription(string description)
		{
			Description = description;
			return this;
		}

		public ApplicationCommandBuilder WithDefaultPermission(bool enabledByDefault)
		{
			DefaultPermission = enabledByDefault;
			return this;
		}

		public ApplicationCommandBuilder AddOption(ApplicationCommandOptionBuilder builder)
		{
			Options.Add(builder);
			return this;
		}

		public ApplicationCommandBuilder WithMethod(MethodInfo methodInfo)
		{
			Method = methodInfo;
			return this;
		}

		public DiscordApplicationCommand Build() =>
			new(Name, Description, Options?.Select(x => x.Build()), DefaultPermission, Type);
	}
}