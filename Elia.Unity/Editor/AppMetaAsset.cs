using Elia.Unity.Components.App;
using UnityEditor;

namespace Elia.Unity.Editor
{
    /// <summary>
    /// Editor asset to creae <see cref="AppMeta"/> object.
    /// </summary>
	public sealed class AppMetaAsset
	{
		[MenuItem("Assets/Create/ELIA/AppMetaAsset")]
		public static void CreateAsset()
		{
			ScriptableObjectUtility.CreateAsset<AppMeta>();
		}
	}
}
