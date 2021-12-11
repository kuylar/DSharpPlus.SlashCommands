using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DSharpPlus.Entities;

namespace DSharpPlus.SlashCommands.Entities
{
	public class ApplicationCommandOptionBuilder
	{
		public ApplicationCommandOptionType Type { get; private set; }
		public string Name { get; private set; }
		public string Description { get; private set; }
		public bool? Required { get; private set; } = true;
		public List<DiscordApplicationCommandOptionChoice> Choices { get; private set; }
		public List<ApplicationCommandOptionBuilder> Options { get; private set; }
		public ChannelType[] ChannelType { get; private set; }
		public long? MinValue { get; private set; }
		public long? MaxValue { get; private set; }
		public MethodInfo AutoCompleteMethod { get; private set; }
		public MethodInfo Method { get; private set; }

		public ApplicationCommandOptionBuilder(ApplicationCommandOptionType type)
		{
			Type = type;

			switch (Type)
			{
				case ApplicationCommandOptionType.Integer:
				case ApplicationCommandOptionType.Number:
				case ApplicationCommandOptionType.String:
					Choices = new List<DiscordApplicationCommandOptionChoice>();
					break;
				case ApplicationCommandOptionType.SubCommandGroup:
				case ApplicationCommandOptionType.SubCommand:
					Options = new List<ApplicationCommandOptionBuilder>();
					Required = false;
					break;
				case ApplicationCommandOptionType.Channel:
					ChannelType = Array.Empty<ChannelType>();
					break;
			}
		}

		public ApplicationCommandOptionBuilder WithName(string name)
		{
			Name = name;
			return this;
		}

		public ApplicationCommandOptionBuilder WithDescription(string description)
		{
			Description = description;
			return this;
		}

		public ApplicationCommandOptionBuilder IsRequired(bool required)
		{
			Required = required;
			return this;
		}

		public ApplicationCommandOptionBuilder AddChoice(string name, object value)
		{
			if (Type is not (ApplicationCommandOptionType.Integer or ApplicationCommandOptionType.Number or
				ApplicationCommandOptionType.String))
				throw new InvalidOperationException(
					"Only slash command options of type Integer, Number or String can have choices added in them");
			Choices.Add(new DiscordApplicationCommandOptionChoice(name, value));
			return this;
		}

		public ApplicationCommandOptionBuilder WithChannelTypes(params ChannelType[] channelType)
		{
			if (Type != ApplicationCommandOptionType.Channel)
				throw new InvalidOperationException(
					"Only slash command options of type Channel can have channel types added in them");

			ChannelType = channelType;
			return this;
		}

		public ApplicationCommandOptionBuilder WithMinMaxValue(long? min, long? max)
		{
			if (Type is not (ApplicationCommandOptionType.Integer or ApplicationCommandOptionType.Number))
				throw new InvalidOperationException(
					"Only slash command options of type Integer or Number can have minimum and maximum values added in them");

			MinValue = min;
			MaxValue = max;

			return this;
		}

		public ApplicationCommandOptionBuilder AddOption(ApplicationCommandOptionBuilder builder)
		{
			if (Type is not ApplicationCommandOptionType.SubCommandGroup &&
			    builder.Type is (ApplicationCommandOptionType.SubCommand or ApplicationCommandOptionType.SubCommandGroup
				    ))
				throw new InvalidOperationException(
					"Only slash command options of type SubCommandGroup can have options added in them");

			Options.Add(builder);
			return this;
		}

		public ApplicationCommandOptionBuilder AddOptions(params ApplicationCommandOptionBuilder[] builders)
		{
			foreach (ApplicationCommandOptionBuilder builder in builders) AddOption(builder);
			return this;
		}

		public ApplicationCommandOptionBuilder WithMethod(MethodInfo methodInfo)
		{
			if (Type is not (ApplicationCommandOptionType.SubCommand or ApplicationCommandOptionType.SubCommandGroup))
				throw new InvalidOperationException(
					"Only slash command options of type SubCommand or SubCommandGroup can have minimum and maximum values added in them");

			Method = methodInfo;
			return this;
		}

		public ApplicationCommandOptionBuilder WithAutocomplete(MethodInfo methodInfo)
		{
			if (Type is not (ApplicationCommandOptionType.Integer or ApplicationCommandOptionType.Number or
				ApplicationCommandOptionType.String))
				throw new InvalidOperationException(
					"Only slash command options of type Integer, Number or String can have choices added in them");

			// TODO: method checks

			AutoCompleteMethod = methodInfo;
			return this;
		}


		public DiscordApplicationCommandOption Build() =>
			new(Name, Description, Type, Required, Choices, Options?.Select(x => x.Build()), ChannelType,
				AutoCompleteMethod != null, MinValue, MaxValue);
	}
}