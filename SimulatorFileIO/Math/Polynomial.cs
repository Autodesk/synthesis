using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class Polynomial : RWObject
{
    public float[] coeff;

    public Polynomial(params float[] coeff)
    {
        this.coeff = coeff;
    }

    public float Evaluate(float x)
    {
        float result = 0;
        float pow = 1;
        for (int i = 0; i < coeff.Length; i++)
        {
            result += coeff[i] * pow;
            pow *= x;
        }
        return result;
    }

    public void WriteData(System.IO.BinaryWriter writer)
    {
        writer.Write(coeff.Length);
        for (int i = 0; i < coeff.Length; i++)
        {
            writer.Write(coeff[i]);
        }
    }

    public void ReadData(System.IO.BinaryReader reader)
    {
        coeff = new float[reader.ReadInt32()];
        for (int i = 0; i < coeff.Length; i++)
        {
            coeff[i] = reader.ReadSingle();
        }
    }
}
