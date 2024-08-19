
namespace EasyAnimator
{
   
    public class DefaultFadeValueAttribute : DefaultValueAttribute
    {
         

        public override object Primary => EasyAnimatorPlayable.DefaultFadeDuration;

         

        public DefaultFadeValueAttribute()
        {
            // This won't change so there's no need to box the value every time by overriding the property.
            Secondary = 0f;
        }

         
    }
}

