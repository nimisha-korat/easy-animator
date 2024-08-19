
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using Object = UnityEngine.Object;
using EasyAnimator.Units;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
#endif

namespace EasyAnimator
{

    [CreateAssetMenu(menuName = Strings.MenuPrefix + "Controller Transition/Base", order = Strings.AssetMenuOrder + 5)]
    [HelpURL(Strings.DocsURLs.APIDocumentation + "/" + nameof(ControllerTransitionAsset))]
    public class ControllerTransitionAsset : EasyAnimatorTransitionAsset<ControllerTransition>
    {
        [Serializable]
        public class UnShared :
            EasyAnimatorTransitionAsset.UnShared<ControllerTransitionAsset, ControllerTransition, ControllerState>,
            ControllerState.ITransition
        { }
    }

     

    [Serializable]
    public abstract class ControllerTransition<TState> : EasyAnimatorTransition<TState>, IAnimationClipCollection
        where TState : ControllerState
    {
         

        [SerializeField]
        private RuntimeAnimatorController _Controller;

        public ref RuntimeAnimatorController Controller => ref _Controller;

        public override Object MainObject => _Controller;

#if UNITY_EDITOR
        public const string ControllerFieldName = nameof(_Controller);
#endif

         

        [SerializeField]
        [Tooltip(Strings.Tooltips.NormalizedStartTime)]
        [AnimationTime(AnimationTimeAttribute.Units.Normalized)]
        [DefaultValue(0, float.NaN)]
        private float _NormalizedStartTime;

        public override float NormalizedStartTime
        {
            get => _NormalizedStartTime;
            set => _NormalizedStartTime = value;
        }

         

        [SerializeField, Tooltip("If false, stopping this state will reset all its layers to their default state")]
        private bool _KeepStateOnStop;

        public ref bool KeepStateOnStop => ref _KeepStateOnStop;

         

        public override float MaximumDuration
        {
            get
            {
                if (_Controller == null)
                    return 0;

                var duration = 0f;

                var clips = _Controller.animationClips;
                for (int i = 0; i < clips.Length; i++)
                {
                    var length = clips[i].length;
                    if (duration < length)
                        duration = length;
                }

                return duration;
            }
        }

         

        public override bool IsValid => _Controller != null;

         

        public static implicit operator RuntimeAnimatorController(ControllerTransition<TState> transition)
            => transition?._Controller;

         

        public override void Apply(EasyAnimatorState state)
        {
            base.Apply(state);

            var controllerState = State;
            if (controllerState != null)
            {
                controllerState.KeepStateOnStop = _KeepStateOnStop;

                if (!float.IsNaN(_NormalizedStartTime))
                {
                    if (!_KeepStateOnStop)
                    {
                        controllerState.Playable.Play(controllerState.DefaultStateHashes[0], 0, _NormalizedStartTime);
                    }
                    else
                    {
                        state.NormalizedTime = _NormalizedStartTime;
                    }
                }
            }
            else
            {
                if (!float.IsNaN(_NormalizedStartTime))
                    state.NormalizedTime = _NormalizedStartTime;
            }
        }

         

        void IAnimationClipCollection.GatherAnimationClips(ICollection<AnimationClip> clips)
        {
            if (_Controller != null)
                clips.Gather(_Controller.animationClips);
        }

         
    }

     

   
    [Serializable]
    public class ControllerTransition : ControllerTransition<ControllerState>, ControllerState.ITransition
    {
         

        public override ControllerState CreateState() => State = new ControllerState(Controller, KeepStateOnStop);

         

        public ControllerTransition() { }

        public ControllerTransition(RuntimeAnimatorController controller) => Controller = controller;

         

        public static implicit operator ControllerTransition(RuntimeAnimatorController controller)
            => new ControllerTransition(controller);

         
        #region Drawer
#if UNITY_EDITOR
         

        [CustomPropertyDrawer(typeof(ControllerTransition<>), true)]
        [CustomPropertyDrawer(typeof(ControllerTransition), true)]
        public class Drawer : Editor.TransitionDrawer
        {
             

            private readonly string[] Parameters;
            private readonly string[] ParameterPrefixes;

             

            public Drawer() : this(null) { }

