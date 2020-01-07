using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elia.Unity.Infrastructure
{
	/// <summary>
	/// Interface for Analytics providers
	/// </summary>
	public interface IAnalyticsProvider
	{
		/// <summary>
		/// Invoked on provider initialized.
		/// </summary>
		Action<IAnalyticsProvider> ProviderInitialized { get; set; }

		/// <summary>
		/// Logs event with parameters.
		/// </summary>
		/// <param name="eventName">Event name</param>
		/// <param name="keyValueTuples">Array of key values representing event parameters</param>
		void LogEvent(string eventName, Tuple<string, object>[] keyValueTuples);
	}
}
