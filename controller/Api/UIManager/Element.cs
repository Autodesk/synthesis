using UnityEngine.UIElements;

namespace SynthesisAPI.UIManager
{
    public class Element
    {
        public string pathToElement { get; }
        public string treeName { get; }
        public string Key { get; }
        public VisualTreeAsset visualTreeAsset { get; }
        public VisualElement visualElement { get; }

        public Element(string pathToElement, string treeName, string key)
        {
            this.pathToElement = pathToElement;
            this.treeName = treeName;
            this.Key = key;

            this.visualTreeAsset = /*AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(pathToElement);*/ null;
            this.visualElement = visualTreeAsset.CloneTree();
        }

        public Element(string treeName, string key)
        {
            
        }
        
    }
}