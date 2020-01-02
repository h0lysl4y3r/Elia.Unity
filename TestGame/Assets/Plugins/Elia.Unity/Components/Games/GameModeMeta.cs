using System.Collections.Generic;
using UnityEngine;

namespace Elia.Unity.Components.Games
{
    /// <summary>
    /// <see cref="Modules.Game"/> mode metadata derived from <see cref="ScriptableObject"/>.
    /// </summary>
	public abstract class GameModeMeta : ScriptableObject
	{
        /// <summary>
        /// Game mode name
        /// </summary>
		public string Name;

        /// <summary>
        /// <see cref="Level"/> progress type
        /// </summary>
		public LevelProgressTypeEnum LevelProgressType;

        /// <summary>
        /// List of scene names to load during play of <see cref="GameModeMeta"/>
        /// </summary>
		public List<string> SceneNames;

        /// <summary>
        /// Returns threshold distance value to get to next level
        /// </summary>
        /// <param name="level">Level number</param>
        /// <returns>Threshold distance value</returns>
		public virtual float GetDistanceToNextLevel(int level)
		{
			return 0f;
		}

        /// <summary>
        /// Returns threshold distance value to get to next level
        /// </summary>
        /// <param name="level">Level number</param>
        /// <returns>Threshold score value</returns>
		public virtual int GetScoreToNextLevel(int level)
		{
			return 0;
		}
	}
}
