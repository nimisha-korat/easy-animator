

using System;
using UnityEngine;

namespace EasyAnimator
{
   
    [CreateAssetMenu(menuName = Strings.MenuPrefix + "Controller Transition/Float 3", order = Strings.AssetMenuOrder + 8)]
    [HelpURL(Strings.DocsURLs.APIDocumentation + "/" + nameof(Float3ControllerTransitionAsset))]
    public class Float3ControllerTransitionAsset : EasyAnimatorTransitionAsset<Float3ControllerTransition>
    {
        [Serializable]
        public class UnShared :
            EasyAnimatorTransitionAsset.UnShared<Float3ControllerTransitionAsset, Float3ControllerTransition, Float3ControllerState>,
            Float3ControllerState.ITransition
        { }
    }

    [Serializable]
    public class Float3ControllerTransition : ControllerTransition<Float3ControllerState>, Float3ControllerState.ITransition
    {
         

        [SerializeField]
        private string _ParameterNameX;

        public ref string ParameterNameX => ref _ParameterNameX;

         

        [SerializeField]
        private string _ParameterNameY;

        public ref string ParameterNameY => ref _ParameterNameY;

         

        [SerializeField]
        private string _ParameterNameZ;

        public ref string ParameterNameZ => ref _ParameterNameZ;

         

        public Float3ControllerTransition() { }

        public Float3ControllerTransition(RuntimeAnimatorController controller,
            string parameterNameX, string parameterNameY, string parameterNameZ)
        {
            Controller = controller;
            _ParameterNameX = parameterNameX;
            _ParameterNameY = parameterNameY;
            _ParameterNameZ = parameterNameZ;
        }

         

        public override Float3ControllerState CreateState()
            => State = new Float3ControllerState(Controller, _ParameterNameX, _ParameterNameY, _ParameterNameZ, KeepStateOnStop);

         
        #region Drawer
#if UNITY_EDITOR
         

        [UnityEditor.CustomPropertyDrawer(typeof(Float3ControllerTransition), true)]
        public class Drawer : ControllerTransition.Drawer
        {
             

            public Drawer() : base(nameof(_ParameterNameX), nameof(_ParameterNameY), nameof(_ParameterNameZ)) { }

             
        }

         
#endif
        #endregion
         
    }
}
