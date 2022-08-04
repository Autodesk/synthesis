using System.Linq;
using Synthesis.Gizmo;
using Synthesis.UI.Dynamic;
using SynthesisAPI.Simulation;

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
                () => DynamicUIManager.CreateModal<AddFieldModal>()
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
            // CreateButton(
            //     "Move",
            //     SynthesisAssetCollection.GetSpriteByName("fieldimport"),
            //     () => {
            //         var robot = SimulationManager.SimulationObjects.Values.FirstOrDefault(x => x is RobotSimObject);
            //         if (robot != null)
            //             GizmoManager.SpawnGizmo(robot as RobotSimObject);
            //     }
            // );
            CreateButton(
                "Test Modal",
                SynthesisAssetCollection.GetSpriteByName("fieldimport"),
                () => {
                    DynamicUIManager.CreateModal<ScrollViewTestModal>();
                }
            );
            CreateButton(
                "Pickup",
                SynthesisAssetCollection.GetSpriteByName("fieldimport"),
                () => {
                    DynamicUIManager.CreatePanel<ConfigureGamepiecePickupPanel>();
                }
            );
            CreateButton(
                "Shooting",
                SynthesisAssetCollection.GetSpriteByName("fieldimport"),
                () => {
                    DynamicUIManager.CreatePanel<ConfigureShotTrajectoryPanel>();
                }
            );
        }
    }
}
