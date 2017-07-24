using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using GopherAPI.Other;

namespace GopherAPI.STL
{
    public class STLMesh
    {
        internal readonly UInt32 MeshID;

        /// <summary>
        /// An array of all the facets of the mesh
        /// </summary>
        public readonly Facet[] Facets;

        /// <summary>
        /// An array of all the verteces of all the Facets
        /// </summary>
        public Vec3[] VertexArray
        {
            get
            {
                List<Vec3> ret = new List<Vec3>();
                foreach (var facet in Facets)
                {
                    ret.Add(facet.Point1);
                    ret.Add(facet.Point2);
                    ret.Add(facet.Point3);
                }
                return ret.ToArray();
            }
        }

        /// <summary>
        /// An array of all the normals of all the Facets
        /// </summary>
        public Vec3[] NormalArray
        {
            get
            {
                List<Vec3> ret = new List<Vec3>();
                foreach (var facet in Facets)
                    ret.Add(facet.Normal);
                return ret.ToArray();
            }
        }

        /// <summary>
        /// If IsDefault is true, this will be ignored and the color will be set to the bumper color
        /// </summary>
        public readonly Color PartColor;
        /// <summary>
        /// True only if the mesh is part of the bumper
        /// </summary>
        public readonly bool IsDefault;
        /// <summary>
        /// Associates a mesh with a specific attribute (as of right now that just means hitboxes). 
        /// As it is not used for .robot files, the reader will always assign it to UInt32.MaxValue;
        /// </summary>
        internal readonly UInt32 AttributeID;

        public readonly TransformationMatrix TransMat;

        /// <summary>
        /// Returns Facets.Length
        /// </summary>
        public int Size { get => Facets.Length; }

        public STLMesh(UInt32 meshID, Facet[] facets, Color partColor, bool isDefault,
            UInt32 attributeID, TransformationMatrix transMat)
        {
            MeshID = meshID;
            Facets = facets;
            PartColor = partColor;
            IsDefault = isDefault;
            AttributeID = attributeID;
            TransMat = transMat;
        }
        public STLMesh(UInt32 meshID, STLFile stl, UInt32 attributeID, TransformationMatrix transMat)
        {
            MeshID = meshID;
            Facets = stl.Facets;
            PartColor = stl.Color;
            IsDefault = stl.IsDefault;
            AttributeID = attributeID;
            TransMat = transMat;
        }
    }
}
