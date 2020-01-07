using System;
using Elia.Unity.Components.GameObjects;
using Elia.Unity.Infrastructure;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Elia.Unity.Modules
{
	/// <summary>
	/// Analytics module registers analytics providers and provide interface to log events.
	/// </summary>
	[AddComponentMenu("ELIA/Modules/Analytics")]
	public sealed class Analytics : BehaviourAwareSingleton<Analytics>, IAnalyticsProvider
	{
		/// <summary>
		/// Invoked on provider initialize.
		/// </summary>
		public Action<IAnalyticsProvider> ProviderInitialized { get; set; }

		private List<IAnalyticsProvider> _providers;

		protected override void Awake()
		{
			base.Awake();

			if (_providers == null) _providers = new List<IAnalyticsProvider>();
		}

		/// <summary>
		/// Adds analytics provider.
		/// </summary>
		/// <typeparam name="T">Type of provider</typeparam>
		/// <exception cref="Exception">Thrown if provider is already assigned</exception>
		public void AddProvider<T>()
			where T : BehaviourAware, IAnalyticsProvider
		{
			if (_providers == null) _providers = new List<IAnalyticsProvider>();
			if (_providers.Any(x => x.GetType() == typeof(T))) throw new Exception(string.Format(Texts.Errors.ObjectAlreadyAssigned, typeof(T).Name));

			var provider = gameObject.AddComponent<T>();
			_providers.Add(provider);
			provider.ProviderInitialized += (p) =>
			{
				ProviderInitialized?.Invoke(p);
			};
		}

		/// <summary>
		/// Removes analytics provider.
		/// </summary>
		/// <typeparam name="T">Type of provider</typeparam>
		public void RemoveProvider<T>()
			where T : IAnalyticsProvider
		{
			_providers.RemoveAll(x => x.GetType() == typeof(T));
		}

		/// <summary>
		/// Logs event with parameters.
		/// </summary>
		/// <param name="eventName">Event name</param>
		/// <param name="keyValueTuples">Array of key values representing event parameters</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="eventName"/> is null</exception>
		public void LogEvent(string eventName, Tuple<string, object>[] keyValueTuples)
		{
			if (_providers == null)
			{
				App.Instance.Logger.LogError(Texts.Tags.Analytics, string.Format(Texts.Errors.ObjectIsInitialized, nameof(Analytics)));
				return;
			}

			foreach (var provider in _providers)
			{
				provider.LogEvent(eventName, keyValueTuples);
			}
		}
	}
}
