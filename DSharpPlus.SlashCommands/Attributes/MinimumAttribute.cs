using System;

namespace DSharpPlus.SlashCommands
{
	/// <summary>
	///     Sets a minimum value for this slash command option. Only valid for <see cref="long" /> or <see cref="double" />
	///     parameters.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter)]
	public class MinimumAttribute : Attribute
	{
		/// <summary>
		///     The value.
		/// </summary>
		public object Value { get; internal set; }

		/// <summary>
		///     Sets a minimum value for this slash command option. Only valid for <see cref="long" /> or <see cref="double" />
		///     parameters.
		/// </summary>
		public MinimumAttribute(long value)
		{
			Value = value;
		}

		/// <summary>
		///     Sets a minimum value for this slash command option. Only valid for <see cref="long" /> or <see cref="double" />
		///     parameters.
		/// </summary>
		public MinimumAttribute(double value)
		{
			Value = value;
		}
	}
}