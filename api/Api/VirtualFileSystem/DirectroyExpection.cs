using System;

namespace SynthesisAPI.VirtualFileSystem
{
    public class DirectroyExpection : SynthesisExpection
    {
        public DirectroyExpection() { }
        public DirectroyExpection(string message) : base(message) { }
        public DirectroyExpection(string message, Exception inner) : base(message, inner) { }
    }

    public class DirectroyDepthExpection : DirectroyExpection
    {
        public DirectroyDepthExpection(int depth) : base($"FileSystem: traversing path would exceed maximum directory depth of {FileSystem.MaxDirectoryDepth} (Path depth is {depth})") { }
    }
}
