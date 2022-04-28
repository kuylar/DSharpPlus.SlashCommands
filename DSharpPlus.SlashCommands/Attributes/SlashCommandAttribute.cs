using System;

namespace DSharpPlus.SlashCommands
{
	/// <summary>
	///     Marks this method as a slash command.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method)]
	public sealed class SlashCommandAttribute : Attribute
	{
		/// <summary>
		///     Gets the name of this command.
		/// </summary>
		public string Name { get; }

		/// <summary>
		///     Gets the description of this command.
		/// </summary>
		public string Description { get; }

		/// <summary>
		///     Gets whether this command should be localized using an ILocalizationProvider.
		/// </summary>
		public bool ApplyLocalization { get; }

		/// <summary>
		/// Gets the default permissions for this command
		/// </summary>
		public bool DefaultPermission { get; } = true;

		/// <summary>
		/// Gets the default permissions for this command
		/// </summary>
		public Permissions? DefaultPermissions { get; }
		
		/// <summary>
		/// Gets if this command should only be used in guilds
		/// </summary>
		public bool GuildOnly { get; }

		/// <summary>
		///     Marks this method as a slash command.
		/// </summary>
		/// <param name="name">Sets the name of this slash command.</param>
		/// <param name="description">Sets the description of this slash command.</param>
		/// <param name="defaultPermission">Sets whether the command should be enabled by default.</param>
		/// <param name="applyLocalization">Sets whether the command should be localized using an ILocalizationProvider.</param>
		[Obsolete("Use the new Slash Commands v2 compliant overload instead")]
		public SlashCommandAttribute(string name, string description, bool defaultPermission, bool applyLocalization = false)
		{
			Name = name.ToLower();
			Description = description;
			DefaultPermission = defaultPermission;
			ApplyLocalization = applyLocalization;
		}

		/// <summary>
		///     Marks this method as a slash command.
		/// </summary>
		/// <param name="name">Sets the name of this slash command.</param>
		/// <param name="description">Sets the description of this slash command.</param>
		/// <param name="applyLocalization">Sets whether the command should be localized using an ILocalizationProvider.</param>
		/// <param name="permissions">Sets the default permissions for this command</param>
		/// <param name="guildOnly">Sets if this command should only be used in guilds</param>
		public SlashCommandAttribute(string name, string description, Permissions permissions, bool guildOnly = false, bool applyLocalization = false)
		{
			Name = name.ToLower();
			Description = description;
			ApplyLocalization = applyLocalization;
			DefaultPermissions = permissions;
			GuildOnly = guildOnly;
		}

		/// <summary>
		///     Marks this method as a slash command.
		/// </summary>
		/// <param name="name">Sets the name of this slash command.</param>
		/// <param name="description">Sets the description of this slash command.</param>
		/// <param name="applyLocalization">Sets whether the command should be localized using an ILocalizationProvider.</param>
		public SlashCommandAttribute(string name, string description, bool applyLocalization = false)
		{
			Name = name.ToLower();
			Description = description;
			ApplyLocalization = applyLocalization;
		}
	}
}