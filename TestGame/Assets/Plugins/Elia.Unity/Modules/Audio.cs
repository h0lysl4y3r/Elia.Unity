using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using System.Linq;
using Elia.Unity.Serialization;
using Elia.Unity.Components.GameObjects;

namespace Elia.Unity.Modules
{
    /// <summary>
    /// Audio related module that manages clips, audio sources and audio mixers.
    /// </summary>
	[AddComponentMenu("ELIA/Modules/Audio")]
	public sealed class Audio : BehaviourAwareSingleton<Audio>
	{
		#region Inner Types

        /// <summary>
        /// Type of audio to play
        /// </summary>
		public enum AudioType
		{
			Effect,
			LoopEffect,
			Music
		}

		#endregion

		#region Actions

		/// <summary>
		/// Action invoked on any clip started playing.
		/// </summary>
		public Action<AudioType> ClipStarted;

		/// <summary>
		/// Action invoked on any clip stopped playing.
		/// </summary>
		public Action<AudioType> ClipStopped;

		#endregion

		#region Members

		/// <summary>
		/// Attenuation value in dB of muted signal
		/// </summary>
		public const float MutedAttenuation = -80f;

		/// <summary>
		/// Map of effect clip keys to <see cref="AudioClip"/> instances.
		/// </summary>
        [SerializeField]
        public StringAudioClipDictionary EffectClipLibrary = StringAudioClipDictionary.New<StringAudioClipDictionary>();
        private List<string> _effectClips;

        /// <summary>
        /// Map of loop effect clip keys to <see cref="AudioClip"/> instances.
        /// </summary>
        [SerializeField]
        public StringAudioClipDictionary LoopEffectClipLibrary = StringAudioClipDictionary.New<StringAudioClipDictionary>();
        private List<string> _loopEffectClips;

        /// <summary>
        /// Map of music clip keys to <see cref="AudioClip"/> instances.
        /// </summary>
        [SerializeField]
        public StringAudioClipDictionary MusicClipLibrary = StringAudioClipDictionary.New<StringAudioClipDictionary>();
        private List<string> _musicClips;

        /// <summary>
        /// Map of clip keys to clip groups
        /// </summary>
        [SerializeField]
        public StringStringDictionary ClipGroups = StringStringDictionary.New<StringStringDictionary>();
        private Dictionary<string, List<string>> _groupClips;

		public bool PauseEffects;
		public bool PauseLoopEffects;
		public bool PauseMusic;

		/// <summary>
		/// Manages volume of music related audio
		/// </summary>
		public float MusicVolume
		{
			get
			{
				if (_musicAudioMixerGroup != null)
				{
					float volume;
					if (_musicAudioMixerGroup.Item1.audioMixer.GetFloat(_musicAudioMixerGroup.Item2, out volume))
						return volume;
					return MutedAttenuation;
				}
				else
				{
					return _musicAudioSourcePool[0].volume;
				}
			}
			set
			{
				if (_musicAudioMixerGroup != null)
				{
					_musicAudioMixerGroup.Item1.audioMixer.SetFloat(_musicAudioMixerGroup.Item2, value);
				}
				else
				{
					var count = _musicAudioSourcePool.Count;
					for (var i = 0; i < count; i++)
					{
						_musicAudioSourcePool[i].volume = value;
					}
				}
			}
		}

		/// <summary>
		/// Manages volume of effects related audio
		/// </summary>
		public float EffectsVolume
		{
			get
			{
				if (_effectsAudioMixerGroup != null)
				{
					float volume;
					if (_effectsAudioMixerGroup.Item1.audioMixer.GetFloat(_effectsAudioMixerGroup.Item2, out volume))
						return volume;
					return MutedAttenuation;
				}
				else
				{
					return _effectsAudioSourcePool[0].volume;
				}
			}
			set
			{
				if (_effectsAudioMixerGroup != null)
				{
					_effectsAudioMixerGroup.Item1.audioMixer.SetFloat(_effectsAudioMixerGroup.Item2, value);
				}
				else
				{
					var count = _effectsAudioSourcePool.Count;
					for (var i = 0; i < count; i++)
					{
						_effectsAudioSourcePool[i].volume = value;
					}
				}
			}
		}

