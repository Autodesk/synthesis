using SynthesisAPI.EnvironmentManager;
using SynthesisAPI.EnvironmentManager.Components;
using UnityEngine;
using Sprite = SynthesisAPI.EnvironmentManager.Components.Sprite;

namespace Engine.ModuleLoader.Adapters {
    public sealed class SpriteAdapter : MonoBehaviour, IApiAdapter<Sprite> {
        private Sprite instance             = null;
        private new SpriteRenderer renderer = null;
        private Material defaultMaterial;
        private bool updated = false;

        public void Awake() {
            if ((renderer = gameObject.GetComponent<SpriteRenderer>()) == null)
                renderer = gameObject.AddComponent<SpriteRenderer>();

            if (instance == null) {
                gameObject.SetActive(false);
            }
            defaultMaterial = renderer.material;
        }

        public void Update() {
            if (updated) {
                var collider = instance.Entity?.GetComponent<MeshCollider2D>();
                if (collider != null) {
                    collider.Changed = true;
                }
                updated = false;
            }
            if (instance.Changed) {
                renderer.sprite = instance._sprite;
                if (instance.Entity?.GetComponent<AlwaysOnTop>() != null)
                    renderer.material = new Material(Shader.Find("Custom/AlwaysOnTop"));
                else
                    renderer.material = defaultMaterial;

                renderer.flipX          = instance._flipX;
                renderer.flipY          = instance._flipY;
                renderer.enabled        = instance._visible;
                instance.Bounds._bounds = renderer.bounds;
                instance.ProcessedChanges();

                updated = true;
            }
        }

        public void SetInstance(Sprite sprite) {
            instance = sprite;
            gameObject.SetActive(true);
        }

        public static Sprite NewInstance() {
            return new Sprite();
        }
    }
}