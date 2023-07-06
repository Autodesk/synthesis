#define DEBUG_ANALYTICS

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Synthesis.PreferenceManager;
using Unity.Services.Analytics;
using Unity.Services.Core;
using UnityEngine;

namespace Analytics
{
    // TODO: after testing, disable when in editor
    
    /// <summary>
    /// Handles unity analytics initialization and sending custom events.
    /// View analytics on the <a href = "https://dashboard.unity3d.com/">Unity Dashboard</a>
    /// </summary>
    public class AnalyticsManager : MonoBehaviour
    {
        public const string USE_ANALYTICS_PREF = "analytics/use_analytics";

        private static bool _useAnalytics;

        private async void Start()
        {
            _useAnalytics = PreferenceManager.GetPreference<bool>(USE_ANALYTICS_PREF);
            
            SynthesisAPI.EventBus.EventBus.NewTypeListener<PostPreferenceSaveEvent>(e => {
                bool useAnalytics = PreferenceManager.GetPreference<bool>(USE_ANALYTICS_PREF);
                
                if (useAnalytics == _useAnalytics)
                    return;
                
                _useAnalytics = useAnalytics;
                
                if (useAnalytics)
                    StartDataCollection();
                else
                    StopDataCollection();
            });
            
            await UnityServices.InitializeAsync();
            
            #if DEBUG_ANALYTICS
                Debug.Log("<color=#A2D9FF> Unity services initialized</color>");
            #endif

            //TODO: check if the user gives consent to collect information
            
            if (PreferenceManager.GetPreference<bool>(USE_ANALYTICS_PREF))
                StartDataCollection();
        }

        /// <summary>Tells unity analytics to start collecting data</summary>
        public static void StartDataCollection()
        {
            if (!_useAnalytics)
                return;
            
            if (UnityServices.State != ServicesInitializationState.Initialized)
            {
                Debug.LogError("Unity services not yet initialized. Call UnityServices.InitializeAsync() before starting data collection.");
                return;
            }

            AnalyticsService.Instance.StartDataCollection();
            
            #if DEBUG_ANALYTICS
                Debug.Log("<color=#A2D9FF>Analytics data collected started</color>");
            #endif
        }

        /// <summary>Tells unity analytics to stop collecting data</summary>
        public static void StopDataCollection()
        {
            if (UnityServices.State != ServicesInitializationState.Initialized)
            {
                Debug.LogError("Unity services not yet initialized. Call UnityServices.InitializeAsync() before starting data collection.");
                return;
            }
            
            AnalyticsService.Instance.StopDataCollection();
            
            #if DEBUG_ANALYTICS
                Debug.Log("<color=#A2D9FF>Analytics data collected stopped</color>");
            #endif
        }

        /// <summary>Records a custom event</summary>
        /// <param name="name">The name of the event</param>
        /// <param name="parameters">The parameters sent with the event</param>
        public static async void LogCustomEvent(AnalyticsEvent name, params (string name, object data)[] parameters)
        {
            if (!_useAnalytics)
                return;
            
            Dictionary<string, object> parameterDictionary = null;
            if (parameters.Length > 0)
                parameterDictionary = parameters.ToDictionary(x => x.name, x => x.data);
            
            // Wait until unity services are initialized 
            while (UnityServices.State != ServicesInitializationState.Initialized)
                await Task.Delay(1000);

            if (parameterDictionary == null)
                AnalyticsService.Instance.CustomData(name.ToString());
            else AnalyticsService.Instance.CustomData(name.ToString(), parameterDictionary);

            #if DEBUG_ANALYTICS
                Debug.Log($"<color=#A2D9FF>Logged custom event: \"{name}\"{((parameterDictionary != null) ? $" with parameters: {string.Join(", ", parameterDictionary)}" : "")} </color>");
            #endif
        }
    }

    /// <summary>
    /// A specific custom event. <b>Make sure to add new events to the event manager on the
    /// <a href = "https://dashboard.unity3d.com/">Unity Dashboard</a></b>
    /// </summary>
    public enum AnalyticsEvent
    {
        ModalCreated,
        PanelCreated,
        ActiveModalClosed,
        PanelClosed,
        ExitedToMenu,
        ApplicationQuit,
        SettingsSaved,
        SettingsReset
    }
}
