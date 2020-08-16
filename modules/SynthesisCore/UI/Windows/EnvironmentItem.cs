using SynthesisAPI.UIManager.VisualElements;
using SynthesisAPI.Utilities;

namespace SynthesisCore.UI
{
    public class EnvironmentItem
    {
        public VisualElement EnvironmentElement { get; }

        public EnvironmentItem(VisualElement environmentElement, FileInfo fileInfo)
        {
            EnvironmentElement = environmentElement;

            SetInformation(fileInfo);
            RegisterButtons();
        }

        private void SetInformation(FileInfo fileInfo)
        {
            var nameLabel = (Label) EnvironmentElement.Get("name");
            var lastModifiedLabel = (Label) EnvironmentElement.Get("last-modified-date");
            
            nameLabel.Text = nameLabel.Text.Replace("%name%", fileInfo.Name);
            lastModifiedLabel.Text = lastModifiedLabel.Text.Replace("%time%", fileInfo.LastModified);
        }
        
        private void RegisterButtons()
        {
            Button deleteButton = (Button) EnvironmentElement.Get("delete-button"); 
            Button setButton = (Button) EnvironmentElement.Get("set-button");

            // trash can button to delete Environment from imported list
            deleteButton.Subscribe(x =>
            {
                Logger.Log("Deleted Environment from list");
            });
            
            // check mark button to set Environment
            setButton.Subscribe(x =>
            {
                Logger.Log("Set environment");
                // implementation to set environment from file reference
            });
        }

    }

}