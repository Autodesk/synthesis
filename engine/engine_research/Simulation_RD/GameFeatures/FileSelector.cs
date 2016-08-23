using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace Simulation_RD.GameFeatures
{
    /// <summary>
    /// Can be used to move throughout a file structure
    /// </summary>
    class FileSelector
    {
        /// <summary>
        /// The current directory
        /// </summary>
        public string current { get; private set; }

        /// <summary>
        /// All sub directories
        /// </summary>
        public IEnumerable<string> Directories { get; private set; }

        /// <summary>
        /// Starts a new file selector at the given directory
        /// </summary>
        /// <param name="startDirectory"></param>
        public FileSelector(string startDirectory)
        {
            MoveDirectory(startDirectory);
        }

        /// <summary>
        /// Moves into the given directory
        /// </summary>
        /// <param name="directory"></param>
        public void MoveInto(string directory)
        {
            if (!Directories.Contains(directory))
                throw new ArgumentException("That directory does not exist");

            MoveDirectory(directory);
        }

        /// <summary>
        /// Moves up one level
        /// </summary>
        public void MoveUp()
        {
            IEnumerable<string> directoryParts = current.Split(new[] { '\\' });
            directoryParts = directoryParts.Take(directoryParts.Count() - 1);
            MoveDirectory(string.Join("\\", directoryParts));
        }

        /// <summary>
        /// Evaluates current and child directories
        /// </summary>
        /// <param name="dir"></param>
        private void MoveDirectory(string dir)
        {
            current = dir;
            Directories = Directory.EnumerateDirectories(current);
        }
    }
}
