using System;
using UnityEngine;

namespace Elia.Unity.Components.Games
{
    /// <summary>
    /// <see cref="Modules.Game"/> metadata.
    /// </summary>
	public class GameMeta : ScriptableObject
	{
        /// <summary>
        /// Game id
        /// </summary>
		public string Id;
        /// <summary>
        /// Game name
        /// </summary>
		public string Name;
        /// <summary>
        /// Name of currency type 1
        /// </summary>
		public string Currency1Name;
        /// <summary>
        /// Name of currency type 2
        /// </summary>
		public string Currency2Name;
        /// <summary>
        /// Name of currency type 3
        /// </summary>
		public string Currency3Name;
        /// <summary>
        /// Name of currency type 4
        /// </summary>
		public string Currency4Name;
        /// <summary>
        /// Name of score type 1
        /// </summary>
		public string Score1Name;
        /// <summary>
        /// Name of score type 2
        /// </summary>
		public string Score2Name;
        /// <summary>
        /// Name of score type 3
        /// </summary>
		public string Score3Name;
        /// <summary>
        /// Name of score type 4
        /// </summary>
		public string Score4Name;
	}
}
