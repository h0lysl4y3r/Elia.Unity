using UnityEngine;

namespace Elia.Unity.Helpers
{
    /// <summary>
    /// Helper methods within <see cref="App"/> context.
    /// </summary>
	public static class AppHelpers
	{
        /// <summary>
        /// Determines whether application runs on mobile or desktop.
        /// </summary>
        /// <returns>True if application runs on mobile</returns>
		public static bool IsTouchDevice()
		{
			var platform = Application.platform;
			return platform == RuntimePlatform.IPhonePlayer || platform == RuntimePlatform.Android;
		}
	}
}