            public Drawer(params string[] parameters) : base(ControllerFieldName)
            {
                Parameters = parameters;
                if (parameters == null)
                    return;

                ParameterPrefixes = new string[parameters.Length];

                for (int i = 0; i < ParameterPrefixes.Length; i++)
                {
                    ParameterPrefixes[i] = "." + parameters[i];
                }
            }

             

            protected override void DoChildPropertyGUI(ref Rect area, SerializedProperty rootProperty,
                SerializedProperty property, GUIContent label)
            {
                var path = property.propertyPath;

                if (ParameterPrefixes != null)
                {
                    var controllerProperty = rootProperty.FindPropertyRelative(MainPropertyName);
                    var controller = controllerProperty.objectReferenceValue as AnimatorController;
                    if (controller != null)
                    {
                        for (int i = 0; i < ParameterPrefixes.Length; i++)
                        {
                            if (path.EndsWith(ParameterPrefixes[i]))
                            {
                                area.height = Editor.EasyAnimatorGUI.LineHeight;
                                DoParameterGUI(area, controller, property);
                                return;
                            }
                        }
                    }
                }

                EditorGUI.BeginChangeCheck();

                base.DoChildPropertyGUI(ref area, rootProperty, property, label);

                // When the controller changes, validate all parameters.
                if (EditorGUI.EndChangeCheck() &&
                    Parameters != null &&
                    path.EndsWith(MainPropertyPathSuffix))
                {
                    var controller = property.objectReferenceValue as AnimatorController;
                    if (controller != null)
                    {
                        for (int i = 0; i < Parameters.Length; i++)
                        {
                            property = rootProperty.FindPropertyRelative(Parameters[i]);
                            var parameterName = property.stringValue;
                            if (!HasFloatParameter(controller, parameterName))
                            {
                                parameterName = GetFirstFloatParameterName(controller);
                                if (!string.IsNullOrEmpty(parameterName))
                                    property.stringValue = parameterName;
                            }
                        }
                    }
                }
            }

             

            protected void DoParameterGUI(Rect area, AnimatorController controller, SerializedProperty property)
            {
                var parameterName = property.stringValue;
                var parameters = controller.parameters;

                using (ObjectPool.Disposable.AcquireContent(out var label, property))
                {
                    label = EditorGUI.BeginProperty(area, label, property);

                    var xMax = area.xMax;
                    area.width = EditorGUIUtility.labelWidth;
                    EditorGUI.PrefixLabel(area, label);

                    area.x += area.width;
                    area.xMax = xMax;
                }

                var color = GUI.color;
                if (!HasFloatParameter(controller, parameterName))
                    GUI.color = Editor.EasyAnimatorGUI.ErrorFieldColor;

                using (ObjectPool.Disposable.AcquireContent(out var label, parameterName))
                {
                    if (EditorGUI.DropdownButton(area, label, FocusType.Passive))
                    {
                        property = property.Copy();

                        var menu = new GenericMenu();

                        for (int i = 0; i < parameters.Length; i++)
                        {
                            var parameter = parameters[i];
                            Editor.Serialization.AddPropertyModifierFunction(menu, property, parameter.name,
                                parameter.type == AnimatorControllerParameterType.Float,
                                (targetProperty) =>
                                {
                                    targetProperty.stringValue = parameter.name;
                                });
                        }

                        if (menu.GetItemCount() == 0)
                            menu.AddDisabledItem(new GUIContent("No Parameters"));

                        menu.ShowAsContext();
                    }
                }

                GUI.color = color;

                EditorGUI.EndProperty();
            }

             

            private static bool HasFloatParameter(AnimatorController controller, string name)
            {
                if (string.IsNullOrEmpty(name))
                    return false;

                var parameters = controller.parameters;

                for (int i = 0; i < parameters.Length; i++)
                {
                    var parameter = parameters[i];
                    if (parameter.type == AnimatorControllerParameterType.Float && name == parameters[i].name)
                    {
                        return true;
                    }
                }

                return false;
            }

             

            private static string GetFirstFloatParameterName(AnimatorController controller)
            {
                var parameters = controller.parameters;

                for (int i = 0; i < parameters.Length; i++)
                {
                    var parameter = parameters[i];
                    if (parameter.type == AnimatorControllerParameterType.Float)
                    {
                        return parameter.name;
                    }
                }

                return "";
            }

             
        }

         
#endif
        #endregion
         
    }
}
