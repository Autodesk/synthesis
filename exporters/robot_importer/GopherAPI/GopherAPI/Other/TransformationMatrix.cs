using System;
using System.Collections.Generic;
using System.IO;
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

        public byte[] Binary
        {
            get
            {
                var stream = new MemoryStream();
                using (var writer = new BinaryWriter(stream))
                {
                    foreach (var f in values)
                    {
                        writer.Write(f);
                    }
                }
                var ret = stream.ToArray();
                stream.Dispose();
                return ret;
            }
        }

        public TransformationMatrix()
        {
            values = new float[] { -1f, -1f, -1f, -1f, -1f, -1f, -1f, -1f, -1f, -1f, -1f, -1f, -1f, -1f, -1f, -1f };
        }
    }
}
