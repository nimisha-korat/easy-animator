

using UnityEngine;
using UnityEngine.Playables;

namespace EasyAnimator
{
    
    public interface IPlayableWrapper
    {

        IPlayableWrapper Parent { get; }

        float Weight { get; }

        Playable Playable { get; }

        int ChildCount { get; }

        EasyAnimatorNode GetChild(int index);

       
        bool KeepChildrenConnected { get; }

       
        float Speed { get; set; }


      
        bool ApplyAnimatorIK { get; set; }


        bool ApplyFootIK { get; set; }

    }
}

 
#if UNITY_EDITOR
 

namespace EasyAnimator.Editor
{
   
    public static partial class EasyAnimatorEditorUtilities
    {
         

        public static void AddContextMenuIK(UnityEditor.GenericMenu menu, IPlayableWrapper ik)
        {
            menu.AddItem(new GUIContent("Inverse Kinematics/Apply Animator IK ?"),
                ik.ApplyAnimatorIK,
                () => ik.ApplyAnimatorIK = !ik.ApplyAnimatorIK);
            menu.AddItem(new GUIContent("Inverse Kinematics/Apply Foot IK ?"),
                ik.ApplyFootIK,
                () => ik.ApplyFootIK = !ik.ApplyFootIK);
        }

         
        public static void NormalizeChildWeights(this IPlayableWrapper parent)
        {
            var totalWeight = 0f;
            var childCount = parent.ChildCount;
            for (int i = 0; i < childCount; i++)
            {
                var child = parent.GetChild(i);
                if (child.IsValid())
                    totalWeight += child.Weight;
            }

            if (totalWeight == 0 ||// Can't normalize.
                Mathf.Approximately(totalWeight, 1))// Already normalized.
                return;

            totalWeight = 1f / totalWeight;
            for (int i = 0; i < childCount; i++)
            {
                var child = parent.GetChild(i);
                if (child.IsValid())
                    child.Weight *= totalWeight;
            }
        }

         
    }
}

 
#endif
 

