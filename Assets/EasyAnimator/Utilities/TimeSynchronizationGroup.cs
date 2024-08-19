

using System.Collections.Generic;
using UnityEngine;

namespace EasyAnimator
{
   
    public class TimeSynchronizationGroup : HashSet<object>
    {
         

        private EasyAnimatorComponent _EasyAnimator;

        public EasyAnimatorComponent EasyAnimator
        {
            get => _EasyAnimator;
            set
            {
                _EasyAnimator = value;
                NormalizedTime = null;
            }
        }

         

        public float? NormalizedTime { get; set; }

         

        public TimeSynchronizationGroup(EasyAnimatorComponent EasyAnimator) => EasyAnimator = EasyAnimator;

         

        public bool StoreTime(object key) => StoreTime(key, EasyAnimator.States.Current);

        public bool StoreTime(object key, EasyAnimatorState state)
        {
            if (state != null && Contains(key))
            {
                NormalizedTime = state.NormalizedTime;
                return true;
            }
            else
            {
                NormalizedTime = null;
                return false;
            }
        }

         

        public bool SyncTime(object key) => SyncTime(key, Time.deltaTime);

        public bool SyncTime(object key, float deltaTime) => SyncTime(key, EasyAnimator.States.Current, deltaTime);

        public bool SyncTime(object key, EasyAnimatorState state) => SyncTime(key, state, Time.deltaTime);

        public bool SyncTime(object key, EasyAnimatorState state, float deltaTime)
        {
            if (NormalizedTime == null ||
                state == null ||
                !Contains(key))
                return false;

            // Setting the Time forces it to stay at that value after the next animation update.
            // But we actually want it to keep playing, so we need to add deltaTime manually.
            state.Time = NormalizedTime.Value * state.Length + deltaTime * state.EffectiveSpeed;
            return true;
        }

         
    }
}
