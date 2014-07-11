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
    /// An interface for a structure with nD position.
    /// </summary>
    public interface IVertex
    {
        /// <summary>
        /// Position of the vertex.
        /// </summary>
        double[] Position
        {
            get;
            set;
        }
    }

    /// <summary>
    /// "Default" vertex.
    /// </summary>
    public class DefaultVertex : IVertex
    {
        public int indexNumber = -1;
        public double[] normal;
        public int faceCount;

        /// <summary>
        /// Position of the vertex.
        /// </summary>
        public double[] Position
        {
            get;
            set;
        }

        public override int GetHashCode()
        {
            return (int) (Position[0] * 73856093) ^ (int) (Position[1] * 19349663) ^ (int) (Position[2] * 83492791);
        }

        public override bool Equals(object obj)
        {
            if (obj == this)
            {
                return true;
            }
            else if (obj is IVertex)
            {
                return Constants.SamePosition(Position, ((IVertex) obj).Position, 3);
            }
            else
            {
                return false;
            }
        }
    }

}
