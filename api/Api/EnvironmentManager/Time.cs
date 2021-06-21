using UnityTime = UnityEngine.Time;

namespace SynthesisAPI.EnvironmentManager
{
    public static class Time
    {
        public static float TimeSinceStartup => UnityTime.fixedTime;
        public static float RealTimeSinceStartup => UnityTime.realtimeSinceStartup;
        public static float TimeScale
        {
            get => UnityTime.timeScale;
            set => UnityTime.timeScale = value;
        }
        public static float TimeSinceLastFrameUpdate => UnityTime.deltaTime;
        public static float TimeSinceLastPhysicsUpdate => UnityTime.fixedDeltaTime;

        public static float TimeMilliseconds => (UnityTime.time);
    }
}
