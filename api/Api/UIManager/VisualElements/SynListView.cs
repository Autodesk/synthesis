using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using SynthesisAPI.EventBus;
using SynthesisAPI.Runtime;
using UnityEngine.UIElements;

namespace SynthesisAPI.UIManager.VisualElements
{
    public class SynListView: SynVisualElement
    {
        private EventBus.EventBus.EventCallback _callback;
        
        public ListView Element
        {
            get => (_visualElement as ListView)!;
            set => _visualElement = value;
        }
        
        public static explicit operator ListView(SynListView e) => e.Element;
        public static explicit operator SynListView(ListView e) => new SynListView(e);
        public static explicit operator SynListView(VisualElement e) => new SynListView((e as ListView)!);

        private (IList Source, Func<SynVisualElement> MakeItem, Action<SynVisualElement, int> BindItem) PopulateParams =
            (new List<object>(), () => null!, (element, i) => { });

        public SynListView()
        {
            Element = ApiProvider.InstantiateFocusable<ListView>()!;
            if (Element == null)
                throw new Exception();
            Element.selectionType = SelectionType.Single;
        }

        public SynListView(ListView element)
        {
            Element = element;
            Element.selectionType = SelectionType.Single;
        }
        
        // Unsure if this is needed
        ~SynListView()
        {
            /*if (EventBus.EventBus.HasTagSubscriber(EventTag))
                EventBus.EventBus.RemoveTagListener(EventTag, _callback);*/
        }

        public void Populate(IList source, Func<SynVisualElement> makeItem, Action<SynVisualElement, int> bindItem)
        {
            PopulateParams = (source, makeItem, bindItem);
            PostUxmlLoad();
        }

        public int SelectedIndex => Element.selectedIndex;
        public object SelectedItem => Element.selectedItem;
        
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
            Element.makeItem = () => PopulateParams.MakeItem().VisualElement;
            Element.bindItem = (element, index) => PopulateParams.BindItem(element.GetSynVisualElement(), index);
            Element.itemsSource = PopulateParams.Source;
            for (int i = 0; i < Element.itemsSource.Count; i++)
            {
                var elem = Element.makeItem();
                Element.bindItem(elem, i);
                Element.Add(elem);
            }
            ApiProvider.Log("Calling children");
            base.PostUxmlLoad();
            return null!;
        }

        protected override dynamic DynamicVisualElement
        {
            get => Element;
            set => Element = value is ListView ? value : Element;
        }
    }
}