		/// <summary>
		/// Manages volume of loop effects related audio
		/// </summary>
		public float LoopEffectsVolume
		{
			get
			{
				if (_loopEffectsAudioMixerGroup != null)
				{
					float volume;
					if (_loopEffectsAudioMixerGroup.Item1.audioMixer.GetFloat(_loopEffectsAudioMixerGroup.Item2, out volume))
						return volume;
					return MutedAttenuation;
				}
				else
				{
					return _loopEffectsAudioSourcePool[0].volume;
				}
			}
			set
			{
				if (_loopEffectsAudioMixerGroup != null)
				{
					_loopEffectsAudioMixerGroup.Item1.audioMixer.SetFloat(_loopEffectsAudioMixerGroup.Item2, value);
				}
				else
				{
					var count = _loopEffectsAudioSourcePool.Count;
					for (var i = 0; i < count; i++)
					{
						_loopEffectsAudioSourcePool[i].volume = value;
					}
				}
			}
		}

		private List<AudioSource> _effectsAudioSourcePool;
        private List<AudioSource> _loopEffectsAudioSourcePool;
		private List<AudioSource> _musicAudioSourcePool;

		private Dictionary<AudioSource, bool> _effectsPlaying;
		private Dictionary<AudioSource, bool> _loopEffectsPlaying;
		private Dictionary<AudioSource, bool> _musicPlaying;

		private DotNet.Tuple<AudioMixerGroup, string> _effectsAudioMixerGroup;
		private DotNet.Tuple<AudioMixerGroup, string> _loopEffectsAudioMixerGroup;
		private DotNet.Tuple<AudioMixerGroup, string> _musicAudioMixerGroup;

		private float _fadeMusicDuration;
		private float _fadeMusicTime = -1f;
		private float _fadeMusicStartVolume;
		private float _fadeMusicTargetVolume;

		private int _lastEffectClipIndex = -1;
		private int _lastLoopEffectClipIndex = -1;
		private int _lastMusicClipIndex = -1;

		#endregion

		#region MonoBehaviour

		protected override void Awake()
        {
            base.Awake();

			// create first audio source for pools
            _effectsAudioSourcePool = new List<AudioSource>() { CreateAudioSourceInScene(GetEffectsAudioSourceName(1)) };
            _loopEffectsAudioSourcePool = new List<AudioSource>() { CreateAudioSourceInScene(GetLoopEffectsAudioSourceName(1)) };
			_musicAudioSourcePool = new List<AudioSource>() { CreateAudioSourceInScene(GetMusicAudioSourceName(1)) };

			_effectsPlaying = new Dictionary<AudioSource, bool>();
			_loopEffectsPlaying = new Dictionary<AudioSource, bool>();
			_musicPlaying = new Dictionary<AudioSource, bool>();

			// convert clip group mapping to group clip mapping
			if (ClipGroups != null && ClipGroups.Dictionary.Count > 0)
            {
                _groupClips = new Dictionary<string, List<string>>();
                foreach (var kvp in ClipGroups.Dictionary)
                {
                    if (!_groupClips.ContainsKey(kvp.Value)) _groupClips[kvp.Value] = new List<string>();
                    _groupClips[kvp.Value].Add(kvp.Key);
                }
            }

            // get clip list from dictionary
            if (EffectClipLibrary != null) _effectClips = EffectClipLibrary.Dictionary.Keys.ToList();
            if (LoopEffectClipLibrary != null) _loopEffectClips = LoopEffectClipLibrary.Dictionary.Keys.ToList();
            if (MusicClipLibrary != null) _musicClips = MusicClipLibrary.Dictionary.Keys.ToList();
        }

        protected override void Update()
		{
			base.Update();

			UpdateMusicFade();

			// throttle a bit
			if (Time.frameCount % 3 == 0) UpdateAudioPlaying(AudioType.Effect, _effectsAudioSourcePool);
			if (Time.frameCount % 3 == 1) UpdateAudioPlaying(AudioType.LoopEffect, _loopEffectsAudioSourcePool);
			if (Time.frameCount % 3 == 2) UpdateAudioPlaying(AudioType.Music, _musicAudioSourcePool);
		}

		private void UpdateAudioPlaying(AudioType audioType, List<AudioSource> pool)
		{
			for (var i = 0; i < pool.Count; i++)
			{
				var audioSource = pool[i];
				if (!audioSource.isPlaying && IsAudioSourcePlaying(audioType, audioSource))
				{
					UpdateAudioSourcePlaying(audioType, audioSource, false);
					ClipStopped?.Invoke(audioType);
				}
			}
		}

