using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GopherAPI.Reader;
using GopherAPI.STL;
using GopherAPI.Properties;
using GopherAPI.Writer;
using GopherAPI.Other;
using System.Drawing;
using GopherAPI;
using System.IO;
using System.Diagnostics;

namespace GopherTester
{
    class Program
    {
        static void SEPrompt(Exception e)
        {
            Console.Write("Show exception? [Y/N]");
            if (Console.ReadLine() == "Y")
            {
                Console.WriteLine(e.ToString());
                Console.Read();
            }
        }
        static void Main(string[] args)
        {
            FieldReader reader;
            Stopwatch sw = Stopwatch.StartNew();
            Console.Write(sw.ElapsedMilliseconds.ToString() + ": ");
            Console.Write("Loading File into memory...");
            reader = new FieldReader(@"C:\Users\t_howab\Desktop\Fields\test8.field");
            Console.WriteLine("DONE");
            Console.Write(sw.ElapsedMilliseconds.ToString() + ": ");
            Console.Write("Slicing and Dicing...");
            reader.PreProcess();
            Console.WriteLine("DONE");
            Console.Write(sw.ElapsedMilliseconds.ToString() + ": ");
            Console.Write("Processing Image...");
            reader.ProcessImage();
            Console.WriteLine("DONE");
            Console.WriteLine(sw.ElapsedMilliseconds.ToString() + ": ");
            Console.WriteLine("Slicing and Dicing Twice...");
            reader.PreProcessSTL();
            Console.WriteLine("DONE");

        }
    }
}