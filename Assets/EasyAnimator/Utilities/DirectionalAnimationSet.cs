
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Object = UnityEngine.Object;

namespace EasyAnimator
{
   
    [CreateAssetMenu(menuName = Strings.MenuPrefix + "Directional Animation Set/4 Directions", order = Strings.AssetMenuOrder + 10)]
    [HelpURL(Strings.DocsURLs.APIDocumentation + "/" + nameof(DirectionalAnimationSet))]
    public class DirectionalAnimationSet : ScriptableObject, IAnimationClipSource
    {
         

        [SerializeField]
        private AnimationClip _Up;

        public AnimationClip Up
        {
            get => _Up;
            set
            {
                AssertCanSetClips();
                _Up = value;
                EasyAnimatorUtilities.SetDirty(this);
            }
        }

         

        [SerializeField]
        private AnimationClip _Right;

        public AnimationClip Right
        {
            get => _Right;
            set
            {
                AssertCanSetClips();
                _Right = value;
                EasyAnimatorUtilities.SetDirty(this);
            }
        }

         

        [SerializeField]
        private AnimationClip _Down;

        public AnimationClip Down
        {
            get => _Down;
            set
            {
                AssertCanSetClips();
                _Down = value;
                EasyAnimatorUtilities.SetDirty(this);
            }
        }

         

        [SerializeField]
        private AnimationClip _Left;

        public AnimationClip Left
        {
            get => _Left;
            set
            {
                AssertCanSetClips();
                _Left = value;
                EasyAnimatorUtilities.SetDirty(this);
            }
        }

         

#if UNITY_ASSERTIONS
        private bool _AllowSetClips;
#endif

        [System.Diagnostics.Conditional(Strings.Assertions)]
        public void AllowSetClips(bool allow = true)
        {
#if UNITY_ASSERTIONS
            _AllowSetClips = allow;
#endif
        }

        [System.Diagnostics.Conditional(Strings.Assertions)]
        public void AssertCanSetClips()
        {
#if UNITY_ASSERTIONS
            EasyAnimatorUtilities.Assert(_AllowSetClips, $"{nameof(AllowSetClips)}() must be called before attempting to set any of" +
                $" the animations in a {nameof(DirectionalAnimationSet)} to ensure that they are not changed accidentally.");
#endif
        }

         

        public virtual AnimationClip GetClip(Vector2 direction)
        {
            if (direction.x >= 0)
            {
                if (direction.y >= 0)
                    return direction.x > direction.y ? _Right : _Up;
                else
                    return direction.x > -direction.y ? _Right : _Down;
            }
            else
            {
                if (direction.y >= 0)
                    return direction.x < -direction.y ? _Left : _Up;
                else
                    return direction.x < direction.y ? _Left : _Down;
            }
        }

         
        #region Directions
         

        public virtual int ClipCount => 4;

         

       
        public enum Direction
        {
            Up,

            Right,

            Down,

            Left,
        }

         

        protected virtual string GetDirectionName(int direction) => ((Direction)direction).ToString();

         

        public AnimationClip GetClip(Direction direction)
        {
            switch (direction)
            {
                case Direction.Up: return _Up;
                case Direction.Right: return _Right;
                case Direction.Down: return _Down;
                case Direction.Left: return _Left;
                default: throw new ArgumentException($"Unsupported {nameof(Direction)}: {direction}");
            }
        }

        public virtual AnimationClip GetClip(int direction) => GetClip((Direction)direction);

         

        public void SetClip(Direction direction, AnimationClip clip)
        {
            switch (direction)
            {
                case Direction.Up: Up = clip; break;
                case Direction.Right: Right = clip; break;
                case Direction.Down: Down = clip; break;
                case Direction.Left: Left = clip; break;
                default: throw new ArgumentException($"Unsupported {nameof(Direction)}: {direction}");
            }
        }

        public virtual void SetClip(int direction, AnimationClip clip) => SetClip((Direction)direction, clip);

         
        #region Conversion
         

        public static Vector2 DirectionToVector(Direction direction)
        {
            switch (direction)
            {
                case Direction.Up: return Vector2.up;
                case Direction.Right: return Vector2.right;
                case Direction.Down: return Vector2.down;
                case Direction.Left: return Vector2.left;
                default: throw new ArgumentException($"Unsupported {nameof(Direction)}: {direction}");
            }
        }

        public virtual Vector2 GetDirection(int direction) => DirectionToVector((Direction)direction);

         

        public static Direction VectorToDirection(Vector2 vector)
        {
            if (vector.x >= 0)
            {
                if (vector.y >= 0)
                    return vector.x > vector.y ? Direction.Right : Direction.Up;
                else
                    return vector.x > -vector.y ? Direction.Right : Direction.Down;
            }
            else
            {
                if (vector.y >= 0)
                    return vector.x < -vector.y ? Direction.Left : Direction.Up;
                else
                    return vector.x < vector.y ? Direction.Left : Direction.Down;
            }
        }

         

        public static Vector2 SnapVectorToDirection(Vector2 vector)
        {
            var magnitude = vector.magnitude;
            var direction = VectorToDirection(vector);
            vector = DirectionToVector(direction) * magnitude;
            return vector;
        }

