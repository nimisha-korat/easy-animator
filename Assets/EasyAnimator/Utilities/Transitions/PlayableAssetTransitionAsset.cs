

using EasyAnimator.Units;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using Object = UnityEngine.Object;

namespace EasyAnimator
{
    
    [CreateAssetMenu(menuName = Strings.MenuPrefix + "Playable Asset Transition", order = Strings.AssetMenuOrder + 9)]
    [HelpURL(Strings.DocsURLs.APIDocumentation + "/" + nameof(PlayableAssetTransitionAsset))]
    public class PlayableAssetTransitionAsset : EasyAnimatorTransitionAsset<PlayableAssetTransition>
    {
        [Serializable]
        public class UnShared :
            EasyAnimatorTransitionAsset.UnShared<PlayableAssetTransitionAsset, PlayableAssetTransition, PlayableAssetState>,
            PlayableAssetState.ITransition
        { }
    }


    [Serializable]
    public class PlayableAssetTransition : EasyAnimatorTransition<PlayableAssetState>,
        PlayableAssetState.ITransition, IAnimationClipCollection
    {
         

        [SerializeField, Tooltip("The asset to play")]
        private PlayableAsset _Asset;

        public ref PlayableAsset Asset => ref _Asset;

        public override Object MainObject => _Asset;

        public override object Key => _Asset;

         

        [SerializeField]
        [Tooltip(Strings.Tooltips.OptionalSpeed)]
        [AnimationSpeed]
        [DefaultValue(1f, -1f)]
        private float _Speed = 1;

        public override float Speed
        {
            get => _Speed;
            set => _Speed = value;
        }

         

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

         

        [SerializeField]
        [Tooltip("The objects controlled by each of the tracks in the Asset")]
#if UNITY_2020_2_OR_NEWER
        [NonReorderable]
#endif
        private Object[] _Bindings;

        public ref Object[] Bindings => ref _Bindings;

         

        public override float MaximumDuration => _Asset != null ? (float)_Asset.duration : 0;

        public override bool IsValid => _Asset != null;

         

        public override PlayableAssetState CreateState()
        {
            State = new PlayableAssetState(_Asset);
            State.SetBindings(_Bindings);
            return State;
        }

         

        public override void Apply(EasyAnimatorState state)
        {
            base.Apply(state);
            ApplyDetails(state, _Speed, _NormalizedStartTime);
        }

         

        void IAnimationClipCollection.GatherAnimationClips(ICollection<AnimationClip> clips) => clips.GatherFromAsset(_Asset);

         
#if UNITY_EDITOR
         

        [UnityEditor.CustomPropertyDrawer(typeof(PlayableAssetTransition), true)]
        public class Drawer : Editor.TransitionDrawer
        {
             

            public Drawer() : base(nameof(_Asset)) { }

             

            public override float GetPropertyHeight(UnityEditor.SerializedProperty property, GUIContent label)
            {
                _CurrentAsset = null;

                var height = base.GetPropertyHeight(property, label);

                if (property.isExpanded)
                {
                    var bindings = property.FindPropertyRelative(nameof(_Bindings));
                    bindings.isExpanded = true;
                    height -= Editor.EasyAnimatorGUI.StandardSpacing + Editor.EasyAnimatorGUI.LineHeight;
                }

                return height;
            }

             

            private static PlayableAsset _CurrentAsset;

            protected override void DoChildPropertyGUI(ref Rect area, UnityEditor.SerializedProperty rootProperty,
                UnityEditor.SerializedProperty property, GUIContent label)
            {
                var path = property.propertyPath;
                if (path.EndsWith($".{nameof(_Asset)}"))
                {
                    _CurrentAsset = property.objectReferenceValue as PlayableAsset;
                }
                else if (path.EndsWith($".{nameof(_Bindings)}"))
                {
                    IEnumerator<PlayableBinding> outputEnumerator;
                    var outputCount = 0;
                    var firstBindingIsAnimation = false;
                    if (_CurrentAsset != null)
                    {
                        var outputs = _CurrentAsset.outputs;
                        _CurrentAsset = null;
                        outputEnumerator = outputs.GetEnumerator();

                        while (outputEnumerator.MoveNext())
                        {
                            if (PlayableAssetState.ShouldSkipBinding(outputEnumerator.Current, out _, out _))
                                continue;

                            if (outputCount == 0 && outputEnumerator.Current.outputTargetType == typeof(Animator))
                                firstBindingIsAnimation = true;

                            outputCount++;
                        }

                        outputEnumerator = outputs.GetEnumerator();
                    }
                    else outputEnumerator = null;

                    // Bindings.
                    property.Next(true);
                    // Array.
                    property.Next(true);
                    // Array Size.

                    var color = GUI.color;
                    var miniButton = Editor.EasyAnimatorGUI.MiniButton;
                    var sizeArea = area;
                    var bindingCount = property.intValue;
                    if (bindingCount != outputCount && !(bindingCount == 0 && outputCount == 1 && firstBindingIsAnimation))
                    {
                        GUI.color = Editor.EasyAnimatorGUI.WarningFieldColor;

                        var labelText = label.text;

                        var countLabel = outputCount.ToString();
                        var fixSizeWidth = Editor.EasyAnimatorGUI.CalculateWidth(miniButton, countLabel);
                        var fixSizeArea = Editor.EasyAnimatorGUI.StealFromRight(ref sizeArea, fixSizeWidth, Editor.EasyAnimatorGUI.StandardSpacing);
                        if (GUI.Button(fixSizeArea, countLabel, miniButton))
                            property.intValue = outputCount;

                        label.text = labelText;
                    }
                    UnityEditor.EditorGUI.PropertyField(sizeArea, property, label, false);
                    GUI.color = color;

                    UnityEditor.EditorGUI.indentLevel++;

                    bindingCount = property.intValue;
                    for (int i = 0; i < bindingCount; i++)
                    {
                        Editor.EasyAnimatorGUI.NextVerticalArea(ref area);
                        property.Next(false);

                        if (outputEnumerator != null && outputEnumerator.MoveNext())
                        {
                            CheckIfSkip:
                            if (PlayableAssetState.ShouldSkipBinding(outputEnumerator.Current, out var name, out var type))
                            {
                                outputEnumerator.MoveNext();
                                goto CheckIfSkip;
                            }

                            label.text = name;

                            var targetObject = property.serializedObject.targetObject;
                            var allowSceneObjects = targetObject != null && !UnityEditor.EditorUtility.IsPersistent(targetObject);

                            label = UnityEditor.EditorGUI.BeginProperty(area, label, property);
                            var fieldArea = area;
                            var obj = property.objectReferenceValue;
                            var objExists = obj != null;

                            if (objExists)
                            {
                                if (i == 0 && type == typeof(Animator))
                                {
                                    DoRemoveButton(ref fieldArea, label, property, ref obj,
                                        "This Animation Track is the first Track" +
                                        " so it will automatically control the EasyAnimator output and likely does not need a binding.");
                                }
                                else if (type == null)
                                {
                                    DoRemoveButton(ref fieldArea, label, property, ref obj,
                                        "This Animation Track does not need a binding.");
                                    type = typeof(Object);
                                }
                                else if (!type.IsAssignableFrom(obj.GetType()))
                                {
                                    DoRemoveButton(ref fieldArea, label, property, ref obj,
                                        "This binding has the wrong type for this Animation Track.");
                                }
                            }

                            if (type != null || objExists)
                            {
                                property.objectReferenceValue =
                                    UnityEditor.EditorGUI.ObjectField(fieldArea, label, obj, type, allowSceneObjects);
                            }
                            else
                            {
                                UnityEditor.EditorGUI.LabelField(fieldArea, label);
                            }

                            UnityEditor.EditorGUI.EndProperty();
                        }
                        else
                        {
                            GUI.color = Editor.EasyAnimatorGUI.WarningFieldColor;

                            UnityEditor.EditorGUI.PropertyField(area, property, false);
                        }

                        GUI.color = color;
                    }

                    UnityEditor.EditorGUI.indentLevel--;
                    return;
                }

                base.DoChildPropertyGUI(ref area, rootProperty, property, label);
            }

             

            private static void DoRemoveButton(ref Rect area, GUIContent label, UnityEditor.SerializedProperty property,
                ref Object obj, string tooltip)
            {
                label.tooltip = tooltip;
                GUI.color = Editor.EasyAnimatorGUI.WarningFieldColor;
                var miniButton = Editor.EasyAnimatorGUI.MiniButton;

                var text = label.text;
                label.text = "x";

                var xWidth = Editor.EasyAnimatorGUI.CalculateWidth(miniButton, label);
                var xArea = Editor.EasyAnimatorGUI.StealFromRight(
                    ref area, xWidth, Editor.EasyAnimatorGUI.StandardSpacing);
                if (GUI.Button(xArea, label, miniButton))
                    property.objectReferenceValue = obj = null;

                label.text = text;
            }

             
        }

         
#endif
         
    }
}
