namespace DSharpPlus.SlashCommands
{
	public interface ILocalizationProvider
	{
		/// <summary>
		/// Provide a localized string for the key. Return null to not localize
		/// </summary>
		/// <param name="localizationContext">Where this localization string will be used at</param>
		/// <param name="language">The language you should translate to</param>
		/// <param name="key">The key of this localization</param>
		/// <returns></returns>
		public string GetLocalizedString(LocalizationContext localizationContext, Localization language, string key);
	}
}