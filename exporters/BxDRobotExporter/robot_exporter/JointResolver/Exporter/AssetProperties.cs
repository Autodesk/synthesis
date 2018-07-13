using System;
using System.Collections.Generic;
using Inventor;

public class AssetProperties
{
    public uint color = 0;
    public double transparency = 0;
    public double translucency = 0;
    public double specular = 0;

    public AssetProperties()
    {
        
    }

    public AssetProperties(Asset asset)
    {
        CopyAsset(asset);
    }

    public void CopyAsset(Asset asset)
    {
        foreach (AssetValue val in asset)
        {
            //is this one supposed to be different from the others? "DisplayName vs Name"
            if (val.DisplayName.Equals("Color") && val.ValueType == AssetValueTypeEnum.kAssetValueTypeColor)
            {
                Color tempColor = ((ColorAssetValue)val).Value;
                color = ((uint)tempColor.Red << 0) | ((uint)tempColor.Green << 8) | ((uint)tempColor.Blue << 16) | ((((uint)(tempColor.Opacity * 255)) & 0xFF) << 24);
            }
            /*    //I am unable to find any reference to gloss in the API, and I've found the value changes from material to material
            else if (val.Name.Equals("generic_glossiness") && val.ValueType == AssetValueTypeEnum.kAssetValueTypeFloat)
            {
                generic_glossiness = ((FloatAssetValue)val).Value;
            }
            */
            //opacity is a double
            else if (val.DisplayName.Equals("Transparency") && val.ValueType == AssetValueTypeEnum.kAssetValueTypeFloat)
            {
                transparency = ((FloatAssetValue)val).Value;
            }
            else if (val.DisplayName.Equals("Translucency") && val.ValueType == AssetValueTypeEnum.kAssetValueTypeFloat)
            {
                translucency = ((FloatAssetValue)val).Value;
            }
            else if (val.ValueType == AssetValueTypeEnum.kAssetValueTextureType)
            {
                AssetTexture tex = ((TextureAssetValue)val).Value;
            }
            else if (val.Name.Contains("reflectivity") && val.Name.Contains("0deg") && val.ValueType == AssetValueTypeEnum.kAssetValueTypeFloat)
            {
                specular = ((FloatAssetValue)val).Value;
            }
        }
    }

    public static AssetProperties Create(dynamic surf)
    {
        try
        {
            return new AssetProperties(surf.Appearance);
        }
        catch
        {
            try
            {
                return new AssetProperties(surf.Parent.Appearance);
            }
            catch
            {
                try
                {
                    return new AssetProperties(surf.Parent.Parent.Appearance);
                }
                catch
                {
                    return null;
                }
            }
        }
    }

    public override bool Equals(object other)
    {
        if (other is AssetProperties otherAsset)
            return color == otherAsset.color && transparency == otherAsset.transparency && translucency == otherAsset.translucency && specular == otherAsset.specular;
        else
            return base.Equals(other);
    }
}
