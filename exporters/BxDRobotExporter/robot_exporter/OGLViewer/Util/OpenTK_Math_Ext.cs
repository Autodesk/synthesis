using OpenTK;

namespace OGLViewer
{
    public static class OpenTK_Math_Ext
    {

        public static BXDVector3 Multiply(this Matrix4 mat, BXDVector3 v)
        {
            float resX = mat[0, 3] + (v.x * mat[0, 0]) + (v.y * mat[0, 1])
                    + (v.z * mat[0, 2]);
            float resY = mat[1, 3] + (v.x * mat[1, 0]) + (v.y * mat[1, 1])
                    + (v.z * mat[1, 2]);
            float resZ = mat[2, 3] + (v.x * mat[2, 0]) + (v.y * mat[2, 1])
                    + (v.z * mat[2, 2]);
            return new BXDVector3((float)resX, (float)resY, (float)resZ);
        }

        public static BXDVector3 Rotate(this Matrix4 mat, BXDVector3 v)
        {
            float resX = (v.x * mat[0, 0]) + (v.y * mat[0, 1])
                    + (v.z * mat[0, 2]);
            float resY = (v.x * mat[1, 0]) + (v.y * mat[1, 1])
                    + (v.z * mat[1, 2]);
            float resZ = (v.x * mat[2, 0]) + (v.y * mat[2, 1])
                    + (v.z * mat[2, 2]);
            return new BXDVector3((float)resX, (float)resY, (float)resZ);
        }

        public static Vector3 ToTK(this BXDVector3 vec)
        {
            return new Vector3(vec.x, vec.y, vec.z);
        }

        public static BXDVector3 ToBXD(this Vector3 vec)
        {
            return new BXDVector3(vec.X, vec.Y, vec.Z);
        }
    }
}
