
public class RigidNode : RigidNode_Base
{
    public CustomRigidGroup group;

    public RigidNode()
        : this(null)
    {
    }
    public RigidNode(CustomRigidGroup grp)
    {
        this.group = grp;
    }

    public override object GetModel()
    {
        return group;
    }
}