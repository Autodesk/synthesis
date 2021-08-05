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
            if (UpdaterLink == string.Empty)
                Debug.LogWarning("No updater link provided");
            else
                Process.Start(UpdaterLink);

            if (Application.isEditor)
                Debug.Log("Would exit, but it's editor mode");
            else
                Application.Quit();

            base.Close();
        }
    }
}
