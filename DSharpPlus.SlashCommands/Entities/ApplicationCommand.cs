using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DSharpPlus.SlashCommands.Entities
{
	public class ApplicationCommand
	{
		public ApplicationCommandBuilder Builder;
		public ulong GuildId;
		public Dictionary<string, MethodInfo> Methods;
		public Dictionary<string, MethodInfo> AutocompleteMethods;

		internal ApplicationCommand(ApplicationCommandBuilder builder, ulong guildId)
		{
			Builder = builder;
			GuildId = guildId;
			Methods = new Dictionary<string, MethodInfo>();
			AutocompleteMethods = new Dictionary<string, MethodInfo>();

			if (builder.Options?.FirstOrDefault()?.Type is ApplicationCommandOptionType.SubCommand)
			{
				foreach (ApplicationCommandOptionBuilder option in builder.Options)
				{
					Methods.Add(option.Name, option.Method);
					string optionName = option.Name + " ";
					foreach (ApplicationCommandOptionBuilder autocompleteOption in option.Options.Where(x =>
						x.AutoCompleteMethod != null))
						AutocompleteMethods.Add(optionName + autocompleteOption.Name,
							autocompleteOption.AutoCompleteMethod);
				}
			}
			else if (builder.Options?.FirstOrDefault()?.Type is ApplicationCommandOptionType.SubCommandGroup)
			{
				foreach (ApplicationCommandOptionBuilder group in builder.Options)
				{
					string groupName = group.Name + " ";
					foreach (ApplicationCommandOptionBuilder option in group.Options)
					{
						Methods.Add(groupName + option.Name, option.Method);
						string optionName = groupName + option.Name + " ";
						foreach (ApplicationCommandOptionBuilder autocompleteOption in option.Options.Where(x =>
							x.AutoCompleteMethod != null))
							AutocompleteMethods.Add(optionName + autocompleteOption.Name,
								autocompleteOption.AutoCompleteMethod);
					}
				}
			}
			else
			{
				Methods.Add(string.Empty, builder.Method);

				foreach (ApplicationCommandOptionBuilder autocompleteOption in builder.Options?.Where(x =>
					x.AutoCompleteMethod != null) ?? Array.Empty<ApplicationCommandOptionBuilder>())
					AutocompleteMethods.Add(autocompleteOption.Name, autocompleteOption.AutoCompleteMethod);
			}
		}
	}
}