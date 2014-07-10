using System;
using System.Collections.Generic;
using Inventor;

public class AssetProperties
{
    public Color color = null;
    public AssetTexture colorTexture = null;
    public double generic_glossiness;
    public double generic_transparency;

    public AssetProperties(Asset asset)
    {
        foreach (AssetValue val in asset)
        {
            //is this one supposed to be different from the others? "DisplayName vs Name"
            if (val.DisplayName.Equals("Color") && val.ValueType == AssetValueTypeEnum.kAssetValueTypeColor)
            {
                ColorAssetValue colVal = (ColorAssetValue)val;
                color = colVal.Value;
                if (colVal.HasConnectedTexture)
                {
                    colorTexture = colVal.ConnectedTexture;
                    List<object> lst = new List<object>();
                    foreach (object oo in colorTexture)
                    {
                        lst.Add(oo);
                        // "Offset X", "Offset Y", "Size X", "Size Y"
                        // "U Offset", "U Repeat", "U Scale", "UV Scale"
                        // "V Offset", "V Repeat", "V Scale"
                        // "Angle", "Source"
                        // 1/Mats/Finishes.Flooring.Vinyl.Checker.Black-White.jpg
                    }
                    Console.WriteLine(lst.Count);
                }
            }
            /*    //I am unable to find any reference to gloss in the API, and I've found the value changes from material to material
            else if (val.Name.Equals("generic_glossiness") && val.ValueType == AssetValueTypeEnum.kAssetValueTypeFloat)
            {
                generic_glossiness = ((FloatAssetValue)val).Value;
            }
            */
            //opacity is a double
            else if (val.Name.Equals("Opacity") && val.ValueType == AssetValueTypeEnum.kAssetValueTypeFloat)
            {
                generic_transparency = ((FloatAssetValue)val).Value;
            }
            else if (val.ValueType == AssetValueTypeEnum.kAssetValueTextureType)
            {
                AssetTexture tex = ((TextureAssetValue)val).Value;
            }
        }
    }
}
