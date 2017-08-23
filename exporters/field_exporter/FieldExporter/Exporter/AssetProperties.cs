using Inventor;

public class AssetProperties
{
    public Color color = null;
    public double transparency;
    public double translucency;
    public double specular = 0;

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

    public AssetProperties(Asset asset)
    {
        foreach (AssetValue val in asset)
        {
            //is this one supposed to be different from the others? "DisplayName vs Name"
            if (val.DisplayName.Equals("Color") && val.ValueType == AssetValueTypeEnum.kAssetValueTypeColor)
            {
                color = ((ColorAssetValue) val).Value;
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
                transparency = ((FloatAssetValue) val).Value;
            }
            else if (val.DisplayName.Equals("Translucency") && val.ValueType == AssetValueTypeEnum.kAssetValueTypeFloat)
            {
                translucency = ((FloatAssetValue) val).Value;
            }
            else if (val.ValueType == AssetValueTypeEnum.kAssetValueTextureType)
            {
                AssetTexture tex = ((TextureAssetValue) val).Value;
            }
            else if (val.Name.Contains("reflectivity") && val.Name.Contains("0deg") && val.ValueType == AssetValueTypeEnum.kAssetValueTypeFloat)
            {
                specular = ((FloatAssetValue) val).Value;
            }
        }
    }
    
    /// <summary>
    /// Used for determing if two AssetProperties are identical.
    /// </summary>
    /// <param name="props"></param>
    /// <returns></returns>
    public bool Equals(AssetProperties props)
    {
        return
            props.color.Red == color.Red && props.color.Green == color.Green && props.color.Blue == color.Blue &&
            transparency == props.transparency &&
            translucency == props.translucency &&
            props.specular == specular;
    }
}
