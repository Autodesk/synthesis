using System;
using System.Collections;
using System.Collections.Generic;
using SynthesisAPI.Utilities;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

using UButton = UnityEngine.UI.Button;
using UToggle = UnityEngine.UI.Toggle;
using USlider = UnityEngine.UI.Slider;
using UImage = UnityEngine.UI.Image;
using UScrollbar = UnityEngine.UI.Scrollbar;
using UScrollView = UnityEngine.UI.ScrollRect;

using Logger = SynthesisAPI.Utilities.Logger;
using Math = System.Math;

#nullable enable

namespace Synthesis.UI.Dynamic {

    #region Abstracts

    public abstract class PanelDynamic {
        private float _leftContentPadding, _rightContentPadding;
        public float LeftContentPadding => _leftContentPadding;
        public float RightContentPadding => _rightContentPadding;

        private Vector2 _mainContentSize; // Shouldn't really be used after init is called
        private GameObject _unityObject;
        protected GameObject UnityObject => _unityObject;
        // Default for Modal
        private Button _cancelButton;
        protected Button CancelButton => _cancelButton;
        private Button _acceptButton;
        protected Button AcceptButton => _acceptButton;
        private Image _panelImage;
        protected Image PanelImage => _panelImage;
        private Image _panelBackground;
        protected Image PanelBackground => _panelBackground;
        private Label _title;
        protected Label Title => _title;
        
        private Content _mainContent;
        protected Content MainContent => _mainContent;

        private bool _hidden = false;
        public bool Hidden {
            get => _hidden;
            set {
                if (value != _hidden) {
                    _hidden = value;
                    if (_hidden) {
                        _unityObject.SetActive(false);
                    } else {
                        _unityObject.SetActive(true);
                    }
                    OnVisibilityChange();
                }
            }
        }

        protected PanelDynamic(Vector2 mainContentSize, float leftContentPadding = 20f, float rightContentPadding = 20f) {
            _mainContentSize = mainContentSize;
            _leftContentPadding = leftContentPadding;
            _rightContentPadding = rightContentPadding;
        }

        public void Create_Internal(GameObject unityObject) {
            _unityObject = unityObject;

            // Grab Customizable Modal Components
            var header = _unityObject.transform.Find("Header");
            var headerRt = header.GetComponent<RectTransform>();
            _panelImage = new Image(null, header.Find("Image").gameObject);
            // _panelImage.SetColor(new Color(1, 1, 1, 0));

            _panelBackground = new Image(null, unityObject);
            _panelBackground.SetColor(ColorManager.TryGetColor(ColorManager.SYNTHESIS_BLACK));
            _panelBackground.SetCornerRadius(15);

            _title = new Label(null, header.Find("Title").gameObject, null);

            var footer = _unityObject.transform.Find("Footer");
            var footerRt = footer.GetComponent<RectTransform>();
            _cancelButton = new Button(null!, footer.Find("Cancel").gameObject, null);
            _cancelButton.AddOnClickedEvent(b => {
                if (!DynamicUIManager.ClosePanel(this.GetType()))
                    Logger.Log("Failed to Close Panel", LogLevel.Error);
            });
            _cancelButton.Image.SetColor(ColorManager.SYNTHESIS_CANCEL);
            _cancelButton.Label.SetColor(ColorManager.TryGetColor(ColorManager.SYNTHESIS_ORANGE_CONTRAST_TEXT));
            _acceptButton = new Button(null!, footer.Find("Accept").gameObject, null);
            _acceptButton.Image.SetColor(ColorManager.SYNTHESIS_ACCEPT);
            _acceptButton.Label.SetColor(ColorManager.TryGetColor(ColorManager.SYNTHESIS_ORANGE_CONTRAST_TEXT));

            // Create Inital Content Component
            var hiddenContentT = _unityObject.transform.Find("Content");
            var hiddenRt = hiddenContentT.GetComponent<RectTransform>();
            hiddenRt.sizeDelta = new Vector2(hiddenRt.sizeDelta.x, _mainContentSize.y);
            hiddenRt.anchorMin = new Vector2(0, 1);
            hiddenRt.anchorMax = new Vector2(1, 1);
            hiddenRt.pivot = new Vector2(0.5f, 1);
            hiddenRt.anchoredPosition = new Vector2(0, -headerRt.sizeDelta.y);
            var actualContentObj = GameObject.Instantiate(SynthesisAssetCollection.GetUIPrefab("content-base"), hiddenContentT);
            actualContentObj.name = "CentralContent";
            var contentRt = actualContentObj.GetComponent<RectTransform>();
            contentRt.offsetMax = new Vector2(-_rightContentPadding, contentRt.offsetMax.y);
            contentRt.offsetMin = new Vector2(_leftContentPadding, contentRt.offsetMin.y);
            var modalRt = _unityObject.GetComponent<RectTransform>();
            modalRt.sizeDelta = new Vector2(
                _mainContentSize.x + (_leftContentPadding + _rightContentPadding),
                hiddenRt.sizeDelta.y + headerRt.sizeDelta.y + footerRt.sizeDelta.y
            );
            _mainContent = new Content(null!, actualContentObj, _mainContentSize);
        }

