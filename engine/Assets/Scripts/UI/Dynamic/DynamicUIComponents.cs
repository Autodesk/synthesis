using System;
using System.Collections;
using System.Collections.Generic;
using Synthesis.Util;
using SynthesisAPI.Utilities;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UI;
using UI.Dynamic.Modals.Spawning;
using UI.EventListeners;
using UnityEngine.EventSystems;
using UnityEngine.PlayerLoop;
using Utilities.ColorManager;
using UButton     = UnityEngine.UI.Button;
using UToggle     = UnityEngine.UI.Toggle;
using USlider     = UnityEngine.UI.Slider;
using UImage      = UnityEngine.UI.Image;
using UScrollbar  = UnityEngine.UI.Scrollbar;
using UScrollView = UnityEngine.UI.ScrollRect;

using Logger = SynthesisAPI.Utilities.Logger;
using Math   = System.Math;

#nullable enable

namespace Synthesis.UI.Dynamic {

#region Abstracts

    public abstract class PanelDynamic {
        public const float MAIN_CONTENT_HORZ_PADDING = 25f;
        public Vector2 TweenDirection                = Vector2.right;
        public bool IsClosing                        = false;

        private float _leftContentPadding, _rightContentPadding;
        public float LeftContentPadding  => _leftContentPadding;
        public float RightContentPadding => _rightContentPadding;

        private Vector2 _mainContentSize; // Shouldn't really be used after init is called
        private GameObject _unityObject;
        public GameObject UnityObject => _unityObject;
        // Default for Modal
        private Transform _footer;
        protected Transform Footer => _footer;
        private Button _cancelButton;
        private RectTransform _headerRt;
        protected RectTransform HeaderRt => _headerRt;
        protected Button CancelButton    => _cancelButton;
        private Button _acceptButton;
        protected Button AcceptButton => _acceptButton;

        private Image _panelIcon;
        protected Image PanelIcon => _panelIcon;