	#endregion

		#region AudioMixerGroup

	/// <summary>
	/// Adds <paramref name="audioMixerGroup"/> related to music with <paramref name="volumeParamName"/> as name of volume custom parameter to manage volume via <see cref="AudioMixerGroup"/>.
	/// </summary>
	/// <param name="audioMixerGroup">Instance of <see cref="AudioMixerGroup"/></param>
	/// <param name="volumeParamName">Name of volume custom parameter</param>
	/// <exception cref="ArgumentNullException">Thrown if <paramref name="audioMixerGroup"/> or <paramref name="volumeParamName"/> is null</exception>
	public void AddMusicAudioMixerGroup(AudioMixerGroup audioMixerGroup, string volumeParamName)
		{
			if (audioMixerGroup == null) throw new ArgumentNullException(nameof(audioMixerGroup));
			if (volumeParamName == null) throw new ArgumentNullException(nameof(volumeParamName));
			if (_effectsAudioMixerGroup != null && audioMixerGroup == _effectsAudioMixerGroup.Item1)
				throw new Exception(string.Format(Texts.Errors.ObjectAlreadyAssigned, nameof(audioMixerGroup)));

			_musicAudioMixerGroup = new DotNet.Tuple<AudioMixerGroup, string>(audioMixerGroup, volumeParamName);
		}

        /// <summary>
        /// Adds <paramref name="audioMixerGroup"/> related to effects with <paramref name="volumeParamName"/> as name of volume custom parameter to manage volume via <see cref="AudioMixerGroup"/>.
        /// </summary>
        /// <param name="audioMixerGroup">Instance of <see cref="AudioMixerGroup"/></param>
        /// <param name="volumeParamName">Name of volume custom parameter</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="audioMixerGroup"/> or <paramref name="volumeParamName"/> is null</exception>
		public void AddEffectsAudioMixerGroup(AudioMixerGroup audioMixerGroup, string volumeParamName)
		{
			if (audioMixerGroup == null) throw new ArgumentNullException(nameof(audioMixerGroup));
			if (volumeParamName == null) throw new ArgumentNullException(nameof(volumeParamName));
			if (_musicAudioMixerGroup != null && audioMixerGroup == _musicAudioMixerGroup.Item1)
				throw new Exception(string.Format(Texts.Errors.ObjectAlreadyAssigned, nameof(audioMixerGroup)));

			_effectsAudioMixerGroup = new DotNet.Tuple<AudioMixerGroup, string>(audioMixerGroup, volumeParamName);
		}

        /// <summary>
        /// Adds <paramref name="audioMixerGroup"/> related to loop effects with <paramref name="volumeParamName"/> as name of volume custom parameter to manage volume via <see cref="AudioMixerGroup"/>.
        /// </summary>
        /// <param name="audioMixerGroup">Instance of <see cref="AudioMixerGroup"/></param>
        /// <param name="volumeParamName">Name of volume custom parameter</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="audioMixerGroup"/> or <paramref name="volumeParamName"/> is null</exception>
        public void AddLoopEffectsAudioMixerGroup(AudioMixerGroup audioMixerGroup, string volumeParamName)
        {
            if (audioMixerGroup == null) throw new ArgumentNullException(nameof(audioMixerGroup));
            if (volumeParamName == null) throw new ArgumentNullException(nameof(volumeParamName));
            if (_loopEffectsAudioMixerGroup != null && audioMixerGroup == _loopEffectsAudioMixerGroup.Item1)
                throw new Exception(string.Format(Texts.Errors.ObjectAlreadyAssigned, nameof(audioMixerGroup)));

            _loopEffectsAudioMixerGroup = new DotNet.Tuple<AudioMixerGroup, string>(audioMixerGroup, volumeParamName);
        }

        #endregion

		#region Fade Out

		/// <summary>
		/// Fades music.
		/// </summary>
		/// <param name="duration">Duration of fade</param>
		/// <param name="targetVolume">Target volume</param>
		/// <returns></returns>
		public void FadeMusic(float duration, float targetVolume)
		{
			_fadeMusicStartVolume = MusicVolume;
			_fadeMusicTargetVolume = targetVolume;
			_fadeMusicTime = 0f;
			_fadeMusicDuration = duration;
		}

