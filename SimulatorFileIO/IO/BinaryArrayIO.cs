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
            len = arr.Length - off;
        if (typeof(RWObject).IsAssignableFrom(typeof(T)))
        {
            writer.Write(len);
            for (int i = off; i < off + len; i++)
            {
                writer.Write((RWObject) arr[i]);
            }
            return;
        }
        else if (typeof(T).IsPrimitive)
        {
            int size = Buffer.ByteLength(new T[1]);
            byte[] res = new byte[len * size];
            Buffer.BlockCopy(arr, off * size, res, 0, res.Length);
            writer.Write(len);
            writer.Write(res);
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

    public static T[] ReadArray<T>(this BinaryReader reader, RWObjectExtensions.ReadObjectFully readInternal = null)
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
        else if (typeof(T).IsPrimitive)
        {
            int len = reader.ReadInt32();
            int size = Buffer.ByteLength(new T[1]);
            byte[] res = new byte[len * size];
            int count = reader.Read(res, 0, res.Length);
            while (count != res.Length)
            {
                int read = reader.Read(res, count, res.Length - count);
                count += read;
                if (read < 0)
                {
                    throw new IOException("Failed to read enough bytes to fill array!\n");
                }
            }
            T[] dest = new T[len];
            Buffer.BlockCopy(res, 0, dest, 0, res.Length);
            return dest;
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
