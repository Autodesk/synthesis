using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TopButton : MonoBehaviour {
    [SerializeField]
    public TMP_Text Tag;
    [SerializeField]
    public Image Underline;
    [SerializeField]
    public Button ActualButton;

    public void SetUnderlineHeight(float height) {
        RectTransform rt = Underline.GetComponent<RectTransform>();
        rt.sizeDelta     = new Vector2(rt.sizeDelta.x, height);
    }

    public void SetUnderlineColor(Color c) {
        Underline.color = c;
    }
}
