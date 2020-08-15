using SynthesisAPI.AssetManager;
using SynthesisAPI.UIManager;
using SynthesisAPI.UIManager.UIComponents;
using SynthesisAPI.UIManager.VisualElements;
using SynthesisAPI.Utilities;

namespace SynthesisCore.UI
{
    public class EngineToolbar
    {
        private static bool toolbarCreated = false;

        public static void CreateToolbar()
        {
            if (toolbarCreated)
                return;

            var environmentAsset = AssetManager.GetAsset<VisualElementAsset>("/modules/synthesis_core/UI/uxml/Environment.uxml");
            var environmentsAsset = AssetManager.GetAsset<VisualElementAsset>("/modules/synthesis_core/UI/uxml/Environments.uxml");
            var entityAsset = AssetManager.GetAsset<VisualElementAsset>("/modules/synthesis_core/UI/uxml/Entity.uxml");
            var entitiesAsset = AssetManager.GetAsset<VisualElementAsset>("/modules/synthesis_core/UI/uxml/Entities.uxml");

            Panel entitiesWindow = new Panel("Entities", entitiesAsset,
                element =>
                {
                    Utilities.RegisterOKCloseButtons(element, "Entities");
                    AddEntityToList(entityAsset.GetElement("entity"), element, "Test-Entity", "10d");
                });
            
            UIManager.AddPanel(entitiesWindow);
            
            Panel environmentsWindow = new Panel("Environments", environmentsAsset,
                element =>
                {
                    Utilities.RegisterOKCloseButtons(element, "Environments");
                    AddEnvironmentToList(environmentAsset.GetElement("environment"), element, "Test-Environment", "3d");
                });
            
            UIManager.AddPanel(environmentsWindow);
            
            var engineTab = new Tab("Engine", Ui.ToolbarAsset, toolbarElement => {
                var designCategory = ToolbarTools.AddButtonCategory(toolbarElement, "ENTITIES");
                ToolbarTools.AddButton(designCategory, "add-entity-button", "/modules/synthesis_core/UI/images/add-icon.png",
                    _ => UIManager.TogglePanel("Entities"));
                ToolbarTools.AddButton(designCategory, "change-environment-button", "/modules/synthesis_core/UI/images/environments-icon.png",
                    _ => UIManager.TogglePanel("Environments"));
            });
            
            UIManager.AddTab(engineTab);
            UIManager.SetDefaultTab(engineTab.Name);
            
            toolbarCreated = true;
        }
        
        private void LoadEntitiesWindowContent(VisualElement entitiesWindow)
        {
            var entityAsset = AssetManager.GetAsset<VisualElementAsset>("/modules/synthesis_core/UI/uxml/Entity.uxml");

            // foreach (/* loop through files stored for entities, waiting on File Browser */)
            // {
            //     VisualElement entityElement = entityAsset.GetElement("entity");
            //
            //     Label nameLabel = (Label) entityElement.Get("name");
            //     Label lastModifiedLabel = (Label) entityElement.Get("last-modified-date");
            //
            //     nameLabel.Text = nameLabel.Text.Replace("%name%", /* Get file name */);
            //     lastModifiedLabel.Text = lastModifiedLabel.Text.Replace("%time%", /* Get last modified */);
            //
            //     ListView entityList = (ListView) entitiesWindow.Get("entity-list");
            //     entityList.Add(entityList);
            // }
        }
        
        private void LoadEnvironmentsWindowContent(VisualElement environmentsWindow)
        {
            
        }

        private static void AddEntityToList(VisualElement entityElement, VisualElement entitiesWindow, string name, string lastModified)
        {
            Label nameLabel = (Label) entityElement?.Get("name");
            Label lastModifiedLabel = (Label) entityElement?.Get("last-modified-date");
            
            nameLabel.Text = nameLabel.Text.Replace("%name%", name);
            lastModifiedLabel.Text = lastModifiedLabel.Text.Replace("%time%", lastModified);
            
            ListView entityList = (ListView) entitiesWindow.Get("entity-list");
            entityList.Add(entityElement);
        }

        private static void AddEnvironmentToList(VisualElement environmentElement, VisualElement environmentsWindow, string name, string lastModified)
        {
            Label nameLabel = (Label) environmentElement.Get("name");
            Label lastModifiedLabel = (Label) environmentElement.Get("last-modified-date");
            
            nameLabel.Text = nameLabel.Text.Replace("%name%", name);
            lastModifiedLabel.Text = lastModifiedLabel.Text.Replace("%time%", lastModified);
            
            ListView environmentList = (ListView) environmentsWindow.Get("environment-list");
            environmentList.Add(environmentElement);
        }
        
    }

}
