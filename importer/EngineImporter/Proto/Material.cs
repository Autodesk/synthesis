using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;
using Logger = Synthesis.Import.Logger;

using UMaterial = UnityEngine.Material;

namespace Synthesis.Proto {
    /// <summary>
    /// Partial class to add utility functions and properties to Protobuf types
    /// </summary>
    public partial class Material {
        public static implicit operator UMaterial(Material m) {
            Color c = new Color32((byte)m.Red, (byte)m.Green, (byte)m.Blue, (byte)m.Alpha);
            // Logger.Log($"Color: {m.Red}, {m.Green}, {m.Blue}, {m.Alpha}, Spec: {m.Specular}");
            // if (m.Emissive)
            //     Logger.Log("Emissive is true and shouldn't be");
            // string shaderType = m.Emissive
            //     ? "Standard"
            //     : (m.Alpha != 255 ? "Transparent/" : "") + (m.Specular > 0 ? "Specular" : "Diffuse");
            // Logger.Log($"Shader Type: {shaderType}");
            // UMaterial mat = new UMaterial(Shader.Find(shaderType));
            // mat.SetColor("_Color", c);
            // if (m.Specular > 0) {
            //     mat.SetFloat("_Shininess", m.Specular);
            //     mat.SetColor("_SpecColor", c);
            // }
            // if (m.Emissive) {
            //     mat.EnableKeyword("_EMISSION");
            //     mat.SetColor("_EmissionColor", Color.black);
            // }
            // return mat;

            // TODO: Something here breaks transparent materials for builds
            var mat = new UMaterial(Shader.Find("Standard"));
            mat.SetColor("_Color", c);
            if (c.a < 1.0f) {
                mat.SetOverrideTag("RenderTag", "Transparent");
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.DisableKeyword("_ALPHATEST_ON");
                mat.EnableKeyword("_ALPHABLEND_ON");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.renderQueue = 3000;
            }
            mat.SetFloat("_Roughness", 1.0f - m.Specular);
            return mat;
        }

        public static explicit operator Material(BXDAMesh.BXDASurface surface) {
            Material mat = new Material();
            if (surface.hasColor) {
                mat.Red = (int)(surface.color & 0xFF);
                mat.Green = (int)((surface.color >> 8) & 0xFF);
                mat.Blue = (int)((surface.color >> 16) & 0xFF);
                mat.Alpha = (int)((surface.color >> 24) & 0xFF);
                if (surface.transparency != 0)
                    mat.Alpha = (int)(surface.transparency * 255f);
                else if (surface.translucency != 0)
                    mat.Alpha = (int)(surface.translucency * 255f);
                if (mat.Alpha == 0) // No invisible objects
                    mat.Alpha = 10;
                mat.Emissive = false;
                mat.Specular = surface.specular;
            } else {
                // Default Color
                mat = new Material() { Red = 7, Green = 7, Blue = 7, Alpha = 255, Emissive = false, Specular = 0.0f };
            }

            return mat;
        }
    }
}
