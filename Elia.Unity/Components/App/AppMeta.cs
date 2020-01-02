using System;
using UnityEngine;

namespace Elia.Unity.Components.App
{
	public class AppMeta : ScriptableObject
	{
		// Data

        /// <summary>
        /// Application id
        /// </summary>
		public string Id;
        /// <summary>
        /// Application name
        /// </summary>
		public string Name;
        /// <summary>
        /// Application description
        /// </summary>
		[Multiline]
		public string Description;
        /// <summary>
        /// Application version
        /// </summary>
		public Version Version => new Version(VersionMajor, VersionMinor, VersionBuild);
        /// <summary>
        /// Application version major
        /// </summary>
		public int VersionMajor;
        /// <summary>
        /// Application version minor
        /// </summary>
		public int VersionMinor;
        /// <summary>
        /// Application version build
        /// </summary>
		public int VersionBuild;

		// Settings

        /// <summary>
        /// True if this is production build
        /// </summary>
		public bool IsProduction;
        /// <summary>
        /// Custom level of debug information
        /// </summary>
		public int DebugLevel;
        /// <summary>
        /// True to auto (de)activate screen's <see cref="GameObject"/> upon (de)activation.
        /// </summary>
		public bool ScreensAutoActivateGameObjects = true;
        /// <summary>
        /// True to auto (de)activate modal's <see cref="GameObject"/> upon (de)activation.
        /// </summary>
		public bool ModalsAutoActivateGameObjects = true;
        /// <summary>
        /// True to auto (de)activate overlay's <see cref="GameObject"/> upon (de)activation.
        /// </summary>
		public bool OverlaysAutoActivateGameObjects = true;
        /// <summary>
        /// True to auto activate screen's/modal's/overlay's <see cref="GameObject"/> upon activation. After activation (Awake is called) is immediately deactivated.
        /// </summary>
	    public bool ActivateDeactivateGUIOnStartup = true;
		/// <summary>
		/// True to set <see cref="Time.timeScale"/> to 0 on <see cref="Modules.App"/> pause.
		/// </summary>
		public bool SetTimeScaleOnPause = false;
	}
}
