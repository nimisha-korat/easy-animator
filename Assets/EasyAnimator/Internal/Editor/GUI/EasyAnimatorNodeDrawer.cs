

#if UNITY_EDITOR

using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using Object = UnityEngine.Object;

namespace EasyAnimator.Editor
{
   
    public interface IEasyAnimatorNodeDrawer
    {
        void DoGUI();
    }

     

   
    public abstract class EasyAnimatorNodeDrawer<T> : IEasyAnimatorNodeDrawer where T : EasyAnimatorNode
    {
         

        public T Target { get; protected set; }

        public ref bool IsExpanded => ref Target._IsInspectorExpanded;

         

        protected abstract GUIStyle RegionStyle { get; }

         

        public virtual void DoGUI()
        {
            if (!Target.IsValid)
                return;

            EasyAnimatorGUI.BeginVerticalBox(RegionStyle);
            {
                DoHeaderGUI();
                DoDetailsGUI();
            }
            EasyAnimatorGUI.EndVerticalBox(RegionStyle);

            CheckContextMenu(GUILayoutUtility.GetLastRect());

        }

         

        protected virtual void DoHeaderGUI()
        {
            var area = EasyAnimatorGUI.LayoutSingleLineRect(EasyAnimatorGUI.SpacingMode.Before);
            DoLabelGUI(area);
            DoFoldoutGUI(area);
        }

        protected abstract void DoLabelGUI(Rect area);

        protected abstract void DoFoldoutGUI(Rect area);

        protected abstract void DoDetailsGUI();

         

        protected void DoNodeDetailsGUI()
        {
            var area = EasyAnimatorGUI.LayoutSingleLineRect(EasyAnimatorGUI.SpacingMode.Before);
            area.xMin += EditorGUI.indentLevel * EasyAnimatorGUI.IndentSize;
            var xMin = area.xMin;
            var xMax = area.xMax;

            var labelWidth = EditorGUIUtility.labelWidth;
            var indentLevel = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            // Is Playing.
            var state = Target as EasyAnimatorState;
            if (state != null)
            {
                var label = EasyAnimatorGUI.BeginTightLabel("Is Playing");
                area.width = EditorGUIUtility.labelWidth + 16;
                state.IsPlaying = EditorGUI.Toggle(area, label, state.IsPlaying);
                EasyAnimatorGUI.EndTightLabel();

                area.x += area.width;
                area.xMax = xMax;
            }

            EasyAnimatorGUI.SplitHorizontally(area, "Speed", "Weight",
                out var speedWidth, out var weightWidth, out var speedRect, out var weightRect);

            // Speed.
            EditorGUIUtility.labelWidth = speedWidth;
            EditorGUI.BeginChangeCheck();
            var speed = EditorGUI.FloatField(speedRect, "Speed", Target.Speed);
            if (EditorGUI.EndChangeCheck())
                Target.Speed = speed;
            if (EasyAnimatorGUI.TryUseClickEvent(speedRect, 2))
                Target.Speed = Target.Speed != 1 ? 1 : 0;

            // Weight.
            EditorGUIUtility.labelWidth = weightWidth;
            EditorGUI.BeginChangeCheck();
            var weight = EditorGUI.FloatField(weightRect, "Weight", Target.Weight);
            if (EditorGUI.EndChangeCheck())
                SetWeight(Mathf.Max(weight, 0));
            if (EasyAnimatorGUI.TryUseClickEvent(weightRect, 2))
                SetWeight(Target.Weight != 1 ? 1 : 0);

            // Not really sure why this is necessary.
            // It allows the dummy ID added when the Real Speed is hidden to work properly.
            GUIUtility.GetControlID(FocusType.Passive);

            // Real Speed (Mixer Synchronization changes the internal Playable Speed without setting the State Speed).
            speed = (float)Target._Playable.GetSpeed();
            if (Target.Speed != speed)
            {
                using (new EditorGUI.DisabledScope(true))
                {
                    area = EasyAnimatorGUI.LayoutSingleLineRect(EasyAnimatorGUI.SpacingMode.Before);
                    area.xMin = xMin;

                    var label = EasyAnimatorGUI.BeginTightLabel("Real Speed");
                    EditorGUIUtility.labelWidth = EasyAnimatorGUI.CalculateLabelWidth(label);
                    EditorGUI.FloatField(area, label, speed);
                    EasyAnimatorGUI.EndTightLabel();
                }
            }
            else// Add a dummy ID so that subsequent IDs don't change when the Real Speed appears or disappears.
            {
                GUIUtility.GetControlID(FocusType.Passive);
            }

            EditorGUI.indentLevel = indentLevel;
            EditorGUIUtility.labelWidth = labelWidth;

            DoFadeDetailsGUI();
        }

         

