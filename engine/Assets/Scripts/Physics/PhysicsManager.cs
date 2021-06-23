using UnityEngine;

namespace Synthesis.Physics
{
    /// <summary>
    /// Controls physics within unity
    /// </summary>
    public class PhysicsManager : MonoBehaviour
    {
        private static float StepRate;

        [SerializeField]
        public static bool IsPaused { get; private set; } = false;
        [SerializeField]
        public static bool IsFast { get; private set; } = false;
        [SerializeField]
        public static bool IsSlow { get; private set; } = false;

        // Use this for initialization
        void Awake()
        {
            
        }

        void OnEnable() {
            UnityEngine.Physics.autoSimulation = false;
        }

        // Update is called once per frame
        void Update()
        {
            if (IsSlow)
                StepRate = Time.fixedDeltaTime / 2;
            else if (IsFast)
                StepRate = Time.fixedDeltaTime * 2;
            else
                StepRate = Time.fixedDeltaTime;
            if(!IsPaused) StepForward();
        }

        public static void Pause()
        {
            IsPaused = true;
        }

        public static void Play()
        {
            IsPaused = false;
        }

        public static void TogglePlay()
        {
            IsPaused = !IsPaused;
        }
        public static void StepForward()
        {
            UnityEngine.Physics.Simulate(StepRate);
        }
        public static void StepBackward()
        {
            //TODO: save position and rotation of objects each frame and then lerp backward
        }
        public static void SpeedUp()
        {
            IsSlow = false;
            IsFast = !IsFast;
        }
        public static void SlowDown()
        {
            IsFast = false;
            IsSlow = !IsSlow;
        }
    }
}