        public abstract void Create();
        public abstract void Update();
        public abstract void Delete();

        protected virtual void OnVisibilityChange() { }

        public void Delete_Internal() {
            GameObject.Destroy(_unityObject);
        }
    }

    public abstract class ModalDynamic {

        public const float MAIN_CONTENT_HORZ_PADDING = 20f;

        private Vector2 _mainContentSize; // Shouldn't really be used after init is called
        private GameObject _unityObject;

        // Default for Modal
        private Button _cancelButton;
        protected Button CancelButton => _cancelButton;
        private Button _acceptButton;
        protected Button AcceptButton => _acceptButton;
        private Image _modalImage;
        protected Image ModalImage => _modalImage;
        private Image _modalBackground;
        protected Image ModalBackground => _modalBackground;
        private Label _title;
        protected Label Title => _title;
        private Label _description;
        protected Label Description => _description;
        
        private Content _mainContent;
        protected Content MainContent => _mainContent;

        protected ModalDynamic(Vector2 mainContentSize) {
            _mainContentSize = mainContentSize;
        }

        public void Create_Internal(GameObject unityObject) {
            _unityObject = unityObject;

            // Grab Customizable Modal Components
            var header = _unityObject.transform.Find("Header");
            var headerRt = header.GetComponent<RectTransform>();
            _modalImage = new Image(null, header.Find("Image").gameObject);
            // _modalImage.SetColor(new Color(1, 1, 1, 0));

            _modalBackground = new Image(null, unityObject);
            _modalBackground.SetColor(ColorManager.TryGetColor(ColorManager.SYNTHESIS_BLACK));
            _modalBackground.SetCornerRadius(20);
            
            _title = new Label(null, header.Find("Title").gameObject, null);
            _title.SetColor(ColorManager.TryGetColor(ColorManager.SYNTHESIS_WHITE));
            
            _description = new Label( null, header.Find("Description").gameObject, null);
            _description.SetColor(ColorManager.TryGetColor(ColorManager.SYNTHESIS_WHITE));

            var footer = _unityObject.transform.Find("Footer");
            var footerRt = footer.GetComponent<RectTransform>();
            _cancelButton = new Button(null!, footer.Find("Cancel").gameObject, null);
            _cancelButton.AddOnClickedEvent(b => {
                if (!DynamicUIManager.CloseActiveModal())
                    Logger.Log("Failed to Close Modal", LogLevel.Error);
            });
            _cancelButton.Image.SetColor(ColorManager.SYNTHESIS_CANCEL);
            _cancelButton.Label.SetColor(ColorManager.TryGetColor(ColorManager.SYNTHESIS_ORANGE_CONTRAST_TEXT));
            _acceptButton = new Button(null!, footer.Find("Accept").gameObject, null);
            _acceptButton.Image.SetColor(ColorManager.SYNTHESIS_ACCEPT);
            _acceptButton.Label.SetColor(ColorManager.TryGetColor(ColorManager.SYNTHESIS_ORANGE_CONTRAST_TEXT));

            // Create Inital Content Component
            var hiddenContentT = _unityObject.transform.Find("Content");
            var hiddenRt = hiddenContentT.GetComponent<RectTransform>();
            hiddenRt.sizeDelta = new Vector2(hiddenRt.sizeDelta.x, _mainContentSize.y);
            hiddenRt.anchorMin = new Vector2(0, 1);
            hiddenRt.anchorMax = new Vector2(1, 1);
            hiddenRt.pivot = new Vector2(0.5f, 1);
            hiddenRt.anchoredPosition = new Vector2(0, -headerRt.sizeDelta.y);
            var actualContentObj = GameObject.Instantiate(SynthesisAssetCollection.GetUIPrefab("content-base"), hiddenContentT);
            actualContentObj.name = "CentralContent";
            var contentRt = actualContentObj.GetComponent<RectTransform>();
            contentRt.offsetMax = new Vector2(-MAIN_CONTENT_HORZ_PADDING, contentRt.offsetMax.y);
            contentRt.offsetMin = new Vector2(MAIN_CONTENT_HORZ_PADDING, contentRt.offsetMin.y);
            var modalRt = _unityObject.GetComponent<RectTransform>();
            modalRt.sizeDelta = new Vector2(
                _mainContentSize.x + (MAIN_CONTENT_HORZ_PADDING * 2),
                hiddenRt.sizeDelta.y + headerRt.sizeDelta.y + footerRt.sizeDelta.y
            );
            _mainContent = new Content(null!, actualContentObj, _mainContentSize);
        }

