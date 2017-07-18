using System;
using System.Collections.Generic;
public class Mesh

{
    public readonly UInt32 MeshID;
    public readonly Facet[] Facets;
    //public readonly Color PartColor;
    public readonly bool IsDefault;
    public readonly UInt32 AttributeID;
    public readonly UInt32 TransMat00;
    public readonly UInt32 TransMat33;

    public int Size
    {
        get => Facets.Length;
    }

    public readonly UInt32 numFacets; //number of facets

    String header; //ASCII .stl header

    public Mesh(UInt32 meshID, Facet[] facets, bool isDefault, UInt32 attributeID, UInt32 transMat00, UInt32 transMat33)
	{
        MeshID = meshID;
        Facets = facets;
        IsDefault = isDefault;
        AttributeID = attributeID;
        TransMat00 = transMat00;
        TransMat33 = transMat33;

        
    }
}
