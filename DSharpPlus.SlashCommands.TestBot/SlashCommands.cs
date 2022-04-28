using System;
using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace DSharpPlus.SlashCommands.TestBot
{
	public class SlashCommands : ApplicationCommandModule
	{
		#region Before / After

		public override Task<bool> BeforeSlashExecutionAsync(InteractionContext ctx)
		{
			Console.WriteLine($"Before executing the slash command '{ctx.CommandName}'");
			return Task.FromResult(true);
		}

		public override Task AfterSlashExecutionAsync(InteractionContext ctx)
		{
			Console.WriteLine($"After executing the slash command '{ctx.CommandName}'");
			return Task.FromResult(true);
		}

		#endregion

		[SlashCommand("string", "String")]
		public async Task StringCommand(InteractionContext ctx,
			[Option("string", "Required string")]
			string required,
			[Option("opt-string", "Optional string")]
			string optional = "DEFAULT_VALUE") =>
			await ctx.CreateResponseAsync($"Required: {required}\nOptional: {optional}");

		[SlashCommand("int", "Integer")]
		public async Task IntegerCommand(InteractionContext ctx,
			[Option("int", "Required int")]
			long required,
			[Option("opt-int", "Optional int")]
			long optional = long.MinValue) =>
			await ctx.CreateResponseAsync($"Required: {required}\nOptional: {optional}");

		[SlashCommand("bool", "Boolean")]
		public async Task BooleanCommand(InteractionContext ctx,
			[Option("bool", "Required bool")]
			bool required,
			[Option("opt-bool", "Optional bool")]
			bool optional = false) =>
			await ctx.CreateResponseAsync($"Required: {required}\nOptional: {optional}");

		[SlashCommand("user", "User")]
		public async Task UserCommand(InteractionContext ctx,
			[Option("user", "Required user")]
			DiscordUser required,
			[Option("opt-user", "Optional user")]
			DiscordUser optional = null) =>
			await ctx.CreateResponseAsync($"Required: {required.Mention}\nOptional: {optional?.Mention}");

		[SlashCommand("channel", "Channel")]
		public async Task ChannelCommand(InteractionContext ctx,
			[Option("channel", "Required channel")]
			DiscordChannel required,
			[Option("opt-channel", "Optional channel")]
			DiscordChannel optional = null) =>
			await ctx.CreateResponseAsync($"Required: {required.Mention}\nOptional: {optional?.Mention}");

		[SlashCommand("role", "Role")]
		public async Task RoleCommand(InteractionContext ctx,
			[Option("role", "Required role")]
			DiscordRole required,
			[Option("opt-role", "Optional role")]
			DiscordRole optional = null) =>
			await ctx.CreateResponseAsync($"Required: {required.Mention}\nOptional: {optional?.Mention}");

		[SlashCommand("mentionable", "Mentionable")]
		public async Task MentionableCommand(InteractionContext ctx,
			[Option("mentionable", "Required mentionable")]
			SnowflakeObject required,
			[Option("opt-mentionable", "Optional mentionable")]
			SnowflakeObject optional = null) =>
			await ctx.CreateResponseAsync($"Required: {required}\nOptional: {optional}");

		[SlashCommand("number", "Number")]
		public async Task NumberCommand(InteractionContext ctx,
			[Option("number", "Required number")]
			double required,
			[Option("opt-number", "Optional number")]
			double optional = double.MinValue) =>
			await ctx.CreateResponseAsync($"Required: {required}\nOptional: {optional}");

		[SlashCommand("attributes", "Test of all attributes")]
		public async Task Attributes(InteractionContext ctx,
			[Option("autocomplete", "Non-functional autocomplete")]
			[Autocomplete(typeof(IAutocompleteProvider))]
			string autocomplete,
			[Option("vc", "Select a voice channel")]
			[ChannelTypes(ChannelType.Voice)]
			DiscordChannel channel,
			[Option("choices", "You must select one from many")]
			[Choice("I lied, there arent 'many', theres only one", "lol")]
			string choice,
			[Option("number", "Number between 10-15")]
			[Minimum(10)] [Maximum(15)]
			double number) =>
			await ctx.CreateResponseAsync($"Autocomplete: {autocomplete}\nVoice channel: {channel.Mention}\nChoice: `{choice}`\nNumber: 9<{number}<16");

		[SlashCommand("autocomplete", "Autocomplete")]
		public async Task AutocompleteCommand(InteractionContext ctx,
			[Option("option1", "Functional autocomplete")]
			[Autocomplete(typeof(Autocomplete))]
			string option1,
			[Option("option2", "Functional autocomplete")]
			[Autocomplete(typeof(Autocomplete))]
			string option2) =>
			await ctx.CreateResponseAsync($"You chose:\n{option1}\n{option2}");

		public enum CoolEnum
		{
			[ChoiceName("Option with a name attribute")]
			OptionWithName,
			OptionWithoutAName
		}
		
		[SlashCommand("enum", "Enum")]
		public async Task EnumCommand(InteractionContext ctx,
			[Option("enum", "Fun fact: this will fail")]
			CoolEnum option) =>
			await ctx.CreateResponseAsync($"You chose:\n{option}");
		
		[SlashCommand("attachment", "Attachment")]
		public async Task AttachmentCommand(InteractionContext ctx,
			[Option("required", "I require your files!")]
			DiscordAttachment required,
			[Option("optional", "I require your files!")]
			DiscordAttachment optional = null) =>
			await ctx.CreateResponseAsync($"Your file: [{required.MediaType}]\n{required.ProxyUrl}\n\nYour (optional) file: [{optional?.MediaType}]\n{optional?.ProxyUrl}");
		
		[SlashCommand("member", "Member")]
		public async Task MemberCommand(InteractionContext ctx,
			[Option("member", "Fun fact: this will fail")]
			DiscordUser user) =>
			await ctx.CreateResponseAsync($"{(user as DiscordMember)?.DisplayName ?? "`<NULL>`"} ");

		[SlashCommand("error", "Error")]
		public async Task ErrorCommand(InteractionContext ctx) => throw new Exception("epic error");

		[SlashCommand("adminOnlyCommand", "Only administrators can use this command", Permissions.Administrator)]
		public async Task AdminCommand(InteractionContext ctx) => 
			await ctx.CreateResponseAsync($"Congrats! Yo'ure an admin!!");

		[SlashCommand("deleteMessages", "Delete some messages", Permissions.ManageMessages)]
		public async Task DeleteMessagesCommand(InteractionContext ctx) => 
			await ctx.CreateResponseAsync("delete them yourself");

		//		[SlashCommand("localization-test", "Localization Test Command", applyLocalization: true)]
//		public async Task LocalTestCommand(InteractionContext ctx) => await ctx.CreateResponseAsync($"Your localization value is: {LocalizationExtensions.GetLanguageFromCode(ctx.Interaction.Locale).GetNativeName()}");
	}
}