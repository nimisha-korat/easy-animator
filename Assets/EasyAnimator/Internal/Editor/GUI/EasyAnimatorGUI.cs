
#if UNITY_EDITOR

using System;
using System.Collections;
using UnityEditor;
using UnityEngine;

namespace EasyAnimator.Editor
{
 
    public static class EasyAnimatorGUI
    {
         
        #region Standard Values
         

        public static readonly Color
            WarningFieldColor = new Color(1, 0.9f, 0.6f);

        public static readonly Color
            ErrorFieldColor = new Color(1, 0.6f, 0.6f);

         

        public static readonly GUILayoutOption[]
            DontExpandWidth = { GUILayout.ExpandWidth(false) };

         

        public static float LineHeight => EditorGUIUtility.singleLineHeight;

         

        public static float StandardSpacing => EditorGUIUtility.standardVerticalSpacing;

         

        private static float _IndentSize = -1;

        public static float IndentSize
        {
            get
            {
                if (_IndentSize < 0)
                {
                    var indentLevel = EditorGUI.indentLevel;
                    EditorGUI.indentLevel = 1;
                    _IndentSize = EditorGUI.IndentedRect(new Rect()).x;
                    EditorGUI.indentLevel = indentLevel;
                }

                return _IndentSize;
            }
        }

         

        private static float _ToggleWidth = -1;

        public static float ToggleWidth
        {
            get
            {
                if (_ToggleWidth == -1)
                    _ToggleWidth = GUI.skin.toggle.CalculateWidth(GUIContent.none);
                return _ToggleWidth;
            }
        }

         

        public static Color TextColor => GUI.skin.label.normal.textColor;

         

        private static GUIStyle _MiniButton;

        public static GUIStyle MiniButton
        {
            get
            {
                if (_MiniButton == null)
                {
                    _MiniButton = new GUIStyle(EditorStyles.miniButton)
                    {
                        margin = new RectOffset(0, 0, 2, 0),
                        padding = new RectOffset(2, 3, 2, 2),
                        alignment = TextAnchor.MiddleCenter,
                        fixedHeight = LineHeight,
                        fixedWidth = LineHeight - 1
                    };
                }

                return _MiniButton;
            }
        }

         
        #endregion
         
        #region Layout
         

        public static void RepaintEverything() => UnityEditorInternal.InternalEditorUtility.RepaintAllViews();

         

        public enum SpacingMode
        {
            None,

            Before,

            After,

            BeforeAndAfter
        }

        public static Rect LayoutSingleLineRect(SpacingMode spacing = SpacingMode.None)
        {
            Rect rect;
            switch (spacing)
            {
                case SpacingMode.None:
                    return GUILayoutUtility.GetRect(0, LineHeight);

                case SpacingMode.Before:
                    rect = GUILayoutUtility.GetRect(0, LineHeight + StandardSpacing);
                    rect.yMin += StandardSpacing;
                    return rect;

                case SpacingMode.After:
                    rect = GUILayoutUtility.GetRect(0, LineHeight + StandardSpacing);
                    rect.yMax -= StandardSpacing;
                    return rect;

                case SpacingMode.BeforeAndAfter:
                    rect = GUILayoutUtility.GetRect(0, LineHeight + StandardSpacing * 2);
                    rect.yMin += StandardSpacing;
                    rect.yMax -= StandardSpacing;
                    return rect;

                default:
                    throw new ArgumentException($"Unknown {nameof(StandardSpacing)}: " + spacing, nameof(spacing));
            }
        }

         

        public static void NextVerticalArea(ref Rect area)
        {
            if (area.height > 0)
                area.y += area.height + StandardSpacing;
        }

         

        public static Rect StealFromLeft(ref Rect area, float width, float padding = 0)
        {
            var newRect = new Rect(area.x, area.y, width, area.height);
            area.xMin += width + padding;
            return newRect;
        }

        public static Rect StealFromRight(ref Rect area, float width, float padding = 0)
        {
            area.width -= width + padding;
            return new Rect(area.xMax + padding, area.y, width, area.height);
        }

         

