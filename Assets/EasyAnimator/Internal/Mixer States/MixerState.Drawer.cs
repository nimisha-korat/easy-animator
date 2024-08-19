
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value.

using System;

namespace EasyAnimator
{
   
    partial class MixerState
    {
         

        protected virtual int ParameterCount => 0;

        protected virtual string GetParameterName(int index) => throw new NotSupportedException();

        protected virtual UnityEngine.AnimatorControllerParameterType GetParameterType(int index) => throw new NotSupportedException();

        protected virtual object GetParameterValue(int index) => throw new NotSupportedException();

        protected virtual void SetParameterValue(int index, object value) => throw new NotSupportedException();

         
#if UNITY_EDITOR
         

        protected internal override Editor.IEasyAnimatorNodeDrawer CreateDrawer() => new Drawer<MixerState>(this);

         

        public class Drawer<T> : Editor.ParametizedEasyAnimatorStateDrawer<T> where T : MixerState
        {
             

            public Drawer(T state) : base(state) { }

             

            public override int ParameterCount => Target.ParameterCount;

            public override string GetParameterName(int index) => Target.GetParameterName(index);

            public override UnityEngine.AnimatorControllerParameterType GetParameterType(int index) => Target.GetParameterType(index);

            public override object GetParameterValue(int index) => Target.GetParameterValue(index);

            public override void SetParameterValue(int index, object value) => Target.SetParameterValue(index, value);

             
        }

         
#endif
         
    }
}

