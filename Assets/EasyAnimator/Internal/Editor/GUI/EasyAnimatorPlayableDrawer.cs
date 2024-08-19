
#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Playables;
using Object = UnityEngine.Object;

namespace EasyAnimator.Editor
{
    
    public sealed class EasyAnimatorPlayableDrawer
    {
         

        private readonly List<EasyAnimatorLayerDrawer>
            LayerInfos = new List<EasyAnimatorLayerDrawer>();

        private int _LayerCount;

         

        public void DoGUI(IEasyAnimatorComponent[] targets)
        {
            if (targets.Length != 1)
                return;

            DoGUI(targets[0]);
        }

         

        public void DoGUI(IEasyAnimatorComponent target)
        {
            DoNativeAnimatorControllerGUI(target);

            if (!target.IsPlayableInitialized)
            {
                DoPlayableNotInitializedGUI(target);
                return;
            }

            EditorGUI.BeginChangeCheck();

            var playable = target.Playable;

            // Gather the during the layout event and use the same ones during subsequent events to avoid GUI errors
            // in case they change (they shouldn't, but this is also more efficient).
            if (Event.current.type == EventType.Layout)
            {
                EasyAnimatorLayerDrawer.GatherLayerEditors(playable, LayerInfos, out _LayerCount);
            }

            DoRootGUI(playable);

            for (int i = 0; i < _LayerCount; i++)
                LayerInfos[i].DoGUI();

            DoLayerWeightWarningGUI(target);

            if (ShowInternalDetails)
                DoInternalDetailsGUI(playable);

            if (EditorGUI.EndChangeCheck() && !playable.IsGraphPlaying)
                playable.Evaluate();
        }

         

        private void DoNativeAnimatorControllerGUI(IEasyAnimatorComponent target)
        {
            if (!EditorApplication.isPlaying &&
                !target.IsPlayableInitialized)
                return;

            var animator = target.Animator;
            if (animator == null)
                return;

            var controller = (AnimatorController)animator.runtimeAnimatorController;
            if (controller == null)
                return;

            EasyAnimatorGUI.BeginVerticalBox(GUI.skin.box);

            var label = EasyAnimatorGUI.GetNarrowText("Native Animator Controller");

            EditorGUI.BeginChangeCheck();
            controller = (AnimatorController)EditorGUILayout.ObjectField(label, controller, typeof(AnimatorController), true);
            if (EditorGUI.EndChangeCheck())
                animator.runtimeAnimatorController = controller;

            var layers = controller.layers;
            for (int i = 0; i < layers.Length; i++)
            {
                var layer = layers[i];

                var runtimeState = animator.IsInTransition(i) ?
                    animator.GetNextAnimatorStateInfo(i) :
                    animator.GetCurrentAnimatorStateInfo(i);

                var states = layer.stateMachine.states;
                var editorState = GetState(states, runtimeState.shortNameHash);

                var area = EasyAnimatorGUI.LayoutSingleLineRect(EasyAnimatorGUI.SpacingMode.Before);

                var weight = i == 0 ? 1 : animator.GetLayerWeight(i);

                string stateName;
                if (editorState != null)
                {
                    stateName = editorState.name;

                    var isLooping = editorState.motion != null && editorState.motion.isLooping;
                    EasyAnimatorStateDrawer<ClipState>.DoTimeHighlightBarGUI(
                        area, true, weight, runtimeState.normalizedTime * runtimeState.length, runtimeState.length, isLooping);
                }
                else
                {
                    stateName = "State Not Found";
                }

                EasyAnimatorGUI.DoWeightLabel(ref area, weight);

                stateName = EasyAnimatorGUI.GetNarrowText(stateName);

                EditorGUI.LabelField(area, layer.name, stateName);
            }

            EasyAnimatorGUI.EndVerticalBox(GUI.skin.box);
        }

         

        private static AnimatorState GetState(ChildAnimatorState[] states, int nameHash)
        {
            for (int i = 0; i < states.Length; i++)
            {
                var state = states[i].state;
                if (state.nameHash == nameHash)
                {
                    return state;
                }
            }

            return null;
        }

         

