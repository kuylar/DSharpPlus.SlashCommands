using System;

namespace DSharpPlus.SlashCommands
{
	/// <summary>
	///     Sets a minimum allowed length for this slash command option. Only valid for <see cref="string" /> parameters.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter)]
	public class MinimumLengthAttribute : Attribute
	{
		/// <summary>
		///     The value.
		/// </summary>
		public int Length { get; internal set; }

		/// <summary>
		///     Sets a minimum allowed length for this slash command option. Only valid for <see cref="string" /> parameters.
		/// </summary>
		public MinimumLengthAttribute(int length)
		{
			Length = length;
		}
	}
}