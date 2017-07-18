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
        static void Main(string[] args)
        {
            FieldReader reader;
            Stopwatch sw = Stopwatch.StartNew();
            Console.Write(sw.ElapsedMilliseconds.ToString() + ": ");
            Console.Write("Loading File into memory...");
            try
            {
                reader = new FieldReader(@"C:\Users\t_howab\Desktop\Fields\test7.field");
            }
            catch(Exception e)
            {
                Console.WriteLine("FAILED");
                Console.WriteLine(e.ToString());
                Console.Read();
                return;
            }
            Console.WriteLine("DONE");
            Console.Write(sw.ElapsedMilliseconds.ToString() + ": ");
            Console.Write("Slicing and Dicing...");
            try
            {
                reader.PreProcess();

            }
            catch (Exception e)
            {
                Console.WriteLine("FAILED");
                Console.Read();
                return;
            }
            Console.WriteLine("DONE");
            Console.Write(sw.ElapsedMilliseconds.ToString() + ": ");
            Console.Write("Processing Image...");
            try
            {
                reader.ProcessImage();
            }
            catch (Exception e)
            {
                Console.WriteLine("FAILED");
                Console.Read();
                return;
            }
            Console.WriteLine("DONE");
            Console.WriteLine(sw.ElapsedMilliseconds.ToString() + ": ");
            Console.WriteLine("Slicing and Dicing Twice...");
            try
            {
                reader.PreProcessSTL();
            }
            catch (Exception e)
            {
                Console.WriteLine("FAILED");
                Console.Read();
                return;
            }
            Console.WriteLine("DONE");

        }
    }
}