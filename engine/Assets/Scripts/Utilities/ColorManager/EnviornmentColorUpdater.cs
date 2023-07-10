using SynthesisAPI.EventBus;
using UnityEngine;
using UnityEngine.Serialization;

namespace Utilities.ColorManager
{
    public class EnviornmentColorUpdater : MonoBehaviour
    {
        [SerializeField] private Material _skyboxMaterial;
        [SerializeField] private Material _floorMaterial;
        
        private static readonly int BOTTOM_COLOR = Shader.PropertyToID("_BottomColor");
        private static readonly int TOP_COLOR = Shader.PropertyToID("_TopColor");
        private static readonly int GRID_COLOR = Shader.PropertyToID("_GridColor");

        // Start is called before the first frame update
        void Start()
        {
            AssignColors();
        
            EventBus.NewTypeListener<ColorManager.OnThemeChanged>(x => { AssignColors(); });
        }

        private void AssignColors()
        {
            _skyboxMaterial.SetColor(TOP_COLOR, 
                ColorManager.GetColor(ColorManager.SynthesisColor.SkyboxTop));
            _skyboxMaterial.SetColor(BOTTOM_COLOR, 
                ColorManager.GetColor(ColorManager.SynthesisColor.SkyboxBottom));

            var floorColor = ColorManager.GetColor(ColorManager.SynthesisColor.FloorGrid);
            floorColor.a = 0.48f;
            _floorMaterial.SetColor(GRID_COLOR, floorColor);
        }
    }
}
