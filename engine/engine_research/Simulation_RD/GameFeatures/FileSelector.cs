using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Simulation_RD.GameFeatures
{
    class FileSelector
    {
        public string current { get; private set; }
        public IEnumerable<string> Directories { get; private set; }

        public FileSelector(string startDirectory)
        {
            MoveDirectory(startDirectory);
        }

        public void MoveInto(string directory)
        {
            if (!Directories.Contains(directory))
                throw new ArgumentException("That directory does not exist");

            MoveDirectory(directory);
        }

        public void MoveUp()
        {
            IEnumerable<string> directoryParts = current.Split(new[] { '\\' });
            directoryParts = directoryParts.Take(directoryParts.Count() - 1);
            MoveDirectory(string.Join("\\", directoryParts));
        }

        private void MoveDirectory(string dir)
        {
            current = dir;
            Directories = Directory.EnumerateDirectories(current);
        }
    }
}
