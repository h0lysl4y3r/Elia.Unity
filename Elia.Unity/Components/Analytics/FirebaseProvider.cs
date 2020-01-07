using Firebase;
using Firebase.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Elia.Unity;
using Elia.Unity.Infrastructure;
using Firebase.Analytics;

namespace Elia.Unity.Components.Analytics
{
    /// <summary>
    /// Provider for Firebase analytics.
    /// </summary>
	public class FirebaseProvider : BehaviourAware, IAnalyticsProvider
	{
        private DependencyStatus _dependencyStatus = DependencyStatus.UnavailableOther;
        private bool _firebaseInitialized = false;

        /// <summary>
        /// Invoked on provider initialized.
        /// </summary>
        public Action<IAnalyticsProvider> ProviderInitialized { get; set; }

        protected override void Start()
		{
			base.Start();

            // When the app starts, check to make sure that we have
            // the required dependencies to use Firebase, and if not,
            // add them if possible.
            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
                _dependencyStatus = task.Result;
                if (_dependencyStatus == DependencyStatus.Available)
                {
                    InitializeFirebase();
                }
                else
                {
                    Modules.App.Instance.Logger.LogError(Texts.Tags.Analytics, 
                        string.Format(Texts.Errors.CouldNotResolveDependenciesForObject, "Firebase: ") + _dependencyStatus);
                }
            });
        }

        /// <summary>
        /// Initializes Firebase.
        /// </summary>
        private void InitializeFirebase()
        {
            FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
            FirebaseAnalytics.SetSessionTimeoutDuration(new TimeSpan(0, 30, 0));
            _firebaseInitialized = true;
            ProviderInitialized?.Invoke(this);
        }

        /// <summary>
        /// Logs event with parameters.
        /// </summary>
        /// <param name="eventName">Event name</param>
        /// <param name="keyValueTuples">Array of key values representing event parameters</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="eventName"/> is null</exception>
        public void LogEvent(string eventName, Tuple<string, object>[] keyValueTuples)
        {
            if (eventName == null) throw new ArgumentNullException(nameof(eventName));
            if (!_firebaseInitialized)
            {
                Modules.App.Instance.Logger.LogError(Texts.Tags.Analytics, string.Format(Texts.Errors.ObjectIsInitialized, "Firebase"));
                return;
            }

            if (keyValueTuples == null)
            {
                FirebaseAnalytics.LogEvent(eventName);
            } else
            {
                var parameters = new List<Parameter>();
                parameters.AddRange(keyValueTuples.Where(x => x.Item2 is double).Select(x => new Parameter(x.Item1, (double)x.Item2)));
                parameters.AddRange(keyValueTuples.Where(x => x.Item2 is string).Select(x => new Parameter(x.Item1, (string)x.Item2)));
                parameters.AddRange(keyValueTuples.Where(x => x.Item2 is int).Select(x => new Parameter(x.Item1, (int)x.Item2)));
                FirebaseAnalytics.LogEvent(eventName, parameters.ToArray());
            }
        }
    }
}
