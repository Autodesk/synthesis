using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

using UButton = UnityEngine.UI.Button;
using UToggle = UnityEngine.UI.Toggle;
using USlider = UnityEngine.UI.Slider;

#nullable enable

namespace Synthesis.UI.Dynamic {
    public abstract class ModalDynamic {

        public const float MAIN_CONTENT_HORZ_PADDING = 35;

        private Vector2 _mainContentSize; // Shouldn't really be used after init is called
        private GameObject _unityObject;

        // Default for Modal
        private Button _cancelButton;
        protected Button CancelButton => _cancelButton;
        private Button _acceptButton;
        protected Button AcceptButton => _acceptButton;
        private Image _modalImage;
        protected Image ModalImage => _modalImage;
        private Label _title;
        protected Label Title => _title;
        private Label _description;
        protected Label Description => _description;
        
        private Content _mainContent;
        protected Content MainContent => _mainContent;

        protected ModalDynamic(Vector2 mainContentSize) {
            _mainContentSize = mainContentSize;
        }

        public void Init(GameObject unityObject) {
            _unityObject = unityObject;

            // Grab Customizable Modal Components
            var header = _unityObject.transform.Find("Header");
            _modalImage = header.Find("Image").GetComponent<Image>();
            _title = new Label(null, header.Find("Title").gameObject, null);
            _description = new Label( null, header.Find("Description").gameObject, null);

            var footer = _unityObject.transform.Find("Footer");
            _cancelButton = new Button(null!, footer.Find("Cancel").gameObject, null);
            _acceptButton = new Button(null!, footer.Find("Accept").gameObject, null);

            // Create Inital Content Component
            var hiddenContentT = _unityObject.transform.Find("Content");
            var hiddenRt = hiddenContentT.GetComponent<RectTransform>();
            hiddenRt.sizeDelta = new Vector2(hiddenRt.sizeDelta.x, _mainContentSize.y);
            var actualContentObj = GameObject.Instantiate(SynthesisAssetCollection.GetModalPrefab("content-base"), hiddenContentT);
            actualContentObj.name = "CentralContent";
            var contentRt = actualContentObj.GetComponent<RectTransform>();
            contentRt.offsetMax = new Vector2(-MAIN_CONTENT_HORZ_PADDING, contentRt.offsetMax.y);
            contentRt.offsetMin = new Vector2(MAIN_CONTENT_HORZ_PADDING, contentRt.offsetMin.y);
            var modalRt = _unityObject.GetComponent<RectTransform>();
            modalRt.sizeDelta = new Vector2(_mainContentSize.x + (MAIN_CONTENT_HORZ_PADDING * 2), modalRt.sizeDelta.y);
            _mainContent = new Content(null!, actualContentObj, _mainContentSize);
        }

        public abstract void Create();
        public abstract void Delete();
    }

    public abstract class UIComponent {

        // public static readonly Func<UIComponent, UIComponent> VerticalLayoutTemplate = (UIComponent component) => {
        //     return component.SetTopStretch<UIComponent>(anchoredY: component.Parent!.HeightOfChildren - component.Size.y);
        // };

        public float HeightOfChildren {
            get {
                float sum = 0f;
                Children.ForEach(x => sum += x.Size.y);
                return sum;
            }
        }
        public float WidthOfChildren {
            get {
                float sum = 0f;
                Children.ForEach(x => sum += x.Size.x);
                return sum;
            }
        }

        protected bool _eventsActive = true;
        public bool EventsActive => _eventsActive;

        public Vector2 Size { get; protected set; }
        protected GameObject RootGameObject;
        public RectTransform RootRectTransform { get; protected set; }
        protected UIComponent? Parent;
        protected List<UIComponent> Children = new List<UIComponent>();

        public UIComponent(UIComponent? parentComponent, GameObject rootGameObject) {
            Parent = parentComponent;
            RootGameObject = rootGameObject;
            RootRectTransform = rootGameObject.GetComponent<RectTransform>();
            Size = RootRectTransform.sizeDelta;
        }

