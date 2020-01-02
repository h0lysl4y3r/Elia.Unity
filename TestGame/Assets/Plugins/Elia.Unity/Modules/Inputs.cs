using Elia.Unity.Helpers;
using Elia.Unity.DotNet;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Elia.Unity.Modules
{
    /// <summary>
    /// Basic Unity input management component. Encapsulates mobile and desktop input.
    /// </summary>
	[AddComponentMenu("ELIA/Modules/Inputs")]
	public sealed class Inputs : BehaviourAwareSingleton<Inputs>
	{
        #region Actions

        /// <summary>
        /// Invoked on button pressed. Button is specified by id.
        /// </summary>
        public Action<int> Pressed;

        /// <summary>
        /// Invoked on button held every frame. Button is specified by id and position is absolute.
        /// To get delta position, call <see cref="GetDeltaPosition"/> method.
        /// </summary>
        public Action<TupleValue<int, Vector3>> Held;

        /// <summary>
        /// Invoked on button released. Button is specified by id.
        /// </summary>
        public Action<int> Released;

        #endregion

        #region Members

        /// <summary>
        /// If true, auto polls for input changes and invokes <see cref="Pressed"/> action
        /// </summary>
        public bool AutoPollPressed { get; set; }

		/// <summary>
		/// If true, auto polls for input changes and invokes actions <see cref="Held"/> action
		/// </summary>
		public bool AutoPollHeld { get; set; }

		/// <summary>
		/// If true, auto polls for input changes and invokes <see cref="Released"/> action
		/// </summary>
		public bool AutoPollReleased { get; set; }

		private Vector3 _lastMousePosition;
		private long _mousePositionUpdates;

		#endregion

		#region MonoBehaviour

		protected override void LateUpdate()
		{
			base.LateUpdate();

			_mousePositionUpdates++;
			_lastMousePosition = Input.mousePosition;

            for (var i = 0; i < 3; i++)
            {
                if (AutoPollPressed && IsPressed(i)) Pressed?.Invoke(i);
                if (AutoPollHeld && IsHeld(i)) Held?.Invoke(new TupleValue<int, Vector3>(i, GetPosition(i)));
                if (AutoPollReleased && IsReleased(i)) Released?.Invoke(i);
            }
        }

        #endregion

        #region Queries

        /// <summary>
        /// Polls whether button specified was pressed.
        /// </summary>
        /// <param name="button">Unity id of button</param>
        /// <returns>True if button was pressed</returns>
        public bool IsPressed(int button)
		{
			if (AppHelpers.IsTouchDevice())
			{
				if (button >= Input.touches.Length) return false;
				return Input.touches[button].phase == TouchPhase.Began;

			}
			else
			{
				return Input.GetMouseButtonDown(button);
			}
		}

        /// <summary>
        /// Polls whether button specified is being held.
        /// </summary>
        /// <param name="button">Unity id of button</param>
        /// <returns>True if button is being held</returns>
		public bool IsHeld(int button)
		{
			if (AppHelpers.IsTouchDevice())
			{
				if (button >= Input.touches.Length) return false;
				var touch = Input.touches[button];
				return touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary;

			}
			else
			{
				return Input.GetMouseButtonDown(button);
			}
		}

        /// <summary>
        /// Polls whether button specified was released.
        /// </summary>
        /// <param name="button">Unity id of button</param>
        /// <returns>True if button was released</returns>
		public bool IsReleased(int button)
		{
			if (AppHelpers.IsTouchDevice())
			{
				if (button >= Input.touches.Length) return false;
				var touch = Input.touches[button];
				return touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled;
			}
			else
			{
				return Input.GetMouseButtonDown(button);
			}
		}

        /// <summary>
        /// Returns <see cref="Vector3"/> coordinates of last touch/mouse position. On desktop <paramref name="touch"/> does not have to be specified.
        /// </summary>
        /// <param name="touch">Unity id of touch</param>
        /// <returns><see cref="Vector3"/> coordinates of last touch/mouse position</returns>
		public Vector3 GetPosition(int touch = 0)
		{
			if (AppHelpers.IsTouchDevice())
			{
				if (touch >= Input.touches.Length) return Vector3.zero;
				return Input.touches[touch].position;
			}
			else
			{
				return Input.mousePosition;
			}
		}

        /// <summary>
        /// Returns <see cref="Vector3"/> coordinates of last frame delta touch/mouse position. On desktop <paramref name="touch"/> does not have to be specified.
        /// </summary>
        /// <param name="touch">Unity id of touch</param>
        /// <returns><see cref="Vector3"/> coordinates of last frame delta touch/mouse position</returns>
		public Vector3 GetDeltaPosition(int touch = 0)
		{
			if (AppHelpers.IsTouchDevice())
			{
				if (touch >= Input.touches.Length) return Vector3.zero;
				return Input.touches[touch].deltaPosition;
			}
			else
			{
				if (_mousePositionUpdates == 0) return Input.mousePosition;
				return Input.mousePosition - _lastMousePosition;
			}
		}

		#endregion
	}
}
