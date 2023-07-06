using System.Collections;
using UnityEngine;

namespace Synthesis.CEF {
    public class GetBrowserTexture : MonoBehaviour {
        public OffScreenCEFBrowser Browser;

        private Material _material;

        private void Start() {
            _material = GetComponent<MeshRenderer>().material;
			
			// Map the material so it's facing the correct way. 
			_material.mainTextureScale = new Vector2(1.0f, -1.0f);
			_material.mainTextureOffset = new Vector2(0.0f, 1.0f);
            _material.SetTexture("_MainTex", Browser.BrowserTexture);
        }
    }
} // namespace Synthesis.CEF
