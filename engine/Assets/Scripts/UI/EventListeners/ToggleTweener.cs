using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Utilities.ColorManager;

namespace UI.EventListeners {
    public class ToggleTweener : MonoBehaviour {
        [HideInInspector]
        public Synthesis.UI.Dynamic.Toggle _synthesisToggle;
        [SerializeField]
        private Toggle _unityToggle;
        [SerializeField]
        private GradientImageUpdater _background;
        [SerializeField]
        private Image _checkmark;

        private string _tweenKey = Guid.NewGuid() + "_toggle";

        private Color _offColor;
        private Color _onColorLeft;
        private Color _onColorRight;

        private bool _tweenEnabled;

        private void Start() {
            StartCoroutine(LateStart());
            _prevState = _unityToggle.isOn;
            SetLerpedColor(1, _prevState);
        }

        private IEnumerator LateStart() {
            yield return new WaitForEndOfFrame();
            _tweenEnabled = true;
        }

        private bool _prevState;

        public void OnStateChanged() {
            if (_unityToggle.isOn == _prevState)
                return;

            _prevState = _unityToggle.isOn;

            if (!_tweenEnabled) {
                return;
            }

            SynthesisTween.CancelTween(_tweenKey);

            SynthesisTween.MakeTween(_tweenKey, 0f, 1f, 0.05f,
                (t, a, b) => SynthesisTweenInterpolationFunctions.FloatInterp(t, (float) a, (float) b),
                SynthesisTweenScaleFunctions.EaseInCubic, (s) => SetLerpedColor(s.CurrentProgress, _unityToggle.isOn));
        }

        private void SetLerpedColor(float progress, bool state) {
            progress = Mathf.Clamp(progress, 0.01f, 0.99f);

            float percent = state ? 1 - progress : progress;
            if (_synthesisToggle != null) {
                var left = Color.Lerp(
                    ColorManager.GetColor(_synthesisToggle.EnabledColor.left), _synthesisToggle.DisabledColor, percent);
                var right = Color.Lerp(ColorManager.GetColor(_synthesisToggle.EnabledColor.right),
                    _synthesisToggle.DisabledColor, percent);

                _background.StartColor = left;
                _background.EndColor   = right;
                _background.Refresh();

                if (_checkmark != null)
                    _checkmark.color = new Color(0, 0, 0, 1 - percent);
            }
        }
    }
}
