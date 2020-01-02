using Elia.Unity.Components.GameObjects;
using Elia.Unity.Infrastructure;
using UnityEngine;

namespace Elia.Unity.Modules
{
	[AddComponentMenu("ELIA/Modules/Analytics")]
	public sealed class Analytics : BehaviourAwareSingleton<Analytics>
	{


		public void AddProvider(IAnalyticsProvider provider)
		{

		}

		public void RemoveProvider(IAnalyticsProvider provider)
		{

		}
	}
}
