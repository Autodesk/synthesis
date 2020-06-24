using System.Collections;
using System.Collections.Generic;
using Unity.UIElements.Runtime;
using UnityEngine;
using UnityEngine.UIElements;

public class TestUI : MonoBehaviour
{
    public PanelRenderer panelRenderer;

    public VisualTreeAsset defaultTab;

    // Start is called before the first frame update
    void Start()
    {
        panelRenderer.postUxmlReload += Bind;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerable<Object> Bind()
    {
        var root = panelRenderer.visualTree;
        VisualElement tabContainer = root.Q<VisualElement>(name: "tab-panel");


        Button customTab = defaultTab.CloneTree().Q<Button>(name: "default-tab");
        customTab.text = "BLAH";

        tabContainer.Add(customTab);

        return null;
    }

}
