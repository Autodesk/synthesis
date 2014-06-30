using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Inventor;

class AssetProperties
{
    public Color color = null;
    public double generic_glossiness;
    public double generic_transparency;
    public AssetProperties(Asset asset)
    {
        foreach (AssetValue val in asset)
        {
            if (val.Name.Equals("generic_diffuse") && val.ValueType == AssetValueTypeEnum.kAssetValueTypeColor)
            {
                color = ((ColorAssetValue)val).Value;
            }
            else if (val.Name.Equals("generic_glossiness") && val.ValueType == AssetValueTypeEnum.kAssetValueTypeFloat)
            {
                generic_glossiness = ((FloatAssetValue)val).Value;
            }
            else if (val.Name.Equals("generic_transparency") && val.ValueType == AssetValueTypeEnum.kAssetValueTypeFloat)
            {
                generic_transparency = ((FloatAssetValue)val).Value;
            }
        }
    }
}
