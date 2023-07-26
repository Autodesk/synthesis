using UnityEngine;
using System;
using System.Runtime.InteropServices;

public class CEFTest : MonoBehaviour {
    [DllImport("libsynthesis_cef_wrapper.dylib")]
    private static extern IntPtr CreateOffscreenCefClientInterop(int width, int height);

    [DllImport("libsynthesis_cef_wrapper.dylib")]
    private static extern void DestroyOffscreenCefClientInterop(IntPtr interop);

    private void Start() {
        IntPtr client = CreateOffscreenCefClientInterop(1280, 720);
        Debug.Log("Client: " + client);
        DestroyOffscreenCefClientInterop(client);

        Debug.Log("Done");
    }
}
