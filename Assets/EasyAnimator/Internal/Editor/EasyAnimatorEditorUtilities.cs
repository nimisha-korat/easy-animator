

#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace EasyAnimator.Editor
{
    
    public static partial class EasyAnimatorEditorUtilities
    {
         
        #region Misc
         

        public const BindingFlags
            AnyAccessBindings = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static,
            InstanceBindings = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
            StaticBindings = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;

         

        public static TAttribute GetAttribute<TAttribute>(this ICustomAttributeProvider member, bool inherit = false)
            where TAttribute : Attribute
        {
            var type = typeof(TAttribute);
            if (member.IsDefined(type, inherit))
                return (TAttribute)member.GetCustomAttributes(type, inherit)[0];
            else
                return null;
        }

         

        public static bool IsNaN(this Vector2 vector) => float.IsNaN(vector.x) || float.IsNaN(vector.y);

        public static bool IsNaN(this Vector3 vector) => float.IsNaN(vector.x) || float.IsNaN(vector.y) || float.IsNaN(vector.z);

         

        public static T FindAssetOfType<T>() where T : Object
        {
            var filter = typeof(Component).IsAssignableFrom(typeof(T)) ? $"t:{nameof(GameObject)}" : $"t:{typeof(T).Name}";
            var guids = AssetDatabase.FindAssets(filter);
            if (guids.Length == 0)
                return null;

            for (int i = 0; i < guids.Length; i++)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[i]);
                var asset = AssetDatabase.LoadAssetAtPath<T>(path);
                if (asset != null)
                    return asset;
            }

            return null;
        }

         

        // The "g" format gives a lower case 'e' for exponentials instead of upper case 'E'.
        private static readonly ConversionCache<float, string>
            FloatToString = new ConversionCache<float, string>((value) => $"{value:g}");

        public static string ToStringCached(this float value) => FloatToString.Convert(value);

         

        public static PlayModeStateChange PlayModeState { get; private set; }

        public static bool IsChangingPlayMode =>
            PlayModeState == PlayModeStateChange.ExitingEditMode ||
            PlayModeState == PlayModeStateChange.ExitingPlayMode;

        [InitializeOnLoadMethod]
        private static void WatchForPlayModeChanges()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                PlayModeState = EditorApplication.isPlaying ?
                    PlayModeStateChange.EnteredPlayMode :
                    PlayModeStateChange.ExitingEditMode;

            EditorApplication.playModeStateChanged += (change) => PlayModeState = change;
        }

         
        #endregion
         
        #region Collections
         

        public static void SetCount<T>(List<T> list, int count)
        {
            if (list.Count < count)
            {
                while (list.Count < count)
                    list.Add(default);
            }
            else
            {
                list.RemoveRange(count, list.Count - count);
            }
        }

         

        public static bool RemoveMissingAndDuplicates(ref List<GameObject> list)
        {
            if (list == null)
            {
                list = new List<GameObject>();
                return false;
            }

            var modified = false;

            using (ObjectPool.Disposable.AcquireSet<Object>(out var previousItems))
            {
                for (int i = list.Count - 1; i >= 0; i--)
                {
                    var item = list[i];
                    if (item == null || previousItems.Contains(item))
                    {
                        list.RemoveAt(i);
                        modified = true;
                    }
                    else
                    {
                        previousItems.Add(item);
                    }
                }
            }

            return modified;
        }

         

        public static void RemoveDestroyedObjects<TKey, TValue>(Dictionary<TKey, TValue> dictionary) where TKey : Object
        {
            using (ObjectPool.Disposable.AcquireList<TKey>(out var oldObjects))
            {
                foreach (var obj in dictionary.Keys)
                {
                    if (obj == null)
                        oldObjects.Add(obj);
                }

                for (int i = 0; i < oldObjects.Count; i++)
                {
                    dictionary.Remove(oldObjects[i]);
                }
            }
        }

        public static bool InitializeCleanDictionary<TKey, TValue>(ref Dictionary<TKey, TValue> dictionary) where TKey : Object
        {
            if (dictionary == null)
            {
                dictionary = new Dictionary<TKey, TValue>();
                return true;
            }
            else
            {
                RemoveDestroyedObjects(dictionary);
                return false;
            }
        }

         
        #endregion
         
        #region Context Menus
         

        public static void AddFadeFunction(GenericMenu menu, string label, bool isEnabled, EasyAnimatorNode node, Action<float> startFade)
        {
            // Fade functions need to be delayed twice since the context menu itself causes the next frame delta
            // time to be unreasonably high (which would skip the start of the fade).
            menu.AddFunction(label, isEnabled,
                () => EditorApplication.delayCall +=
                () => EditorApplication.delayCall +=
                () =>
                {
                    startFade(node.CalculateEditorFadeDuration());
                });
        }

        public static float CalculateEditorFadeDuration(this EasyAnimatorNode node, float defaultDuration = 1)
            => node.FadeSpeed > 0 ? 1 / node.FadeSpeed : defaultDuration;

         

        public static void AddDocumentationLink(GenericMenu menu, string label, string linkSuffix)
        {
            if (linkSuffix[0] == '/')
                linkSuffix = Strings.DocsURLs.Documentation + linkSuffix;

            menu.AddItem(new GUIContent(label), false, () =>
            {
                EditorUtility.OpenWithDefaultApp(linkSuffix);
            });
        }

         

        [MenuItem("CONTEXT/" + nameof(AnimationClip) + "/Toggle Looping", validate = true)]
        [MenuItem("CONTEXT/" + nameof(AnimationClip) + "/Toggle Legacy", validate = true)]
        private static bool ValidateEditable(MenuCommand command)
        {
            return (command.context.hideFlags & HideFlags.NotEditable) != HideFlags.NotEditable;
        }

         

        [MenuItem("CONTEXT/" + nameof(AnimationClip) + "/Toggle Looping")]
        private static void ToggleLooping(MenuCommand command)
        {
            var clip = (AnimationClip)command.context;
            SetLooping(clip, !clip.isLooping);
        }

        public static void SetLooping(AnimationClip clip, bool looping)
        {
            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = looping;
            AnimationUtility.SetAnimationClipSettings(clip, settings);

            Debug.Log($"Set {clip.name} to be {(looping ? "Looping" : "Not Looping")}." +
                " Note that you may need to restart Unity for this change to take effect.", clip);

            // None of these let us avoid the need to restart Unity.
            //EditorUtility.SetDirty(clip);
            //AssetDatabase.SaveAssets();

            //var path = AssetDatabase.GetAssetPath(clip);
            //AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        }

         

        [MenuItem("CONTEXT/" + nameof(AnimationClip) + "/Toggle Legacy")]
        private static void ToggleLegacy(MenuCommand command)
        {
            var clip = (AnimationClip)command.context;
            clip.legacy = !clip.legacy;
        }

         

        [MenuItem("CONTEXT/" + nameof(Animator) + "/Restore Bind Pose", priority = 110)]
        private static void RestoreBindPose(MenuCommand command)
        {
            var animator = (Animator)command.context;

            Undo.RegisterFullObjectHierarchyUndo(animator.gameObject, "Restore bind pose");

            const string TypeName = "UnityEditor.AvatarSetupTool, UnityEditor";
            var type = Type.GetType(TypeName);
            if (type == null)
                throw new TypeLoadException($"Unable to find the type '{TypeName}'");

            const string MethodName = "SampleBindPose";
            var method = type.GetMethod(MethodName, StaticBindings);
            if (method == null)
                throw new MissingMethodException($"Unable to find the method '{MethodName}'");

            method.Invoke(null, new object[] { animator.gameObject });
        }

         
        #endregion
         
        #region Type Names
         

        private static readonly Dictionary<Type, string>
            TypeNames = new Dictionary<Type, string>
            {
                { typeof(object), "object" },
                { typeof(void), "void" },
                { typeof(bool), "bool" },
                { typeof(byte), "byte" },
                { typeof(sbyte), "sbyte" },
                { typeof(char), "char" },
                { typeof(string), "string" },
                { typeof(short), "short" },
                { typeof(int), "int" },
                { typeof(long), "long" },
                { typeof(ushort), "ushort" },
                { typeof(uint), "uint" },
                { typeof(ulong), "ulong" },
                { typeof(float), "float" },
                { typeof(double), "double" },
                { typeof(decimal), "decimal" },
            };

        private static readonly Dictionary<Type, string>
            FullTypeNames = new Dictionary<Type, string>(TypeNames);

         

        public static string GetNameCS(this Type type, bool fullName = true)
        {
            if (type == null)
                return "null";

            // Check if we have already got the name for that type.
            var names = fullName ? FullTypeNames : TypeNames;

            if (names.TryGetValue(type, out var name))
                return name;

            var text = ObjectPool.AcquireStringBuilder();

            if (type.IsArray)// Array = TypeName[].
            {
                text.Append(type.GetElementType().GetNameCS(fullName));

                text.Append('[');
                var dimensions = type.GetArrayRank();
                while (dimensions-- > 1)
                    text.Append(',');
                text.Append(']');

                goto Return;
            }

            if (type.IsPointer)// Pointer = TypeName*.
            {
                text.Append(type.GetElementType().GetNameCS(fullName));
                text.Append('*');

                goto Return;
            }

            if (type.IsGenericParameter)// Generic Parameter = TypeName (for unspecified generic parameters).
            {
                text.Append(type.Name);
                goto Return;
            }

            var underlyingType = Nullable.GetUnderlyingType(type);
            if (underlyingType != null)// Nullable = TypeName != null ?
            {
                text.Append(underlyingType.GetNameCS(fullName));
                text.Append('?');

                goto Return;
            }

            // Other Type = Namespace.NestedTypes.TypeName<GenericArguments>.

            if (fullName && type.Namespace != null)// Namespace.
            {
                text.Append(type.Namespace);
                text.Append('.');
            }

            var genericArguments = 0;

            if (type.DeclaringType != null)// Account for Nested Types.
            {
                // Count the nesting level.
                var nesting = 1;
                var declaringType = type.DeclaringType;
                while (declaringType.DeclaringType != null)
                {
                    declaringType = declaringType.DeclaringType;
                    nesting++;
                }

                // Append the name of each outer type, starting from the outside.
                while (nesting-- > 0)
                {
                    // Walk out to the current nesting level.
                    // This avoids the need to make a list of types in the nest or to insert type names instead of appending them.
                    declaringType = type;
                    for (int i = nesting; i >= 0; i--)
                        declaringType = declaringType.DeclaringType;

                    // Nested Type Name.
                    genericArguments = AppendNameAndGenericArguments(text, declaringType, fullName, genericArguments);
                    text.Append('.');
                }
            }

            // Type Name.
            AppendNameAndGenericArguments(text, type, fullName, genericArguments);

            Return:// Remember and return the name.
            name = text.ReleaseToString();
            names.Add(type, name);
            return name;
        }

         

        public static int AppendNameAndGenericArguments(StringBuilder text, Type type, bool fullName = true, int skipGenericArguments = 0)
        {
            var name = type.Name;
            text.Append(name);

            if (type.IsGenericType)
            {
                var backQuote = name.IndexOf('`');
                if (backQuote >= 0)
                {
                    text.Length -= name.Length - backQuote;

                    var genericArguments = type.GetGenericArguments();
                    if (skipGenericArguments < genericArguments.Length)
                    {
                        text.Append('<');

                        var firstArgument = genericArguments[skipGenericArguments];
                        skipGenericArguments++;

                        if (firstArgument.IsGenericParameter)
                        {
                            while (skipGenericArguments < genericArguments.Length)
                            {
                                text.Append(',');
                                skipGenericArguments++;
                            }
                        }
                        else
                        {
                            text.Append(firstArgument.GetNameCS(fullName));

                            while (skipGenericArguments < genericArguments.Length)
                            {
                                text.Append(", ");
                                text.Append(genericArguments[skipGenericArguments].GetNameCS(fullName));
                                skipGenericArguments++;
                            }
                        }

                        text.Append('>');
                    }
                }
            }

            return skipGenericArguments;
        }

         
        #endregion
         
        #region Dummy EasyAnimator Component
         

        public sealed class DummyEasyAnimatorComponent : IEasyAnimatorComponent
        {
             

            public DummyEasyAnimatorComponent(Animator animator, EasyAnimatorPlayable playable)
            {
                Animator = animator;
                Playable = playable;
                InitialUpdateMode = animator.updateMode;
            }

             

            public bool enabled => true;

            public GameObject gameObject => Animator.gameObject;

            public Animator Animator { get; set; }

            public EasyAnimatorPlayable Playable { get; private set; }

            public bool IsPlayableInitialized => true;

            public bool ResetOnDisable => false;

            public AnimatorUpdateMode UpdateMode
            {
                get => Animator.updateMode;
                set => Animator.updateMode = value;
            }

             

            public object GetKey(AnimationClip clip) => clip;

             

            public string AnimatorFieldName => null;

            public string ActionOnDisableFieldName => null;

            public AnimatorUpdateMode? InitialUpdateMode { get; private set; }

             
        }

         
        #endregion
         
    }
}

#endif

