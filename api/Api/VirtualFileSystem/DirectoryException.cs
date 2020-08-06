using System;

namespace SynthesisAPI.VirtualFileSystem
{
    public class DirectoryException : SynthesisException
    {
        public DirectoryException() { }
        public DirectoryException(string message) : base(message) { }
        public DirectoryException(string message, Exception inner) : base(message, inner) { }
    }

    public class DirectroyDepthExpection : DirectoryException
    {
        public DirectroyDepthExpection(int depth) : base($"FileSystem: traversing path would exceed maximum directory depth of {FileSystem.MaxDirectoryDepth} (Path depth is {depth})") { }
    }
}
