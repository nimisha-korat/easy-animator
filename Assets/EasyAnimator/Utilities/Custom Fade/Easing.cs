
using System;
using static UnityEngine.Mathf;
using NormalizedDelegate = System.Func<float, float>;

namespace EasyAnimator
{

    public static class Easing
    {
         
        #region Delegates
         

        public const float Ln2 = 0.693147180559945f;

         

        public delegate float RangedDelegate(float start, float end, float value);

         

      
        public enum Function
        {
            Linear,

            QuadraticIn,
            QuadraticOut,
            QuadraticInOut,

            CubicIn,
            CubicOut,
            CubicInOut,

            QuarticIn,
            QuarticOut,
            QuarticInOut,

            QuinticIn,
            QuinticOut,
            QuinticInOut,

            SineIn,
            SineOut,
            SineInOut,

            ExponentialIn,
            ExponentialOut,
            ExponentialInOut,

            CircularIn,
            CircularOut,
            CircularInOut,

            BackIn,
            BackOut,
            BackInOut,

            BounceIn,
            BounceOut,
            BounceInOut,

            ElasticIn,
            ElasticOut,
            ElasticInOut,
        }

        public const int FunctionCount = (int)Function.ElasticInOut + 1;

         

        private static NormalizedDelegate[] _FunctionDelegates;

        public static NormalizedDelegate GetDelegate(this Function function)
        {
            var i = (int)function;
            NormalizedDelegate del;

            if (_FunctionDelegates == null)
            {
                _FunctionDelegates = new NormalizedDelegate[FunctionCount];
            }
            else
            {
                del = _FunctionDelegates[i];
                if (del != null)
                    return del;
            }

            switch (function)
            {
                case Function.Linear: del = Linear; break;
                case Function.QuadraticIn: del = Quadratic.In; break;
                case Function.QuadraticOut: del = Quadratic.Out; break;
                case Function.QuadraticInOut: del = Quadratic.InOut; break;
                case Function.CubicIn: del = Cubic.In; break;
                case Function.CubicOut: del = Cubic.Out; break;
                case Function.CubicInOut: del = Cubic.InOut; break;
                case Function.QuarticIn: del = Quartic.In; break;
                case Function.QuarticOut: del = Quartic.Out; break;
                case Function.QuarticInOut: del = Quartic.InOut; break;
                case Function.QuinticIn: del = Quintic.In; break;
                case Function.QuinticOut: del = Quintic.Out; break;
                case Function.QuinticInOut: del = Quintic.InOut; break;
                case Function.SineIn: del = Sine.In; break;
                case Function.SineOut: del = Sine.Out; break;
                case Function.SineInOut: del = Sine.InOut; break;
                case Function.ExponentialIn: del = Exponential.In; break;
                case Function.ExponentialOut: del = Exponential.Out; break;
                case Function.ExponentialInOut: del = Exponential.InOut; break;
                case Function.CircularIn: del = Circular.In; break;
                case Function.CircularOut: del = Circular.Out; break;
                case Function.CircularInOut: del = Circular.InOut; break;
                case Function.BackIn: del = Back.In; break;
                case Function.BackOut: del = Back.Out; break;
                case Function.BackInOut: del = Back.InOut; break;
                case Function.BounceIn: del = Bounce.In; break;
                case Function.BounceOut: del = Bounce.Out; break;
                case Function.BounceInOut: del = Bounce.InOut; break;
                case Function.ElasticIn: del = Elastic.In; break;
                case Function.ElasticOut: del = Elastic.Out; break;
                case Function.ElasticInOut: del = Elastic.InOut; break;
                default: throw new ArgumentOutOfRangeException(nameof(function));
            }

            _FunctionDelegates[i] = del;
            return del;
        }

         

        private static NormalizedDelegate[] _DerivativeDelegates;

