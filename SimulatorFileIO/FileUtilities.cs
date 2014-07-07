using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

class FileUtilities
{
    public static string SanatizeFileName(string fileName, char sanity = '_')
    {
        foreach (char c in System.IO.Path.GetInvalidFileNameChars())
        {
            fileName = fileName.Replace(c, sanity);
        }
        return fileName;
    }
}
