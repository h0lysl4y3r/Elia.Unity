using System;
using System.Collections.Generic;
using Elia.Unity.Modules;
using UnityEngine;
using UnityEngine.UI;

namespace Elia.Unity.Components.GUI
{
    /// <summary>
    /// Represents GUI tabs that user can switch among.
    /// </summary>
	public sealed class Tabs : BehaviourAware
	{
		#region Inner Types

        /// <summary>
        /// Single tab state
        /// </summary>
		public enum TabStateEnum
		{
			Active,
			Disabled,
			Selected,
		}

        #endregion

        #region Actions

        /// <summary>
        /// Invoked within <see cref="PopulateTabs"/> method on tabs were populated.
        /// </summary>
        public Action Populated;

        /// <summary>
        /// Invoked within <see cref="SetTabState"/> and <see cref="ToggleSelectedTab"/> methods on single tab (at given index) state was changed.
        /// </summary>
		public Action<int, TabStateEnum> TabStateChanged;

		#endregion

		#region Members

        /// <summary>
        /// True if tabs were populated
        /// </summary>
		public bool IsPopulated { get; private set; }

        /// <summary>
        /// Number of tabs populated
        /// </summary>
		public int TabCount { get { return _tabStates != null ? _tabStates.Count : 0; } }

        /// <summary>
        /// If true, disables buttons having state <see cref="TabStateEnum.Disabled"/>. Default is true.
        /// </summary>
        public bool AutoDisableButtons { get; set; }

        /// <summary>
        /// Parent <see cref="GameObject"/> that is used to populate tabs from.
        /// </summary>
		public GameObject TabsParentGo;

		private List<Button> _tabButtons;
		private List<TabStateEnum> _tabStates;

		#endregion

		#region MonoBehaviour

		protected override void Awake()
		{
			AutoDisableButtons = true;
			_tabButtons = new List<Button>();
			_tabStates = new List<TabStateEnum>();

			base.Awake();
		}

		protected override void Start()
		{
			PopulateTabs();

			base.Start();
		}

        #endregion

        #region Init

        /// <summary>
        /// Populates tabs. To locate tabs, it traverses children of <see cref="TabsParentGo"/> object.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown if <see cref="TabsParentGo"/> is null</exception>
        private void PopulateTabs()
		{
			if (TabsParentGo == null)
				throw new ArgumentNullException(nameof(TabsParentGo));

			foreach (Transform tabTransform in TabsParentGo.transform)
			{
				var tabGo = tabTransform.gameObject;
				var tabButton = tabGo.GetComponent<Button>();

				_tabButtons.Add(tabButton);
				_tabStates.Add(_tabStates.Count == 0 ? TabStateEnum.Selected : TabStateEnum.Active);
			}

			Populated?.Invoke();
			IsPopulated = true;
		}

        #endregion

        #region Queries

        /// <summary>
        /// Returns index of currently selected tab.
        /// </summary>
        /// <returns>Index of currently selected tab</returns>
        public int GetSelectedTabIndex()
		{
			for (var i = 0; i < _tabStates.Count; i++)
			{
				if (_tabStates[i] == TabStateEnum.Selected)
					return i;
			}
			return -1;
		}

        /// <summary>
        /// Returns state of <see cref="TabStateEnum"/> type of tab at <paramref name="index"/>.
        /// </summary>
        /// <param name="index">Tab index</param>
        /// <returns>State of <see cref="TabStateEnum"/> type of tab at <paramref name="index"/></returns>
		public TabStateEnum GetTabState(int index)
		{
			if (index < 0 || index >= TabCount)
				throw new ArgumentOutOfRangeException(nameof(index));

			return _tabStates[index];
		}

        /// <summary>
        /// Returns <see cref="Button"/> component of tab and <paramref name="index"/>.
        /// </summary>
        /// <param name="index">Tab index</param>
        /// <returns><see cref="Button"/> component of tab and <paramref name="index"/></returns>
		public Button GetTabButton(int index)
		{
			if (index < 0 || index >= TabCount)
				throw new ArgumentOutOfRangeException(nameof(index));

			return _tabButtons[index];
		}

        #endregion

        #region Tab Manipulation

        /// <summary>
        /// Sets <paramref name="state"/> of given tab at <paramref name="index"/>. Invokes <see cref="TabStateChanged"/> on state change.
        /// </summary>
        /// <param name="index">Tab index</param>
        /// <param name="state">Tab state</param>
        /// <returns>True if succeeded</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="index"/> is out of range</exception>
        public bool SetTabState(int index, TabStateEnum state)
		{
			if (index < 0 || index >= TabCount)
				throw new ArgumentOutOfRangeException(nameof(index));

			if (_tabStates[index] == state)
			{
				Modules.App.Instance?.Logger.LogWarning(Texts.Tags.Tabs, Texts.Errors.TabStateIsAlreadySet);
				return false;
			}

			_tabStates[index] = state;
			if (AutoDisableButtons) _tabButtons[index].enabled = state != TabStateEnum.Disabled;

			TabStateChanged?.Invoke(index, state);

			return true;
		}

        /// <summary>
        /// Selects tab at <paramref name="index"/>, all other tabs (besides with state <see cref="TabStateEnum.Disabled"/>) are set to state <see cref="TabStateEnum.Active"/>. Invokes <see cref="TabStateChanged"/> on change of tab states.
        /// </summary>
        /// <param name="index">Tab index</param>
        /// <returns>True if succeeded</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="index"/> is out of range</exception>
		public bool ToggleSelectedTab(int index)
		{
			if (index < 0 || index >= TabCount)
				throw new ArgumentOutOfRangeException(nameof(index));

			if (_tabStates[index] == TabStateEnum.Selected
				|| _tabStates[index] == TabStateEnum.Disabled)
			{
				Modules.App.Instance?.Logger.LogWarning(Texts.Tags.Tabs, _tabStates[index] == TabStateEnum.Selected ? Texts.Errors.TabStateIsAlreadySet : Texts.Errors.TabIsDisabled);
				return false;
			}

			for (var i = 0; i < _tabStates.Count; i++)
			{
				if (_tabStates[i] == TabStateEnum.Disabled) continue;
                var previousState = _tabStates[i];
                _tabStates[i] = TabStateEnum.Active;
                if (previousState != _tabStates[i]) TabStateChanged?.Invoke(i, TabStateEnum.Active);
			}

			_tabStates[index] = TabStateEnum.Selected;
			TabStateChanged?.Invoke(index, TabStateEnum.Selected);

			return true;
		}

		#endregion
	}
}
