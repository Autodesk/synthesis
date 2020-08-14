using UnityEngine;
using SynthesisAPI.EnvironmentManager.Components;
using SynthesisAPI.EnvironmentManager;
using static Engine.ModuleLoader.Api;

namespace Engine.ModuleLoader.Adapters
{
    public class ParentAdapter : MonoBehaviour, IApiAdapter<Parent>
    {
        private Parent instance;

        public void Awake() { }

        public void Update()
        {
            if (instance.Changed)
            {
                GameObject parent = (Entity)instance == 0 ? ApiProviderData.EntityParent : ApiProviderData.GameObjects[instance];
                gameObject.transform.SetParent(parent.transform);
                instance.ProcessedChanges();

                var transform = instance.Entity?.GetComponent<SynthesisAPI.EnvironmentManager.Components.Transform>();
                if (transform != null)
                {
                    transform.Changed = true;
                }
            }
        }

        public void OnDestroy()
        {
            instance.Entity.Value.RemoveEntity();
        }

        public void SetInstance(Parent parent)
        {
            instance = parent;
        }

        public static Parent NewInstance()
        {
            return new Parent();
        }
    }
}
