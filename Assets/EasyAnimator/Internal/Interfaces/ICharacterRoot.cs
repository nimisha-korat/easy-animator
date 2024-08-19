

using UnityEngine;

namespace EasyAnimator
{
  
    public interface ICharacterRoot
    {
       
              Transform transform { get; }

       }
}

#if UNITY_EDITOR

namespace EasyAnimator.Editor
{
   
    partial class EasyAnimatorEditorUtilities
    {

      
        public static Transform FindRoot(GameObject gameObject)
        {
            var root = gameObject.GetComponentInParent<ICharacterRoot>();
            if (root != null)
                return root.transform;

#if UNITY_EDITOR
            var path = UnityEditor.AssetDatabase.GetAssetPath(gameObject);
            if (!string.IsNullOrEmpty(path))
                return gameObject.transform.root;

            var status = UnityEditor.PrefabUtility.GetPrefabInstanceStatus(gameObject);
            if (status != UnityEditor.PrefabInstanceStatus.NotAPrefab)
            {
                gameObject = UnityEditor.PrefabUtility.GetOutermostPrefabInstanceRoot(gameObject);
                return gameObject.transform;
            }
#endif

            var animators = ObjectPool.AcquireList<Animator>();
            gameObject.GetComponentsInChildren(true, animators);
            var animatorCount = animators.Count;

            var parent = gameObject.transform;
            while (parent.parent != null)
            {
                animators.Clear();
                parent.parent.GetComponentsInChildren(true, animators);

                if (animatorCount == 0)
                    animatorCount = animators.Count;
                else if (animatorCount != animators.Count)
                    break;

                parent = parent.parent;
            }

            ObjectPool.Release(animators);

            return parent;
        }

       
        public static Transform FindRoot(Object obj)
        {
            if (obj is ICharacterRoot iRoot)
                return iRoot.transform;

            return TryGetGameObject(obj, out var gameObject) ? FindRoot(gameObject) : null;
        }

       
        public static bool TryGetGameObject(Object obj, out GameObject gameObject)
        {
            if (obj is GameObject go)
            {
                gameObject = go;
                return true;
            }

            if (obj is Component component)
            {
                gameObject = component.gameObject;
                return true;
            }

            gameObject = null;
            return false;
        }

    }
}

#endif

