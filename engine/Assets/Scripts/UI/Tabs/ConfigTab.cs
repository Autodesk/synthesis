using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Synthesis.UI.Tabs {
    public class ConfigTab : Tab {
        public override void Create() {
            CreateButton("Controls", SynthesisAssetCollection.GetSpriteByName("DriverStationView Gray"),
                () => LayoutManager.OpenPanel(SynthesisAssetCollection.GetPanelByName("InputPanel")));
            CreateButton("Drivers", SynthesisAssetCollection.GetSpriteByName("joint-icon-disabled"),
                () => LayoutManager.OpenPanel(SynthesisAssetCollection.GetPanelByName("Configure-Joints-Panel")));
        }
    }
}