        public virtual Vector2 Snap(Vector2 vector) => SnapVectorToDirection(vector);

         
        #endregion
         
        #region Collections
         

        public void AddClips(AnimationClip[] clips, int index)
        {
            var count = ClipCount;
            for (int i = 0; i < count; i++)
                clips[index + i] = GetClip(i);
        }

        public void GetAnimationClips(List<AnimationClip> clips)
        {
            var count = ClipCount;
            for (int i = 0; i < count; i++)
                clips.Add(GetClip(i));
        }

         

        public void AddDirections(Vector2[] directions, int index)
        {
            var count = ClipCount;
            for (int i = 0; i < count; i++)
                directions[index + i] = GetDirection(i);
        }

         

        public void AddClipsAndDirections(AnimationClip[] clips, Vector2[] directions, int index)
        {
            AddClips(clips, index);
            AddDirections(directions, index);
        }

         
        #endregion
         
        #endregion
         
        #region Editor Functions
         
#if UNITY_EDITOR
         

        [UnityEditor.CustomEditor(typeof(DirectionalAnimationSet), true), UnityEditor.CanEditMultipleObjects]
        private class Editor : EasyAnimator.Editor.ScriptableObjectEditor { }

         

        public virtual int SetClipByName(AnimationClip clip)
        {
            var name = clip.name;

            int bestDirection = -1;
            int bestDirectionIndex = -1;

            var directionCount = ClipCount;
            for (int i = 0; i < directionCount; i++)
            {
                var index = name.LastIndexOf(GetDirectionName(i));
                if (bestDirectionIndex < index)
                {
                    bestDirectionIndex = index;
                    bestDirection = i;
                }
            }

            if (bestDirection >= 0)
                SetClip(bestDirection, clip);

            return bestDirection;
        }

         

        [UnityEditor.MenuItem("CONTEXT/" + nameof(DirectionalAnimationSet) + "/Find Animations")]
        private static void FindSimilarAnimations(UnityEditor.MenuCommand command)
        {
            var set = (DirectionalAnimationSet)command.context;

            UnityEditor.Undo.RecordObject(set, "Find Animations");

            var directory = UnityEditor.AssetDatabase.GetAssetPath(set);
            directory = Path.GetDirectoryName(directory);

            var guids = UnityEditor.AssetDatabase.FindAssets(
                $"{set.name} t:{nameof(AnimationClip)}",
                new string[] { directory });

            for (int i = 0; i < guids.Length; i++)
            {
                var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[i]);
                var clip = UnityEditor.AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
                if (clip == null)
                    continue;

                set.SetClipByName(clip);
            }
        }

         

        [UnityEditor.MenuItem(Strings.CreateMenuPrefix + "Directional Animation Set/From Selection",
            priority = Strings.AssetMenuOrder + 12)]
        private static void CreateDirectionalAnimationSet()
        {
            var nameToAnimations = new Dictionary<string, List<AnimationClip>>();

            var selection = UnityEditor.Selection.objects;
            for (int i = 0; i < selection.Length; i++)
            {
                var clip = selection[i] as AnimationClip;
                if (clip == null)
                    continue;

                var name = clip.name;
                for (Direction direction = 0; direction < (Direction)4; direction++)
                {
                    name = name.Replace(direction.ToString(), "");
                }

                if (!nameToAnimations.TryGetValue(name, out var clips))
                {
                    clips = new List<AnimationClip>();
                    nameToAnimations.Add(name, clips);
                }

                clips.Add(clip);
            }

            if (nameToAnimations.Count == 0)
                throw new InvalidOperationException("No clips are selected");

            var sets = new List<Object>();
            foreach (var nameAndAnimations in nameToAnimations)
            {
                var set = nameAndAnimations.Value.Count <= 4 ?
                    CreateInstance<DirectionalAnimationSet>() :
                    CreateInstance<DirectionalAnimationSet8>();

                set.AllowSetClips();
                for (int i = 0; i < nameAndAnimations.Value.Count; i++)
                {
                    set.SetClipByName(nameAndAnimations.Value[i]);
                }

                var path = UnityEditor.AssetDatabase.GetAssetPath(nameAndAnimations.Value[0]);
                path = $"{Path.GetDirectoryName(path)}/{nameAndAnimations.Key}.asset";
                UnityEditor.AssetDatabase.CreateAsset(set, path);

                sets.Add(set);
            }

            UnityEditor.Selection.objects = sets.ToArray();
        }

         

        [UnityEditor.MenuItem("CONTEXT/" + nameof(DirectionalAnimationSet) + "/Toggle Looping")]
        private static void ToggleLooping(UnityEditor.MenuCommand command)
        {
            var set = (DirectionalAnimationSet)command.context;

            var count = set.ClipCount;
            for (int i = 0; i < count; i++)
            {
                var clip = set.GetClip(i);
                if (clip == null)
                    continue;

                var isLooping = !clip.isLooping;
                for (i = 0; i < count; i++)
                {
                    clip = set.GetClip(i);
                    if (clip == null)
                        continue;

                    EasyAnimator.Editor.EasyAnimatorEditorUtilities.SetLooping(clip, isLooping);
                }

                break;
            }
        }

         
#endif
         
        #endregion
         
    }
}
