using GopherAPI.STL;

namespace GopherAPI
{
    public struct DriveTrain
    {
        public readonly Wheel[] Wheels;
        public readonly STLMesh[] DriveBase;

        public DriveTrain(Wheel[] wheels, STLMesh[] driveBase)
        {
            Wheels = wheels;
            DriveBase = driveBase;
        }
    }
}
