

#if UNITY_EDITOR

using System;
using UnityEditor;
using UnityEngine;

namespace EasyAnimator.Editor
{
    
    public abstract class BaseEasyAnimatorComponentEditor : UnityEditor.Editor
    {
         

        [NonSerialized]
        private IEasyAnimatorComponent[] _Targets;
        public IEasyAnimatorComponent[] Targets => _Targets;

        private readonly EasyAnimatorPlayableDrawer
            PlayableDrawer = new EasyAnimatorPlayableDrawer();

         

        protected virtual void OnEnable()
        {
            var targets = this.targets;
            _Targets = new IEasyAnimatorComponent[targets.Length];
            GatherTargets();
        }

         

        private void GatherTargets()
        {
            for (int i = 0; i < _Targets.Length; i++)
                _Targets[i] = (IEasyAnimatorComponent)targets[i];
        }

         

        public override void OnInspectorGUI()
        {
            _LastRepaintTime = EditorApplication.timeSinceStartup;

            // Normally the targets wouldn't change after OnEnable, but the trick EasyAnimatorComponent.Reset uses to
            // swap the type of an existing component when a new one is added causes the old target to be destroyed.
            GatherTargets();

            serializedObject.Update();

            var area = GUILayoutUtility.GetRect(0, 0);

            DoOtherFieldsGUI();
            PlayableDrawer.DoGUI(_Targets);

            area.yMax = GUILayoutUtility.GetLastRect().yMax;
            EasyAnimatorLayerDrawer.HandleDragAndDropAnimations(area, _Targets[0], 0);

            serializedObject.ApplyModifiedProperties();
        }

         

        [NonSerialized]
        private double _LastRepaintTime = double.NegativeInfinity;

        public override bool RequiresConstantRepaint()
        {
            if (_Targets.Length != 1)
                return false;

            var target = _Targets[0];
            if (!target.IsPlayableInitialized)
            {
                if (!EditorApplication.isPlaying ||
                    target.Animator == null ||
                    target.Animator.runtimeAnimatorController == null)
                    return false;
            }

            if (EasyAnimatorPlayableDrawer.RepaintConstantly)
                return true;

            return EditorApplication.timeSinceStartup > _LastRepaintTime + EasyAnimatorSettings.InspectorRepaintInterval;
        }

         

        protected void DoOtherFieldsGUI()
        {
            var property = serializedObject.GetIterator();

            if (!property.NextVisible(true))
                return;

            do
            {
                var path = property.propertyPath;
                if (path == "m_Script")
                    continue;

                using (ObjectPool.Disposable.AcquireContent(out var label, property))
                {
                    // Let the target try to override.
                    if (DoOverridePropertyGUI(path, property, label))
                        continue;

                    // Otherwise draw the property normally.
                    EditorGUILayout.PropertyField(property, label, true);
                }
            }
            while (property.NextVisible(false));
        }

         

        protected virtual bool DoOverridePropertyGUI(string path, SerializedProperty property, GUIContent label) => false;

         
    }
}

#endif

