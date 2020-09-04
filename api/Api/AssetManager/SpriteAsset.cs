using MathNet.Spatial.Euclidean;
using SynthesisAPI.Utilities;
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
        public SpriteAsset(string name, Permissions perm, string sourcePath)
        {
            Init(name, perm, sourcePath);
        }

        public override IEntry Load(byte[] data)
        {
            Texture2D texture = new Texture2D(1, 1); // LoadImage will replace with incoming image size

            if (!texture.LoadImage(data)) // LoadImage works for PNG and JPEG images as byte arrays
            {
                throw new Exception("Failed to load image");
            }

            Sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new UnityEngine.Vector2(.5f, .5f));

            return this;
        }

        /// <summary>
        /// Create a sprite asset from a texture
        /// </summary>
        /// <param name="texture">Texture to use</param>
        /// <param name="position">Defines rectangular section of the texture to use for the sprite</param>
        /// <param name="size">Defines rectangular section of the texture to use for the sprite</param>
        /// <param name="pivot">Sprite's pivot point relative to its graphic rectangle</param>
        internal void SetSprite(Texture2D texture, Vector2D position, Vector2D size, Vector2D pivot)
        {
            Sprite = Sprite.Create(texture, new Rect(MathUtil.MapVector2D(position), MathUtil.MapVector2D(size)), MathUtil.MapVector2D(pivot));
        }

        internal Sprite Sprite { get; private set; }
    }
}