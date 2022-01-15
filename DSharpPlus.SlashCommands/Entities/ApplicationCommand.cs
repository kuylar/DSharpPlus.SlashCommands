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

			if (builder.Options != null && builder.Options.Any() && builder.Options.First().Type is ApplicationCommandOptionType.SubCommand or ApplicationCommandOptionType
				.SubCommandGroup)
			{
				foreach (ApplicationCommandOptionBuilder option in builder.Options)
					switch (option.Type)
					{
						case ApplicationCommandOptionType.SubCommand:
						{
							Methods.Add(option.Name, option.Method);
							string optionName = option.Name + " ";
							foreach (ApplicationCommandOptionBuilder autocompleteOption in option.Options.Where(x =>
								x.AutoCompleteMethod != null))
								AutocompleteMethods.Add(optionName + autocompleteOption.Name,
									autocompleteOption.AutoCompleteMethod);
							break;
						}
						case ApplicationCommandOptionType.SubCommandGroup:
						{
							string groupName = option.Name + " ";
							foreach (ApplicationCommandOptionBuilder subOption in option.Options)
							{
								Methods.Add(groupName + subOption.Name, subOption.Method);
								string optionName = groupName + subOption.Name + " ";
								foreach (ApplicationCommandOptionBuilder autocompleteOption in subOption.Options.Where(x =>
									x.AutoCompleteMethod != null))
									AutocompleteMethods.Add(optionName + autocompleteOption.Name,
										autocompleteOption.AutoCompleteMethod);
							}
							break;
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