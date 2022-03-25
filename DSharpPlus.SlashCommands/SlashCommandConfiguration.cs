using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace DSharpPlus.SlashCommands
{
    /// <summary>
    ///     A configuration for a <see cref="SlashCommandsExtension" />.
    /// </summary>
    public sealed class SlashCommandsConfiguration
	{
        /// <summary>
        ///     <para>Sets the service provider.</para>
        ///     <para>
        ///         Objects in this provider are used when instantiating slash command modules. This allows passing data around
        ///         without resorting to static members.
        ///     </para>
        ///     <para>Defaults to null.</para>
        /// </summary>
        public IServiceProvider Services { internal get; set; } = new ServiceCollection().BuildServiceProvider(true);

        /// <summary>
        /// The localization provider to localize the command names and descriptions.
        /// Defaults to <see cref="NullLocalizationProvider"/> which doesn't use any localization
        /// </summary>
        public ILocalizationProvider LocalizationProvider = new NullLocalizationProvider();

        /// <summary>
        /// The languages that the lib try to localize to.
        /// Contains all languages by default
        /// </summary>
        public List<Localization> LocalizationWhitelist = new()
        {
	        Localization.Danish,
	        Localization.German,
	        Localization.BritishEnglish,
	        Localization.AmericanEnglish,
	        Localization.Spanish,
	        Localization.French,
	        Localization.Croatian,
	        Localization.Italian,
	        Localization.Lithuanian,
	        Localization.Hungarian,
	        Localization.Dutch,
	        Localization.Norwegian,
	        Localization.Polish,
	        Localization.Portuguese,
	        Localization.Romanian,
	        Localization.Finnish,
	        Localization.Swedish,
	        Localization.Vietnamese,
	        Localization.Turkish,
	        Localization.Czech,
	        Localization.Greek,
	        Localization.Bulgarian,
	        Localization.Russian,
	        Localization.Ukrainian,
	        Localization.Hindi,
	        Localization.Thai,
	        Localization.Chinese,
	        Localization.Japanese,
	        Localization.TaiwanChinese,
	        Localization.Korean
        };
	}
}