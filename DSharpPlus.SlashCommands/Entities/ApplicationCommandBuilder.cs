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
		public bool GuildOnly { get; private set; }
		public Permissions? DefaultPermissions { get; private set; }
		
		public Dictionary<Localization, string> NameLocalizations { get; private set; }
		public Dictionary<Localization, string> DescriptionLocalizations { get; private set; }

		public ApplicationCommandBuilder(ApplicationCommandType type)
		{
			if (type == ApplicationCommandType.AutoCompleteRequest)
				throw new ArgumentException("ApplicationCommand type cannot be AutoCompleteRequest");
			Type = type;
			if (type == ApplicationCommandType.SlashCommand) Options = new List<ApplicationCommandOptionBuilder>();
			NameLocalizations = new Dictionary<Localization, string>();
			DescriptionLocalizations = new Dictionary<Localization, string>();
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
		[Obsolete("Deprecated by Discord. Use the new slash command permissions instead (WithDefaultPermissions())")]
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
		/// Adds a localization data for this commands name
		/// </summary>,
		/// <param name="language">The language to apply this text to</param>
		/// <param name="value">The text to show with this language</param>
		/// <returns>The ApplicationCommandBuilder to be chained</returns>
		public ApplicationCommandBuilder WithNameLocalization(Localization language, string value)
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
		public ApplicationCommandBuilder WithDescriptionLocalization(Localization language, string value)
		{
			if (DescriptionLocalizations.ContainsKey(language))
				DescriptionLocalizations[language] = value;
			else
				DescriptionLocalizations.Add(language, value);
			return this;
		}

		/// <summary>
		/// Sets if this command should only be available in guilds
		/// </summary>
		/// <param name="guildOnly">If this command should only available in guilds</param>
		/// <returns>The ApplicationCommandBuilder to be chained</returns>
		public ApplicationCommandBuilder WithGuildOnly(bool guildOnly)
		{
			GuildOnly = guildOnly;
			return this;
		}

		/// <summary>
		/// Sets the default permissions of this command
		/// </summary>
		/// <param name="permissions">The default permissions for this command</param>
		/// <returns>The ApplicationCommandBuilder to be chained</returns>
		public ApplicationCommandBuilder WithDefaultPermissions(Permissions? permissions)
		{
			DefaultPermissions = permissions;
			return this;
		}


		/// <summary>
		/// Builds this ApplicationCommandBuilder
		/// </summary>
		/// <returns>A DiscordApplicationCommand to be sent to Discord</returns>
		public DiscordApplicationCommand Build() =>
			new(Name, Description, Options?.Select(x => x.Build()), DefaultPermission, Type,
				NameLocalizations.ToDictionary(x => x.Key.GetLanguageCode(), x => x.Value),
				DescriptionLocalizations.ToDictionary(x => x.Key.GetLanguageCode(), x => x.Value),
				!GuildOnly, DefaultPermissions);
	}
}