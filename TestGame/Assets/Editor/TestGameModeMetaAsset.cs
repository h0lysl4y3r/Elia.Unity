using Elia.Unity.Components.Games;
using Elia.Unity.Editor;
using UnityEditor;

public sealed class TestGameModeMetaAsset
{
	[MenuItem("Assets/Create/ELIA/TestGameModeMetaAsset")]
	public static void CreateAsset()
	{
		ScriptableObjectUtility.CreateAsset<TestGameModeMeta>();
	}
}
