

namespace EasyAnimator
{
   
    public static partial class Validate
    {
         

       
        public enum Value
        {
            Any,

            ZeroToOne,

            IsNotNegative,

            IsFinite,

            IsFiniteOrNaN,
        }

         

        public static void ValueRule(ref float value, Value rule)
        {
            switch (rule)
            {
                case Value.Any:
                default:
                    return;

                case Value.ZeroToOne:
                    if (!(value >= 0))// Reversed comparison to include NaN.
                        value = 0;
                    else if (value > 1)
                        value = 1;
                    break;

                case Value.IsNotNegative:
                    if (!(value >= 0))// Reversed comparison to include NaN.
                        value = 0;
                    break;

                case Value.IsFinite:
                    if (float.IsNaN(value))
                        value = 0;
                    else if (float.IsPositiveInfinity(value))
                        value = float.MaxValue;
                    else if (float.IsNegativeInfinity(value))
                        value = float.MinValue;
                    break;

                case Value.IsFiniteOrNaN:
                    if (float.IsPositiveInfinity(value))
                        value = float.MaxValue;
                    else if (float.IsNegativeInfinity(value))
                        value = float.MinValue;
                    break;
            }
        }

         
    }
}

