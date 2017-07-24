using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GopherAPI.Reader;
using GopherAPI.STL;
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
            Gopher.ProgressCallback = delegate (string message)
            {
                Console.WriteLine(message);
            };
        }
    }
}