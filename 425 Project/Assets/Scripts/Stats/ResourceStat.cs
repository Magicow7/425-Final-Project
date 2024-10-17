using UnityEngine;

namespace Stat
{
    public class ResourceStat : Stat
    {
        public override float Value
        {
            get => CurStat.Value;
            set => CurStat.Value = Mathf.Clamp(value, 0, MaxValue);
        }

        public float Percentage => Value / MaxValue;
        
        public float MaxValue => MaxStat.Value;

        public AttributeStat MaxStat { get; private set; }
        public AttributeStat CurStat { get; private set; }
        
        public ResourceStat(float baseValue, float modValue, float modMult)
        {
            MaxStat = new AttributeStat(baseValue, modValue, modMult);
            CurStat = new AttributeStat(MaxStat.Value);
            MaxStat.OnValueChanged += (prev, cur) =>
            {
                float delta = cur - prev;
                CurStat.Value -= delta;
            };
        }
        
        public ResourceStat(float baseValue)
        {
            MaxStat = new AttributeStat(baseValue);
            CurStat = new AttributeStat(MaxStat.Value);
        }

        public void Refill()
        {
            Value = MaxValue;
        }

        public bool TrySpendResource(float value)
        {
            if (Value - value > 0)
            {
                Value -= value;
                return true;
            }

            return false;
        }
    }
}