

#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Object = UnityEngine.Object;

namespace EasyAnimator.Editor
{
   
    internal sealed partial class EasyAnimatorToolsWindow : EditorWindow
    {
         

        public const string Name = "EasyAnimator Tools";

        public static EasyAnimatorToolsWindow Instance { get; private set; }

        [SerializeField] private PackTextures _PackTextures;
        [SerializeField] private ModifySprites _ModifySprites;
        [SerializeField] private RenameSprites _RenameSprites;
        [SerializeField] private GenerateSpriteAnimations _GenerateSpriteAnimations;
        [SerializeField] private RemapSpriteAnimation _RemapSpriteAnimation;
        [SerializeField] private RemapAnimationBindings _RemapAnimationBindings;
        [SerializeField] private Vector2 _Scroll;
        [SerializeField] private int _CurrentPanel = -1;

        private Panel[] _Panels;
        private string[] _PanelNames;

        private SerializedObject _SerializedObject;

        private SerializedObject SerializedObject
            => _SerializedObject ?? (_SerializedObject = new SerializedObject(this));

         

        private void OnEnable()
        {
            titleContent = new GUIContent(Name);
            Instance = this;

            if (_PackTextures == null)
                _PackTextures = new PackTextures();
            if (_ModifySprites == null)
                _ModifySprites = new ModifySprites();
            if (_RenameSprites == null)
                _RenameSprites = new RenameSprites();
            if (_GenerateSpriteAnimations == null)
                _GenerateSpriteAnimations = new GenerateSpriteAnimations();
            if (_RemapSpriteAnimation == null)
                _RemapSpriteAnimation = new RemapSpriteAnimation();
            if (_RemapAnimationBindings == null)
                _RemapAnimationBindings = new RemapAnimationBindings();

            _Panels = new Panel[]
            {
                _PackTextures,
                _ModifySprites,
                _RenameSprites,
                _GenerateSpriteAnimations,
                _RemapSpriteAnimation,
                _RemapAnimationBindings,
                new Settings(),
            };
            _PanelNames = new string[_Panels.Length];

            for (int i = 0; i < _Panels.Length; i++)
            {
                var panel = _Panels[i];
                panel.OnEnable(i);
                _PanelNames[i] = panel.Name;
            }

            Undo.undoRedoPerformed += Repaint;

            OnSelectionChange();
        }

         

        private void OnDisable()
        {
            Undo.undoRedoPerformed -= Repaint;

            for (int i = 0; i < _Panels.Length; i++)
                _Panels[i].OnDisable();
        }

         

        private void OnSelectionChange()
        {
            for (int i = 0; i < _Panels.Length; i++)
                _Panels[i].OnSelectionChanged();

            Repaint();
        }

         

        private void OnGUI()
        {
            EditorGUIUtility.labelWidth = Mathf.Min(EditorGUIUtility.labelWidth, position.width * 0.5f);

            _Scroll = GUILayout.BeginScrollView(_Scroll);
            GUILayout.BeginVertical();
            GUILayout.EndVertical();
            for (int i = 0; i < _Panels.Length; i++)
                _Panels[i].DoGUI();
            GUILayout.EndScrollView();
        }

         

        private static new void Repaint() => ((EditorWindow)Instance).Repaint();

        private static void RecordUndo() => Undo.RecordObject(Instance, Name);

         

        private static void BeginChangeCheck() => EditorGUI.BeginChangeCheck();

        private static bool EndChangeCheck()
        {
            if (EditorGUI.EndChangeCheck())
            {
                RecordUndo();
                return true;
            }
            else return false;
        }

        private static bool EndChangeCheck<T>(ref T field, T value)
        {
            if (EndChangeCheck())
            {
                field = value;
                return true;
            }
            else return false;
        }

         

        private static ReorderableList CreateReorderableList<T>(List<T> list, string name,
            ReorderableList.ElementCallbackDelegate drawElementCallback, bool showFooter = false)
        {
            var reorderableList = new ReorderableList(list, typeof(T))
            {
                drawHeaderCallback = (area) => GUI.Label(area, name),
                drawElementCallback = drawElementCallback,
                elementHeight = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing,
            };

            if (!showFooter)
            {
                reorderableList.footerHeight = 0;
                reorderableList.displayAdd = false;
                reorderableList.displayRemove = false;
            }

            return reorderableList;
        }

         

        private static ReorderableList CreateReorderableObjectList<T>(List<T> objects, string name, bool showFooter = false) where T : Object
        {
            var reorderableList = CreateReorderableList(objects, name, (area, index, isActive, isFocused) =>
            {
                area.y = Mathf.Ceil(area.y + EditorGUIUtility.standardVerticalSpacing * 0.5f);
                area.height = EditorGUIUtility.singleLineHeight;

                BeginChangeCheck();
                var obj = (T)EditorGUI.ObjectField(area, objects[index], typeof(T), false);
                if (EndChangeCheck())
                {
                    objects[index] = obj;
                }
            }, showFooter);

            if (showFooter)
            {
                reorderableList.onAddCallback = (list) => list.list.Add(null);
            }

            return reorderableList;
        }

         

        private static ReorderableList CreateReorderableStringList(List<string> strings, string name,
            Func<Rect, int, string> doElementGUI)
        {
            return CreateReorderableList(strings, name, (area, index, isActive, isFocused) =>
            {
                area.y = Mathf.Ceil(area.y + EditorGUIUtility.standardVerticalSpacing * 0.5f);
                area.height = EditorGUIUtility.singleLineHeight;

                BeginChangeCheck();
                var str = doElementGUI(area, index);
                if (EndChangeCheck())
                {
                    strings[index] = str;
                }
            });
        }

        private static ReorderableList CreateReorderableStringList(List<string> strings, string name)
        {
            return CreateReorderableStringList(strings, name, (area, index) =>
            {
                return EditorGUI.TextField(area, strings[index]);
            });
        }

         

        public static Sprite[] LoadAllSpritesInTexture(Texture2D texture)
            => LoadAllSpritesAtPath(AssetDatabase.GetAssetPath(texture));

        public static Sprite[] LoadAllSpritesAtPath(string path)
        {
            var assets = AssetDatabase.LoadAllAssetsAtPath(path);
            var sprites = new List<Sprite>();
            for (int j = 0; j < assets.Length; j++)
            {
                if (assets[j] is Sprite sprite)
                    sprites.Add(sprite);
            }
            return sprites.ToArray();
        }

         

        public static int NaturalCompare(Object a, Object b) => EditorUtility.NaturalCompare(a.name, b.name);

         

        [MenuItem(Strings.EasyAnimatorToolsMenuPath)]
        public static void Open() => GetWindow<EasyAnimatorToolsWindow>();

        public static void Open(Type panel)
        {
            var window = GetWindow<EasyAnimatorToolsWindow>();
            for (int i = 0; i < window._Panels.Length; i++)
            {
                if (window._Panels[i].GetType() == panel)
                {
                    window._CurrentPanel = i;
                    return;
                }
            }
        }

         
    }
}

#endif