        private void SetAnchorOffset(Vector2 aMin, Vector2 aMax, Vector2 oMin, Vector2 oMax) {
            RootRectTransform.anchorMin = aMin;
            RootRectTransform.anchorMax = aMax;
            RootRectTransform.offsetMin = oMin;
            RootRectTransform.offsetMax = oMax;
        }
        public T SetTopStretch<T>(float leftPadding = 0f, float rightPadding = 0f, float anchoredY = 0f) where T : UIComponent {
            SetAnchorOffset(new Vector2(0, 1), new Vector2(1, 1), new Vector2(leftPadding, -Size.y), new Vector2(-rightPadding, 0));
            RootRectTransform.pivot = new Vector2(RootRectTransform.pivot.x, 1);
            RootRectTransform.anchoredPosition = new Vector2(RootRectTransform.anchoredPosition.x, -anchoredY);
            return (this as T)!;
        }
        public T SetBottomStretch<T>(float leftPadding = 0f, float rightPadding = 0f, float anchoredY = 0f) where T : UIComponent {
            SetAnchorOffset(new Vector2(0, 0), new Vector2(1, 0), new Vector2(leftPadding, 0), new Vector2(-rightPadding, Size.y));
            RootRectTransform.pivot = new Vector2(RootRectTransform.pivot.x, 0);
            RootRectTransform.anchoredPosition = new Vector2(RootRectTransform.anchoredPosition.x, anchoredY);
            return (this as T)!;
        }
        public T SetLeftStretch<T>(float topPadding = 0f, float bottomPadding = 0f, float anchoredX = 0f) where T : UIComponent {
            SetAnchorOffset(new Vector2(0, 0), new Vector2(0, 1), new Vector2(0, bottomPadding), new Vector2(Size.x, -topPadding));
            RootRectTransform.pivot = new Vector2(0, RootRectTransform.pivot.y);
            RootRectTransform.anchoredPosition = new Vector2(anchoredX, RootRectTransform.anchoredPosition.y);
            return (this as T)!;
        }
        public T SetRightStretch<T>(float topPadding = 0f, float bottomPadding = 0f, float anchoredX = 0f) where T : UIComponent {
            SetAnchorOffset(new Vector2(1, 0), new Vector2(1, 1), new Vector2(-Size.x, bottomPadding), new Vector2(0, -topPadding));
            RootRectTransform.pivot = new Vector2(1, RootRectTransform.pivot.y);
            RootRectTransform.anchoredPosition = new Vector2(-anchoredX, RootRectTransform.anchoredPosition.y);
            return (this as T)!;
        }
        public T SetStretch<T>(float leftPadding = 0f, float rightPadding = 0f, float topPadding = 0f, float bottomPadding = 0f) where T: UIComponent{
            SetAnchorOffset(new Vector2(0, 0), new Vector2(1, 1), new Vector2(leftPadding, bottomPadding), new Vector2(-rightPadding, -topPadding));
            return (this as T)!;
        }
        public T SetPivot<T>(Vector2 pivot) where T : UIComponent {
            RootRectTransform.pivot = pivot;
            return (this as T)!;
        }
        public T SetAnchoredPosition<T>(Vector2 pos) where T : UIComponent {
            RootRectTransform.anchoredPosition = pos;
            return (this as T)!;
        }
        public T SetSize<T>(Vector2 size) where T: UIComponent {
            Size = size;
            RootRectTransform.sizeDelta = size;
            return (this as T)!;
        }
        public T SetWidth<T>(float width) where T : UIComponent {
            Size = new Vector2(width, Size.y);
            RootRectTransform.sizeDelta = Size;
            return (this as T)!;
        }
        public T SetHeight<T>(float height) where T : UIComponent {
            Size = new Vector2(Size.x, height);
            RootRectTransform.sizeDelta = Size;
            return (this as T)!;
        }
        public T EnableEvents<T>() where T : UIComponent {
            _eventsActive = true;
            return (this as T)!;
        }
        public T DisableEvents<T>() where T : UIComponent {
            _eventsActive = false;
            return (this as T)!;
        }
    }

    public class Content : UIComponent {

        /// <summary>
        /// Creates a Content UIComponent from an existing GameObject with a set parent
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="unityObject"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public Content(UIComponent? parent, GameObject unityObject, Vector2? size) : base(parent, unityObject) {
            if (size != null) {
                RootRectTransform.sizeDelta = size.Value;
                Size = size.Value;
            } else {
                Size = RootRectTransform.sizeDelta;
            }
        }

