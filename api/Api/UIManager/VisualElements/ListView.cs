using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using SynthesisAPI.EventBus;
using SynthesisAPI.Runtime;
using _SelectionType = UnityEngine.UIElements.SelectionType;
using _UnityListView = UnityEngine.UIElements.ListView;

namespace SynthesisAPI.UIManager.VisualElements
{
    public class ListView: VisualElement
    {
        /*
        /// <summary>
        /// Wrapper for the Unity ListView
        /// This is necessary because these methods are currently protected and we need access to them
        /// </summary>
        public class _UnityListViewWrapper : _UnityListView
        {
            public new void AddToSelection(int index) => base.AddToSelection(index);
            public new void ClearSelection() => base.ClearSelection();
            public new void RemoveFromSelection(int index) => base.RemoveFromSelection(index);
            public new void SetSelection(int index) => base.SetSelection(index);
        }
        */

        private EventBus.EventBus.EventCallback? _callback;

        private protected _UnityListView Element
        {
            get => (_visualElement as _UnityListView)!;
            set => _visualElement = value;
        }
        
        // internal static explicit operator UnityListViewWrapper(ListView e) => e.Element;
        // internal static explicit operator ListView(UnityListViewWrapper e) => new ListView(e);
        // public static explicit operator SynListView(VisualElement e) => new SynListView((e as UnityListView)!);

        private (IList Source, Func<VisualElement> MakeItem, Action<VisualElement, int> BindItem) PopulateParams =
            (new List<object>(), () => null!, (element, i) => { });

        public ListView()
        {
            // Element = ApiProvider.InstantiateFocusable<UnityListView>()!;
            Element = ApiProvider.CreateUnityType<_UnityListView>()!;
            if (Element == null)
                throw new Exception();
            Element.selectionType = _SelectionType.Single;
        }

        internal ListView(_UnityListView element)
        {
            Element = element;
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

        public void Refresh()
        {
            Element.Refresh();
        }

        public int SelectedIndex => Element.selectedIndex;
        public object SelectedItem => Element.selectedItem;
        public int ItemHeight
        {
            get => Element.itemHeight;
            set => Element.itemHeight = value;
        }
        // public void ClearSelection() => Element.ClearSelection();
        public void ScrollTo(VisualElement visualElement) => Element.ScrollTo(visualElement.UnityVisualElement);
        public void ScrollToItem(int index) => Element.ScrollToItem(index);

        public string SelectedEventTag => $"listview-button/{Element.name}";
        public string ItemChosenEventTag => $"item-chosen/{Element.name}";

        public class SelectionChangedEvent: IEvent
        {
            public List<object> SelectedObjects { get; private set; }
            public SelectionChangedEvent(List<object> selectedObjects)
            {
                SelectedObjects = selectedObjects;
            }
        }

        public class ItemChosenEvent : IEvent
        {
            public object Item{ get; private set; }
            public ItemChosenEvent(object item)
            {
                Item = item;
            }
        }

        public void SubscribeOnSelectionChanged(Action<IEvent> callback)
        {
            if (_callback != null)
                EventBus.EventBus.RemoveTagListener(SelectedEventTag, _callback);
            _callback = e => callback(e);
            EventBus.EventBus.NewTagListener(SelectedEventTag, _callback);
            
            Element.onSelectionChange += l => EventBus.EventBus.Push(SelectedEventTag, new SelectionChangedEvent((List<object>)l));
        }

        public void SubscribeOnItemChosen(Action<IEvent> callback)
        {
            if (_callback != null)
                EventBus.EventBus.RemoveTagListener(ItemChosenEventTag, _callback);
            _callback = e => callback(e);
            EventBus.EventBus.NewTagListener(ItemChosenEventTag, _callback);

            Element.onItemsChosen += o => EventBus.EventBus.Push(ItemChosenEventTag, new ItemChosenEvent(o));
        }

        public override IEnumerable<Object> PostUxmlLoad()
        {
            Element.makeItem = () => PopulateParams.MakeItem().UnityVisualElement;
            Type t = PopulateParams.MakeItem().GetType();
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            Element.bindItem = (element, index) => PopulateParams.BindItem(
                (VisualElement)Activator.CreateInstance(t, flags, null, new object[] { element }, null),
                index);
            Element.itemsSource = PopulateParams.Source;
            base.PostUxmlLoad();
            return null!;
        }
    }
}