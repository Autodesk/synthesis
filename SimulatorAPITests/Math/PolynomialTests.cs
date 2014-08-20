using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

[TestClass]
public class PolynomialTests
{
    [TestMethod]
    public void Polynomial_ReadWrite()
    {
        float[] array = TestUtils.MakeRandomArray<float>(10, (double d) =>
        {
            return (float) d;
        });
        Polynomial poly = new Polynomial((float[]) array.Clone());
        Polynomial result = TestUtils.WriteReadObject(poly);
        AssertExtensions.AreEqual(poly.coeff, result.coeff, 0, "Polynomial coefficients");
    }

    [TestMethod]
    public void Polynomial_Evaluate()
    {
        Polynomial poly = new Polynomial(new float[] { 8E6F, 17, 647 });
        float x = 498.0F;
        float expected = 8E6F + (17.0F * x) + (647.0F * x * x);
        Assert.AreEqual(expected, poly.Evaluate(x), "Polynomial evaluate for quadratic was incorrect!");
    }

    [TestMethod]
    public void Polynomial_ToString()
    {
        Polynomial poly = new Polynomial(new float[] { 3.14f, 2.1f, 1.4f });
        Assert.AreEqual("1.4x^2 + 2.1x + 3.14", poly.ToString(), "Polynomial ToString method failed.");
    }
}