        public (Content left, Content right) SplitLeftRight(float leftWidth, float padding) {
            var leftContentObject = GameObject.Instantiate(SynthesisAssetCollection.GetModalPrefab("content-base"), base.RootGameObject.transform);
            var leftRt = leftContentObject.GetComponent<RectTransform>();
            leftRt.anchorMax = new Vector2(0f, 0.5f);
            leftRt.anchorMin = new Vector2(0f, 0.5f);
            leftRt.anchoredPosition = new Vector2(leftWidth / 2f, 0f);
            // leftRt.sizeDelta = new Vector2(leftWidth, leftRt.sizeDelta.y);
            var leftContent = new Content(this, leftContentObject, new Vector2(leftWidth, Size.y));

            var rightContentObject = GameObject.Instantiate(SynthesisAssetCollection.GetModalPrefab("content-base"), base.RootGameObject.transform);
            var rightRt = rightContentObject.GetComponent<RectTransform>();
            rightRt.anchorMax = new Vector2(1f, 0.5f);
            rightRt.anchorMin = new Vector2(1f, 0.5f);
            float rightWidth = (Size.x - leftWidth) - padding;
            rightRt.anchoredPosition = new Vector2(-rightWidth / 2f, 0f);
            // rightRt.sizeDelta = new Vector2(rightWidth, rightRt.sizeDelta.y);
            var rightContent = new Content(this, rightContentObject, new Vector2(rightWidth, Size.y));

            base.Children.Add(leftContent);
            base.Children.Add(rightContent);

            return (leftContent, rightContent);
        }
        public (Content top, Content bottom) SplitTopBottom(float topHeight, float padding) {
            throw new NotImplementedException();
        }

        public Label CreateLabel(float height = 15f) {
            var labelObj = GameObject.Instantiate(SynthesisAssetCollection.GetModalPrefab("label-base"), base.RootGameObject.transform);
            var label = new Label(this, labelObj, new Vector2(Size.x, height));
            base.Children.Add(label);
            return label;
        }
        public Toggle CreateToggle(bool isOn = false, string label = "New Toggle") {
            var toggleObj = GameObject.Instantiate(SynthesisAssetCollection.GetModalPrefab("toggle-base"), base.RootGameObject.transform);
            var toggle = new Toggle(this, toggleObj, isOn, label);
            base.Children.Add(toggle);
            return toggle;
        }
        public Slider CreateSlider(string label = "New Slider", string unitSuffix = "", float minValue = 0, float maxValue = 1, float currentValue = 0) {
            var sliderObj = GameObject.Instantiate(SynthesisAssetCollection.GetModalPrefab("slider-base"), base.RootGameObject.transform);
            var slider = new Slider(this, sliderObj, label, unitSuffix, minValue, maxValue, currentValue);
            base.Children.Add(slider);
            return slider;
        }
        public Button CreateButton(string text = "New Button") {
            var buttonObj = GameObject.Instantiate(SynthesisAssetCollection.GetModalPrefab("button-base"), base.RootGameObject.transform);
            var button = new Button(this, buttonObj, null);
            base.Children.Add(button);
            return button;
        }
        public LabeledButton CreateLabeledButton() {
            var lButtonObj = GameObject.Instantiate(SynthesisAssetCollection.GetModalPrefab("labeled-button-base"), base.RootGameObject.transform);
            var lButton = new LabeledButton(this, lButtonObj);
            base.Children.Add(lButton);
            return lButton;
        }
        public Dropdown CreateDropdown() {
            var dropdownObj = GameObject.Instantiate(SynthesisAssetCollection.GetModalPrefab("dropdown-base"), base.RootGameObject.transform);
            var dropdown = new Dropdown(this, dropdownObj, null);
            base.Children.Add(dropdown);
            return dropdown;
        }
        public InputField CreateInputField() {
            var inputFieldObj = GameObject.Instantiate(SynthesisAssetCollection.GetModalPrefab("input-field-base"), base.RootGameObject.transform);
            var inputField = new InputField(this, inputFieldObj);
            base.Children.Add(inputField);
            return inputField;
        }
        public ScrollView CreateScrollView() {
            throw new NotImplementedException();
        }
    }