        public abstract void Create();
        public abstract void Update();
        public abstract void Delete();

        public void Delete_Internal() {
            GameObject.Destroy(_unityObject);
        }
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
        public Rect RectOfChildren(UIComponent negate = null!) {
            if (Children.Count == 0)
                return new Rect();
            bool hasInital = false;
            Rect r = new Rect();
            for (int i = 0; i < Children.Count; i++) {
                if (Children[i] == negate)
                    continue;
                if (!hasInital) {
                    r = Children[i].RootRectTransform.GetOffsetRect();
                    continue;
                }
                var childTrans = Children[i].RootRectTransform;
                var childRect = childTrans.GetOffsetRect();
                
                if (childRect.xMin < r.xMin)
                    r.xMin = childRect.xMin;
                if (childRect.xMax > r.xMax)
                    r.xMax = childRect.xMax;
                if (childRect.yMin < r.yMin)
                    r.yMin = childRect.yMin;
                if (childRect.yMax > r.yMax)
                    r.yMax = childRect.yMax;
            }
            return r;
        }

        protected bool _eventsActive = true;
        public bool EventsActive => _eventsActive;

        public Vector2 Size { get; protected set; }
        public GameObject RootGameObject { get; protected set; }
        public RectTransform RootRectTransform { get; protected set; }
        public UIComponent? Parent { get; protected set; }
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
        public T ShiftOffsetMax<T>(Vector2 shift) where T : UIComponent {
            RootRectTransform.offsetMax += shift;
            return (this as T)!;
        }
        public T ShiftOffsetMin<T>(Vector2 shift) where T : UIComponent {
            RootRectTransform.offsetMin += shift;
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
        public T SetAnchors<T>(Vector2 min, Vector2 max) where T : UIComponent {
            RootRectTransform.anchorMin = min;
            RootRectTransform.anchorMax = max;
            return (this as T)!;
        }
        public T SetAnchorCenter<T>() where T : UIComponent {
            RootRectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            RootRectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            return (this as T)!;
        }
        public T SetAnchorTop<T>() where T : UIComponent {
            RootRectTransform.anchorMin = new Vector2(0.5f, 1);
            RootRectTransform.anchorMax = new Vector2(0.5f, 1);
            return (this as T)!;
        }
    }

    #endregion

    #region Components

    public class Content : UIComponent {

        private Image? _image;
        public Image? Image => _image;

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

            var uImg = unityObject.GetComponent<UImage>();
            if (uImg != null) {
                _image = new Image(this, unityObject);
            }
        }

