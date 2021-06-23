using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Synthesis.UI;

namespace Synthesis.Attributes {
    public class ContextMenuOptionAttribute : Attribute {

        public string Title { get; private set; } = string.Empty;
        public Action Callback { get; private set; } = null;
        public Sprite Icon { get; private set; } = null;

        public ContextMenuOptionAttribute() { }

        public ContextMenuOptionAttribute(string title) {
            Title = title;
        }

        public ContextMenuOptionAttribute(string title, string icon) {
            Title = title;
            Icon = (Sprite)typeof(ImageManager).GetProperty(icon).GetValue(null);
        }

        public ContextMenuOptionAttribute(string title, Action callback) {
            Title = title;
            Callback = callback;
        }

        public ContextMenuOptionAttribute(string title, string icon, Action callback) {
            Title = title;
            Icon = (Sprite)typeof(ImageManager).GetProperty(icon).GetValue(null);
            Callback = callback;
        }
    }
}
