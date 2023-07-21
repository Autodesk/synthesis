using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

public static class SynthesisTween {
    private static SynthesisTweenComponent _tweenComp;

    private static Dictionary<string, SynthesisTweenConfig> _tweens = new Dictionary<string, SynthesisTweenConfig>();

    private static void EnsureComponent() {
        if (_tweenComp != null)
            return;

        _tweenComp = new GameObject("TweenComp").AddComponent<SynthesisTweenComponent>();
        Object.DontDestroyOnLoad(_tweenComp.gameObject);
    }

    public static void MakeTween(string key, object start, object end, float duration,
        Func<float, object, object, object> interpolateFunc, Func<float, float> scalingFunc,
        Action<SynthesisTweenStatus> callback) {
        EnsureComponent();

        _tweens.Add(key, new SynthesisTweenConfig { Key = key, Start = start, End = end, Duration = duration,
            StartTime = Time.realtimeSinceStartup, Interpolation = interpolateFunc, Scaling = scalingFunc,
            Callback = callback });
    }

    public static void CancelTween(string key) {
        _tweens.Remove(key);
    }

    private class SynthesisTweenComponent : MonoBehaviour {
        private void Update() {
            string[] keys = new string[_tweens.Count];
            _tweens.Keys.CopyTo(keys, 0);
            foreach (var key in keys) {
                var config   = _tweens[key];
                var progress = Mathf.Clamp((Time.realtimeSinceStartup - config.StartTime) / config.Duration, 0f, 1f);
                var val      = config.Interpolation(config.Scaling(progress), config.Start, config.End);
                config.Callback(new SynthesisTweenStatus(val, progress));
                if (progress >= 1f) {
                    _tweens.Remove(key);
                }
            }
        }
    }

    private struct SynthesisTweenConfig {
        public string Key;
        public object Start;
        public object End;
        public float Duration;
        public float StartTime;
        public Func<float, object, object, object> Interpolation;
        public Func<float, float> Scaling;
        public Action<SynthesisTweenStatus> Callback;
    }

    public struct SynthesisTweenStatus {
        public T CurrentValue<T>() => (T) _currentValue;

        private object _currentValue;
        public float CurrentProgress;

        public SynthesisTweenStatus(object val, float prog) {
            _currentValue   = val;
            CurrentProgress = prog;
        }
    }
}

public static class SynthesisTweenScaleFunctions {
    public static Func<float, float> EaseOutQuad = x => -(x * x) + 2 * x;
    public static Func<float, float> EaseOutCubic = x => x * x * x - 3 * x * x + 3 * x;
}

public static class SynthesisTweenInterpolationFunctions {
    public static Func<float, float, float, float> FloatInterp = (t, a, b) => (1 - t) * a + t * b;
}
