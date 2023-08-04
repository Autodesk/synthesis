using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DigitalRuby.Tween;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using Synthesis.UI;
using TMPro;

[RequireComponent(typeof(Button))]

public class LinksTween : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
    private readonly string _growKey   = "link_grow";
    private readonly string _shrinkKey = "link_shrink";
    private TMP_Text text;
    public float sizeStart;
    public float sizeEnd;

    public void OnPointerEnter(PointerEventData eventData) {
        text      = gameObject.GetComponent<TMP_Text>();
        sizeStart = text.fontSize;
        if (!SynthesisTween.TweenExists(_growKey))
            SynthesisTween.MakeTween(_growKey, text.fontSize, sizeEnd, 1f,
                (t, a, b) => SynthesisTweenInterpolationFunctions.FloatInterp(t, (float) a, (float) b),
                SynthesisTweenScaleFunctions.EaseOutCubic, TweenUp);
    }

    public void OnPointerExit(PointerEventData eventData) {
        SynthesisTween.CancelTween(_growKey);
        SynthesisTween.CancelTween(_shrinkKey);

        SynthesisTween.MakeTween(_shrinkKey, text.fontSize, sizeStart, 1f,
            (t, a, b) => SynthesisTweenInterpolationFunctions.FloatInterp(t, (float) a, (float) b),
            SynthesisTweenScaleFunctions.EaseOutCubic, TweenDown);
    }

    private void TweenUp(SynthesisTween.SynthesisTweenStatus status) {
        if (text.fontSize < sizeEnd)
            gameObject.GetComponent<TMP_Text>().fontSize += 1;
    }

    private void TweenDown(SynthesisTween.SynthesisTweenStatus status) {
        if (text.fontSize > sizeStart)
            gameObject.GetComponent<TMP_Text>().fontSize -= 1;
    }

    public void OnDestroy() {
        SynthesisTween.CancelTween(_growKey);
    }
}
