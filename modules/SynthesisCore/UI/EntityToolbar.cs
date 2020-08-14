using SynthesisAPI.EnvironmentManager;
using SynthesisAPI.EventBus;
using SynthesisAPI.UIManager;
using SynthesisAPI.UIManager.UIComponents;
using SynthesisAPI.UIManager.VisualElements;
using SynthesisCore.Systems;

namespace SynthesisCore.UI
{
    public class EntityToolbar
    {
        private static Tab entityTab;
        private static Entity selectedEntity;

        private static bool toolbarCreated = false;
        private static bool openedMoveArrows = false;
        
        private static void CreateToolbar()
        {
            entityTab = new Tab("Entity", Ui.ToolbarAsset, toolbarElement => {
                // Populate tabs of toolbar
                var testCategory = ToolbarTools.AddButtonCategory(toolbarElement, "MODIFY");
                ToolbarTools.AddButton(testCategory, "move-entity-button", "/modules/synthesis_core/UI/images/move-entity-icon.png",
                    _ => {
                        openedMoveArrows = true;
                        MoveArrows.MoveEntity(selectedEntity);
                    });
                ToolbarTools.AddButton(testCategory, "delete-entity-button", "/modules/synthesis_core/UI/images/delete-icon.png",
                    _ => EnvironmentManager.RemoveEntity(selectedEntity));
            });
            toolbarCreated = true;
        }

        public static void Open(Entity entity)
        {
            if (!toolbarCreated)
                CreateToolbar();

            selectedEntity = entity;

            UIManager.AddTab(entityTab);
            UIManager.SelectTab(entityTab.Name);
        }

        public static void Close()
        {
            if (openedMoveArrows)
            {
                MoveArrows.StopMovingEntity();
            }
            UIManager.RemoveTab(entityTab.Name);
        }
    }
}
