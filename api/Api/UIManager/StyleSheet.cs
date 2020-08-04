using System.Collections.Generic;
using SynthesisAPI.UIManager.VisualElements;

namespace SynthesisAPI.UIManager
{
    public class StyleSheet
    {
        private Dictionary<string, UssClass> classes = new Dictionary<string, UssClass>();

        public StyleSheet(string path)
        {
            string[] lines = System.IO.File.ReadAllLines(path);
            ParseLines(lines);
        }

        private void ParseLines(string[] lines)
        {
            UssClass currentClass = null;
            
            foreach (string line in lines)
            {
                if (line.StartsWith("."))
                {
                    string[] lineContents = line.Split('.');
                    string className = lineContents[1];

                    currentClass = new UssClass(className);
                } else if (line.StartsWith("}"))
                {
                    if (currentClass != null)
                    {
                        classes.Add(currentClass.ClassName, currentClass);
                    }
                }
                else
                {
                    if (currentClass != null && line.Length > 2)
                    {
                        string[] lineContents = line.Split(':');
                        string propertyName = lineContents[0];
                        string propertyValue = lineContents[1].Substring(1, lineContents[1].Length - 1);

                        // use parseentry somewhere here?
                        
                        currentClass.AddProperty(propertyName, propertyValue);
                    }
                }
            }
        }

        public bool HasClass(string className)
        {
            return classes.ContainsKey(className);
        }

        public void ApplyClassToVisualElement(string className, VisualElement visualElement)
        {
            UssClass ussClass = classes[className];

            foreach (string propertyName in ussClass.GetProperties())
            {
                visualElement.SetStyleProperty(propertyName, ussClass.GetPropertyValue(propertyName));
            }
        }

    }
}