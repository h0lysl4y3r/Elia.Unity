using System;

namespace Elia.Unity.Helpers
{
    /// <summary>
    /// String formatting helper methods.
    /// </summary>
	public static class Formatting
	{
		public const string HourFormat = "h";
		public const string DoubleHourFormat = "hh";
		public const string MinuteFormat = "m";
		public const string DoubleMinuteFormat = "mm";
		public const string SecondFormat = "s";
		public const string DoubleSecondFormat = "ss";

        /// <summary>
        /// Formats time given in seconds by desired format string.
        /// </summary>
        /// <param name="seconds">Time in seconds</param>
        /// <param name="format">Format string, e.g. mm:ss for output 01:01</param>
        /// <returns>Formatted time in seconds as string value</returns>
		public static string FormatTime(int seconds, string format = "mm:ss")
		{
			var t = new TimeSpan(0, 0, seconds);
			var hours = t.Hours;
			var minutes = format.Contains(HourFormat) ? t.Minutes : (int)t.TotalMinutes;
			var secondsVal = format.Contains(MinuteFormat) ? t.Seconds : (int)t.TotalSeconds;

			var value = format.Replace(DoubleHourFormat, hours.ToString("00"));
			value = value.Replace(HourFormat, hours.ToString());

			value = value.Replace(DoubleMinuteFormat, minutes.ToString("00"));
			value = value.Replace(MinuteFormat, minutes.ToString());

			value = value.Replace(DoubleSecondFormat, secondsVal.ToString("00"));
			value = value.Replace(SecondFormat, secondsVal.ToString());

			return value;
		}
	}
}
