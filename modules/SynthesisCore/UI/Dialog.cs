using SynthesisAPI.AssetManager;
using SynthesisAPI.UIManager;
using SynthesisAPI.UIManager.VisualElements;

namespace SynthesisCore.UI
{
    public class Dialog
    {
        private static VisualElementAsset dialogAsset =
            AssetManager.GetAsset<VisualElementAsset>("/modules/synthesis_core/UI/uxml/Dialog.uxml");
        
        public static void SendDialog(DialogInfo dialogInfo)
        {
            VisualElement dialogElement = dialogAsset.GetElement("dialog");
            
            Label titleLabel = (Label) dialogElement.Get("title");
            titleLabel.Text = titleLabel.Text
                .Replace("%title%", dialogInfo.Title);

            Label promptLabel = (Label) dialogElement.Get("prompt");
            promptLabel.Text = promptLabel.Text
                .Replace("%prompt%", dialogInfo.Prompt);

            Label descriptionLabel = (Label) dialogElement.Get("description");
            descriptionLabel.Text = descriptionLabel.Text
                .Replace("%description%", dialogInfo.Description);

            Button submitButton = (Button) dialogElement.Get("submit-button");
            submitButton.Text = submitButton.Text
                .Replace("%blue%", dialogInfo.SubmitButtonText);

            Button closeButton = (Button) dialogElement.Get("close-button");
            closeButton.Text = closeButton.Text
                .Replace("%white%", dialogInfo.CloseButtonText);
            
            submitButton.Subscribe(e =>
            {
                dialogInfo.SubmitButtonAction?.Invoke(e);
                dialogElement.RemoveFromHierarchy();
            });
            
            closeButton.Subscribe(e =>
            {
                dialogInfo.CloseButtonAction?.Invoke(e);
                dialogElement.RemoveFromHierarchy();
            });
            
            dialogElement.SetStyleProperty("position", "absolute");
            dialogElement.SetStyleProperty("left", "50%");
            dialogElement.SetStyleProperty("margin-left", "-200px"); // should be set to negative half of dialog width
            dialogElement.SetStyleProperty("top", "50%");
            dialogElement.SetStyleProperty("margin-top", "-100px"); // should be set to negative half of dialog height

            UIManager.RootElement.Get("dialog-free-roam").Add(dialogElement);
        }
    }
}