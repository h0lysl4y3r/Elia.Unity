using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Elia.Unity.Components.Debug
{
    public class DebugConsoleRoot : MonoBehaviour
    {
        /// <summary>
        /// Instance of <see cref="Text"/> where text is written
        /// </summary>
        public Text LogText;

        /// <summary>
        /// Instance of <see cref="InputField"/> where commands written
        /// </summary>
        public InputField LogInput;

        /// <summary>
        /// Alternative method via <see cref="Button.onClick"/> action of command confirmation to <see cref="InputField"/> submit
        /// </summary>
        public Button SendButton;
    }
}
