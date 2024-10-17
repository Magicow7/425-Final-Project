using System;
using Unity.VisualScripting;
using UnityEngine;

namespace Stat
{
    [Serializable]
    public abstract class Stat
    {
        public abstract float Value { get; set; }
        
        public event Action<float, float> OnValueChanged = delegate { };

        protected void ValueChanged(float prev, float cur)
        {
            OnValueChanged(prev, cur);
        }
    }
}