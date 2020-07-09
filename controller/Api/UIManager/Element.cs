using UnityEngine.UIElements;

namespace SynthesisAPI.UIManager
{
    public class Element
    {
        public string pathToElement { get; }
        public string treeName { get; }
        public string uniqueKey { get; }
        public VisualTreeAsset visualTreeAsset { get; }
        public VisualElement visualElement { get; }

        public Element(string pathToElement, string treeName, string uniqueKey)
        {
            this.pathToElement = pathToElement;
            this.treeName = treeName;
            this.uniqueKey = uniqueKey;

            this.visualTreeAsset = /*AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(pathToElement);*/ null;
            this.visualElement = visualTreeAsset.CloneTree();
        }
    }
}