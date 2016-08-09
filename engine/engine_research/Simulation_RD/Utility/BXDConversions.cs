using OpenTK;

namespace Simulation_RD.Utility
{
    /// <summary>
    /// Things that convert BXD into Bullet/OpenGL
    /// </summary>
    public static class BXDConversions
    {
        /// <summary>
        /// BXD Vector3 -> OpenGL Vector3
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static Vector3 Convert(this BXDVector3 val)
        {
            return new Vector3(val.x, val.y, val.z);
        }
    }
}
