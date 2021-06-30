using System.Text;

public class Polynomial : BinaryRWObject
{
    public float[] coeff;

    public Polynomial()
    {
        coeff = new float[0];
    }

    /// <summary>
    /// Coefficients are based on array indicies, so the first one is multiplied by 1, the second by x, the third by x^2...
    /// </summary>
    /// <param name="coeff">Coeffecients for the polynomial</param>
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

    public void WriteBinaryData(System.IO.BinaryWriter writer)
    {
        writer.WriteArray(coeff);
    }

    public void ReadBinaryData(System.IO.BinaryReader reader)
    {
        coeff = reader.ReadArray<float>();
    }

    public override string ToString()
    {
        StringBuilder result = new StringBuilder();
        for (int i = coeff.Length - 1; i >= 0; i--)
        {
            if (coeff[i] != 0)
            {
                if (result.Length > 0)
                {
                    result.Append(" + ");
                }
                result.Append(coeff[i]);
                if (i > 0)
                {
                    result.Append('x');
                    if (i > 1)
                    {
                        result.Append('^').Append(i);
                    }
                }
            }
        }
        return result.ToString();
    }
}
