using System;

namespace DSharpPlus.SlashCommands
{
    /// <summary>
    /// Handles autocomplete choices for a slash command parameter.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
	public class AutocompleteAttribute : Attribute
	{
        /// <summary>
        /// The provider for this autocomplete parameter.
        /// </summary>
        public Type Provider { get; }

        /// <summary>
        /// Handles autocomplete choices for a slash command parameter.
        /// </summary>
        /// <param name="provider">
        /// The type of the autcomplete provider. This should inherit from
        /// <see cref="IAutocompleteProvider" />.
        /// </param>
        public AutocompleteAttribute(Type provider)
		{
			Provider = provider;
		}
	}
}