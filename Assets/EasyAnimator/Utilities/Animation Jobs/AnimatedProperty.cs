
using System;
using UnityEngine;
using UnityEngine.Animations;
using Unity.Collections;

namespace EasyAnimator
{
 
    public abstract class AnimatedProperty<TJob, TValue> : EasyAnimatorJob<TJob>, IDisposable
        where TJob : struct, IAnimationJob
        where TValue : struct
    {
         

        protected NativeArray<PropertyStreamHandle> _Properties;

        protected NativeArray<TValue> _Values;

         
        #region Initialisation
         

        public AnimatedProperty(IEasyAnimatorComponent EasyAnimator, int propertyCount,
            NativeArrayOptions options = NativeArrayOptions.ClearMemory)
        {
            _Properties = new NativeArray<PropertyStreamHandle>(propertyCount, Allocator.Persistent, options);
            _Values = new NativeArray<TValue>(propertyCount, Allocator.Persistent);
            CreateJob();

            var playable = EasyAnimator.Playable;
            CreatePlayable(playable);
            playable.Disposables.Add(this);
        }

        public AnimatedProperty(IEasyAnimatorComponent EasyAnimator, string propertyName)
            : this(EasyAnimator, 1, NativeArrayOptions.UninitializedMemory)
        {
            var animator = EasyAnimator.Animator;
            _Properties[0] = animator.BindStreamProperty(animator.transform, typeof(Animator), propertyName);
        }

        public AnimatedProperty(IEasyAnimatorComponent EasyAnimator, params string[] propertyNames)
            : this(EasyAnimator, propertyNames.Length, NativeArrayOptions.UninitializedMemory)
        {
            var count = propertyNames.Length;

            var animator = EasyAnimator.Animator;
            var transform = animator.transform;
            for (int i = 0; i < count; i++)
                InitializeProperty(animator, i, transform, typeof(Animator), propertyNames[i]);
        }

         

        public void InitializeProperty(Animator animator, int index, string name)
            => InitializeProperty(animator, index, animator.transform, typeof(Animator), name);

        public void InitializeProperty(Animator animator, int index, Transform transform, Type type, string name)
            => _Properties[index] = animator.BindStreamProperty(transform, type, name);

         

        protected abstract void CreateJob();

         
        #endregion
         
        #region Accessors
         

        public TValue Value => this[0];

        public static implicit operator TValue(AnimatedProperty<TJob, TValue> properties) => properties[0];

         

        public TValue GetValue(int index) => _Values[index];

        public TValue this[int index] => _Values[index];

         

        public void GetValues(ref TValue[] values)
        {
            EasyAnimatorUtilities.SetLength(ref values, _Values.Length);
            _Values.CopyTo(values);
        }

        public TValue[] GetValues()
        {
            var values = new TValue[_Values.Length];
            _Values.CopyTo(values);
            return values;
        }

         
        #endregion
         

        void IDisposable.Dispose() => Dispose();

        protected virtual void Dispose()
        {
            if (_Properties.IsCreated)
            {
                _Properties.Dispose();
                _Values.Dispose();
            }
        }

        public override void Destroy()
        {
            Dispose();
            base.Destroy();
        }

         
    }
}
