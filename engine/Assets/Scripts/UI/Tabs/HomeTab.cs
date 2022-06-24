using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SynthesisAPI.Utilities;
using Logger = SynthesisAPI.Utilities.Logger;
using Synthesis.UI.Dynamic;

namespace Synthesis.UI.Tabs {
    public class HomeTab : Tab {
        public override void Create() {
            CreateButton(
                "Load Robot",
                SynthesisAssetCollection.GetSpriteByName("robotimport"),
                () => ModalManager.CreateModal<AddRobotModal>()
            );
            CreateButton(
                "Load Field",
                SynthesisAssetCollection.GetSpriteByName("fieldimport"),
                () => LayoutManager.OpenPanel(SynthesisAssetCollection.GetPanelByName("Load-Field-Panel"))
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