        public (Content left, Content right) SplitLeftRight(float leftWidth, float padding) {
            var leftContentObject = GameObject.Instantiate(SynthesisAssetCollection.GetUIPrefab("content-base"), base.RootGameObject.transform);
            var leftRt = leftContentObject.GetComponent<RectTransform>();
            leftRt.anchorMax = new Vector2(0f, 0.5f);
            leftRt.anchorMin = new Vector2(0f, 0.5f);
            leftRt.anchoredPosition = new Vector2(leftWidth / 2f, 0f);
            // leftRt.sizeDelta = new Vector2(leftWidth, leftRt.sizeDelta.y);
            var leftContent = new Content(this, leftContentObject, new Vector2(leftWidth, Size.y));

            var rightContentObject = GameObject.Instantiate(SynthesisAssetCollection.GetUIPrefab("content-base"), base.RootGameObject.transform);
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
            var labelObj = GameObject.Instantiate(SynthesisAssetCollection.GetUIPrefab("label-base"), base.RootGameObject.transform);
            var label = new Label(this, labelObj, new Vector2(Size.x, height));
            base.Children.Add(label);
            return label;
        }
        public Toggle CreateToggle(bool isOn = false, string label = "New Toggle") {
            var toggleObj = GameObject.Instantiate(SynthesisAssetCollection.GetUIPrefab("toggle-base"), base.RootGameObject.transform);
            var toggle = new Toggle(this, toggleObj, isOn, label);
            base.Children.Add(toggle);
            return toggle;
        }
        public Slider CreateSlider(string label = "New Slider", string unitSuffix = "", float minValue = 0, float maxValue = 1, float currentValue = 0) {
            var sliderObj = GameObject.Instantiate(SynthesisAssetCollection.GetUIPrefab("slider-base"), base.RootGameObject.transform);
            var slider = new Slider(this, sliderObj, label, unitSuffix, minValue, maxValue, currentValue);
            base.Children.Add(slider);
            return slider;
        }
        public Button CreateButton(string text = "New Button") {
            var buttonObj = GameObject.Instantiate(SynthesisAssetCollection.GetUIPrefab("button-base"), base.RootGameObject.transform);
            var button = new Button(this, buttonObj, null);
            button.StepIntoLabel(l => l.SetText(text));
            base.Children.Add(button);
            return button;
        }
        public LabeledButton CreateLabeledButton() {
            var lButtonObj = GameObject.Instantiate(SynthesisAssetCollection.GetUIPrefab("labeled-button-base"), base.RootGameObject.transform);
            var lButton = new LabeledButton(this, lButtonObj);
            base.Children.Add(lButton);
            return lButton;
        }
        public Dropdown CreateDropdown() {
            var dropdownObj = GameObject.Instantiate(SynthesisAssetCollection.GetUIPrefab("dropdown-base"), base.RootGameObject.transform);
            var dropdown = new Dropdown(this, dropdownObj, null);
            base.Children.Add(dropdown);
            return dropdown;
        }
        public LabeledDropdown CreateLabeledDropdown() {
            var lDropdownObj = GameObject.Instantiate(SynthesisAssetCollection.GetUIPrefab("labeled-dropdown-base"), base.RootGameObject.transform);
            var lDropdown = new LabeledDropdown(this, lDropdownObj);
            base.Children.Add(lDropdown);
            return lDropdown;
        }
        public InputField CreateInputField() {
            var inputFieldObj = GameObject.Instantiate(SynthesisAssetCollection.GetUIPrefab("input-field-base"), base.RootGameObject.transform);
            var inputField = new InputField(this, inputFieldObj);
            base.Children.Add(inputField);
            return inputField;
        }
        public ScrollView CreateScrollView() {
            var scrollViewObj = GameObject.Instantiate(SynthesisAssetCollection.GetUIPrefab("scroll-view-base"), base.RootGameObject.transform);
            var scrollView = new ScrollView(this, scrollViewObj);
            base.Children.Add(scrollView);
            return scrollView;
        }
        public Content CreateSubContent(Vector2 size) {
            var contentObj = GameObject.Instantiate(SynthesisAssetCollection.GetUIPrefab("content-base"), base.RootGameObject.transform);
            var content = new Content(this, contentObj, size);
            base.Children.Add(content);
            return content;
        }

        public Content StepIntoImage(Action<Image> mod) {
            if (_image != null)
                mod(_image);
            return this;
        }

        public Content EnsureImage() {
            if (_image == null) {
                base.RootGameObject.AddComponent<UImage>();
                _image = new Image(this, base.RootGameObject);
                _image.SetSprite(SynthesisAssetCollection.GetSpriteByName("250r-rounded"));
                _image.SetMultiplier(50);
            }
            return this;
        }
    }