    public class ScrollView : UIComponent {
        private Content _content;

        public ScrollView(UIComponent? parent, GameObject unityObject) : base(parent, unityObject) { }
    }

    public class Label : UIComponent {

        private TMP_Text _unityText;

        public string Text => _unityText.text;
        public FontStyles FontStyle => _unityText.fontStyle;

        public static readonly Func<Label, Label> VerticalLayoutTemplate = (Label label) => {
            return label.SetTopStretch(anchoredY: label.Parent!.HeightOfChildren - label.Size.y);
        };
        public static readonly Func<Label, Label> BigLabelTemplate = (Label label) => {
            return label.SetHeight<Label>(30).SetFontSize(22).ApplyTemplate(Label.VerticalLayoutTemplate).SetHorizontalAlignment(HorizontalAlignmentOptions.Left)
                .SetVerticalAlignment(VerticalAlignmentOptions.Middle);
        };

        public Label(UIComponent? parent, GameObject unityObject, Vector2? size) : base(parent, unityObject) {
            _unityText = unityObject.GetComponent<TMP_Text>();
            if (size.HasValue) {
                Size = size.Value;
                RootRectTransform.anchoredPosition = Vector2.zero;
                RootRectTransform.sizeDelta = Size;
            } else {
                size = RootRectTransform.sizeDelta;
            }
        }

        public Label SetText(string text) {
            _unityText.text = text;
            return this;
        }
        public Label SetFontSize(float fontSize) {
            _unityText.fontSize = fontSize;
            return this;
        }
        public Label SetHorizontalAlignment(HorizontalAlignmentOptions alignment) {
            _unityText.horizontalAlignment = alignment;
            return this;
        }
        public Label SetVerticalAlignment(VerticalAlignmentOptions alignment) {
            _unityText.verticalAlignment = alignment;
            return this;
        }
        public Label SetTopStretch(float leftPadding = 0f, float rightPadding = 0f, float anchoredY = 0f)
            => base.SetTopStretch<Label>(leftPadding, rightPadding, anchoredY);
        public Label SetBottomStretch(float leftPadding = 0f, float rightPadding = 0f, float anchoredY = 0f)
            => base.SetBottomStretch<Label>(leftPadding, rightPadding, anchoredY);
        public Label SetLeftStretch(float topPadding = 0f, float bottomPadding = 0f, float anchoredX = 0f)
            => base.SetLeftStretch<Label>(topPadding, bottomPadding, anchoredX);
        public Label SetRightStretch(float topPadding = 0f, float bottomPadding = 0f, float anchoredX = 0f)
            => base.SetRightStretch<Label>(topPadding, bottomPadding, anchoredX);
        public Label SetStretch(float leftPadding = 0f, float rightPadding = 0f, float topPadding = 0f, float bottomPadding = 0f)
            => base.SetStretch<Label>(leftPadding, rightPadding, topPadding, bottomPadding);
    }

    public class Toggle : UIComponent {

        public static readonly Func<Toggle, Toggle> VerticalLayoutTemplate = (Toggle toggle)
            => toggle.SetTopStretch<Toggle>(leftPadding: 15f, anchoredY: toggle.Parent!.HeightOfChildren - toggle.Size.y);

        public event Action<Toggle, bool> OnStateChanged;
        private Label _titleLabel;
        public Label TitleLabel => _titleLabel;
        private UToggle _unityToggle;

        public bool State {
            get => _unityToggle.isOn;
            set {
                SetState(value);
            }
        }

        public Toggle(UIComponent? parent, GameObject unityObject, bool isOn, string text) : base(parent, unityObject) {
            _titleLabel = new Label(this, RootGameObject.transform.Find("Label").gameObject, null);
            _unityToggle = RootGameObject.transform.Find("Toggle").GetComponent<UToggle>();
            _unityToggle.onValueChanged.AddListener(x => {{
                if (_eventsActive && OnStateChanged != null)
                    OnStateChanged(this, x);
            }});
            DisableEvents<Toggle>().SetState(isOn).EnableEvents<Toggle>();
            _titleLabel.SetText(text);
        }

