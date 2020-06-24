using System.Collections;
using System.Collections.Generic;
using Unity.UIElements.Runtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class NewUIManager : MonoBehaviour
{
    public PanelRenderer SynthesisToolbar;
    private VisualElement SynthesisToolbarTree;
    private VisualElement SynthesisTabContainer;

    private Dictionary<string, Button> Tabs = new Dictionary<string, Button>();

    public void AddTab(string tabName)
    {
        if (!Tabs.ContainsKey(tabName))
        {
            SynthesisToolbarTree = SynthesisToolbar.visualTree;
            SynthesisTabContainer = SynthesisToolbarTree.Q<VisualElement>(name: "tab-panel");

            Button CustomTab = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI/Toolbar/Toolbar.uxml").CloneTree().Q<Button>(name: "default-tab");

            CustomTab.name = tabName;
            CustomTab.text = tabName;
            SynthesisTabContainer.Add(CustomTab);

            Tabs.Add(tabName, CustomTab);
        } else
        {
            Debug.LogError("Tab failed to be added because a tab with that name already exists");
        }

    }

    public void RemoveTab(string tabName)
    {
        if (Tabs.ContainsKey(tabName))
        {
            if (Tabs.Count > 1)
            {
                SynthesisTabContainer = SynthesisToolbarTree.Q<VisualElement>(name: "tab-panel");

                SynthesisTabContainer.Remove(Tabs[tabName]);
                Tabs.Remove(tabName);
            } else
            {
                Debug.LogError("Tab failed to be removed because at least 1 tab must be present");
            }
        } else
        {
            Debug.LogError("Removal of tab [" + tabName + "] failed because no such tab exists");
        }
    }

    public void DisplayPane(string ModuleName)
    {

    }

    // Start is called before the first frame update
    void Start()
    {
        SynthesisToolbar.postUxmlReload += Bind;
    }

    // Update is called once per frame
    void Update()
    {

    }

    IEnumerable<Object> Bind()
    {
        // this has to be kept here for some reason otherwise adding tabs doesn't work
        AddTab("Test1");

        //foreach (string moduleName in Tabs.Keys)
        //{
        //    Button ClickableTab = SynthesisToolbarTree.Q<Button>(name: moduleName);
        //    ClickableTab.clickable.clicked += () =>
        //    {
        //        DisplayPane(moduleName);
        //    };
        //}


        // hard coded buttons to be removed later
        Button test1Button = SynthesisToolbarTree.Q<Button>(name: "Test1");
        test1Button.clickable.clicked += () =>
        {
            SynthesisToolbarTree.Q<VisualElement>(name: "workspace-panel").style.backgroundColor = Color.blue;
        };

        Button addButton = SynthesisToolbarTree.Q<Button>(name: "add-button");
        addButton.clickable.clicked += () =>
        {
            AddTab("New tab");
        };

        Button removeButton = SynthesisToolbarTree.Q<Button>(name: "remove-button");
        removeButton.clickable.clicked += () =>
        {
            RemoveTab("New tab");
        };

        return null;
    }

}
