using SynthesisAPI.AssetManager;
using SynthesisAPI.UIManager.VisualElements;
using SynthesisAPI.Utilities;

namespace SynthesisCore.UI
{
    public class EntityItem
    {
        public VisualElement EntityElement { get; }

        public EntityItem(VisualElementAsset entityAsset, FileInfo fileInfo)
        {
            EntityElement = entityAsset.GetElement("entity");
            
            SetInformation(fileInfo);
            RegisterButtons();
        }

        private void SetInformation(FileInfo fileInfo)
        {
            var nameLabel = (Label) EntityElement.Get("name");
            var lastModifiedLabel = (Label) EntityElement.Get("last-modified-date");
            
            nameLabel.Text = nameLabel.Text.Replace("%name%", fileInfo.Name);
            lastModifiedLabel.Text = lastModifiedLabel.Text.Replace("%time%", fileInfo.LastModified);
        }
        
        private void RegisterButtons()
        {
            Button deleteButton = (Button) EntityElement.Get("delete-button");
            Button spawnButton = (Button) EntityElement.Get("spawn-button");
            
            // trash can button to delete Environment from imported list
            deleteButton.Subscribe(x =>
            {
                EntityElement.RemoveFromHierarchy();
                // delete from registered entities (to come)
            });
            
            // check mark button to set Environment
            spawnButton.Subscribe(x =>
            {
                Logger.Log("Spawn entity");
                // implementation to add entity from file reference (to come)
            });
        }

    }
}