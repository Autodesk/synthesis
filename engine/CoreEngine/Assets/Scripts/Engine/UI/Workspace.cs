using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEngine.VFX;

public class Workspace
{
    public string ModuleName { get; set; }

    private VisualTreeAsset Pane;
    
    public Workspace(string moduleName)
    {
        this.ModuleName = moduleName;

        this.Pane = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Scripts/Testing/Modules/" + ModuleName + "/ui/Pane.uxml");

    }

}
