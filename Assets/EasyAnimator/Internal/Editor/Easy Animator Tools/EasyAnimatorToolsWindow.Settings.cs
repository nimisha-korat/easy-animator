

#if UNITY_EDITOR

using System;

namespace EasyAnimator.Editor
{
    partial class EasyAnimatorToolsWindow
    {
       
        internal sealed class Settings : Panel
        {
             

            public override string Name => "Settings";

            public override string Instructions => null;

            public override string HelpURL => Strings.DocsURLs.APIDocumentation + "." + nameof(Editor) + "/" + nameof(EasyAnimatorSettings);

             

            [NonSerialized]
            private UnityEditor.Editor _SettingsEditor;

             

            public override void OnEnable(int index)
            {
                base.OnEnable(index);

                var settings = EasyAnimatorSettings.Instance;
                if (settings != null)
                    _SettingsEditor = UnityEditor.Editor.CreateEditor(settings);
            }

            public override void OnDisable()
            {
                base.OnDisable();
                DestroyImmediate(_SettingsEditor);
            }

             

            public override void DoBodyGUI()
            {
                if (_SettingsEditor != null)
                    _SettingsEditor.OnInspectorGUI();
            }

             
        }
    }
}

#endif

