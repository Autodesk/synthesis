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
#if UNITY_EDITOR
    public StyleSheet styleSheet;
    public PanelRenderer renderer;
    public VisualElement generated;

    public UnityWebRequest request;

    public TextureAsset asset;
    public Texture2D texture;

    void Start()
    {
        asset = AssetManager.Import<TextureAsset>("image/texture", "/temp", "test.jpeg",
            Permissions.PublicReadWrite, $"test{Path.DirectorySeparatorChar}test.jpeg");
        
        XmlDocument doc = new XmlDocument();
        doc.Load($"Assets{Path.DirectorySeparatorChar}TestButton.uxml");
        generated = UIParser.CreateVisualElement("Test_UI", doc);
        renderer.postUxmlReload += () =>
        {
            renderer.visualTree.Q(name = "screen").Add(generated);
            return null;
        };
    }
#endif
}