        public static NormalizedDelegate GetDerivativeDelegate(this Function function)
        {
            var i = (int)function;
            NormalizedDelegate del;

            if (_DerivativeDelegates == null)
            {
                _DerivativeDelegates = new NormalizedDelegate[FunctionCount];
            }
            else
            {
                del = _DerivativeDelegates[i];
                if (del != null)
                    return del;
            }

            switch (function)
            {
                case Function.Linear: del = LinearDerivative; break;
                case Function.QuadraticIn: del = Quadratic.InDerivative; break;
                case Function.QuadraticOut: del = Quadratic.OutDerivative; break;
                case Function.QuadraticInOut: del = Quadratic.InOutDerivative; break;
                case Function.CubicIn: del = Cubic.InDerivative; break;
                case Function.CubicOut: del = Cubic.OutDerivative; break;
                case Function.CubicInOut: del = Cubic.InOutDerivative; break;
                case Function.QuarticIn: del = Quartic.InDerivative; break;
                case Function.QuarticOut: del = Quartic.OutDerivative; break;
                case Function.QuarticInOut: del = Quartic.InOutDerivative; break;
                case Function.QuinticIn: del = Quintic.InDerivative; break;
                case Function.QuinticOut: del = Quintic.OutDerivative; break;
                case Function.QuinticInOut: del = Quintic.InOutDerivative; break;
                case Function.SineIn: del = Sine.InDerivative; break;
                case Function.SineOut: del = Sine.OutDerivative; break;
                case Function.SineInOut: del = Sine.InOutDerivative; break;
                case Function.ExponentialIn: del = Exponential.InDerivative; break;
                case Function.ExponentialOut: del = Exponential.OutDerivative; break;
                case Function.ExponentialInOut: del = Exponential.InOutDerivative; break;
                case Function.CircularIn: del = Circular.InDerivative; break;
                case Function.CircularOut: del = Circular.OutDerivative; break;
                case Function.CircularInOut: del = Circular.InOutDerivative; break;
                case Function.BackIn: del = Back.InDerivative; break;
                case Function.BackOut: del = Back.OutDerivative; break;
                case Function.BackInOut: del = Back.InOutDerivative; break;
                case Function.BounceIn: del = Bounce.InDerivative; break;
                case Function.BounceOut: del = Bounce.OutDerivative; break;
                case Function.BounceInOut: del = Bounce.InOutDerivative; break;
                case Function.ElasticIn: del = Elastic.InDerivative; break;
                case Function.ElasticOut: del = Elastic.OutDerivative; break;
                case Function.ElasticInOut: del = Elastic.InOutDerivative; break;
                default: throw new ArgumentOutOfRangeException(nameof(function));
            }

            _DerivativeDelegates[i] = del;
            return del;
        }

         

        private static RangedDelegate[] _RangedFunctionDelegates;

