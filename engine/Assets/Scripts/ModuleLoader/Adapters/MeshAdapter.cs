using System.Collections.Generic;
using System.Linq;
using Engine.Util;
using SynthesisAPI.Utilities;
using UnityEditor;
using UnityEngine;
using Logger = UnityEngine.Logger;
using Mesh   = SynthesisAPI.EnvironmentManager.Components.Mesh;

namespace Engine.ModuleLoader.Adapters {
    public sealed class MeshAdapter : MonoBehaviour, IApiAdapter<Mesh> {
        private Material defaultMaterial = null;

        public void SetInstance(Mesh mesh) {
            instance = mesh;

            if ((filter = gameObject.GetComponent<MeshFilter>()) == null)
                filter = gameObject.AddComponent<MeshFilter>();
            if ((renderer = gameObject.GetComponent<MeshRenderer>()) == null)
                renderer = gameObject.AddComponent<MeshRenderer>();

            if (defaultMaterial == null) {
                var s                 = Shader.Find("Universal Render Pipeline/Lit");
                defaultMaterial       = new Material(s);
                defaultMaterial.color = new Color(0.2f, 0.2f, 0.2f);
                defaultMaterial.SetFloat("_Smoothness", 0.2f);
            }
            renderer.material = defaultMaterial;

            filter.mesh           = new UnityEngine.Mesh();
            filter.mesh.vertices  = Misc.MapAll(instance._vertices, Misc.MapVector3D).ToArray();
            filter.mesh.uv        = Misc.MapAll(instance._uvs, x => new Vector2((float)x.X, (float)x.Y)).ToArray();
            filter.mesh.triangles = instance.Triangles.ToArray();
            filter.mesh.RecalculateNormals();

            instance.PropertyChanged += (s, e) => {
                switch (e.PropertyName.ToLower()) {
                    case "vertices":
                        filter.mesh.vertices = Misc.MapAll(instance._vertices, Misc.MapVector3D).ToArray();
                        break;
                    case "uvs":
                        filter.mesh.uv = Misc.MapAll(instance._uvs, x => new Vector2((float)x.X, (float)x.Y)).ToArray();
                        break;
                    case "triangles":
                        filter.mesh.triangles = instance._triangles.ToArray();
                        break;
                    case "color":
                        renderer.material.color =
                            new Color(instance._color.r, instance._color.g, instance._color.b, instance._color.a);
                        break;
                    case "recalculate":
                        filter.mesh.RecalculateNormals();
                        break;
                    default:
                        SynthesisAPI.Utilities.Logger.Log("Property not setup", LogLevel.Warning);
                        break;
                }
            };
        }

        public static Mesh NewInstance() {
            return new Mesh();
        }

        internal Mesh instance;
        internal MeshFilter filter;
        internal MeshRenderer renderer;
    }
}