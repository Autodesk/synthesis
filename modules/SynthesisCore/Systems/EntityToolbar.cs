using SynthesisAPI.EnvironmentManager;
using SynthesisAPI.EventBus;
using SynthesisAPI.UIManager;
using SynthesisAPI.UIManager.UIComponents;
using SynthesisAPI.UIManager.VisualElements;

namespace SynthesisCore.Systems
{
    public class EntityToolbar
    {
        private static Tab entityTab;
        private static Entity selectedEntity;

        private static bool toolbarCreated = false;
        private static bool openedMoveArrows = false;
        
        private static void CreateToolbar()
        {
            entityTab = new Tab("Entity", Ui.TabToolbar, toolbarElement => {
                // Populate tabs of toolbar
                AddButton(toolbarElement, "move-entity-button", "/modules/synthesis_core/UI/images/move-entity-icon.png",
                    _ => {
                        openedMoveArrows = true;
                        MoveArrows.MoveEntity(selectedEntity);
                    });
                AddButton(toolbarElement, "delete-entity-button", "/modules/synthesis_core/UI/images/delete-icon.png",
                    _ => EnvironmentManager.RemoveEntity(selectedEntity));
            });
            toolbarCreated = true;
        }

        private static void AddButton(VisualElement toolbarElement, string buttonName, string iconPath, System.Action<IEvent> callback)
         {
            var buttonContainer = Ui.Toolbartabcontainer.GetElement(buttonName);
            toolbarElement.Get(className: "tab-toolbar").Add(buttonContainer);
            var button = (Button)buttonContainer.Get(className: "toolbar-tab-button");
            button.Name = buttonName;
            button.Subscribe(e => callback(e));
            var iconContainer = button.Get(className: "toolbar-tab-icon");
            iconContainer.SetStyleProperty("background-image", iconPath);
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