        public static RangedDelegate GetRangedDelegate(this Function function)
        {
            var i = (int)function;
            RangedDelegate del;

            if (_RangedFunctionDelegates == null)
            {
                _RangedFunctionDelegates = new RangedDelegate[FunctionCount];
            }
            else
            {
                del = _RangedFunctionDelegates[i];
                if (del != null)
                    return del;
            }

            switch (function)
            {
                case Function.Linear: del = Linear; break;
                case Function.QuadraticIn: del = Quadratic.In; break;
                case Function.QuadraticOut: del = Quadratic.Out; break;
                case Function.QuadraticInOut: del = Quadratic.InOut; break;
                case Function.CubicIn: del = Cubic.In; break;
                case Function.CubicOut: del = Cubic.Out; break;
                case Function.CubicInOut: del = Cubic.InOut; break;
                case Function.QuarticIn: del = Quartic.In; break;
                case Function.QuarticOut: del = Quartic.Out; break;
                case Function.QuarticInOut: del = Quartic.InOut; break;
                case Function.QuinticIn: del = Quintic.In; break;
                case Function.QuinticOut: del = Quintic.Out; break;
                case Function.QuinticInOut: del = Quintic.InOut; break;
                case Function.SineIn: del = Sine.In; break;
                case Function.SineOut: del = Sine.Out; break;
                case Function.SineInOut: del = Sine.InOut; break;
                case Function.ExponentialIn: del = Exponential.In; break;
                case Function.ExponentialOut: del = Exponential.Out; break;
                case Function.ExponentialInOut: del = Exponential.InOut; break;
                case Function.CircularIn: del = Circular.In; break;
                case Function.CircularOut: del = Circular.Out; break;
                case Function.CircularInOut: del = Circular.InOut; break;
                case Function.BackIn: del = Back.In; break;
                case Function.BackOut: del = Back.Out; break;
                case Function.BackInOut: del = Back.InOut; break;
                case Function.BounceIn: del = Bounce.In; break;
                case Function.BounceOut: del = Bounce.Out; break;
                case Function.BounceInOut: del = Bounce.InOut; break;
                case Function.ElasticIn: del = Elastic.In; break;
                case Function.ElasticOut: del = Elastic.Out; break;
                case Function.ElasticInOut: del = Elastic.InOut; break;
                default: throw new ArgumentOutOfRangeException(nameof(function));
            }

            _RangedFunctionDelegates[i] = del;
            return del;
        }

         

        private static RangedDelegate[] _RangedDerivativeDelegates;

        public static RangedDelegate GetRangedDerivativeDelegate(this Function function)
        {
            var i = (int)function;
            RangedDelegate del;

            if (_RangedDerivativeDelegates == null)
            {
                _RangedDerivativeDelegates = new RangedDelegate[FunctionCount];
            }
            else
            {
                del = _RangedDerivativeDelegates[i];
                if (del != null)
                    return del;
            }

            switch (function)
            {
                case Function.Linear: del = LinearDerivative; break;
                case Function.QuadraticIn: del = Quadratic.InDerivative; break;
                case Function.QuadraticOut: del = Quadratic.OutDerivative; break;
                case Function.QuadraticInOut: del = Quadratic.InOutDerivative; break;
                case Function.CubicIn: del = Cubic.InDerivative; break;
                case Function.CubicOut: del = Cubic.OutDerivative; break;
                case Function.CubicInOut: del = Cubic.InOutDerivative; break;
                case Function.QuarticIn: del = Quartic.InDerivative; break;
                case Function.QuarticOut: del = Quartic.OutDerivative; break;
                case Function.QuarticInOut: del = Quartic.InOutDerivative; break;
                case Function.QuinticIn: del = Quintic.InDerivative; break;
                case Function.QuinticOut: del = Quintic.OutDerivative; break;
                case Function.QuinticInOut: del = Quintic.InOutDerivative; break;
                case Function.SineIn: del = Sine.InDerivative; break;
                case Function.SineOut: del = Sine.OutDerivative; break;
                case Function.SineInOut: del = Sine.InOutDerivative; break;
                case Function.ExponentialIn: del = Exponential.InDerivative; break;
                case Function.ExponentialOut: del = Exponential.OutDerivative; break;
                case Function.ExponentialInOut: del = Exponential.InOutDerivative; break;
                case Function.CircularIn: del = Circular.InDerivative; break;
                case Function.CircularOut: del = Circular.OutDerivative; break;
                case Function.CircularInOut: del = Circular.InOutDerivative; break;
                case Function.BackIn: del = Back.InDerivative; break;
                case Function.BackOut: del = Back.OutDerivative; break;
                case Function.BackInOut: del = Back.InOutDerivative; break;
                case Function.BounceIn: del = Bounce.InDerivative; break;
                case Function.BounceOut: del = Bounce.OutDerivative; break;
                case Function.BounceInOut: del = Bounce.InOutDerivative; break;
                case Function.ElasticIn: del = Elastic.InDerivative; break;
                case Function.ElasticOut: del = Elastic.OutDerivative; break;
                case Function.ElasticInOut: del = Elastic.InOutDerivative; break;
                default: throw new ArgumentOutOfRangeException(nameof(function));
            }

            _RangedDerivativeDelegates[i] = del;
            return del;
        }

         

