using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using SynthesisAPI.EventBus;
using SynthesisAPI.Runtime;
using UnityEngine.UIElements;
using UnityListView = UnityEngine.UIElements.ListView;

namespace SynthesisAPI.UIManager.VisualElements
{
    public class ListView: VisualElement
    {
        private EventBus.EventBus.EventCallback _callback;
        
        public UnityListView Element
        {
            get => (_visualElement as UnityListView)!;
            set => _visualElement = value;
        }
        
        public static explicit operator UnityListView(ListView e) => e.Element;
        public static explicit operator ListView(UnityListView e) => new ListView(e);
        // public static explicit operator SynListView(VisualElement e) => new SynListView((e as UnityListView)!);

        private (IList Source, Func<VisualElement> MakeItem, Action<VisualElement, int> BindItem) PopulateParams =
            (new List<object>(), () => null!, (element, i) => { });

        public ListView()
        {
            // Element = ApiProvider.InstantiateFocusable<UnityListView>()!;
            Element = ApiProvider.CreateUnityType<UnityListView>()!;
            if (Element == null)
                throw new Exception();
            Element.selectionType = SelectionType.Single;
        }

        public ListView(UnityListView element)
        {
            Element = element;
            Element.selectionType = SelectionType.Single;
        }
        
        // Unsure if this is needed
        ~ListView()
        {
            /*if (EventBus.EventBus.HasTagSubscriber(EventTag))
                EventBus.EventBus.RemoveTagListener(EventTag, _callback);*/
        }

        public void Populate(IList source, Func<VisualElement> makeItem, Action<VisualElement, int> bindItem)
        {
            PopulateParams = (source, makeItem, bindItem);
            PostUxmlLoad();
        }

        public int SelectedIndex => Element.selectedIndex;
        public object SelectedItem => Element.selectedItem;

        public int ItemHeight {
            get => Element.itemHeight;
            set => Element.itemHeight = value;
        }
        
        public string EventTag => $"button/{Element.name}";

        public void SubscribeOnSelectionChanged(Action<IEvent> callback)
        {
            _callback = e => callback(e);
            EventBus.EventBus.NewTagListener(EventTag, _callback);
            PostUxmlLoad();
        }

        public override IEnumerable<Object> PostUxmlLoad()
        {
            if (PopulateParams.Source.Count < 1)
                return null!;
            ApiProvider.Log("Didn't return");
            if (Element == null)
                throw new Exception("This should be impossible");
            Element.makeItem = () => PopulateParams.MakeItem().UnityVisualElement;
            Element.bindItem = (element, index) => PopulateParams.BindItem(element.GetVisualElement(), index);
            Element.itemsSource = PopulateParams.Source;
            for (int i = 0; i < Element.itemsSource.Count; i++)
            {
                var elem = Element.makeItem();
                Element.bindItem(elem, i);
                Element.Add(elem);
            }
            ApiProvider.Log("Calling children", LogLevel.Warning);
            base.PostUxmlLoad();
            return null!;
        }

        protected override dynamic DynamicVisualElement
        {
            get => Element;
            set => Element = value is UnityListView ? value : Element;
        }
    }
}