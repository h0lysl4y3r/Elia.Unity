using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Elia.Unity.Components.Games
{
    /// <summary>
    /// Manages single step within <see cref="Tutorial"/>.
    /// </summary>
    public abstract class TutorialStep
    {
        /// <summary>
        /// Invoked on <see cref="TutorialStep"/> finishes.
        /// </summary>
        public Action Finished;

        /// <summary>
        /// Instance of parent <see cref="Tutorial"/>
        /// </summary>
        public Tutorial Tutorial { get; private set; }

        public TutorialStep(Tutorial tutorial)
        {
            Tutorial = tutorial;
        }

        /// <summary>
        /// Callback invoked when <see cref="TutorialStep"/> is started in <see cref="Tutorial"/>.
        /// </summary>
        public virtual void OnStarted()
        {
        }

        /// <summary>
        /// Callback invoked when <see cref="TutorialStep"/> is finished in <see cref="Tutorial"/>.
        /// </summary>
        public virtual void OnFinished()
        {
        }
    }
}
