using System.Collections.Generic;
using SynthesisAPI.UIManager.VisualElements;

namespace SynthesisAPI.UIManager
{
    public class UssParser
    {
        private void ApplyStylesFromUss(VisualElement visualElement, List<string> lines)
        {
            List<UssClass> classes = new List<UssClass>();

            UssClass currentClass;
            
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
                        classes.Add(currentClass);
                    }
                }
                else
                {
                    if (currentClass != null && line.Length > 2)
                    {
                        string[] lineContents = line.Split(':');
                        string propertyName = lineContents[0];
                        string propertyValue = lineContents[1].Substring(1);

                        currentClass.AddProperty(propertyName, propertyValue);
                    }
                }
                
            }

            foreach (UssClass ussClass in classes)
            {
                foreach (string propertyName in ussClass.GetProperties())
                {
                    visualElement.SetStyleProperty(propertyName, ussClass.GetPropertyValue(propertyName));
                }
            }
        }
    }
}