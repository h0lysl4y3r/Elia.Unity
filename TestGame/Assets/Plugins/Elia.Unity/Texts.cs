namespace Elia.Unity
{
    /// <summary>
    /// Collection of string constants used usually as error messages.
    /// </summary>
	public static class Texts
	{
		public static class Errors
		{
			// Exceptions
			public static string MemberCannotBeNullOrEmpty = "Member cannot be null or empty.";
			public static string TooManyObjectsOfGivenTypeFound = "Too many objects of given type found.";
			public static string TypeSpecifiedMustBeOfDifferentType = "Type specified must be of different type.";
			public static string CollectionDoesNotContainItem = "Collection does not contain item.";
			public static string NoScenesToLoadOrUnloadFound = "No scenes to load/unload found.";
			public static string NoGamesToRunFound = "No games to run found.";
            public static string MissionHasNotEnded = "Mission has not ended.";
            public static string ObjectCannotBeNull = "{0} cannot be null.";
			public static string ObjectAlreadyAssigned = "{0} already assigned.";
			public static string ObjectNotFound = "{0} not found.";
            public static string ObjectIsNotActive = "{0} is not active.";

            // GUI
            public static string ScreenIsAlreadyActive = "Screen is already active.";
			public static string ScreenIsNotActive = "Screen is not active.";
			public static string OverlayIsAlreadyActive = "Overlay is already active.";
			public static string OverlayIsNotActive = "Overlay is not active.";
			public static string ModalIsAlreadyActive = "Modal is already active.";
			public static string ModalIsNotActive = "Modal is not active.";
			public static string TabStateIsAlreadySet = "Tab state is already set.";
			public static string TabIsDisabled = "Tab is disabled.";
            public static string TutorialIsNotActive = "Tutorial is not active.";
            public static string TutorialIsActive = "Tutorial is active.";
            public static string TutorialStepWasAlreadyAssigned = "Tutorial step was already assigned.";
            public static string CommandNotFound = "Command not found.";
            public static string CommandError = "Command error.";
        }

        /// <summary>
        /// Collection of tags usually determining error context.
        /// </summary>
		public static class Tags
		{
			public static string Screens = "Screens";
			public static string Overlays = "Overlays";
			public static string Modals = "Modals";
			public static string Tabs = "Tabs";
			public static string Audio = "Audio";
		}
	}
}
