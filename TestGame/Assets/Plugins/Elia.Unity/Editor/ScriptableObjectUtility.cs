using System;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace Elia.Unity.Editor
{
    /// <summary>
    /// <see cref="ScriptableObject"/> utility class.
    /// </summary>
	public static class ScriptableObjectUtility
	{
        /// <summary>
        /// Creates editor asset of <typeparamref name="T"/> type.
        /// </summary>
        /// <typeparam name="T">Asset type param</typeparam>
		public static void CreateAsset<T>() where T : ScriptableObject
		{
			CreateAsset(typeof(T));
		}

        /// <summary>
        /// Creates and saves (as .asset file) editor asset of <paramref name="type"/>.
        /// </summary>
        /// <param name="type">Asset type</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="type"/> is not subclass of <see cref="ScriptableObject"/></exception>
		public static void CreateAsset(Type type)
		{
			if (!type.IsSubclassOf(typeof(ScriptableObject)))
				throw new ArgumentException(nameof(type));

			var asset = ScriptableObject.CreateInstance(type);

			var path = AssetDatabase.GetAssetPath(Selection.activeObject);
			if (path == "")
			{
				path = "Assets";
			}
			else if (Path.GetExtension(path) != "")
			{
				path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
			}

			var assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + "/New " + type.ToString() + ".asset");

			AssetDatabase.CreateAsset(asset, assetPathAndName);

			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
			EditorUtility.FocusProjectWindow();
			Selection.activeObject = asset;
		}
	}
}