    public class ScrollView : UIComponent {
        private UScrollView _unityScrollView;
        private Image _backgroundImage;
        private Content _content;
        public Content Content => _content;

        public ScrollView(UIComponent? parent, GameObject unityObject) : base(parent, unityObject) {
            _unityScrollView = unityObject.GetComponent<UScrollView>();

            _content = new Content(this, unityObject.transform.Find("Viewport").Find("Content").gameObject, null);

            _backgroundImage = new Image(this, unityObject);
            _backgroundImage.SetColor(ColorManager.SYNTHESIS_BLACK_ACCENT);
        }

        public ScrollView StepIntoContent(Action<Content> mod) {
            mod(_content);
            return this;
        }
    }

    public class Label : UIComponent {

        private TMP_Text _unityText;

        public string Text => _unityText.text;
        public FontStyles FontStyle => _unityText.fontStyle;

        public static readonly Func<Label, Label> VerticalLayoutTemplate = (Label label) => {
            return label.SetTopStretch(anchoredY: label.Parent!.HeightOfChildren - label.Size.y + 15f);
        };
        public static readonly Func<Label, Label> BigLabelTemplate = (Label label) => {
            return label.SetHeight<Label>(30).SetFontSize(24).ApplyTemplate(Label.VerticalLayoutTemplate).SetHorizontalAlignment(HorizontalAlignmentOptions.Left)
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

            SetColor(ColorManager.TryGetColor(ColorManager.SYNTHESIS_WHITE));
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
        public Label SetFont(TMP_FontAsset font) {
            _unityText.font = font;
            return this;
        }
        public Label SetFontStyle(FontStyles styles) {
            _unityText.fontStyle = styles;
            return this;
        }
        public Label SetColor(string c)
            => SetColor(ColorManager.TryGetColor(c));
        public Label SetColor(Color c) {
            _unityText.color = c;
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
            => toggle.SetTopStretch<Toggle>(leftPadding: 15f, anchoredY: toggle.Parent!.HeightOfChildren - toggle.Size.y + 15f);

        public event Action<Toggle, bool> OnStateChanged;
        private Label _titleLabel;
        public Label TitleLabel => _titleLabel;
        private UToggle _unityToggle;

        private Image _disabledImage;
        public Image DisabledImage => _disabledImage;
        private Image _enabledImage;
        public Image EnabledImage => _enabledImage;

        private Color _disabledColor;
        private Color DisabledColor {
            get => _disabledColor;
            set {
                _disabledColor = value;
                _disabledImage.SetColor(_disabledColor);
            }
        }
        private Color _enabledColor;
        public Color EnabledColor {
            get => _enabledColor;
            set {
                _enabledColor = value;
                _enabledImage.SetColor(_enabledColor);
            }
        }

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

            _disabledImage = new Image(this, _unityToggle.transform.Find("Background").gameObject);
            _enabledImage = new Image(this, _unityToggle.transform.Find("Background").Find("Checkmark").gameObject);

            DisabledColor = ColorManager.TryGetColor(ColorManager.SYNTHESIS_BLACK_ACCENT);
            EnabledColor = ColorManager.TryGetColor(ColorManager.SYNTHESIS_ORANGE);
        }

        public Toggle SetState(bool state) {
            _unityToggle.isOn = state;
            return this;
        }
        public Toggle AddOnStateChangedEvent(Action<Toggle, bool> callback) {
            OnStateChanged += callback;
            return this;
        }
        public Toggle SetEnabledColor(Color c) {
            EnabledColor = c;
            return this;
        }
        public Toggle SetEnabledColor(string s) {
            EnabledColor = ColorManager.TryGetColor(s);
            return this;
        }
        public Toggle SetDisabledColor(Color c) {
            DisabledColor = c;
            return this;
        }
        public Toggle SetDisabledColor(string s) {
            DisabledColor = ColorManager.TryGetColor(s);
            return this;
        }
    }

    public class Slider : UIComponent {

        public static readonly Func<Slider, Slider> VerticalLayoutTemplate = (Slider slider)
            => slider.SetTopStretch<Slider>(leftPadding: 15f, anchoredY: slider.Parent!.HeightOfChildren - slider.Size.y + 15f);

