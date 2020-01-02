using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Elia.Unity.Components.Debug
{
    /// <summary>
    /// Manages GUI debug console with commands invoking functions.
    /// </summary>
	public class DebugConsole : BehaviourAware
	{
        /// <summary>
        /// Root <see cref="GameObject"/> containing console GUI
        /// </summary>
        public DebugConsoleRoot Root
        {
            get { return _root; }
            set
            {
                var prevRoot = _root;
                _root = value;
                if (_root != prevRoot) OnRootChanged();
            }
        }
        private DebugConsoleRoot _root;

        /// <summary>
        /// Instance of <see cref="KeyCode"/> to toggle console GUI
        /// </summary>
        public KeyCode? ToggleConsoleKey { get; set; }

		/// <summary>
		/// True to focus input again on command processed
		/// </summary>
		public bool FocusInputOnCommandProcessed { get; set; }

		private List<DotNet.Tuple<string, Func<string>>> _commandTuples;
        private bool _toggleConsole;

        protected override void Awake()
        {
            base.Awake();

            _commandTuples = new List<DotNet.Tuple<string, Func<string>>>();
        }

        /// <summary>
        /// Callback invoked on <see cref="Root"/> changed.
        /// </summary>
        private void OnRootChanged()
        {
            if (Root == null) return;
            Root.LogInput.onEndEdit.AddListener((string text) =>
            {
                ProcessInputText(text);
            });

            Root.SendButton.onClick.AddListener(() =>
            {
                ProcessInputText(Root.LogInput.text);
            });
        }

        protected override void Update()
        {
            base.Update();

            if (ToggleConsoleKey.HasValue && Input.GetKeyDown(ToggleConsoleKey.Value))
            {
                ToggleConsole();
            }
        }

        /// <summary>
        /// Toggles console GUI
        /// </summary>
        /// <returns>True if succeeded</returns>
        public bool ToggleConsole()
        {
            if (Root == null) return false;
            _toggleConsole = !_toggleConsole;
            Root.gameObject.SetActive(_toggleConsole);
            return true;
        }

        /// <summary>
        /// Adds command to internal list that is compared to input.
        /// </summary>
        /// <param name="commandName">Command name</param>
        /// <param name="func">Function that is invoked upon command written and submitted. Result is written to console text</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="commandName"/> or <paramref name="func"/> is null</exception>
        public void AddCommand(string commandName, Func<string> func)
        {
            if (commandName == null) throw new ArgumentNullException(nameof(commandName));
            if (func == null) throw new ArgumentNullException(nameof(func));

            _commandTuples.Add(new DotNet.Tuple<string, Func<string>>(commandName, func));
        }

        /// <summary>
        /// Processes text and refocuses input.
        /// </summary>
        /// <param name="text">Input text</param>
        private void ProcessInputText(string text)
        {
            if (text != null && text.Length > 0)
            {
                Root.LogText.text += text + "\n";
                ProcessCommand(text);
            }
            Root.LogInput.text = "";

			if (FocusInputOnCommandProcessed)
			{
				if (!EventSystem.current.alreadySelecting)
				{
					EventSystem.current.SetSelectedGameObject(Root.LogInput.gameObject, null);
				}
				Root.LogInput.OnPointerClick(new PointerEventData(EventSystem.current));
			}
		}

        /// <summary>
        /// Processes input text as command.
        /// </summary>
        /// <param name="text">Input text command</param>
        private void ProcessCommand(string text)
        {
            var command = _commandTuples.FirstOrDefault(x => x.Item1 == text);
            if (command == null)
            {
                Root.LogText.text += Texts.Errors.CommandNotFound + "\n";
            } else
            {
                var result = command.Item2.Invoke();
                if (result != null)
                    Root.LogText.text += result + "\n";
                else
                    Root.LogText.text += Texts.Errors.CommandError + "\n";
            }
        }
    }
}

