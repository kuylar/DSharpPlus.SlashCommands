namespace DSharpPlus.SlashCommands
{
	public class NullLocalizationProvider : ILocalizationProvider
	{
		public string GetLocalizedString(LocalizationContext localizationContext, Localization language, string key)
		{
			return null;
		}
	}
}