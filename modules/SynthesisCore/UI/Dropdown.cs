using System.Collections.Generic;
using MathNet.Spatial.Euclidean;
using SynthesisAPI.AssetManager;
using SynthesisAPI.UIManager;
using SynthesisAPI.UIManager.VisualElements;

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

        #region Properties

        public string Name { get; private set; }

        public int Count { get => Selected == null ? _options.Count : _options.Count + 1; }

        public string Selected { get; set; }

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

        public int ItemHeight { get => _listView.ItemHeight; set { _listView.ItemHeight = value; RefreshListView(); SetButtonHeight(); } }

        public delegate void SubscribeEvent(string s);

        public event SubscribeEvent OnValueChanged;

        #endregion

        public Dropdown(string name)
        {
            Init(name);
        }

        public Dropdown(string name, List<string> options)
        {
            for (int i = 0; i < options.Count; i++)
            {
                if (i == 0)
                    Selected = options[i];
                else
                    _options.Add(options[i]);
            }
            Init(name);
        }

        public Dropdown(string name, params string[] options)
        {
            for (int i = 0; i < options.Length; i++)
            {
                if (i == 0)
                    Selected = options[i];
                else
                    _options.Add(options[i]);
            }
            Init(name);
        }

        private void Init(string name)
        {
            Name = name;
            if (_dropdownAsset == null)
                _dropdownAsset = AssetManager.GetAsset<VisualElementAsset>("/modules/synthesis_core/UI/uxml/Dropdown.uxml");
            _visualElement = _dropdownAsset.GetElement(name);
            CreateButton();
            CreateListView();
            //default height property
            ItemHeight = 30;
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
                ToggleListView();
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
                                    button.SetStyleProperty("left", "-10px");
                                    button.SetStyleProperty("width", "110%");
                                });
            RefreshListView();
            //lose focus
            _listView.OnLoseFocus(() => OnLoseFocus());
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
            Selected = _tmp;
            RefreshButton();
            ToggleListView();
            OnValueChanged?.Invoke(Selected);
        }

        public bool Add(string option)
        {
            if (_options.Contains(option) || Selected == option)
                return false;
            if (Selected == null)
            {
                Selected = option;
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
                Selected = null;
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

        private void RefreshButton()
        {
            _button.Text = Selected == null ? " " : Selected;
        }

        private void SetButtonHeight()
        {
            _button.SetStyleProperty("height", ItemHeight.ToString());
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

        private void ToggleListView()
        {
            //toggle list view
            if (_isListViewVisible)
                _listView.RemoveFromHierarchy(); //hides list view
            else
            {
                UpdateListView();
                UIManager.RootElement.Add(_listView); //shows list view
                _listView.Focus();
            }
            _isListViewVisible = !_isListViewVisible;
            ToggleIcon();

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
        private void OnLoseFocus()
        {
            if (_isListViewVisible)
                ToggleListView();
        }
    }
}