        private void DoRootGUI(EasyAnimatorPlayable playable)
        {
            var labelWidth = EditorGUIUtility.labelWidth;
            EasyAnimatorGUI.BeginVerticalBox(GUI.skin.box);

            using (ObjectPool.Disposable.AcquireContent(out var label, "Is Graph Playing"))
            {
                const string SpeedLabel = "Speed";

                var isPlayingWidth = EasyAnimatorGUI.CalculateLabelWidth(label.text);
                var speedWidth = EasyAnimatorGUI.CalculateLabelWidth(SpeedLabel);

                var area = EasyAnimatorGUI.LayoutSingleLineRect();
                var isPlayingArea = area;
                var speedArea = area;
                isPlayingArea.width = isPlayingWidth + EasyAnimatorGUI.ToggleWidth;
                speedArea.xMin = isPlayingArea.xMax;

                EditorGUIUtility.labelWidth = isPlayingWidth;
                playable.IsGraphPlaying = EditorGUI.Toggle(isPlayingArea, label, playable.IsGraphPlaying);

                EditorGUIUtility.labelWidth = speedWidth;
                EditorGUI.BeginChangeCheck();
                var speed = EditorGUI.FloatField(speedArea, SpeedLabel, playable.Speed);
                if (EditorGUI.EndChangeCheck())
                    playable.Speed = speed;
                if (EasyAnimatorGUI.TryUseClickEvent(speedArea, 2))
                    playable.Speed = playable.Speed != 1 ? 1 : 0;
            }

            EasyAnimatorGUI.EndVerticalBox(GUI.skin.box);
            EditorGUIUtility.labelWidth = labelWidth;

            CheckContextMenu(GUILayoutUtility.GetLastRect(), playable);
        }

         

        private void DoPlayableNotInitializedGUI(IEasyAnimatorComponent target)
        {
            if (!EditorApplication.isPlaying ||
                target.Animator == null ||
                EditorUtility.IsPersistent(target.Animator))
                return;

            EditorGUILayout.HelpBox("Playable is not initialized." +
                " It will be initialized automatically when something needs it, such as playing an animation.",
                 MessageType.Info);

            if (EasyAnimatorGUI.TryUseClickEventInLastRect(1))
            {
                var menu = new GenericMenu();

                menu.AddItem(new GUIContent("Initialize"), false, () => target.Playable.Evaluate());

                EasyAnimatorEditorUtilities.AddDocumentationLink(menu, "Layer Documentation", Strings.DocsURLs.Layers);

                menu.ShowAsContext();
            }
        }

         

        private void DoLayerWeightWarningGUI(IEasyAnimatorComponent target)
        {
            if (_LayerCount == 0)
            {
                EditorGUILayout.HelpBox(
                    "No layers have been created, which likely means no animations have been played yet.",
                    MessageType.Warning);
                return;
            }

            if (!target.gameObject.activeInHierarchy ||
                !target.enabled ||
                (target.Animator != null && target.Animator.runtimeAnimatorController != null))
                return;

            if (_LayerCount == 1)
            {
                var layer = LayerInfos[0].Target;
                if (layer.Weight == 0)
                    EditorGUILayout.HelpBox(
                        layer + " is at 0 weight, which likely means no animations have been played yet.",
                        MessageType.Warning);
                return;
            }

            for (int i = 0; i < _LayerCount; i++)
            {
                var layer = LayerInfos[i].Target;
                if (layer.Weight == 1 &&
                    !layer.IsAdditive &&
                    layer._Mask == null &&
                    Mathf.Approximately(layer.GetTotalWeight(), 1))
                    return;
            }

            EditorGUILayout.HelpBox(
                "There are no Override layers at weight 1, which will likely give undesirable results." +
                " Click here for more information.",
                MessageType.Warning);

            if (EasyAnimatorGUI.TryUseClickEventInLastRect())
                EditorUtility.OpenWithDefaultApp(Strings.DocsURLs.Layers + "#blending");
        }

         

        private string _UpdateListLabel;
        private static GUIStyle _InternalDetailsStyle;

        internal void DoInternalDetailsGUI(EasyAnimatorPlayable playable)
        {
            if (Event.current.type == EventType.Layout)
            {
                var text = ObjectPool.AcquireStringBuilder();
                playable.AppendInternalDetails(text, "", " - ");
                _UpdateListLabel = text.ReleaseToString();
            }

            if (_UpdateListLabel == null)
                return;

            if (_InternalDetailsStyle == null)
                _InternalDetailsStyle = new GUIStyle(GUI.skin.box)
                {
                    alignment = TextAnchor.MiddleLeft,
                    wordWrap = false,
                    stretchWidth = true,
                };

            using (ObjectPool.Disposable.AcquireContent(out var content, _UpdateListLabel, null, false))
            {
                var height = _InternalDetailsStyle.CalcHeight(content, 0);
                var area = GUILayoutUtility.GetRect(0, height, _InternalDetailsStyle);
                GUI.Box(area, content, _InternalDetailsStyle);

                CheckContextMenu(area, playable);
            }
        }

         
        #region Context Menu
         

