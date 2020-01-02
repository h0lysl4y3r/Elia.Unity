using System.Linq;
using UnityEngine;

namespace Elia.Unity.Logging
{
    /// <summary>
    /// Factory object to manage <see cref="Logger"/> instances.
    /// </summary>
	public class LoggerFactory
	{
		#region Members

        /// <summary>
        /// <see cref="LoggerFactory"/> singleton.
        /// </summary>
		public static LoggerFactory Instance
		{
			get
			{
				if (_instance == null) _instance = new LoggerFactory();
				return _instance;
			}
		}
		private static LoggerFactory _instance;
		private LoggerFactory() { }

		private ILoggerProvider[] _providers = new ILoggerProvider[0];
		private readonly object _sync = new object();

        #endregion

        #region Logger

        /// <summary>
        /// Creates <see cref="Logger"/> instance.
        /// </summary>
        /// <returns><see cref="Logger"/> instance</returns>
        public ILogger CreateLogger()
		{
			return new Logger(this);
		}

		#endregion

		#region Providers
        
        /// <summary>
        /// Adds <paramref name="provider"/> to factory.
        /// </summary>
        /// <param name="provider"><see cref="ILoggerProvider"/> instance</param>
		public void AddProvider(ILoggerProvider provider)
		{
			lock (_sync)
			{
				_providers = _providers.Concat(new[] { provider }).ToArray();
			}
		}

        /// <summary>
        /// Returns factory <see cref="ILoggerProvider"/> providers.
        /// </summary>
        /// <returns><see cref="ILoggerProvider"/> providers</returns>
		internal ILoggerProvider[] GetProviders()
		{
			return _providers;
		}

        /// <summary>
        /// Disposes factory <see cref="ILoggerProvider"/> providers.
        /// </summary>
		public void DisposeProviders()
		{
			foreach (var provider in _providers)
			{
				try
				{
					provider.Dispose();
				}
				catch
				{
					// Swallow exceptions on dispose
				}
			}
		}

		#endregion
	}
}
