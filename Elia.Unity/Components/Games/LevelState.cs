using Elia.Unity.Serialization;

namespace Elia.Unity.Components.Games
{
    /// <summary>
    /// <see cref="Level"/> state that is serialized.
    /// </summary>
	public abstract class LevelState
	{
        /// <summary>
        /// Id of current scene from <see cref="GameModeMeta.SceneNames"/> that should be loaded
        /// </summary>
		[SerializePrefs]
		public virtual int CurrentSceneId { get; set; }
        /// <summary>
        /// Id of current level
        /// </summary>
        [SerializePrefs]
        public virtual int CurrentLevelId { get; set; }
        /// <summary>
        /// Value of score type 1
        /// </summary>
        [SerializePrefs]
		public virtual double Score1 { get; set; }
        /// <summary>
        /// Value of score type 2
        /// </summary>
		[SerializePrefs]
		public virtual double Score2 { get; set; }
        /// <summary>
        /// Value of score type 3
        /// </summary>
		[SerializePrefs]
		public virtual double Score3 { get; set; }
        /// <summary>
        /// Value of score type 4
        /// </summary>
		[SerializePrefs]
		public virtual double Score4 { get; set; }
        /// <summary>
        /// Value of currency type 1
        /// </summary>
		[SerializePrefs]
		public virtual double Currency1 { get; set; }
        /// <summary>
        /// Value of currency type 2
        /// </summary>
		[SerializePrefs]
		public virtual double Currency2 { get; set; }
        /// <summary>
        /// Value of currency type 3
        /// </summary>
		[SerializePrefs]
		public virtual double Currency3 { get; set; }
        /// <summary>
        /// Value of currency type 4
        /// </summary>
		[SerializePrefs]
		public virtual double Currency4 { get; set; }
        /// <summary>
        /// Current game time
        /// </summary>
		[SerializePrefs]
		public virtual double Time { get; set; }
	}
}
