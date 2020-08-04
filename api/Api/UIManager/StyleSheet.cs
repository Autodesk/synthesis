using System.Collections.Generic;

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
                string line = rawLine.Trim(); // removes indentation spaces on left 
                
                if (line.StartsWith("."))
                {
                    string[] lineContents = line.Split('.');
                    string className = lineContents[1].Substring(0, lineContents[1].Length - 2); // substring to remove " {" from line

                    currentClass = new UssClass(className);
                    //ApiProvider.Log("[UI] New class found with name [" + className + "]");
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
                        currentClass.AddLine(line.Substring(0, line.Length - 1)); // substring to remove semicolon
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
            //ApiProvider.Log("[UI] Attempting to apply class [" + className + "] to [" + visualElement.name + "]");
            UssClass ussClass = classes[className];

            foreach (string line in ussClass.Lines)
            {
                visualElement = UIParser.ParseEntry(line, visualElement);
            }

            return visualElement;
        }

    }
}