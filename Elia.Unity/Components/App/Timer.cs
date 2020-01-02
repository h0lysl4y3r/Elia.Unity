using System;
using UnityEngine;

namespace Elia.Unity.Components.App
{
    /// <summary>
    /// Unity time wrapper.
    /// </summary>
	public sealed class Timer
	{
        #region Actions

        /// <summary>
        /// Invoked in <see cref="StartTimer"/> method.
        /// </summary>
        public Action Started;

        /// <summary>
        /// Invoked in <see cref="ResetTimer"/> method if notification is allowed.
        /// </summary>
		public Action Reset;

        /// <summary>
        /// Invoked in <see cref="IsPaused"/> property on value change.
        /// </summary>
		public Action<bool> Paused;

        #endregion

        #region Members

        #region Queries

        /// <summary>
        /// Integer value of <see cref="RealTimeFloat"/> (value in seconds)
        /// </summary>
        public int RealTime { get { return (int) RealTimeFloat; } }

        /// <summary>
        /// Difference of <see cref="Time.realtimeSinceStartup"/> and real time set at <see cref="ResetRealTime"/> or <see cref="ResetTimer"/> method call (in seconds)
        /// </summary>
        public double RealTimeFloat { get { return Time.realtimeSinceStartup - _realTime; } }

        /// <summary>
        /// Integer value of <see cref="GameTimeFloat"/> (value in seconds)
        /// </summary>
        public int GameTime { get { return (int) GameTimeFloat; } }

        /// <summary>
        /// Scaled time elapsed since start of this component (value in seconds)
        /// </summary>
		public double GameTimeFloat { get { return _gameTime; } }

        /// <summary>
        /// Integer value of <see cref="FixedTimeFloat"/> (value in seconds)
        /// </summary>
        public int FixedTime { get { return (int)FixedTimeFloat; } }

        /// <summary>
        /// Fixed time (given by <see cref="FixedUpdate"/> method) elapsed since start of this component (value in seconds)
        /// </summary>
		public double FixedTimeFloat { get { return FixedTimeFloat; } }

        /// <summary>
        /// Time scale to influence <see cref="GameTimeFloat"/>
        /// </summary>
		public double TimeScale { get { return _timeScale; } }

		/// <summary>
		/// True to set <see cref="Time.timeScale"/> to 0 on paused
		/// </summary>
		public bool AffectUnityTimeScale { get; set; }

		#endregion

		/// <summary>
		/// Gets/sets timer's pause status. On value changed <see cref="Paused"/> is invoked.
		/// </summary>
		public bool IsPaused
		{
			get { return _isPaused; }
			set
			{
				if (_isPaused == value) return;

				_isPaused = value;
				if (AffectUnityTimeScale) Time.timeScale = _isPaused ? 0f : 1f;
				Paused?.Invoke(_isPaused);
			}
		}
		private bool _isPaused = true;

		private double _realTime;
		private double _gameTime;
        private double _fixedTime;
        private double _timeScale;

        #endregion

        #region Constructors and Init

        /// <summary>
        /// Warning: Timer should be constructed in <see cref="MonoBehaviour"/> Start method, since <see cref="Time.realtimeSinceStartup"/> is not yet ready there.
        /// Alternatively <see cref="ResetTimer"/> method may be called in <see cref="MonoBehaviour"/> Start method to set real time correctly.
        /// </summary>
        public Timer()
		{
			ResetTimer(false);
			SetTimeScale();
		}

        /// <summary>
        /// Returns instance of <see cref="Timer"/> and calls <see cref="StartTimer"/> method.
        /// </summary>
        /// <returns>Started instance of <see cref="Timer"/></returns>
		public static Timer CreateAndStartTimer()
		{
			var timer = new Timer();
			timer.StartTimer();
			return timer;
		}

		#endregion

		#region Management

        /// <summary>
        /// If <see cref="IsPaused"/> is true, updates <see cref="GameTimeFloat"/>.
        /// </summary>
		public void Update()
		{
			if (IsPaused) return;

			_gameTime += Time.unscaledDeltaTime * _timeScale;
		}

        /// <summary>
        /// If <see cref="IsPaused"/> is true, updates <see cref="FixedTimeFloat"/>.
        /// </summary>
        public void FixedUpdate()
        {
            if (IsPaused) return;

            _fixedTime += Time.fixedDeltaTime;
        }

        /// <summary>
        /// Sets <see cref="IsPaused"/> to false and invokes <see cref="Started"/> action.
        /// </summary>
		public void StartTimer()
		{
			IsPaused = false;
			Started?.Invoke();
		}

        /// <summary>
        /// Resets real time and game time and invokes <see cref="Reset"/> action if <paramref name="notifyReset"/> is set to true.
        /// </summary>
        /// <param name="notifyReset">True to invoke <see cref="Reset"/> action</param>
		public void ResetTimer(bool notifyReset = true)
		{
			ResetRealTime();
			ResetGameTime();
            ResetFixedTime();

			if (notifyReset) Reset?.Invoke();
		}

        /// <summary>
        /// Resets game time
        /// </summary>
		public void ResetGameTime()
		{
			_gameTime = 0f;
		}

        /// <summary>
        /// Resets fixed time
        /// </summary>
		public void ResetFixedTime()
        {
            _fixedTime = 0f;
        }

        /// <summary>
        /// Resets real time to latest <see cref="Time.realtimeSinceStartup"/>.
        /// </summary>
		public void ResetRealTime()
		{
			_realTime = Time.realtimeSinceStartup;
		}

        /// <summary>
        /// Sets current time scale of <see cref="Timer"/>. If <paramref name="newScale"/> is -1f, <see cref="Time.timeScale"/> is used.
        /// </summary>
        /// <param name="newScale">New time scale</param>
		public void SetTimeScale(float newScale = -1f)
		{
			_timeScale = newScale >= 0 ? newScale : Time.timeScale;
		}

        /// <summary>
        /// Offsets game, fixed and real time by given value.
        /// </summary>
        /// <param name="gameTimeOffset">Game time offset</param>
        /// <param name="realTimeOffset">Real time offset</param>
        /// <param name="fixedTimeOffset">Fixed time offset</param>
		public void OffsetTime(double gameTimeOffset, double realTimeOffset, double fixedTimeOffset)
		{
			if (_gameTime + gameTimeOffset < 0f) throw new ArgumentOutOfRangeException(nameof(gameTimeOffset));
			if (_realTime + realTimeOffset < 0f) throw new ArgumentOutOfRangeException(nameof(realTimeOffset));
            if (_fixedTime + fixedTimeOffset < 0f) throw new ArgumentOutOfRangeException(nameof(fixedTimeOffset));

            _gameTime += gameTimeOffset;
            _realTime += realTimeOffset;
            _fixedTime += fixedTimeOffset;

        }

		#endregion
	}
}
