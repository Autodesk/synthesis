using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DigitalRuby.Tween;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using Synthesis.UI;

[RequireComponent(typeof(Button))]
public class MenuButtonTween : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler {
    public float IndicateScaleFactor = 1.1f;
    public float ClickedScaleFactor  = 0.98f;
    private float _completionTime    = 0.2f;
    private Button _button;

    public const string INDICATE_TWEEN = "indicate";
    public const string RETURN_TWEEN   = "return";
    public const string CLICKED_TWEEN  = "pressed";

    private Vector3Tween _activeTween = null;
    private string _state             = "exit";

    private Action<ITween<Vector3>> updateButtonScale;

    public void Start() {
        _button           = GetComponent<Button>();
        updateButtonScale = t => { gameObject.transform.localScale = t.CurrentValue; };
    }

    public void OnPointerEnter(PointerEventData eventData) {
        _state = "enter";
        // MenuManager.Instance.SpotlightButton(transform);
        TweenFactory.RemoveTweenKey(gameObject.name + CLICKED_TWEEN, TweenStopBehavior.DoNotModify);
        TweenFactory.RemoveTweenKey(gameObject.name + RETURN_TWEEN, TweenStopBehavior.DoNotModify);
        // transform.GetComponent<Image>().
        _activeTween = gameObject.Tween(gameObject.name + INDICATE_TWEEN, gameObject.transform.localScale,
            Vector3.one * IndicateScaleFactor, _completionTime, TweenScaleFunctions.CubicEaseOut, updateButtonScale);
    }

    public void OnPointerExit(PointerEventData eventData) {
        _state = "exit";
        TweenFactory.RemoveTweenKey(gameObject.name + INDICATE_TWEEN, TweenStopBehavior.DoNotModify);
        if (_activeTween.State == TweenState.Running && _activeTween.Key.Equals(gameObject.name + CLICKED_TWEEN)) {
            _activeTween.ContinueWith(new Vector3Tween().Setup(Vector3.one * ClickedScaleFactor, Vector3.one,
                _completionTime, TweenScaleFunctions.CubicEaseOut, updateButtonScale));
        } else {
            // transform.position = new Vector3(transform.position.x, transform.position.y, 0f);
            _activeTween = gameObject.Tween(gameObject.name + RETURN_TWEEN, gameObject.transform.localScale,
                Vector3.one, _completionTime, TweenScaleFunctions.CubicEaseOut, updateButtonScale);
        }
    }

    public void OnPointerClick(PointerEventData eventData) {
        _state = "click";
        if (TweenFactory.RemoveTweenKey(gameObject.name + CLICKED_TWEEN, TweenStopBehavior.DoNotModify)) {
            gameObject.transform.localScale = Vector3.one;
        }
        TweenFactory.RemoveTweenKey(gameObject.name + INDICATE_TWEEN, TweenStopBehavior.DoNotModify);
        _activeTween = gameObject.Tween(gameObject.name + CLICKED_TWEEN, gameObject.transform.localScale,
            Vector3.one * ClickedScaleFactor, _completionTime, TweenScaleFunctions.CubicEaseOut, updateButtonScale,
            x => {
                if (!_state.Equals("exit"))
                    OnPointerEnter(null);
            });
    }

    public void OnDestroy() {
        TweenFactory.RemoveTweenKey(gameObject.name + CLICKED_TWEEN, TweenStopBehavior.DoNotModify);
        TweenFactory.RemoveTweenKey(gameObject.name + INDICATE_TWEEN, TweenStopBehavior.DoNotModify);
        TweenFactory.RemoveTweenKey(gameObject.name + RETURN_TWEEN, TweenStopBehavior.DoNotModify);
    }
}
