using UnityEngine;

namespace Elia.Unity.Logging
{
    /// <summary>
    /// Wraps Unity <see cref="Debug.unityLogger"/>.
    /// </summary>
	public class UnityLogProvider : ILoggerProvider
	{
		#region ILoggerProvider
	
		public ILogger CreateLogger()
		{
			return Debug.unityLogger;
		}

		public void Dispose()
		{
		}

		#endregion
	}
}
