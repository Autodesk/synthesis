using UnityEngine;
using SynthesisAPI.EnvironmentManager.Components;
using SynthesisAPI.EnvironmentManager;
using static Engine.ModuleLoader.Api;

namespace Engine.ModuleLoader.Adapters {
    public class ParentAdapter : MonoBehaviour, IApiAdapter<Parent> {
        private Parent instance;

        public void SetInstance(Parent parent) {
            instance = parent;

            instance.PropertyChanged += (s, e) => {
                if (e.PropertyName == "ParentEntity") {
                    GameObject _parent =
                        (Entity)instance == 0 ? ApiProviderData.EntityParent : ApiProviderData.GameObjects[instance];
                    gameObject.transform.SetParent(_parent.transform);
                }
            };
        }

        public void OnDestroy() {
            instance.Entity.Value.RemoveEntity();
        }

        public static Parent NewInstance() {
            return new Parent();
        }
    }
}