        public event Action<Slider, float> OnValueChanged;
        private Func<float, string> _customValuePresentation = (x) => Math.Round(x, 2).ToString();
        private USlider _unitySlider;
        public (float min, float max) SlideRange => (_unitySlider.minValue, _unitySlider.maxValue);
        public float Value => _unitySlider.value;
        private Label _titleLabel;
        private Label _valueLabel;
        private string _unitSuffix = string.Empty;

        private Image _backgroundImage;
        private Image _fillImage;
        private Image _handleImage;

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

            _backgroundImage = new Image(this, _unitySlider.transform.Find("Background").gameObject);
            _backgroundImage.SetColor(ColorManager.TryGetColor(ColorManager.SYNTHESIS_BLACK_ACCENT));

            _fillImage = new Image(this, _unitySlider.transform.Find("Fill Area").Find("Fill").gameObject);
            _fillImage.SetColor(ColorManager.TryGetColor(ColorManager.SYNTHESIS_ORANGE));

            _handleImage = new Image(this, _unitySlider.transform.Find("Handle Slide Area").Find("Handle").gameObject);
            _handleImage.SetColor(ColorManager.TryGetColor(ColorManager.SYNTHESIS_ORANGE_ACCENT));

            if (unitSuffix != null)
                _unitSuffix = unitSuffix;

            SetRange((minValue, maxValue));
            SetValue(currentValue);
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
        public Slider SetSlideDirection(USlider.Direction direction) {
            _unitySlider.direction = direction;
            return this;
        }
        public Slider SetCustomValuePresentation(Func<float, string> mod) {
            if (_customValuePresentation == null)
                return this;
            _customValuePresentation = mod;
            return this;
        }
        public Slider StepIntoTitleLabel(Action<Label> mod) {
            mod(_titleLabel);
            return this;
        }
        public Slider StepIntoValueLabel(Action<Label> mod) {
            mod(_valueLabel);
            return this;
        }
        public Slider StepIntoBackgroundImage(Action<Image> mod) {
            mod(_backgroundImage);
            return this;
        }
        public Slider StepIntoFillImage(Action<Image> mod) {
            mod(_fillImage);
            return this;
        }
        public Slider StepIntoHandleImage(Action<Image> mod) {
            mod(_handleImage);
            return this;
        }
    }

    public class InputField : UIComponent {

        public static readonly Func<InputField, InputField> VerticalLayoutTemplate = (InputField inputField)
            => inputField.SetTopStretch<InputField>(leftPadding: 15f, anchoredY: inputField.Parent!.HeightOfChildren - inputField.Size.y + 15f);

        public event Action<InputField, string> OnValueChanged;
        private Label _hint;
        public Label Hint => _hint;
        private Label _label;
        public Label Label => _label;
        private Image _backgroundImage;
        public Image BackgroundImage => _backgroundImage;
        private TMP_InputField _tmpInput;
        public TMP_InputField.ContentType ContentType => _tmpInput.contentType;
        public string Value => _tmpInput.text;

