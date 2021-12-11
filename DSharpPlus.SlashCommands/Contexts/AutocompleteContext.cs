using System;
using System.Collections.Generic;
using DSharpPlus.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace DSharpPlus.SlashCommands
{
	/// <summary>
	///     Represents a context for an autocomplete interaction.
	/// </summary>
	public class AutocompleteContext
	{
		/// <summary>
		///     The interaction created.
		/// </summary>
		public DiscordInteraction Interaction { get; internal set; }

		/// <summary>
		///     Gets the client for this interaction.
		/// </summary>
		public DiscordClient Client { get; internal set; }

		/// <summary>
		///     Gets the guild this interaction was executed in.
		/// </summary>
		public DiscordGuild Guild { get; internal set; }

		/// <summary>
		///     Gets the channel this interaction was executed in.
		/// </summary>
		public DiscordChannel Channel { get; internal set; }

		/// <summary>
		///     Gets the user which executed this interaction.
		/// </summary>
		public DiscordUser User { get; internal set; }

		/// <summary>
		///     Gets the member which executed this interaction, or null if the command is in a DM.
		/// </summary>
		public DiscordMember Member
			=> User is DiscordMember member ? member : null;

		/// <summary>
		///     Gets the slash command module this interaction was created in.
		/// </summary>
		public SlashCommandsExtension SlashCommandsExtension { get; internal set; }

		/// <summary>
		///     <para>Gets the service provider.</para>
		///     <para>This allows passing data around without resorting to static members.</para>
		///     <para>Defaults to null.</para>
		/// </summary>
		public IServiceProvider Services { get; internal set; } = new ServiceCollection().BuildServiceProvider(true);

		/// <summary>
		///     The options already provided.
		/// </summary>
		public IReadOnlyList<DiscordInteractionDataOption> Options { get; internal set; }

		/// <summary>
		///     The option to auto-fill for.
		/// </summary>
		public DiscordInteractionDataOption FocusedOption { get; internal set; }

		/// <summary>
		///     The given value of the focused option.
		/// </summary>
		public object OptionValue => FocusedOption.Value;
	}
}