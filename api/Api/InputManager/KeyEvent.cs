using SynthesisAPI.EventBus;
using UnityEngine;

namespace Api.InputManager
{
    public class KeyEvent : IEvent
    {
        public string KeyString;

        public KeyEvent(string keyString)
        {
            KeyString = keyString;
        }

        internal KeyEvent(KeyCode kc)
        {
            KeyString = kc.ToString();
        }
    }
}