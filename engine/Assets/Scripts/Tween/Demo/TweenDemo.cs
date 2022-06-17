/*
The MIT License (MIT)
Copyright (c) 2016 Digital Ruby, LLC
http://www.digitalruby.com
Created by Jeff Johnson

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using UnityEngine;
using System.Collections;

// for your own scripts make sure to add the following line:
using DigitalRuby.Tween;
using UnityEngine.SceneManagement;

namespace DigitalRuby.Tween
{
    public class TweenDemo : MonoBehaviour
    {
        public GameObject Circle;
        public Light Light;

        private SpriteRenderer spriteRenderer;

        private void TweenMove()
        {
            System.Action<ITween<Vector3>> updateCirclePos = (t) =>
            {
                Circle.gameObject.transform.position = t.CurrentValue;
            };

            System.Action<ITween<Vector3>> circleMoveCompleted = (t) =>
            {
                Debug.Log("Circle move completed");
            };

            Vector3 currentPos = Circle.transform.position;
            Vector3 startPos = Camera.main.ViewportToWorldPoint(Vector3.zero);
            Vector3 midPos = Camera.main.ViewportToWorldPoint(Vector3.one);
            Vector3 endPos = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0.5f));
            currentPos.z = startPos.z = midPos.z = endPos.z = 0.0f;

            // completion defaults to null if not passed in
            Circle.gameObject.Tween("MoveCircle", currentPos, startPos, 1.75f, TweenScaleFunctions.CubicEaseIn, updateCirclePos)
                .ContinueWith(new Vector3Tween().Setup(startPos, midPos, 1.75f, TweenScaleFunctions.Linear, updateCirclePos))
                .ContinueWith(new Vector3Tween().Setup(midPos, endPos, 1.75f, TweenScaleFunctions.CubicEaseOut, updateCirclePos, circleMoveCompleted));
        }

        private void TweenColor()
        {
            System.Action<ITween<Color>> updateColor = (t) =>
            {
                spriteRenderer.color = t.CurrentValue;
            };

            Color endColor = UnityEngine.Random.ColorHSV(0.0f, 1.0f, 0.0f, 1.0f, 0.5f, 1.0f, 1.0f, 1.0f);

            // completion defaults to null if not passed in
            Circle.gameObject.Tween("ColorCircle", spriteRenderer.color, endColor, 1.0f, TweenScaleFunctions.QuadraticEaseOut, updateColor);
        }

        private void TweenRotate()
        {
            System.Action<ITween<float>> circleRotate = (t) =>
            {
                // start rotation from identity to ensure no stuttering
                Circle.transform.rotation = Quaternion.identity;
                Circle.transform.Rotate(Camera.main.transform.forward, t.CurrentValue);
            };

            float startAngle = Circle.transform.rotation.eulerAngles.z;
            float endAngle = startAngle + 720.0f;

            // completion defaults to null if not passed in
            Circle.gameObject.Tween("RotateCircle", startAngle, endAngle, 2.0f, TweenScaleFunctions.CubicEaseInOut, circleRotate);
        }

        private void TweenReset()
        {
            SceneManager.LoadScene(0, LoadSceneMode.Single);
        }

        private void Start()
        {
            // for demo purposes, clear all tweens when new level loads, default is false
            TweenFactory.ClearTweensOnLevelLoad = true;
            spriteRenderer = Circle.GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                TweenMove();
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                TweenColor();
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                TweenRotate();
            }
            if (Input.GetKeyDown(KeyCode.R))
            {
                TweenReset();
            }
        }
    }
}