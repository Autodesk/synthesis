using GopherAPI.STL;
using GopherAPI.Properties;

namespace GopherAPI
{
    public struct Wheel
    {
        public readonly STLMesh Mesh;
        public readonly Joint Joint;

        public Wheel(STLMesh mesh, Joint joint)
        {
            Mesh = mesh; Joint = joint;
        }
    }
}
