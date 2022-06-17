﻿/*
The MIT License (MIT)
Copyright (c) 2016 Digital Ruby, LLC
http://www.digitalruby.com
Created by Jeff Johnson

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

#if UNITY || UNITY_2017_4_OR_NEWER

#define IS_UNITY

#endif

using System;
using System.Collections.Generic;

using UnityEngine;

namespace DigitalRuby.Tween
{
    /// <summary>
    /// State of an ITween object
    /// </summary>
    public enum TweenState
    {
        /// <summary>
        /// The tween is running.
        /// </summary>
        Running,

        /// <summary>
        /// The tween is paused.
        /// </summary>
        Paused,

        /// <summary>
        /// The tween is stopped.
        /// </summary>
        Stopped
    }

    /// <summary>
    /// The behavior to use when manually stopping a tween.
    /// </summary>
    public enum TweenStopBehavior
    {
        /// <summary>
        /// Does not change the current value.
        /// </summary>
        DoNotModify,

        /// <summary>
        /// Causes the tween to progress to the end value immediately.
        /// </summary>
        Complete
    }

#if IS_UNITY

    /// <summary>
    /// Tween manager - do not add directly as a script, instead call the static methods in your other scripts.
    /// </summary>
    public class TweenFactory : MonoBehaviour
    {
        private static GameObject root;
        private static readonly List<ITween> tweens = new List<ITween>();
        private static GameObject toDestroy;

        private static void EnsureCreated()
        {
            if (root == null && Application.isPlaying)
            {
                root = GameObject.Find("DigitalRubyTween");
                if (root == null || root.GetComponent<TweenFactory>() == null)
                {
                    if (root != null)
                    {
                        toDestroy = root;
                    }
                    root = new GameObject { name = "DigitalRubyTween", hideFlags = HideFlags.HideAndDontSave };
                    root.AddComponent<TweenFactory>().hideFlags = HideFlags.HideAndDontSave;
                }
                if (Application.isPlaying)
                {
                    GameObject.DontDestroyOnLoad(root);
                }
            }
        }

        private void Start()
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += SceneManagerSceneLoaded;
            if (toDestroy != null)
            {
                GameObject.Destroy(toDestroy);
                toDestroy = null;
            }
        }

        private void SceneManagerSceneLoaded(UnityEngine.SceneManagement.Scene s, UnityEngine.SceneManagement.LoadSceneMode m)
        {
            if (ClearTweensOnLevelLoad)
            {
                tweens.Clear();
            }
        }

        private void Update()
        {
            ITween t;

            for (int i = tweens.Count - 1; i >= 0; i--)
            {
                t = tweens[i];
                if (t.Update(t.TimeFunc()) && i < tweens.Count && tweens[i] == t)
                {
                    tweens.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Start and add a float tween
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="start">Start value</param>
        /// <param name="end">End value</param>
        /// <param name="duration">Duration in seconds</param>
        /// <param name="scaleFunc">Scale function</param>
        /// <param name="progress">Progress handler</param>
        /// <param name="completion">Completion handler</param>
        /// <returns>FloatTween</returns>
        public static FloatTween Tween(object key, float start, float end, float duration, Func<float, float> scaleFunc, System.Action<ITween<float>> progress, System.Action<ITween<float>> completion = null)
        {
            FloatTween t = new FloatTween();
            t.Key = key;
            t.Setup(start, end, duration, scaleFunc, progress, completion);
            t.Start();
            AddTween(t);

            return t;
        }

        /// <summary>
        /// Start and add a Vector2 tween
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="start">Start value</param>
        /// <param name="end">End value</param>
        /// <param name="duration">Duration in seconds</param>
        /// <param name="scaleFunc">Scale function</param>
        /// <param name="progress">Progress handler</param>
        /// <param name="completion">Completion handler</param>
        /// <returns>Vector2Tween</returns>
        public static Vector2Tween Tween(object key, Vector2 start, Vector2 end, float duration, Func<float, float> scaleFunc, System.Action<ITween<Vector2>> progress, System.Action<ITween<Vector2>> completion = null)
        {
            Vector2Tween t = new Vector2Tween();
            t.Key = key;
            t.Setup(start, end, duration, scaleFunc, progress, completion);
            t.Start();
            AddTween(t);

            return t;
        }

        /// <summary>
        /// Start and add a Vector3 tween
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="start">Start value</param>
        /// <param name="end">End value</param>
        /// <param name="duration">Duration in seconds</param>
        /// <param name="scaleFunc">Scale function</param>
        /// <param name="progress">Progress handler</param>
        /// <param name="completion">Completion handler</param>
        /// <returns>Vector3Tween</returns>
        public static Vector3Tween Tween(object key, Vector3 start, Vector3 end, float duration, Func<float, float> scaleFunc, System.Action<ITween<Vector3>> progress, System.Action<ITween<Vector3>> completion = null)
        {
            Vector3Tween t = new Vector3Tween();
            t.Key = key;
            t.Setup(start, end, duration, scaleFunc, progress, completion);
            t.Start();
            AddTween(t);

            return t;
        }

        /// <summary>
        /// Start and add a Vector4 tween
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="start">Start value</param>
        /// <param name="end">End value</param>
        /// <param name="duration">Duration in seconds</param>
        /// <param name="scaleFunc">Scale function</param>
        /// <param name="progress">Progress handler</param>
        /// <param name="completion">Completion handler</param>
        /// <returns>Vector4Tween</returns>
        public static Vector4Tween Tween(object key, Vector4 start, Vector4 end, float duration, Func<float, float> scaleFunc, System.Action<ITween<Vector4>> progress, System.Action<ITween<Vector4>> completion = null)
        {
            Vector4Tween t = new Vector4Tween();
            t.Key = key;
            t.Setup(start, end, duration, scaleFunc, progress, completion);
            t.Start();
            AddTween(t);

            return t;
        }

        /// <summary>
        /// Start and add a Color tween
        /// </summary>
        /// <param name="start">Start value</param>
        /// <param name="end">End value</param>
        /// <param name="duration">Duration in seconds</param>
        /// <param name="scaleFunc">Scale function</param>
        /// <param name="progress">Progress handler</param>
        /// <param name="completion">Completion handler</param>
        /// <returns>ColorTween</returns>
        public static ColorTween Tween(object key, Color start, Color end, float duration, Func<float, float> scaleFunc, System.Action<ITween<Color>> progress, System.Action<ITween<Color>> completion = null)
        {
            ColorTween t = new ColorTween();
            t.Key = key;
            t.Setup(start, end, duration, scaleFunc, progress, completion);
            t.Start();
            AddTween(t);

            return t;
        }

        /// <summary>
        /// Start and add a Quaternion tween
        /// </summary>
        /// <param name="start">Start value</param>
        /// <param name="end">End value</param>
        /// <param name="duration">Duration in seconds</param>
        /// <param name="scaleFunc">Scale function</param>
        /// <param name="progress">Progress handler</param>
        /// <param name="completion">Completion handler</param>
        /// <returns>QuaternionTween</returns>
        public static QuaternionTween Tween(object key, Quaternion start, Quaternion end, float duration, Func<float, float> scaleFunc, System.Action<ITween<Quaternion>> progress, System.Action<ITween<Quaternion>> completion = null)
        {
            QuaternionTween t = new QuaternionTween();
            t.Key = key;
            t.Setup(start, end, duration, scaleFunc, progress, completion);
            t.Start();
            AddTween(t);

            return t;
        }

        /// <summary>
        /// Add a tween
        /// </summary>
        /// <param name="tween">Tween to add</param>
        public static void AddTween(ITween tween)
        {
            EnsureCreated();
            if (tween.Key != null)
            {
                RemoveTweenKey(tween.Key, AddKeyStopBehavior);
            }
            tweens.Add(tween);
        }

        /// <summary>
        /// Remove a tween
        /// </summary>
        /// <param name="tween">Tween to remove</param>
        /// <param name="stopBehavior">Stop behavior</param>
        /// <returns>True if removed, false if not</returns>
        public static bool RemoveTween(ITween tween, TweenStopBehavior stopBehavior)
        {
            tween.Stop(stopBehavior);
            return tweens.Remove(tween);
        }

        /// <summary>
        /// Remove a tween by key
        /// </summary>
        /// <param name="key">Key to remove</param>
        /// <param name="stopBehavior">Stop behavior</param>
        /// <returns>True if removed, false if not</returns>
        public static bool RemoveTweenKey(object key, TweenStopBehavior stopBehavior)
        {
            if (key == null)
            {
                return false;
            }

            bool foundOne = false;
            for (int i = tweens.Count - 1; i >= 0; i--)
            {
                ITween t = tweens[i];
                if (key.Equals(t.Key))
                {
                    t.Stop(stopBehavior);
                    tweens.RemoveAt(i);
                    foundOne = true;
                }
            }
            return foundOne;
        }

        /// <summary>
        /// Clear all tweens
        /// </summary>
        public static void Clear()
        {
            tweens.Clear();
        }

        /// <summary>
        /// Stop behavior if you add a tween with a key and tweens already exist with the key
        /// </summary>
        public static TweenStopBehavior AddKeyStopBehavior = TweenStopBehavior.DoNotModify;

        /// <summary>
        /// Whether to clear tweens on level load, default is false
        /// </summary>
        public static bool ClearTweensOnLevelLoad { get; set; }

        /// <summary>
        /// Default time func
        /// </summary>
        public static Func<float> DefaultTimeFunc = TimeFuncDeltaTime;

        /// <summary>
        /// Time func delta time instance
        /// </summary>
        public static readonly Func<float> TimeFuncDeltaTimeFunc = TimeFuncDeltaTime;

        /// <summary>
        /// Time func unscaled delta time instance
        /// </summary>
        public static readonly Func<float> TimeFuncUnscaledDeltaTimeFunc = TimeFuncUnscaledDeltaTime;

        /// <summary>
        /// Time func that uses Time.deltaTime
        /// </summary>
        /// <returns>Time.deltaTime</returns>
        private static float TimeFuncDeltaTime()
        {
            return Time.deltaTime;
        }

        /// <summary>
        /// Time func that uses Time.unscaledDeltaTime
        /// </summary>
        /// <returns>Time.unscaledDeltaTime</returns>
        private static float TimeFuncUnscaledDeltaTime()
        {
            return Time.unscaledDeltaTime;
        }
    }

    /// <summary>
    /// Extensions for tween for game objects - unity only
    /// </summary>
    public static class GameObjectTweenExtensions
    {
        /// <summary>
        /// Start and add a float tween
        /// </summary>
        /// <param name="obj">Game object</param>
        /// <param name="key">Key</param>
        /// <param name="start">Start value</param>
        /// <param name="end">End value</param>
        /// <param name="duration">Duration in seconds</param>
        /// <param name="scaleFunc">Scale function</param>
        /// <param name="progress">Progress handler</param>
        /// <param name="completion">Completion handler</param>
        /// <returns>FloatTween</returns>
        public static FloatTween Tween(this GameObject obj, object key, float start, float end, float duration, Func<float, float> scaleFunc, System.Action<ITween<float>> progress, System.Action<ITween<float>> completion = null)
        {
            FloatTween t = TweenFactory.Tween(key, start, end, duration, scaleFunc, progress, completion);
            t.GameObject = obj;
            t.Renderer = obj.GetComponent<Renderer>();
            return t;
        }

        /// <summary>
        /// Start and add a Vector2 tween
        /// </summary>
        /// <param name="obj">Game object</param>
        /// <param name="key">Key</param>
        /// <param name="start">Start value</param>
        /// <param name="end">End value</param>
        /// <param name="duration">Duration in seconds</param>
        /// <param name="scaleFunc">Scale function</param>
        /// <param name="progress">Progress handler</param>
        /// <param name="completion">Completion handler</param>
        /// <returns>Vector2Tween</returns>
        public static Vector2Tween Tween(this GameObject obj, object key, Vector2 start, Vector2 end, float duration, Func<float, float> scaleFunc, System.Action<ITween<Vector2>> progress, System.Action<ITween<Vector2>> completion = null)
        {
            Vector2Tween t = TweenFactory.Tween(key, start, end, duration, scaleFunc, progress, completion);
            t.GameObject = obj;
            t.Renderer = obj.GetComponent<Renderer>();
            return t;
        }

        /// <summary>
        /// Start and add a Vector3 tween
        /// </summary>
        /// <param name="obj">Game object</param>
        /// <param name="key">Key</param>
        /// <param name="start">Start value</param>
        /// <param name="end">End value</param>
        /// <param name="duration">Duration in seconds</param>
        /// <param name="scaleFunc">Scale function</param>
        /// <param name="progress">Progress handler</param>
        /// <param name="completion">Completion handler</param>
        /// <returns>Vector3Tween</returns>
        public static Vector3Tween Tween(this GameObject obj, object key, Vector3 start, Vector3 end, float duration, Func<float, float> scaleFunc, System.Action<ITween<Vector3>> progress, System.Action<ITween<Vector3>> completion = null)
        {
            Vector3Tween t = TweenFactory.Tween(key, start, end, duration, scaleFunc, progress, completion);
            t.GameObject = obj;
            t.Renderer = obj.GetComponent<Renderer>();
            return t;
        }

        /// <summary>
        /// Start and add a Vector4 tween
        /// </summary>
        /// <param name="obj">Game object</param>
        /// <param name="key">Key</param>
        /// <param name="start">Start value</param>
        /// <param name="end">End value</param>
        /// <param name="duration">Duration in seconds</param>
        /// <param name="scaleFunc">Scale function</param>
        /// <param name="progress">Progress handler</param>
        /// <param name="completion">Completion handler</param>
        /// <returns>Vector4Tween</returns>
        public static Vector4Tween Tween(this GameObject obj, object key, Vector4 start, Vector4 end, float duration, Func<float, float> scaleFunc, System.Action<ITween<Vector4>> progress, System.Action<ITween<Vector4>> completion = null)
        {
            Vector4Tween t = TweenFactory.Tween(key, start, end, duration, scaleFunc, progress, completion);
            t.GameObject = obj;
            t.Renderer = obj.GetComponent<Renderer>();
            return t;
        }

        /// <summary>
        /// Start and add a Color tween
        /// </summary>
        /// <param name="obj">Game object</param>
        /// <param name="start">Start value</param>
        /// <param name="end">End value</param>
        /// <param name="duration">Duration in seconds</param>
        /// <param name="scaleFunc">Scale function</param>
        /// <param name="progress">Progress handler</param>
        /// <param name="completion">Completion handler</param>
        /// <returns>ColorTween</returns>
        public static ColorTween Tween(this GameObject obj, object key, Color start, Color end, float duration, Func<float, float> scaleFunc, System.Action<ITween<Color>> progress, System.Action<ITween<Color>> completion = null)
        {
            ColorTween t = TweenFactory.Tween(key, start, end, duration, scaleFunc, progress, completion);
            t.GameObject = obj;
            t.Renderer = obj.GetComponent<Renderer>();
            return t;
        }

        /// <summary>
        /// Start and add a Quaternion tween
        /// </summary>
        /// <param name="obj">Game object</param>
        /// <param name="start">Start value</param>
        /// <param name="end">End value</param>
        /// <param name="duration">Duration in seconds</param>
        /// <param name="scaleFunc">Scale function</param>
        /// <param name="progress">Progress handler</param>
        /// <param name="completion">Completion handler</param>
        /// <returns>QuaternionTween</returns>
        public static QuaternionTween Tween(this GameObject obj, object key, Quaternion start, Quaternion end, float duration, Func<float, float> scaleFunc, System.Action<ITween<Quaternion>> progress, System.Action<ITween<Quaternion>> completion = null)
        {
            QuaternionTween t = TweenFactory.Tween(key, start, end, duration, scaleFunc, progress, completion);
            t.GameObject = obj;
            t.Renderer = obj.GetComponent<Renderer>();
            return t;
        }
    }

#endif

    /// <summary>
    /// Interface for a tween object.
    /// </summary>
    public interface ITween
    {
        /// <summary>
        /// The key that identifies this tween - can be null
        /// </summary>
        object Key { get; }

        /// <summary>
        /// Gets the current state of the tween.
        /// </summary>
        TweenState State { get; }

        /// <summary>
        /// Time function
        /// </summary>
        System.Func<float> TimeFunc { get; set; }

        /// <summary>
        /// Start the tween.
        /// </summary>
        void Start();

        /// <summary>
        /// Pauses the tween.
        /// </summary>
        void Pause();

        /// <summary>
        /// Resumes the paused tween.
        /// </summary>
        void Resume();

        /// <summary>
        /// Stops the tween.
        /// </summary>
        /// <param name="stopBehavior">The behavior to use to handle the stop.</param>
        void Stop(TweenStopBehavior stopBehavior);

        /// <summary>
        /// Updates the tween.
        /// </summary>
        /// <param name="elapsedTime">The elapsed time to add to the tween.</param>
        /// <returns>True if done, false if not</returns>
        bool Update(float elapsedTime);
    }

    /// <summary>
    /// Interface for a tween object that handles a specific type.
    /// </summary>
    /// <typeparam name="T">The type to tween.</typeparam>
    public interface ITween<T> : ITween where T : struct
    {
        /// <summary>
        /// Gets the current value of the tween.
        /// </summary>
        T CurrentValue { get; }

        /// <summary>
        /// Gets the current progress of the tween.
        /// </summary>
        float CurrentProgress { get; }

        /// <summary>
        /// Initialize a tween.
        /// </summary>
        /// <param name="start">The start value.</param>
        /// <param name="end">The end value.</param>
        /// <param name="duration">The duration of the tween.</param>
        /// <param name="scaleFunc">A function used to scale progress over time.</param>
        /// <param name="progress">Progress callback</param>
        /// <param name="completion">Called when the tween completes</param>
        Tween<T> Setup(T start, T end, float duration, Func<float, float> scaleFunc, System.Action<ITween<T>> progress, System.Action<ITween<T>> completion = null);
    }

    /// <summary>
    /// An implementation of a tween object.
    /// </summary>
    /// <typeparam name="T">The type to tween.</typeparam>
    public class Tween<T> : ITween<T> where T : struct
    {
        private readonly Func<ITween<T>, T, T, float, T> lerpFunc;

        private float currentTime;
        private float duration;
        private Func<float, float> scaleFunc;
        private System.Action<ITween<T>> progressCallback;
        private System.Action<ITween<T>> completionCallback;
        private TweenState state;

        private T start;
        private T end;
        private T value;

        private ITween continueWith;

        /// <summary>
        /// The key that identifies this tween - can be null
        /// </summary>
        public object Key { get; set; }

        /// <summary>
        /// Gets the current time of the tween.
        /// </summary>
        public float CurrentTime { get { return currentTime; } }

        /// <summary>
        /// Gets the duration of the tween.
        /// </summary>
        public float Duration { get { return duration; } }

        /// <summary>
        /// Delay before starting the tween
        /// </summary>
        public float Delay { get; set; }

        /// <summary>
        /// Gets the current state of the tween.
        /// </summary>
        public TweenState State { get { return state; } }

        /// <summary>
        /// Gets the starting value of the tween.
        /// </summary>
        public T StartValue { get { return start; } }

        /// <summary>
        /// Gets the ending value of the tween.
        /// </summary>
        public T EndValue { get { return end; } }

        /// <summary>
        /// Gets the current value of the tween.
        /// </summary>
        public T CurrentValue { get { return value; } }

        /// <summary>
        /// Time function - returns elapsed time for next frame
        /// </summary>
        public System.Func<float> TimeFunc { get; set; }

#if IS_UNITY

        /// <summary>
        /// The game object - null if none
        /// </summary>
        public GameObject GameObject { get; set; }

        /// <summary>
        /// The renderer - null if none
        /// </summary>
        public Renderer Renderer { get; set; }

        /// <summary>
        /// Whether to force update even if renderer is null or not visible or deactivated, default is false
        /// </summary>
        public bool ForceUpdate { get; set; }

#endif

        /// <summary>
        /// Gets the current progress of the tween (0 - 1).
        /// </summary>
        public float CurrentProgress { get; private set; }

        /// <summary>
        /// Initializes a new Tween with a given lerp function.
        /// </summary>
        /// <remarks>
        /// C# generics are good but not good enough. We need a delegate to know how to
        /// interpolate between the start and end values for the given type.
        /// </remarks>
        /// <param name="lerpFunc">The interpolation function for the tween type.</param>
        public Tween(Func<ITween<T>, T, T, float, T> lerpFunc)
        {
            this.lerpFunc = lerpFunc;
            state = TweenState.Stopped;

#if IS_UNITY

            TimeFunc = TweenFactory.DefaultTimeFunc;

#else

            // TODO: Implement your own time functions

#endif

        }

        /// <summary>
        /// Initialize a tween.
        /// </summary>
        /// <param name="start">The start value.</param>
        /// <param name="end">The end value.</param>
        /// <param name="duration">The duration of the tween.</param>
        /// <param name="scaleFunc">A function used to scale progress over time.</param>
        /// <param name="progress">Progress callback</param>
        /// <param name="completion">Called when the tween completes</param>
        public Tween<T> Setup(T start, T end, float duration, Func<float, float> scaleFunc, System.Action<ITween<T>> progress, System.Action<ITween<T>> completion = null)
        {
            scaleFunc = (scaleFunc ?? TweenScaleFunctions.Linear);
            currentTime = 0;
            this.duration = duration;
            this.scaleFunc = scaleFunc;
            this.progressCallback = progress;
            this.completionCallback = completion;
            this.start = start;
            this.end = end;

            return this;
        }

        /// <summary>
        /// Starts a tween. Setup must be called first.
        /// </summary>
        public void Start()
        {
            if (state != TweenState.Running)
            {
                if (duration <= 0.0f && Delay <= 0.0f)
                {
                    // complete immediately
                    value = end;
                    if (progressCallback != null)
                    {
                        progressCallback(this);
                    }
                    if (completionCallback != null)
                    {
                        completionCallback(this);
                    }
                    return;
                }

                state = TweenState.Running;
                UpdateValue();
            }
        }

        /// <summary>
        /// Pauses the tween.
        /// </summary>
        public void Pause()
        {
            if (state == TweenState.Running)
            {
                state = TweenState.Paused;
            }
        }

        /// <summary>
        /// Resumes the paused tween.
        /// </summary>
        public void Resume()
        {
            if (state == TweenState.Paused)
            {
                state = TweenState.Running;
            }
        }

        /// <summary>
        /// Stops the tween.
        /// </summary>
        /// <param name="stopBehavior">The behavior to use to handle the stop.</param>
        public void Stop(TweenStopBehavior stopBehavior)
        {
            if (state != TweenState.Stopped)
            {
                state = TweenState.Stopped;
                if (stopBehavior == TweenStopBehavior.Complete)
                {
                    currentTime = duration;
                    UpdateValue();
                    if (completionCallback != null)
                    {
                        completionCallback.Invoke(this);
                        completionCallback = null;
                    }
                    if (continueWith != null)
                    {
                        continueWith.Start();

#if IS_UNITY

                        TweenFactory.AddTween(continueWith);

#else

                        // TODO: Implement your own continueWith handling

#endif

                        continueWith = null;
                    }
                }
            }
        }

        /// <summary>
        /// Updates the tween.
        /// </summary>
        /// <param name="elapsedTime">The elapsed time to add to the tween.</param>
        /// <returns>True if done, false if not</returns>
        public bool Update(float elapsedTime)
        {
            if (state == TweenState.Running)
            {
                if (Delay > 0.0f)
                {
                    currentTime += elapsedTime;
                    if (currentTime <= Delay)
                    {
                        // delay is not over yet
                        return false;
                    }
                    else
                    {
                        // set to left-over time beyond delay
                        currentTime = (currentTime - Delay);
                        Delay = 0.0f;
                    }
                }
                else
                {
                    currentTime += elapsedTime;
                }

                if (currentTime >= duration)
                {
                    Stop(TweenStopBehavior.Complete);
                    return true;
                }
                else
                {
                    UpdateValue();
                    return false;
                }
            }
            return (state == TweenState.Stopped);
        }

        /// <summary>
        /// Set another tween to execute when this tween finishes. Inherits the Key and if using Unity, GameObject, Renderer and ForceUpdate properties.
        /// </summary>
        /// <typeparam name="TNewTween">Type of new tween</typeparam>
        /// <param name="tween">New tween</param>
        /// <returns>New tween</returns>
        public Tween<TNewTween> ContinueWith<TNewTween>(Tween<TNewTween> tween) where TNewTween : struct
        {
            tween.Key = Key;

#if IS_UNITY

            tween.GameObject = GameObject;
            tween.Renderer = Renderer;
            tween.ForceUpdate = ForceUpdate;

#endif

            continueWith = tween;
            return tween;
        }

        /// <summary>
        /// Helper that uses the current time, duration, and delegates to update the current value.
        /// </summary>
        private void UpdateValue()
        {

#if IS_UNITY

            if (Renderer == null || Renderer.isVisible || ForceUpdate)
            {

#endif

                CurrentProgress = scaleFunc(currentTime / duration);
                value = lerpFunc(this, start, end, CurrentProgress);
                if (progressCallback != null)
                {
                    progressCallback.Invoke(this);
                }

#if IS_UNITY

            }

#endif

        }
    }

    /// <summary>
    /// Object used to tween float values.
    /// </summary>
    public class FloatTween : Tween<float>
    {
        private static float LerpFloat(ITween<float> t, float start, float end, float progress) { return start + (end - start) * progress; }
        private static readonly Func<ITween<float>, float, float, float, float> LerpFunc = LerpFloat;

        /// <summary>
        /// Initializes a new FloatTween instance.
        /// </summary>
        public FloatTween() : base(LerpFunc) { }
    }

    /// <summary>
    /// Object used to tween Vector2 values.
    /// </summary>
    public class Vector2Tween : Tween<Vector2>
    {
        private static Vector2 LerpVector2(ITween<Vector2> t, Vector2 start, Vector2 end, float progress) { return Vector2.Lerp(start, end, progress); }
        private static readonly Func<ITween<Vector2>, Vector2, Vector2, float, Vector2> LerpFunc = LerpVector2;

        /// <summary>
        /// Initializes a new Vector2Tween instance.
        /// </summary>
        public Vector2Tween() : base(LerpFunc) { }
    }

    /// <summary>
    /// Object used to tween Vector3 values.
    /// </summary>
    public class Vector3Tween : Tween<Vector3>
    {
        private static Vector3 LerpVector3(ITween<Vector3> t, Vector3 start, Vector3 end, float progress) { return Vector3.Lerp(start, end, progress); }
        private static readonly Func<ITween<Vector3>, Vector3, Vector3, float, Vector3> LerpFunc = LerpVector3;

        /// <summary>
        /// Initializes a new Vector3Tween instance.
        /// </summary>
        public Vector3Tween() : base(LerpFunc) { }
    }

    /// <summary>
    /// Object used to tween Vector4 values.
    /// </summary>
    public class Vector4Tween : Tween<Vector4>
    {
        private static Vector4 LerpVector4(ITween<Vector4> t, Vector4 start, Vector4 end, float progress) { return Vector4.Lerp(start, end, progress); }
        private static readonly Func<ITween<Vector4>, Vector4, Vector4, float, Vector4> LerpFunc = LerpVector4;

        /// <summary>
        /// Initializes a new Vector4Tween instance.
        /// </summary>
        public Vector4Tween() : base(LerpFunc) { }
    }

    /// <summary>
    /// Object used to tween Color values.
    /// </summary>
    public class ColorTween : Tween<Color>
    {
        private static Color LerpColor(ITween<Color> t, Color start, Color end, float progress) { return Color.Lerp(start, end, progress); }
        private static readonly Func<ITween<Color>, Color, Color, float, Color> LerpFunc = LerpColor;

        /// <summary>
        /// Initializes a new ColorTween instance.
        /// </summary>
        public ColorTween() : base(LerpFunc) { }
    }

    /// <summary>
    /// Object used to tween Quaternion values.
    /// </summary>
    public class QuaternionTween : Tween<Quaternion>
    {
        private static Quaternion LerpQuaternion(ITween<Quaternion> t, Quaternion start, Quaternion end, float progress) { return Quaternion.Lerp(start, end, progress); }
        private static readonly Func<ITween<Quaternion>, Quaternion, Quaternion, float, Quaternion> LerpFunc = LerpQuaternion;

        /// <summary>
        /// Initializes a new QuaternionTween instance.
        /// </summary>
        public QuaternionTween() : base(LerpFunc) { }
    }

    /// <summary>
    /// Tween scale functions
    /// Implementations based on http://theinstructionlimit.com/flash-style-tweeneasing-functions-in-c, which are based on http://www.robertpenner.com/easing/
    /// </remarks>
    public static class TweenScaleFunctions
    {
        private const float halfPi = Mathf.PI * 0.5f;

        /// <summary>
        /// A linear progress scale function.
        /// </summary>
        public static readonly Func<float, float> Linear = LinearFunc;
        private static float LinearFunc(float progress) { return progress; }

        /// <summary>
        /// A quadratic (x^2) progress scale function that eases in.
        /// </summary>
        public static readonly Func<float, float> QuadraticEaseIn = QuadraticEaseInFunc;
        private static float QuadraticEaseInFunc(float progress) { return EaseInPower(progress, 2); }

        /// <summary>
        /// A quadratic (x^2) progress scale function that eases out.
        /// </summary>
        public static readonly Func<float, float> QuadraticEaseOut = QuadraticEaseOutFunc;
        private static float QuadraticEaseOutFunc(float progress) { return EaseOutPower(progress, 2); }

        /// <summary>
        /// A quadratic (x^2) progress scale function that eases in and out.
        /// </summary>
        public static readonly Func<float, float> QuadraticEaseInOut = QuadraticEaseInOutFunc;
        private static float QuadraticEaseInOutFunc(float progress) { return EaseInOutPower(progress, 2); }

        /// <summary>
        /// A cubic (x^3) progress scale function that eases in.
        /// </summary>
        public static readonly Func<float, float> CubicEaseIn = CubicEaseInFunc;
        private static float CubicEaseInFunc(float progress) { return EaseInPower(progress, 3); }

        /// <summary>
        /// A cubic (x^3) progress scale function that eases out.
        /// </summary>
        public static readonly Func<float, float> CubicEaseOut = CubicEaseOutFunc;
        private static float CubicEaseOutFunc(float progress) { return EaseOutPower(progress, 3); }

        /// <summary>
        /// A cubic (x^3) progress scale function that eases in and out.
        /// </summary>
        public static readonly Func<float, float> CubicEaseInOut = CubicEaseInOutFunc;
        private static float CubicEaseInOutFunc(float progress) { return EaseInOutPower(progress, 3); }

        /// <summary>
        /// A quartic (x^4) progress scale function that eases in.
        /// </summary>
        public static readonly Func<float, float> QuarticEaseIn = QuarticEaseInFunc;
        private static float QuarticEaseInFunc(float progress) { return EaseInPower(progress, 4); }

        /// <summary>
        /// A quartic (x^4) progress scale function that eases out.
        /// </summary>
        public static readonly Func<float, float> QuarticEaseOut = QuarticEaseOutFunc;
        private static float QuarticEaseOutFunc(float progress) { return EaseOutPower(progress, 4); }

        /// <summary>
        /// A quartic (x^4) progress scale function that eases in and out.
        /// </summary>
        public static readonly Func<float, float> QuarticEaseInOut = QuarticEaseInOutFunc;
        private static float QuarticEaseInOutFunc(float progress) { return EaseInOutPower(progress, 4); }

        /// <summary>
        /// A quintic (x^5) progress scale function that eases in.
        /// </summary>
        public static readonly Func<float, float> QuinticEaseIn = QuinticEaseInFunc;
        private static float QuinticEaseInFunc(float progress) { return EaseInPower(progress, 5); }

        /// <summary>
        /// A quintic (x^5) progress scale function that eases out.
        /// </summary>
        public static readonly Func<float, float> QuinticEaseOut = QuinticEaseOutFunc;
        private static float QuinticEaseOutFunc(float progress) { return EaseOutPower(progress, 5); }

        /// <summary>
        /// A quintic (x^5) progress scale function that eases in and out.
        /// </summary>
        public static readonly Func<float, float> QuinticEaseInOut = QuinticEaseInOutFunc;
        private static float QuinticEaseInOutFunc(float progress) { return EaseInOutPower(progress, 5); }

        /// <summary>
        /// A sine progress scale function that eases in.
        /// </summary>
        public static readonly Func<float, float> SineEaseIn = SineEaseInFunc;
        private static float SineEaseInFunc(float progress) { return Mathf.Sin(progress * halfPi - halfPi) + 1; }

        /// <summary>
        /// A sine progress scale function that eases out.
        /// </summary>
        public static readonly Func<float, float> SineEaseOut = SineEaseOutFunc;
        private static float SineEaseOutFunc(float progress) { return Mathf.Sin(progress * halfPi); }

        /// <summary>
        /// A sine progress scale function that eases in and out.
        /// </summary>
        public static readonly Func<float, float> SineEaseInOut = SineEaseInOutFunc;
        private static float SineEaseInOutFunc(float progress) { return (Mathf.Sin(progress * Mathf.PI - halfPi) + 1) / 2; }

        private static float EaseInPower(float progress, int power)
        {
            return Mathf.Pow(progress, power);
        }

        private static float EaseOutPower(float progress, int power)
        {
            int sign = power % 2 == 0 ? -1 : 1;
            return (sign * (Mathf.Pow(progress - 1, power) + sign));
        }

        private static float EaseInOutPower(float progress, int power)
        {
            progress *= 2.0f;
            if (progress < 1)
            {
                return Mathf.Pow(progress, power) / 2.0f;
            }
            else
            {
                int sign = power % 2 == 0 ? -1 : 1;
                return (sign / 2.0f * (Mathf.Pow(progress - 2, power) + sign * 2));
            }
        }
    }
}