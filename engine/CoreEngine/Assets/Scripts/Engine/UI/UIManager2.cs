using System.Collections;
using System.Collections.Generic;
using Unity.UIElements.Runtime;
using UnityEngine;
using UnityEngine.UIElements;

public class UIManager2 : MonoBehaviour
{
    public PanelRenderer SynthesisToolbar;

    private Dictionary<string, Workspace> Workspaces;

    public UIManager2()
    {
        this.Workspaces = new Dictionary<string, Workspace>();
        
        // try putting this under Start if it doesn't work
        LoadModuleWorkspaces();
        AddModuleTabs();
    }

    private void LoadModuleWorkspaces()
    {
        // in the future will loop through a specific Modules folder/directory, for now just using test data
        // for ( ... in ... )
        //   Workspaces.Add (...)
        Workspaces.Add("Falcon", new Workspace("Falcon"));
    }

    private void AddModuleTabs()
    {
        foreach (string moduleName in Workspaces.Keys)
        {
            // addTab(moduleName)
        }
    }

    public void AddTab(string moduleName)
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

        return null;
    }

}
