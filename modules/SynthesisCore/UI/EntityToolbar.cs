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
        private static bool isOpen = false;
        
        private static void CreateToolbar()
        {
            entityTab = new Tab("Entity", Ui.ToolbarAsset, toolbarElement => {
                // Populate tabs of toolbar
                var modifyCategory = ToolbarTools.AddButtonCategory(toolbarElement, "MODIFY");
                ToolbarTools.AddButton(modifyCategory, "move-entity-button", "/modules/synthesis_core/UI/images/move-entity-icon.png",
                    _ => {
                        openedMoveArrows = true;
                        MoveArrows.MoveEntity(selectedEntity);
                    });
                ToolbarTools.AddButton(modifyCategory, "delete-entity-button", "/modules/synthesis_core/UI/images/delete-icon.png",
                    _ => EnvironmentManager.RemoveEntity(selectedEntity));
            });
            toolbarCreated = true;
        }

        public static void Open(Entity entity)
        {
            if (!toolbarCreated)
                CreateToolbar();

            selectedEntity = entity;
            
            if(!isOpen)
                UIManager.AddTab(entityTab);
            
            UIManager.SelectTab(entityTab.Name);
            
            isOpen = true;
        }

        public static void Close()
        {
            if (openedMoveArrows)
            {
                MoveArrows.StopMovingEntity();
            }
            UIManager.RemoveTab(entityTab.Name);
            isOpen = false;
        }
    }
}