        public InputField(UIComponent? parent, GameObject unityObject) : base(parent, unityObject) {
            var ifObj = unityObject.transform.Find("InputField");
            _tmpInput = ifObj.GetComponent<TMP_InputField>();
            _hint = new Label(this, ifObj.Find("Text Area").Find("Placeholder").gameObject, null);
            _label = new Label(this, unityObject.transform.Find("Label").gameObject, null);
            _tmpInput.onValueChanged.AddListener(x => {
                if (_eventsActive && OnValueChanged != null)
                    OnValueChanged(this, x);
            });

            _backgroundImage = new Image(this, ifObj.gameObject);
            _backgroundImage.SetColor(ColorManager.TryGetColor(ColorManager.SYNTHESIS_BLACK_ACCENT));
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
            return lb.SetTopStretch<LabeledButton>(leftPadding: 15f, anchoredY: lb.Parent!.HeightOfChildren - lb.Size.y + 15f);
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
            => button.SetTopStretch<Button>(leftPadding: 15f, anchoredY: button.Parent!.HeightOfChildren - button.Size.y + 15f);

        public event Action<Button> OnClicked;
        private Label? _label;
        public Label? Label => _label;
        private UButton _unityButton;
        private Image _image;
        public Image Image => _image;
        // public UButton UnityButton => _unityButton;

        public Button(UIComponent? parent, GameObject unityObject, Vector2? size) : base(parent, unityObject) {
            if (size != null) {
                Size = size.Value;
            }

            var labelTransform = unityObject.transform.Find("Text (TMP)");
            if (labelTransform != null) {
                _label = new Label(this, labelTransform.gameObject, null);
                _label.SetColor(ColorManager.SYNTHESIS_ORANGE_CONTRAST_TEXT);
            }

            _unityButton = unityObject.GetComponent<UButton>();
            _unityButton.onClick.AddListener(() => {
                if (_eventsActive && OnClicked != null) {
                    OnClicked(this);
                }
            });

            _image = new Image(this, unityObject);
            _image.SetColor(ColorManager.TryGetColor(ColorManager.SYNTHESIS_ORANGE));
        }

        public Button StepIntoLabel(Action<Label> mod) {
            if (_label != null)
                mod(_label);
            return this;
        }
        public Button StepIntoImage(Action<Image> mod) {
            mod(_image);
            return this;
        }
        public Button AddOnClickedEvent(Action<Button> callback) {
            OnClicked += callback;
            return this;
        }
    }

    public class Dropdown : UIComponent {

        public static readonly Func<Dropdown, Dropdown> VerticalLayoutTemplate = (Dropdown dropdown)
            => dropdown.SetTopStretch<Dropdown>(leftPadding: 15f, anchoredY: dropdown.Parent!.HeightOfChildren - dropdown.Size.y + 15f);

        public event Action<Dropdown, int, TMP_Dropdown.OptionData> OnValueChanged;
        private Image _image;
        public Image Image => _image;
        private Content _viewport;
        public Content Viewport => _viewport;
        private TMP_Dropdown _tmpDropdown;
        public IReadOnlyList<TMP_Dropdown.OptionData> Options => _tmpDropdown.options.AsReadOnly();
        public int Value => _tmpDropdown.value;
        public TMP_Dropdown.OptionData SelectedOption => _tmpDropdown.options[Value];
        
        private Image _headerImage;
        public Image HeaderImage => _headerImage;
        private Label _headerLabel;
        public Label HeaderLabel => _headerLabel;

        private Image _itemBackgroundImage;
        public Image ItemBackgroundImage => _itemBackgroundImage;
        private Image _itemCheckmarkImage;
        public Image ItemCheckmarkImage => _itemCheckmarkImage;
        private Label _itemLabel;
        public Label ItemLabel => _itemLabel;
        private Image _viewportImage;
        public Image ViewportImage => ViewportImage;

        public Dropdown(UIComponent? parent, GameObject unityObject, Vector2? size) : base(parent, unityObject) {
            if (size != null) {
                Size = size.Value;
            }

            _image = new Image(this, unityObject.transform.Find("Header").Find("Arrow").gameObject);
            _viewport = new Content(this, unityObject.transform.Find("Template").Find("Viewport").gameObject, null);
            _tmpDropdown = unityObject.transform.GetComponent<TMP_Dropdown>();

            _tmpDropdown.onValueChanged.AddListener(x => {
                // TODO?
                if (_eventsActive && OnValueChanged != null)
                    OnValueChanged(this, x, this.Options[x]);
            });
            var eventHandler = _tmpDropdown.gameObject.AddComponent<UIEventHandler>();
            eventHandler.OnPointerClickedEvent += e => {
                ShowOnTop();
            };

            _headerImage = new Image(this, unityObject.transform.Find("Header").gameObject);
            _headerImage.SetColor(ColorManager.TryGetColor(ColorManager.SYNTHESIS_ORANGE));

            _headerLabel = new Label(this, unityObject.transform.Find("Header").Find("Label").gameObject, null);
            _headerLabel.SetColor(ColorManager.TryGetColor(ColorManager.SYNTHESIS_ORANGE_CONTRAST_TEXT));

            var itemObj = unityObject.transform.Find("Template").Find("Viewport").Find("Content").Find("Item");

            _itemBackgroundImage = new Image(this, itemObj.Find("Item Background").gameObject);
            _itemBackgroundImage.SetColor(ColorManager.TryGetColor(ColorManager.SYNTHESIS_ORANGE));

            _itemCheckmarkImage = new Image(this, itemObj.Find("Item Checkmark").gameObject);
            _itemCheckmarkImage.SetColor(ColorManager.TryGetColor(ColorManager.SYNTHESIS_BLACK));

            _itemLabel = new Label(this, itemObj.Find("Item Label").gameObject, null);
            _itemLabel.SetColor(ColorManager.TryGetColor(ColorManager.SYNTHESIS_ORANGE_CONTRAST_TEXT));

            _viewportImage = new Image(this, unityObject.transform.Find("Template").Find("Viewport").gameObject);
            _viewportImage.SetColor(ColorManager.TryGetColor(ColorManager.SYNTHESIS_ORANGE));

            // TODO: Get some more control over the individual items in the dropdown
            // _viewport.StepIntoImage(i => i.SetColor(ColorManager.TryGetColor(ColorManager.SYNTHESIS_BLACK_ACCENT)));
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
        public Dropdown ShowOnTop() {
            RootRectTransform.SetAsLastSibling();
            return this;
        }
        public Dropdown StepIntoImage(Action<Image> mod) {
            mod(_image);
            return this;
        }
    }

