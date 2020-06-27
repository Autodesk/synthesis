using SynthesisAPI.Modules;
using SynthesisAPI.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockApi
{
    public class ApiInstance : IApiProvider
    {
        public Component AddComponent(Type t, Guid objectId)
        {
            throw new NotImplementedException();
        }

        public TComponent AddComponent<TComponent>(Guid objectId) where TComponent : Component
        {
            throw new NotImplementedException();
        }

        public Component GetComponent(Type t, Guid id)
        {
            throw new NotImplementedException();
        }

        public TComponent GetComponent<TComponent>(Guid id) where TComponent : Component
        {
            throw new NotImplementedException();
        }

        public List<Component> GetComponents(Guid objectId)
        {
            throw new NotImplementedException();
        }

        public List<TComponent> GetComponents<TComponent>(Guid id) where TComponent : Component
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

        public global::UnityEngine.Transform GetTransformById(Guid id)
        {
            throw new NotImplementedException();
        }

        public (Guid Id, bool valid) Instantiate(SynthesisAPI.Modules.Object o)
        {
            throw new NotImplementedException();
        }

        public TUnityType InstantiateFocusable<TUnityType>() where TUnityType : global::UnityEngine.UIElements.Focusable
        {
            throw new NotImplementedException();
        }

        public void Log(object o)
        {
            throw new NotImplementedException();
        }

        public void RegisterModule(IModule module)
        {
            throw new NotImplementedException();
        }
    }
}