        public static float Lerp(float start, float end, float value) => start + (end - start) * value;

        public static float UnLerp(float start, float end, float value) => start == end ? 0 : (value - start) / (end - start);

         

        public static float ReScale(float start, float end, float value, NormalizedDelegate function)
            => Lerp(start, end, function(UnLerp(start, end, value)));

         
        #endregion
         
        #region Linear
         

        public static float Linear(float value) => value;

         

        public static float LinearDerivative(float value) => 1;

         

        public static float Linear(float start, float end, float value) => value;

         

        public static float LinearDerivative(float start, float end, float value) => end - start;

         
        #endregion
         
        #region Quadratic
         

       
        public static class Quadratic
        {
             

           
            public static float In(float value) => value * value;

            public static float Out(float value)
            {
                value--;
                return -value * value + 1;
            }

           
            public static float InOut(float value)
            {
                value *= 2;
                if (value <= 1)
                {
                    return 0.5f * value * value;
                }
                else
                {
                    value -= 2;
                    return 0.5f * (-value * value + 2);
                }
            }

             

            public static float InDerivative(float value) => 2 * value;

            public static float OutDerivative(float value) => 2 - 2 * value;

            public static float InOutDerivative(float value)
            {
                value *= 2;
                if (value <= 1)
                {
                    return 2 * value;
                }
                else
                {
                    value--;
                    return 2 - 2 * value;
                }
            }

             
            // Ranged Variants.
             

            public static float In(float start, float end, float value) => Lerp(start, end, In(UnLerp(start, end, value)));
            public static float Out(float start, float end, float value) => Lerp(start, end, Out(UnLerp(start, end, value)));
            public static float InOut(float start, float end, float value) => Lerp(start, end, InOut(UnLerp(start, end, value)));

            public static float InDerivative(float start, float end, float value) => InDerivative(UnLerp(start, end, value)) * (end - start);
            public static float OutDerivative(float start, float end, float value) => OutDerivative(UnLerp(start, end, value)) * (end - start);
            public static float InOutDerivative(float start, float end, float value) => InOutDerivative(UnLerp(start, end, value)) * (end - start);

             
        }

         
        #endregion
         
        #region Cubic
         

       
        public static class Cubic
        {
             

           
            public static float In(float value) => value * value * value;

          
            public static float Out(float value)
            {
                value--;
                return value * value * value + 1;
            }

            public static float InOut(float value)
            {
                value *= 2;
                if (value <= 1)
                {
                    return 0.5f * value * value * value;
                }
                else
                {
                    value -= 2;
                    return 0.5f * (value * value * value + 2);
                }
            }

             

            public static float InDerivative(float value) => 3 * value * value;

            public static float OutDerivative(float value)
            {
                value--;
                return 3 * value * value;
            }

            public static float InOutDerivative(float value)
            {
                value *= 2;
                if (value <= 1)
                {
                    return 3 * value * value;
                }
                else
                {
                    value -= 2;
                    return 3 * value * value;
                }
            }

             
            // Ranged Variants.
             

            public static float In(float start, float end, float value) => Lerp(start, end, In(UnLerp(start, end, value)));
            public static float Out(float start, float end, float value) => Lerp(start, end, Out(UnLerp(start, end, value)));
            public static float InOut(float start, float end, float value) => Lerp(start, end, InOut(UnLerp(start, end, value)));

            public static float InDerivative(float start, float end, float value) => InDerivative(UnLerp(start, end, value)) * (end - start);
            public static float OutDerivative(float start, float end, float value) => OutDerivative(UnLerp(start, end, value)) * (end - start);
            public static float InOutDerivative(float start, float end, float value) => InOutDerivative(UnLerp(start, end, value)) * (end - start);

             
        }

         
        #endregion
         