        public static void SplitHorizontally(Rect area, string label0, string label1,
             out float width0, out float width1, out Rect rect0, out Rect rect1)
        {
            width0 = CalculateLabelWidth(label0);
            width1 = CalculateLabelWidth(label1);

            const float Padding = 1;

            rect0 = rect1 = area;

            var remainingWidth = area.width - width0 - width1 - Padding;
            rect0.width = width0 + remainingWidth * 0.5f;
            rect1.xMin = rect0.xMax + Padding;
        }

         

        public static float CalculateWidth(this GUIStyle style, GUIContent content)
        {
            style.CalcMinMaxWidth(content, out _, out var width);
            return Mathf.Ceil(width);
        }

        public static float CalculateWidth(this GUIStyle style, string text)
        {
            using (ObjectPool.Disposable.AcquireContent(out var content, text, null, false))
                return style.CalculateWidth(content);
        }

         

        public static ConversionCache<string, float> CreateWidthCache(GUIStyle style)
            => new ConversionCache<string, float>((text) => style.CalculateWidth(text));

         

        private static ConversionCache<string, float> _LabelWidthCache;

        public static float CalculateLabelWidth(string text)
        {
            if (_LabelWidthCache == null)
                _LabelWidthCache = CreateWidthCache(GUI.skin.label);

            return _LabelWidthCache.Convert(text);
        }

         

        public static void BeginVerticalBox(GUIStyle style)
        {
            if (style == null)
            {
                GUILayout.BeginVertical();
                return;
            }

            GUILayout.BeginVertical(style);
            EditorGUIUtility.labelWidth -= style.padding.left;
        }

        public static void EndVerticalBox(GUIStyle style)
        {
            if (style != null)
                EditorGUIUtility.labelWidth += style.padding.left;

            GUILayout.EndVertical();
        }

         
        #endregion
         
        #region Labels
         

        private static GUIStyle _WeightLabelStyle;
        private static float _WeightLabelWidth = -1;

        public static void DoWeightLabel(ref Rect area, float weight)
        {
            var label = WeightToShortString(weight, out var isExact);

            if (_WeightLabelStyle == null)
                _WeightLabelStyle = new GUIStyle(GUI.skin.label);

            if (_WeightLabelWidth < 0)
            {
                _WeightLabelStyle.fontStyle = FontStyle.Italic;
                _WeightLabelWidth = _WeightLabelStyle.CalculateWidth("0.0");
            }

            _WeightLabelStyle.normal.textColor = Color.Lerp(Color.grey, TextColor, weight);
            _WeightLabelStyle.fontStyle = isExact ? FontStyle.Normal : FontStyle.Italic;

            var weightArea = StealFromRight(ref area, _WeightLabelWidth);

            GUI.Label(weightArea, label, _WeightLabelStyle);
        }

         

        private static ConversionCache<float, string> _ShortWeightCache;

        private static string WeightToShortString(float weight, out bool isExact)
        {
            isExact = true;

            if (weight == 0)
                return "0.0";
            if (weight == 1)
                return "1.0";

            isExact = false;

            if (weight >= -0.5f && weight < 0.05f)
                return "~0.";
            if (weight >= 0.95f && weight < 1.05f)
                return "~1.";

            if (weight <= -99.5f)
                return "-??";
            if (weight >= 999.5f)
                return "???";

            if (_ShortWeightCache == null)
                _ShortWeightCache = new ConversionCache<float, string>((value) =>
                {
                    if (value < -9.5f) return $"{value:F0}";
                    if (value < -0.5f) return $"{value:F0}.";
                    if (value < 9.5f) return $"{value:F1}";
                    if (value < 99.5f) return $"{value:F0}.";
                    return $"{value:F0}";
                });

            var rounded = weight > 0 ? Mathf.Floor(weight * 10) : Mathf.Ceil(weight * 10);
            isExact = Mathf.Approximately(weight * 10, rounded);

            return _ShortWeightCache.Convert(weight);
        }

         

        private static float _TightLabelWidth;

