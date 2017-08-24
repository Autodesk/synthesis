public enum ElevatorType : byte
{
    NOT_MULTI = 0, //Single stage elevator, constitutes most frc elevator
    CASCADING_STAGE_1 = 1,
    CASCADING_STAGE_2 = 2,
    CONTINUOUS_STAGE_1 = 3,
    CONTINUOUS_STAGE_2 = 4
}
public class ElevatorDriverMeta : JointDriverMeta
{
    public ElevatorType type;

    public ElevatorDriverMeta()
    {
    }

    protected override void WriteDataInternal(System.IO.BinaryWriter writer)
    {
        writer.Write((byte)((int)type));
    }

    protected override void ReadDataInternal(System.IO.BinaryReader reader)
    {
        type = (ElevatorType)reader.ReadByte();
    }

}