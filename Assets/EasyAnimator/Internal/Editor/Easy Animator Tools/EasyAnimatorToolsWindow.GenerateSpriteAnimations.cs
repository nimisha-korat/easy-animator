

#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace EasyAnimator.Editor
{
    partial class EasyAnimatorToolsWindow
    {
      
        [Serializable]
        public sealed class GenerateSpriteAnimations : SpriteModifierPanel
        {
             
            #region Panel
             

            [NonSerialized] private readonly List<string> Names = new List<string>();
            [NonSerialized] private readonly Dictionary<string, List<Sprite>> NameToSprites = new Dictionary<string, List<Sprite>>();
            [NonSerialized] private ReorderableList _Display;
            [NonSerialized] private bool _NamesAreDirty;

             

            public override string Name => "Generate Sprite Animations";

            public override string HelpURL => Strings.DocsURLs.GenerateSpriteAnimations;

            public override string Instructions
            {
                get
                {
                    if (Sprites.Count == 0)
                        return "Select the Sprites you want to generate animations from.";

                    return "Click Generate.";
                }
            }

             

            public override void OnEnable(int index)
            {
                base.OnEnable(index);

                _Display = CreateReorderableList(Names, "Animations to Generate", (area, elementIndex, isActive, isFocused) =>
                {
                    area.y = Mathf.Ceil(area.y + EditorGUIUtility.standardVerticalSpacing * 0.5f);
                    area.height = EditorGUIUtility.singleLineHeight;

                    var name = Names[elementIndex];
                    var sprites = NameToSprites[name];

                    BeginChangeCheck();
                    name = EditorGUI.TextField(area, name);
                    if (EndChangeCheck())
                    {
                        Names[elementIndex] = name;
                    }

                    for (int i = 0; i < sprites.Count; i++)
                    {
                        area.y += area.height + EditorGUIUtility.standardVerticalSpacing;

                        var sprite = sprites[i];
                        BeginChangeCheck();
                        sprite = (Sprite)EditorGUI.ObjectField(area, sprite, typeof(Sprite), false);
                        if (EndChangeCheck())
                        {
                            sprites[i] = sprite;
                        }
                    }
                });

                _Display.elementHeightCallback = (elementIndex) =>
                {
                    var lineCount = NameToSprites[Names[elementIndex]].Count + 1;
                    return
                        EditorGUIUtility.singleLineHeight * lineCount +
                        EditorGUIUtility.standardVerticalSpacing * lineCount;
                };
            }

             

            public override void OnSelectionChanged()
            {
                NameToSprites.Clear();
                Names.Clear();
                _NamesAreDirty = true;
            }

             

            public override void DoBodyGUI()
            {
                EditorGUILayout.PropertyField(EasyAnimatorSettings.NewAnimationFrameRate);

                var sprites = Sprites;

                if (_NamesAreDirty)
                {
                    _NamesAreDirty = false;
                    GatherNameToSprites(sprites, NameToSprites);
                    Names.AddRange(NameToSprites.Keys);
                }

                using (new EditorGUI.DisabledScope(true))
                {
                    _Display.DoLayoutList();

                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.FlexibleSpace();

                        GUI.enabled = sprites.Count > 0;

                        if (GUILayout.Button("Generate"))
                        {
                            EasyAnimatorGUI.Deselect();
                            GenerateAnimationsBySpriteName(sprites);
                        }
                    }
                    GUILayout.EndHorizontal();
                }

                EditorGUILayout.HelpBox("This function is also available via:" +
                    "\n - The 'Assets/Create/EasyAnimator' menu." +
                    "\n - The Cog icon in the top right of the Inspector for Sprite and Texture assets",
                    MessageType.Info);
            }

             
            #endregion
             
            #region Methods
             

            private static void GenerateAnimationsBySpriteName(List<Sprite> sprites)
            {
                if (sprites.Count == 0)
                    return;

                sprites.Sort(NaturalCompare);

                var nameToSprites = new Dictionary<string, List<Sprite>>();
                GatherNameToSprites(sprites, nameToSprites);

                var pathToSprites = new Dictionary<string, List<Sprite>>();

                var message = ObjectPool.AcquireStringBuilder()
                    .Append("Do you wish to generate the following animations?");

                const int MaxLines = 25;
                var line = 0;
                foreach (var nameToSpriteGroup in nameToSprites)
                {
                    var path = AssetDatabase.GetAssetPath(nameToSpriteGroup.Value[0]);
                    path = Path.GetDirectoryName(path);
                    path = Path.Combine(path, nameToSpriteGroup.Key + ".anim");
                    pathToSprites.Add(path, nameToSpriteGroup.Value);

                    if (++line <= MaxLines)
                    {
                        message.AppendLine()
                            .Append("- ")
                            .Append(path)
                            .Append(" (")
                            .Append(nameToSpriteGroup.Value.Count)
                            .Append(" frames)");
                    }
                }

                if (line > MaxLines)
                {
                    message.AppendLine()
                        .Append("And ")
                        .Append(line - MaxLines)
                        .Append(" others.");
                }

                if (!EditorUtility.DisplayDialog("Generate Sprite Animations?", message.ReleaseToString(), "Generate", "Cancel"))
                    return;

                foreach (var pathToSpriteGroup in pathToSprites)
                    CreateAnimation(pathToSpriteGroup.Key, pathToSpriteGroup.Value.ToArray());

                AssetDatabase.SaveAssets();
            }

             

            private static char[] _Numbers, _TrimOther;

            private static void GatherNameToSprites(List<Sprite> sprites, Dictionary<string, List<Sprite>> nameToSprites)
            {
                for (int i = 0; i < sprites.Count; i++)
                {
                    var sprite = sprites[i];
                    var name = sprite.name;

                    // Remove numbers from the end.
                    if (_Numbers == null)
                        _Numbers = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
                    name = name.TrimEnd(_Numbers);

                    // Then remove other characters from the end.
                    if (_TrimOther == null)
                        _TrimOther = new char[] { ' ', '_', '-' };
                    name = name.TrimEnd(_TrimOther);

                    // Doing both at once would turn "Attack2-0" (Attack 2 Frame 0) into "Attack" (losing the number).

                    if (!nameToSprites.TryGetValue(name, out var spriteGroup))
                    {
                        spriteGroup = new List<Sprite>();
                        nameToSprites.Add(name, spriteGroup);
                    }

                    // Add the sprite to the group if it's not a duplicate.
                    if (spriteGroup.Count == 0 || spriteGroup[spriteGroup.Count - 1] != sprite)
                        spriteGroup.Add(sprite);
                }
            }

             

            private static void CreateAnimation(string path, params Sprite[] sprites)
            {
                var frameRate = EasyAnimatorSettings.NewAnimationFrameRate.floatValue;

                var clip = new AnimationClip
                {
                    frameRate = frameRate,
                };

                var spriteKeyFrames = new ObjectReferenceKeyframe[sprites.Length];
                for (int i = 0; i < spriteKeyFrames.Length; i++)
                {
                    spriteKeyFrames[i] = new ObjectReferenceKeyframe
                    {
                        time = i / (float)frameRate,
                        value = sprites[i]
                    };
                }

                var spriteBinding = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");
                AnimationUtility.SetObjectReferenceCurve(clip, spriteBinding, spriteKeyFrames);

                AssetDatabase.CreateAsset(clip, path);
            }

             
            #endregion
             
            #region Menu Functions
             

            private const string GenerateAnimationsBySpriteNameFunctionName = "Generate Animations By Sprite Name";

             

            [MenuItem(Strings.CreateMenuPrefix + GenerateAnimationsBySpriteNameFunctionName, validate = true)]
            private static bool ValidateGenerateAnimationsBySpriteName()
            {
                var selection = Selection.objects;
                for (int i = 0; i < selection.Length; i++)
                {
                    var selected = selection[i];
                    if (selected is Sprite || selected is Texture)
                        return true;
                }

                return false;
            }

            [MenuItem(Strings.CreateMenuPrefix + GenerateAnimationsBySpriteNameFunctionName, priority = Strings.AssetMenuOrder + 13)]
            private static void GenerateAnimationsBySpriteName()
            {
                var sprites = new List<Sprite>();

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

                GenerateAnimationsBySpriteName(sprites);
            }

             

            private static List<Sprite> _CachedSprites;

            
            private static List<Sprite> GetCachedSpritesToGenerateAnimations()
            {
                if (_CachedSprites == null)
                    return _CachedSprites = new List<Sprite>();

                // Delay the call in case multiple objects are selected.
                if (_CachedSprites.Count == 0)
                {
                    EditorApplication.delayCall += () =>
                    {
                        GenerateAnimationsBySpriteName(_CachedSprites);
                        _CachedSprites.Clear();
                    };
                }

                return _CachedSprites;
            }

             

            [MenuItem("CONTEXT/" + nameof(Sprite) + GenerateAnimationsBySpriteNameFunctionName)]
            private static void GenerateAnimationsFromSpriteByName(MenuCommand command)
            {
                GetCachedSpritesToGenerateAnimations().Add((Sprite)command.context);
            }

             

            [MenuItem("CONTEXT/" + nameof(TextureImporter) + GenerateAnimationsBySpriteNameFunctionName, validate = true)]
            private static bool ValidateGenerateAnimationsFromTextureBySpriteName(MenuCommand command)
            {
                var importer = (TextureImporter)command.context;
                var sprites = LoadAllSpritesAtPath(importer.assetPath);
                return sprites.Length > 0;
            }

            
            [MenuItem("CONTEXT/" + nameof(TextureImporter) + GenerateAnimationsBySpriteNameFunctionName)]
            private static void GenerateAnimationsFromTextureBySpriteName(MenuCommand command)
            {
                var cachedSprites = GetCachedSpritesToGenerateAnimations();
                var importer = (TextureImporter)command.context;
                cachedSprites.AddRange(LoadAllSpritesAtPath(importer.assetPath));
            }

             
            #endregion
             
        }
    }
}

#endif

