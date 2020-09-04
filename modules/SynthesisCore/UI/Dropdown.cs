using System;
using System.Collections.Generic;
using MathNet.Spatial.Euclidean;
using SynthesisAPI.AssetManager;
using SynthesisAPI.EventBus;
using SynthesisAPI.InputManager;
using SynthesisAPI.InputManager.InputEvents;
using SynthesisAPI.InputManager.Inputs;
using SynthesisAPI.UIManager;
using SynthesisAPI.UIManager.VisualElements;
using SynthesisAPI.Utilities;

namespace SynthesisCore.UI
{
    public class Dropdown
    {
        #region UIElements

        private static VisualElementAsset _dropdownAsset;

        private VisualElement _visualElement;

        private Button _button;

        private VisualElement _buttonIcon;

        private ListView _listView;

        #endregion

        public static implicit operator VisualElement(Dropdown d) => d._visualElement;

        private List<string> _options = new List<string>();

        private bool _isListViewVisible = false;

        private static bool _isDeselectDropdownAssigned = false;

        #region Properties

        public string Name { get; private set; }

        private int id;
        private static int Id = 0;

        public int Count { get => Selected == null ? _options.Count : _options.Count + 1; }

        private string _selected;
        public string Selected { get => _selected; set => Select(value); }

        public IEnumerable<string> Options
        {
            get
            {
                if (Selected == null)
                    return _options;
                else
                {
                    List<string> lst = new List<string>(_options);
                    lst.Add(Selected);
                    return lst;
                }
            }
        }

        public int ItemHeight { 
            get => _listView.ItemHeight; 
            set { 
                _listView.ItemHeight = value; 
                RefreshListView(); 
                SetButtonHeight(); 
            }
        }

        public string EventTag => $"dropdown-selection/{Name}-{id}";
        public class SelectionEvent : IEvent
        {
            public readonly string DropdownName;
            public readonly string SelectionName;
            public SelectionEvent(string name, string selection)
            {
                DropdownName = name;
                SelectionName = selection;
            }
        }

        #endregion

        public Dropdown(string name)
        {
            Init(name);
        }

        public Dropdown(string name, int defaultValueIndex, List<string> options)
        {
            for (int i = 0; i < options.Count; i++)
            {
                if (i == defaultValueIndex)
                    _selected = options[i];
                else
                    _options.Add(options[i]);
            }
            Init(name);
        }

        public Dropdown(string name, int defaultValueIndex, params string[] options)
        {
            for (int i = 0; i < options.Length; i++)
            {
                if (i == defaultValueIndex)
                    _selected = options[i];
                else
                    _options.Add(options[i]);
            }
            Init(name);
        }

        private void Init(string name)
        {
            Name = name;
            id = Id;
            Id++;
            if (_dropdownAsset == null)
                _dropdownAsset = AssetManager.GetAsset<VisualElementAsset>("/modules/synthesis_core/UI/uxml/Dropdown.uxml");
            _visualElement = _dropdownAsset.GetElement(name);
            CreateButton();
            CreateListView();
            //default height property
            ItemHeight = 30;

            //assign button once for all dropdowns
            if (!_isDeselectDropdownAssigned)
                InputManager.AssignDigitalInput("_deselect dropdown", new MouseDown("mouse 0"));

            //mouse click listener to close dropdown
            EventBus.NewTagListener("input/_deselect dropdown", e =>
            {
                if (e is MouseDownEvent downEvent && downEvent.State == DigitalState.Down)
                {
                    var point = new Vector2D(downEvent.MousePosition.X, ApplicationWindow.Height - downEvent.MousePosition.Y);
                    if (_isListViewVisible && !_listView.ContainsPoint(point) && !_button.ContainsPoint(point)) //auto close if clicked anywhere but dropdown
                        CloseListView();
                }
            });

            _isDeselectDropdownAssigned = true;
        }

        private void CreateButton()
        {
            //init visual elements
            _button = (Button)_visualElement.Get("selected-button");
            _buttonIcon = _button.Get("dropdown-icon");
            RefreshButton();
            ToggleIcon();
            _button.Subscribe(x =>
            {
                if (_isListViewVisible)
                    CloseListView();
                else
                    OpenListView();
            });
        }

