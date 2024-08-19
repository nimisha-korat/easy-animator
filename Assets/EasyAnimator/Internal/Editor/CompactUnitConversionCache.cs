

#if UNITY_EDITOR

using EasyAnimator.Editor;
using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEditor;

namespace EasyAnimator.Editor
{
  
    public sealed class CompactUnitConversionCache
    {
         

        public readonly string Suffix;

        public readonly string ApproximateSuffix;

        public readonly string ConvertedZero;

        public readonly string ConvertedSmallPositive;

        public readonly string ConvertedSmallNegative;

        public readonly float SuffixWidth;

        private List<ConversionCache<float, string>>
            Caches = new List<ConversionCache<float, string>>();

        private static ConversionCache<string, float>
            WidthCache = EasyAnimatorGUI.CreateWidthCache(EditorStyles.numberField);

        public static readonly float
            FieldPadding = EditorStyles.numberField.padding.horizontal;

        public static readonly float
            ApproximateSymbolWidth = WidthCache.Convert("~") - FieldPadding;

        public static readonly string
            DecimalSeparator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;

        public const float
            SmallExponentialThreshold = 0.0001f;

        public const float
            LargeExponentialThreshold = 9999999f;

         

        public CompactUnitConversionCache(string suffix)
        {
            Suffix = suffix;
            ApproximateSuffix = "~" + suffix;
            ConvertedZero = "0" + Suffix;
            ConvertedSmallPositive = "0" + ApproximateSuffix;
            ConvertedSmallNegative = "-0" + ApproximateSuffix;
            SuffixWidth = WidthCache.Convert(suffix);
        }

         

        public string Convert(float value, float width)
        {
            if (value == 0)
                return ConvertedZero;

            if (!EasyAnimatorSettings.AnimationTimeFields.showApproximations)
                return GetCache(0).Convert(value);

            if (value < SmallExponentialThreshold &&
                value > -SmallExponentialThreshold)
                return value > 0 ? ConvertedSmallPositive : ConvertedSmallNegative;

            var index = CalculateCacheIndex(value, width);
            return GetCache(index).Convert(value);
        }

         

        private int CalculateCacheIndex(float value, float width)
        {
            //if (value > LargeExponentialThreshold ||
            //    value < -LargeExponentialThreshold)
            //    return 0;

            var valueString = value.ToStringCached();

            // It the approximated string wouldn't be shorter than the original, don't approximate.
            if (valueString.Length < 2 + ApproximateSuffix.Length)
                return 0;

            // If the field is wide enough to fit the full value, don't approximate.
            width -= FieldPadding + ApproximateSymbolWidth * 0.75f;
            var valueWidth = WidthCache.Convert(valueString) + SuffixWidth;
            if (valueWidth <= width)
                return 0;

            // If the number of allowed characters would include the full value, don't approximate.
            var suffixedLength = valueString.Length + Suffix.Length;
            var allowedCharacters = (int)(suffixedLength * width / valueWidth);
            if (allowedCharacters + 2 >= suffixedLength)
                return 0;

            return allowedCharacters;
        }

         

        private ConversionCache<float, string> GetCache(int characterCount)
        {
            while (Caches.Count <= characterCount)
                Caches.Add(null);

            var cache = Caches[characterCount];
            if (cache == null)
            {
                if (characterCount == 0)
                {
                    cache = new ConversionCache<float, string>((value) =>
                    {
                        return value.ToStringCached() + Suffix;
                    });
                }
                else
                {
                    cache = new ConversionCache<float, string>((value) =>
                    {
                        var valueString = value.ToStringCached();

                        if (value > LargeExponentialThreshold ||
                            value < -LargeExponentialThreshold)
                            goto IsExponential;

                        var decimalIndex = valueString.IndexOf(DecimalSeparator);
                        if (decimalIndex < 0 || decimalIndex > characterCount)
                            goto IsExponential;

                        // Not exponential.
                        return valueString.Substring(0, characterCount) + ApproximateSuffix;

                        IsExponential:
                        var digits = Math.Max(0, characterCount - ApproximateSuffix.Length - 1);
                        var format = GetExponentialFormat(digits);
                        valueString = value.ToString(format);
                        TrimExponential(ref valueString);
                        return valueString + Suffix;
                    });
                }

                Caches[characterCount] = cache;
            }

            return cache;
        }

         

        private static List<string> _ExponentialFormats;

        public static string GetExponentialFormat(int digits)
        {
            if (_ExponentialFormats == null)
                _ExponentialFormats = new List<string>();

            while (_ExponentialFormats.Count <= digits)
                _ExponentialFormats.Add("g" + _ExponentialFormats.Count);

            return _ExponentialFormats[digits];
        }

         

        private static void TrimExponential(ref string valueString)
        {
            var length = valueString.Length;
            if (length <= 4 ||
                valueString[length - 4] != 'e' ||
                valueString[length - 2] != '0')
                return;

            valueString =
                valueString.Substring(0, length - 2) +
                valueString[length - 1];
        }

         
    }
}

#endif

