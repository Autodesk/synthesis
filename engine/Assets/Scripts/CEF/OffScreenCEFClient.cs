using System;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;
using Xilium.CefGlue;

public class OffScreenCEFClient : CefClient {
    private readonly OffScreenCEFRenderHandler _renderHandler;
    private readonly OffScreenCEFLoadHandler _loadHandler;

    private readonly ReadWriteLockerSlim _browserLock;

    private static byte[] _browserTextureBytes;

    private static CefBrowserHost _browserHost;

    public OffScreenCEFClient(int width, int height) {
        _renderHandler = new OffScreenCEFRenderHandler(width, height);
        _loadHandler = new OffScreenCEFLoadHandler();

        _browserLock = new ReadWriteLockerSlim();

        if (_browserTextureBytes == null) {
            _browserTextureBytes = new byte[width * height * 4];
        }

        Debug.Log("OffScreenCEFClient created"); // TODO: remove
    }

    public void UpdateTexture(Texture2D texture) {
        if (_browserHost == null) {
            return;
        }

        _browserLock.EnterReadLock();
        try {
            texture.LoadRawTextureData(_browserTextureBytes);
            texture.Apply();
        } finally {
            _browserLock.ExitReadLock();
        }
    }

    public void ShutDown() {
        if (_browserHost != null) {
            _browserHost.Dispose();
        }
    }

    protected override CefRenderHandler GetRenderHandler() {
        return _renderHandler;
    }

    protected override CefLoadHandler GetLoadHandler() {
        return _loadHandler;
    }

    protected override bool GetScreenInfo(CefBrowser browser, CefScreenInfo screenInfo) {
        return false; // TODO?
    }

    protected override void OnCursorChange(CefBrowser browser, IntPtr cursorHandle, CefCursorType type, CefCursorInfo customCursorInfo) {
        // TODO
    }

    protected override void OnPopupSize(CefBrowser browser, CefRect rect) {
        // TODO
    }

    protected override void OnScrollOffsetChanged(CefBrowser browser, double x, double y) {
        // TODO
    }

    internal class OffScreenCEFLoadHandler : CefLoadHandler {
        protected override void OnLoadStart(CefBrowser browser, CefFrame frame) {
            if (browser != null) {
                _browserHost = browser.GetHost();
            }
        }
    }

    internal class OffScreenCEFRenderHandler : CefRenderHandler {
        private readonly int _width;
        private readonly int _height;

        public OffScreenCEFRenderHandler(int width, int height) {
            _width = width;
            _height = height;
        }

        protected override bool GetRootScreenRect(CefBrowser browser, ref CefRectangle rect) {
            return GetViewRect(browser, ref rect);
        }

        protected override bool GetViewRect(CefBrowser browser, ref CefRectangle rect) {
            rect.X = 0;
            rect.Y = 0;
            rect.Width = _width;
            rect.Height = _height;
            return true;
        }

        protected override bool GetScreenPoint(CefBrowser browser, int viewX, int viewY, ref int screenX, ref int screenY) {
            screenX = viewX;
            screenY = viewY;
            return true;
        }

        protected override void OnPaint(CefBrowser browser, CefPaintElementType type, CefRectangle[] dirtyRects, IntPtr buffer, int width, int height) {
            if (browser == null) {
                return;
            }

            _browserLock.EnterWriteLock();
            try {
                Marshal.Copy(buffer, _browserTextureBytes, 0, _browserTextureBytes.Length);
            } finally {
                _browserLock.ExitWriteLock();
            }
        }
    }
}
