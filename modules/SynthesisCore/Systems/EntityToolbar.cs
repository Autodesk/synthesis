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
            entityTab = new Tab("Entity", Ui.ToolbarAsset, toolbarElement => {
                // Populate tabs of toolbar
                var testCategory = AddButtonCategory(toolbarElement, "MODIFY");
                AddButton(testCategory, "move-entity-button", "/modules/synthesis_core/UI/images/move-entity-icon.png",
                    _ => {
                        openedMoveArrows = true;
                        MoveArrows.MoveEntity(selectedEntity);
                    });
                AddButton(testCategory, "delete-entity-button", "/modules/synthesis_core/UI/images/delete-icon.png",
                    _ => EnvironmentManager.RemoveEntity(selectedEntity));
            });
            toolbarCreated = true;
        }

        private static VisualElement AddButtonCategory(VisualElement toolbarElement, string label)
        {
            var category = Ui.ToolbarCategoryAsset.GetElement($"toolbar-category-{label}");
            toolbarElement.Get(className: "toolbar").Add(category);

            var labelElement = (Label) category.Get(className: "toolbar-category-label");
            labelElement.Text = label;
            return category.Get(className: "toolbar-category-inner");
        }

        private static void AddButton(VisualElement toolbarCategory, string buttonName, string iconPath, System.Action<IEvent> callback)
         {
            var buttonContainer = Ui.ToolbarButtonAsset.GetElement(buttonName);
            toolbarCategory.Get(className: "toolbar-category-inner").Add(buttonContainer);
            var button = (Button)buttonContainer.Get(className: "toolbar-button");
            button.Name = buttonName;
            button.Subscribe(e => callback(e));
            var iconContainer = button.Get(className: "toolbar-button-icon");
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
