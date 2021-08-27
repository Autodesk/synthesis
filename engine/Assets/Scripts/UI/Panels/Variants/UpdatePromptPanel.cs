using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Synthesis.UI.Panels;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Synthesis.UI.Panels {
    public class UpdatePromptPanel: Panel {
        public string UpdaterLink = string.Empty;

        public void Agreed() {

            bool updateAgreed = false;

            if (UpdaterLink == string.Empty)
            {
                Debug.LogWarning("No updater link provided");
            }
            else
            {
                updateAgreed = true;

                Process.Start(UpdaterLink);

                var update = new AnalyticsEvent(category: "Startup", action: "Update Prompted", label: $"Update Agreed");
                AnalyticsManager.LogEvent(update);
                AnalyticsManager.PostData();
            }

            if(updateAgreed == false)
            {
                Debug.Log("Update Declined");
                var update = new AnalyticsEvent(category: "Startup", action: "Update Prompted", label: $"Update Declined");
                AnalyticsManager.LogEvent(update);
                AnalyticsManager.PostData();
            }

            if (Application.isEditor)
                Debug.Log("Would exit, but it's editor mode");
            else
                Application.Quit();

            base.Close();
        }
    }
}
