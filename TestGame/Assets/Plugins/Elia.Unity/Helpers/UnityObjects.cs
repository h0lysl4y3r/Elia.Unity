using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Elia.Unity.Helpers
{
    /// <summary>
    /// Unity objects helper methods.
    /// </summary>
	public static class UnityObjects
	{
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
		public static List<T> FindObjectsOfTypeAll<T>()
		{
			List<T> results = new List<T>();
			for (int i = 0; i < SceneManager.sceneCount; i++)
			{
				var s = SceneManager.GetSceneAt(i);
				if (s.isLoaded)
				{
					var allGameObjects = s.GetRootGameObjects();
					for (int j = 0; j < allGameObjects.Length; j++)
					{
						var go = allGameObjects[j];
						results.AddRange(go.GetComponentsInChildren<T>(true));
					}
				}
			}
			return results;
		}

		public static bool IsCanvasGroupEnabled(this GameObject go)
		{
			var canvasGroup = go.GetComponent<CanvasGroup>();
			return canvasGroup != null && canvasGroup.alpha >= 1f;
		}

		public static void EnableCanvasGroup(this GameObject go, bool state, float setAlpha = -1f)
		{
			var canvasGroup = go.GetComponent<CanvasGroup>();
			if (canvasGroup != null)
				EnableCanvasGroup(canvasGroup, state, true, setAlpha);
		}

		public static void SetCanvasGroupAlpha(this GameObject go, bool state, float setAlpha = -1f)
		{
			var canvasGroup = go.GetComponent<CanvasGroup>();
			if (canvasGroup != null)
				SetCanvasGroupAlpha(canvasGroup, state, setAlpha);
		}

		public static void SetCanvasGroupAlpha(this CanvasGroup canvasGroup, bool state, float setAlpha = -1f)
		{
			var alpha = (setAlpha != -1f) ? setAlpha
				: (state) ? 1f : 0f;
			canvasGroup.alpha = alpha;
		}

		public static void EnableCanvasGroup(this CanvasGroup canvasGroup, bool state, bool alsoGraphicRaycaster, float setAlpha = -1f)
		{
			var alpha = (setAlpha != -1f) ? setAlpha
				: (state) ? 1f : 0f;
			canvasGroup.alpha = alpha;
			canvasGroup.interactable = state;
			canvasGroup.blocksRaycasts = state;
			if (alsoGraphicRaycaster)
			{
				var graphicRaycaster = canvasGroup.gameObject.GetComponent<GraphicRaycaster>();
				if (graphicRaycaster != null)
					graphicRaycaster.enabled = state;
			}
		}

		public static void EnableCanvasGroupAndRaycaster(this CanvasGroup canvasGroup, bool state, GraphicRaycaster graphicRaycaster, float setAlpha = -1f)
		{
			var alpha = (setAlpha != -1f) ? setAlpha
				: (state) ? 1f : 0f;
			canvasGroup.alpha = alpha;
			canvasGroup.interactable = state;
			canvasGroup.blocksRaycasts = state;
			if (graphicRaycaster != null) graphicRaycaster.enabled = state;
		}
	}
}
