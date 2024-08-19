


#if UNITY_EDITOR

using UnityEditor;

namespace EasyAnimator.Editor
{
    public sealed class SerializedArrayProperty
    {
         

        private SerializedProperty _Property;

        public SerializedProperty Property
        {
            get => _Property;
            set
            {
                _Property = value;
                Refresh();
            }
        }

         

        private string _Path;

        public string Path => _Path ?? (_Path = Property.propertyPath);

         

        private int _Count;

        public int Count
        {
            get => _Count;
            set => Property.arraySize = _Count = value;
        }

         

        private bool _HasMultipleDifferentValues;
        private bool _GotHasMultipleDifferentValues;

        public bool HasMultipleDifferentValues
        {
            get
            {
                if (!_GotHasMultipleDifferentValues)
                {
                    _GotHasMultipleDifferentValues = true;
                    _HasMultipleDifferentValues = Property.hasMultipleDifferentValues;
                }

                return _HasMultipleDifferentValues;
            }
        }

         

        public void Refresh()
        {
            _Path = null;
            _Count = _Property != null ? _Property.arraySize : 0;
            _GotHasMultipleDifferentValues = false;
        }

         

        public SerializedProperty GetElement(int index)
        {
            var element = Property.GetArrayElementAtIndex(index);
            if (!HasMultipleDifferentValues || element.propertyPath.StartsWith(Path))
                return element;
            else
                return null;
        }

         
    }
}

#endif

