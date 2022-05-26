using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SynthesisAPI.Utilities;
using Logger = SynthesisAPI.Utilities.Logger;

namespace Synthesis.UI.Tabs {
    public class HomeTab : Tab {
        public override void Create() {
            CreateButton(
                "Load Robot",
                SynthesisAssetCollection.GetSpriteByName("robotimport"),
                () => LayoutManager.OpenPanel(SynthesisAssetCollection.GetPanelByName("Load-Robot-Panel"))
            );
            CreateButton(
                "Load Field",
                SynthesisAssetCollection.GetSpriteByName("fieldimport"),
                () => LayoutManager.OpenPanel(SynthesisAssetCollection.GetPanelByName("Load-Field-Panel"))
            );
            CreateButton(
                "Controls",
                SynthesisAssetCollection.GetSpriteByName("DriverStationView Gray"),
                () => LayoutManager.OpenPanel(SynthesisAssetCollection.GetPanelByName("InputPanel"))
            );
            CreateDivider();
            CreateButton(
                "Robots",
                SynthesisAssetCollection.GetSpriteByName("Multiplayer1-Gray"),
                () => LayoutManager.OpenPanel(SynthesisAssetCollection.GetPanelByName("Multiplayer-Panel"))
            );
        }
    }
}