        #region Quartic
         

      
        public static class Quartic
        {
             

           
            public static float In(float value) => value * value * value * value;

            
            public static float Out(float value)
            {
                value--;
                return -value * value * value * value + 1;
            }

            
            public static float InOut(float value)
            {
                value *= 2;
                if (value <= 1)
                {
                    return 0.5f * value * value * value * value;
                }
                else
                {
                    value -= 2;
                    return 0.5f * (-value * value * value * value + 2);
                }
            }

             

            public static float InDerivative(float value) => 4 * value * value * value;

            public static float OutDerivative(float value)
            {
                value--;
                return -4 * value * value * value;
            }

            public static float InOutDerivative(float value)
            {
                value *= 2;
                if (value <= 1)
                {
                    return 4 * value * value * value;
                }
                else
                {
                    value -= 2;
                    return -4 * value * value * value;
                }
            }

             
            // Ranged Variants.
             

            public static float In(float start, float end, float value) => Lerp(start, end, In(UnLerp(start, end, value)));
            public static float Out(float start, float end, float value) => Lerp(start, end, Out(UnLerp(start, end, value)));
            public static float InOut(float start, float end, float value) => Lerp(start, end, InOut(UnLerp(start, end, value)));

            public static float InDerivative(float start, float end, float value) => InDerivative(UnLerp(start, end, value)) * (end - start);
            public static float OutDerivative(float start, float end, float value) => OutDerivative(UnLerp(start, end, value)) * (end - start);
            public static float InOutDerivative(float start, float end, float value) => InOutDerivative(UnLerp(start, end, value)) * (end - start);

             
        }

         
        #endregion
         
        #region Quintic
         

      
        public static class Quintic
        {
             

           
            public static float In(float value) => value * value * value * value * value;

           
            public static float Out(float value)
            {
                value--;
                return value * value * value * value * value + 1;
            }

            public static float InOut(float value)
            {
                value *= 2;
                if (value <= 1)
                {
                    return 0.5f * value * value * value * value * value;
                }
                else
                {
                    value -= 2;
                    return 0.5f * (value * value * value * value * value + 2);
                }
            }

             

            public static float InDerivative(float value) => 5 * value * value * value * value;

            public static float OutDerivative(float value)
            {
                value--;
                return 5 * value * value * value * value;
            }

            public static float InOutDerivative(float value)
            {
                value *= 2;
                if (value <= 1)
                {
                    return 5 * value * value * value * value;
                }
                else
                {
                    value -= 2;
                    return 5 * value * value * value * value;
                }
            }

             
            // Ranged Variants.
             

            public static float In(float start, float end, float value) => Lerp(start, end, In(UnLerp(start, end, value)));
            public static float Out(float start, float end, float value) => Lerp(start, end, Out(UnLerp(start, end, value)));
            public static float InOut(float start, float end, float value) => Lerp(start, end, InOut(UnLerp(start, end, value)));

            public static float InDerivative(float start, float end, float value) => InDerivative(UnLerp(start, end, value)) * (end - start);
            public static float OutDerivative(float start, float end, float value) => OutDerivative(UnLerp(start, end, value)) * (end - start);
            public static float InOutDerivative(float start, float end, float value) => InOutDerivative(UnLerp(start, end, value)) * (end - start);

             
        }

         
        #endregion
         
        #region Sine
         

       
        public static class Sine
        {
             

            public static float In(float value) => -Cos(value * (PI * 0.5f)) + 1;

            public static float Out(float value) => Sin(value * (PI * 0.5f));

     
            public static float InOut(float value) => -0.5f * (Cos(PI * value) - 1);

             

            public static float InDerivative(float value) => 0.5f * PI * Sin(0.5f * PI * value);

            public static float OutDerivative(float value) => PI * 0.5f * Cos(value * (PI * 0.5f));

