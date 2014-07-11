/******************************************************************************
 *
 *    MIConvexHull, Copyright (C) 2013 David Sehnal, Matthew Campbell
 *
 *  This library is free software; you can redistribute it and/or modify it 
 *  under the terms of  the GNU Lesser General Public License as published by 
 *  the Free Software Foundation; either version 2.1 of the License, or 
 *  (at your option) any later version.
 *
 *  This library is distributed in the hope that it will be useful, 
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of 
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Lesser 
 *  General Public License for more details.
 *  
 *****************************************************************************/

namespace MIConvexHull
{
    /// <summary>
    /// A convex face representation containing adjacency information.
    /// </summary>
    public abstract class ConvexFace<TVertex, TFace>
        where TVertex : IVertex
        where TFace : ConvexFace<TVertex, TFace>
    {
        /// <summary>
        /// Adjacency. Array of length "dimension".
        /// If F = Adjacency[i] then the vertices shared with F are Vertices[j] where j != i.
        /// In the context of triangulation, can be null (indicates the cell is at boundary).
        /// </summary>
        public TFace[] Adjacency
        {
            get;
            set;
        }

        /// <summary>
        /// The vertices stored in clockwise order (for dimensions 2 - 4, higher dimensions the order is arbitrary).
        /// Unless I accidentally switch some index somewhere in which case the order is CCW. Either way, it is consistent.
        /// 3D Normal = (V[1] - V[0]) x (V[2] - V[1]).
        /// </summary>
        public TVertex[] Vertices
        {
            get;
            set;
        }

        /// <summary>
        /// The normal vector of the face. Null if used in triangulation.
        /// </summary>
        public double[] Normal
        {
            get;
            set;
        }
    }

    /// <summary>
    /// A default convex face representation.
    /// </summary>
    /// <typeparam name="TVertex"></typeparam>
    public class DefaultConvexFace<TVertex> : ConvexFace<TVertex, DefaultConvexFace<TVertex>>, System.IComparable<DefaultConvexFace<TVertex>>
        where TVertex : IVertex
    {
        private double areaSQ = -1;
        public double getAreaSQ(bool force = false)
        {
            if (areaSQ == -1 || force)
            {
                double[] a = Constants.subtract(Vertices[1].Position, Vertices[0].Position, Vertices[0].Position.Length);
                double[] b = Constants.subtract(Vertices[2].Position, Vertices[0].Position, Vertices[0].Position.Length);
                double[] res = new double[]{
                    (a[1]*b[2]) - (a[2]*b[1]),
                    (a[2]*b[0]) - (a[0]*b[2]),
                    (a[0]*b[1]) - (a[1]*b[0])
                };
                areaSQ = Constants.normSq(res, res.Length) / 4.0;
            }
            return areaSQ;
        }

        public int CompareTo(DefaultConvexFace<TVertex> v)
        {
            return getAreaSQ().CompareTo(v.getAreaSQ());
        }
    }
}
