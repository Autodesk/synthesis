using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UIElements;

namespace SynthesisAPI.UIManager
{
    public static class UIManager
    {
        private Dictionary<string, Element> elements = new Dictionary<string, Element>();

        /// <summary>
        /// Registers a UI element to be displayed
        /// </summary>
        /// <param name="element">Element to be displayed</param>
        /// <param name="isVisible">Whether the element is immediately visible or hidden</param>
        /// <exception cref="Exception">Thrown if a UI element already exists with the specified key</exception>
        public void RegisterElement(Element element, bool isVisible)
        {
            if (!elements.ContainsKey(element.Key))
            {
                
            }
            else
            {
                throw new Exception("Could not add UI element with key [" + element.Key + "] because the key is not unique");
            }
        }

        /// <summary>
        /// Removes a UI element entirely, consider <see cref="ToggleElement"/> first
        /// </summary>
        /// <param name="key">Key of the element</param>
        /// <exception cref="Exception">Thrown if a UI element with the specified key cannot be found</exception>
        public void UnregisterElement(string key)
        {
            if (elements.ContainsKey(key))
            {
                
            }
            else
            {
                throw new Exception("Could not unregister UI element with key [" + key + "] because the key does not exist");
            }
        }

        /// <summary>
        /// Toggles an exist UI element's visibility
        /// </summary>
        /// <param name="key">Key of the element</param>
        /// <exception cref="Exception">Thrown if a UI element with the specified key cannot be found</exception>
        public void ToggleElement(string key)
        {
            if (elements.ContainsKey(key))
            {
                // possibly remove this method, expand methods in individual Element class
                // maybe add GetElementByKey for outside usage and accessing
            }
            else
            {
                throw new Exception("Could not toggle UI element with key [" + key + "] ");
            }
        }

        public void AddTab(string tabName)
        {
            
        }

        public void RemoveTab(string tabName)
        {
            
        }

        private void HighlightTab(String tabName)
        {
            
        }
        
        private void SetChildrenVisibility()
        {
            
        }

        public void CenterElementHorizontally(string key, float width)
        {
            
        }

    }
}
