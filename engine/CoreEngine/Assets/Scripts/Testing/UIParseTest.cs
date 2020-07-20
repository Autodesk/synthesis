﻿using SynthesisAPI.Modules;
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
using Synthesis.Util;
using SynthesisAPI.AssetManager;
using SynthesisAPI.UIManager.UIComponents;
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
    public SynListView generated;
    public SynVisualElementAsset entry;

    public UnityWebRequest request;

    public TextureAsset asset;
    public Texture2D texture;

    private void Start()
    {
        ApiProvider.Log("Testing");

        Panel testTab = new Panel("Test Panel",
            AssetManager.Import<SynVisualElementAsset>("text/uxml", false, "/temp", "test-panel.uxml",
                Permissions.PublicReadWrite, $"test{Path.DirectorySeparatorChar}Modules.uxml"),
            sve => { });
        UIManager.AddPanel(testTab);
        UIManager.ShowPanel("Test Panel");

        ApiProvider.Log("=====");
        UIManager.RecursivePrint(PanelRenderer.visualTree);
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
