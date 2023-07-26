using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DigitalRuby.Tween;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using Synthesis.UI;

[RequireComponent(typeof(Button))]

public class MenuSlide: MonoBehaviour, IPointerClickHandler {
    private string KEY = "slide";

    public void OnPointerClick(PointerEventData eventData) {
        SynthesisTween.MakeTween(KEY, gameObject.GetComponent<RectTransform>().anchoredPosition.x, -3000f, 1f,
                (t, a, b) => SynthesisTweenInterpolationFunctions.FloatInterp(t, (float) a, (float) b),
                SynthesisTweenScaleFunctions.EaseOutCubic, TweenProgress);    
    }

    public void TweenProgress(SynthesisTween.SynthesisTweenStatus status) {
        gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(status.CurrentValue<float>() - 10f, gameObject.GetComponent<RectTransform>().anchoredPosition.y);
    }
}