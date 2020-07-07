using SynthesisAPI.Modules;
using SynthesisAPI.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SynthesisAPI.EnvironmentManager;
using UnityEngine.UIElements;

#nullable enable

namespace MockApi
{
    public class ApiInstance : IApiProvider
    {
        public void Log(object o)
        {
            throw new NotImplementedException();
        }

        public uint AddEntity()
        {
            throw new NotImplementedException();
        }

        public Component AddComponent(Type t, uint entity)
        {
            throw new NotImplementedException();
        }

        public TComponent AddComponent<TComponent>(uint entity) where TComponent : Component
        {
            throw new NotImplementedException();
        }

        public Component GetComponent(Type t, uint entity)
        {
            throw new NotImplementedException();
        }

        public TComponent GetComponent<TComponent>(uint entity) where TComponent : Component
        {
            throw new NotImplementedException();
        }

        public List<Component> GetComponents(uint entity)
        {
            throw new NotImplementedException();
        }

        public T? CreateUnityType<T>(params object[] args) where T : class
        {
            throw new NotImplementedException();
        }

        public TUnityType? InstantiateFocusable<TUnityType>() where TUnityType : Focusable
        {
            throw new NotImplementedException();
        }

        public VisualElement? GetRootVisualElement()
        {
            throw new NotImplementedException();
        }
    }
}
