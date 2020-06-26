using System.Collections;
using System.Collections.Generic;
using Unity.UIElements.Runtime;
using UnityEngine;
using UnityEngine.UIElements;


public class TestModules : MonoBehaviour
{
    public PanelRenderer synthesisToolbar;
    public VisualTreeAsset synthesisToolbarTab;
    private VisualElement synthesisToolbarTree;

    private void ShowModulesScreen()
    {
        
    }
    
    // Start is called before the first frame update
    void Start()
    {
        synthesisToolbar.postUxmlReload += Bind;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerable<Object> Bind()
    {
        synthesisToolbarTree = synthesisToolbar.visualTree;
        
        Button modulesButton = synthesisToolbarTree.Q<Button>(name: "test-modules-button");
        modulesButton.clickable.clicked += () =>
        {
            ShowModulesScreen();
        };

        return null;
    }
    
}
