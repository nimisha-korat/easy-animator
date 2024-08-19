

#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace EasyAnimator.Editor
{
    partial class EasyAnimatorToolsWindow
    {
       
        [Serializable]
        public abstract class SpriteModifierPanel : Panel
        {
             

            private static readonly List<Sprite> SelectedSprites = new List<Sprite>();
            private static bool _HasGatheredSprites;

            public static List<Sprite> Sprites
            {
                get
                {
                    if (!_HasGatheredSprites)
                    {
                        _HasGatheredSprites = true;
                        GatherSelectedSprites(SelectedSprites);
                    }

                    return SelectedSprites;
                }
            }

            public override void OnSelectionChanged()
            {
                _HasGatheredSprites = false;
            }

             

            public static void GatherSelectedSprites(List<Sprite> sprites)
            {
                sprites.Clear();

                var selection = Selection.objects;
                for (int i = 0; i < selection.Length; i++)
                {
                    var selected = selection[i];
                    if (selected is Sprite sprite)
                    {
                        sprites.Add(sprite);
                    }
                    else if (selected is Texture2D texture)
                    {
                        sprites.AddRange(LoadAllSpritesInTexture(texture));
                    }
                }

                sprites.Sort(NaturalCompare);
            }

             

            protected virtual string AreYouSure => "Are you sure you want to modify these Sprites?";

            protected virtual void PrepareToApply() { }

            protected virtual void Modify(ref SpriteMetaData data, Sprite sprite) { }

            protected virtual void Modify(TextureImporter importer, List<Sprite> sprites)
            {
                var spriteSheet = importer.spritesheet;
                var hasError = false;

                for (int iSprite = 0; iSprite < sprites.Count; iSprite++)
                {
                    var sprite = sprites[iSprite];
                    for (int iSpriteData = 0; iSpriteData < spriteSheet.Length; iSpriteData++)
                    {
                        ref var spriteData = ref spriteSheet[iSpriteData];
                        if (spriteData.name == sprite.name &&
                            spriteData.rect == sprite.rect)
                        {
                            Modify(ref spriteData, sprite);
                            sprites.RemoveAt(iSprite--);

                            if (spriteData.rect.xMin < 0 ||
                                spriteData.rect.yMin < 0 ||
                                spriteData.rect.xMax > sprite.texture.width ||
                                spriteData.rect.yMax > sprite.texture.height)
                            {
                                hasError = true;
                                Debug.LogError($"This modification would have put '{sprite.name}' out of bounds" +
                                    $" so '{importer.assetPath}' was not modified.");
                            }

                            break;
                        }
                    }
                }

                if (!hasError)
                {
                    importer.spritesheet = spriteSheet;
                    EditorUtility.SetDirty(importer);
                    importer.SaveAndReimport();
                }
            }

             

            protected void AskAndApply()
            {
                if (!EditorUtility.DisplayDialog("Are You Sure?",
                    AreYouSure + "\n\nThis operation cannot be undone.",
                    "Modify", "Cancel"))
                    return;

                PrepareToApply();

                var pathToSprites = new Dictionary<string, List<Sprite>>();
                var sprites = Sprites;
                for (int i = 0; i < sprites.Count; i++)
                {
                    var sprite = sprites[i];

                    var path = AssetDatabase.GetAssetPath(sprite);

                    if (!pathToSprites.TryGetValue(path, out var spritesAtPath))
                        pathToSprites.Add(path, spritesAtPath = new List<Sprite>());

                    spritesAtPath.Add(sprite);
                }

                foreach (var asset in pathToSprites)
                {
                    var importer = (TextureImporter)AssetImporter.GetAtPath(asset.Key);

                    Modify(importer, asset.Value);

                    if (asset.Value.Count > 0)
                    {
                        var message = ObjectPool.AcquireStringBuilder()
                            .Append("Unable to find data at '")
                            .Append(asset.Key)
                            .Append("' for ")
                            .Append(asset.Value.Count)
                            .Append(" Sprites:");

                        for (int i = 0; i < sprites.Count; i++)
                        {
                            message.AppendLine()
                                .Append(" - ")
                                .Append(sprites[i].name);
                        }

                        Debug.LogError(message.ReleaseToString(), AssetDatabase.LoadAssetAtPath<Object>(asset.Key));
                    }
                }
            }

             
        }
    }
}

#endif

