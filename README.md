# Kuylar.DSharpPlus.SlashCommands

> You must use the nightly versions of DSharpPlus to use this package. Make sure to enable the Pre-release checkbox in your NuGet manager

This is a "better" replacement for the current DSharpPlus. [Most of the same documentation](https://github.com/DSharpPlus/DSharpPlus/blob/master/DSharpPlus.SlashCommands/README.md) will also apply to this extension. 

---

# Localization tutorial

> WARNING: In the time of writing this document (25 March 2022), slash command localization are in open beta and not available right away. Before developing them, make sure you enabled the build override (can be found in the https://discord.gg/discord-developers server)

To localize your slash commands, you will need a Localization Provider. An example localization provider can be seen below.

```c#
// Warning: This is an example. PLEASE do not write a localization provider using ifs and switches
public class LocalizationProvider : ILocalizationProvider
{
	public string GetLocalizedString(LocalizationContext localizationContext, Localization language, string key)
	{
		if (key is not ("color" or "colorDescription")) return null;
		if (localizationContext is not (LocalizationContext.SlashCommandName or LocalizationContext
			.SlashCommandDescription)) return null;

		switch (language)
		{
			case Localization.AmericanEnglish:
				if (key == "color") return "Color";
				if (key == "colorDescription") return "Get a color!";
				break;
			case Localization.BritishEnglish:
				if (key == "coloor") return "Colour";
				if (key == "colorDescription") return "Get a colour!";
				break;
			default: return null;
		}

		return null;
	}
}
```

As you can see, we use the keys and the localization context to make sure that were translating into the correct string.

After adding a localization provider, you can add a slash command like so:

```c#
// See how the keys and these values match each other
[SlashCommand("color", "colorDescription", applyLocalization: true)]
public async Task LocalTestCommand(InteractionContext ctx) => await ctx.CreateResponseAsync("You get **blue**!");
```

And don't forget to register our localization provider 

```c#
SlashCommandsExtension slash = _client.UseSlashCommands(new SlashCommandsConfiguration
{
	LocalizationProvider = new LocalizationProvider()
});
```

If you did all these correctly, the command should appear differently if you set your Discord client when you change your client's language between English UK and English US.