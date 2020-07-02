using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynthesisAPI.VirtualFileSystem
{
    public class DirectroyExpection : Exception {
        public DirectroyExpection() { }
        public DirectroyExpection(string message) : base(message) { }
        public DirectroyExpection(string message, Exception inner) : base(message, inner) { }
    }

    public class DirectroyDepthExpection : DirectroyExpection
    {
        public DirectroyDepthExpection(int depth) : base($"FileSystem: traversing path would exceed maximum directory depth of {FileSystem.MaxDirectoryDepth} (Path depth is {depth})") { }
    }
}
