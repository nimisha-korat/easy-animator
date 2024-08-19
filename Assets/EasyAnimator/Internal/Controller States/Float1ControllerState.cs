

using System;
using UnityEngine;

namespace EasyAnimator
{
   
    public sealed class Float1ControllerState : ControllerState
    {
         

        public new interface ITransition : ITransition<Float1ControllerState> { }

         

        private ParameterID _ParameterID;

        public new ParameterID ParameterID
        {
            get => _ParameterID;
            set
            {
                _ParameterID = value;
                _ParameterID.ValidateHasParameter(Controller, AnimatorControllerParameterType.Float);
            }
        }

    
        public float Parameter
        {
            get => Playable.GetFloat(_ParameterID.Hash);
            set => Playable.SetFloat(_ParameterID.Hash, value);
        }

         

        public Float1ControllerState(RuntimeAnimatorController controller, ParameterID parameter,
            bool keepStateOnStop = false)
            : base(controller, keepStateOnStop)
        {
            _ParameterID = parameter;
            _ParameterID.ValidateHasParameter(controller, AnimatorControllerParameterType.Float);
        }

         

        public override int ParameterCount => 1;

        public override int GetParameterHash(int index) => _ParameterID;

         
    }
}

