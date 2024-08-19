
using System;
using System.Text;
using UnityEngine;
using UnityEngine.Animations;

namespace EasyAnimator
{
   
    public abstract class MixerState<TParameter> : ManualMixerState
    {
         
        #region Properties
         

        private TParameter[] _Thresholds = Array.Empty<TParameter>();

         

        private TParameter _Parameter;

     
        public TParameter Parameter
        {
            get => _Parameter;
            set
            {
                _Parameter = value;
                WeightsAreDirty = true;
                RequireUpdate();
            }
        }

         
        #endregion
         
        #region Thresholds
         

        public bool HasThresholds => _Thresholds.Length >= ChildCount;

         

        public TParameter GetThreshold(int index) => _Thresholds[index];

         

        public void SetThreshold(int index, TParameter threshold)
        {
            _Thresholds[index] = threshold;
            OnThresholdsChanged();
        }

         

        public void SetThresholds(params TParameter[] thresholds)
        {
#if UNITY_ASSERTIONS
            if (thresholds == null)
                throw new ArgumentNullException(nameof(thresholds));
#endif

            if (thresholds.Length != ChildCount)
                throw new ArgumentOutOfRangeException(nameof(thresholds), "Incorrect threshold count. There are " + ChildCount +
                    " states, but the specified thresholds array contains " + thresholds.Length + " elements.");

            _Thresholds = thresholds;
            OnThresholdsChanged();
        }

         

        public bool ValidateThresholdCount()
        {
            var count = ChildCount;
            if (_Thresholds.Length != count)
            {
                _Thresholds = new TParameter[count];
                return true;
            }
            else return false;
        }

         

        public virtual void OnThresholdsChanged()
        {
            WeightsAreDirty = true;
            RequireUpdate();
        }

         

        public void CalculateThresholds(Func<EasyAnimatorState, TParameter> calculate)
        {
            ValidateThresholdCount();

            for (int i = ChildCount - 1; i >= 0; i--)
            {
                var state = GetChild(i);
                if (state == null)
                    continue;

                _Thresholds[i] = calculate(state);
            }

            OnThresholdsChanged();
        }

         

        public override void RecreatePlayable()
        {
            base.RecreatePlayable();
            WeightsAreDirty = true;
            RequireUpdate();
        }

         
        #endregion
         
        #region Initialisation
         

        public override void Initialize(int portCount)
        {
            base.Initialize(portCount);
            _Thresholds = new TParameter[portCount];
            OnThresholdsChanged();
        }

         

        public void Initialize(AnimationClip[] clips, TParameter[] thresholds)
        {
            Initialize(clips);
            _Thresholds = thresholds;
            OnThresholdsChanged();
        }

        public void Initialize(AnimationClip[] clips, Func<EasyAnimatorState, TParameter> calculateThreshold)
        {
            Initialize(clips);
            CalculateThresholds(calculateThreshold);
        }

         

        public ClipState CreateChild(int index, AnimationClip clip, TParameter threshold)
        {
            SetThreshold(index, threshold);
            return CreateChild(index, clip);
        }

        public EasyAnimatorState CreateChild(int index, EasyAnimator.ITransition transition, TParameter threshold)
        {
            SetThreshold(index, threshold);
            return CreateChild(index, transition);
        }

         

        public void SetChild(int index, EasyAnimatorState state, TParameter threshold)
        {
            SetChild(index, state);
            SetThreshold(index, threshold);
        }

         
        #endregion
         
        #region Descriptions
         

        public override string GetDisplayKey(EasyAnimatorState state) => $"[{state.Index}] {_Thresholds[state.Index]}";

         

        protected override void AppendDetails(StringBuilder text, string separator)
        {
            text.Append(separator);
            text.Append($"{nameof(Parameter)}: ");
            AppendParameter(text, Parameter);

            text.Append(separator).Append("Thresholds: ");
            for (int i = 0; i < _Thresholds.Length; i++)
            {
                if (i > 0)
                    text.Append(", ");

                AppendParameter(text, _Thresholds[i]);
            }

            base.AppendDetails(text, separator);
        }

         

        public virtual void AppendParameter(StringBuilder description, TParameter parameter)
        {
            description.Append(parameter);
        }

         
        #endregion
         
    }
}