		/// <summary>
		/// Updates music fade process
		/// </summary>
		private void UpdateMusicFade()
		{
			if (_fadeMusicTime < 0f) return;

			_fadeMusicTime += Time.unscaledDeltaTime;
			var volume = Mathf.Lerp(_fadeMusicStartVolume, _fadeMusicTargetVolume, _fadeMusicTime / _fadeMusicDuration);
			MusicVolume = volume;

			if (_fadeMusicTime >= _fadeMusicDuration) _fadeMusicTime = -1f;
		}

		#endregion

		#region Play/Pause

		/// <summary>
		/// Sets pause status of <see cref="Audio"/> module. Affects instances of <see cref="AudioSource"/> based on <see cref="PauseEffects"/>, <see cref="PauseLoopEffects"/> and <see cref="PauseMusic"/>.
		/// </summary>
		/// <param name="pauseStatus">Pause status</param>
		public void SetPause(bool pauseStatus)
		{
			if (PauseEffects)
			{
				for (var i = 0; i < _effectsAudioSourcePool.Count; i++)
					if (pauseStatus)
						_effectsAudioSourcePool[i].Pause();
					else
						_effectsAudioSourcePool[i].UnPause();
			}

			if (PauseLoopEffects)
			{
				for (var i = 0; i < _loopEffectsAudioSourcePool.Count; i++)
					if (pauseStatus)
						_loopEffectsAudioSourcePool[i].Pause();
					else
						_loopEffectsAudioSourcePool[i].UnPause();
			}

			if (PauseMusic)
			{
				for (var i = 0; i < _musicAudioSourcePool.Count; i++)
					if (pauseStatus)
						_musicAudioSourcePool[i].Pause();
					else
						_musicAudioSourcePool[i].UnPause();
			}
		}

		/// <summary>
		/// Plays random instance of <see cref="AudioClip"/> from clip group by <paramref name="groupName"/>.
		/// </summary>
		/// <param name="audioType">Instance of <see cref="AudioType"/></param>
		/// <param name="groupName">Clip group name</param>
		/// <param name="position">Position of <see cref="AudioSource"/> in world</param>
		/// <param name="timeOffset"><see cref="AudioSource.time"/> offset</param>
		/// <returns>True if succeeded</returns>
		/// <exception cref="NullReferenceException">Thrown if <see cref="ClipGroups"/> is null</exception>
		/// <exception cref="ArgumentNullException">Thrown if no instance of <see cref="AudioClip"/> is found</exception>
		/// <exception cref="Exception">Thrown if no clip library exists</exception>
		public bool PlayRandomClipFromGroup(AudioType audioType, string groupName, Vector3? position = null, float timeOffset = 0f)
		{
			if (_groupClips == null) throw new NullReferenceException(string.Format(Texts.Errors.ObjectCannotBeNull, nameof(ClipGroups)));
			if (!_groupClips.ContainsKey(groupName)) return false;

			var clipNames = _groupClips[groupName];

			// HACK: Unity does not play audio with zero time scale
			var isZeroTimeScale = Time.timeScale == 0f;
			if (isZeroTimeScale) Time.timeScale = 1f;

			var clipName = clipNames[UnityEngine.Random.Range(0, clipNames.Count)];
			PlayClipFromLibrary(audioType, clipName, position, timeOffset);

			if (isZeroTimeScale) Time.timeScale = 0f;

			return true;
		}

