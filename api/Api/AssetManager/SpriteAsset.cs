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

            _sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new UnityEngine.Vector2(.5f, .5f));

            return this;
        }

        private Sprite _sprite = null!;

        [ExposedApi]
        public Sprite Sprite {
            get {
                using var _ = ApiCallSource.StartExternalCall();
                return GetSpriteInner();
            } 
        }

        private Sprite GetSpriteInner()
        {
            ApiCallSource.AssertAccess(Permissions, Access.Read);
            return _sprite;
        }
    }
}
