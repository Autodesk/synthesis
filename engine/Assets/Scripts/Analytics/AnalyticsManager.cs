#define DEBUG_ANALYTICS

using System.Collections.Generic;
using Unity.Services.Analytics;
using Unity.Services.Core;
using UnityEngine;

namespace Analytics
{
    // TODO: after testing, disable when in editor
    
    /// <summary>
    /// Contains all analytics logic and handles sending custom events. 
    /// </summary>
    public class AnalyticsManager : MonoBehaviour
    {
        public const bool debug = false;

        private async void Start()
        {
            await UnityServices.InitializeAsync();

            //TODO: check if the user gives consent to collect information
            StartDataCollection();
            
            #if DEBUG_ANALYTICS
                Debug.Log("<color=#A2D9FF> Unity services initialized</color>");
            #endif
        }

        /// <summary>Tells unity analytics to start collecting data</summary>
        public static void StartDataCollection()
        {
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
            AnalyticsService.Instance.StopDataCollection();
            
            #if DEBUG_ANALYTICS
                Debug.Log("<color=#A2D9FF>Analytics data collected stopped</color>");
            #endif
        }

        /// <summary>Records a custom event</summary>
        /// <param name="name">The name of the event</param>
        /// <param name="parameters">The parameters sent with the event</param>
        public static void LogCustomEvent(string name, Dictionary<string, object> parameters = null)
        {
            if (UnityServices.State != ServicesInitializationState.Initialized)
            {
                Debug.LogError("Unity services not yet initialized. Call UnityServices.InitializeAsync() before logging custom events.");
                return;
            }
            
            if (parameters == null)
                AnalyticsService.Instance.CustomData(name);
            else AnalyticsService.Instance.CustomData(name, parameters);

            #if DEBUG_ANALYTICS
                Debug.Log($"<color=#A2D9FF>Logged custom event: \"{name}\"{((parameters != null) ? $" with parameters: {string.Join(", ", parameters)}" : "")} </color>");
            #endif
        }
    }
}
