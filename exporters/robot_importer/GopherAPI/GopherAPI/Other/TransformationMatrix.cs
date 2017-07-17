using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GopherAPI.Other
{
    /// <summary>
    /// A simple 4x4 matrix containing floats
    /// </summary>
    public class TransformationMatrix
    {
        private float[] values;
        public object this[int x, int y]
        {
            get { return values[y + (4 * x)]; }
            set { values[y + (4 * x)] = (float)value; }
        }

        public TransformationMatrix()
        {
            values = new float[] { -1f, -1f, -1f, -1f, -1f, -1f, -1f, -1f, -1f, -1f, -1f, -1f, -1f, -1f, -1f, -1f };
        }
    }
}
