using System;

namespace GopherAPI.Other
{
    /// <summary>
    /// 
    /// </summary>
    public class Vec3
    {
        private float[] values;
        public float this[int index]
        {
            get { return values[index]; }
            set { values[index] = (float)value; }
        }
        public Vec3()
        {
            values = new float[] { 0f, 0f, 0f };
        }
        public Vec3(float[] vec)
        {
            if (vec.Length == 3)
                values = vec;
            else
                throw new Exception("ERROR: Expected 3 floats but got " + vec.Length.ToString());
        }
        public Vec3(float x, float y, float z)
        {
            values = new float[] { x, y, z };
        }
    }
}
