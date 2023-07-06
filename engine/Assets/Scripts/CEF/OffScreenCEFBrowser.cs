using System;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;
using Xilium.CefGlue;

namespace Synthesis.CEF {
    public class OffScreenCEFBrowser : MonoBehaviour {
        [HideInInspector]
        public Texture2D BrowserTexture;

        private bool _quit = false;
        private static OffScreenCEFClient _client;

        private const int _width  = 1280;
        private const int _height = 720;

        private void Awake() {
            BrowserTexture = new Texture2D(_width, _height, TextureFormat.RGBA32, false);
            CefRuntime.Load();

            var mainArgs = new CefMainArgs(new string[] {});
            var mainApp  = new OffScreenCEFApp();
            var settings = new CefSettings {
                MultiThreadedMessageLoop   = false,
                SingleProcess              = true,
                WindowlessRenderingEnabled = true,
                NoSandbox                  = true,
            };

            try {
                CefRuntime.Initialize(mainArgs, settings, mainApp, IntPtr.Zero);
            } catch (Exception e) {
                Debug.LogError(e);
            }

            Debug.Log(Application.dataPath + "/Scripts/CEF/HTML/index.html"); // TODO: remove

            string url          = Application.dataPath + "/Scripts/CEF/HTML/index.html";
            _client             = new OffScreenCEFClient(_width, _height);
            var browserSettings = new CefBrowserSettings();
            var windowSettings  = CefWindowInfo.Create();
            windowSettings.SetAsWindowless(IntPtr.Zero, false);
            CefBrowserHost.CreateBrowser(windowSettings, _client, browserSettings, url);

            StartCoroutine("RunCEFMessageLoop");
            DontDestroyOnLoad(gameObject);
        }

        private void Shutdown() {
            _quit = true;
            _client.Shutdown();
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
} // namespace Synthesis.CEF
