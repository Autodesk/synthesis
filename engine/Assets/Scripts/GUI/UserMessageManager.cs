using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Synthesis.GUI
{
    /// <summary>
    /// Renders timed messages at the bottom of the screen.
    /// </summary>
    public class UserMessageManager
    {
        // Dimensions of the overlay at the bottom.
        private const int WIDTH = 400;
        private const int HEIGHT = 400;

        public static float scale = 1;

        /// <summary>
        /// Object representing a timed message.
        /// </summary>
        private class UserMessage
        {
            public const float FADE_TIME = 1f; // s
            public readonly String message;
            public readonly Color foreground, background;
            public readonly float lifetime;
            public readonly float endTime;
            public float ttl;

            public UserMessage(String msg, float ttl, Color foreground, Color background)
            {
                this.message = msg;
                this.foreground = foreground;
                this.background = background;
                this.lifetime = ttl;
                this.endTime = Time.time + ttl;
                this.ttl = ttl;
            }
        };

        /// <summary>
        /// All currently registered messages.
        /// </summary>
        private static List<UserMessage> messages = new List<UserMessage>();

        /// <summary>
        /// Adds the given message to the renderer.
        /// </summary>
        /// <param name="msg">Message</param>
        /// <param name="ttl">Time to live in seconds</param>
        /// <param name="foreground">Foreground color</param>
        /// <param name="background">Background color</param>
        public static void Dispatch(String msg, float ttl, Color foreground, Color background)
        {
            lock (messages)
            {
                messages.Insert(0, new UserMessage(msg, ttl, foreground, background));
            }
        }

        /// <summary>
        /// Adds the given message to the renderer.
        /// </summary>
        /// <param name="msg">Message</param>
        /// <param name="ttl">Time to live in seconds</param>
        public static void Dispatch(String msg, float ttl)
        {
            Dispatch(msg, ttl, Color.white, new Color(0f, 0f, 0f, 0.9f));
        }

        /// <summary>
        /// Renders the messages
        /// </summary>
        public static void Render()
        {
            lock (messages)
            {
                Color initFG = UnityEngine.GUI.color;
                Color initBG = UnityEngine.GUI.backgroundColor;

                GUILayout.BeginArea(new Rect((Screen.width / 2) - (WIDTH / 2), Screen.height - HEIGHT - 10, WIDTH, HEIGHT));

                float y = 0;

                foreach (UserMessage msg in messages)
                {
                    msg.ttl = msg.endTime - Time.time;
                    Color fg = msg.foreground, bg = msg.background;
                    float alpha = msg.ttl < UserMessage.FADE_TIME ? msg.ttl / UserMessage.FADE_TIME : 1f;
                    fg.a *= alpha;
                    bg.a *= alpha;
                    UnityEngine.GUI.color = fg;
                    UnityEngine.GUI.backgroundColor = bg;
                    GUIContent content = new GUIContent(msg.message);
                    Vector2 size = UnityEngine.GUI.skin.GetStyle("Button").CalcSize(content);
                    if (UnityEngine.GUI.Button(new Rect((WIDTH - size.x) / 2f, HEIGHT - y - size.y, size.x, size.y), content))
                    {
                        msg.ttl = -1;
                    }
                    y += size.y + 2;
                }
                messages.RemoveAll((UserMessage msg) =>
                {
                    return msg.ttl <= 0;
                });
                GUILayout.EndArea();

                UnityEngine.GUI.color = initFG;
                UnityEngine.GUI.backgroundColor = initBG;
            }
        }
    }
}
