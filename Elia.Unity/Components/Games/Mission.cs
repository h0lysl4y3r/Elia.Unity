using System;

namespace Elia.Unity.Components.Games
{
    public abstract class Mission
    {
        #region Members

        public Level Level { get; private set; }

        public string Name { get; private set; }

        /// <summary>
        /// True if <see cref="Mission"/> was initialized
        /// </summary>
        public bool HasBegun { get; private set; }

        /// <summary>
        /// True if <see cref="Mission"/> was finalized
        /// </summary>
		public bool HasEnded { get; private set; }

        /// <summary>
        /// True if <see cref="Mission"/> was successful
        /// </summary>
		public bool HasSucceeded { get; private set; }

        private Action _missionEnded;

        #endregion

        #region Constructor

        public Mission(string name, Level level, Action missionEnded)
        {
            Name = name;
            Level = level;
            _missionEnded = missionEnded;
        }

        #endregion

        #region Management

        /// <summary>
        /// Used to initialize <see cref="Mission"/>. Sets <see cref="HasBegun"/> to true.
        /// </summary>
		public virtual void Begin()
        {
            if (HasBegun) return;
            HasBegun = true;
            HasEnded = false;
            HasSucceeded = false;
        }

        /// <summary>
        /// Used to finalize <see cref="Mission"/>. Sets <see cref="HasBegun"/> to false. 
        /// Invokes missionEnded action provided in <see cref="Mission"/> constructor.
        /// Is called automatically, so should be 
        /// </summary>
		public virtual void End(bool succeeded)
        {
            if (HasEnded) return;
            HasBegun = false;
            HasEnded = true;
            HasSucceeded = succeeded;

            _missionEnded?.Invoke();
        }

        /// <summary>
        /// Reacts to higher level component changed pause status.
        /// </summary>
        /// <param name="pauseStatus">Pause status</param>
		public virtual void Pause(bool pauseStatus)
        {
        }

        /// <summary>
        /// Updates mission
        /// </summary>
        public virtual void Update() { }

        #endregion
    }
}


