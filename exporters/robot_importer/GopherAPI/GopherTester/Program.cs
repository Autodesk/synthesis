using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GopherAPI.Reader;
using System.Diagnostics;

namespace GopherTester
{
    class Program
    {
        static void Main(string[] args)
        {
            Stopwatch sw = Stopwatch.StartNew();
            Console.Write(sw.ElapsedMilliseconds.ToString() + ": " + "Loading File...");
            FieldReader reader = new FieldReader(@"C:\users\t_howab\desktop\test6.field");
            Console.Write("DONE\n");
            //Console.ReadLine();
            Console.Write(sw.ElapsedMilliseconds.ToString() + ": " + "Slicing and Dicing...");
            reader.PreProcess();
            Console.Write("DONE\n");
            //Console.ReadLine();
            Console.Write(sw.ElapsedMilliseconds.ToString() + ": " + "Reading Image...");
            reader.ProcessImage();
            Console.Write("DONE\n");
            //Console.ReadLine();
            Console.Write(sw.ElapsedMilliseconds.ToString() + ": " + "Slicing and Dicing twice...");
            reader.PreProcessSTL();
            Console.Write("DONE\n");
            //Console.ReadLine();
            Console.Write(sw.ElapsedMilliseconds.ToString() + ": " + "Reading the twice sliced dice...");
            reader.ProcessMeshes();
            Console.Write("DONE\n");
            Console.WriteLine("Operation completed in " + sw.ElapsedMilliseconds.ToString() + "ms");
            Console.ReadLine();
            //Console.Write("Reading attributes...");
            //reader.ProcessAttributes();
            //Console.Write("DONE");
            //Console.ReadLine();
            //Console.Write("Reading Joints...");
            //reader.ProcessJoints();
            //Console.Write("DONE");
            //Console.ReadLine();
        }
    }
}