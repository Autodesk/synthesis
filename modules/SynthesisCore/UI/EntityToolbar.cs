using System.Reflection;
using SynthesisAPI.EnvironmentManager;
using SynthesisAPI.EnvironmentManager.Components;
using SynthesisAPI.Modules.Attributes;
using SynthesisAPI.UIManager;
using SynthesisAPI.UIManager.UIComponents;
using SynthesisAPI.UIManager.VisualElements;
using SynthesisAPI.EventBus;
using SynthesisCore.EntityMovement;

namespace SynthesisCore.UI
{
    public static class EntityToolbar
    {
        private static Tab entityTab;
        private static bool toolbarCreated = false;
        private static bool isToolbarBound = false;

        private static bool openedMoveArrows = false;
        private static bool openedJointEditor = false;

        private static bool isEntitySelected => Selectable.Selected != null;

        private static VisualElement moveEntityButton = null;
        private const string MoveEntityButtonIcon = "/modules/synthesis_core/UI/images/move-entity-icon.png";
        private const string MoveEntityButtonIconDisabled = "/modules/synthesis_core/UI/images/move-entity-icon-disabled.png";

        private static VisualElement deleteEntityButton = null;
        private const string DeleteEntityButtonIcon = "/modules/synthesis_core/UI/images/delete-icon.png";
        private const string DeleteEntityButtonIconDisabled = "/modules/synthesis_core/UI/images/delete-icon-disabled.png";

        private static VisualElement editJointsButton = null;
        private const string EditJointsButtonIcon = "/modules/synthesis_core/UI/images/joint-icon.png";
        private const string EditJointsButtonIconDisabled = "/modules/synthesis_core/UI/images/joint-icon-disabled.png";

        public static void CreateToolbar()
        {
            if (toolbarCreated)
                return;

            entityTab = new Tab("Entity", Ui.ToolbarAsset, toolbarElement => {
                // Populate tabs of toolbar
                var modifyCategory = ToolbarTools.AddButtonCategory(toolbarElement, "MODIFY");
                moveEntityButton = ToolbarTools.AddButton(modifyCategory, "move-entity-button", "Move Entity", MoveEntityButtonIconDisabled,
                    _ => {
                        if (isEntitySelected)
                        {
                            if (!MoveArrows.IsMovingEntity)
                                MoveArrows.MoveEntity(Selectable.Selected.Entity.Value);
                            else
                                MoveArrows.StopMovingEntity();
                        }
                    });
                deleteEntityButton = ToolbarTools.AddButton(modifyCategory, "delete-entity-button", "Delete Entity", DeleteEntityButtonIconDisabled,
                    _ => {
                        if (isEntitySelected) {
                            var prev = Selectable.Selected;
                            EnvironmentManager.RemoveEntity(Selectable.Selected.Entity.Value);
                            Selectable.ResetSelected();
                            EventBus.Push(new Selectable.SelectionChangeEvent(null, prev));
                        }
                    });

                var jointCategory = ToolbarTools.AddButtonCategory(toolbarElement, "JOINTS");
                editJointsButton = ToolbarTools.AddButton(jointCategory, "joints-button", "Edit Joints", EditJointsButtonIconDisabled, 
                    _ => {
                        if (isEntitySelected)
                        {
                            openedJointEditor = !openedJointEditor;
                            if (openedJointEditor)
                                UIManager.ShowPanel("Joints");
                            else
                                UIManager.ClosePanel("Joints");
                        }
                    });

                isToolbarBound = true;
                UpdateIcons();
            });

            UIManager.AddTab(entityTab);

            JointsWindow.CreateWindow();
            UIManager.AddPanel(JointsWindow.Panel);

            toolbarCreated = true;
        }

        private static void UpdateIcons()
        {
            if (!toolbarCreated || !isToolbarBound)
                return;

            if (isEntitySelected)
            {
                moveEntityButton.Get(className: "toolbar-button-icon").SetStyleProperty("background-image", MoveEntityButtonIcon);
                deleteEntityButton.Get(className: "toolbar-button-icon").SetStyleProperty("background-image", DeleteEntityButtonIcon);
                editJointsButton.Get(className: "toolbar-button-icon").SetStyleProperty("background-image", EditJointsButtonIcon);
            }
            else
            {
                moveEntityButton.Get(className: "toolbar-button-icon").SetStyleProperty("background-image", MoveEntityButtonIconDisabled);
                deleteEntityButton.Get(className: "toolbar-button-icon").SetStyleProperty("background-image", DeleteEntityButtonIconDisabled);
                editJointsButton.Get(className: "toolbar-button-icon").SetStyleProperty("background-image", EditJointsButtonIconDisabled);
            }
        }

        public class EntityToolbarCallbacks
        {
            [Callback]
            public void OnSelectionChange(Selectable.SelectionChangeEvent _)
            {
                if (!toolbarCreated || !isToolbarBound)
                    return;

                UpdateIcons();

                if (isEntitySelected)
                {

                }
                else
                {
                    if (openedJointEditor)
                    {
                        JointsWindow.OnWindowClose();
                        UIManager.ClosePanel("Joints");
                        openedJointEditor = false;
                    }
                    if (openedMoveArrows)
                    {
                        MoveArrows.StopMovingEntity();
                        openedMoveArrows = false;
                    }
                }
            }
        }
    }
}
