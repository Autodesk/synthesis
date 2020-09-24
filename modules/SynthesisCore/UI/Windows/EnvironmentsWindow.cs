using SynthesisAPI.AssetManager;
using SynthesisAPI.UIManager;
using SynthesisAPI.UIManager.UIComponents;
using SynthesisAPI.UIManager.VisualElements;
using SynthesisAPI.Utilities;
using System;
using System.IO;

namespace SynthesisCore.UI.Windows
{
    public class EnvironmentsWindow
    {
        public Panel Panel { get; }
        private VisualElement Window;
        private VisualElementAsset EnvironmentAsset;
        private ListView EnvironmentList;

        public EnvironmentsWindow()
        {
            EnvironmentAsset = AssetManager.GetAsset<VisualElementAsset>("/modules/synthesis_core/UI/uxml/Environment.uxml");

            var environmentsAsset = AssetManager.GetAsset<VisualElementAsset>("/modules/synthesis_core/UI/uxml/Environments.uxml");
            Panel = new Panel("Environments", environmentsAsset, OnWindowOpen);
        }
        
        private void OnWindowOpen(VisualElement environmentsWindow)
        {
            Window = environmentsWindow;
            Window.SetStyleProperty("position", "absolute");
            Window.IsDraggable = true;
            
            EnvironmentList = (ListView) Window.Get("environment-list");
            
            LoadWindowContents();   
            RegisterButtons();
        }

        private void LoadWindowContents()
        {
            // concrete implementation waiting on file browser, currently just using this stub code to show functionality
            for (int i = 0; i < 20; i++)
            {
                EnvironmentList.Add(new EnvironmentItem(EnvironmentAsset, new FileInfo("2019 Destination: Deep Space Field", "3d")).EnvironmentElement);
            }
        }

        private void RegisterButtons()
        {
            string Environments = "Environments";

            Button importButton = (Button) Window.Get("import-button");
            importButton?.Subscribe(x =>
            {
                try
                {
                    Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + Path.DirectorySeparatorChar + "Autodesk" + Path.DirectorySeparatorChar + "Synthesis" + Path.DirectorySeparatorChar + Environments);
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
                    {
                        FileName = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + Path.DirectorySeparatorChar + "Autodesk" + Path.DirectorySeparatorChar + "Synthesis" + Path.DirectorySeparatorChar + Environments,
                        UseShellExecute = true,
                        Verb = "open"
                    });
                }
                catch(Exception)
                {
                    Logger.Log(Environments + " directory not found", LogLevel.Warning);
                }
            });
            
            Button okButton = (Button) Window.Get("ok-button");
            okButton?.Subscribe(x => UIManager.ClosePanel(Panel.Name));

            Button closeButton = (Button) Window.Get("close-button");
            closeButton?.Subscribe(x => UIManager.ClosePanel(Panel.Name));
        }
        
    }

}