        public Toggle SetState(bool state) {
            _unityToggle.isOn = state;
            return this;
        }
        public Toggle AddOnStateChangedEvent(Action<Toggle, bool> callback) {
            OnStateChanged += callback;
            return this;
        }
    }

    public class Slider : UIComponent {

        public static readonly Func<Slider, Slider> VerticalLayoutTemplate = (Slider slider)
            => slider.SetTopStretch<Slider>(leftPadding: 15f, anchoredY: slider.Parent!.HeightOfChildren - slider.Size.y);

        public event Action<Slider, float> OnValueChanged;
        private Func<float, string> _customValuePresentation = (x) => Math.Round(x, 2).ToString();
        private USlider _unitySlider;
        public (float min, float max) SlideRange => (_unitySlider.minValue, _unitySlider.maxValue);
        public float Value => _unitySlider.value;
        private Label _titleLabel;
        private Label _valueLabel;
        private string _unitSuffix = string.Empty;

        public Slider(UIComponent? parent, GameObject unityObject, string label, string unitSuffix, float minValue, float maxValue, float currentValue) : base(parent, unityObject) {
            var infoObj = unityObject.transform.Find("Info");
            _titleLabel = new Label(this, infoObj.Find("Label").gameObject, null);
            _valueLabel = new Label(this, infoObj.Find("Value").gameObject, null);
            _unitySlider = unityObject.transform.Find("Slider").GetComponent<USlider>();
            _titleLabel.SetText(label);
            _unitySlider.onValueChanged.AddListener(x => {
                var roundedVal = Math.Round(x, 2);
                _valueLabel.SetText(roundedVal.ToString() + (this._unitSuffix == string.Empty ? "" : this._unitSuffix));
                if (_eventsActive && OnValueChanged != null)
                    OnValueChanged(this, x);
            });

            if (unitSuffix != null)
                _unitSuffix = unitSuffix;
        }

        public Slider SetRange((float min, float max) range) => SetRange(range.min, range.max);
        public Slider SetRange(float min, float max) {
            _unitySlider.minValue = min;
            _unitySlider.maxValue = max;
            return this;
        }
        public Slider SetValue(float value, bool mute = false) {
            var before = _eventsActive;
            if (mute)
                _eventsActive = false;
            _unitySlider.value = value;
            if (mute)
                _eventsActive = before;
            return this;
        }
        public Slider AddOnValueChangedEvent(Action<Slider, float> callback) {
            OnValueChanged += callback;
            return this;
        }
        public Slider SetCustomValuePresentation(Func<float, string> mod) {
            if (_customValuePresentation == null)
                return this;
            _customValuePresentation = mod;
            return this;
        }
    }

    public class InputField : UIComponent {

        public static readonly Func<InputField, InputField> VerticalLayoutTemplate = (InputField inputField)
            => inputField.SetTopStretch<InputField>(leftPadding: 15f, anchoredY: inputField.Parent!.HeightOfChildren - inputField.Size.y);

        public event Action<InputField, string> OnValueChanged;
        private Label _hint;
        public Label Hint => _hint;
        private Label _label;
        public Label Label => _label;
        private TMP_InputField _tmpInput;
        public TMP_InputField.ContentType ContentType => _tmpInput.contentType;

        public InputField(UIComponent? parent, GameObject unityObject) : base(parent, unityObject) {
            var ifObj = unityObject.transform.Find("InputField");
            _tmpInput = ifObj.GetComponent<TMP_InputField>();
            _hint = new Label(this, ifObj.Find("Text Area").Find("Placeholder").gameObject, null);
            _label = new Label(this, unityObject.transform.Find("Label").gameObject, null);
            _tmpInput.onValueChanged.AddListener(x => {
                if (_eventsActive && OnValueChanged != null)
                    OnValueChanged(this, x);
            });
        }

        public InputField StepIntoHint(Action<Label> mod) {
            mod(_hint);
            return this;
        }
        public InputField StepIntoLabel(Action<Label> mod) {
            mod(_label);
            return this;
        }
        public InputField AddOnValueChangedEvent(Action<InputField, string> callback) {
            OnValueChanged += callback;
            return this;
        }
        public InputField SetContentType(TMP_InputField.ContentType type) {
            _tmpInput.contentType = type;
            return this;
        }
        public InputField SetValue(string val) {
            _tmpInput.SetTextWithoutNotify(val);
            return this;
        }
    }