        public static string BeginTightLabel(string label)
        {
            _TightLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = CalculateLabelWidth(label) + EditorGUI.indentLevel * IndentSize;
            return GetNarrowText(label);
        }

        public static void EndTightLabel()
        {
            EditorGUIUtility.labelWidth = _TightLabelWidth;
        }

         

        private static ConversionCache<string, string> _NarrowTextCache;

        public static string GetNarrowText(string text)
        {
            if (EditorGUIUtility.wideMode ||
                string.IsNullOrEmpty(text))
                return text;

            if (_NarrowTextCache == null)
                _NarrowTextCache = new ConversionCache<string, string>((str) => str.Replace(" ", ""));

            return _NarrowTextCache.Convert(text);
        }

         

        public static Texture LoadIcon(string name)
        {
            var icon = (Texture)EditorGUIUtility.Load(name);
            if (icon != null)
                icon.filterMode = FilterMode.Bilinear;
            return icon;
        }

         
        #endregion
         
        #region Events
         

        public static bool TryUseClickEvent(Rect area, int button = -1)
        {
            var currentEvent = Event.current;
            if (currentEvent.type == EventType.MouseUp &&
                (button < 0 || currentEvent.button == button) &&
                area.Contains(currentEvent.mousePosition))
            {
                GUI.changed = true;
                currentEvent.Use();

                if (currentEvent.button == 2)
                    Deselect();

                return true;
            }
            else return false;
        }

        public static bool TryUseClickEventInLastRect(int button = -1)
            => TryUseClickEvent(GUILayoutUtility.GetLastRect(), button);

         

        public static void HandleDragAndDrop<T>(Rect dropArea, Func<T, bool> validate, Action<T> onDrop,
            DragAndDropVisualMode mode = DragAndDropVisualMode.Link) where T : class
        {
            if (!dropArea.Contains(Event.current.mousePosition))
                return;

            bool isDrop;
            switch (Event.current.type)
            {
                case EventType.DragUpdated:
                    isDrop = false;
                    break;

                case EventType.DragPerform:
                    isDrop = true;
                    break;

                default:
                    return;
            }

            TryDrop(DragAndDrop.objectReferences, validate, onDrop, isDrop, mode);
        }

         

        private static void TryDrop<T>(IEnumerable objects, Func<T, bool> validate, Action<T> onDrop, bool isDrop,
            DragAndDropVisualMode mode) where T : class
        {
            if (objects == null)
                return;

            var droppedAny = false;

            foreach (var obj in objects)
            {
                var t = obj as T;

                if (t != null && (validate == null || validate(t)))
                {
                    Deselect();

                    if (!isDrop)
                    {
                        DragAndDrop.visualMode = mode;
                        break;
                    }
                    else
                    {
                        onDrop(t);
                        droppedAny = true;
                    }
                }
            }

            if (droppedAny)
                GUIUtility.ExitGUI();
        }

         

        public static void HandleDragAndDropAnimations(Rect dropArea, Action<AnimationClip> onDrop,
            DragAndDropVisualMode mode = DragAndDropVisualMode.Link)
        {
            HandleDragAndDrop(dropArea, (clip) => !clip.legacy, onDrop, mode);

            HandleDragAndDrop<IAnimationClipSource>(dropArea, null, (source) =>
            {
                using (ObjectPool.Disposable.AcquireList<AnimationClip>(out var clips))
                {
                    source.GetAnimationClips(clips);
                    TryDrop(clips, (clip) => !clip.legacy, onDrop, true, mode);
                }
            }, mode);

            HandleDragAndDrop<IAnimationClipCollection>(dropArea, null, (collection) =>
            {
                using (ObjectPool.Disposable.AcquireSet<AnimationClip>(out var clips))
                {
                    collection.GatherAnimationClips(clips);
                    TryDrop(clips, (clip) => !clip.legacy, onDrop, true, mode);
                }
            }, mode);
        }

         

        public static void Deselect() => GUIUtility.keyboardControl = 0;

         
        #endregion
         
    }
}

#endif

