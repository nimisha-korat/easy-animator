

#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;
using Object = UnityEngine.Object;

namespace EasyAnimator.Editor
{
    partial class EasyAnimatorToolsWindow
    {
      
        public abstract class Panel
        {
             

            private readonly AnimBool FullAnimator = new AnimBool();
            private readonly AnimBool BodyAnimator = new AnimBool();

            private int _Index;

             

            public bool IsVisible => Instance._CurrentPanel == _Index || Instance._CurrentPanel < 0;

             

            public bool IsExpanded
            {
                get { return Instance._CurrentPanel == _Index; }
                set
                {
                    if (value)
                        Instance._CurrentPanel = _Index;
                    else if (IsExpanded)
                        Instance._CurrentPanel = -1;
                }
            }

             

            public abstract string Name { get; }

            public abstract string Instructions { get; }

            public virtual string HelpURL => Strings.DocsURLs.EasyAnimatorTools;

            public virtual void OnSelectionChanged() { }

             

            public virtual void OnEnable(int index)
            {
                _Index = index;
                FullAnimator.value = FullAnimator.target = IsVisible;
                BodyAnimator.value = BodyAnimator.target = IsExpanded;
            }

            public virtual void OnDisable() { }

             

            public virtual void DoGUI()
            {
                var enabled = GUI.enabled;

                FullAnimator.target = IsVisible;

                if (EditorGUILayout.BeginFadeGroup(FullAnimator.faded))
                {
                    GUILayout.BeginVertical(EditorStyles.helpBox);

                    DoHeaderGUI();

                    BodyAnimator.target = IsExpanded;

                    if (EditorGUILayout.BeginFadeGroup(BodyAnimator.faded))
                    {
                        var instructions = Instructions;
                        if (!string.IsNullOrEmpty(instructions))
                            EditorGUILayout.HelpBox(instructions, MessageType.Info);

                        DoBodyGUI();
                    }
                    EditorGUILayout.EndFadeGroup();

                    GUILayout.EndVertical();
                }
                EditorGUILayout.EndFadeGroup();

                if (FullAnimator.isAnimating || BodyAnimator.isAnimating)
                    Repaint();

                GUI.enabled = enabled;
            }

             

           
            public virtual void DoHeaderGUI()
            {
                var area = EasyAnimatorGUI.LayoutSingleLineRect(EasyAnimatorGUI.SpacingMode.BeforeAndAfter);
                var click = GUI.Button(area, Name, EditorStyles.boldLabel);

                area.xMin = area.xMax - area.height;
                GUI.DrawTexture(area, HelpIcon);

                if (click)
                {
                    if (area.Contains(Event.current.mousePosition))
                    {
                        Application.OpenURL(HelpURL);
                        return;
                    }
                    else
                    {
                        IsExpanded = !IsExpanded;
                    }
                }
            }

             

            public abstract void DoBodyGUI();

             

            public static bool SaveModifiedAsset<T>(string saveTitle, string saveMessage,
                T obj, Action<T> modify) where T : Object
            {
                var originalPath = AssetDatabase.GetAssetPath(obj);

                var extension = Path.GetExtension(originalPath);
                if (extension[0] == '.')
                    extension = extension.Substring(1, extension.Length - 1);

                var directory = Path.GetDirectoryName(originalPath);

                var newName = Path.GetFileNameWithoutExtension(AssetDatabase.GenerateUniqueAssetPath(originalPath));
                var savePath = EditorUtility.SaveFilePanelInProject(saveTitle, newName, extension, saveMessage, directory);
                if (string.IsNullOrEmpty(savePath))
                    return false;

                if (originalPath != savePath)
                {
                    obj = Instantiate(obj);
                    AssetDatabase.CreateAsset(obj, savePath);
                }

                modify(obj);

                AssetDatabase.SaveAssets();

                return true;
            }

             

            private static Texture _HelpIcon;

            public static Texture HelpIcon
            {
                get
                {
                    if (_HelpIcon == null)
                        _HelpIcon = EasyAnimatorGUI.LoadIcon("_Help");
                    return _HelpIcon;
                }
            }

             

            private static int _DropIndex;

            protected void HandleDragAndDropIntoList<T>(Rect area, IList<T> list, bool overwrite,
                Func<T, bool> validate = null) where T : Object
            {
                if (overwrite)
                {
                    _DropIndex = 0;
                    EasyAnimatorGUI.HandleDragAndDrop(area, validate, (drop) =>
                    {
                        if (_DropIndex < list.Count)
                        {
                            RecordUndo();
                            list[_DropIndex++] = drop;
                        }
                    });
                }
                else
                {
                    EasyAnimatorGUI.HandleDragAndDrop(area, validate, (drop) =>
                    {
                        list.Add(drop);
                    });
                }
            }

             
        }
    }
}

#endif