            public static float InOutDerivative(float value) => 0.5f * PI * Sin(PI * value);

             
            // Ranged Variants.
             

            public static float In(float start, float end, float value) => Lerp(start, end, In(UnLerp(start, end, value)));
            public static float Out(float start, float end, float value) => Lerp(start, end, Out(UnLerp(start, end, value)));
            public static float InOut(float start, float end, float value) => Lerp(start, end, InOut(UnLerp(start, end, value)));

            public static float InDerivative(float start, float end, float value) => InDerivative(UnLerp(start, end, value)) * (end - start);
            public static float OutDerivative(float start, float end, float value) => OutDerivative(UnLerp(start, end, value)) * (end - start);
            public static float InOutDerivative(float start, float end, float value) => InOutDerivative(UnLerp(start, end, value)) * (end - start);

             
        }

         
        #endregion
         
        #region Exponential
         

        
        public static class Exponential
        {
             

           
            public static float In(float value) => Pow(2, 10 * (value - 1));

            public static float Out(float value) => -Pow(2, -10 * value) + 1;

          
            public static float InOut(float value)
            {
                value *= 2;
                if (value <= 1)
                {
                    return 0.5f * Pow(2, 10 * (value - 1));
                }
                else
                {
                    value--;
                    return 0.5f * (-Pow(2, -10 * value) + 2);
                }
            }

             

            public static float InDerivative(float value) => 10 * Ln2 * Pow(2, 10 * (value - 1));

            public static float OutDerivative(float value) => 5 * Ln2 * Pow(2, 1 - 10 * value);

            public static float InOutDerivative(float value)
            {
                value *= 2;
                if (value <= 1)
                {
                    return 10 * Ln2 * Pow(2, 10 * (value - 1));
                }
                else
                {
                    value--;
                    return 5 * Ln2 * Pow(2, 1 - 10 * value);
                }
            }

             
            // Ranged Variants.
             

            public static float In(float start, float end, float value) => Lerp(start, end, In(UnLerp(start, end, value)));
            public static float Out(float start, float end, float value) => Lerp(start, end, Out(UnLerp(start, end, value)));
            public static float InOut(float start, float end, float value) => Lerp(start, end, InOut(UnLerp(start, end, value)));

            public static float InDerivative(float start, float end, float value) => InDerivative(UnLerp(start, end, value)) * (end - start);
            public static float OutDerivative(float start, float end, float value) => OutDerivative(UnLerp(start, end, value)) * (end - start);
            public static float InOutDerivative(float start, float end, float value) => InOutDerivative(UnLerp(start, end, value)) * (end - start);

             
        }

         
        #endregion
         
        #region Circular
         

      
        public static class Circular
        {
             

           
            public static float In(float value) => -(Sqrt(1 - value * value) - 1);

          
            public static float Out(float value)
            {
                value--;
                return Sqrt(1 - value * value);
            }

           
            public static float InOut(float value)
            {
                value *= 2;
                if (value <= 1)
                {
                    return -0.5f * (Sqrt(1 - value * value) - 1);
                }
                else
                {
                    value -= 2;
                    return 0.5f * (Sqrt(1 - value * value) + 1);
                }
            }

             

            public static float InDerivative(float value) => value / Sqrt(1 - value * value);

            public static float OutDerivative(float value)
            {
                value--;
                return -value / Sqrt(1 - value * value);
            }

            public static float InOutDerivative(float value)
            {
                value *= 2;
                if (value <= 1)
                {
                    return value / (2 * Sqrt(1 - value * value));
                }
                else
                {
                    value -= 2;
                    return -value / (2 * Sqrt(1 - value * value));
                }
            }

             
            // Ranged Variants.
             

            public static float In(float start, float end, float value) => Lerp(start, end, In(UnLerp(start, end, value)));
            public static float Out(float start, float end, float value) => Lerp(start, end, Out(UnLerp(start, end, value)));
            public static float InOut(float start, float end, float value) => Lerp(start, end, InOut(UnLerp(start, end, value)));

