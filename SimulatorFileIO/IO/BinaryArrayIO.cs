using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;

public static class BinaryArrayIO
{
    public static void WriteArray<T>(this BinaryWriter writer, T[] arr, int off = 0, int len = -1)
    {
        if (len == -1)
            len = arr.Length;
        if (typeof(RWObject).IsAssignableFrom(typeof(T)))
        {
            writer.Write(len);
            for (int i = off; i < off + len; i++)
            {
                writer.Write((RWObject) arr[i]);
            }
            return;
        }
        else
        {
            MethodInfo tMeth = typeof(BinaryWriter).GetMethod("Write", new Type[] { typeof(T) });
            if (tMeth == null)
            {
                throw new IOException("Unsupported write object.");
            }
            writer.Write(len);
            for (int i = off; i < off + len; i++)
            {
                tMeth.Invoke(writer, new object[] { arr[i] });
            }
        }
    }

    public delegate RWObject ReadRWObject(BinaryReader reader);

    public static T[] ReadArray<T>(this BinaryReader reader, ReadRWObject readInternal = null)
    {
        if (typeof(RWObject).IsAssignableFrom(typeof(T)))
        {
            if (readInternal == null)
            {
                // Try to find a constructor...
                ConstructorInfo ctr = typeof(T).GetConstructor(new Type[0]);
                if (ctr == null)
                {
                    throw new IOException("Can't read array of RWObjects directly!\n");
                }
                else
                {
                    readInternal = (BinaryReader rdr) =>
                    {
                        RWObject ro = (RWObject) ctr.Invoke(new object[0]);
                        ro.ReadData(rdr);
                        return ro;
                    };
                }
            }
            int len = reader.ReadInt32();
            T[] arr = new T[len];
            for (int i = 0; i < len; i++)
            {
                arr[i] = (T) readInternal(reader);
            }
            return arr;
        }
        else
        {
            MethodInfo tMeth = typeof(BinaryReader).GetMethod("Read" + typeof(T).Name);
            if (tMeth == null)
            {
                throw new IOException("Unsupported read object.");
            }
            int len = reader.ReadInt32();
            T[] arr = new T[len];
            for (int i = 0; i < len; i++)
            {
                arr[i] = (T) tMeth.Invoke(reader, new object[0]);
            }
            return arr;
        }
    }
}