		/// <summary>
		/// Plays random instance of <see cref="AudioClip"/> from library of <see cref="AudioType"/>.
		/// </summary>
		/// <param name="audioType">Instance of <see cref="AudioType"/></param>
		/// <param name="position">Position of <see cref="AudioSource"/> in world</param>
		/// <param name="timeOffset"><see cref="AudioSource.time"/> offset</param>
		/// <returns>True if succeeded</returns>
		/// <exception cref="ArgumentNullException">Thrown if no instance of <see cref="AudioClip"/> is found</exception>
		/// <exception cref="Exception">Thrown if no clip library exists</exception>
		public bool PlayRandomClipFromLibrary(AudioType audioType, Vector3? position = null, float timeOffset = 0f)
		{
			string clipName;
			var index = -1;
			switch (audioType)
			{
				case AudioType.Effect:
					if (_effectClips == null) return false;
					index = GetRandomIndex(0, _effectClips.Count, _lastEffectClipIndex);
					_lastEffectClipIndex = index;
					clipName = _effectClips[index];
					break;
				case AudioType.LoopEffect:
					if (_loopEffectClips == null) return false;
					index = GetRandomIndex(0, _loopEffectClips.Count, _lastLoopEffectClipIndex);
					_lastLoopEffectClipIndex = index;
					clipName = _loopEffectClips[index];
					break;
				case AudioType.Music:
					if (_musicClips == null) return false;
					index = GetRandomIndex(0, _musicClips.Count, _lastMusicClipIndex);
					_lastMusicClipIndex = index;
					clipName = _musicClips[index];
					break;
				default:
					return false;
			}

			return PlayClipFromLibrary(audioType, clipName, position, timeOffset);
		}

		/// <summary>
		/// Plays clip defined by <paramref name="clipName"/> from library given by <see cref="AudioType"/>.
		/// </summary>
		/// <param name="audioType">Instance of <see cref="AudioType"/></param>
		/// <param name="clipName">Name of <see cref="AudioClip"/> in clip library</param>
		/// <param name="position">Position of <see cref="AudioSource"/> in world</param>
		/// <param name="timeOffset"><see cref="AudioSource.time"/> offset</param>
		/// <returns>True if succeeded</returns>
		/// <exception cref="ArgumentNullException">Thrown if instance of <see cref="AudioClip"/> defined by <paramref name="clipName"/> is null</exception>
		/// <exception cref="Exception">Thrown if no clip library exists</exception>
		public bool PlayClipFromLibrary(AudioType audioType, string clipName, Vector3? position = null, float timeOffset = 0f)
		{
			var library = GetClipLibrary(audioType);
			if (library == null) throw new Exception(string.Format(Texts.Errors.ObjectCannotBeNull, nameof(library)));

			if (!library.Dictionary.ContainsKey(clipName)) return false;

			var clip = library.Dictionary[clipName];
			PlayClip(audioType, clip, position, timeOffset);

			return true;
		}

		/// <summary>
		/// Plays clip of <see cref="AudioClip"/> type via <see cref="AudioSource"/> of given <paramref name="audioType"/>.
		/// </summary>
		/// <param name="audioType">Instance of <see cref="AudioType"/></param>
		/// <param name="clip">Instance of <see cref="AudioClip"/></param>
		/// <param name="position">Position of <see cref="AudioSource"/> in world</param>
		/// <param name="timeOffset"><see cref="AudioSource.time"/> offset</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="clip"/> is null</exception>
		private void PlayClip(AudioType audioType, AudioClip clip, Vector3? position = null, float timeOffset = 0f)
		{
			if (clip == null) throw new ArgumentNullException(nameof(clip));

			// HACK: Unity does not play audio with zero time scale
			var isZeroTimeScale = Time.timeScale == 0f;
			if (isZeroTimeScale) Time.timeScale = 1f;

			var audioSource = GetAvailableAudioSource(audioType);
			audioSource.clip = clip;
			audioSource.volume = GetVolume(audioType);
			if (timeOffset != 0f) audioSource.time = timeOffset;
			if (position.HasValue)
				audioSource.gameObject.transform.position = position.Value;
			else
				audioSource.gameObject.transform.position = Vector3.zero;
			audioSource.Play();

			if (isZeroTimeScale) Time.timeScale = 0f;

			UpdateAudioSourcePlaying(audioType, audioSource, true);
			ClipStarted?.Invoke(audioType);
		}

		/// <summary>
		/// Stops all sounds of <paramref name="audioType"/>.
		/// </summary>
		/// <param name="audioType">Instance of <see cref="AudioType"/></param>
		public void StopAllClips(AudioType audioType)
		{
			switch (audioType)
			{
				case AudioType.Effect:
					for (var i = 0; i < _effectsAudioSourcePool.Count; i++)
					{
						UpdateAudioSourcePlaying(audioType, _effectsAudioSourcePool[i], false);
						_effectsAudioSourcePool[i].Stop();
					}
					break;
				case AudioType.LoopEffect:
					for (var i = 0; i < _loopEffectsAudioSourcePool.Count; i++)
					{
						UpdateAudioSourcePlaying(audioType, _loopEffectsAudioSourcePool[i], false);
						_loopEffectsAudioSourcePool[i].Stop();
					}
					break;
				case AudioType.Music:
					for (var i = 0; i < _musicAudioSourcePool.Count; i++)
					{
						UpdateAudioSourcePlaying(audioType, _musicAudioSourcePool[i], false);
						_musicAudioSourcePool[i].Stop();
					}
					break;
				default:
					throw new ArgumentNullException(nameof(audioType));
			}

			ClipStopped?.Invoke(audioType);
		}

