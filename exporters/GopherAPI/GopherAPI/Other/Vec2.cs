using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GopherAPI.Other
{
    /// <summary>
    /// A fixed size vector of 2 floats. I use it for degrees of freedom in joints.
    /// </summary>
    public class Vec2
    {
        private float[] data;
        
        public float this[int i]
        {
            get
            {
                if (i > 1 || i < 0)
                    throw new IndexOutOfRangeException();
                else
                    return data[i];
            }
            set
            {
                if (i > 1 || i < 0)
                    throw new IndexOutOfRangeException();
                else
                    data[i] = value;
            }
        }

        /// <summary>
        /// The x value of the vector
        /// </summary>
        public float X { get => data[0]; set => data[0] = value; }
        /// <summary>
        /// The y value of the vector
        /// </summary>
        public float Y { get => data[1]; set => data[1] = value; }

        public Vec2()
        {
            data = new float[2];
        }

        public Vec2(float[] data)
        {
            if (data.Length != 2)
                throw new ArgumentException("data");
            else
                this.data = data;
        }

        public Vec2(float X, float Y)
        {
            data = new float[] { X, Y };
        }
    }
}
