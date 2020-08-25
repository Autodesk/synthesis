using SynthesisAPI.AssetManager;
using SynthesisAPI.UIManager.VisualElements;
using SynthesisAPI.Utilities;

namespace SynthesisCore.UI.Windows
{
    public class EnvironmentItem
    {
        public VisualElement EnvironmentElement { get; }

        public EnvironmentItem(VisualElementAsset environmentAsset, FileInfo fileInfo)
        {
            EnvironmentElement = environmentAsset.GetElement("environment");

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
                EnvironmentElement.RemoveFromHierarchy();
                // delete from registered environments (to come)
            });
            
            // check mark button to set Environment
            setButton.Subscribe(x =>
            {
                Logger.Log("Set environment");
                // implementation to set environment from file reference (to come)
            });
        }

    }

}