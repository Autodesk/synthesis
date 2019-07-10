using System;

public class Maybe<T>
{
    private T value;
    private bool isValid;

    public Maybe()
    {
        isValid = false;
    }

    public Maybe(T v)
    {
        value = v;
        isValid = true;
    }

    public bool IsValid()
    {
        return isValid;
    }

    public void Set(T v)
    {
        value = v;
        isValid = true;
    }

    public T Get()
    {
        if (isValid)
        {
            return value;
        }
        throw new Exception("Maybe type invalid");
    }
}