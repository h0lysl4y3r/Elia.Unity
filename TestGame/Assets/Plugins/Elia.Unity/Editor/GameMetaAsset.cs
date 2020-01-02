using Elia.Unity.Components.Games;
using UnityEditor;

namespace Elia.Unity.Editor
{
    /// <summary>
    /// Editor asset to creae <see cref="GameMeta"/> object.
    /// </summary>
	public sealed class GameMetaAsset
	{
		[MenuItem("Assets/Create/ELIA/GameMetaAsset")]
		public static void CreateAsset()
		{
			ScriptableObjectUtility.CreateAsset<GameMeta>();
		}
	}
}
