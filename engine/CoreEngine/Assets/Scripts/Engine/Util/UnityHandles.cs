using System;
using UnityEngine;
using Synthesis.Simulator;

namespace Synthesis.Util
{
    public class UnityHandles : MonoBehaviour
    {
        public MeshRenderer PlaneMeshRenderer;

        private static UnityHandles instance = null;

        public delegate void CallBack();
        public static event CallBack OnUpdate = () => { };
        public static event CallBack OnFixedUpdate = () => { };
        public static event CallBack OnLateUpdate = () => { };

        void Start()
        {
            _ = SimulatorHandler.Instance;
            // DontDestroyOnLoad(gameObject);
        }

        void Update()
        {
            OnUpdate();
        }
        void FixedUpdate()
        {
            OnFixedUpdate();
        }
        void LateUpdate()
        {
            OnLateUpdate();
        }

        public static UnityHandles Instance { 
            get {
                if (instance == null) instance = FindObjectOfType<UnityHandles>();
                return instance;
            } 
        }
    }
}