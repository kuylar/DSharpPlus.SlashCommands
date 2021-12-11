using System;

namespace DSharpPlus.SlashCommands
{
	/// <summary>
	///     Sets a maximum value for this slash command option. Only valid for <see cref="long" /> or <see cref="double" />
	///     parameters.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter)]
	public class MaximumAttribute : Attribute
	{
		/// <summary>
		///     The value.
		/// </summary>
		public object Value { get; internal set; }

		/// <summary>
		///     Sets a maximum value for this slash command option. Only valid for <see cref="long" /> or <see cref="double" />
		///     parameters.
		/// </summary>
		public MaximumAttribute(long value)
		{
			Value = value;
		}

		/// <summary>
		///     Sets a maximum value for this slash command option. Only valid for <see cref="long" /> or <see cref="double" />
		///     parameters.
		/// </summary>
		public MaximumAttribute(double value)
		{
			Value = value;
		}
	}
}