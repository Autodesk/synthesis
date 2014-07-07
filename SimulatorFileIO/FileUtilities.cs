using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

class FileUtilities
{
    public static string sanatizeFileName(string fName, char sanity = '_')
    {
        foreach (char c in System.IO.Path.GetInvalidFileNameChars())
        {
            fName = fName.Replace(c, sanity);
        }
        return fName;
    }
}
