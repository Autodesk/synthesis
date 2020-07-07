using SynthesisAPI.Modules;
using SynthesisAPI.Runtime;
using SynthesisAPI.UIManager;
using SynthesisAPI.UIManager.VisualElements;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Xml;
using JetBrains.Annotations;
using SynthesisAPI.AssetManager;
using SynthesisAPI.VirtualFileSystem;
using Unity.UIElements.Runtime;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;

public class UIParseTest : MonoBehaviour
{
    public StyleSheet styleSheet;
    public PanelRenderer renderer;
    public VisualElement generated;

    public UnityWebRequest request;

    public TextureAsset asset;
    public Texture2D texture;

    void Start()
    {
        Debug.Log("Starting");

        
        asset = AssetManager.Import<TextureAsset>("image/texture", "/temp", "test.jpeg",
            Permissions.PublicReadWrite, $"test{Path.DirectorySeparatorChar}test.jpeg");
        
        XmlDocument doc = new XmlDocument();
        doc.Load($"Assets{Path.DirectorySeparatorChar}TestButton.uxml");
        generated = UIParser.CreateVisualElement("Test_UI", doc);
        renderer.postUxmlReload += () =>
        {
            renderer.visualTree.Q(name = "screen").Add(generated);
            RecursivePrint(generated);
            return null;
        };
        // UIManager.AddVisualElement(generated);
        
        
    }

    private void RecursivePrint(VisualElement e, int level = 0)
    {
        // e.styleSheets.Add(styleSheet);
        Debug.Log($"{level}: {e.name}, {e.GetType().Name}");
        foreach (VisualElement a in e.Children())
        {
            if (asset.TextureData != null)
                a.style.backgroundImage = new StyleBackground(asset.TextureData);
            else
                Debug.Log("Texture doesn't exist yet");
            
            RecursivePrint(a, level + 1);
        }
    }
}
