using SynthesisAPI.EventBus;
using UnityEngine;

namespace Utilities.ColorManager {
    public class EnviornmentColorUpdater : MonoBehaviour {
        private Material _skyboxMaterial;
        private Material _floorMaterial;

        [SerializeField]
        private GameObject _skybox;
        [SerializeField]
        private GameObject _floor;

        private static readonly int BOTTOM_COLOR = Shader.PropertyToID("_BottomColor");
        private static readonly int TOP_COLOR    = Shader.PropertyToID("_TopColor");
        private static readonly int GRID_COLOR   = Shader.PropertyToID("_GridColor");

        void Start() {
            _skyboxMaterial = _skybox.GetComponent<MeshRenderer>().material;
            _floorMaterial  = _floor.GetComponent<MeshRenderer>().material;

            AssignColors();

            EventBus.NewTypeListener<ColorManager.OnThemeChanged>(x => { AssignColors(); });
        }

        private void AssignColors() {
            _skyboxMaterial.SetColor(TOP_COLOR, ColorManager.GetColor(ColorManager.SynthesisColor.SkyboxTop));
            _skyboxMaterial.SetColor(BOTTOM_COLOR, ColorManager.GetColor(ColorManager.SynthesisColor.SkyboxBottom));

            _floorMaterial.SetColor(GRID_COLOR, ColorManager.GetColor(ColorManager.SynthesisColor.FloorGrid));
        }
    }
}
