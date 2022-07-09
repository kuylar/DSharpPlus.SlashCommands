using System;

namespace DSharpPlus.SlashCommands
{
	/// <summary>
	///     Sets a maximum allowed length for this slash command option. Only valid for <see cref="long" /> or <see cref="double" />
	///     parameters.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter)]
	public class MaximumLengthAttribute : Attribute
	{
		/// <summary>
		///     The value.
		/// </summary>
		public int Length { get; internal set; }

		/// <summary>
		///     Sets a maximum allowed length for this slash command option. Only valid for <see cref="string" /> parameters.
		/// </summary>
		public MaximumLengthAttribute(int length)
		{
			Length = length;
		}
	}
}