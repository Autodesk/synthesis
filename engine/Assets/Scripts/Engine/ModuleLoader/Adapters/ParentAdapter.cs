using UnityEngine;
using System.Collections;
using SynthesisAPI.EnvironmentManager.Components;
using SynthesisAPI.EnvironmentManager;
using static Engine.ModuleLoader.Api;

namespace Engine.ModuleLoader.Adapters
{
    public class ParentAdapter : MonoBehaviour, IApiAdapter<Parent>
    {
        private Parent instance;

        // Use this for initialization
        void Awake()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (!((Entity)instance).EntityExists())
                SynthesisAPI.Runtime.ApiProvider.RemoveEntityFromScene(instance.Entity.Value);
            else if (instance.Changed)
            {
                GameObject parent = (Entity)instance == 0 ? ApiProviderData.EntityParent : ApiProviderData.GameObjects[instance];
                gameObject.transform.SetParent(parent.transform);
                instance.ProcessedChanges();
            }
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
