using SynthesisAPI.Runtime;
using SynthesisAPI.UIManager;
using SynthesisAPI.UIManager.VisualElements;
using System.Collections.Generic;
using SynthesisAPI.AssetManager;
using SynthesisAPI.UIManager.UIComponents;
using SynthesisAPI.Utilities;
using Unity.UIElements.Runtime;
using UnityEngine;
using UnityEngine.Networking;
// using UnityEngine.UIElements;
using SynVisualElementAsset = SynthesisAPI.AssetManager.VisualElementAsset;
using SynListView = SynthesisAPI.UIManager.VisualElements.ListView;
using Logger = SynthesisAPI.Utilities.Logger;

public class UIParseTest : MonoBehaviour
{
#if UNITY_EDITOR
    public new PanelRenderer renderer;
    public SynListView generated;
    public SynVisualElementAsset entry;

    public UnityWebRequest request;

    public SpriteAsset asset;
    public Texture2D texture;

    private void Start()
    {
        var selectionPanel = AssetManager.GetAsset<VisualElementAsset>("/modules/synthesis_core/SelectionList.uxml");
        Panel p = new Panel("SelectionPanel", selectionPanel, (ve) =>
        {
            var closeButton = ve.Get(name: "close-button") as Button;
            if (closeButton != null)
            {
                closeButton.Subscribe(e => UIManager.ClosePanel("SelectionPanel"));
            }

            var list = ve.Get(name: "selections") as ListView;
            if (list != null)
            {
                var teamNames = new List<string>() { "Mean Machine", "Spartan Robotics", "Shockwave" };
                list.Populate(teamNames,
                    () =>
                    {
                        var a = new Label();
                        a.SetStyleProperty("height", "30px");
                        return a;
                    },
                    (element, index) =>
                    {
                        (element as Label).Color = (1.0f, 1.0f, 1.0f, 1.0f);
                        (element as Label).Name = $"sel-{teamNames[index]}";
                        (element as Label).Text = teamNames[index];
                        // element.SetStyleProperty("width", "30px");
                    }
                );

                var selectButton = ve.Get(name: "select-button") as Button;
                if (selectButton != null)
                {
                    selectButton.Subscribe(
                        e => Logger.Log($"You've selected team \"{teamNames[list.SelectedIndex]}\"")
                    );
                }
            }
        });

        UIManager.AddPanel(p);
        UIManager.ShowPanel(p.Name);

        /*
        var a = AssetManager.Search("ToolbarTest.uxml");
        var asset = AssetManager.GetAsset<SynVisualElementAsset>("/modules/synthesis_core/ToolbarTest.uxml");
        
        Tab testTab = new Tab("Test Toolbar",
            AssetManager.GetAsset<SynVisualElementAsset>("/modules/synthesis_core/ToolbarTest.uxml"),
            sve => ApiProvider.Log("Running Tab Binding"));
        
        UIManager.AddTab(testTab);
        */
    }
#endif
}