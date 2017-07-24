using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using GopherAPI.STL;
using GopherAPI.Nodes;
using GopherAPI.Reader;
using GopherAPI.Nodes.Joint;
using GopherAPI.Nodes.Colliders;
using GopherAPI.Nodes.Joint.Driver;

namespace GopherAPI
{
    /// <summary>
    /// Be sure to assign a delagate to ProgressCallback
    /// </summary>
    public static class Gopher
    {
        /// <summary>
        /// A delegate used to give unity information on file loading. 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="percent">Should be out of 100. I.E. 10.0f would translate to 10%</param>
        public delegate void ProgressDelegate(string message);

        /// <summary>
        /// Set this as a function that tells unity the progress of loading files.
        /// </summary>
        public static ProgressDelegate ProgressCallback;

        /// <summary>
        /// Reads a robot file into a GopherField class from a path. Make sure you have defined the progress delegate.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static GopherField ReadField(string path)
        {
            ProgressCallback("Loading file into memory...");
            var reader = new FieldReader(path);
            ProgressCallback("Pre-Processing loaded file...");
            reader.PreProcess();
            ProgressCallback("Pre-Processing STL meshes...");
            reader.PreProcessSTL();
            ProgressCallback("Processing Meshes...");
            reader.ProcessSTL();
            ProgressCallback("Processing Joints...");
            reader.ProcessJoints();
            ProgressCallback("Processing colliders...");
            reader.ProcessColliders();
            ProgressCallback("Generating node tree...");
            return FieldNodeGenerator.FieldFactory(reader.Field);
        }

        /// <summary>
        /// Reads a robot file into a GopherRobot class from a path. Make sure you have defined the progress delegate.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static GopherRobot ReadRobot(string path)
        {
            ProgressCallback("Loading file into memory...");
            var reader = new RobotReader(path);
            ProgressCallback("Pre-Processing loaded file...");
            reader.PreProcess();
            ProgressCallback("Pre-Processing STL meshes...");
            reader.PreProcessSTL();
            ProgressCallback("Processing Meshes...");
            reader.ProcessSTL();
            ProgressCallback("Processing Joints...");
            reader.ProcessJoints();
            ProgressCallback("Processing joint drivers...");
            reader.ProcessDrivers();
            ProgressCallback("Generating node tree...");
            return RobotNodeGenerator.RobotFactory(reader.Robot);
        }

        /// <summary>
        /// Use this to get a thumbnail from a field or robot file
        /// </summary>
        /// <param name="path">The path of the file to get the thumbnail from</param>
        /// <returns>A Bitmap object</returns>
        public static Bitmap ReadThumbnail(string path)
        {
            var reader = new GopherReader_Base(path);
            reader.PreProcess();
            return reader.ProcessThumbnail();
        }
    }
}
