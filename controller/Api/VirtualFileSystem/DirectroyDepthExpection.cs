using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynthesisAPI.VirtualFileSystem
{
    public class DirectroyDepthExpection : Exception
    {
        public DirectroyDepthExpection(int depth) : base($"FileSystem: traversing path would exceed maximum directory depth of {FileSystem.MaxDirectoryDepth} (Path depth is {depth})") { }
    }
}
