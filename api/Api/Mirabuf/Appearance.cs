using UnityEngine;
using UMaterial = UnityEngine.Material;

namespace Mirabuf.Material {
    public partial class Appearance {
        private UMaterial _unityMaterial = null;
        public UMaterial UnityMaterial {
            get {
                if (_unityMaterial == null) {
                    Color32 c = new Color32((byte)Albedo.R, (byte)Albedo.G, (byte)Albedo.B, (byte)Albedo.A);
                    
                    // TODO: Something here breaks transparent materials for builds
                    _unityMaterial = new UMaterial(Shader.Find("Standard"));
                    _unityMaterial.SetColor("_Color", c);
                    if (c.a < 1.0f) {
                        _unityMaterial.SetOverrideTag("RenderTag", "Transparent");
                        _unityMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                        _unityMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                        _unityMaterial.SetInt("_ZWrite", 0);
                        _unityMaterial.DisableKeyword("_ALPHATEST_ON");
                        _unityMaterial.EnableKeyword("_ALPHABLEND_ON");
                        _unityMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                        _unityMaterial.renderQueue = 3000;
                    }
                    _unityMaterial.SetFloat("_Roughness", (float)Roughness);
                    _unityMaterial.enableInstancing = true;
                    // TODO: Specular and Metallic
                }
                return _unityMaterial;
            }
        }

        public static readonly Appearance DefaultAppearance = new Appearance {
            Albedo = new Color { A = 255, R = 12, G = 12, B = 12 },
            Roughness = 0.7,
            Metallic = 0.1,
            Specular = 0.1
        };
    }
}
