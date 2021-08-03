using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace RobotProofOfConcept
{
    class Program
    {
        static void Main(string[] args)
        {
            RobotConcept concept = new RobotConcept();
            concept.Run();
            foreach (var s in concept.Robot.CurrentSignals)
            {
                Console.Write("IO: ");
                Console.WriteLine(s.Value.Io.ToString());
                Console.Write("Device Type: ");
                Console.WriteLine(s.Value.DeviceType);
                Console.Write("Value: ");
                Console.WriteLine(s.Value.Value);
            }
            
            Console.WriteLine("\nUpdating Values...");
            concept.RunUpdate();
            Thread.Sleep(500);
            foreach (var s in concept.Robot.CurrentSignals)
            {
                Console.Write("IO: ");
                Console.WriteLine(s.Value.Io.ToString());
                Console.Write("Device Type: ");
                Console.WriteLine(s.Value.DeviceType);
                Console.Write("Value: ");
                Console.WriteLine(s.Value.Value);
            }
        }
    }
}
