
using System;
using UnityEngine;

namespace EasyAnimator
{
  
    [CreateAssetMenu(menuName = Strings.MenuPrefix + "Directional Animation Set/8 Directions", order = Strings.AssetMenuOrder + 11)]
    [HelpURL(Strings.DocsURLs.APIDocumentation + "/" + nameof(DirectionalAnimationSet8))]
    public class DirectionalAnimationSet8 : DirectionalAnimationSet
    {
         

        [SerializeField]
        private AnimationClip _UpRight;

        public AnimationClip UpRight
        {
            get => _UpRight;
            set
            {
                AssertCanSetClips();
                _UpRight = value;
                EasyAnimatorUtilities.SetDirty(this);
            }
        }

         

        [SerializeField]
        private AnimationClip _DownRight;

        public AnimationClip DownRight
        {
            get => _DownRight;
            set
            {
                AssertCanSetClips();
                _DownRight = value;
                EasyAnimatorUtilities.SetDirty(this);
            }
        }

         

        [SerializeField]
        private AnimationClip _DownLeft;

        public AnimationClip DownLeft
        {
            get => _DownLeft;
            set
            {
                AssertCanSetClips();
                _DownLeft = value;
                EasyAnimatorUtilities.SetDirty(this);
            }
        }

         

        [SerializeField]
        private AnimationClip _UpLeft;

        public AnimationClip UpLeft
        {
            get => _UpLeft;
            set
            {
                AssertCanSetClips();
                _UpLeft = value;
                EasyAnimatorUtilities.SetDirty(this);
            }
        }

         

        public override AnimationClip GetClip(Vector2 direction)
        {
            var angle = Mathf.Atan2(direction.y, direction.x);
            var octant = Mathf.RoundToInt(8 * angle / (2 * Mathf.PI) + 8) % 8;
            switch (octant)
            {
                case 0: return Right;
                case 1: return _UpRight;
                case 2: return Up;
                case 3: return _UpLeft;
                case 4: return Left;
                case 5: return _DownLeft;
                case 6: return Down;
                case 7: return _DownRight;
                default: throw new ArgumentOutOfRangeException("Invalid octant");
            }
        }

         
        #region Directions
         

       
        public static class Diagonals
        {
             

            public const float OneOverSqrt2 = 0.70710678118f;

            public static Vector2 UpRight => new Vector2(OneOverSqrt2, OneOverSqrt2);

            public static Vector2 DownRight => new Vector2(OneOverSqrt2, -OneOverSqrt2);

            public static Vector2 DownLeft => new Vector2(-OneOverSqrt2, -OneOverSqrt2);

            public static Vector2 UpLeft => new Vector2(-OneOverSqrt2, OneOverSqrt2);

             
        }

         

        public override int ClipCount => 8;

         

       
        public new enum Direction
        {
            Up,

            Right,

            Down,

            Left,

            UpRight,

            DownRight,

            DownLeft,

            UpLeft,
        }

         

        protected override string GetDirectionName(int direction) => ((Direction)direction).ToString();

         

        public AnimationClip GetClip(Direction direction)
        {
            switch (direction)
            {
                case Direction.Up: return Up;
                case Direction.Right: return Right;
                case Direction.Down: return Down;
                case Direction.Left: return Left;
                case Direction.UpRight: return _UpRight;
                case Direction.DownRight: return _DownRight;
                case Direction.DownLeft: return _DownLeft;
                case Direction.UpLeft: return _UpLeft;
                default: throw new ArgumentException($"Unsupported {nameof(Direction)}: {direction}");
            }
        }

        public override AnimationClip GetClip(int direction) => GetClip((Direction)direction);

         

        public void SetClip(Direction direction, AnimationClip clip)
        {
            switch (direction)
            {
                case Direction.Up: Up = clip; break;
                case Direction.Right: Right = clip; break;
                case Direction.Down: Down = clip; break;
                case Direction.Left: Left = clip; break;
                case Direction.UpRight: UpRight = clip; break;
                case Direction.DownRight: DownRight = clip; break;
                case Direction.DownLeft: DownLeft = clip; break;
                case Direction.UpLeft: UpLeft = clip; break;
                default: throw new ArgumentException($"Unsupported {nameof(Direction)}: {direction}");
            }
        }

        public override void SetClip(int direction, AnimationClip clip) => SetClip((Direction)direction, clip);

         

        public static Vector2 DirectionToVector(Direction direction)
        {
            switch (direction)
            {
                case Direction.Up: return Vector2.up;
                case Direction.Right: return Vector2.right;
                case Direction.Down: return Vector2.down;
                case Direction.Left: return Vector2.left;
                case Direction.UpRight: return Diagonals.UpRight;
                case Direction.DownRight: return Diagonals.DownRight;
                case Direction.DownLeft: return Diagonals.DownLeft;
                case Direction.UpLeft: return Diagonals.UpLeft;
                default: throw new ArgumentException($"Unsupported {nameof(Direction)}: {direction}");
            }
        }

        public override Vector2 GetDirection(int direction) => DirectionToVector((Direction)direction);

         

        public new static Direction VectorToDirection(Vector2 vector)
        {
            var angle = Mathf.Atan2(vector.y, vector.x);
            var octant = Mathf.RoundToInt(8 * angle / (2 * Mathf.PI) + 8) % 8;
            switch (octant)
            {
                case 0: return Direction.Right;
                case 1: return Direction.UpRight;
                case 2: return Direction.Up;
                case 3: return Direction.UpLeft;
                case 4: return Direction.Left;
                case 5: return Direction.DownLeft;
                case 6: return Direction.Down;
                case 7: return Direction.DownRight;
                default: throw new ArgumentOutOfRangeException("Invalid octant");
            }
        }

         

        public new static Vector2 SnapVectorToDirection(Vector2 vector)
        {
            var magnitude = vector.magnitude;
            var direction = VectorToDirection(vector);
            vector = DirectionToVector(direction) * magnitude;
            return vector;
        }

        public override Vector2 Snap(Vector2 vector) => SnapVectorToDirection(vector);

         
        #endregion
         
        #region Name Based Operations
         
#if UNITY_EDITOR
         

        public override int SetClipByName(AnimationClip clip)
        {
            var name = clip.name;

            var directionCount = ClipCount;
            for (int i = directionCount - 1; i >= 0; i--)
            {
                if (name.Contains(GetDirectionName(i)))
                {
                    SetClip(i, clip);
                    return i;
                }
            }

            return -1;
        }

         
#endif
         
        #endregion
         
    }
}
