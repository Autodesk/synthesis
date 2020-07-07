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
    
    void Awake()
    {
        // ApiProvider.RegisterApiProvider(new ApiInstance());
    }

    void Start()
    {
        Debug.Log("Starting");

        // asset = AssetManager.Import<TextureAsset>("image/texture", "/temp", "test.jpeg",
            // Permissions.PublicReadWrite, $"test{Path.DirectorySeparatorChar}test.jpeg");
        
        XmlDocument doc = new XmlDocument();
        doc.Load($"Assets{Path.DirectorySeparatorChar}TestButton.uxml");
        generated = UIParser.CreateVisualElement("Test_UI", doc);
        renderer.postUxmlReload += () =>
        {
            renderer.visualTree.Q(name = "screen").Add(generated);
            return null;
        };
        // UIManager.AddVisualElement(generated);
        
        // RecursivePrint(generated);
    }

    private bool gate = false;

    private void Update()
    {
        if (!gate)
        {
            if (asset.TextureData != null)
            {
                gate = true;
                RecursivePrint(generated);
            }
        }
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

public class ApiInstance : IApiProvider
{
    public SynthesisAPI.Modules.Component AddComponent(Type t, Guid objectId)
    {
        throw new NotImplementedException();
    }

    public TComponent AddComponent<TComponent>(Guid objectId) where TComponent : SynthesisAPI.Modules.Component
    {
        throw new NotImplementedException();
    }

    public SynthesisAPI.Modules.Component GetComponent(Type t, Guid id)
    {
        throw new NotImplementedException();
    }

    public TComponent GetComponent<TComponent>(Guid id) where TComponent : SynthesisAPI.Modules.Component
    {
        throw new NotImplementedException();
    }

    public List<SynthesisAPI.Modules.Component> GetComponents(Guid objectId)
    {
        throw new NotImplementedException();
    }

    public List<TComponent> GetComponents<TComponent>(Guid id) where TComponent : SynthesisAPI.Modules.Component
    {
        throw new NotImplementedException();
    }

    public List<IModule> GetModules()
    {
        throw new NotImplementedException();
    }

    public SynthesisAPI.Modules.Object GetObject(Guid objectId)
    {
        throw new NotImplementedException();
    }

    public T CreateUnityType<T>(params object[] args)
    {
        return (T)Activator.CreateInstance(typeof(T), args);
    }

    public Transform GetTransformById(Guid id)
    {
        throw new NotImplementedException();
    }

    public (Guid Id, bool valid) Instantiate(SynthesisAPI.Modules.Object o)
    {
        throw new NotImplementedException();
    }

    public TUnityType InstantiateFocusable<TUnityType>() where TUnityType : Focusable
    {
        Debug.Log($"Creating instance of type: {typeof(TUnityType).FullName}");
        dynamic a = (TUnityType) Activator.CreateInstance(typeof(TUnityType));
        return a;
    }

    public VisualElement GetRootVisualElement()
    {
        PanelRenderer prr = GameObject.FindGameObjectWithTag("UI_RENDERER").GetComponent<PanelRenderer>();
        prr.RecreateUIFromUxml(); // Incase it hasn't loaded uxml data yet
        return prr.visualTree;
    }

    public void Log(object o)
    {
        // Debug.Log(o);
    }

    public void RegisterModule(IModule module)
    {
        throw new NotImplementedException();
    }
}
