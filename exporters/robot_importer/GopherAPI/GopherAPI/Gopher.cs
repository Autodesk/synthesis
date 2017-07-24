using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using GopherAPI.STL;
using GopherAPI.Nodes;
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
            throw new NotImplementedException();
        }

        /// <summary>
        /// Reads a robot file into a GopherRobot class from a path. Make sure you have defined the progress delegate.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static GopherRobot ReadRobot(string path)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Use this to get a thumbnail from a field or robot file
        /// </summary>
        /// <param name="path">The path of the file to get the thumbnail from</param>
        /// <returns>A Bitmap object</returns>
        public static Bitmap ReadThumbnail(string path)
        {
            //TODO implement a streamlined version of the reader here
            throw new NotImplementedException();
        }
    }
}
