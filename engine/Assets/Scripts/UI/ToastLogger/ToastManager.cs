using Engine;
using SynthesisAPI.Utilities;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class ToastManager : MonoBehaviour {
    public GameObject toast;
    public Transform scrollTransform;
    public GameObject toastView;
    private static ToastLogger t = new ToastLogger();

    private void Start() {
        t.SetUpToastObject(toast, scrollTransform, this);
    }

    public void onAddToast() {
        if (!toastView.activeInHierarchy)
            toastView.SetActive(true);
    }

    public void onRemoveToast() {
        // One of the children is an empty child
        if (scrollTransform.childCount <= 2) {
            toastView.SetActive(false);
        }
    }

    public void ClearAll() {
        foreach (Transform g in scrollTransform) {
            // The empty object is there under the scroll rect because for some reason it fixes initial spawn issues
            if (g.gameObject.name != "Empty") {
                Destroy(g.gameObject);
            }

            toastView.SetActive(false);
        }
    }

    // Call this to send a toast using this instance of ToastLogger
    public static void Log(object o, LogLevel logLevel = LogLevel.Info, [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0) {
        t.Log(o, logLevel, memberName, filePath, lineNumber);
    }
}
