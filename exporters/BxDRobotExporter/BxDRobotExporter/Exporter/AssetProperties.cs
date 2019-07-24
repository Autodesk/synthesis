using System;
using Inventor;

namespace BxDRobotExporter.Exporter
{
    public class AssetProperties
    {
        public uint Color = 0xFFFFFFFF;
        public double Transparency = 0;
        public double Translucency = 0;
        public double Specular = 0;

        public AssetProperties() { }

        public AssetProperties(Asset asset)
        {
            try
            {
                Color tempColor = ((ColorAssetValue)asset["generic_diffuse"]).Value;
                Color = ((uint)tempColor.Red << 0) | ((uint)tempColor.Green << 8) | ((uint)tempColor.Blue << 16) | ((((uint)(tempColor.Opacity * 255)) & 0xFF) << 24);
            }
            catch (ArgumentException) { Color = 0xFFFFFFFF; }

            try { Transparency = ((FloatAssetValue)asset["generic_transparency"]).Value; }
            catch (ArgumentException) { Transparency = 0; }

            try { Translucency = ((FloatAssetValue)asset["generic_refraction_translucency_weight"]).Value; }
            catch (ArgumentException) { Translucency = 0; }

            try { Specular = ((FloatAssetValue)asset["generic_reflectivity_at_0deg"]).Value; }
            catch (ArgumentException) { Specular = 0.2; }
        }
    }
}
