using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Button))]

public class MenuSlideTween : MonoBehaviour, IPointerClickHandler {
    private readonly string _key               = "slide";
    private const float SCREEN_TWO_ANCHOR_DIFF = 4000f;
    private float _imageAnchorDiff;
    private float _textAnchorDiff;
    private Transform _screenTransform;

    public void OnPointerClick(PointerEventData eventData) {
        _screenTransform = GameObject.Find("Canvas").transform.Find("MenuPanel").transform.Find("ScreenSpace");
        _imageAnchorDiff = GetRectTransform("ImageContainer").anchoredPosition.x;
        _textAnchorDiff  = GetRectTransform("ScreenOneText").anchoredPosition.x;

        SynthesisTween.MakeTween(_key, GetRectTransform("Anchor").anchoredPosition.x, -SCREEN_TWO_ANCHOR_DIFF, 0.5f,
            (t, a, b) => SynthesisTweenInterpolationFunctions.FloatInterp(t, (float) a, (float) b),
            SynthesisTweenScaleFunctions.EaseOutCubic, TweenProgress);
    }

    private void TweenProgress(SynthesisTween.SynthesisTweenStatus status) {
        MoveObject(GetRectTransform("Anchor"), status.CurrentValue<float>());
        MoveObject(GetRectTransform("ImageContainer"), status.CurrentValue<float>() + _imageAnchorDiff);
        MoveObject(GetRectTransform("ScreenOneText"), status.CurrentValue<float>() + _textAnchorDiff);
        MoveObject(GetRectTransform("ScreenTwo"), status.CurrentValue<float>() + SCREEN_TWO_ANCHOR_DIFF);
    }

    private void MoveObject(RectTransform t, float pos) {
        t.anchoredPosition = new Vector2(pos, t.anchoredPosition.y);
    }

    private RectTransform GetRectTransform(string name) {
        return _screenTransform.transform.Find(name).gameObject.GetComponent<RectTransform>();
    }

    public void OnDestroy() {
        SynthesisTween.CancelTween(_key);
    }
}
