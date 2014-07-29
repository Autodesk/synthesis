using System.IO;

//find proper specs for these
public enum PneumaticDiameter : byte
{
    HIGH = 0,
    MEDIUM = 1,
    LOW = 2
}

public enum PneumaticPressure : byte
{
    HIGH = 0,
    MEDIUM = 1,
    LOW = 2
}

public class PneumaticDriverMeta : JointDriverMeta
{
   /// <summary>
   /// Stores the variables concerning a wheel, such as its position (which may be removed later) and radius.  
   /// </summary>

   public PneumaticType type
   {
       get;
       set;
   }

   public float widthMM
   {
       get;
       set;
   }

   public float pressurePSI
   {
       get;
       set;
   }

   public BXDVector3 center
   {
       get;
       set;
   }


   public PneumaticDriverMeta()
   {
       center = new BXDVector3();
   }

   //Writes the position of the wheel to the file.
   protected override void WriteDataInternal(BinaryWriter writer)
   {
       writer.Write((byte)((int)type));

       writer.Write(center.x);
       writer.Write(center.y);
       writer.Write(center.z);

       writer.Write(widthMM);
   }

   //Reads the position of the wheel from the file.
   protected override void ReadDataInternal(BinaryReader reader)
   {        
       type = (PneumaticType)reader.ReadByte();
       widthMM = reader.ReadSingle();

       center.x = reader.ReadSingle();
       center.y = reader.ReadSingle();
       center.z = reader.ReadSingle();

       widthMM = reader.ReadSingle();
   }

   public string GetTypeString()
   {
       switch (type)
       {
           case WheelType.OMNI:
               return "Omni Wheel";
           case WheelType.MECANUM:
               return "Mecanum";
           default:
               return "Normal";
       }
   }

   public override string ToString()
   {
       return "WheelMeta[rad=" + radius + "]";
   }