using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Button))]

public class MenuSlideTween: MonoBehaviour, IPointerClickHandler {
    private string KEY = "slide";
    private Transform screenTransform;

    public void OnPointerClick(PointerEventData eventData) {
        screenTransform = GameObject.Find("Canvas").transform.Find("MenuPanel").transform.Find("ScreenSpace").transform;
        SynthesisTween.MakeTween(KEY, GetRectTransform("ScreenOne").anchoredPosition.x, -4000f, 1f,
                (t, a, b) => SynthesisTweenInterpolationFunctions.FloatInterp(t, (float) a, (float) b),
                SynthesisTweenScaleFunctions.EaseInOutQuint, TweenProgress);
    }

    public void TweenProgress(SynthesisTween.SynthesisTweenStatus status) {
        MoveObject(GetRectTransform("ScreenOne"), status.CurrentValue<float>());
        MoveObject(GetRectTransform("ScreenTwo"), status.CurrentValue<float>() + 4000f);
    }

    public void MoveObject(RectTransform t, float pos) {
        t.anchoredPosition = new Vector2(pos - 10f, t.anchoredPosition.y);
    }

    private RectTransform GetRectTransform(string name) {
        return screenTransform.Find(name).gameObject.GetComponent<RectTransform>();
    }


    public void OnDestroy() {
        SynthesisTween.CancelTween(KEY);
    }
}

