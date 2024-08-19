

#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace EasyAnimator.Editor
{
 
    public sealed class BoolPref
    {
         

        public const string KeyPrefix = nameof(EasyAnimator) + "/";

        public readonly string Key;

        public readonly string MenuItem;

        public readonly bool DefaultValue;

         

        private bool _HasValue;
        private bool _Value;

        public bool Value
        {
            get
            {
                if (!_HasValue)
                {
                    _HasValue = true;
                    _Value = EditorPrefs.GetBool(Key, DefaultValue);
                }

                return _Value;
            }
            set
            {
                if (_Value == value &&
                    _HasValue)
                    return;

                _Value = value;
                _HasValue = true;
                EditorPrefs.SetBool(Key, value);
            }
        }

        public static implicit operator bool(BoolPref pref) => pref.Value;

         

        public BoolPref(string menuItem, bool defaultValue)
            : this(null, menuItem, defaultValue) { }

        public BoolPref(string keyPrefix, string menuItem, bool defaultValue)
        {
            MenuItem = menuItem + " ?";
            Key = KeyPrefix + keyPrefix + menuItem;
            DefaultValue = defaultValue;
        }

         

        public void AddToggleFunction(GenericMenu menu)
        {
            menu.AddItem(new GUIContent(MenuItem), Value, () =>
            {
                Value = !Value;
            });
        }

         

        public override string ToString() => $"{nameof(BoolPref)} ({nameof(Key)} = '{Key}', {nameof(Value)} = {Value})";

         
    }
}

#endif

