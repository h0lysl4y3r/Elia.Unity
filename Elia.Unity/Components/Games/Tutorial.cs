using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Elia.Unity.Components.Games
{
    /// <summary>
    /// Manages <see cref="Modules.Game"/> tutorial related state management.
    /// </summary>
    public class Tutorial
    {
        #region Actions

        /// <summary>
        /// Invoked within <see cref="End"/>. Set to true if <see cref="CurrentStepIndex"/> points to last instance of <see cref="TutorialStep"/>.
        /// </summary>
        public Action<bool> Ended;

        #endregion

        /// <summary>
        /// Instance of parent <see cref="Modules.Game"/>
        /// </summary>
        public Modules.Game Game { get; private set; }

        /// <summary>
        /// True if <see cref="Tutorial"/> is active
        /// </summary>
        public bool IsActive { get; private set; }

        /// <summary>
        /// Current step, instance of <see cref="TutorialStep"/>
        /// </summary>
        public TutorialStep CurrentStep { get; private set; }

		/// <summary>
		/// Index of current step
		/// </summary>
		public int CurrentStepIndex { get; private set; }

		/// <summary>
		/// Collection of <see cref="Tutorial"/> step instances
		/// </summary>
		public ReadOnlyCollection<TutorialStep> Steps { get; private set; }

        public Tutorial(Modules.Game game)
        {
            Game = game;
        }

        /// <summary>
        /// Sets instance collection of <see cref="TutorialStep"/> type to run.
        /// </summary>
        /// <param name="steps">Instance collection of <see cref="TutorialStep"/> type</param>
        /// <exception cref="Exception">Thrown if <see cref="Tutorial.IsActive"/> is true</exception>
        /// <exception cref="NullReferenceException">Thrown if <paramref name="steps"/> is null</exception>
        public void SetSteps(IEnumerable<TutorialStep> steps)
        {
            if (steps == null) throw new NullReferenceException(nameof(steps));
            if (IsActive) throw new Exception(Texts.Errors.TutorialIsActive);

            CurrentStepIndex = -1;
            CurrentStep = null;

            var list = new List<TutorialStep>();
            list.AddRange(steps);
            Steps = list.AsReadOnly();
        }

        /// <summary>
        /// Runs either step defined by <paramref name="specificStep"/> or next <see cref="TutorialStep"/> given by <see cref="CurrentStepIndex"/>.
		/// If <paramref name="specificStep"/> is specified, does not run <see cref="TutorialStep.OnFinished"/> on <see cref="CurrentStep"/>.
        /// </summary>
        /// <param name="specificStep">Instance of <see cref="TutorialStep"/> (optional)</param>
        /// <exception cref="Exception">Thrown if <see cref="IsActive"/> is false, or if <see cref="CurrentStep"/> is equal to <paramref name="specificStep"/> or if <paramref name="specificStep"/> is not within <see cref="Steps"/> collection or if index of <paramref name="specificStep"/> within <see cref="Steps"/> collection is lower than <see cref="CurrentStepIndex"/></exception>
        public void Run(TutorialStep specificStep = null)
        {
            if (!IsActive) throw new Exception(Texts.Errors.TutorialIsNotActive);
            if (specificStep != null && CurrentStep == specificStep)
                throw new Exception(string.Format(Texts.Errors.ObjectAlreadyAssigned, nameof(specificStep)));

			// don't execute if run from specific step is called
			if (specificStep == null) CurrentStep?.OnFinished();

            int stepIndex = -1;
            if (specificStep != null)
            {
                stepIndex = GetTutorialStepIndex(specificStep);
                if (stepIndex < CurrentStepIndex) throw new Exception(Texts.Errors.TutorialStepWasAlreadyAssigned);
				CurrentStepIndex = stepIndex;
				CurrentStep = specificStep;
			}
			else
            {
				AdvanceStep();
            }

            CurrentStep.OnStarted();
        }

		/// <summary>
		/// Updates <see cref="CurrentStep"/> and <see cref="CurrentStepIndex"/> to next step in <see cref="Steps"/> collection.
		/// </summary>
		private void AdvanceStep()
		{
			if (CurrentStepIndex + 1 >= Steps.Count)
			{
				End();
				return;
			}

			CurrentStepIndex = CurrentStepIndex + 1;
			CurrentStep = Steps[CurrentStepIndex];
		}

		/// <summary>
		/// Finishes <see cref="CurrentStep"/>. Advances to next step, if possible.
		/// </summary>
		/// <exception cref="Exception">Thrown if <see cref="IsActive"/> is false, or if <see cref="CurrentStep"/> is equal to <paramref name="specificStep"/> or if <paramref name="specificStep"/> is not within <see cref="Steps"/> collection or if index of <paramref name="specificStep"/> within <see cref="Steps"/> collection is lower than <see cref="CurrentStepIndex"/></exception>
		/// <exception cref="ArgumentNullException">Thrown if <see cref="CurrentStep"/> is null</exception>
		public void FinishCurrentStep()
		{
			if (!IsActive) throw new Exception(Texts.Errors.TutorialIsNotActive);
			if (CurrentStep == null) throw new ArgumentNullException(nameof(CurrentStep));

			CurrentStep.OnFinished();
			//AdvanceStep();
		}

		/// <summary>
		/// Begins <see cref="Tutorial"/>, Sets <see cref="IsActive"/> to true.
		/// </summary>
		/// <returns>True if succeeded</returns>
		public bool Begin()
        {
            if (IsActive) return false;

            if (Steps == null || Steps.Count == 0) throw new Exception(string.Format(Texts.Errors.ObjectCannotBeNull, nameof(Steps)));

            IsActive = true;

            return true;
        }

        /// <summary>
        /// Ends <see cref="Tutorial"/>. Invokes <see cref="Ended"/> action. Sets <see cref="IsActive"/> to false.
        /// </summary>
        /// <returns>True if succeeded</returns>
        public bool End()
        {
            if (!IsActive) return false;

            IsActive = false;
            var isLastStep = CurrentStepIndex + 1 == Steps.Count;
            Ended?.Invoke(isLastStep);

			CurrentStepIndex = -1;
			CurrentStep = null;

			return true;
        }

        /// <summary>
        /// Returns index of <paramref name="step"/> within <see cref="Steps"/> collection.
        /// </summary>
        /// <param name="step">Instance of <see cref="TutorialStep"/></param>
        /// <returns>Index of <paramref name="step"/> within <see cref="Steps"/> collection</returns>
        private int GetTutorialStepIndex(TutorialStep step)
        {
            if (Steps == null || Steps.Count == 0) return -1;
            for (var i = 0; i < Steps.Count; i++)
                if (Steps[i] == step) return i;
            return -1;
        }
    }
}
