using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class UserMessageManager
{
    private const int WIDTH = 400;
    private const int HEIGHT = 400;

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

    private static List<UserMessage> messages = new List<UserMessage>();

    public static void Dispatch(String msg, float ttl, Color foreground, Color background)
    {
        lock (messages)
        {
            messages.Insert(0, new UserMessage(msg, ttl, foreground, background));
        }
    }

    public static void Dispatch(String msg, float ttl = 10)
    {
        Dispatch(msg, ttl, Color.white, new Color(0f, 0f, 0f, 0.9f));
    }

    public static void Render()
    {
        lock (messages)
        {
            GUILayout.BeginArea(new Rect((Screen.width / 2) - (WIDTH / 2), Screen.height - HEIGHT - 10, WIDTH, HEIGHT));
            float deltaTime = Time.deltaTime;
            float y = 0;
            foreach (UserMessage msg in messages)
            {
                msg.ttl -= deltaTime;
                Color fg = msg.foreground, bg = msg.background;
                float alpha = Math.Min(2f * msg.ttl / msg.lifetime, 1f);
                fg.a *= alpha;
                bg.a *= alpha;
                GUI.color = fg;
                GUI.backgroundColor = bg;
                GUIContent content = new GUIContent(msg.message);
                Vector2 size = GUI.skin.GetStyle("Button").CalcSize(content);
                if (GUI.Button(new Rect((WIDTH - size.x) / 2f, HEIGHT - y - size.y, size.x, size.y), content))
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
        }
    }
}
