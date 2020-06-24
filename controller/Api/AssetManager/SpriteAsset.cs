using SynthesisAPI.VirtualFileSystem;
using System;
using UnityEngine;

namespace SynthesisAPI.AssetManager
{
    /// <summary>
    /// Representation of a Unity Sprite asset
    /// 
    /// Only PNG and JPEG images are supported for import
    /// </summary>
    public class SpriteAsset : Asset
    {
        public SpriteAsset(string name, Guid owner, Permissions perm, string sourcePath)
        {
            Init(name, owner, perm, sourcePath);
        }

        public override IEntry Load(byte[] data)
        {
            Texture2D texture = new Texture2D(1, 1); // LoadImage will replace with incoming image size

            if (!texture.LoadImage(data)) // LoadImage works for PNG and JPEG images as byte arrays
            {
                throw new Exception("Failed to load image");
            }

            Sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(.5f, .5f));

            return this;
        }

        public Sprite Sprite { get; private set; }
    }
}
