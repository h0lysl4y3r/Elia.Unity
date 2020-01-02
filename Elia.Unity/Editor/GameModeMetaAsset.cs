using Elia.Unity.Components.Games;
using UnityEditor;

namespace Elia.Unity.Editor
{
    /// <summary>
    /// Editor asset to creae <see cref="GameModeMeta"/> object.
    /// </summary>
	public sealed class GameModeMetaAsset
	{
		[MenuItem("Assets/Create/ELIA/GameModeMetaAsset")]
		public static void CreateAsset()
		{
			ScriptableObjectUtility.CreateAsset<GameModeMeta>();
		}
	}
}
