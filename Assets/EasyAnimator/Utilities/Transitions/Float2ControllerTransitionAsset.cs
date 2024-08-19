
using System;
using UnityEngine;

namespace EasyAnimator
{
   
    [CreateAssetMenu(menuName = Strings.MenuPrefix + "Controller Transition/Float 2", order = Strings.AssetMenuOrder + 7)]
    [HelpURL(Strings.DocsURLs.APIDocumentation + "/" + nameof(Float2ControllerTransitionAsset))]
    public class Float2ControllerTransitionAsset : EasyAnimatorTransitionAsset<Float2ControllerTransition>
    {
        [Serializable]
        public class UnShared :
            EasyAnimatorTransitionAsset.UnShared<Float2ControllerTransitionAsset, Float2ControllerTransition, Float2ControllerState>,
            Float2ControllerState.ITransition
        { }
    }

    [Serializable]
    public class Float2ControllerTransition : ControllerTransition<Float2ControllerState>, Float2ControllerState.ITransition
    {
         

        [SerializeField]
        private string _ParameterNameX;

        public ref string ParameterNameX => ref _ParameterNameX;

         

        [SerializeField]
        private string _ParameterNameY;

        public ref string ParameterNameY => ref _ParameterNameY;

         

        public Float2ControllerTransition() { }

        public Float2ControllerTransition(RuntimeAnimatorController controller, string parameterNameX, string parameterNameY)
        {
            Controller = controller;
            _ParameterNameX = parameterNameX;
            _ParameterNameY = parameterNameY;
        }

         

        public override Float2ControllerState CreateState()
            => State = new Float2ControllerState(Controller, _ParameterNameX, _ParameterNameY, KeepStateOnStop);

         
        #region Drawer
#if UNITY_EDITOR
         

        [UnityEditor.CustomPropertyDrawer(typeof(Float2ControllerTransition), true)]
        public class Drawer : ControllerTransition.Drawer
        {
             

            public Drawer() : base(nameof(_ParameterNameX), nameof(_ParameterNameY)) { }

             
        }

         
#endif
        #endregion
         
    }
}
