using SynthesisAPI.UIManager;
using SynthesisAPI.AssetManager;
using SynthesisAPI.UIManager.UIComponents;
using Unity.UIElements.Runtime;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;
using SynVisualElementAsset = SynthesisAPI.AssetManager.VisualElementAsset;
using SynListView = SynthesisAPI.UIManager.VisualElements.ListView;
using Logger = SynthesisAPI.Utilities.Logger;

public class UIParseTest : MonoBehaviour
{
#if UNITY_EDITOR
    public StyleSheet styleSheet;
    public PanelRenderer renderer;
    public SynListView generated;
    public SynVisualElementAsset entry;

    public UnityWebRequest request;

    public SpriteAsset asset;
    public Texture2D texture;

    private void Start()
    {
        Logger.Log("Testing");

        // AssetManager.Import<TextureAsset>("image/texture", false, "/temp", "blank.png",
            // Permissions.PublicReadOnly, $"test{Path.DirectorySeparatorChar}Blank.png");

        /*Tab testTab = new Tab("Test Toolbar",
            AssetManager.Import<SynVisualElementAsset>("text/uxml", false, "/temp", "test-toolbar.uxml",
                Permissions.PublicReadWrite, $"test{Path.DirectorySeparatorChar}ToolbarTest.uxml"),
            sve => ApiProvider.Log("Running Tab Binding"));*/

        var a = AssetManager.Search("ToolbarTest.uxml");
        var asset = AssetManager.GetAsset<SynVisualElementAsset>("/modules/synthesis_core/ToolbarTest.uxml");
        
        Tab testTab = new Tab("Test Toolbar",
            AssetManager.GetAsset<SynVisualElementAsset>("/modules/synthesis_core/ToolbarTest.uxml"),
            sve => Logger.Log("Running Tab Binding"));
        
        UIManager.AddTab(testTab);
        // UIManager.ShowPanel("Test Panel");

        Logger.Log("=====");
        // UIManager.RecursivePrint(PanelRenderer.visualTree);
    }

    /*void Start()
    {
        asset = AssetManager.Import<TextureAsset>("image/texture", false, "/temp", "test.jpeg",
            Permissions.PublicReadWrite, $"test{Path.DirectorySeparatorChar}test.jpeg");

        entry = AssetManager.Import<SynVisualElementAsset>("text/uxml", false, "/temp", "test-entry.uxml",
            Permissions.PublicReadWrite, $"test{Path.DirectorySeparatorChar}test-entry.uxml");
        
        XmlDocument doc = new XmlDocument();
        doc.Load($"Assets{Path.DirectorySeparatorChar}UI{Path.DirectorySeparatorChar}TestListView.uxml");
        generated = ((VisualElement)UIParser.CreateVisualElement("Test_UI", doc).Get(name: "test-list-view")).GetSynVisualElement() as SynListView;
        // var doc2 = new XmlDocument();
        // doc.Load($"Assets{Path.DirectorySeparatorChar}UI{Path.DirectorySeparatorChar}TestListEntry.uxml");
        // entry = UIParser.CreateVisualElement("Test_Entry", doc);
        // ApiProvider.Log($"Type: {listView.GetType().FullName}");

        var sourceItem = new List<string>() {"Hi", "Hello"};
        generated.Populate(sourceItem, () => entry.GetElement("entry"),
            (element, index) =>
            {
                var label = (element.Get(name: "test-label") as SynLabel);
                label.Text = sourceItem[index];
            });
        
        renderer.postUxmlReload += () =>
        {
            // generated.
            PanelRenderer.visualTree.Q<VisualElement>(name: "screen").Add((ListView)generated);
            ApiProvider.Log("PostUxmlLoad");
            generated.PostUxmlLoad();
            return null;
        };
    }*/
#endif
}
