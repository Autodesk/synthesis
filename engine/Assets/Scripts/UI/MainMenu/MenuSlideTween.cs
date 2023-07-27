using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Button))]

public class MenuSlideTween: MonoBehaviour, IPointerClickHandler {
    private string KEY = "slide";
    private float imageAnchorDiff;
    private float textAnchorDiff;
    private Transform screenTransform;

    public void OnPointerClick(PointerEventData eventData) {
        screenTransform = GameObject.Find("Canvas").transform.Find("MenuPanel").transform.Find("ScreenSpace");
        imageAnchorDiff = GetRectTransform("MakeAnythingImage").anchoredPosition.x;
        textAnchorDiff = GetRectTransform("ScreenOneText").anchoredPosition.x;
        
        SynthesisTween.MakeTween(KEY, GetRectTransform("Anchor").anchoredPosition.x, -4000f, 1f,
                (t, a, b) => SynthesisTweenInterpolationFunctions.FloatInterp(t, (float) a, (float) b),
                SynthesisTweenScaleFunctions.EaseInOutQuint, TweenProgress);
    }

    public void TweenProgress(SynthesisTween.SynthesisTweenStatus status) {
        MoveObject(GetRectTransform("Anchor"), status.CurrentValue<float>());
        MoveObject(GetRectTransform("MakeAnythingImage"), status.CurrentValue<float>() + imageAnchorDiff);
        MoveObject(GetRectTransform("ScreenOneText"), status.CurrentValue<float>() + textAnchorDiff);
        MoveObject(GetRectTransform("ScreenTwo"), status.CurrentValue<float>() + 4000f);
    }

    public void MoveObject(RectTransform t, float pos) {
        t.anchoredPosition = new Vector2(pos - 10f, t.anchoredPosition.y);
    }

    private RectTransform GetRectTransform(string name) {
        return screenTransform.transform.Find(name).gameObject.GetComponent<RectTransform>();
    }


    public void OnDestroy() {
        SynthesisTween.CancelTween(KEY);
    }
}

