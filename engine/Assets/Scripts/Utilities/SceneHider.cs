using Synthesis.Physics;
using UnityEngine;

namespace Utilities {
    /// <summary>Hiding the scene hides the field, all robots, and disables the main HUD</summary>
    public static class SceneHider {
        private static readonly GameObject _sceneObject;
        static SceneHider() => _sceneObject = GameObject.Find("Game");

        private static bool _isHidden;
        private static int _hiddenCounter;

        public static bool IsHidden {
            get => _isHidden;
            set {
                if (value)
                    _hiddenCounter++;
                else
                    _hiddenCounter--;

                var shouldHide = _hiddenCounter > 0;
                if (shouldHide != _isHidden) {
                    _isHidden = shouldHide;
                    if (_isHidden) {
                        MainHUD.Collapsed = true;
                        _sceneObject.SetActive(false);
                        PhysicsManager.IsFrozen = true;
                    } else {
                        MainHUD.Collapsed = false;
                        _sceneObject.SetActive(true);
                        PhysicsManager.IsFrozen = false;
                    }
                }
            }
        }
    }
}