            public static float InDerivative(float start, float end, float value) => InDerivative(UnLerp(start, end, value)) * (end - start);
            public static float OutDerivative(float start, float end, float value) => OutDerivative(UnLerp(start, end, value)) * (end - start);
            public static float InOutDerivative(float start, float end, float value) => InOutDerivative(UnLerp(start, end, value)) * (end - start);

             
        }

         
        #endregion
         
        #region Back
         

       
        public static class Back
        {
             

            private const float C = 1.758f;

             

            public static float In(float value) => value * value * ((C + 1) * value - C);

            public static float Out(float value)
            {
                value -= 1;
                return value * value * ((C + 1) * value + C) + 1;
            }

            public static float InOut(float value)
            {
                value *= 2;
                if (value <= 1)
                {
                    return 0.5f * value * value * ((C + 1) * value - C);
                }
                else
                {
                    value -= 2;
                    return 0.5f * (value * value * ((C + 1) * value + C) + 2);
                }
            }

             

            public static float InDerivative(float value) => 3 * (C + 1) * value * value - 2 * C * value;

            public static float OutDerivative(float value)
            {
                value -= 1;
                return (C + 1) * value * value + 2 * value * ((C + 1) * value + C);
            }

            public static float InOutDerivative(float value)
            {
                value *= 2;
                if (value <= 1)
                {
                    return 3 * (C + 1) * value * value - 2 * C * value;
                }
                else
                {
                    value -= 2;
                    return (C + 1) * value * value + 2 * value * ((C + 1) * value + C);
                }
            }

             
            // Ranged Variants.
             

            public static float In(float start, float end, float value) => Lerp(start, end, In(UnLerp(start, end, value)));
            public static float Out(float start, float end, float value) => Lerp(start, end, Out(UnLerp(start, end, value)));
            public static float InOut(float start, float end, float value) => Lerp(start, end, InOut(UnLerp(start, end, value)));

            public static float InDerivative(float start, float end, float value) => InDerivative(UnLerp(start, end, value)) * (end - start);
            public static float OutDerivative(float start, float end, float value) => OutDerivative(UnLerp(start, end, value)) * (end - start);
            public static float InOutDerivative(float start, float end, float value) => InOutDerivative(UnLerp(start, end, value)) * (end - start);

             
        }

         
        #endregion
         
        #region Bounce
         

        public static class Bounce
        {
             

            public static float In(float value)
            {
                return 1 - Out(1 - value);
            }

            public static float Out(float value)
            {
                switch (value)
                {
                    case 0: return 0;
                    case 1: return 1;
                }

                if (value < (1f / 2.75f))
                {
                    return 7.5625f * value * value;
                }
                else if (value < (2f / 2.75f))
                {
                    value -= 1.5f / 2.75f;
                    return 7.5625f * value * value + 0.75f;
                }
                else if (value < (2.5f / 2.75f))
                {
                    value -= 2.25f / 2.75f;
                    return 7.5625f * value * value + 0.9375f;
                }
                else
                {
                    value -= 2.625f / 2.75f;
                    return 7.5625f * value * value + 0.984375f;
                }
            }

            public static float InOut(float value)
            {
                if (value < 0.5f)
                    return 0.5f * In(value * 2);
                else
                    return 0.5f + 0.5f * Out(value * 2 - 1);
            }

             

            public static float InDerivative(float value) => OutDerivative(1 - value);

            public static float OutDerivative(float value)
            {
                if (value < (1f / 2.75f))
                {
                    return 2 * 7.5625f * value;
                }
                else if (value < (2f / 2.75f))
                {
                    value -= 1.5f / 2.75f;
                    return 2 * 7.5625f * value;
                }
                else if (value < (2.5f / 2.75f))
                {
                    value -= 2.25f / 2.75f;
                    return 2 * 7.5625f * value;
                }
                else
                {
                    value -= 2.625f / 2.75f;
                    return 2 * 7.5625f * value;
                }
            }