        private void CheckContextMenu(Rect clickArea, EasyAnimatorPlayable playable)
        {
            if (!EasyAnimatorGUI.TryUseClickEvent(clickArea, 1))
                return;

            var menu = new GenericMenu();

            menu.AddDisabledItem(new GUIContent(playable.Graph.GetEditorName() ?? "Unnamed Graph"), false);
            menu.AddDisabledItem(new GUIContent("Command Count: " + playable.CommandCount), false);
            menu.AddDisabledItem(new GUIContent("Frame ID: " + playable.FrameID), false);
            AddDisposablesFunctions(menu, playable.Disposables);

            AddUpdateModeFunctions(menu, playable);
            EasyAnimatorEditorUtilities.AddContextMenuIK(menu, playable);
            AddRootFunctions(menu, playable);

            menu.AddSeparator("");

            AddDisplayOptions(menu);

            menu.AddItem(new GUIContent("Log Details Of Everything"), false,
                () => Debug.Log(playable.GetDescription(), playable.Component as Object));
            AddPlayableGraphVisualizerFunction(menu, "", playable._Graph);

            EasyAnimatorEditorUtilities.AddDocumentationLink(menu, "Inspector Documentation", Strings.DocsURLs.Inspector);

            menu.ShowAsContext();
        }

         

        public static void AddRootFunctions(GenericMenu menu, EasyAnimatorPlayable playable)
        {
            menu.AddFunction("Add Layer",
                playable.Layers.Count < EasyAnimatorPlayable.LayerList.DefaultCapacity,
                () => playable.Layers.Count++);
            menu.AddFunction("Remove Layer",
                playable.Layers.Count > 0,
                () => playable.Layers.Count--);

            menu.AddItem(new GUIContent("Keep Children Connected ?"),
                playable.KeepChildrenConnected,
                () => playable.KeepChildrenConnected = !playable.KeepChildrenConnected);
        }

         

        private void AddUpdateModeFunctions(GenericMenu menu, EasyAnimatorPlayable playable)
        {
            var modes = Enum.GetValues(typeof(DirectorUpdateMode));
            for (int i = 0; i < modes.Length; i++)
            {
                var mode = (DirectorUpdateMode)modes.GetValue(i);
                menu.AddItem(new GUIContent("Update Mode/" + mode), playable.UpdateMode == mode,
                    () => playable.UpdateMode = mode);
            }
        }

         

        private void AddDisposablesFunctions(GenericMenu menu, List<IDisposable> disposables)
        {
            var prefix = $"{nameof(EasyAnimatorPlayable.Disposables)}: {disposables.Count}";
            if (disposables.Count == 0)
            {
                menu.AddDisabledItem(new GUIContent(prefix), false);
            }
            else
            {
                prefix += "/";
                for (int i = 0; i < disposables.Count; i++)
                {
                    menu.AddDisabledItem(new GUIContent(prefix + disposables[i]), false);
                }
            }
        }

         

        public static void AddPlayableGraphVisualizerFunction(GenericMenu menu, string prefix, PlayableGraph graph)
        {
            var type = Type.GetType("GraphVisualizer.PlayableGraphVisualizerWindow, Unity.PlayableGraphVisualizer.Editor");

            menu.AddFunction(prefix + "Playable Graph Visualizer", type != null, () =>
            {
                var window = EditorWindow.GetWindow(type);

                var field = type.GetField("m_CurrentGraph", EasyAnimatorEditorUtilities.AnyAccessBindings);

                if (field != null)
                    field.SetValue(window, graph);
            });
        }

         
        #endregion
         
        #region Prefs
         

        private const string
            KeyPrefix = "Inspector ",
            MenuPrefix = "Display Options/";

        internal static readonly BoolPref
            SortStatesByName = new BoolPref(KeyPrefix, MenuPrefix + "Sort States By Name", true),
            HideInactiveStates = new BoolPref(KeyPrefix, MenuPrefix + "Hide Inactive States", false),
            RepaintConstantly = new BoolPref(KeyPrefix, MenuPrefix + "Repaint Constantly", true),
            SeparateActiveFromInactiveStates = new BoolPref(KeyPrefix, MenuPrefix + "Separate Active From Inactive States", false),
            ScaleTimeBarByWeight = new BoolPref(KeyPrefix, MenuPrefix + "Scale Time Bar by Weight", true),
            ShowInternalDetails = new BoolPref(KeyPrefix, MenuPrefix + "Show Internal Details", false),
            VerifyAnimationBindings = new BoolPref(KeyPrefix, MenuPrefix + "Verify Animation Bindings", true),
            AutoNormalizeWeights = new BoolPref(KeyPrefix, MenuPrefix + "Auto Normalize Weights", true),
            UseNormalizedTimeSliders = new BoolPref("Inspector", nameof(UseNormalizedTimeSliders), false);

         

        public static void AddDisplayOptions(GenericMenu menu)
        {
            RepaintConstantly.AddToggleFunction(menu);
            SortStatesByName.AddToggleFunction(menu);
            HideInactiveStates.AddToggleFunction(menu);
            SeparateActiveFromInactiveStates.AddToggleFunction(menu);
            ScaleTimeBarByWeight.AddToggleFunction(menu);
            VerifyAnimationBindings.AddToggleFunction(menu);
            ShowInternalDetails.AddToggleFunction(menu);
            AutoNormalizeWeights.AddToggleFunction(menu);
        }

         
        #endregion
         
    }
}

#endif

