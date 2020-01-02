using System;
using UnityEngine;

namespace Elia.Unity.Logging
{
    /// <summary>
    /// Interface for class to provide <see cref="ILogger"/> instances.
    /// </summary>
	public interface ILoggerProvider : IDisposable
	{
        /// <summary>
        /// Crete logger instance.
        /// </summary>
        /// <returns>Logger instance</returns>
		ILogger CreateLogger();
	}
}
