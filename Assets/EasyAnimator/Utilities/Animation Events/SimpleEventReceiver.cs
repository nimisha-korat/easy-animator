

using UnityEngine;
using UnityEngine.Serialization;

namespace EasyAnimator
{
   
    [AddComponentMenu(Strings.MenuPrefix + "Simple Event Receiver")]
    [HelpURL(Strings.DocsURLs.APIDocumentation + "/" + nameof(SimpleEventReceiver))]
    public class SimpleEventReceiver : MonoBehaviour
    {
         

        [SerializeField, FormerlySerializedAs("onEvent")]
        private AnimationEventReceiver _OnEvent;

        public ref AnimationEventReceiver OnEvent => ref _OnEvent;

         

        private void Event(AnimationEvent animationEvent)
        {
            _OnEvent.SetFunctionName(nameof(Event));
            _OnEvent.HandleEvent(animationEvent);
        }

         
    }
}
