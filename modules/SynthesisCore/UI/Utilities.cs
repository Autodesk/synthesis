using SynthesisAPI.UIManager;
using SynthesisAPI.UIManager.VisualElements;

namespace SynthesisCore.UI
{
    public static class Utilities
    {
        public static void RegisterOKCloseButtons(VisualElement visualElement, string panelName)
        {
            Button okButton = (Button)visualElement.Get("ok-button");
            okButton?.Subscribe(x =>
            {
                UIManager.ClosePanel(panelName);
            });

            Button closeButton = (Button)visualElement.Get("close-button");
            closeButton?.Subscribe(x =>
            {
                UIManager.ClosePanel(panelName);
            });
        }
    }
}
