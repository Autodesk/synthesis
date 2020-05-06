using SynthesisAPI;
using Synthesis.Core.Object;

namespace Synthesis.Core.Module {

  public class EngineHandle : IEngineHandle {

    public void CreateObject(string name) => ObjectLoader.Instance.CreateObj(name);

  }

}