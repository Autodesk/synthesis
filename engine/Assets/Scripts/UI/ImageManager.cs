using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Synthesis.UI {
    public class ImageManager : MonoBehaviour {
        public Sprite closeIcon;
        public Sprite collapseIcon;

        public static Sprite CloseIcon    => Instance.closeIcon;
        public static Sprite CollapseIcon => Instance.collapseIcon;

        private static ImageManager instance;
        public static ImageManager Instance {
            get {
                if (instance == null)
                    instance = GameObject.FindObjectOfType<ImageManager>();
                return instance;
            }
        }
    }
}
