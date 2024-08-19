
#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace EasyAnimator.Editor
{
    partial class EasyAnimatorToolsWindow
    {
        
        [Serializable]
        public sealed class RemapSpriteAnimation : AnimationModifierPanel
        {
             

            [SerializeField] private List<Sprite> _NewSprites;

            [NonSerialized] private readonly List<Sprite> OldSprites = new List<Sprite>();
            [NonSerialized] private bool _OldSpritesAreDirty;
            [NonSerialized] private ReorderableList _OldSpriteDisplay;
            [NonSerialized] private ReorderableList _NewSpriteDisplay;
            [NonSerialized] private EditorCurveBinding _SpriteBinding;
            [NonSerialized] private ObjectReferenceKeyframe[] _SpriteKeyframes;

             

            public override string Name => "Remap Sprite Animation";

            public override string HelpURL => Strings.DocsURLs.RemapSpriteAnimation;

            public override string Instructions
            {
                get
                {
                    if (Animation == null)
                        return "Select the animation you want to remap.";

                    if (OldSprites.Count == 0)
                        return "The selected animation does not use Sprites.";

                    return "Assign the New Sprites that you want to replace the Old Sprites with then click Save As." +
                        " You can Drag and Drop multiple Sprites onto the New Sprites list at the same time.";
                }
            }

             

            public override void OnEnable(int index)
            {
                base.OnEnable(index);

                if (_NewSprites == null)
                    _NewSprites = new List<Sprite>();

                if (Animation == null)
                    _NewSprites.Clear();

                _OldSpriteDisplay = CreateReorderableObjectList(OldSprites, "Old Sprites");
                _NewSpriteDisplay = CreateReorderableObjectList(_NewSprites, "New Sprites");
            }

             

            protected override void OnAnimationChanged()
            {
                base.OnAnimationChanged();
                _OldSpritesAreDirty = true;
            }

             

            public override void DoBodyGUI()
            {
                base.DoBodyGUI();
                GatherOldSprites();

                GUILayout.BeginHorizontal();
                {
                    GUILayout.BeginVertical();
                    GUI.enabled = false;
                    _OldSpriteDisplay.DoLayoutList();
                    GUI.enabled = true;
                    GUILayout.EndVertical();

                    GUILayout.BeginVertical();
                    _NewSpriteDisplay.DoLayoutList();
                    GUILayout.EndVertical();

                    HandleDragAndDropIntoList(GUILayoutUtility.GetLastRect(), _NewSprites, overwrite: true);
                }
                GUILayout.EndHorizontal();

                GUI.enabled = Animation != null;

                GUILayout.BeginHorizontal();
                {
                    GUILayout.FlexibleSpace();

                    if (GUILayout.Button("Reset"))
                    {
                        EasyAnimatorGUI.Deselect();
                        RecordUndo();
                        _NewSprites.Clear();
                        _OldSpritesAreDirty = true;
                    }

                    if (GUILayout.Button("Save As"))
                    {
                        if (SaveAs())
                        {
                            _OldSpritesAreDirty = true;
                        }
                    }
                }
                GUILayout.EndHorizontal();
            }

             

            private void GatherOldSprites()
            {
                if (!_OldSpritesAreDirty)
                    return;

                _OldSpritesAreDirty = false;

                OldSprites.Clear();
                _NewSprites.Clear();

                if (Animation == null)
                    return;

                var bindings = AnimationUtility.GetObjectReferenceCurveBindings(Animation);
                for (int iBinding = 0; iBinding < bindings.Length; iBinding++)
                {
                    var binding = bindings[iBinding];
                    if (binding.type == typeof(SpriteRenderer) && binding.propertyName == "m_Sprite")
                    {
                        _SpriteBinding = binding;
                        _SpriteKeyframes = AnimationUtility.GetObjectReferenceCurve(Animation, binding);

                        for (int iKeyframe = 0; iKeyframe < _SpriteKeyframes.Length; iKeyframe++)
                        {
                            var reference = _SpriteKeyframes[iKeyframe].value as Sprite;
                            if (reference != null)
                                OldSprites.Add(reference);
                        }

                        _NewSprites.AddRange(OldSprites);

                        return;
                    }
                }
            }

             

            protected override void Modify(AnimationClip animation)
            {
                for (int i = 0; i < _SpriteKeyframes.Length; i++)
                {
                    _SpriteKeyframes[i].value = _NewSprites[i];
                }

                AnimationUtility.SetObjectReferenceCurve(animation, _SpriteBinding, _SpriteKeyframes);
            }

             
        }
    }
}

#endif

