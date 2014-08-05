using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class Polynomial
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
}
