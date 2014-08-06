using System;

class TestUtils
{
    public delegate T CreateObjectFromValue<T>(double val);
    public static T[] MakeRandomArray<T>(int len, CreateObjectFromValue<T> create)
    {
        T[] arr = new T[len];
        Random rand = new Random();
        for (int i = 0; i < arr.Length; i++)
        {
            arr[i] = create(rand.NextDouble() * 1000.0);
        }
        return arr;
    }
}
