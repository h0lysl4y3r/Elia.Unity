using System.Text;
using UnityEngine;
using Elia.Unity.Components.GUI;
using UnityEngine.UI;

namespace Elia.Unity.Components.App
{
    /// <summary>
    /// GUI modal for Unity exceptions.
    /// </summary>
	public class UnityExceptionsModal : Modal
	{
        /// <summary>
        /// Exception meta data
        /// </summary>
		public class ExceptionData
		{
			public string Condition { get; set; }
			public string StackTrace { get; set; }
			public LogType Type { get; set; }
		}

        /// <summary>
        /// True to show exceptions
        /// </summary>
		public bool HandleExceptions;

        /// <summary>
        /// True to show errors
        /// </summary>
		public bool HandleErrors;

		protected override void OnEnable()
		{
			base.OnEnable();

			Application.logMessageReceived += HandleError;
		}

		protected override void OnDisable()
		{
			base.OnDisable();

			Application.logMessageReceived -= HandleError;
		}

		protected override void OnActivated()
		{
			base.OnActivated();

			if (Data != null)
			{
				var text = GetErrorText();
				if (text != null)
				{
					var data = Data as ExceptionData;
					var message = CreateErrorMessage(data.Condition, data.StackTrace, data.Type);
					text.text = message;
				}

				var button = GetErrorButton();
				if (button != null)
				{
					button.onClick.RemoveAllListeners();
					button.onClick.AddListener(OnErrorButtonClicked);
				}
			}
		}

        /// <summary>
        /// Activates modal.
        /// </summary>
        /// <param name="condition">Error metadata - condition</param>
        /// <param name="stackTrace">Error metadata - stackTrace</param>
        /// <param name="type">Error metadata - type</param>
		private void HandleError(string condition, string stackTrace, LogType type)
		{
			if (Modules.App.Instance.DebugLevel <= 0) return; // don't show at given debug level

			if (HandleExceptions && type == LogType.Exception
				|| HandleErrors && type == LogType.Error)
			{
				var data = new ExceptionData()
				{
					Condition = condition,
					StackTrace = stackTrace,
					Type = type
				};
				Modules.Modals.Instance.SetModalActive<UnityExceptionsModal>(true, data);
			}
		}

        /// <summary>
        /// Creates error message from meta data.
        /// </summary>
        /// <param name="condition"><Error metadata - condition/param>
        /// <param name="stackTrace">Error metadata - stackTrace</param>
        /// <param name="type">Error metadata - type</param>
        /// <returns></returns>
		private string CreateErrorMessage(string condition, string stackTrace, LogType type)
		{
			var sb = new StringBuilder();
			sb.AppendLine(string.Concat(type.ToString(), ":", condition))
				.AppendLine(stackTrace);
			return sb.ToString();
		}

        /// <summary>
        /// Returns modal's <see cref="Text"/> component.
        /// </summary>
        /// <returns></returns>
		private Text GetErrorText()
		{
			return gameObject.GetComponentInChildren<Text>();
		}

        /// <summary>
        /// Returns modal's <see cref="Button"/> component.
        /// </summary>
        /// <returns></returns>
		private Button GetErrorButton()
		{
			return gameObject.GetComponentInChildren<Button>();
		}

        /// <summary>
        /// Callback that deactivates the modal.
        /// </summary>
		private void OnErrorButtonClicked()
		{
			Modules.Modals.Instance.SetModalActive<UnityExceptionsModal>(false);
		}
	}
}
