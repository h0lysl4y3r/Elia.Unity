using System;
using System.Collections.Generic;
using Elia.Unity.Components.GameObjects;
using Elia.Unity.Extensions;
using UnityEngine;

namespace Elia.Unity.Modules
{
    /// <summary>
    /// Module that provides resources/assets management.
    /// </summary>
	[AddComponentMenu("ELIA/Modules/Assets")]
	public sealed class Assets : BehaviourAwareSingleton<Assets>
	{
		#region Members

		private Dictionary<string, Sprite[]> _spriteMap;
        private Dictionary<string, UnityEngine.Object[]> _objectMap;

        #endregion

        #region Singleton

        public override void OnSingletonInstantiating()
		{
			if (IsInstantiated) return;
			Initialize();
		}

		private void Initialize()
		{
			if (_spriteMap == null) _spriteMap = new Dictionary<string, Sprite[]>(StringComparer.Ordinal);
            if (_objectMap == null) _objectMap = new Dictionary<string, UnityEngine.Object[]>(StringComparer.Ordinal);
        }

		#endregion

		#region MonoBehaviour

		protected override void Awake()
		{
			Initialize();

			base.Awake();
		}

        #endregion

        #region Objects

        /// <summary>
        /// Returns single loaded <see cref="UnityEngine.Object"/> instance.
        /// </summary>
        /// <param name="key">String key to <see cref="UnityEngine.Object"/> instance</param>
        /// <returns>Instances of <see cref="UnityEngine.Object"/> type</returns>
        public UnityEngine.Object GetObject(string key)
        {
            UnityEngine.Object[] objects;
            _objectMap.TryGetValue(key, out objects);
            if (objects == null || objects.Length != 1) return null;
            return objects[0];
        }

        /// <summary>
        /// Loades single <see cref="UnityEngine.Object"/> resource and stores them under <paramref name="key"/>.
        /// </summary>
        /// <param name="key">Map key to stored instance</param>
        /// <param name="path">Path to resource in Unity asset folder</param>
        /// <returns></returns>
        public bool LoadObject(string key, string path)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (path == null) throw new ArgumentNullException(nameof(path));

            if (_objectMap.ContainsKey(key)) return false;

            var obj = Resources.Load<UnityEngine.Object>(path);
            if (obj == null) return false;

            _objectMap[key] = new[] { obj };

            return true;
        }

        /// <summary>
        /// Returns array of loaded <see cref="Sprite"/> instances.
        /// </summary>
        /// <param name="key">String key to <see cref="Sprite"/> array</param>
        /// <returns>Array of loaded <see cref="Sprite"/> instances</returns>
        public UnityEngine.Object[] GetObjects(string key)
        {
            if (!_objectMap.ContainsKey(key)) return null;
            return _objectMap[key];
        }

        /// <summary>
        /// Loades <see cref="UnityEngine.Object"/> resources and stores them under <paramref name="key"/>.
        /// </summary>
        /// <param name="key">Map key to stored instances</param>
        /// <param name="path">Path to resources in Unity asset folder</param>
        /// <returns></returns>
        public bool LoadObjects(string key, string path)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (path == null) throw new ArgumentNullException(nameof(path));

            if (_objectMap.ContainsKey(key)) return false;

            var objects = Resources.LoadAll<UnityEngine.Object>(path);
            if (objects == null) return false;

            _objectMap[key] = objects;

            return true;
        }

        /// <summary>
        /// UnLoades stored <see cref="UnityEngine.Object"/> instances.
        /// </summary>
        /// <param name="key">Map key to stored instances</param>
        /// <returns></returns>
        public bool UnloadObjects(string key)
		{
			if (key == null) throw new ArgumentNullException(nameof(key));

			if (!_spriteMap.ContainsKey(key)) return false;

            var sprites = _spriteMap[key];
			_spriteMap.Remove(key);

            foreach (var sprite in sprites)
            {
                Resources.UnloadAsset(sprite);
            }

            return true;
		}

        #endregion

        #region Sprites

        /// <summary>
        /// Returns array of loaded <see cref="Sprite"/> instances.
        /// </summary>
        /// <param name="key">String key to <see cref="Sprite"/> array</param>
        /// <returns>Array of loaded <see cref="Sprite"/> instances</returns>
        public Sprite[] GetSprites(string key)
        {
            Sprite[] sprites;
            _spriteMap.TryGetValue(key, out sprites);
            return sprites;
        }

        /// <summary>
        /// Loades <see cref="Sprite"/> resources and stores them under <paramref name="key"/>.
        /// </summary>
        /// <param name="key">Map key to stored instances</param>
        /// <param name="path">Path to resources in Unity asset folder</param>
        /// <returns></returns>
		public bool LoadSprites(string key, string path)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (path == null) throw new ArgumentNullException(nameof(path));

            if (_spriteMap.ContainsKey(key)) return false;

            var sprites = Resources.LoadAll<Sprite>(path);
            if (sprites.IsNullOrEmpty()) return false;

            _spriteMap[key] = sprites;

            return true;
        }

        /// <summary>
        /// UnLoades stored <see cref="Sprite"/> instances.
        /// </summary>
        /// <param name="key">Map key to stored instances</param>
        /// <returns></returns>
		public bool UnloadSprites(string key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));

            if (!_spriteMap.ContainsKey(key)) return false;

            var sprites = _spriteMap[key];
            _spriteMap.Remove(key);

            foreach (var sprite in sprites)
            {
                Resources.UnloadAsset(sprite);
            }

            return true;
        }

        /// <summary>
        /// Returns single loaded <see cref="Sprite"/> instance.
        /// </summary>
        /// <param name="key">String key to <see cref="Sprite"/> instance</param>
        /// <returns>Instances of <see cref="Sprite"/> type</returns>
        public Sprite GetSprite(string key)
        {
            Sprite[] sprites;
            _spriteMap.TryGetValue(key, out sprites);
            if (sprites == null || sprites.Length != 1) return null;
            return sprites[0];
        }

        /// <summary>
        /// Loades single <see cref="Sprite"/> resource and stores them under <paramref name="key"/>.
        /// </summary>
        /// <param name="key">Map key to stored instance</param>
        /// <param name="path">Path to resource in Unity asset folder</param>
        /// <returns></returns>
        public bool LoadSprite(string key, string path)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (path == null) throw new ArgumentNullException(nameof(path));

            if (_spriteMap.ContainsKey(key)) return false;

            var sprite = Resources.Load<Sprite>(path);
            if (sprite == null) return false;

            _spriteMap[key] = new[] { sprite };

            return true;
        }

        #endregion
    }
}
