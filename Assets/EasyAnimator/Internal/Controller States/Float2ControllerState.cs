

using System;
using UnityEngine;

namespace EasyAnimator
{
     
    public sealed class Float2ControllerState : ControllerState
    {
         

        public new interface ITransition : ITransition<Float2ControllerState> { }

         

        private ParameterID _ParameterXID;

        public ParameterID ParameterXID
        {
            get => _ParameterXID;
            set
            {
                _ParameterXID = value;
                _ParameterXID.ValidateHasParameter(Controller, AnimatorControllerParameterType.Float);
            }
        }

      
        public float ParameterX
        {
            get => Playable.GetFloat(_ParameterXID.Hash);
            set => Playable.SetFloat(_ParameterXID.Hash, value);
        }

         

        private ParameterID _ParameterYID;

        public ParameterID ParameterYID
        {
            get => _ParameterYID;
            set
            {
                _ParameterYID = value;
                _ParameterYID.ValidateHasParameter(Controller, AnimatorControllerParameterType.Float);
            }
        }

      
        public float ParameterY
        {
            get => Playable.GetFloat(_ParameterYID.Hash);
            set => Playable.SetFloat(_ParameterYID.Hash, value);
        }

         

       
        public Vector2 Parameter
        {
            get => new Vector2(ParameterX, ParameterY);
            set
            {
                ParameterX = value.x;
                ParameterY = value.y;
            }
        }

         

        public Float2ControllerState(RuntimeAnimatorController controller,
            ParameterID parameterX, ParameterID parameterY, bool keepStateOnStop = false)
            : base(controller, keepStateOnStop)
        {
            _ParameterXID = parameterX;
            _ParameterXID.ValidateHasParameter(Controller, AnimatorControllerParameterType.Float);

            _ParameterYID = parameterY;
            _ParameterYID.ValidateHasParameter(Controller, AnimatorControllerParameterType.Float);
        }

         

        public override int ParameterCount => 2;

        public override int GetParameterHash(int index)
        {
            switch (index)
            {
                case 0: return _ParameterXID;
                case 1: return _ParameterYID;
                default: throw new ArgumentOutOfRangeException(nameof(index));
            };
        }

         
    }
}

