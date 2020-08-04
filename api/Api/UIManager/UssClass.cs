using System.Collections.Generic;

namespace SynthesisAPI.UIManager
{
    public class UssClass
    {
        private Dictionary<string, string> values = new Dictionary<string,string>();
        public string ClassName { get; }
        
        public UssClass(string className)
        {
            this.ClassName = className;
        }

        public void AddProperty(string propertyName, string value)
        {
            values.Add(propertyName, value);
        }

        public Dictionary<string, string>.KeyCollection GetProperties()
        {
            return values.Keys;
        }

        public string GetPropertyValue(string propertyName)
        {
            return values[propertyName];
        }
        
    }
}