    public class LabeledDropdown : UIComponent {

        private Dropdown _dropdown;
        public Dropdown Dropdown => _dropdown;
        private Label _label;
        public Label Label => _label;

        public LabeledDropdown(UIComponent? parent, GameObject unityObject) : base(parent, unityObject) {
            _dropdown = new Dropdown(this, unityObject.transform.Find("Dropdown").gameObject, null);
            _label = new Label(this, unityObject.transform.Find("Label").gameObject, null);
        }

        public LabeledDropdown StepIntoDropdown(Action<Dropdown> mod) {
            mod(_dropdown);
            return this;
        }
        public LabeledDropdown StepIntoLabel(Action<Label> mod) {
            mod(_label);
            return this;
        }
    }

    public class Image : UIComponent {

        private UImage _unityImage;
        public Sprite Sprite {
            get => _unityImage.sprite;
            set {
                _unityImage.sprite = value;
            }
        }
        public Color Color {
            get => _unityImage.color;
            set {
                _unityImage.color = value;
            }
        }

        public Image(UIComponent? parent, GameObject unityObject) : base(parent, unityObject) {
            _unityImage = unityObject.GetComponent<UImage>();
        }

        public Image SetSprite(Sprite s) {
            Sprite = s;
            return this;
        }
        public Image SetColor(string c)
            => SetColor(ColorManager.TryGetColor(c));
        public Image SetColor(Color c) {
            _unityImage.color = c;
            return this;
        }
        public Image SetCornerRadius(float r) {
            _unityImage.pixelsPerUnitMultiplier = 250f / r;
            return this;
        }
        public Image SetMultiplier(float m) {
            _unityImage.pixelsPerUnitMultiplier = m;
            return this;
        }
    }

    public class Scrollbar : UIComponent {

        public UScrollbar _unityScrollbar;
        private Image _backgroundImage;
        public Image BackgroundImage => _backgroundImage;
        private Image _handleImage;
        public Image HandleImage => _handleImage;

        public Scrollbar(UIComponent? parent, GameObject unityObject) : base(parent, unityObject) {
            _unityScrollbar = unityObject.GetComponent<UScrollbar>();
            
            _backgroundImage = new Image(this, unityObject);
            _backgroundImage.SetColor(ColorManager.SYNTHESIS_WHITE_ACCENT);

            _handleImage = new Image(this, unityObject.transform.Find("Sliding Area").Find("Handle").gameObject);
            _handleImage.SetColor(ColorManager.SYNTHESIS_WHITE);
        }
    }

    #endregion

    public class UIEventHandler : MonoBehaviour, IPointerClickHandler {
        public event Action<PointerEventData> OnPointerClickedEvent;

        public void OnPointerClick(PointerEventData pointerEventData) {
            if (OnPointerClickedEvent != null)
                OnPointerClickedEvent(pointerEventData);
        }
    }
}
