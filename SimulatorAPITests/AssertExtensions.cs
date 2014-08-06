using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

public class AssertExtensions
{
    public static void AreEqual<T>(T[] a, T[] b, int bStartInA = 0, string prefix = null)
    {
        if (prefix == null)
        {
            prefix = typeof(T).Name + "[]";
        }

        Assert.AreEqual(a.Length, b.Length + bStartInA, prefix + ": Length not equal");
        for (int i = 0; i < b.Length; i++)
        {
            Assert.AreEqual(a[i + bStartInA], b[i], ": Value " + i + " not equal");
        }
    }
}