        protected virtual bool AutoNormalizeSiblingWeights => false;

        private void SetWeight(float weight)
        {
            if (weight < 0 ||
                weight > 1 ||
                Mathf.Approximately(Target.Weight, 1) ||
                !AutoNormalizeSiblingWeights)
                goto JustSetWeight;

            var parent = Target.Parent;
            if (parent == null)
                goto JustSetWeight;

            var totalWeight = 0f;
            var siblingCount = parent.ChildCount;
            for (int i = 0; i < siblingCount; i++)
            {
                var sibling = parent.GetChild(i);
                if (sibling.IsValid())
                    totalWeight += sibling.Weight;
            }

            // If the weights weren't previously normalized, don't normalize them now.
            if (!Mathf.Approximately(totalWeight, 1))
                goto JustSetWeight;

            var siblingWeightMultiplier = (totalWeight - weight) / (totalWeight - Target.Weight);

            for (int i = 0; i < siblingCount; i++)
            {
                var sibling = parent.GetChild(i);
                if (sibling != Target && sibling.IsValid())
                    sibling.Weight *= siblingWeightMultiplier;
            }

            JustSetWeight:
            Target.Weight = weight;
        }

         

        private void DoFadeDetailsGUI()
        {
            var area = EasyAnimatorGUI.LayoutSingleLineRect(EasyAnimatorGUI.SpacingMode.Before);
            area = EditorGUI.IndentedRect(area);

            var speedLabel = EasyAnimatorGUI.GetNarrowText("Fade Speed");
            var targetLabel = EasyAnimatorGUI.GetNarrowText("Target Weight");

            EasyAnimatorGUI.SplitHorizontally(area, speedLabel, targetLabel,
                out var speedWidth, out var weightWidth, out var speedRect, out var weightRect);

            var labelWidth = EditorGUIUtility.labelWidth;
            var indentLevel = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            EditorGUI.BeginChangeCheck();

            // Fade Speed.
            EditorGUIUtility.labelWidth = speedWidth;
            Target.FadeSpeed = EditorGUI.DelayedFloatField(speedRect, speedLabel, Target.FadeSpeed);
            if (EasyAnimatorGUI.TryUseClickEvent(speedRect, 2))
            {
                Target.FadeSpeed = Target.FadeSpeed != 0 || EasyAnimatorPlayable.DefaultFadeDuration == 0 ?
                    0 :
                    Math.Abs(Target.Weight - Target.TargetWeight) / EasyAnimatorPlayable.DefaultFadeDuration;
            }

            // Target Weight.
            EditorGUIUtility.labelWidth = weightWidth;
            Target.TargetWeight = Mathf.Max(0, EditorGUI.FloatField(weightRect, targetLabel, Target.TargetWeight));
            if (EasyAnimatorGUI.TryUseClickEvent(weightRect, 2))
            {
                if (Target.TargetWeight != Target.Weight)
                    Target.TargetWeight = Target.Weight;
                else if (Target.TargetWeight != 1)
                    Target.TargetWeight = 1;
                else
                    Target.TargetWeight = 0;
            }

            if (EditorGUI.EndChangeCheck() && Target.FadeSpeed != 0)
                Target.StartFade(Target.TargetWeight, 1 / Target.FadeSpeed);

            EditorGUI.indentLevel = indentLevel;
            EditorGUIUtility.labelWidth = labelWidth;
        }

         
        #region Context Menu
         

        protected const string DetailsPrefix = "Details/";

        protected void CheckContextMenu(Rect clickArea)
        {
            if (!EasyAnimatorGUI.TryUseClickEvent(clickArea, 1))
                return;

            var menu = new GenericMenu();

            menu.AddDisabledItem(new GUIContent(Target.ToString()));

            PopulateContextMenu(menu);

            menu.AddItem(new GUIContent(DetailsPrefix + "Log Details"), false,
                () => Debug.Log(Target.GetDescription(), Target.Root?.Component as Object));

            menu.AddItem(new GUIContent(DetailsPrefix + "Log Details Of Everything"), false,
                () => Debug.Log(Target.Root.GetDescription(), Target.Root?.Component as Object));
            EasyAnimatorPlayableDrawer.AddPlayableGraphVisualizerFunction(menu, DetailsPrefix, Target.Root._Graph);

            menu.ShowAsContext();
        }

        protected abstract void PopulateContextMenu(GenericMenu menu);

         
        #endregion
         
    }
}

#endif

