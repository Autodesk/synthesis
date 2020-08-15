using _UnityVisualElement = UnityEngine.UIElements.VisualElement;
using System.Collections.Generic;
using SynthesisAPI.Utilities;

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
                    if(line[line.Length - 1] == '{')
                    {
                        line = line.Substring(0, line.Length - 1).Trim(); // remove " {" from line
                    }
                    string[] lineContents = line.Split('.');
                    string className = lineContents[1]; 

                    currentClass = new UssClass(className);
                    //Logger.Log("[UI] New class found with name [" + className + "]");
                }
                else if (line.StartsWith("}"))
                {
                    if (currentClass != null)
                    {
                        if (HasClass(currentClass.ClassName))
                        {
                            Logger.Log($"Stylesheet defines class with duplicate name: {currentClass.ClassName}", LogLevel.Warning);
                        }
                        else
                        {
                            classes.Add(currentClass.ClassName, currentClass);
                        }
                    }
                }
                else if (line.StartsWith("#"))
                {
                    Logger.Log("Stylesheet contains ID identifier '#' which is not currently supported", LogLevel.Error);
                }
                else
                {
                    if (currentClass != null && line.Length > 2)
                    {
                        if (line.StartsWith("border-") && line.EndsWith("px;"))
                        {
                            currentClass.AddLine(line.Substring(0, line.Length - 3)); // substring to remove px and semicolon
                        }
                        else
                        {
                            currentClass.AddLine(line.Substring(0, line.Length - 1)); // substring to remove semicolon
                        }
                    }
                }
            }
        }

        public bool HasClass(string className)
        {
            return classes.ContainsKey(className);
        }

        internal _UnityVisualElement ApplyClassToVisualElement(string className, _UnityVisualElement visualElement)
        {
            //Logger.Log("[UI] Attempting to apply class [" + className + "] to [" + visualElement.name + "]");
            UssClass ussClass = classes[className];

            foreach (string line in ussClass.Lines)
            {
                visualElement = UIParser.ParseEntry(line, visualElement);
            }

            return visualElement;
        }

    }
}