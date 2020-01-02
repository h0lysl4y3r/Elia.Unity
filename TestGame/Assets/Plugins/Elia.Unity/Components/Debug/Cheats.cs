using System;
using System.Collections.Generic;
using System.Text;
using Elia.Unity.DotNet;
using Elia.Unity.Modules;

namespace Elia.Unity.Components.Debug
{
	/// <summary>
	/// Cheat management.
	/// </summary>
	public class Cheats
	{
		/// <summary>
		/// Max time delay between inputs in input sequence
		/// </summary>
		public static float MaxInputTimeDelayInSequence = 0.5f;

		/// <summary>
		/// True to use cheats also in production build
		/// </summary>
		public bool UseCheatsInProduction;

		private App.AppMeta _appMeta;
		private App.Timer _timer;
		private List<DotNet.Tuple<string, string, Action>> _cheatTuples;
		private string _currentInputSequence;
		private List<string> _cheatsInvoked;
		private double _lastInputTime;

		public Cheats(App.AppMeta appMeta, App.Timer timer)
		{
			if (appMeta == null) throw new ArgumentNullException(nameof(appMeta));
			if (timer == null) throw new ArgumentNullException(nameof(timer));
			_appMeta = appMeta;
			_timer = timer;

			_cheatTuples = new List<DotNet.Tuple<string, string, Action>>();
			_cheatsInvoked = new List<string>();

			Inputs.Instance.AutoPollPressed = true;
			Inputs.Instance.Pressed += OnInputPressed;
		}

		/// <summary>
		/// Invoke cheat <see cref="Action"/> instance if input sequence is pressed
		/// </summary>
		private void UpdateCheats()
		{
			for (var i = 0; i < _cheatTuples.Count; i++)
			{
				var tuple = _cheatTuples[i];
				if (_cheatsInvoked.Contains(tuple.Item2)) continue; // already invoked
				if (tuple.Item1 != _currentInputSequence) continue;

				_cheatsInvoked.Add(tuple.Item2);
				tuple.Item3.Invoke();
			}
		}

		/// <summary>
		/// Handles input button pressed.
		/// </summary>
		/// <param name="button">Unity id of button pressed</param>
		private void OnInputPressed(int button)
		{
			if (_lastInputTime > 0 && _timer.RealTimeFloat - _lastInputTime > MaxInputTimeDelayInSequence)
			{
				_currentInputSequence = null;
				_cheatsInvoked.Clear();
			}

			_lastInputTime = _timer.RealTimeFloat;
			if (_currentInputSequence == null)
				_currentInputSequence = button.ToString();
			else
				_currentInputSequence += button.ToString();

			if (_appMeta != null && (UseCheatsInProduction || !_appMeta.IsProduction))
				UpdateCheats();
		}

		/// <summary>
		/// Adds cheat to <see cref="Cheats"/>
		/// </summary>
		/// <param name="inputSequence">Sequence of inputs to invoke cheat <see cref="Action"/></param>
		/// <param name="cheatName">Cheat name</param>
		/// <param name="action">Instance of <see cref="Action"/> to invoke</param>
		/// <exception cref="Exception">Thrown if no instance of <see cref="App.AppMeta"/> was provided</exception>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="action"/> or <paramref name="cheatName"/> or <paramref name="inputSequence"/> is null</exception>
		public void AddCheat(int[] inputSequence, string cheatName, Action action)
		{
			if (_appMeta == null) throw new Exception(string.Format(Texts.Errors.ObjectNotFound, nameof(_appMeta)));
			if (inputSequence == null) throw new ArgumentNullException(nameof(inputSequence));
			if (cheatName == null) throw new ArgumentNullException(nameof(cheatName));
			if (action == null) throw new ArgumentNullException(nameof(action));

			// convert sequence to string for easier compare
			StringBuilder inputString = new StringBuilder();
			for (var i = 0; i < inputSequence.Length; i++)
				inputString.Append(inputSequence[i]);

			_cheatTuples.Add(new DotNet.Tuple<string, string, Action>(inputString.ToString(), cheatName, action));
		}

		/// <summary>
		/// Removes cheat from <see cref="Cheats"/>
		/// </summary>
		/// <param name="cheatName">Cheat name</param>
		/// <exception cref="Exception">Thrown if no instance of <see cref="App.AppMeta"/> was provided</exception>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="cheatName"/> is null</exception>
		public bool RemoveCheat(string cheatName)
		{
			if (_appMeta == null) throw new Exception(string.Format(Texts.Errors.ObjectNotFound, nameof(_appMeta)));
			if (cheatName == null) throw new ArgumentNullException(nameof(cheatName));

			return _cheatTuples.RemoveAll(x => x.Item2 == cheatName) > 0;
		}
	}
}