        private Button _middleButton;

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
                    if (_unityObject != null)
                        _unityObject.SetActive(!_hidden);
                    OnVisibilityChange();
                }
            }
        }

        protected Button MiddleButton {
            get {
                if (_middleButton == null) {
                    GameObject buttonPrefab = SynthesisAssetCollection.GetUIPrefab("dynamic-panel-base")
                                                  .transform.Find("Footer")
                                                  .Find("Accept")
                                                  .gameObject;
                    RectTransform buttonTransform =
                        GameObject.Instantiate(buttonPrefab, Footer).GetComponent<RectTransform>();

                    buttonTransform.anchorMin = new Vector2(0.5f, 0f);
                    buttonTransform.anchorMax = new Vector2(0.5f, 0f);
                    buttonTransform.pivot     = new Vector2(1f, 0f);

                    buttonTransform.localPosition = new Vector3(
                        buttonTransform.rect.width / 2f, AcceptButton.RootGameObject.transform.localPosition.y, 0);

                    Button middleButton = new Button(null!, buttonTransform.gameObject, null);
                    middleButton.Image.SetColor(ColorManager.SynthesisColor.AcceptButton);
                    middleButton.Label?.SetColor(ColorManager.SynthesisColor.AcceptCancelButtonText);

                    _middleButton = middleButton;
                    return middleButton;
                }

                return _middleButton;
            }
        }

        public Action OnAccepted;

        protected PanelDynamic(Vector2 mainContentSize, float leftContentPadding = MAIN_CONTENT_HORZ_PADDING,
            float rightContentPadding = MAIN_CONTENT_HORZ_PADDING) {
            _mainContentSize     = mainContentSize;
            _leftContentPadding  = leftContentPadding;
            _rightContentPadding = rightContentPadding;
        }

        public void Create_Internal(GameObject unityObject) {
            _unityObject = unityObject;

            var header = _unityObject.transform.Find("Header");
            _headerRt  = header.GetComponent<RectTransform>();

            _panelIcon = new Image(null, header.Find("Image").gameObject);
            _panelIcon.SetColor(ColorManager.SynthesisColor.MainText);
            _panelIcon.RootGameObject.SetActive(false);

            _panelBackground = new Image(null, unityObject);
            _panelBackground.SetColor(ColorManager.GetColor(ColorManager.SynthesisColor.Background));
            _panelBackground.SetCornerRadius(20);

            _title = new Label(null, header.Find("Title").gameObject, null);

            _footer      = _unityObject.transform.Find("Footer");
            var footerRt = _footer.GetComponent<RectTransform>();

            _cancelButton = new Button(null!, _footer.Find("Cancel").gameObject, null);
            _cancelButton.AddOnClickedEvent(b => {
                if (!DynamicUIManager.ClosePanel(this.GetType()))
                    Logger.Log("Failed to Close Panel", LogLevel.Error);
            });
            _cancelButton.Image.SetColor(ColorManager.GetColor(ColorManager.SynthesisColor.CancelButton));

            _cancelButton.Label?.SetColor(ColorManager.GetColor(ColorManager.SynthesisColor.AcceptCancelButtonText));
            _acceptButton = new Button(null!, _footer.Find("Accept").gameObject, null);

            _acceptButton.Image.SetColor(ColorManager.GetColor(ColorManager.SynthesisColor.AcceptButton));
            _acceptButton.Label!.SetColor(ColorManager.GetColor(ColorManager.SynthesisColor.AcceptCancelButtonText));
            _acceptButton.AddOnClickedEvent(b => {
                if (OnAccepted != null)
                    OnAccepted.Invoke();
            });

            // Create Inital Content Component
            var hiddenContentT        = _unityObject.transform.Find("Content");
            var hiddenRt              = hiddenContentT.GetComponent<RectTransform>();
            hiddenRt.sizeDelta        = new Vector2(hiddenRt.sizeDelta.x, _mainContentSize.y);
            hiddenRt.anchorMin        = new Vector2(0, 1);
            hiddenRt.anchorMax        = new Vector2(1, 1);
            hiddenRt.pivot            = new Vector2(0.5f, 1);
            hiddenRt.anchoredPosition = new Vector2(0, -_headerRt.sizeDelta.y);
            var actualContentObj =
                GameObject.Instantiate(SynthesisAssetCollection.GetUIPrefab("content-base"), hiddenContentT);
            actualContentObj.name = "CentralContent";
            var contentRt         = actualContentObj.GetComponent<RectTransform>();
            contentRt.offsetMax   = new Vector2(-_rightContentPadding, contentRt.offsetMax.y);
            contentRt.offsetMin   = new Vector2(_leftContentPadding, contentRt.offsetMin.y);
            var modalRt           = _unityObject.GetComponent<RectTransform>();
            modalRt.sizeDelta     = new Vector2(_mainContentSize.x + (_leftContentPadding + _rightContentPadding),
                    hiddenRt.sizeDelta.y + _headerRt.sizeDelta.y + footerRt.sizeDelta.y);
            _mainContent          = new Content(null!, actualContentObj, _mainContentSize);
        }

        public abstract bool Create();
        public abstract void Update();
        public abstract void Delete();

        protected virtual void OnVisibilityChange() {}

        public void Delete_Internal() {
            GameObject.Destroy(_unityObject);
        }

        protected Content Strip(Vector2? newContentSize = null, float leftPadding = 0f, float rightPadding = 0f,
            float topPadding = 0f, float bottomPadding = 0f) {
            CancelButton.RootGameObject.SetActive(false);
            AcceptButton.RootGameObject.SetActive(false);
            Title.RootGameObject.SetActive(false);
            PanelIcon.RootGameObject.SetActive(false);

            var panel = new Content(null, UnityObject, null);
            if (newContentSize.HasValue) {
                panel.SetSize<Content>(new Vector2(newContentSize.Value.x + leftPadding + rightPadding,
                    newContentSize.Value.y + topPadding + bottomPadding));
            }
            panel.SetAnchors<Content>(new Vector2(0.5f, 0.0f), new Vector2(0.5f, 0.0f));
            panel.SetPivot<Content>(new Vector2(0.5f, 0.0f));
            panel.SetAnchoredPosition<Content>(new Vector2(0.0f, 10.0f));
            var newMainContent =
                panel.CreateSubContent(newContentSize ?? new Vector2(panel.Size.x - (rightPadding + leftPadding),
                                                             panel.Size.y - (topPadding + bottomPadding)));
            newMainContent.SetStretch<Content>(leftPadding, rightPadding, topPadding, bottomPadding);

            return newMainContent;
        }
    }

    public abstract class ModalDynamic {
        public const float MAIN_CONTENT_HORZ_PADDING = 35f;

        private Vector2 _mainContentSize; // Shouldn't really be used after init is called
        private GameObject _unityObject;

        public GameObject UnityObject => _unityObject;

        // Default for Modal
        private Button _cancelButton;
        protected Button CancelButton => _cancelButton;
        private Button _acceptButton;
        protected Button AcceptButton => _acceptButton;
        private Image _modalIcon;
        protected Image ModalIcon => _modalIcon;
        private Image _modalBackground;
        protected Image ModalBackground => _modalBackground;

        private Label _title;
        protected Label Title => _title;
        private Label _description;
        protected Label Description => _description;
        private Transform _footer;
        protected Transform Footer => _footer;
        private Content _mainContent;
        protected Content MainContent => _mainContent;
        private Button? _middleButton;

        public Action OnAccepted;
        public Action OnCancelled;

        protected Button MiddleButton {
            get {
                if (_middleButton == null) {
                    GameObject buttonPrefab = SynthesisAssetCollection.GetUIPrefab("dynamic-modal-base")
                                                  .transform.Find("Footer")
                                                  .Find("Accept")
                                                  .gameObject;
                    RectTransform buttonTransform =
                        GameObject.Instantiate(buttonPrefab, Footer).GetComponent<RectTransform>();

                    buttonTransform.anchorMin = new Vector2(0.5f, 0f);
                    buttonTransform.anchorMax = new Vector2(0.5f, 0f);
                    buttonTransform.pivot     = new Vector2(1f, 0f);

                    buttonTransform.localPosition = new Vector3(
                        buttonTransform.rect.width / 2f, AcceptButton.RootGameObject.transform.localPosition.y, 0);

                    Button middleButton = new Button(null!, buttonTransform.gameObject, null);
                    middleButton.Image.SetColor(ColorManager.SynthesisColor.AcceptButton);
                    middleButton.Label?.SetColor(ColorManager.SynthesisColor.AcceptCancelButtonText);

                    _middleButton = middleButton;
                    return middleButton;
                }

                return _middleButton;
            }
        }

        protected ModalDynamic(Vector2 mainContentSize) {
            _mainContentSize = mainContentSize;
        }

        public void Create_Internal(GameObject unityObject) {
            _unityObject = unityObject;

            // Grab Customizable Modal Components
            var header   = _unityObject.transform.Find("Header");
            var headerRt = header.GetComponent<RectTransform>();
            _modalIcon   = new Image(null, header.Find("Image").gameObject);
            _modalIcon.SetColor(ColorManager.SynthesisColor.MainText);

            _modalBackground = new Image(null, unityObject);
            _modalBackground.SetColor(ColorManager.GetColor(ColorManager.SynthesisColor.Background));
            _modalBackground.SetCornerRadius(35);

            _title = new Label(null, header.Find("Title").gameObject, null);
            _title.SetColor(ColorManager.GetColor(ColorManager.SynthesisColor.MainText));

            _description = new Label(null, header.Find("Description").gameObject, null);
            _description.SetColor(ColorManager.GetColor(ColorManager.SynthesisColor.MainText));

            _footer = _unityObject.transform.Find("Footer");

            var footerRt  = _footer.GetComponent<RectTransform>();
            _cancelButton = new Button(null!, _footer.Find("Cancel").gameObject, null);
            _cancelButton.AddOnClickedEvent(b => {
                if (!DynamicUIManager.CloseActiveModal())
                    Logger.Log("Failed to Close Modal", LogLevel.Error);
            });
            _cancelButton.Image.SetColor(ColorManager.GetColor(ColorManager.SynthesisColor.CancelButton));
            _cancelButton.Label.SetColor(ColorManager.GetColor(ColorManager.SynthesisColor.AcceptCancelButtonText));
            _acceptButton = new Button(null!, _footer.Find("Accept").gameObject, null);
            _acceptButton.Image.SetColor(ColorManager.GetColor(ColorManager.SynthesisColor.AcceptButton));
            _acceptButton.Label.SetColor(ColorManager.GetColor(ColorManager.SynthesisColor.AcceptCancelButtonText));

            _cancelButton.AddOnClickedEvent(b => {
                if (OnCancelled != null)
                    OnCancelled.Invoke();
            });
            _acceptButton.AddOnClickedEvent(b => {
                if (OnAccepted != null)
                    OnAccepted.Invoke();
            });

            // Create Inital Content Component
            var hiddenContentT        = _unityObject.transform.Find("Content");
            var hiddenRt              = hiddenContentT.GetComponent<RectTransform>();
            hiddenRt.sizeDelta        = new Vector2(hiddenRt.sizeDelta.x, _mainContentSize.y);
            hiddenRt.anchorMin        = new Vector2(0, 1);
            hiddenRt.anchorMax        = new Vector2(1, 1);
            hiddenRt.pivot            = new Vector2(0.5f, 1);
            hiddenRt.anchoredPosition = new Vector2(0, -headerRt.sizeDelta.y);
            var actualContentObj =
                GameObject.Instantiate(SynthesisAssetCollection.GetUIPrefab("content-base"), hiddenContentT);
            actualContentObj.name = "CentralContent";
            var contentRt         = actualContentObj.GetComponent<RectTransform>();
            contentRt.offsetMax   = new Vector2(-MAIN_CONTENT_HORZ_PADDING, contentRt.offsetMax.y);
            contentRt.offsetMin   = new Vector2(MAIN_CONTENT_HORZ_PADDING, contentRt.offsetMin.y);
            var modalRt           = _unityObject.GetComponent<RectTransform>();
            modalRt.sizeDelta     = new Vector2(_mainContentSize.x + (MAIN_CONTENT_HORZ_PADDING * 2),
                    hiddenRt.sizeDelta.y + headerRt.sizeDelta.y + footerRt.sizeDelta.y);
            _mainContent          = new Content(null!, actualContentObj, _mainContentSize);
        }

        public abstract void Create();
        public abstract void Update();
        public abstract void Delete();

        public void Delete_Internal() {
            GameObject.Destroy(_unityObject);
        }
    }

    public abstract class UIComponent {
        public static Func<UIComponent, UIComponent> VerticalLayout = (u) => {
            var offset = (-u.Parent!.RectOfChildren(u).yMin) + 7.5f;
            u.SetTopStretch<UIComponent>(anchoredY: offset);
            return u;
        };

        public static Func<UIComponent, UIComponent> VerticalLayoutNoSpacing = (u) => {
            var offset = (-u.Parent!.RectOfChildren(u).yMin);
            u.SetTopStretch<UIComponent>(anchoredY: offset);
            return u;
        };

        public static Func<UIComponent, UIComponent> VerticalLayoutBigSpacing = (u) => {
            var offset = (-u.Parent!.RectOfChildren(u).yMin + 15f);
            u.SetTopStretch<UIComponent>(anchoredY: offset);
            return u;
        };

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
            Rect r         = new Rect();
            for (int i = 0; i < Children.Count; i++) {
                if (Children[i] == negate)
                    continue;
                if (!hasInital) {
                    r = Children[i].RootRectTransform.GetOffsetRect();
                    continue;
                }
                var childTrans = Children[i].RootRectTransform;
                var childRect  = childTrans.GetOffsetRect();

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

        public void DeleteAllChildren() {
            Children.ForEach(x => { GameObject.Destroy(x.RootGameObject); });
            Children.Clear();
        }

        protected bool _eventsActive = true;
        // clang-format off
        public bool EventsActive => _eventsActive;
        // clang-format on

        public Vector2 Size { get; protected set; }
        public GameObject RootGameObject { get; protected set; }
        public RectTransform RootRectTransform { get; protected set; }
        public UIComponent? Parent { get; protected set; }
        protected List<UIComponent> Children = new List<UIComponent>();

        public IReadOnlyList<UIComponent> ChildrenReadOnly => Children.AsReadOnly();

        public UIComponent(UIComponent? parentComponent, GameObject rootGameObject) {
            Parent            = parentComponent;
            RootGameObject    = rootGameObject;
            RootRectTransform = rootGameObject.GetComponent<RectTransform>();
            Size              = RootRectTransform.sizeDelta;
        }

        private void SetAnchorOffset(Vector2 aMin, Vector2 aMax, Vector2 oMin, Vector2 oMax) {
            RootRectTransform.anchorMin = aMin;
            RootRectTransform.anchorMax = aMax;
            RootRectTransform.offsetMin = oMin;
            RootRectTransform.offsetMax = oMax;
        }
        public T SetTopStretch<T>(float leftPadding = 0f, float rightPadding = 0f, float anchoredY = 0f)
            where T : UIComponent {
            SetAnchorOffset(
                new Vector2(0, 1), new Vector2(1, 1), new Vector2(leftPadding, -Size.y), new Vector2(-rightPadding, 0));
            RootRectTransform.pivot            = new Vector2(RootRectTransform.pivot.x, 1);
            RootRectTransform.anchoredPosition = new Vector2(RootRectTransform.anchoredPosition.x, -anchoredY);
            return (this as T)!;
        }
        public T SetBottomStretch<T>(float leftPadding = 0f, float rightPadding = 0f, float anchoredY = 0f)
            where T : UIComponent {
            SetAnchorOffset(
                new Vector2(0, 0), new Vector2(1, 0), new Vector2(leftPadding, 0), new Vector2(-rightPadding, Size.y));
            RootRectTransform.pivot            = new Vector2(RootRectTransform.pivot.x, 0);
            RootRectTransform.anchoredPosition = new Vector2(RootRectTransform.anchoredPosition.x, anchoredY);
            return (this as T)!;
        }
        public T SetLeftStretch<T>(float topPadding = 0f, float bottomPadding = 0f, float anchoredX = 0f)
            where T : UIComponent {
            SetAnchorOffset(
                new Vector2(0, 0), new Vector2(0, 1), new Vector2(0, bottomPadding), new Vector2(Size.x, -topPadding));
            RootRectTransform.pivot            = new Vector2(0, RootRectTransform.pivot.y);
            RootRectTransform.anchoredPosition = new Vector2(anchoredX, RootRectTransform.anchoredPosition.y);
            return (this as T)!;
        }
        public T SetRightStretch<T>(float topPadding = 0f, float bottomPadding = 0f, float anchoredX = 0f)
            where T : UIComponent {
            SetAnchorOffset(
                new Vector2(1, 0), new Vector2(1, 1), new Vector2(-Size.x, bottomPadding), new Vector2(0, -topPadding));
            RootRectTransform.pivot            = new Vector2(1, RootRectTransform.pivot.y);
            RootRectTransform.anchoredPosition = new Vector2(-anchoredX, RootRectTransform.anchoredPosition.y);
            return (this as T)!;
        }
        public T SetStretch<T>(
            float leftPadding = 0f, float rightPadding = 0f, float topPadding = 0f, float bottomPadding = 0f)
            where T : UIComponent {
            SetAnchorOffset(new Vector2(0, 0), new Vector2(1, 1), new Vector2(leftPadding, bottomPadding),
                new Vector2(-rightPadding, -topPadding));
            return (this as T)!;
        }
        public T SetPivot<T>(Vector2 pivot)
            where T : UIComponent {
            RootRectTransform.pivot = pivot;
            return (this as T)!;
        }
        public T SetAnchoredPosition<T>(Vector2 pos)
            where T : UIComponent {
            RootRectTransform.anchoredPosition = pos;
            return (this as T)!;
        }
        public T SetAnchor<T>(Vector2 anchorMin, Vector2 anchorMax)
            where T : UIComponent {
            RootRectTransform.anchorMin = anchorMin;
            RootRectTransform.anchorMax = anchorMax;
            return (this as T)!;
        }
        public T SetSize<T>(Vector2 size)
            where T : UIComponent {
            Size                        = size;
            RootRectTransform.sizeDelta = size;
            return (this as T)!;
        }
        public T SetWidth<T>(float width)
            where T : UIComponent {
            Size                        = new Vector2(width, Size.y);
            RootRectTransform.sizeDelta = Size;
            return (this as T)!;
        }
        public T SetHeight<T>(float height)
            where T : UIComponent {
            Size                        = new Vector2(Size.x, height);
            RootRectTransform.sizeDelta = Size;
            return (this as T)!;
        }
        public T ShiftOffsetMax<T>(Vector2 shift)
            where T : UIComponent {
            RootRectTransform.offsetMax += shift;
            return (this as T)!;
        }
        public T ShiftOffsetMin<T>(Vector2 shift)
            where T : UIComponent {
            RootRectTransform.offsetMin += shift;
            return (this as T)!;
        }
        public T EnableEvents<T>()
            where T : UIComponent {
            _eventsActive = true;
            return (this as T)!;
        }
        public T DisableEvents<T>()
            where T : UIComponent {
            _eventsActive = false;
            return (this as T)!;
        }
        public T SetAnchors<T>(Vector2 min, Vector2 max)
            where T : UIComponent {
            RootRectTransform.anchorMin = min;
            RootRectTransform.anchorMax = max;
            return (this as T)!;
        }
        public T SetAnchorCenter<T>()
            where T : UIComponent {
            RootRectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            RootRectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            return (this as T)!;
        }
        public T SetAnchorTop<T>()
            where T : UIComponent {
            RootRectTransform.anchorMin = new Vector2(0.5f, 1);
            RootRectTransform.anchorMax = new Vector2(0.5f, 1);
            return (this as T)!;
        }

        public T SetAnchorLeft<T>()
            where T : UIComponent {
            RootRectTransform.anchorMin = new Vector2(0, 0.5f);
            RootRectTransform.anchorMax = new Vector2(1, 0.5f);
            return (this as T)!;
        }

        public T SetBackgroundColor<T>(ColorManager.SynthesisColor c)
            where T : UIComponent {
            return SetBackgroundColor<T>(ColorManager.GetColor(c));
        }

        public T SetBackgroundColor<T>(ColorManager.SynthesisColor start, ColorManager.SynthesisColor end)
            where T : UIComponent {
            return SetBackgroundColor<T>(ColorManager.GetColor(start), ColorManager.GetColor(end));
        }

        public T SetBackgroundColor<T>(Color color)
            where T : UIComponent {
            return SetBackgroundColor<T>(color, color);
        }

        public T SetBackgroundColor<T>(Color start, Color end)
            where T : UIComponent {
            GradientImageUpdater gradientImage = RootGameObject.GetComponent<GradientImageUpdater>();
            UnityEngine.UI.Image image         = RootGameObject.GetComponent<UnityEngine.UI.Image>();

            if (gradientImage) {
                gradientImage.StartColor = start;
                gradientImage.EndColor   = end;

                if (image)
                    RootGameObject.GetComponent<UImage>().color = Color.white;
                gradientImage.Refresh();
            } else {
                if (image)
                    image.color = start;
            }

            return (this as T)!;
        }

        public T SetHorizontalGradient<T>(bool horizontal)
            where T : UIComponent {
            GradientImageUpdater gradientImage = RootGameObject.GetComponent<GradientImageUpdater>();
            if (gradientImage) {
                gradientImage.GradientAngle = Mathf.PI * (horizontal ? 0 : 1) * (3f / 2f);
                gradientImage.Refresh();
            }

            return (this as T)!;
        }

        public T? CheckIfNull<T>()
            where T : UIComponent {
            return RootGameObject == null ? null : (this as T)!;
        }

        public T SetAlpha<T>(float alpha)
            where T : UIComponent {
            RootGameObject.AddComponent<CanvasGroup>().alpha = alpha;

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
                Size                        = size.Value;
            } else {
                Size = RootRectTransform.sizeDelta;
            }

            var uImg = unityObject.GetComponent<UImage>();
            if (uImg != null) {
                _image = new Image(this, unityObject);
            }
        }

        public (Content left, Content right) SplitLeftRight(float leftWidth, float padding) {
            var leftContentObject = GameObject.Instantiate(
                SynthesisAssetCollection.GetUIPrefab("content-base"), base.RootGameObject.transform);
            var leftRt              = leftContentObject.GetComponent<RectTransform>();
            leftRt.anchorMax        = new Vector2(0f, 0.5f);
            leftRt.anchorMin        = new Vector2(0f, 0.5f);
            leftRt.pivot            = new Vector2(0f, 0.5f);
            leftRt.anchoredPosition = new Vector2(0f, 0f);
            var leftContent         = new Content(this, leftContentObject, new Vector2(leftWidth, Size.y));

            var rightContentObject = GameObject.Instantiate(
                SynthesisAssetCollection.GetUIPrefab("content-base"), base.RootGameObject.transform);
            var rightRt              = rightContentObject.GetComponent<RectTransform>();
            rightRt.anchorMax        = new Vector2(1f, 0.5f);
            rightRt.anchorMin        = new Vector2(1f, 0.5f);
            float rightWidth         = (Size.x - leftWidth) - padding;
            rightRt.pivot            = new Vector2(1f, 0.5f);
            rightRt.anchoredPosition = new Vector2(0f, 0f);
            var rightContent         = new Content(this, rightContentObject, new Vector2(rightWidth, Size.y));

            base.Children.Add(leftContent);
            base.Children.Add(rightContent);

            return (leftContent, rightContent);
        }

        public (Content top, Content bottom) SplitTopBottom(float topHeight, float padding) {
            var topContentObject = GameObject.Instantiate(
                SynthesisAssetCollection.GetUIPrefab("content-base"), base.RootGameObject.transform);
            var topRt              = topContentObject.GetComponent<RectTransform>();
            topRt.anchorMax        = new Vector2(0.5f, 1f);
            topRt.anchorMin        = new Vector2(0.5f, 1f);
            topRt.pivot            = new Vector2(0.5f, 1f);
            topRt.anchoredPosition = new Vector2(0f, 0);
            var topContent         = new Content(this, topContentObject, new Vector2(Size.x, topHeight));

            var bottomContentObject = GameObject.Instantiate(
                SynthesisAssetCollection.GetUIPrefab("content-base"), base.RootGameObject.transform);
            var bottomRt              = bottomContentObject.GetComponent<RectTransform>();
            bottomRt.anchorMax        = new Vector2(0.5f, 0f);
            bottomRt.anchorMin        = new Vector2(0.5f, 0f);
            float bottomHeight        = (Size.y - topHeight) - padding;
            bottomRt.pivot            = new Vector2(0.5f, 0f);
            bottomRt.anchoredPosition = new Vector2(0f, 0);
            // rightRt.sizeDelta = new Vector2(rightWidth, rightRt.sizeDelta.y);
            var bottomContent = new Content(this, bottomContentObject, new Vector2(Size.x, bottomHeight));

            base.Children.Add(topContent);
            base.Children.Add(bottomContent);

            return (topContent, bottomContent);
        }

        public Label CreateLabel(float height = 15f) {
            var labelObj = GameObject.Instantiate(
                SynthesisAssetCollection.GetUIPrefab("label-base"), base.RootGameObject.transform);
            var label = new Label(this, labelObj, new Vector2(Size.x, height));
            base.Children.Add(label);
            return label;
        }

        public Toggle CreateToggle(bool radioSelect = false, bool isOn = false, string label = "New Toggle") {
            var toggleObj = GameObject.Instantiate(
                SynthesisAssetCollection.GetUIPrefab((radioSelect) ? "radio-select-base" : "toggle-base"),
                base.RootGameObject.transform);
            var toggle = new Toggle(this, toggleObj, isOn, label, radioSelect);
            base.Children.Add(toggle);
            return toggle;
        }

        public Slider CreateSlider(string label = "New Slider", string unitSuffix = "", float minValue = 0,
            float maxValue = 1, float currentValue = 0) {
            var sliderObj = GameObject.Instantiate(
                SynthesisAssetCollection.GetUIPrefab("slider-base"), base.RootGameObject.transform);
            var slider = new Slider(this, sliderObj, label, unitSuffix, minValue, maxValue, currentValue);
            base.Children.Add(slider);
            return slider;
        }

        public Button CreateButton(string text = "New Button") {
            var buttonObj = GameObject.Instantiate(
                SynthesisAssetCollection.GetUIPrefab("button-base"), base.RootGameObject.transform);
            var button = new Button(this, buttonObj, null);
            button.StepIntoLabel(l => l.SetText(text));
            base.Children.Add(button);
            return button;
        }

        public LabeledButton CreateLabeledButton() {
            var lButtonObj = GameObject.Instantiate(
                SynthesisAssetCollection.GetUIPrefab("labeled-button-base"), base.RootGameObject.transform);
            var lButton = new LabeledButton(this, lButtonObj);
            base.Children.Add(lButton);
            return lButton;
        }

        public Dropdown CreateDropdown() {
            var dropdownObj = GameObject.Instantiate(
                SynthesisAssetCollection.GetUIPrefab("dropdown-base"), base.RootGameObject.transform);
            var dropdown = new Dropdown(this, dropdownObj, null);
            base.Children.Add(dropdown);
            return dropdown;
        }

        public LabeledDropdown CreateLabeledDropdown() {
            var lDropdownObj = GameObject.Instantiate(
                SynthesisAssetCollection.GetUIPrefab("labeled-dropdown-base"), base.RootGameObject.transform);
            var lDropdown = new LabeledDropdown(this, lDropdownObj);
            base.Children.Add(lDropdown);
            return lDropdown;
        }

        public InputField CreateInputField() {
            var inputFieldObj = GameObject.Instantiate(
                SynthesisAssetCollection.GetUIPrefab("input-field-base"), base.RootGameObject.transform);
            var inputField = new InputField(this, inputFieldObj);
            base.Children.Add(inputField);
            return inputField;
        }

        public ScrollView CreateScrollView() {
            var scrollViewObj = GameObject.Instantiate(
                SynthesisAssetCollection.GetUIPrefab("scroll-view-base"), base.RootGameObject.transform);
            var scrollView = new ScrollView(this, scrollViewObj);
            base.Children.Add(scrollView);
            return scrollView;
        }

        public Content CreateSubContent(Vector2 size) {
            var contentObj = GameObject.Instantiate(
                SynthesisAssetCollection.GetUIPrefab("content-base"), base.RootGameObject.transform);
            var content = new Content(this, contentObj, size);
            base.Children.Add(content);
            return content;
        }

        public NumberInputField CreateNumberInputField() {
            var numberInputFieldObj = GameObject.Instantiate(
                SynthesisAssetCollection.GetUIPrefab("number-input-field-base"), base.RootGameObject.transform);
            var numberInputField = new NumberInputField(this, numberInputFieldObj);
            base.Children.Add(numberInputField);
            return numberInputField;
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
            _backgroundImage.SetColor(ColorManager.GetColor(ColorManager.SynthesisColor.BackgroundSecondary))
                .SetCornerRadius(12);
        }

        public ScrollView StepIntoContent(Action<Content> mod) {
            mod(_content);
            return this;
        }
    }

    public class Label : UIComponent {
        private TMP_Text _unityText;

        public string Text              => _unityText.text;
        public FontStyles FontStyle     => _unityText.fontStyle;
        public bool IsFontSizeAutomatic => _unityText.enableAutoSizing;

        public static readonly Func<Label, Label> VerticalLayoutTemplate = (Label label) => {
            return label.SetTopStretch(anchoredY: label.Parent!.HeightOfChildren - label.Size.y + 15f);
        };
        public static readonly Func<Label, Label> BigLabelTemplate = (Label label) => {
            return label.SetHeight<Label>(30)
                .SetFontSize(24)
                .ApplyTemplate(VerticalLayoutTemplate)
                .SetHorizontalAlignment(HorizontalAlignmentOptions.Left)
                .SetVerticalAlignment(VerticalAlignmentOptions.Middle);
        };

        public Label(UIComponent? parent, GameObject unityObject, Vector2? size) : base(parent, unityObject) {
            _unityText = unityObject.GetComponent<TMP_Text>();
            if (size.HasValue) {
                Size                               = size.Value;
                RootRectTransform.anchoredPosition = Vector2.zero;
                RootRectTransform.sizeDelta        = Size;
            } else {
                size = RootRectTransform.sizeDelta;
            }

            SetColor(ColorManager.GetColor(ColorManager.SynthesisColor.MainText));
        }

        public Label SetAutomaticFontSize(bool a) {
            _unityText.enableAutoSizing = a;
            return this;
        }

        public Label SetText(string text) {
            _unityText.text = text;
            return this;
        }

        public Label SetFontSize(float fontSize) {
            _unityText.fontSize = fontSize;
            return this;
        }

        public Label SetFontMinMaxSize(float min, float max) {
            _unityText.fontSizeMin = min;
            _unityText.fontSizeMax = max;
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

        public Label SetColor(ColorManager.SynthesisColor c) => SetColor(ColorManager.GetColor(c));

        public Label SetColor(Color c) {
            _unityText.color = c;
            return this;
        }

        public Label SetOverflowMode(TextOverflowModes mode) {
            _unityText.overflowMode = mode;
            return this;
        }

        public Label SetWrapping(bool wrappingEnabled) {
            _unityText.enableWordWrapping = wrappingEnabled;
            return this;
        }

        public Label SetTopStretch(float leftPadding = 0f, float rightPadding = 0f,
            float anchoredY = 0f)     => base.SetTopStretch<Label>(leftPadding, rightPadding, anchoredY);
        public Label SetBottomStretch(float leftPadding = 0f, float rightPadding = 0f,
            float anchoredY = 0f)     => base.SetBottomStretch<Label>(leftPadding, rightPadding, anchoredY);
        public Label SetLeftStretch(float topPadding = 0f, float bottomPadding = 0f,
            float anchoredX = 0f)     => base.SetLeftStretch<Label>(topPadding, bottomPadding, anchoredX);
        public Label SetRightStretch(float topPadding = 0f, float bottomPadding = 0f,
            float anchoredX = 0f)     => base.SetRightStretch<Label>(topPadding, bottomPadding, anchoredX);
        public Label SetStretch(float leftPadding = 0f, float rightPadding = 0f, float topPadding = 0f,
            float bottomPadding = 0f) => base.SetStretch<Label>(leftPadding, rightPadding, topPadding, bottomPadding);
    }

    public class Toggle : UIComponent {
        public static readonly Func<Toggle, Toggle> VerticalLayoutTemplate = (Toggle toggle) =>
            toggle.SetTopStretch<Toggle>(
                leftPadding: 15f, anchoredY: toggle.Parent!.HeightOfChildren - toggle.Size.y + 15f);

        public event Action<Toggle, bool> OnStateChanged;
        private GameObject _unityObject;
        private Label _titleLabel;
        public Label TitleLabel => _titleLabel;
        private UToggle _unityToggle;

        private Image _disabledImage;
        public Image DisabledImage => _disabledImage;
        private Image _enabledImage;
        public Image EnabledImage => _enabledImage;

        private Color _disabledColor;
        public Color DisabledColor {
            get => _disabledColor;
            set {
                _disabledColor = value;
                _disabledImage.SetColor(_disabledColor);
            }
        }
        private (ColorManager.SynthesisColor left, ColorManager.SynthesisColor right) _enabledColor;
        public (ColorManager.SynthesisColor left, ColorManager.SynthesisColor right) EnabledColor {
            get => _enabledColor;
            set {
                _enabledColor = value;
                _enabledImage.SetColor(_enabledColor.left, _enabledColor.right);
            }
        }

        public bool State {
            get => _unityToggle.isOn;
            set { SetState(value); }
        }

        public Toggle(UIComponent? parent, GameObject unityObject, bool isOn, string text, bool radioSelect)
            : base(parent, unityObject) {
            _unityObject = unityObject;

            if (unityObject.transform.Find("Toggle").TryGetComponent<ToggleTweener>(out var tweener)) {
                tweener._synthesisToggle = this;
            }

            _titleLabel  = new Label(this, RootGameObject.transform.Find("Label").gameObject, null);
            _unityToggle = RootGameObject.transform.Find("Toggle").GetComponent<UToggle>();
            _unityToggle.onValueChanged.AddListener(x => {
                {
                    if (_eventsActive && OnStateChanged != null)
                        OnStateChanged(this, x);
                }
            });
            DisableEvents<Toggle>().SetState(isOn).EnableEvents<Toggle>();
            _titleLabel.SetText(text);

            _disabledImage = new Image(this, _unityToggle.transform.Find("Background").gameObject);
            _disabledImage.SetCornerRadius((radioSelect) ? 10 : 2);

            _enabledImage = new Image(this, _unityToggle.transform.Find("Background").Find("Checkmark").gameObject);
            _enabledImage.SetCornerRadius((radioSelect) ? 10 : 2);

            DisabledColor = ColorManager.GetColor(ColorManager.SynthesisColor.InteractiveBackground);
            EnabledColor  = (ColorManager.SynthesisColor.InteractiveElementLeft,
                ColorManager.SynthesisColor.InteractiveElementRight);
        }

        public Toggle SetState(bool state, bool notify = true) {
            if (notify)
                _unityToggle.isOn = state;
            else
                _unityToggle.SetIsOnWithoutNotify(state);
            return this;
        }

        public Toggle AddOnStateChangedEvent(Action<Toggle, bool> callback) {
            OnStateChanged += callback;
            return this;
        }

        public Toggle SetEnabledColor(ColorManager.SynthesisColor left, ColorManager.SynthesisColor right) {
            EnabledColor = (left, right);
            return this;
        }

        public Toggle SetDisabledColor(Color c) {
            DisabledColor = c;
            return this;
        }

        public Toggle SetDisabledColor(ColorManager.SynthesisColor s) {
            DisabledColor = ColorManager.GetColor(s);
            return this;
        }

        public Toggle StepIntoLabel(Action<Label> mod) {
            mod(TitleLabel);
            return this;
        }

        public void SetStateWithoutEvents(bool state) {
            DisableEvents<Toggle>();
            SetState(state);
            if (_unityObject.transform.Find("Toggle").TryGetComponent<ToggleTweener>(out var tweener)) {
                tweener.OnStateChanged();
            }
            EnableEvents<Toggle>();
        }
    }

    public class Slider : UIComponent {
        public static readonly Func<Slider, Slider> VerticalLayoutTemplate = (Slider slider) =>
            slider.SetTopStretch<Slider>(
                leftPadding: 15f, anchoredY: slider.Parent!.HeightOfChildren - slider.Size.y + 15f);

        public event Action<Slider, float> OnValueChanged;
        private Func<float, string> _customValuePresentation = (x) => Math.Round(x, 2).ToString();
        private USlider _unitySlider;
        public (float min, float max) SlideRange => (_unitySlider.minValue, _unitySlider.maxValue);
        public float Value                       => _unitySlider.value;
        private Label _titleLabel;
        private Label _valueLabel;
        private string _unitSuffix = string.Empty;

        private Image _backgroundImage;
        private Image _fillImage;
        private Image _handleImage;

        public Slider(UIComponent? parent, GameObject unityObject, string label, string unitSuffix, float minValue,
            float maxValue, float currentValue)
            : base(parent, unityObject) {
            var infoObj  = unityObject.transform.Find("Info");
            _titleLabel  = new Label(this, infoObj.Find("Label").gameObject, null);
            _valueLabel  = new Label(this, infoObj.Find("Value").gameObject, null);
            _unitySlider = unityObject.transform.Find("ScaleAnim").Find("Slider").GetComponent<USlider>();
            _titleLabel.SetText(label);
            _unitySlider.onValueChanged.AddListener(x => {
                var roundedVal = Math.Round(x, 2);
                _valueLabel.SetText(roundedVal.ToString() + (this._unitSuffix == string.Empty ? "" : this._unitSuffix));
                if (_eventsActive && OnValueChanged != null)
                    OnValueChanged(this, x);
            });

            _backgroundImage = new Image(this, _unitySlider.transform.Find("Background").gameObject);
            _backgroundImage.SetColor(ColorManager.SynthesisColor.InteractiveBackground);

            _fillImage = new Image(this, _unitySlider.transform.Find("Fill Area").Find("Fill").gameObject);
            _fillImage.SetColor(ColorManager.SynthesisColor.InteractiveElementLeft,
                ColorManager.SynthesisColor.InteractiveElementRight);
            _fillImage.SetCornerRadius(8);

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

        public Slider EnableRounding() {
            OnValueChanged += (_, value) => { SetValue((int) value); };
            return this;
        }
    }

    public class InputField : UIComponent {
        public static readonly Func<InputField, InputField> VerticalLayoutTemplate = (InputField inputField) =>
            inputField.SetTopStretch<InputField>(
                leftPadding: 15f, anchoredY: inputField.Parent!.HeightOfChildren - inputField.Size.y + 15f);

        public event Action<InputField, string> OnValueChanged;
        private Label _hint;
        public Label Hint => _hint;
        private Label _label;
        public Label Label => _label;
        private Image _backgroundImage;
        public Image BackgroundImage => _backgroundImage;
        private TMP_InputField _tmpInput;
        public TMP_InputField InputText => _tmpInput;
        public string Value             => _tmpInput.text;

        public InputField(UIComponent? parent, GameObject unityObject) : base(parent, unityObject) {
            var ifObj = unityObject.transform.Find("InputField");
            _tmpInput = ifObj.GetComponent<TMP_InputField>();
            _hint     = new Label(this, ifObj.Find("Text Area").Find("Placeholder").gameObject, null);
            _label    = new Label(this, unityObject.transform.Find("Label").gameObject, null);
            _tmpInput.onValueChanged.AddListener(x => {
                if (_eventsActive && OnValueChanged != null)
                    OnValueChanged(this, x);
            });

            _backgroundImage = new Image(this, ifObj.gameObject);
            _backgroundImage.SetColor(ColorManager.SynthesisColor.InteractiveBackground);
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

        public InputField SetCharacterLimit(int length) {
            _tmpInput.characterLimit = length;
            return this;
        }

        public InputField SetValueTextColor(Color color) {
            RootGameObject.transform.Find("InputField/Text Area/Text").GetComponent<TextMeshProUGUI>().color = color;
            return this;
        }
    }

    public class LabeledButton : UIComponent {
        public static readonly Func<LabeledButton, LabeledButton> VerticalLayoutTemplate = (LabeledButton lb) => {
            return lb.SetTopStretch<LabeledButton>(
                leftPadding: 15f, anchoredY: lb.Parent!.HeightOfChildren - lb.Size.y + 15f);
        };

        private Button _button;
        public Button Button => _button;
        private Label _label;
        public Label Label => _label;

        public LabeledButton(UIComponent? parent, GameObject unityObject) : base(parent, unityObject) {
            _button = new Button(this, RootGameObject.transform.Find("Button").gameObject, null);
            _label  = new Label(this, RootGameObject.transform.Find("Label").gameObject, null);
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
        public static readonly Func<Button, Button> VerticalLayoutTemplate = (Button button) =>
            button.SetTopStretch<Button>(
                leftPadding: 15f, anchoredY: button.Parent!.HeightOfChildren - button.Size.y + 15f);

        public static readonly Func<Button, Button> EnableButton = b => {
            b.StepIntoImage(i => i.SetColor(ColorManager.SynthesisColor.InteractiveElementLeft,
                                ColorManager.SynthesisColor.InteractiveElementRight))
                .StepIntoLabel(l => l.SetColor(ColorManager.SynthesisColor.InteractiveElementText))
                .EnableEvents<Button>();

            var eventListener = b.RootGameObject.GetComponentInChildren<HoverEventListener>();
            if (eventListener != null)
                eventListener.enabled = true;

            return b;
        };

        public static readonly Func<Button, Button> EnableAcceptButton = b => {
            b.StepIntoImage(i => i.SetColor(ColorManager.SynthesisColor.AcceptButton))
                .StepIntoLabel(l => l.SetColor(ColorManager.SynthesisColor.AcceptCancelButtonText))
                .EnableEvents<Button>();

            var eventListener = b.RootGameObject.GetComponentInChildren<HoverEventListener>();
            if (eventListener != null)
                eventListener.enabled = true;

            return b;
        };

        public static readonly Func<Button, Button> EnableCancelButton = b => {
            b.StepIntoImage(i => i.SetColor(ColorManager.SynthesisColor.CancelButton))
                .StepIntoLabel(l => l.SetColor(ColorManager.SynthesisColor.AcceptCancelButtonText))
                .EnableEvents<Button>();

            var eventListener = b.RootGameObject.GetComponentInChildren<HoverEventListener>();
            if (eventListener != null)
                eventListener.enabled = true;

            return b;
        };

        public static readonly Func<Button, Button> EnableDeleteButton = b => {
            b.StepIntoImage(i => i.SetColor(ColorManager.SynthesisColor.CancelButton))
                .StepIntoLabel(l => l.SetColor(ColorManager.SynthesisColor.InteractiveElementText))
                .EnableEvents<Button>();

            var eventListener = b.RootGameObject.GetComponentInChildren<HoverEventListener>();
            if (eventListener != null)
                eventListener.enabled = true;

            return b;
        };

        public static readonly Func<Button, Button> DisableButton = b => {
            b.StepIntoImage(i => i.SetColor(ColorManager.SynthesisColor.InteractiveBackground))
                .StepIntoLabel(l => l.SetColor(ColorManager.SynthesisColor.InteractiveElementText))
                .DisableEvents<Button>();

            var eventListener = b.RootGameObject.GetComponentInChildren<HoverEventListener>();
            if (eventListener != null)
                eventListener.enabled = false;

            return b;
        };

        public event Action<Button> OnClicked;
        private Label? _label;
        public Label? Label => _label;
        private UButton _unityButton;
        private Image _image;
        public Image Image => _image;

        public Button(UIComponent? parent, GameObject unityObject, Vector2? size) : base(parent, unityObject) {
            if (size != null) {
                Size = size.Value;
            }

            var labelTransform = unityObject.transform.Find("Button").Find("Text (TMP)");
            if (labelTransform != null) {
                _label = new Label(this, labelTransform.gameObject, null);
                _label.SetColor(ColorManager.SynthesisColor.InteractiveElementText);
            }

            _unityButton = unityObject.transform.Find("Button").GetComponent<UButton>();
            _unityButton.onClick.AddListener(() => {
                if (_eventsActive && OnClicked != null) {
                    OnClicked(this);
                }
            });

            _image = new Image(this, unityObject.transform.Find("Button").gameObject)
                         .SetColor(ColorManager.SynthesisColor.InteractiveElementLeft,
                             ColorManager.SynthesisColor.InteractiveElementRight);

            var gradientUpdater = _image.RootGameObject.GetComponent<GradientImageUpdater>();

            if (gradientUpdater)
                _image.SetCornerRadius(6);
        }

        public Button SetTransition(Selectable.Transition transition) {
            _unityButton.transition = transition;
            return this;
        }

        public Button SetInteractableColors(
            ColorManager.SynthesisColor highlightedColor = ColorManager.SynthesisColor.InteractiveHover,
            ColorManager.SynthesisColor pressedColor     = ColorManager.SynthesisColor.InteractiveSelect,
            float fadeDuration                           = 0.1F) {
            _unityButton.colors = new ColorBlock { normalColor = Color.white,
                highlightedColor                               = ColorManager.GetColor(highlightedColor),
                pressedColor                                   = ColorManager.GetColor(pressedColor),
                selectedColor                                  = ColorManager.GetColor(highlightedColor),
                disabledColor = new Color32(191, 191, 191, 255), fadeDuration = fadeDuration, colorMultiplier = 1F };
            return this;
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
        public static readonly Func<Dropdown, Dropdown> VerticalLayoutTemplate = (Dropdown dropdown) =>
            dropdown.SetTopStretch<Dropdown>(
                leftPadding: 15f, anchoredY: dropdown.Parent!.HeightOfChildren - dropdown.Size.y + 15f);

        public event Action<Dropdown, int, TMP_Dropdown.OptionData> OnValueChanged;
        private Image _image;
        public Image Image => _image;
        private Content _viewport;
        public Content Viewport => _viewport;
        private TMP_Dropdown _tmpDropdown;
        public IReadOnlyList<TMP_Dropdown.OptionData> Options => _tmpDropdown.options.AsReadOnly();
        public int Value                                      => _tmpDropdown.value;
        public TMP_Dropdown.OptionData SelectedOption         => _tmpDropdown.options[Value];

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

            _image       = new Image(this, unityObject.transform.Find("Header").Find("Arrow").gameObject);
            _viewport    = new Content(this, unityObject.transform.Find("Template").Find("Viewport").gameObject, null);
            _tmpDropdown = unityObject.transform.GetComponent<TMP_Dropdown>();

            _tmpDropdown.onValueChanged.AddListener(x => {
                // TODO?
                if (_eventsActive && OnValueChanged != null)
                    OnValueChanged(this, x, this.Options[x]);
            });
            var eventHandler = _tmpDropdown.gameObject.AddComponent<UIEventHandler>();

            eventHandler.OnPointerClickedEvent += e => { ShowOnTop(); };

            _headerImage = new Image(this, unityObject.transform.Find("Header").gameObject);
            _headerImage.SetColor(ColorManager.SynthesisColor.InteractiveElementLeft,
                ColorManager.SynthesisColor.InteractiveElementRight);
            _headerImage.SetCornerRadius(8);

            _headerLabel = new Label(this, unityObject.transform.Find("Header").Find("Label").gameObject, null);
            _headerLabel.SetColor(ColorManager.SynthesisColor.InteractiveElementText);

            var itemObj =
                unityObject.transform.Find("Template").Find("Viewport").Find("Padding").Find("Content").Find("Item");

            /*_itemCheckmarkImage = new Image(this, itemObj.Find("Item Checkmark").gameObject);

            Color bgColor = ColorManager.GetColor(ColorManager.SynthesisColor.Background);
            _itemCheckmarkImage.SetColor(new Color(bgColor.r, bgColor.g, bgColor.b, 0.21f));*/

            _itemLabel = new Label(this, itemObj.Find("Item Label").gameObject, null);
            _itemLabel.SetColor(ColorManager.SynthesisColor.InteractiveElementText);

            _viewportImage = new Image(this, unityObject.transform.Find("Template").Find("Viewport").gameObject);

            _viewportImage.SetColor(ColorManager.SynthesisColor.InteractiveElementRight,
                ColorManager.SynthesisColor.InteractiveElementLeft);
            _viewportImage.SetCornerRadius(15);

            var scrollbarBG     = new Image(this, unityObject.transform.Find("Template").Find("Scrollbar").gameObject);
            var scrollbarHandle = new Image(this, unityObject.transform.Find("Template")
                                                      .Find("Scrollbar")
                                                      .Find("Sliding Area")
                                                      .Find("Handle")
                                                      .gameObject);

            scrollbarBG.SetColor(ColorManager.SynthesisColor.BackgroundSecondary);
            scrollbarBG.SetCornerRadius(10);

            // scrollbarHandle.SetColor(ColorManager.SynthesisColor.InteractiveBackground);
            scrollbarHandle.SetColor(ColorManager.SynthesisColor.Scrollbar);
            scrollbarHandle.SetGradientDirection(Mathf.PI * 1.5f);
            scrollbarHandle.SetCornerRadius(6);
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
            _dropdown = new Dropdown(this, unityObject.transform.Find("dropdown-base").gameObject, null);
            _label    = new Label(this, unityObject.transform.Find("Label").gameObject, null);
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
        private GradientImageUpdater _gradientUpdater;

        private bool _hasCustomSprite = true;
        public Sprite Sprite {
            get => _unityImage.sprite;
            set { _unityImage.sprite = value; }
        }
        public Color Color {
            get => _unityImage.color;
            set { _unityImage.color = value; }
        }

        public UImage UnityImage => _unityImage;

        public Image(UIComponent? parent, GameObject unityObject) : base(parent, unityObject) {
            _unityImage = unityObject.GetComponent<UImage>();

            if (_unityImage.sprite != null) {
                return;
            }

            _hasCustomSprite = false;

            if (unityObject.TryGetComponent<GradientImageUpdater>(out var gradientUpdater)) {
                _gradientUpdater = gradientUpdater;
            } else {
                _gradientUpdater = unityObject.AddComponent<GradientImageUpdater>();
            }
        }

        public Image SetSprite(Sprite? s) {
            _hasCustomSprite = s != null;
            Sprite           = s;
            if (s == null) {
                RootGameObject.TryGetComponent<GradientImageUpdater>(out var gradientUpdater);
                _gradientUpdater =
                    gradientUpdater ? gradientUpdater : RootGameObject.AddComponent<GradientImageUpdater>();
                _unityImage.color = Color.white;
            } else {
                GameObject.Destroy(_gradientUpdater);
            }
            return this;
        }

        public Image SetColor(ColorManager.SynthesisColor c) => SetColor(ColorManager.GetColor(c));

        public Image SetColor(Color c) => SetColor(c, c);

        public Image SetColor(ColorManager.SynthesisColor start, ColorManager.SynthesisColor end) => SetColor(
            ColorManager.GetColor(start), ColorManager.GetColor(end));

        public Image SetColor(Color start, Color end) {
            if (_hasCustomSprite) {
                _unityImage.color = start;
                return this;
            }

            if (start == Color.clear) {
                _gradientUpdater.enabled = false;
                _unityImage.material     = null;
                _unityImage.color        = start;
                return this;
            }

            _gradientUpdater.StartColor = start;
            _gradientUpdater.EndColor   = end;
            _gradientUpdater.Refresh();

            return this;
        }

        public void InvertGradient() {
            if (_hasCustomSprite)
                return;

            (_gradientUpdater.StartColor, _gradientUpdater.EndColor) =
                (_gradientUpdater.EndColor, _gradientUpdater.StartColor);
            _gradientUpdater.Refresh();
        }

        public Image SetCornerRadius(float r) {
            if (_hasCustomSprite)
                _unityImage.pixelsPerUnitMultiplier = 250f / r;
            else {
                _gradientUpdater.Radius = r;
                _gradientUpdater.Refresh();
            }
            return this;
        }

        public Image SetMultiplier(float m) {
            if (_hasCustomSprite)
                _unityImage.pixelsPerUnitMultiplier = m;

            return this;
        }

        public void SetGradientDirection(float angle) {
            if (!_gradientUpdater)
                return;

            _gradientUpdater.GradientAngle = angle;
            _gradientUpdater.Refresh();
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
            _backgroundImage.SetColor(ColorManager.GetColor(ColorManager.SynthesisColor.Scrollbar));

            _handleImage = new Image(this, unityObject.transform.Find("Sliding Area").Find("Handle").gameObject);
            _handleImage.SetColor(ColorManager.GetColor(ColorManager.SynthesisColor.MainText));
        }
    }

    public class NumberInputField : UIComponent {
        public static readonly Func<NumberInputField, NumberInputField> VerticalLayoutTemplate =
            (NumberInputField inputField) => inputField.SetTopStretch<NumberInputField>(
                leftPadding: 15f, anchoredY: inputField.Parent!.HeightOfChildren - inputField.Size.y + 15f);

        public event Action<NumberInputField, int> OnValueChanged;
        private Label _hint;
        public Label Hint => _hint;
        private Button _incrementButton;
        public Button IncrementButton => _incrementButton;
        private Button _decrementButton;
        public Button DecrementButton => _decrementButton;
        private Label _label;
        public Label Label => _label;
        private Image _backgroundImage;
        public Image BackgroundImage => _backgroundImage;
        private TMP_InputField _tmpInput;
        public TMP_InputField.ContentType ContentType => _tmpInput.contentType;

        private int _value = 0;
        public int Value {
            get => _value;
            set {
                _value         = value;
                _tmpInput.text = _value.ToString();
            }
        }

        public NumberInputField(UIComponent? parent, GameObject unityObject) : base(parent, unityObject) {
            var ifObj                = unityObject.transform.Find("InputField");
            _tmpInput                = ifObj.GetComponent<TMP_InputField>();
            _tmpInput.contentType    = TMP_InputField.ContentType.IntegerNumber;
            _tmpInput.characterLimit = 9;
            _hint                    = new Label(this, ifObj.Find("Text Area").Find("Placeholder").gameObject, null);
            _label                   = new Label(this, unityObject.transform.Find("Label").gameObject, null);
            _tmpInput.onValueChanged.AddListener(x => {
                _value = x == "" ? 0 : int.Parse(x);
                if (_eventsActive && OnValueChanged != null)
                    OnValueChanged(this, Value);
            });

            _incrementButton = new Button(this, unityObject.transform.Find("IncrementButton").gameObject, null)
                                   .AddOnClickedEvent(b => Value += Value < Int32.MaxValue ? 1 : 0);
            _decrementButton = new Button(this, unityObject.transform.Find("DecrementButton").gameObject, null)
                                   .AddOnClickedEvent(b => Value -= Value > Int32.MinValue ? 1 : 0);

            _backgroundImage = new Image(this, ifObj.gameObject);
            _backgroundImage.SetColor(ColorManager.GetColor(ColorManager.SynthesisColor.InteractiveBackground));
        }

        public NumberInputField StepIntoHint(Action<Label> mod) {
            mod(_hint);
            return this;
        }

        public NumberInputField StepIntoLabel(Action<Label> mod) {
            mod(_label);
            return this;
        }

        public NumberInputField StepIntoIncrementButton(Action<Button> mod) {
            mod(_incrementButton);
            return this;
        }

        public NumberInputField StepIntoDecrementButton(Action<Button> mod) {
            mod(_decrementButton);
            return this;
        }

        public NumberInputField AddOnValueChangedEvent(Action<NumberInputField, int> callback) {
            OnValueChanged += callback;
            return this;
        }

        public NumberInputField SetValue(int val) {
            _value         = val;
            _tmpInput.text = val.ToString();
            return this;
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