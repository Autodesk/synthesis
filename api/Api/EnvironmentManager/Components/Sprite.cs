using SynthesisAPI.AssetManager;
using SynthesisAPI.Modules.Attributes;
using System.Drawing;

namespace SynthesisAPI.EnvironmentManager.Components
{
	// [BuiltIn]
	public class Sprite : Component
	{
		internal UnityEngine.Sprite _sprite;
		internal bool _flipX;
		internal bool _flipY;
		internal Color _color = Color.FromArgb(255, 255, 255, 255);
		internal bool _visible;

		public Sprite() { }

		public Sprite(SpriteAsset spriteAsset)
		{
			_sprite = spriteAsset.Sprite;
		}

		public void SetSprite(SpriteAsset spriteAsset)
		{
			_sprite = spriteAsset.Sprite;
			Changed = true;
		}

		public bool FlipX
		{
			get => _flipX;
			set
			{
				_flipX = value;
				Changed = true;
			}
		}
		public bool FlipY
		{
			get => _flipY;
			set
			{
				_flipY = value;
				Changed = true;
			}
		}

		public bool Visible
		{
			get => _visible;
			set
			{
				_visible = value;
				Changed = true;
			}
		}

		public Color Color
		{
			get => _color;
			set
			{
				_color = value;
				Changed = true;
			}
		}

		public int Width => _sprite.texture.width;
		public int Height => _sprite.texture.height;

		public bool Changed { get; private set; } = true;
		internal void ProcessedChanges() => Changed = false;
	}
}