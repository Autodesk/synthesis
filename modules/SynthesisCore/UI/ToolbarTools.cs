using SynthesisAPI.EventBus;
using SynthesisAPI.UIManager.VisualElements;
using System;

namespace SynthesisCore.UI
{
    public static class ToolbarTools
    {
        public static VisualElement AddButtonCategory(VisualElement toolbarElement, string label)
        {
            var category = Ui.ToolbarCategoryAsset.GetElement($"toolbar-category-{label}");
            toolbarElement.Get(className: "toolbar").Add(category);

            var labelElement = (Label)category.Get(className: "toolbar-category-label");
            labelElement.Text = label;
            return category.Get(className: "toolbar-category-inner");
        }

        public static void AddButton(VisualElement toolbarCategory, string buttonName, string tooltipText, string iconPath, Action<IEvent> callback)
        {
            var buttonContainer = Ui.ToolbarButtonAsset.GetElement(buttonName);
            toolbarCategory.Get(className: "toolbar-category-inner").Add(buttonContainer);
            var button = (Button)buttonContainer.Get(className: "toolbar-button");
            button.Name = buttonName;
            button.Subscribe(e => callback(e));
            var iconContainer = button.Get(className: "toolbar-button-icon");
            iconContainer.SetStyleProperty("background-image", iconPath);

            buttonContainer.Tooltip = tooltipText;
        }
    }
}
