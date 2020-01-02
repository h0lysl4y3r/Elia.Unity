using UnityEngine;

namespace Elia.Unity.Components.GameObjects
{
    /// <summary>
    /// Simle behaviour that prevents <see cref="GameObject"/> to be destroyed upon other scene loaded.
    /// </summary>
	[AddComponentMenu("ELIA/Components/DontDestroy")]
	public sealed class DontDestroy : MonoBehaviour
	{
		#region MonoBehaviour

		private void Awake()
		{
			DontDestroyOnLoad(transform.gameObject);
		}

		#endregion
	}
}