    public class LabeledButton : UIComponent {

        public static readonly Func<LabeledButton, LabeledButton> VerticalLayoutTemplate = (LabeledButton lb) => {
            return lb.SetTopStretch<LabeledButton>(leftPadding: 15f, anchoredY: lb.Parent!.HeightOfChildren - lb.Size.y);
        };

        private Button _button;
        public Button Button => _button;
        private Label _label;
        public Label Label => _label;

        public LabeledButton(UIComponent? parent, GameObject unityObject) : base(parent, unityObject) {
            _button = new Button(this, RootGameObject.transform.Find("Button").gameObject, null);
            _label = new Label(this, RootGameObject.transform.Find("Label").gameObject, null);
        }

        public LabeledButton StepIntoButton(Action<Button> mod) {
            mod(_button);
            return this;
        }
        public LabeledButton StepIntoLabel(Action<Label> mod) {
            mod(_label);
            return this;
        }
    }

    public class Button : UIComponent {

        public static readonly Func<Button, Button> VerticalLayoutTemplate = (Button button)
            => button.SetTopStretch<Button>(leftPadding: 15f, anchoredY: button.Parent!.HeightOfChildren - button.Size.y);

        public event Action<Button> OnClicked;
        private Label _label;
        public Label Label => _label;
        private UButton _unityButton;
        // public UButton UnityButton => _unityButton;

        public Button(UIComponent? parent, GameObject unityObject, Vector2? size) : base(parent, unityObject) {
            if (size != null) {
                Size = size.Value;
            }

            _label = new Label(this, unityObject.transform.Find("Text (TMP)").gameObject, null);
            _unityButton = unityObject.GetComponent<UButton>();
            _unityButton.onClick.AddListener(() => {
                if (_eventsActive && OnClicked != null) {
                    OnClicked(this);
                }
            });
        }

        public Button StepIntoLabel(Action<Label> mod) {
            mod(_label);
            return this;
        }
        public Button AddOnClickedEvent(Action<Button> callback) {
            OnClicked += callback;
            return this;
        }
    }

    public class Dropdown : UIComponent {

        public static readonly Func<Dropdown, Dropdown> VerticalLayoutTemplate = (Dropdown dropdown)
            => dropdown.SetTopStretch<Dropdown>(leftPadding: 15f, anchoredY: dropdown.Parent!.HeightOfChildren - dropdown.Size.y);

        public event Action<Dropdown, int, TMP_Dropdown.OptionData> OnValueChanged;
        private Label _label;
        public Label Label => _label;
        private TMP_Dropdown _tmpDropdown;
        public IReadOnlyList<TMP_Dropdown.OptionData> Options => _tmpDropdown.options.AsReadOnly();

        public Dropdown(UIComponent? parent, GameObject unityObject, Vector2? size) : base(parent, unityObject) {
            if (size != null) {
                Size = size.Value;
            }

            _label = new Label(this, unityObject.transform.Find("Label").gameObject, null);
            _tmpDropdown = unityObject.transform.Find("Dropdown").GetComponent<TMP_Dropdown>();

            _tmpDropdown.onValueChanged.AddListener(x => {
                // TODO?
                if (_eventsActive && OnValueChanged != null)
                    OnValueChanged(this, x, this.Options[x]);
            });
        }

        public Dropdown SetOptions(string[] options) {
            _tmpDropdown.ClearOptions();
            var optionData = new List<TMP_Dropdown.OptionData>();
            options.ForEach(x => optionData.Add(new TMP_Dropdown.OptionData(x)));
            _tmpDropdown.AddOptions(optionData);
            return this;
        }
        public Dropdown SetValue(int index, bool mute = false) {
            var original = _eventsActive;
            if (mute)
                _eventsActive = false;
            _tmpDropdown.value = index;
            if (mute)
                _eventsActive = original;
            return this;
        }
        public Dropdown AddOnValueChangedEvent(Action<Dropdown, int, TMP_Dropdown.OptionData> callback) {
            OnValueChanged += callback;
            return this;
        }
        public Dropdown StepIntoLabel(Action<Label> mod) {
            mod(_label);
            return this;
        }
    }
}
