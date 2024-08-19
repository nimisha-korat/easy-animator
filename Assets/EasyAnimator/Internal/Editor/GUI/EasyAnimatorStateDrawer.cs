
#if UNITY_EDITOR

using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
using static EasyAnimator.Editor.EasyAnimatorPlayableDrawer;

namespace EasyAnimator.Editor
{
   
    public class EasyAnimatorStateDrawer<T> : EasyAnimatorNodeDrawer<T> where T : EasyAnimatorState
    {
         

        public EasyAnimatorStateDrawer(T target) => Target = target;

         

        protected override GUIStyle RegionStyle => null;

         

        private bool IsAssetUsedAsKey =>
            Target.DebugName == null &&
            (Target.Key == null || ReferenceEquals(Target.Key, Target.MainObject));

         

        protected override bool AutoNormalizeSiblingWeights => AutoNormalizeWeights;

         

        protected override void DoLabelGUI(Rect area)
        {
            string label;
            if (Target.DebugName != null)
            {
                label = Target.DebugName;
            }
            else if (IsAssetUsedAsKey)
            {
                label = "";
            }
            else
            {
                var key = Target.Key;
                if (key is string str)
                    label = $"\"{str}\"";
                else
                    label = key.ToString();
            }

            HandleLabelClick(area);

            EasyAnimatorGUI.DoWeightLabel(ref area, Target.Weight);

            AnimationBindings.DoBindingMatchGUI(ref area, Target);

            var mainObject = Target.MainObject;
            if (!(mainObject is null))
            {
                EditorGUI.BeginChangeCheck();

                mainObject = EditorGUI.ObjectField(area, label, mainObject, typeof(Object), false);

                if (EditorGUI.EndChangeCheck())
                    Target.MainObject = mainObject;
            }
            else if (Target.DebugName != null)
            {
                EditorGUI.LabelField(area, Target.DebugName);
            }
            else
            {
                EditorGUI.LabelField(area, label, Target.ToString());
            }

            // Highlight a section of the label based on the time like a loading bar.
            area.width -= 18;// Remove the area for the Object Picker icon to line the bar up with the field.
            DoTimeHighlightBarGUI(area, Target.IsPlaying, Target.EffectiveWeight, Target.Time, Target.Length, Target.IsLooping);
        }

         

        public static void DoTimeHighlightBarGUI(Rect area, bool isPlaying, float weight, float time, float length, bool isLooping)
        {
            var color = GUI.color;

            if (ScaleTimeBarByWeight)
            {
                var height = area.height;
                area.height = 1 + (area.height - 1) * Mathf.Clamp01(weight);
                area.y += height - area.height;
            }

            // Green = Playing, Yelow = Paused.
            GUI.color = isPlaying ? new Color(0.15f, 0.7f, 0.15f, 0.35f) : new Color(0.7f, 0.7f, 0.15f, 0.35f);

            area = EditorGUI.IndentedRect(area);

            var wrappedTime = GetWrappedTime(time, length, isLooping);
            if (length > 0)
                area.width *= Mathf.Clamp01(wrappedTime / length);

            GUI.DrawTexture(area, Texture2D.whiteTexture);

            GUI.color = color;
        }

         

        private void HandleLabelClick(Rect area)
        {
            var currentEvent = Event.current;
            if (currentEvent.type != EventType.MouseUp ||
                !currentEvent.control ||
                !area.Contains(currentEvent.mousePosition))
                return;

            currentEvent.Use();

            Target.Root.UnpauseGraph();
            var fadeDuration = Target.CalculateEditorFadeDuration(EasyAnimatorPlayable.DefaultFadeDuration);
            Target.Root.Play(Target, fadeDuration);
        }

         

        protected override void DoFoldoutGUI(Rect area)
        {
            float foldoutWidth;
            if (IsAssetUsedAsKey)
            {
                foldoutWidth = EditorGUI.indentLevel * EasyAnimatorGUI.IndentSize;
            }
            else
            {
                foldoutWidth = EditorGUIUtility.labelWidth;
            }

            area.xMin -= 2;
            area.width = foldoutWidth;

            var hierarchyMode = EditorGUIUtility.hierarchyMode;
            EditorGUIUtility.hierarchyMode = true;

            IsExpanded = EditorGUI.Foldout(area, IsExpanded, GUIContent.none, true);

            EditorGUIUtility.hierarchyMode = hierarchyMode;
        }

         

