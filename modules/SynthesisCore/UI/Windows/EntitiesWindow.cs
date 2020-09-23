using SynthesisAPI.AssetManager;
using SynthesisAPI.UIManager;
using SynthesisAPI.UIManager.UIComponents;
using SynthesisAPI.UIManager.VisualElements;
using SynthesisAPI.Utilities;
using System;
using System.IO;

namespace SynthesisCore.UI.Windows
{
    public class EntitiesWindow
    {
        public Panel Panel { get; }
        private VisualElement Window;
        private VisualElementAsset EntityAsset;
        private ListView EntityList;

        public EntitiesWindow()
        {
            EntityAsset = AssetManager.GetAsset<VisualElementAsset>("/modules/synthesis_core/UI/uxml/Entity.uxml");

            var entitiesAsset = AssetManager.GetAsset<VisualElementAsset>("/modules/synthesis_core/UI/uxml/Entities.uxml");
            Panel = new Panel("Entities", entitiesAsset, OnWindowOpen);
        }

        private void OnWindowOpen(VisualElement entitiesWindow)
        {
            Window = entitiesWindow;
            Window.SetStyleProperty("position", "absolute");
            Window.IsDraggable = true;
            
            EntityList = (ListView) Window.Get("entity-list");
            
            LoadWindowContents();
            RegisterButtons();
        }
        
        private void LoadWindowContents()
        {
            // concrete implementation waiting on file browser, currently just using this stub code to show functionality
            for (int i = 0; i < 20; i++)
            {
                EntityList.Add(new EntityItem(EntityAsset, new FileInfo("Godspeed Robot Team 2374", "3d")).EntityElement);
            }
        }

        private void RegisterButtons()
        {
            string Entities = "Robots";

            Button importButton = (Button) Window.Get("import-button");
            importButton?.Subscribe(x =>
            {
                try
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
                    {
                        FileName = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + Path.DirectorySeparatorChar + "Autodesk" + Path.DirectorySeparatorChar + "Synthesis" + Path.DirectorySeparatorChar + Entities,
                        UseShellExecute = true,
                        Verb = "open"
                    });
                }
                catch(System.ComponentModel.Win32Exception)
                {
                    Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + Path.DirectorySeparatorChar + "Autodesk" + Path.DirectorySeparatorChar + "Synthesis" + Path.DirectorySeparatorChar + Entities);
                }
                catch(Exception)
                {
                    Logger.Log(Entities + " directory not found", LogLevel.Warning);
                }
            });
            
            Button okButton = (Button) Window.Get("ok-button");
            okButton?.Subscribe(x => UIManager.ClosePanel(Panel.Name));

            Button closeButton = (Button) Window.Get("close-button");
            closeButton?.Subscribe(x => UIManager.ClosePanel(Panel.Name));
        }
    }
}