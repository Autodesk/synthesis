using System.Collections;
using System.Collections.Generic;
using SynthesisAPI.Utilities;
using UnityEngine;
using Engine;
using System.Runtime.CompilerServices;
public class ToastManager : MonoBehaviour
{
    public GameObject toast;
    public Transform scrollTransform;
    public GameObject toastView;
    private static ToastLogger t = new ToastLogger();
    void Start()
    {
        t.SetUpToastObject(toast, scrollTransform, this);
    }

    public void onAddToast()
    {
        if (!toastView.activeInHierarchy)
            toastView.SetActive(true);
    }
    public void onRemoveToast()
    {
        if (scrollTransform.childCount <= 2)//one of the children is an empty child
            toastView.SetActive(false);
    }
    public void ClearAll()
    {
        foreach (Transform g in scrollTransform)
        {
            //the empty object is there under the scroll rect because for some reason it fixes initial spawn issues
            if (g.gameObject.name != "Empty")
                Destroy(g.gameObject);
            toastView.SetActive(false);
        }

    }

    //call this to send a toast using this instance of ToastLogger
    public static void Log(object o, LogLevel logLevel = LogLevel.Info, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
    {
        t.Log(o, logLevel, memberName, filePath, lineNumber);
    }

}
