
#if UNITY_EDITOR

using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

// Shared File Last Modified: 2020-05-17.
namespace EasyAnimator.Editor
// namespace InspectorGadgets.Editor
{
    public partial class Serialization
    {
        [Serializable]
        public sealed class ObjectReference
        {
             

            [SerializeField] private Object _Object;
            [SerializeField] private int _InstanceID;

             

            public Object Object
            {
                get
                {
                    Initialize();
                    return _Object;
                }
            }

            public int InstanceID => _InstanceID;

             

            public ObjectReference(Object obj)
            {
                _Object = obj;
                if (obj != null)
                    _InstanceID = obj.GetInstanceID();
            }

             

            private void Initialize()
            {
                if (_Object == null)
                    _Object = EditorUtility.InstanceIDToObject(_InstanceID);
                else
                    _InstanceID = _Object.GetInstanceID();
            }

             

            public static implicit operator ObjectReference(Object obj) => new ObjectReference(obj);

            public static implicit operator Object(ObjectReference reference) => reference.Object;

             

            public static ObjectReference[] Convert(params Object[] objects)
            {
                var references = new ObjectReference[objects.Length];
                for (int i = 0; i < objects.Length; i++)
                    references[i] = objects[i];
                return references;
            }

            public static Object[] Convert(params ObjectReference[] references)
            {
                var objects = new Object[references.Length];
                for (int i = 0; i < references.Length; i++)
                    objects[i] = references[i];
                return objects;
            }

             

            public static bool AreSameObjects(ObjectReference[] references, Object[] objects)
            {
                if (references == null)
                    return objects == null;

                if (objects == null)
                    return false;

                if (references.Length != objects.Length)
                    return false;

                for (int i = 0; i < references.Length; i++)
                {
                    if (references[i] != objects[i])
                        return false;
                }

                return true;
            }

             

            public override string ToString() => "Serialization.ObjectReference [" + _InstanceID + "] " + _Object;

             
        }

         

        public static bool IsValid(this ObjectReference reference) => reference?.Object != null;

         
    }
}

#endif
