using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

using UButton = UnityEngine.UI.Button;

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
        private TMP_Text _title;
        protected TMP_Text Title => _title;
        private TMP_Text _description;
        protected TMP_Text Description => _description;
        
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
            _title = header.Find("Title").GetComponent<TMP_Text>();
            _description = header.Find("Description").GetComponent<TMP_Text>();

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
    }

    public abstract class UIComponent {
        protected GameObject RootGameObject;
        protected UIComponent? Parent;
        protected List<UIComponent> Children = new List<UIComponent>();

        public UIComponent(UIComponent? parentComponent, GameObject rootGameObject) {
            Parent = parentComponent;
            RootGameObject = rootGameObject;
        }
    }

    public class Content : UIComponent {

        private Vector2 _size;
        private RectTransform _rTrans;

        public Content(UIComponent parent, GameObject unityObject, Vector2 size) : base(parent, unityObject) {
            _rTrans = unityObject.GetComponent<RectTransform>();
            // _rTrans.offsetMax = Vector2.zero;
            // _rTrans.offsetMin = Vector2.zero;
            _rTrans.sizeDelta = size;
            _size = size;
        }

        public (Content left, Content right) SplitLeftRight(float leftWidth, float padding) {
            var leftContentObject = GameObject.Instantiate(SynthesisAssetCollection.GetModalPrefab("content-base"), base.RootGameObject.transform);
            var leftRt = leftContentObject.GetComponent<RectTransform>();
            leftRt.anchorMax = new Vector2(0f, 0.5f);
            leftRt.anchorMin = new Vector2(0f, 0.5f);
            leftRt.anchoredPosition = new Vector2(leftWidth / 2f, 0f);
            // leftRt.sizeDelta = new Vector2(leftWidth, leftRt.sizeDelta.y);
            var leftContent = new Content(this, leftContentObject, new Vector2(leftWidth, _size.y));

            var rightContentObject = GameObject.Instantiate(SynthesisAssetCollection.GetModalPrefab("content-base"), base.RootGameObject.transform);
            var rightRt = rightContentObject.GetComponent<RectTransform>();
            rightRt.anchorMax = new Vector2(1f, 0.5f);
            rightRt.anchorMin = new Vector2(1f, 0.5f);
            float rightWidth = (_size.x - leftWidth) - padding;
            rightRt.anchoredPosition = new Vector2(-rightWidth / 2f, 0f);
            // rightRt.sizeDelta = new Vector2(rightWidth, rightRt.sizeDelta.y);
            var rightContent = new Content(this, rightContentObject, new Vector2(rightWidth, _size.y));

            return (leftContent, rightContent);
        }
        public (Content top, Content bottom) SplitTopBottom(float topHeight, float padding) {
            throw new NotImplementedException();
        }

        public Label CreateLabel() {
            throw new NotImplementedException();
        }
        public Toggle CreateToggle() {
            throw new NotImplementedException();
        }
        public Slider CreateSlider() {
            throw new NotImplementedException();
        }
        public InputField CreateInputField() {
            throw new NotImplementedException();
        }
        public ScrollView CreateScrollView() {
            throw new NotImplementedException();
        }
    }

    public class ScrollView : UIComponent {
        private Content _content;

        public ScrollView(UIComponent parent, GameObject unityObject) : base(parent, unityObject) { }
    }

    public class Label : UIComponent {
        public string _text = "New Label";
        public string Text {
            get => _text;
            set {
                _text = value;
                // TODO: Update UI objects
            }
        }

        public Label(UIComponent parent, GameObject unityObject) : base(parent, unityObject) { }
    }

    public class Toggle : UIComponent {
        public Toggle(UIComponent parent, GameObject unityObject) : base(parent, unityObject) { }
    }

    public class Slider : UIComponent {
        public Slider(UIComponent parent, GameObject unityObject) : base(parent, unityObject) { }
    }

    public class InputField : UIComponent {
        public InputField(UIComponent parent, GameObject unityObject) : base(parent, unityObject) { }
    }

    public class LabeledButton : UIComponent {
        public LabeledButton(UIComponent parent, GameObject unityObject) : base(parent, unityObject) { }
    }

    public class Button : UIComponent {
        private Vector2 _size;

        private TMP_Text _buttonText;
        public TMP_Text ButtonText => _buttonText;
        private UButton _unityButton;
        public UButton UnityButton => _unityButton;

        public Button(UIComponent parent, GameObject unityObject, Vector2? size) : base(parent, unityObject) {
            if (size == null) {
                size = unityObject.GetComponent<RectTransform>().sizeDelta;
            } else {
                _size = size.Value;
            }

            _buttonText = unityObject.transform.Find("Text (TMP)").GetComponent<TMP_Text>();
            _unityButton = unityObject.GetComponent<UButton>();
        }
    }
}
