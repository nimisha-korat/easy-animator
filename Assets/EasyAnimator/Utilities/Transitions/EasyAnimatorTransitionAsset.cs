
using UnityEngine;
using System;

#if UNITY_EDITOR
using EasyAnimator.Editor;
using UnityEditor;
#endif

namespace EasyAnimator
{
    
    [CreateAssetMenu(menuName = Strings.MenuPrefix + "EasyAnimator Transition", order = Strings.AssetMenuOrder + 0)]
    [HelpURL(Strings.DocsURLs.APIDocumentation + "/" + nameof(EasyAnimatorTransitionAsset))]
    public class EasyAnimatorTransitionAsset : EasyAnimatorTransitionAsset<ITransition>
    {
         

#if UNITY_EDITOR
        protected override void Reset()
        {
            Transition = new ClipTransition();
        }
#endif

         

     
        [Serializable]
        public class UnShared<TAsset, TTransition, TState> : ITransition<TState>, ITransitionWithEvents, IWrapper
            where TAsset : EasyAnimatorTransitionAsset<TTransition>
            where TTransition : ITransition<TState>, IHasEvents
            where TState : EasyAnimatorState
        {
             

            [SerializeField]
            private TAsset _Asset;

            public ref TAsset Asset => ref _Asset;

            object IWrapper.WrappedObject => _Asset;

            public ref TTransition Transition => ref _Asset.Transition;

             

            public virtual bool IsValid => _Asset.IsValid();

             

            private EasyAnimatorState _BaseState;

            public EasyAnimatorState BaseState
            {
                get => _BaseState;
                private set
                {
                    _BaseState = value;
                    if (_State != value)
                        _State = null;
                }
            }

             

            private TState _State;

            public TState State
            {
                get
                {
                    if (_State == null)
                        _State = (TState)BaseState;

                    return _State;
                }
                protected set
                {
                    BaseState = _State = value;
                }
            }

             

            private EasyAnimatorEvent.Sequence _Events;

            public virtual EasyAnimatorEvent.Sequence Events
            {
                get
                {
                    if (_Events == null)
                        _Events = new EasyAnimatorEvent.Sequence(SerializedEvents.GetEventsOptional());

                    return _Events;
                }
            }

            public virtual ref EasyAnimatorEvent.Sequence.Serializable SerializedEvents => ref _Asset.Transition.SerializedEvents;

             

            public virtual void Apply(EasyAnimatorState state)
            {
                BaseState = state;
                _Asset.Apply(state);

                if (_Events == null)
                {
                    _Events = SerializedEvents.GetEventsOptional();
                    if (_Events == null)
                        return;

                    _Events = new EasyAnimatorEvent.Sequence(_Events);
                }

                state.Events = _Events;
            }

             

            public virtual object Key => _Asset.Key;

            public virtual float FadeDuration => _Asset.FadeDuration;

            public virtual FadeMode FadeMode => _Asset.FadeMode;

            public virtual TState CreateState() => State = (TState)_Asset.CreateState();

            EasyAnimatorState ITransition.CreateState() => BaseState = _Asset.CreateState();

             
        }

         

#if UNITY_EDITOR

      
        [CustomPropertyDrawer(typeof(UnShared<,,>), true)]
        public class UnSharedTransitionDrawer : PropertyDrawer
        {
             

            public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            {
                var height = EasyAnimatorGUI.LineHeight;

                if (property.propertyType == SerializedPropertyType.ManagedReference &&
                    property.isExpanded)
                    height += EasyAnimatorGUI.LineHeight + EasyAnimatorGUI.StandardSpacing;

                return height;
            }

             

            public override void OnGUI(Rect area, SerializedProperty property, GUIContent label)
            {
                if (property.propertyType == SerializedPropertyType.ManagedReference)
                {
                    using (new TypeSelectionButton(area, property, true))
                        EditorGUI.PropertyField(area, property, label, true);
                }
                else
                {
                    var transitionProperty = property.FindPropertyRelative("_Asset");
                    EditorGUI.PropertyField(area, transitionProperty, label, false);
                }
            }

             
        }

#endif
    }
}