            public static float InOutDerivative(float value)
            {
                value *= 2;
                if (value <= 1)
                    return OutDerivative(1 - value);
                else
                    return OutDerivative(value - 1);
            }

             
            // Ranged Variants.
             

            public static float In(float start, float end, float value) => Lerp(start, end, In(UnLerp(start, end, value)));
            public static float Out(float start, float end, float value) => Lerp(start, end, Out(UnLerp(start, end, value)));
            public static float InOut(float start, float end, float value) => Lerp(start, end, InOut(UnLerp(start, end, value)));

            public static float InDerivative(float start, float end, float value) => InDerivative(UnLerp(start, end, value)) * (end - start);
            public static float OutDerivative(float start, float end, float value) => OutDerivative(UnLerp(start, end, value)) * (end - start);
            public static float InOutDerivative(float start, float end, float value) => InOutDerivative(UnLerp(start, end, value)) * (end - start);

             
        }

         
        #endregion
         
        #region Elastic
         

        public static class Elastic
        {
             

            public const float TwoThirdsPi = 2f / 3f * PI;

             

            public static float In(float value)
            {
                switch (value)
                {
                    case 0: return 0;
                    case 1: return 1;
                }

                return -Pow(2, 10 * value - 10) * Sin((value * 10 - 10.75f) * TwoThirdsPi);
            }

            public static float Out(float value)
            {
                switch (value)
                {
                    case 0: return 0;
                    case 1: return 1;
                }

                return 1 + Pow(2, -10 * value) * Sin((value * -10 - 0.75f) * TwoThirdsPi);
            }

            public static float InOut(float value)
            {
                switch (value)
                {
                    case 0: return 0;
                    case 0.5f: return 0.5f;
                    case 1: return 1;
                }

                value *= 2;
                if (value <= 1)
                {
                    return 0.5f * (-Pow(2, 10 * value - 10) * Sin((value * 10 - 10.75f) * TwoThirdsPi));
                }
                else
                {
                    value--;
                    return 0.5f + 0.5f * (1 + Pow(2, -10 * value) * Sin((value * -10 - 0.75f) * TwoThirdsPi));
                }
            }

             

            public static float InDerivative(float value)
            {
                return -(5 * Pow(2, 10 * value - 9) *
                    (3 * Ln2 * Sin(PI * (40 * value - 43) / 6) +
                    2 * PI * Cos(PI * (40 * value - 43) / 6))) / 3;
            }

            public static float OutDerivative(float value)
            {
                return -(30 * Ln2 * Sin(2 * PI * (10 * value - 3f / 4f) / 3) -
                    20 * PI * Cos(2 * PI * (10 * value - 3f / 4f) / 3)) /
                    (3 * Pow(2, 10 * value));
            }

            public static float InOutDerivative(float value)
            {
                value *= 2;
                if (value <= 1)
                    return OutDerivative(1 - value);
                else
                    return OutDerivative(value - 1);
            }

             
            // Ranged Variants.
             

            public static float In(float start, float end, float value) => Lerp(start, end, In(UnLerp(start, end, value)));
            public static float Out(float start, float end, float value) => Lerp(start, end, Out(UnLerp(start, end, value)));
            public static float InOut(float start, float end, float value) => Lerp(start, end, InOut(UnLerp(start, end, value)));

            public static float InDerivative(float start, float end, float value) => InDerivative(UnLerp(start, end, value)) * (end - start);
            public static float OutDerivative(float start, float end, float value) => OutDerivative(UnLerp(start, end, value)) * (end - start);
            public static float InOutDerivative(float start, float end, float value) => InOutDerivative(UnLerp(start, end, value)) * (end - start);

             
        }

         
        #endregion
         
    }
}