		/// <summary>
		/// Stops clip defined by <paramref name="clipName"/>. If <paramref name="audioType"/> is not specified, tries to find the clip in all libraries (slower).
		/// </summary>
		/// <param name="clipName">Clip name, key to library</param>
		/// <param name="audioType">Instance of <see cref="AudioType"/></param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="clipName"/> is null</exception>
		public void StopClip(string clipName, AudioType? audioType = null)
		{
			if (clipName == null) throw new ArgumentNullException(nameof(clipName));

			AudioSource audioSource = null;
			if (audioType.HasValue) audioSource = GetAudioSourceByClipName(clipName, audioType.Value);

			if (audioSource == null) audioSource = GetAudioSourceByClipName(clipName, AudioType.Effect);
			if (audioSource == null) audioSource = GetAudioSourceByClipName(clipName, AudioType.LoopEffect);
			if (audioSource == null) audioSource = GetAudioSourceByClipName(clipName, AudioType.Music);
			if (audioSource == null) return;

			audioSource.Stop();
		}

		#endregion

		#region Helpers

		/// <summary>
		/// Returns instance of <see cref="AudioSource"/> given by <paramref name="clipName"/> and <paramref name="audioType"/> which defines the library used.
		/// </summary>
		/// <param name="clipName">Clip name, key to library</param>
		/// <param name="audioType">Instance of <see cref="AudioType"/></param>
		/// <returns></returns>
		private AudioSource GetAudioSourceByClipName(string clipName, AudioType audioType)
		{
			StringAudioClipDictionary clipLibrary = null;
			List<AudioSource> audioSources = null;
			switch (audioType)
			{
				case AudioType.Effect:
					clipLibrary = EffectClipLibrary;
					audioSources = _effectsAudioSourcePool;
					break;
				case AudioType.LoopEffect:
					clipLibrary = LoopEffectClipLibrary;
					audioSources = _loopEffectsAudioSourcePool;
					break;
				case AudioType.Music:
					clipLibrary = MusicClipLibrary;
					audioSources = _musicAudioSourcePool;
					break;
				default:
					return null;
			}

			if (!clipLibrary.Dictionary.ContainsKey(clipName)) return null;

			for (var i = 0; i < audioSources.Count; i++)
			{
				if (audioSources[i].clip == clipLibrary.Dictionary[clipName])
				{
					return audioSources[i];
				}
			}

			return null;
		}

		/// <summary>
		/// Returns volume by <paramref name="audioType"/>.
		/// </summary>
		/// <param name="audioType">Instance of <see cref="AudioType"/></param>
		/// <returns>Volume value</returns>
		private float GetVolume(AudioType audioType)
		{
			switch (audioType)
			{
				case AudioType.Effect:
					return EffectsVolume;
				case AudioType.LoopEffect:
					return LoopEffectsVolume;
				case AudioType.Music:
					return MusicVolume;
				default:
					return 0;
			}
		}

