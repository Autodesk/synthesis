using System.Collections.Generic;
using SynthesisAPI.Runtime;
using SynthesisAPI.UIManager.VisualElements;

namespace SynthesisAPI.UIManager
{
    public class StyleSheet
    {
        private Dictionary<string, UssClass> classes = new Dictionary<string, UssClass>();

        public StyleSheet(string[] contents)
        {
            ParseLines(contents);
        }

        private void ParseLines(string[] lines)
        {
            UssClass currentClass = null;
            
            foreach (string rawLine in lines)
            {
                string line = rawLine.Trim();
                
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
                        currentClass.AddLine(line);
                    }
                }
            }
        }

        public bool HasClass(string className)
        {
            return classes.ContainsKey(className);
        }

        internal UnityEngine.UIElements.VisualElement ApplyClassToVisualElement(string className, UnityEngine.UIElements.VisualElement visualElement)
        {
            ApiProvider.Log("Attempting to apply [" + className + "] to " + visualElement.name);
            
            UssClass ussClass = classes[className];

            foreach (string line in ussClass.Lines)
            {
                visualElement = UIParser.ParseEntry(line, visualElement);
            }

            return visualElement;
        }

    }
}