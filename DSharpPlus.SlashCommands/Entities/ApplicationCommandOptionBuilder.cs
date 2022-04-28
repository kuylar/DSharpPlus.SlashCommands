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

		public Dictionary<Localization, string> NameLocalizations { get; private set; }
		public Dictionary<Localization, string> DescriptionLocalizations { get; private set; }

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
			NameLocalizations = new Dictionary<Localization, string>();
			DescriptionLocalizations = new Dictionary<Localization, string>();
		}

		/// <summary>
		/// Sets the name of this option
		/// </summary>
		/// <param name="name">Name of this option</param>
		/// <returns>ApplicationCommandOptionBuilder to be chained</returns>
		public ApplicationCommandOptionBuilder WithName(string name)
		{
			Name = name;
			return this;
		}

		/// <summary>
		/// Sets the description of this option
		/// </summary>
		/// <param name="description">Description of this option</param>
		/// <returns>ApplicationCommandOptionBuilder to be chained</returns>
		public ApplicationCommandOptionBuilder WithDescription(string description)
		{
			Description = description;
			return this;
		}

		/// <summary>
		/// Sets if this option is required or optional
		/// </summary>
		/// <param name="required">If this option is required</param>
		/// <returns>ApplicationCommandOptionBuilder to be chained</returns>
		public ApplicationCommandOptionBuilder IsRequired(bool required)
		{
			Required = required;
			return this;
		}

		/// <summary>
		/// Adds a choice to this option
		/// </summary>
		/// <param name="name">The string the user will see while selecting this choice</param>
		/// <param name="value">The value you will receive when the user selects this choice</param>
		/// <returns>ApplicationCommandOptionBuilder to be chained</returns>
		public ApplicationCommandOptionBuilder AddChoice(string name, object value)
		{
			if (Type is not (ApplicationCommandOptionType.Integer or ApplicationCommandOptionType.Number or
				ApplicationCommandOptionType.String))
				throw new InvalidOperationException(
					"Only slash command options of type Integer, Number or String can have choices added in them");
			Choices.Add(new DiscordApplicationCommandOptionChoice(name, value));
			return this;
		}

		/// <summary>
		/// Sets the types of channels that is allowed
		/// </summary>
		/// <param name="channelTypes">Allowed channel types</param>
		/// <returns>ApplicationCommandOptionBuilder to be chained</returns>
		public ApplicationCommandOptionBuilder WithChannelTypes(params ChannelType[] channelTypes)
		{
			if (Type != ApplicationCommandOptionType.Channel)
				throw new InvalidOperationException(
					"Only slash command options of type Channel can have channel types added in them");

			ChannelType = channelTypes;
			return this;
		}

		/// <summary>
		/// Sets the minimum and the maximum value of this option
		/// </summary>
		/// <param name="min">Minimum value this option can get</param>
		/// <param name="max">Maximum value this option can get</param>
		/// <returns>ApplicationCommandOptionBuilder to be chained</returns>
		public ApplicationCommandOptionBuilder WithMinMaxValue(long? min, long? max)
		{
			if (Type is not (ApplicationCommandOptionType.Integer or ApplicationCommandOptionType.Number))
				throw new InvalidOperationException(
					"Only slash command options of type Integer or Number can have minimum and maximum values added in them");

			MinValue = min;
			MaxValue = max;

			return this;
		}

		/// <summary>
		/// Adds an option to this option
		/// </summary>
		/// <param name="builder">The option to add</param>
		/// <returns>ApplicationCommandOptionBuilder to be chained</returns>
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

		/// <summary>
		/// Adds multiple options to this option
		/// </summary>
		/// <param name="builders">The options to add</param>
		/// <returns>ApplicationCommandOptionBuilder to be chained</returns>
		public ApplicationCommandOptionBuilder AddOptions(params ApplicationCommandOptionBuilder[] builders)
		{
			foreach (ApplicationCommandOptionBuilder builder in builders) AddOption(builder);
			return this;
		}

		/// <summary>
		/// Sets the method of this option. Can only be used for subcommands
		/// </summary>
		/// <param name="methodInfo">The method</param>
		/// <returns>ApplicationCommandOptionBuilder to be chained</returns>
		public ApplicationCommandOptionBuilder WithMethod(MethodInfo methodInfo)
		{
			if (Type is not (ApplicationCommandOptionType.SubCommand))
				throw new InvalidOperationException(
					"Only slash command options of type SubCommand or SubCommandGroup can have minimum and maximum values added in them");

			Method = methodInfo;
			return this;
		}

		/// <summary>
		/// Sets an autocomplete method for this option
		/// </summary>
		/// <param name="methodInfo">The autocomplete method</param>
		/// <returns>ApplicationCommandOptionBuilder to be chained</returns>
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

		/// <summary>
		/// Adds a localization data for this commands name
		/// </summary>,
		/// <param name="language">The language to apply this text to</param>
		/// <param name="value">The text to show with this language</param>
		/// <returns>The ApplicationCommandBuilder to be chained</returns>
		public ApplicationCommandOptionBuilder WithNameLocalization(Localization language, string value)
		{
			if (NameLocalizations.ContainsKey(language))
				NameLocalizations[language] = value;
			else
				NameLocalizations.Add(language, value);
			return this;
		}

		/// <summary>
		/// Adds a localization data for this commands description
		/// </summary>,
		/// <param name="language">The language to apply this text to</param>
		/// <param name="value">The text to show with this language</param>
		/// <returns>The ApplicationCommandBuilder to be chained</returns>
		public ApplicationCommandOptionBuilder WithDescriptionLocalization(Localization language, string value)
		{
			if (DescriptionLocalizations.ContainsKey(language))
				DescriptionLocalizations[language] = value;
			else
				DescriptionLocalizations.Add(language, value);
			return this;
		}

		/// <summary>
		/// Builds this ApplicationCommandOptionBuilder
		/// </summary>
		/// <returns>A DiscordApplicationCommandOption to be sent to Discord</returns>
		public DiscordApplicationCommandOption Build() =>
			new(Name, Description, Type, Required, Choices, Options?.Select(x => x.Build()), ChannelType,
				AutoCompleteMethod != null, MinValue, MaxValue,
				NameLocalizations.ToDictionary(x => x.Key.GetLanguageCode(), x => x.Value), 
				DescriptionLocalizations.ToDictionary(x => x.Key.GetLanguageCode(), x => x.Value)
			);
	}
}