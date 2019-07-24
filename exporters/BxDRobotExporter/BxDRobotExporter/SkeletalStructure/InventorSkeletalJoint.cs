
namespace BxDRobotExporter.SkeletalStructure
{
    public interface INventorSkeletalJoint
    {
        SkeletalJoint GetWrapped();

        void DetermineLimits();
        void ReloadInventorJoint();
    }
}
