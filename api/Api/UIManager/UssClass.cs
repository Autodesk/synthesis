using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace SynthesisAPI.UIManager
{
    public class UssClass
    {
        private List<string> lines = new List<string>();
        public ReadOnlyCollection<string> Lines { get => lines.AsReadOnly(); }
        public string ClassName { get; }
        
        public UssClass(string className)
        {
            this.ClassName = className;
        }

        public void AddLine(string line)
        {
            lines.Add(line);
        }
        
    }
}