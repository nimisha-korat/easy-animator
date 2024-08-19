

#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace EasyAnimator.Editor
{
    
    public sealed class EasyAnimatorLayerDrawer : EasyAnimatorNodeDrawer<EasyAnimatorLayer>
    {
         

        public readonly List<EasyAnimatorState> ActiveStates = new List<EasyAnimatorState>();

        public readonly List<EasyAnimatorState> InactiveStates = new List<EasyAnimatorState>();

         

        protected override GUIStyle RegionStyle => GUI.skin.box;

         
        #region Gathering
         

        internal static void GatherLayerEditors(EasyAnimatorPlayable EasyAnimator, List<EasyAnimatorLayerDrawer> editors, out int count)
        {
            count = EasyAnimator.Layers.Count;
            for (int i = 0; i < count; i++)
            {
                EasyAnimatorLayerDrawer editor;
                if (editors.Count <= i)
                {
                    editor = new EasyAnimatorLayerDrawer();
                    editors.Add(editor);
                }
                else
                {
                    editor = editors[i];
                }

                editor.GatherStates(EasyAnimator.Layers._Layers[i]);
            }
        }

         

        private void GatherStates(EasyAnimatorLayer layer)
        {
            Target = layer;

            ActiveStates.Clear();
            InactiveStates.Clear();

            foreach (var state in layer)
            {
                if (EasyAnimatorPlayableDrawer.HideInactiveStates && state.Weight == 0)
                    continue;

                if (!EasyAnimatorPlayableDrawer.SeparateActiveFromInactiveStates || state.Weight != 0)
                {
                    ActiveStates.Add(state);
                }
                else
                {
                    InactiveStates.Add(state);
                }
            }

            SortAndGatherKeys(ActiveStates);
            SortAndGatherKeys(InactiveStates);
        }

         

        private static void SortAndGatherKeys(List<EasyAnimatorState> states)
        {
            var count = states.Count;
            if (count == 0)
                return;

            if (EasyAnimatorPlayableDrawer.SortStatesByName)
            {
                states.Sort((x, y) =>
                {
                    if (x.MainObject == null)
                        return y.MainObject == null ? 0 : 1;
                    else if (y.MainObject == null)
                        return -1;

                    return x.MainObject.name.CompareTo(y.MainObject.name);
                });
            }

            // Sort any states that use another state as their key to be right after the key.
            for (int i = 0; i < count; i++)
            {
                var state = states[i];
                var key = state.Key;

                var keyState = key as EasyAnimatorState;
                if (keyState == null)
                    continue;

                var keyStateIndex = states.IndexOf(keyState);
                if (keyStateIndex < 0 || keyStateIndex + 1 == i)
                    continue;

                states.RemoveAt(i);

                if (keyStateIndex < i)
                    keyStateIndex++;

                states.Insert(keyStateIndex, state);

                i--;
            }
        }

         
        #endregion
         

        protected override void DoLabelGUI(Rect area)
        {
            var label = Target.IsAdditive ? "Additive" : "Override";
            if (Target._Mask != null)
                label = $"{label} ({Target._Mask.name})";

            area.xMin += FoldoutIndent;

            EasyAnimatorGUI.DoWeightLabel(ref area, Target.Weight);

            EditorGUIUtility.labelWidth -= FoldoutIndent;
            EditorGUI.LabelField(area, Target.ToString(), label);
            EditorGUIUtility.labelWidth += FoldoutIndent;
        }

         

        const float FoldoutIndent = 12;

        protected override void DoFoldoutGUI(Rect area)
        {
            var hierarchyMode = EditorGUIUtility.hierarchyMode;
            EditorGUIUtility.hierarchyMode = true;

            area.xMin += FoldoutIndent;
            IsExpanded = EditorGUI.Foldout(area, IsExpanded, GUIContent.none, true);

            EditorGUIUtility.hierarchyMode = hierarchyMode;
        }

         

        protected override void DoDetailsGUI()
        {
            if (IsExpanded)
            {
                EditorGUI.indentLevel++;
                GUILayout.BeginHorizontal();
                GUILayout.Space(FoldoutIndent);
                GUILayout.BeginVertical();

                DoLayerDetailsGUI();
                DoNodeDetailsGUI();

                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
                EditorGUI.indentLevel--;
            }

            DoStatesGUI();
        }

         

        private void DoLayerDetailsGUI()
        {
            var area = EasyAnimatorGUI.LayoutSingleLineRect(EasyAnimatorGUI.SpacingMode.Before);
            area = EditorGUI.IndentedRect(area);

            var labelWidth = EditorGUIUtility.labelWidth;
            var indentLevel = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            var additiveLabel = EasyAnimatorGUI.GetNarrowText("Is Additive");

            var additiveWidth = GUI.skin.toggle.CalculateWidth(additiveLabel);
            var maskRect = EasyAnimatorGUI.StealFromRight(ref area, area.width - additiveWidth);

            // Additive.
            EditorGUIUtility.labelWidth = EasyAnimatorGUI.CalculateLabelWidth(additiveLabel);
            Target.IsAdditive = EditorGUI.Toggle(area, additiveLabel, Target.IsAdditive);

            // Mask.
            using (ObjectPool.Disposable.AcquireContent(out var label, "Mask"))
            {
                EditorGUIUtility.labelWidth = EasyAnimatorGUI.CalculateLabelWidth(label.text);
                EditorGUI.BeginChangeCheck();
                Target._Mask = (AvatarMask)EditorGUI.ObjectField(maskRect, label, Target._Mask, typeof(AvatarMask), false);
                if (EditorGUI.EndChangeCheck())
                    Target.SetMask(Target._Mask);
            }

            EditorGUI.indentLevel = indentLevel;
            EditorGUIUtility.labelWidth = labelWidth;
        }

         

        private void DoStatesGUI()
        {
            if (EasyAnimatorPlayableDrawer.HideInactiveStates)
            {
                DoStatesGUI("Active States", ActiveStates);
            }
            else if (EasyAnimatorPlayableDrawer.SeparateActiveFromInactiveStates)
            {
                DoStatesGUI("Active States", ActiveStates);
                DoStatesGUI("Inactive States", InactiveStates);
            }
            else
            {
                DoStatesGUI("States", ActiveStates);
            }

            if (Target.Index == 0 &&
                Target.Weight != 0 &&
                !Target.IsAdditive &&
                !Mathf.Approximately(Target.GetTotalWeight(), 1))
            {
                EditorGUILayout.HelpBox(
                    "The total Weight of all states in this layer does not equal 1, which will likely give undesirable results." +
                    " Click here for more information.",
                    MessageType.Warning);

                if (EasyAnimatorGUI.TryUseClickEventInLastRect())
                    EditorUtility.OpenWithDefaultApp(Strings.DocsURLs.Fading);
            }
        }

         

        private void DoStatesGUI(string label, List<EasyAnimatorState> states)
        {
            var area = EasyAnimatorGUI.LayoutSingleLineRect();

            const string Label = "Weight";
            var width = EasyAnimatorGUI.CalculateLabelWidth(Label);
            GUI.Label(EasyAnimatorGUI.StealFromRight(ref area, width), Label);

            EditorGUI.LabelField(area, label, states.Count.ToString());

            EditorGUI.indentLevel++;
            for (int i = 0; i < states.Count; i++)
            {
                DoStateGUI(states[i]);
            }
            EditorGUI.indentLevel--;
        }

         

        private readonly Dictionary<EasyAnimatorState, IEasyAnimatorNodeDrawer>
            StateInspectors = new Dictionary<EasyAnimatorState, IEasyAnimatorNodeDrawer>();

        private void DoStateGUI(EasyAnimatorState state)
        {
            if (!StateInspectors.TryGetValue(state, out var inspector))
            {
                inspector = state.CreateDrawer();
                StateInspectors.Add(state, inspector);
            }

            inspector.DoGUI();
            DoChildStatesGUI(state);
        }

         

        private void DoChildStatesGUI(EasyAnimatorState state)
        {
            EditorGUI.indentLevel++;

            foreach (var child in state)
            {
                if (child == null)
                    continue;

                DoStateGUI(child);
            }

            EditorGUI.indentLevel--;
        }

         

        public override void DoGUI()
        {
            if (!Target.IsValid)
                return;

            base.DoGUI();

            var area = GUILayoutUtility.GetLastRect();
            HandleDragAndDropAnimations(area, Target.Root.Component, Target.Index);
        }

        public static void HandleDragAndDropAnimations(Rect dropArea, IEasyAnimatorComponent target, int layerIndex)
        {
            if (target == null)
                return;

            EasyAnimatorGUI.HandleDragAndDropAnimations(dropArea, (clip) =>
            {
                target.Playable.Layers[layerIndex].GetOrCreateState(clip);
            });
        }

         
        #region Context Menu
         

        protected override void PopulateContextMenu(GenericMenu menu)
        {
            menu.AddDisabledItem(new GUIContent($"{DetailsPrefix}{nameof(Target.CurrentState)}: {Target.CurrentState}"));
            menu.AddDisabledItem(new GUIContent($"{DetailsPrefix}{nameof(Target.CommandCount)}: {Target.CommandCount}"));

            menu.AddFunction("Stop",
                HasAnyStates((state) => state.IsPlaying || state.Weight != 0),
                () => Target.Stop());

            EasyAnimatorEditorUtilities.AddFadeFunction(menu, "Fade In",
                Target.Index > 0 && Target.Weight != 1, Target,
                (duration) => Target.StartFade(1, duration));
            EasyAnimatorEditorUtilities.AddFadeFunction(menu, "Fade Out",
                Target.Index > 0 && Target.Weight != 0, Target,
                (duration) => Target.StartFade(0, duration));

            EasyAnimatorEditorUtilities.AddContextMenuIK(menu, Target);

            menu.AddSeparator("");

            menu.AddFunction("Destroy States",
                ActiveStates.Count > 0 || InactiveStates.Count > 0,
                () => Target.DestroyStates());

            EasyAnimatorPlayableDrawer.AddRootFunctions(menu, Target.Root);

            menu.AddSeparator("");

            EasyAnimatorPlayableDrawer.AddDisplayOptions(menu);

            EasyAnimatorEditorUtilities.AddDocumentationLink(menu, "Layer Documentation", Strings.DocsURLs.Layers);

            menu.ShowAsContext();
        }

         

        private bool HasAnyStates(Func<EasyAnimatorState, bool> condition)
        {
            foreach (var state in Target)
            {
                if (condition(state))
                    return true;
            }

            return false;
        }

         
        #endregion
         
    }
}

#endif

