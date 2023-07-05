using System;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;
using Xilium.CefGlue;

public class CEFOffScreenBrowser : MonoBehaviour {
    public Texture2D BrowserTexture;

    private bool _quit = false;
    private static OffScreenCEFClient _client;

    private const int _width  = 1280;
    private const int _height = 720;

    private void Awake() {
        BrowserTexture = new Texture2D(_width, _height, TextureFormat.BGRA32, false);
        CefRuntime.Load();

        CefMainArgs mainArgs = new CefMainArgs(new string[0]);
        OffScreenCEFApp app  = new OffScreenCEFApp();
        CefSettings settings = new CefSettings {
            MultiThreadedMessageLoop   = false,
            SingleProcess              = true,
            WindowlessRenderingEnabled = true,
            NoSandbox                  = true,
        };

        try {
            CefRuntime.Initialize(mainArgs, settings, app, IntPtr.Zero);
        } catch (Exception e) {
            Debug.LogError(e);
        }

        string url                         = "https://www.google.com";
        _client                            = new OffScreenCEFClient(_width, _height);
        CefBrowserSettings browserSettings = new CefBrowserSettings();
        CefWindowInfo windowInfo           = CefWindowInfo.Create();
        windowInfo.SetAsWindowless(IntPtr.Zero, true);
        CefBrowserHost.CreateBrowser(windowInfo, _client, browserSettings, url);
    }

    private void OnDisable() {
        _quit = true;
        _client.ShutDown();
        CefRuntime.Shutdown();
    }

    private IEnumerator RunCEFMessageLoop() {
        while (!_quit) {
            CefRuntime.DoMessageLoopWork();
            _client.UpdateTexture(BrowserTexture);
            yield return new WaitForEndOfFrame();
        }
    }
}
