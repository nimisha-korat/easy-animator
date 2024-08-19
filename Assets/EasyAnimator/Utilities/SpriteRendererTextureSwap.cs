
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value.

using System.Collections.Generic;
using UnityEngine;

namespace EasyAnimator
{
 
    [AddComponentMenu(Strings.MenuPrefix + "Sprite Renderer Texture Swap")]
    [HelpURL(Strings.DocsURLs.APIDocumentation + "/" + nameof(SpriteRendererTextureSwap))]
    [DefaultExecutionOrder(DefaultExecutionOrder)]
    public sealed class SpriteRendererTextureSwap : MonoBehaviour
    {
         

        public const int DefaultExecutionOrder = 30000;

         

        [SerializeField]
        [Tooltip("The SpriteRenderer that will have its Sprite modified")]
        private SpriteRenderer _Renderer;

        public ref SpriteRenderer Renderer => ref _Renderer;

         

        [SerializeField]
        [Tooltip("The replacement for the original Sprite texture")]
        private Texture2D _Texture;

      
        public Texture2D Texture
        {
            get => _Texture;
            set
            {
                _Texture = value;
                RefreshSpriteMap();
            }
        }

         

        private Dictionary<Sprite, Sprite> _SpriteMap;

        private void RefreshSpriteMap() => _SpriteMap = GetSpriteMap(_Texture);

         

        private void Awake() => RefreshSpriteMap();

        private void OnValidate() => RefreshSpriteMap();

         

        private void LateUpdate()
        {
            if (_Renderer == null)
                return;

            var sprite = _Renderer.sprite;
            if (TrySwapTexture(_SpriteMap, _Texture, ref sprite))
                _Renderer.sprite = sprite;
        }

         

        public void ClearCache()
        {
            DestroySprites(_SpriteMap);
        }

         

        private static readonly Dictionary<Texture2D, Dictionary<Sprite, Sprite>>
            TextureToSpriteMap = new Dictionary<Texture2D, Dictionary<Sprite, Sprite>>();

         

        public static Dictionary<Sprite, Sprite> GetSpriteMap(Texture2D texture)
        {
            if (texture == null)
                return null;

            if (!TextureToSpriteMap.TryGetValue(texture, out var map))
                TextureToSpriteMap.Add(texture, map = new Dictionary<Sprite, Sprite>());

            return map;
        }

         

        public static bool TrySwapTexture(Dictionary<Sprite, Sprite> spriteMap, Texture2D texture, ref Sprite sprite)
        {
            if (spriteMap == null ||
                sprite == null ||
                texture == null ||
                sprite.texture == texture)
                return false;

            if (!spriteMap.TryGetValue(sprite, out var otherSprite))
            {
                var pivot = sprite.pivot;
                pivot.x /= sprite.rect.width;
                pivot.y /= sprite.rect.height;

                otherSprite = Sprite.Create(texture,
                    sprite.rect, pivot, sprite.pixelsPerUnit,
                    0, SpriteMeshType.FullRect, sprite.border, false);

#if UNITY_ASSERTIONS
                var name = sprite.name;
                var originalTextureName = sprite.texture.name;
                var index = name.IndexOf(originalTextureName);
                if (index >= 0)
                {
                    var newName =
                        texture.name +
                        name.Substring(index + originalTextureName.Length, name.Length - (index + originalTextureName.Length));

                    if (index > 0)
                        newName = name.Substring(0, index) + newName;

                    name = newName;
                }

                otherSprite.name = name;
#endif

                spriteMap.Add(sprite, otherSprite);
            }

            sprite = otherSprite;
            return true;
        }

         

        public static void DestroySprites(Dictionary<Sprite, Sprite> spriteMap)
        {
            if (spriteMap == null)
                return;

            foreach (var sprite in spriteMap.Values)
                Destroy(sprite);

            spriteMap.Clear();
        }

         

        public static void DestroySprites(Texture2D texture)
        {
            if (TextureToSpriteMap.TryGetValue(texture, out var spriteMap))
            {
                TextureToSpriteMap.Remove(texture);
                DestroySprites(spriteMap);
            }
        }

         
    }
}
