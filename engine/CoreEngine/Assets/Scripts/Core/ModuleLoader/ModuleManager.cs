using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using SynthesisAPI;

namespace Synthesis.Core.Module {

  public class ModuleManager : MonoBehaviour {

    // EngineGateway

    private List<IModule> loadedModules = new List<IModule>();

    private void Awake() {
      EngineGateway.AttachHandle(new EngineHandle());

      Type[] ts = Assembly.LoadFrom(@"F:\hunte\source\repos\synth-2020\engine\TestModule\bin\Debug\netstandard2.0\TestModule.dll").GetTypes();
      foreach (Type t in ts) {
        if (Array.Exists(t.GetInterfaces(), x => x == typeof(IModule))) {
          IModule module = (IModule)Activator.CreateInstance(t);
          loadedModules.Add(module);
        }
      }
    }

    private void Start() {
      loadedModules.ForEach(x => x.OnStart());
    }

    private void Update() {
      loadedModules.ForEach(x => x.OnUpdate());
    }

  }

}