using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SynthesisAPI.Utilities;
using TMPro;
using UnityEngine.UI;

using Synthesis.UI.Dynamic;

public class ToastConfig : MonoBehaviour
{
    public Sprite DebugIcon;
    public Sprite WarningIcon;
    public Sprite ErrorIcon;
    public Sprite InfoIcon;

    private static readonly Color DebugColor = new Color(0.7529412f,0.7372549f,0.7372549f);
    private static readonly Color WarningColor = new Color(1,0.6431373f,0.01568628f);
    private static readonly Color ErrorColor = new Color(1,0.07843138f,0.2666667f);
    private static readonly Color InfoColor = new Color(0.03137255f,0.6745098f,0.8627451f);

    public TextMeshProUGUI t;
    public UnityEngine.UI.Image icon;
    public UnityEngine.UI.Image background;

    private ToastManager tm;
    private LogLevel toastLevel;

    public void Init(string text, LogLevel level, ToastManager tm)
    {
        t.text = text;
        toastLevel = level;
        //SET ICON IMAGE
        switch (level)
        {
            case LogLevel.Debug:
                {
                    icon.sprite = DebugIcon;
                    background.color = DebugColor;
                    break;
                }
            case LogLevel.Warning:
                {
                    icon.sprite = WarningIcon;
                    background.color = WarningColor;
                    break;
                }
            case LogLevel.Error:
                {
                    icon.sprite = ErrorIcon;
                    background.color = ErrorColor;
                    break;
                }
            default:
            case LogLevel.Info:
                {
                    icon.sprite = InfoIcon;
                    background.color = InfoColor;
                    break;
                }
        }
        this.tm = tm;

    }
    public void CloseToast()
    {
        tm.onRemoveToast();
        Destroy(gameObject);
    }
    public void ClearAll()
    {
        tm.ClearAll();
    }
    public void ExpandToast(){ 
        ToastModal.toastLevel = toastLevel;  
        ToastModal.toastText = t.text;
        DynamicUIManager.CloseActiveModal();
        DynamicUIManager.CreateModal<ToastModal>();
    }
}
