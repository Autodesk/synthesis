using UnityEngine;
using UMaterial = UnityEngine.Material;
using SynthesisAPI.Utilities;

using Logger = SynthesisAPI.Utilities.Logger;

namespace Mirabuf.Material {
    public partial class Appearance {
        private UMaterial _unityMaterial = null;
        public UMaterial UnityMaterial {
            get {
                if (_unityMaterial == null) {
                    Color32 c = new Color32((byte)Albedo.R, (byte)Albedo.G, (byte)Albedo.B, (byte)Albedo.A);
                    // Logger.Log($"[{c.r}, {c.g}, {c.b}, {c.a}]");
                    
                    // TODO: Something here breaks transparent materials for builds
                    
                    if (c.a < 1.0f) {
                        _unityMaterial = new UMaterial(DefaultTransparentShader);
                        _unityMaterial.SetColor(TRANSPARENT_COLOR, c);
                        _unityMaterial.SetFloat(TRANSPARENT_SMOOTHNESS, 1 - (float)Roughness);
                        // _unityMaterial.SetColor("Color", c);
                        // _unityMaterial.SetFloat("Alpha", c.a);
                        //_unityMaterial.SetOverrideTag("RenderTag", "Transparent");
                        //_unityMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                        //_unityMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                        //_unityMaterial.SetInt("_ZWrite", 0);
                        //_unityMaterial.DisableKeyword("_ALPHATEST_ON");
                        //_unityMaterial.EnableKeyword("_ALPHABLEND_ON");
                        //_unityMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                        // _unityMaterial.renderQueue = 3000;
                    } else {
                        _unityMaterial = new UMaterial(DefaultOpaqueShader);
                        _unityMaterial.SetColor(OPAQUE_COLOR, c);
                        _unityMaterial.SetFloat(OPAQUE_SMOOTHNESS, 1 - (float)Roughness);
                        _unityMaterial.SetFloat(OPAQUE_METALLIC, (float)Metallic);
                    }
                    
                    ///
                    /// To enable the use of the renderQueue use the following:
                    ///     _unityMaterial.renderQueue = 3000;
                    /// 
                    /// It was disabled because it make the Grid shader work
                    ///

                    _unityMaterial.enableInstancing = true;
                    // TODO: Specular and Metallic
                }
                return _unityMaterial;
            }
        }

        public const string OPAQUE_COLOR = "Color_2aa135b32e7e4808b9be05c544657380";
        public const string OPAQUE_SMOOTHNESS = "Vector1_dd87d7fcd1f1419f894566001d248ab9";
        public const string OPAQUE_METALLIC = "OPAQUE_METALLIC";
        private static Shader _defaultOpaqueShader = null;
        public static Shader DefaultOpaqueShader {
            get {
                if (_defaultOpaqueShader == null) {
                    _defaultOpaqueShader = Shader.Find("Shader Graphs/DefaultSynthesisShader");
                }
                return _defaultOpaqueShader;
            }
        }
        public const string TRANSPARENT_COLOR = "Color_48545d7793c14f3d9e1dd2264f072068";
        public const string TRANSPARENT_SMOOTHNESS = "Vector1_d66a0e8b289a457c85b3b4408b4f3c2f";
        private static Shader _defaultTransparentShader = null;
        public static Shader DefaultTransparentShader {
            get {
                if (_defaultTransparentShader == null) {
                    _defaultTransparentShader = Shader.Find("Shader Graphs/DefaultSynthesisTransparentShader");
                }
                return _defaultTransparentShader;
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
