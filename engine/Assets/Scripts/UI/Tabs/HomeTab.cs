using Synthesis.UI.Dynamic;

namespace Synthesis.UI.Tabs {
    public class HomeTab : Tab {
        public override void Create() {
            CreateButton(
                "Load Robot",
                SynthesisAssetCollection.GetSpriteByName("robotimport"),
                () => DynamicUIManager.CreateModal<AddRobotModal>()
            );
            CreateButton(
                "Load Field",
                SynthesisAssetCollection.GetSpriteByName("fieldimport"),
                () => DynamicUIManager.CreateModal<SelectFieldModal>()
            );
            CreateDivider();
            CreateButton(
                "Details",
                SynthesisAssetCollection.GetSpriteByName("Multiplayer1-Gray"),
                () => DynamicUIManager.CreatePanel<RobotDetailsPanel>()
            );
            CreateDivider();
            CreateButton(
                "Scoreboard",
                SynthesisAssetCollection.GetSpriteByName("fieldimport"),
                () => LayoutManager.OpenPanel(SynthesisAssetCollection.GetPanelByName("Scoreboard-Panel")));
        }
    }
}
