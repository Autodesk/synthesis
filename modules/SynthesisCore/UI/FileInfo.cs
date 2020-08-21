namespace SynthesisCore.UI
{
    public struct FileInfo
    {
        // this is a temporary class while the file browser is made

        public string Name { get; }
        public string LastModified { get; }

        public FileInfo(string name, string lastModified)
        {
            Name = name;
            LastModified = lastModified;
        }
        
    }
}