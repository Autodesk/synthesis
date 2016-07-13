using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public static class Extensions
{
	public static void AddAll<TKey, TValue>(this Dictionary<TKey, TValue> d, Dictionary<TKey, TValue> value)
	{
		foreach (KeyValuePair<TKey, TValue> k in value)
		{
			d.Add(k.Key, k.Value);
		}
	}

    public static Texture2D LoadPNG(string filePath)
    {

        Texture2D tex = null;
        byte[] fileData;

        if (File.Exists(filePath))
        {
            fileData = File.ReadAllBytes(filePath);
            tex = new Texture2D(2, 2);
            tex.LoadImage(fileData); //..this will auto-resize the texture dimensions.
        }
        return tex;
    }
}

