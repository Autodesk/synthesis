using SynthesisAPI.Utilities;
using System;

namespace SynthesisAPI.VirtualFileSystem
{
    /// <summary>
    /// Any type of entry managed by the virtual file system
    /// </summary>
    public interface IEntry
    {
        /// <summary>
        /// Name of the entry (used as its identifier in the virtual file system)
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// Access permissions of this entry
        /// </summary>
        public Permissions Permissions { get; internal set; }

        /// <summary>
        /// Parent directory of this entry in the virtual file system
        /// 
        /// (null if unset)
        /// </summary>
        public Directory Parent { get; internal set; }

        [ExposedApi]
        public void Delete();

        internal void DeleteInner();
    }
}
