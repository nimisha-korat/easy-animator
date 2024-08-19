

using System;

#if UNITY_EDITOR
using EasyAnimator.Editor;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using System.Collections;
#endif

namespace EasyAnimator
{
   
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Class | AttributeTargets.Struct, Inherited = true)]
    [System.Diagnostics.Conditional(Strings.UnityEditor)]
    public sealed class EventNamesAttribute : Attribute
    {
         

#if UNITY_EDITOR
        public readonly string[] Names;
#endif

         

      
        public EventNamesAttribute(params string[] names)
        {
#if UNITY_EDITOR
            if (names == null)
                throw new ArgumentNullException(nameof(names));
            else if (names.Length == 0)
                throw new ArgumentOutOfRangeException(nameof(names), "Array must not be empty");

            Names = AddSpecialItems(names);
#endif
        }

         

      
        public EventNamesAttribute(Type type)
        {
#if UNITY_EDITOR
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (type.IsEnum)
            {
                Names = Enum.GetNames(type);
            }
            else
            {
                Names = GatherNamesFromStaticFields(type);
            }

            Names = AddSpecialItems(Names);
#endif
        }

         

       
        public EventNamesAttribute(Type type, string name)
        {
#if UNITY_EDITOR
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            object obj;

            var field = type.GetField(name, EasyAnimatorEditorUtilities.StaticBindings);
            if (field != null)
            {
                obj = field.GetValue(null) as IEnumerable;
                goto GotCollection;
            }

            var property = type.GetProperty(name, EasyAnimatorEditorUtilities.StaticBindings);
            if (property != null)
            {
                obj = property.GetValue(null, null) as IEnumerable;
                goto GotCollection;
            }

            var method = type.GetMethod(name, EasyAnimatorEditorUtilities.StaticBindings, null, Type.EmptyTypes, null);
            if (method != null)
            {
                obj = method.Invoke(null, null) as IEnumerable;
                goto GotCollection;
            }

            throw new ArgumentException($"{type.GetNameCS()} does not contain a member named '{name}'");

            GotCollection:
            if (obj == null)
                throw new ArgumentException($"The collection retrieved from {type.GetNameCS()}.{name} is null");
            if (!(obj is IEnumerable collection))
                throw new ArgumentException($"The object retrieved from {type.GetNameCS()}.{name} is not an {nameof(IEnumerable)}");

            using (ObjectPool.Disposable.AcquireList<string>(out var names))
            {
                names.Add(NoName);
                foreach (var item in collection)
                {
                    if (item == null)
                        continue;

                    var itemName = item.ToString();
                    if (string.IsNullOrEmpty(itemName))
                        continue;

                    names.Add(itemName);
                }

                if (names.Count == 1)
                    throw new ArgumentException($"The collection retrieved from {type.GetNameCS()}.{name} is empty");

                Names = names.ToArray();
            }
#endif
        }

         
#if UNITY_EDITOR
         

        public const string NoName = " ";

         

        private static string[] AddSpecialItems(string[] names)
        {
            if (names == null)
                return null;

            var newNames = new string[names.Length + 1];
            newNames[0] = NoName;
            Array.Copy(names, 0, newNames, 1, names.Length);
            return newNames;
        }

         

        private static string[] GatherNamesFromStaticFields(Type type)
        {
            using (ObjectPool.Disposable.AcquireList<string>(out var names))
            {
                var fields = type.GetFields(EasyAnimatorEditorUtilities.StaticBindings);
                for (int i = 0; i < fields.Length; i++)
                {
                    var field = fields[i];
                    if (field.FieldType == typeof(string))
                    {
                        var name = (string)field.GetValue(null);
                        if (name != null && !names.Contains(name))
                            names.Add(name);
                    }
                }

                if (names.Count > 0)
                    return names.ToArray();
                else
                    return null;
            }
        }

         

        public static string[] GetNames(SerializedProperty property)
        {
            var accessor = property.GetAccessor();
            while (accessor != null)
            {
                var field = accessor.GetField(property);
                var names = GetNames(field);
                if (names != null && names.Length > 0)
                    return names;

                var value = accessor.GetValue(property);
                if (value != null)
                {
                    names = GetNames(value.GetType());
                    if (names != null && names.Length > 0)
                        return names;
                }

                accessor = accessor.Parent;
            }

            // If none of the fields of types they are declared in have names, try the actual type of the target.
            {
                var names = GetNames(property.serializedObject.targetObject.GetType());
                if (names != null && names.Length > 0)
                    return names;
            }

            return null;
        }

         

        private static readonly Dictionary<MemberInfo, string[]>
            MemberToNames = new Dictionary<MemberInfo, string[]>();

        public static string[] GetNames(MemberInfo member)
        {
            if (!MemberToNames.TryGetValue(member, out var names))
            {
                try
                {
                    var attribute = member.GetAttribute<EventNamesAttribute>();
                    if (attribute != null)
                        names = attribute.Names;
                }
                catch (Exception exception)
                {
                    var name = member is Type type ?
                        $"{type.GetNameCS()}" :
                        $"{member.DeclaringType.GetNameCS()}.{member.Name}";

                    Debug.LogError($"{exception.GetType()} thrown by [EventNames] {name}: {exception.Message}");
                }

                MemberToNames.Add(member, names);
            }

            return names;
        }

        public static string[] GetNames(FieldInfo field)
        {
            var names = GetNames((MemberInfo)field);
            if (names != null)
                return names;

            names = GetNames(field.FieldType);
            if (names != null)
                return MemberToNames[field] = names;

            names = GetNames(field.DeclaringType);
            if (names != null)
                return MemberToNames[field] = names;

            return names;
        }

         
#endif
         
    }
}