        private float GetWrappedTime(out float length) => GetWrappedTime(Target.Time, length = Target.Length, Target.IsLooping);

        private static float GetWrappedTime(float time, float length, bool isLooping)
        {
            var wrappedTime = time;

            if (isLooping)
            {
                wrappedTime = EasyAnimatorUtilities.Wrap(wrappedTime, length);
                if (wrappedTime == 0 && time != 0)
                    wrappedTime = length;
            }

            return wrappedTime;
        }

         

        protected override void DoDetailsGUI()
        {
            if (!IsExpanded)
                return;

            EditorGUI.indentLevel++;
            DoTimeSliderGUI();
            DoNodeDetailsGUI();
            DoOnEndGUI();
            EditorGUI.indentLevel--;
        }

         

        private void DoTimeSliderGUI()
        {
            if (Target.Length <= 0)
                return;

            var time = GetWrappedTime(out var length);

            if (length == 0)
                return;

            var area = EasyAnimatorGUI.LayoutSingleLineRect(EasyAnimatorGUI.SpacingMode.Before);

            var normalized = DoNormalizedTimeToggle(ref area);

            string label;
            float max;
            if (normalized)
            {
                label = "Normalized Time";
                time /= length;
                max = 1;
            }
            else
            {
                label = "Time";
                max = length;
            }

            DoLoopCounterGUI(ref area, length);

            EditorGUI.BeginChangeCheck();
            label = EasyAnimatorGUI.BeginTightLabel(label);
            time = EditorGUI.Slider(area, label, time, 0, max);
            EasyAnimatorGUI.EndTightLabel();
            if (EasyAnimatorGUI.TryUseClickEvent(area, 2))
                time = 0;
            if (EditorGUI.EndChangeCheck())
            {
                if (normalized)
                    Target.NormalizedTime = time;
                else
                    Target.Time = time;
            }
        }

         

        private bool DoNormalizedTimeToggle(ref Rect area)
        {
            using (ObjectPool.Disposable.AcquireContent(out var label, "N"))
            {
                var style = EasyAnimatorGUI.MiniButton;

                var width = style.CalculateWidth(label);
                var toggleArea = EasyAnimatorGUI.StealFromRight(ref area, width);

                UseNormalizedTimeSliders.Value = GUI.Toggle(toggleArea, UseNormalizedTimeSliders, label, style);
            }

            return UseNormalizedTimeSliders;
        }

         

        private static ConversionCache<int, string> _LoopCounterCache;

        private void DoLoopCounterGUI(ref Rect area, float length)
        {
            if (_LoopCounterCache == null)
                _LoopCounterCache = new ConversionCache<int, string>((x) => "x" + x);

            string label;
            var normalizedTime = Target.Time / length;
            if (float.IsNaN(normalizedTime))
            {
                label = "NaN";
            }
            else
            {
                var loops = Mathf.FloorToInt(Target.Time / length);
                label = _LoopCounterCache.Convert(loops);
            }

            var width = EasyAnimatorGUI.CalculateLabelWidth(label);

            var labelArea = EasyAnimatorGUI.StealFromRight(ref area, width);

            GUI.Label(labelArea, label);
        }

         

        private void DoOnEndGUI()
        {
            if (!Target.HasEvents)
                return;

            var events = Target.Events;
            var drawer = EventSequenceDrawer.Get(events);
            var area = GUILayoutUtility.GetRect(0, drawer.CalculateHeight(events) + EasyAnimatorGUI.StandardSpacing);
            area.yMin += EasyAnimatorGUI.StandardSpacing;

            using (ObjectPool.Disposable.AcquireContent(out var label, "Events"))
                drawer.Draw(ref area, events, label);
        }

         
        #region Context Menu
         

