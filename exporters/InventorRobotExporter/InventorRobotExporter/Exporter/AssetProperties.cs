using System;
using Inventor;

namespace InventorRobotExporter.Exporter
{
    public class AssetProperties
    {
        public uint color = 0xFFFFFFFF;
        public double transparency = 0;
        public double translucency = 0;
        public double specular = 0;

        public AssetProperties() { }

        public AssetProperties(Asset asset)
        {
            try
            {
                Color tempColor = ((ColorAssetValue)asset["generic_diffuse"]).Value;
                color = ((uint)tempColor.Red << 0) | ((uint)tempColor.Green << 8) | ((uint)tempColor.Blue << 16) | ((((uint)(tempColor.Opacity * 255)) & 0xFF) << 24);
            }
            catch (ArgumentException) { color = 0xFFFFFFFF; }

            try { transparency = ((FloatAssetValue)asset["generic_transparency"]).Value; }
            catch (ArgumentException) { transparency = 0; }

            try { translucency = ((FloatAssetValue)asset["generic_refraction_translucency_weight"]).Value; }
            catch (ArgumentException) { translucency = 0; }

            try { specular = ((FloatAssetValue)asset["generic_reflectivity_at_0deg"]).Value; }
            catch (ArgumentException) { specular = 0.2; }
        }
    }
}
