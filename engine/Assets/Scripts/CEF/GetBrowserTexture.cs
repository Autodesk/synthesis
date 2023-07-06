using System.Collections;
using UnityEngine;

namespace Synthesis.CEF {
    public class GetBrowserTexture : MonoBehaviour {
        public OffScreenCEFBrowser Browser;

        private Material _material;

        private void Start() {
            _material = GetComponent<MeshRenderer>().material;
            _material.SetTexture("_MainTex", Browser.BrowserTexture);
        }
    }
} // namespace Synthesis.CEF
