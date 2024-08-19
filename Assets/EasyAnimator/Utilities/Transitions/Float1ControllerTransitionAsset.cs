

using System;
using UnityEngine;

namespace EasyAnimator
{

    [CreateAssetMenu(menuName = Strings.MenuPrefix + "Controller Transition/Float 1", order = Strings.AssetMenuOrder + 6)]
    [HelpURL(Strings.DocsURLs.APIDocumentation + "/" + nameof(Float1ControllerTransitionAsset))]
    public class Float1ControllerTransitionAsset : EasyAnimatorTransitionAsset<Float1ControllerTransition>
    {
        [Serializable]
        public class UnShared :
            EasyAnimatorTransitionAsset.UnShared<Float1ControllerTransitionAsset, Float1ControllerTransition, Float1ControllerState>,
            Float1ControllerState.ITransition
        { }
    }


    [Serializable]
    public class Float1ControllerTransition : ControllerTransition<Float1ControllerState>, Float1ControllerState.ITransition
    {
         

        [SerializeField]
        private string _ParameterName;

        public ref string ParameterName => ref _ParameterName;

         

        public Float1ControllerTransition() { }

        public Float1ControllerTransition(RuntimeAnimatorController controller, string parameterName)
        {
            Controller = controller;
            _ParameterName = parameterName;
        }

         

        public override Float1ControllerState CreateState()
            => State = new Float1ControllerState(Controller, _ParameterName, KeepStateOnStop);

         
        #region Drawer
#if UNITY_EDITOR
         

        [UnityEditor.CustomPropertyDrawer(typeof(Float1ControllerTransition), true)]
        public class Drawer : ControllerTransition.Drawer
        {
             

            public Drawer() : base(nameof(_ParameterName)) { }

             
        }

         
#endif
        #endregion
         
    }
}
