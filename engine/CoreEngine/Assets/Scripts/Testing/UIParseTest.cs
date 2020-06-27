using SynthesisAPI.Modules;
using SynthesisAPI.Runtime;
using SynthesisAPI.UIManager;
using SynthesisAPI.UIManager.VisualElements;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using UnityEngine.UIElements;

public class UIParseTest : MonoBehaviour
{
    void Awake()
    {
        ApiProvider.RegisterApiProvider(new ApiInstance());
    }

    void Start()
    {
        UnityEngine.UIElements.VisualElement element = new UnityEngine.UIElements.VisualElement();
        var property = typeof(IStyle).GetProperty("height");
        StyleLength len = UIParser.ToStyleLength(" 100%"); // Units => Percent
        property.SetValue(element.style, len);
        Debug.Log(element.style.height.value.unit); // Units => Pixels
        /*XmlDocument testDoc = new XmlDocument();
        testDoc.LoadXml("<Label text=\"Label\" name=\"title\" style=\"height: 100%; margin-left: 10px; margin-right: 10px; -unity-text-align: middle-center; -unity-font-style: bold; font-size: 22px;\" />");
        dynamic element = UIParser.CreateVisualElement(testDoc.FirstChild);
        Debug.Log(element.GetType());*/
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
        dynamic a = (TUnityType) Activator.CreateInstance(typeof(TUnityType));
        Debug.Log(a.style.GetType().FullName);
        return a;
    }

    public void Log(object o)
    {
        Debug.Log(o);
    }

    public void RegisterModule(IModule module)
    {
        throw new NotImplementedException();
    }
}
