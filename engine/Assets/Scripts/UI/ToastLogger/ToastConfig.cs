using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SynthesisAPI.Utilities;
using TMPro;
using UnityEngine.UI;


public class ToastConfig : MonoBehaviour
{
    public Sprite DebugIcon;
    public Sprite WarningIcon;
    public Sprite ErrorIcon;
    public Sprite InfoIcon;

    public TextMeshProUGUI t;
    public Image icon;
    
    private ToastManager tm;

    public void Init(string text, LogLevel level, ToastManager tm){
            t.text = text;
            //SET ICON IMAGE
            switch (level)
            {
                case LogLevel.Debug:
                    {
                        icon.sprite = DebugIcon;
                        break;
                    }
                case LogLevel.Warning:
                    {
                        icon.sprite = WarningIcon;
                        break;
                    }
                case LogLevel.Error:
                    {
                        icon.sprite = ErrorIcon;
                        break;
                    }
                default:
                case LogLevel.Info:
                    {
                        icon.sprite = InfoIcon;
                        break;
                    }
            }
            this.tm = tm;

    }
    public void CloseToast(){
        tm.onRemoveToast();
        Destroy(gameObject);
    }
    public void ClearAll(){
        tm.ClearAll();
    }
}
