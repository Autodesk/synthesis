using GopherAPI.STL;

namespace GopherAPI
{
    public struct DriveTrain
    {
        public readonly Wheel[] Wheels;
        public readonly STLMesh[] DriveBase;

        public bool Contains(STLMesh mesh)
        {
            foreach (var wheel in Wheels)
            {
                if (wheel.Mesh.Equals(mesh))
                    return true;
            }
            foreach (var mesh2 in DriveBase)
            {
                if (mesh2.Equals(mesh))
                    return true;
            }
            return false;
        }

        public DriveTrain(Wheel[] wheels, STLMesh[] driveBase)
        {
            Wheels = wheels;
            DriveBase = driveBase;
        }
    }
}
