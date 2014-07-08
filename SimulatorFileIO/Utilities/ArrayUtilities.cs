using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class ArrayUtilities
{
    public delegate T MakeVector2<T>(double x, double y);
    public delegate T MakeVector3<T>(double x, double y, double z);
    public delegate T MakeColor<T>(byte r, byte g, byte b, byte a);

    public static T[] WrapArray<T>(MakeVector2<T> maker, double[] array)
    {
        T[] results = new T[array.Length / 2];
        for (int i = 0, j = 0; i < results.Length; i++, j += 2)
        {
            results[i] = maker(array[j], array[j + 1]);
        }
        return results;
    }

    public static T[] WrapArray<T>(MakeVector3<T> maker, double[] array)
    {
        T[] results = new T[array.Length / 3];
        for (int i = 0, j = 0; i < results.Length; i++, j += 3)
        {
            results[i] = maker(array[j], array[j + 1], array[j + 2]);
        }
        return results;
    }

    public static T[] WrapArray<T>(MakeColor<T> maker, uint[] array)
    {
        T[] results = new T[array.Length];
        for (int i = 0; i < results.Length; i++)
        {
            uint val = array[i];
            results[i] = maker((byte) (val & 0xFF), (byte) ((val >> 8) & 0xFF), (byte) ((val >> 16) & 0xFF), (byte) ((val >> 24) & 0xFF));
        }
        return results;
    }
}