        private void CreateListView()
        {
            //init visual elements
            _listView = (ListView)_visualElement.Get("options-list");
            //hide list view on start
            _listView.RemoveFromHierarchy();
            //link list view population
            _listView.Populate(_options,
                                () => new Button(),
                                (element, index) =>
                                {
                                    var button = element as Button;
                                    button.Name = $"{_listView.Name}-{_options[index]}";
                                    button.Text = _options[index];
                                    button.Subscribe(x => OnOptionClick(button, index));
                                    button.SetStyleProperty("border-top-width", "0");
                                    button.SetStyleProperty("border-bottom-width", "0");
                                    button.SetStyleProperty("border-right-width", "0");
                                    button.SetStyleProperty("border-left-width", "0");
                                    button.SetStyleProperty("border-top-left-radius", "0");
                                    button.SetStyleProperty("border-top-right-radius", "0");
                                    button.SetStyleProperty("border-bottom-left-radius", "0");
                                    button.SetStyleProperty("border-bottom-right-radius", "0");
                                    button.SetStyleProperty("margin-left", "0");
                                    button.SetStyleProperty("margin-right", "0");
                                    button.SetStyleProperty("margin-top", "0");
                                    button.SetStyleProperty("margin-bottom", "0");
                                });
            RefreshListView();
        }
        private void OnOptionClick(Button button, int index)
        {
            var _tmp = button.Text;
            if (Selected == null)
            {
                _options.RemoveAt(index);
                RefreshListView();
            }
            else
            {
                _options[index] = Selected;
                button.Text = Selected;
            }
            _selected = _tmp;
            RefreshButton();
            CloseListView();
            
            EventBus.Push(EventTag, new SelectionEvent(Name, Selected));
        }

        public void Subscribe(Action<IEvent> action)
        {
            EventBus.NewTagListener(EventTag, e => action(e));
        }

        public bool Add(string option)
        {
            if (_options.Contains(option) || Selected == option)
                return false;
            if (Selected == null)
            {
                _selected = option;
                RefreshButton();
            }
            else
            {
                _options.Add(option);
                ToggleIcon();
                RefreshListView();
            }
            return true;
        }
        public bool Remove(string option)
        {
            if (Selected == option)
            {
                _selected = null;
                RefreshButton();
                return true;
            }
            else if (_options.Remove(option))
            {
                ToggleIcon();
                RefreshListView();
                return true;
            }
            return false;
        }

        private void Select(string option)
        {
            if (Selected != option)
            {
                int index = _options.IndexOf(option);
                if (index == -1) //does not exist
                {
                    Add(option);
                    Select(option);
                }
                else
                {
                    _options[index] = _selected;
                    _selected = option;
                    RefreshAll();
                }
            }
        }

        private void RefreshButton()
        {
            _button.Text = Selected == null ? " " : Selected;
        }

        private void SetButtonHeight()
        {
            _button.SetStyleProperty("height", ItemHeight.ToString());
            _buttonIcon.SetStyleProperty("width", ItemHeight.ToString());
            _buttonIcon.SetStyleProperty("height", ItemHeight.ToString());
        }

        private void RefreshListView()
        {
            _listView.Refresh();
            UpdateListView();
        }

        private void RefreshAll()
        {
            RefreshButton();
            RefreshListView();
        }

        private void UpdateListView()
        {
            //position
            Vector2D position = _visualElement.Position;
            _listView.SetStyleProperty("top", (position.Y + ItemHeight).ToString() + "px");
            _listView.SetStyleProperty("left", position.X.ToString() + "px");
            //width
            Vector2D size = _visualElement.Size;
            _listView.SetStyleProperty("width", size.X.ToString() + "px");
            //height
            int listViewHeight = _options.Count * _listView.ItemHeight;
            _listView.SetStyleProperty("height", listViewHeight.ToString() + "px");
        }

        private void OpenListView()
        {
            if (!_isListViewVisible)
            {
                UpdateListView();
                UIManager.RootElement.Add(_listView); //shows list view
                _isListViewVisible = true;
                ToggleIcon();
            }
        }

        private void CloseListView()
        {
            if (_isListViewVisible)
            {
                _listView.RemoveFromHierarchy(); //hides list view
                _isListViewVisible = false;
                ToggleIcon();
            }
        }

        private void ToggleIcon()
        {
            //toggle icon
            if (_options.Count > 0)
            {
                _buttonIcon.SetStyleProperty("visibility", "visible");
                _buttonIcon.SetStyleProperty("background-image", _isListViewVisible ?
                    "/modules/synthesis_core/UI/images/toolbar-hide-icon.png" :
                    "/modules/synthesis_core/UI/images/toolbar-show-icon.png");
            }
            else
                _buttonIcon.SetStyleProperty("visibility", "hidden");
        }
    }
}