        protected override void PopulateContextMenu(GenericMenu menu)
        {
            AddContextMenuFunctions(menu);

            menu.AddFunction("Play",
                !Target.IsPlaying || Target.Weight != 1,
                () =>
                {
                    Target.Root.UnpauseGraph();
                    Target.Root.Play(Target);
                });

            EasyAnimatorEditorUtilities.AddFadeFunction(menu, "Cross Fade (Ctrl + Click)",
                Target.Weight != 1,
                Target, (duration) =>
                {
                    Target.Root.UnpauseGraph();
                    Target.Root.Play(Target, duration);
                });

            menu.AddSeparator("");
            menu.AddItem(new GUIContent("Destroy State"), false, () => Target.Destroy());

            menu.AddSeparator("");

            AddDisplayOptions(menu);

            EasyAnimatorEditorUtilities.AddDocumentationLink(menu, "State Documentation", Strings.DocsURLs.States);
        }

         

        protected virtual void AddContextMenuFunctions(GenericMenu menu)
        {
            menu.AddDisabledItem(new GUIContent($"{DetailsPrefix}{nameof(EasyAnimatorState.Key)}: {Target.Key}"));

            var length = Target.Length;
            if (!float.IsNaN(length))
                menu.AddDisabledItem(new GUIContent($"{DetailsPrefix}{nameof(EasyAnimatorState.Length)}: {length}"));

            menu.AddDisabledItem(new GUIContent($"{DetailsPrefix}Playable Path: {Target.GetPath()}"));

            var mainAsset = Target.MainObject;
            if (mainAsset != null)
            {
                var assetPath = AssetDatabase.GetAssetPath(mainAsset);
                if (assetPath != null)
                    menu.AddDisabledItem(new GUIContent($"{DetailsPrefix}Asset Path: {assetPath.Replace("/", "->")}"));
            }

            if (Target.HasEvents)
            {
                var events = Target.Events;
                for (int i = 0; i < events.Count; i++)
                {
                    var index = i;
                    AddEventFunctions(menu, "Event " + index, events[index],
                        () => events.SetCallback(index, EasyAnimatorEvent.DummyCallback),
                        () => events.Remove(index));
                }

                AddEventFunctions(menu, "End Event", events.endEvent,
                    () => events.endEvent = new EasyAnimatorEvent(float.NaN, null), null);
            }
        }

         

        private void AddEventFunctions(GenericMenu menu, string name, EasyAnimatorEvent EasyAnimatorEvent,
            GenericMenu.MenuFunction clearEvent, GenericMenu.MenuFunction removeEvent)
        {
            name = $"Events/{name}/";

            menu.AddDisabledItem(new GUIContent($"{name}{nameof(EasyAnimatorState.NormalizedTime)}: {EasyAnimatorEvent.normalizedTime}"));

            bool canInvoke;
            if (EasyAnimatorEvent.callback == null)
            {
                menu.AddDisabledItem(new GUIContent(name + "Callback: null"));
                canInvoke = false;
            }
            else if (EasyAnimatorEvent.callback == EasyAnimatorEvent.DummyCallback)
            {
                menu.AddDisabledItem(new GUIContent(name + "Callback: Dummy"));
                canInvoke = false;
            }
            else
            {
                var label = name +
                    (EasyAnimatorEvent.callback.Target != null ? ("Target: " + EasyAnimatorEvent.callback.Target) : "Target: null");

                var targetObject = EasyAnimatorEvent.callback.Target as Object;
                menu.AddFunction(label,
                    targetObject != null,
                    () => Selection.activeObject = targetObject);

                menu.AddDisabledItem(new GUIContent(
                    $"{name}Declaring Type: {EasyAnimatorEvent.callback.Method.DeclaringType.GetNameCS()}"));

                menu.AddDisabledItem(new GUIContent(
                    $"{name}Method: {EasyAnimatorEvent.callback.Method}"));

                canInvoke = true;
            }

            if (clearEvent != null)
                menu.AddFunction(name + "Clear", canInvoke || !float.IsNaN(EasyAnimatorEvent.normalizedTime), clearEvent);

            if (removeEvent != null)
                menu.AddFunction(name + "Remove", true, removeEvent);

            menu.AddFunction(name + "Invoke", canInvoke, () => EasyAnimatorEvent.Invoke(Target));
        }

         
        #endregion
         
    }
}

#endif

