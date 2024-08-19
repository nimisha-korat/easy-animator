

using EasyAnimator.Units;
using UnityEngine;

namespace EasyAnimator
{
    
    public static class Strings
    {
         

        public const string ProductName = nameof(EasyAnimator);

        public const string MenuPrefix = ProductName + "/";

        public const string CreateMenuPrefix = "Assets/Create/" + MenuPrefix;

        public const string ExamplesMenuPrefix = MenuPrefix + "Examples/";

        public const string EasyAnimatorToolsMenuPath = "Window/Animation/EasyAnimator Tools";

        public const int AssetMenuOrder = 410;

         

        public const string UnityEditor = "UNITY_EDITOR";

        public const string Assertions = "UNITY_ASSERTIONS";

         

        public const string Indent = "    ";

        public const string ProOnlyTag = "";

         


        public static class DocsURLs
        {
             

            public const string Documentation = "";

            public const string APIDocumentation = Documentation + "/api/" + nameof(EasyAnimator);

            public const string ExampleAPIDocumentation = APIDocumentation + ".Examples.";

            public const string DeveloperEmail = "dev@EasyAnimator.com";

             

            public const string OptionalWarning = APIDocumentation + "/" + nameof(EasyAnimator.OptionalWarning);

             
#if UNITY_ASSERTIONS
             

            public const string Docs = Documentation + "/docs/";

            public const string EasyAnimatorEvents = Docs + "manual/events/EasyAnimator";
            public const string ClearAutomatically = EasyAnimatorEvents + "#clear-automatically";
            public const string SharedEventSequences = EasyAnimatorEvents + "#shared-event-sequences";
            public const string AnimatorControllers = Docs + "manual/animator-controllers";
            public const string AnimatorControllersNative = AnimatorControllers + "#native";

             
#endif
             
#if UNITY_EDITOR
             

            public const string Examples = Docs + "examples";
            public const string UnevenGround = Docs + "examples/ik/uneven-ground";

            public const string EasyAnimatorTools = Docs + "manual/tools";
            public const string PackTextures = EasyAnimatorTools + "/pack-textures";
            public const string ModifySprites = EasyAnimatorTools + "/modify-sprites";
            public const string RenameSprites = EasyAnimatorTools + "/rename-sprites";
            public const string GenerateSpriteAnimations = EasyAnimatorTools + "/generate-sprite-animations";
            public const string RemapSpriteAnimation = EasyAnimatorTools + "/remap-sprite-animation";
            public const string RemapAnimationBindings = EasyAnimatorTools + "/remap-animation-bindings";

            public const string Inspector = Docs + "manual/playing/inspector";
            public const string States = Docs + "manual/playing/states";

            public const string Fading = Docs + "manual/blending/fading";
            public const string Layers = Docs + "manual/blending/layers";

            public const string EndEvents = Docs + "manual/events/end";

            public const string TransitionPreviews = Docs + "manual/transitions/previews";

            public const string UpdateModes = Docs + "bugs/update-modes";

            public const string ChangeLogPrefix = Docs + "changes/EasyAnimator-";

            public const string Forum = "";

            public const string Issues = "";

             
#endif
             
        }

         

        public static class Tooltips
        {
             

            public const string MiddleClickReset =
                "\n• Middle Click = reset to default value";

            public const string FadeDuration = ProOnlyTag +
                "The amount of time the transition will take, e.g:" +
                "\n• 0s = Instant" +
                "\n• 0.25s = quarter of a second (Default)" +
                "\n• 0.25x = quarter of the animation length" +
                "\n• " + AnimationTimeAttribute.Tooltip +
                MiddleClickReset;

            public const string Speed = ProOnlyTag +
                "How fast the animation will play, e.g:" +
                "\n• 0x = paused" +
                "\n• 1x = normal speed" +
                "\n• -2x = double speed backwards";

            public const string OptionalSpeed = Speed +
                "\n• Disabled = keep previous speed" +
                MiddleClickReset;

            public const string NormalizedStartTime = ProOnlyTag +
                "• Enabled = always start at this time." +
                "\n• Disabled = continue from the current time." +
                "\n• " + AnimationTimeAttribute.Tooltip;

            public const string EndTime = ProOnlyTag +
                "The time when the End Callback will be triggered." +
                "\n• " + AnimationTimeAttribute.Tooltip +
                "\n\nDisabling the toggle automates the value:" +
                "\n• Speed >= 0 ends at 1x" +
                "\n• Speed < 0 ends at 0x";

            public const string CallbackTime = ProOnlyTag +
                "The time when the Event Callback will be triggered." +
                "\n• " + AnimationTimeAttribute.Tooltip;

             
        }

         
    }
}

