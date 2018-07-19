using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Synthesis.GUI
{
    // TODO: Update this class to work as intended.

    /// <summary>
    /// Renders timed messages at the bottom of the screen.
    /// </summary>
    public class UserMessageManager
    {
        // Dimensions of the overlay at the bottom.
        private const int Width = 800;
        private const int Height = 800;
        private const int FontSize = 24;

        public static float scale = 1;

        private static readonly GUIStyle style = new GUIStyle
        {
            font = Resources.Load<Font>("Fonts/Russo_One"),
            fontSize = FontSize,
            normal = new GUIStyleState
            {
                background = Resources.Load<Texture2D>("Images/blacktexture"),
                textColor = Color.white
            }
        };

        /// <summary>
        /// Object representing a timed message.
        /// </summary>
        private class UserMessage
        {
            public readonly String message;
            public readonly Color foreground, background;
            public readonly float lifetime;
            public float ttl;
            
            public UserMessage(String msg, float ttl, Color foreground, Color background)
            {
                this.message = msg;
                this.foreground = foreground;
                this.background = background;
                this.lifetime = ttl;
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
            Dispatch(msg, ttl, Color.white, new Color(0f, 0f, 0f, 1f));
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

                GUILayout.BeginArea(new Rect((Screen.width / 2) - (Width / 2), Screen.height - Height - 10, Width, Height));
                float deltaTime = Time.deltaTime;
                float y = 0;

                foreach (UserMessage msg in messages)
                {
                    msg.ttl -= deltaTime;
                    Color fg = msg.foreground, bg = msg.background;
                    float alpha = Math.Min(2f * msg.ttl / msg.lifetime, 1f);
                    fg.a *= alpha;
                    bg.a *= alpha;
                    UnityEngine.GUI.color = fg;
                    UnityEngine.GUI.backgroundColor = bg;
                    style.fontSize = (int)Math.Round(FontSize * scale);
                    GUIContent content = new GUIContent(msg.message);
                    Vector2 size = style.CalcSize(content);

                    if (UnityEngine.GUI.Button(new Rect((Width - size.x) / 2f, Height - y - size.y, size.x, size.y), content, style))
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
