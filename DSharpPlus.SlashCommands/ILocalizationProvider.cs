namespace DSharpPlus.SlashCommands
{
	public interface ILocalizationProvider
	{
		public string GetLocalizedString(string localizationContext, string key);
	}
}