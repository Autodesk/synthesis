using SynthesisAPI.AssetManager;
using SynthesisAPI.Modules;
using SynthesisAPI.UIManager.VisualElements;

namespace SynthesisCore.UI.Windows
{
    public class ModuleItem
    {
        public VisualElement ModuleElement { get; }

        public ModuleItem(VisualElementAsset moduleAsset, ModuleManager.ModuleInfo moduleInfo)
        {
            ModuleElement = moduleAsset.GetElement("module");
            
            SetInformation(moduleInfo);
            RegisterButtons();
        }
        
        private void SetInformation(ModuleManager.ModuleInfo moduleInfo)
        {
            var titleLabel = (Label) ModuleElement.Get("title");
            var authorLabel = (Label) ModuleElement.Get("author");
            var descriptionLabel = (Label) ModuleElement.Get("description");

            titleLabel.Text = titleLabel.Text
                .Replace("%name%", moduleInfo.Name)
                .Replace("%version%", moduleInfo.Version);

            authorLabel.Text = authorLabel.Text.Replace("%author%",
                string.IsNullOrEmpty(moduleInfo.Author) ? "Unknown" : moduleInfo.Author);
            descriptionLabel.Text = descriptionLabel.Text.Replace("%description%",
                string.IsNullOrEmpty(moduleInfo.Description) ? "No description" : moduleInfo.Description);
        }
        
        private void RegisterButtons()
        {
            
        }

    }
}