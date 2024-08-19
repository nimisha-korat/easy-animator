

using System;
using UnityEngine;

namespace EasyAnimator
{
   
    public sealed class Float3ControllerState : ControllerState
    {
         

        public new interface ITransition : ITransition<Float3ControllerState> { }

         

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

         

        private ParameterID _ParameterZID;

        public ParameterID ParameterZID
        {
            get => _ParameterZID;
            set
            {
                _ParameterZID = value;
                _ParameterZID.ValidateHasParameter(Controller, AnimatorControllerParameterType.Float);
            }
        }

        public float ParameterZ
        {
            get => Playable.GetFloat(_ParameterZID.Hash);
            set => Playable.SetFloat(_ParameterZID.Hash, value);
        }

         

    
        public Vector3 Parameter
        {
            get => new Vector3(ParameterX, ParameterY, ParameterZ);
            set
            {
                ParameterX = value.x;
                ParameterY = value.y;
                ParameterZ = value.z;
            }
        }

         

        public Float3ControllerState(RuntimeAnimatorController controller,
            ParameterID parameterX, ParameterID parameterY, ParameterID parameterZ, bool keepStateOnStop = false)
            : base(controller, keepStateOnStop)
        {
            _ParameterXID = parameterX;
            _ParameterXID.ValidateHasParameter(Controller, AnimatorControllerParameterType.Float);

            _ParameterYID = parameterY;
            _ParameterYID.ValidateHasParameter(Controller, AnimatorControllerParameterType.Float);

            _ParameterZID = parameterZ;
            _ParameterZID.ValidateHasParameter(Controller, AnimatorControllerParameterType.Float);
        }

         

        public override int ParameterCount => 3;

        public override int GetParameterHash(int index)
        {
            switch (index)
            {
                case 0: return _ParameterXID;
                case 1: return _ParameterYID;
                case 2: return _ParameterZID;
                default: throw new ArgumentOutOfRangeException(nameof(index));
            };
        }

         
    }
}

