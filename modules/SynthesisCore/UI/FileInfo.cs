namespace SynthesisCore.UI
{
    public struct FileInfo
    {
        // this is a temporary class while the file browser is made

        public string Name { get; private set; }
        public string LastModified { get; private set; }

        public FileInfo(string name, string lastModified)
        {
            Name = name;
            LastModified = lastModified;
        }
        
    }
}