		/// <summary>
		/// Returns first available <see cref="AudioSource"/> to play the sound or creates a new instance.
		/// </summary>
		/// <param name="audioType">Instance of <see cref="AudioType"/></param>
		/// <returns>First available <see cref="AudioSource"/> to play the sound or a new instance</returns>
		private AudioSource GetAvailableAudioSource(AudioType audioType)
		{
			List<AudioSource> audioSources;
			switch (audioType)
			{
				case AudioType.Effect:
					audioSources = _effectsAudioSourcePool;
					break;
				case AudioType.LoopEffect:
					audioSources = _loopEffectsAudioSourcePool;
					break;
				case AudioType.Music:
					audioSources = _musicAudioSourcePool;
					break;
				default:
					return null;
			}

			for (var i = 0; i < audioSources.Count; i++)
			{
				if (!audioSources[i].isPlaying)
				{
					if (audioType == AudioType.LoopEffect) audioSources[i].loop = true;
					return audioSources[i];
				}
			}

			string audioSourceName;
			AudioSource audioSource;
			switch (audioType)
			{
				case AudioType.Effect:
					audioSourceName = GetEffectsAudioSourceName(_effectsAudioSourcePool.Count + 1);
					audioSource = CreateAudioSourceInScene(audioSourceName);
					_effectsAudioSourcePool.Add(audioSource);
					break;
				case AudioType.LoopEffect:
					audioSourceName = GetLoopEffectsAudioSourceName(_loopEffectsAudioSourcePool.Count + 1);
					audioSource = CreateAudioSourceInScene(audioSourceName);
					audioSource.loop = true;
					_loopEffectsAudioSourcePool.Add(audioSource);
					break;
				case AudioType.Music:
					audioSourceName = GetMusicAudioSourceName(_musicAudioSourcePool.Count + 1);
					audioSource = CreateAudioSourceInScene(audioSourceName);
					_musicAudioSourcePool.Add(audioSource);
					break;
				default:
					return null;
			}

			return audioSource;
		}

		/// <summary>
		/// Returns appropriate clip library based on <paramref name="audioType"/>.
		/// </summary>
		/// <param name="audioType">Instance of <see cref="AudioType"/></param>
		/// <returns>Clip library based on <paramref name="audioType"/></returns>
		private SerializableDictionary<string, AudioClip> GetClipLibrary(AudioType audioType)
		{
			SerializableDictionary<string, AudioClip> library;
			switch (audioType)
			{
				case AudioType.Effect:
					library = EffectClipLibrary;
					break;
				case AudioType.LoopEffect:
					library = LoopEffectClipLibrary;
					break;
				case AudioType.Music:
					library = MusicClipLibrary;
					break;
				default:
					return null;
			}
			return library;
		}

		/// <summary>
		/// Creates <see cref="GameObject"/> with <see cref="AudioSource"/> named by <paramref name="name"/>.
		/// On created also <see cref="DontDestroy"/> components is added on <see cref="GameObject"/>.
		/// </summary>
		/// <param name="name">Name of <see cref="GameObject"/> instance</param>
		/// <returns>Instance of <see cref="AudioSource"/></returns>
		private AudioSource CreateAudioSourceInScene(string name)
		{
			var go = new GameObject(name);
			go.AddComponent<DontDestroy>();
			return go.AddComponent<AudioSource>();
		}

		private string GetEffectsAudioSourceName(int index)
		{
			return "effectsAudioSource" + index.ToString();
		}

		private string GetLoopEffectsAudioSourceName(int index)
		{
			return "loopEffectsAudioSource" + index.ToString();
		}

		private string GetMusicAudioSourceName(int index)
		{
			return "musicAudioSource" + index.ToString();
		}

		private int GetRandomIndex(int min, int max, int last)
		{
			if (min >= max || min < 0
				|| max - min == 1 && min == last) return min;
			while (true)
			{
				var index = UnityEngine.Random.Range(min, max);
				if (last >= 0 && index == last) continue;
				return index;
			}
		}

		private bool IsAudioSourcePlaying(AudioType audioType, AudioSource audioSource)
		{
			switch (audioType)
			{
				case AudioType.Effect:
					return _effectsPlaying.ContainsKey(audioSource) && _effectsPlaying[audioSource];
				case AudioType.LoopEffect:
					return _loopEffectsPlaying.ContainsKey(audioSource) && _loopEffectsPlaying[audioSource];
				case AudioType.Music:
					return _musicPlaying.ContainsKey(audioSource) && _musicPlaying[audioSource];
				default:
					throw new ArgumentException(nameof(audioType));
			}
		}

		private void UpdateAudioSourcePlaying(AudioType audioType, AudioSource audioSource, bool isPlaying)
		{
			switch (audioType)
			{
				case AudioType.Effect:
					_effectsPlaying[audioSource] = isPlaying;
					break;
				case AudioType.LoopEffect:
					_loopEffectsPlaying[audioSource] = isPlaying;
					break;
				case AudioType.Music:
					_musicPlaying[audioSource] = isPlaying;
					break;
				default:
					throw new ArgumentException(nameof(audioType));
			}
		}

		#endregion
	}
}
