using System;
using UnityEngine;

namespace Stat
{
    [Serializable]
    public class AttributeStat : Stat
    {
        [SerializeField] private float _baseValue;

        [SerializeField] private float _modValue;

        [SerializeField] private float _modMult;

        public AttributeStat(float baseValue, float modValue, float modMult)
        {
            _baseValue = baseValue;
            _modValue = modValue;
            _modMult = modMult;
        }

        public AttributeStat(float baseValue)
        {
            _baseValue = baseValue;
            _modValue = 0;
            _modMult = 1;
        }

        public override float Value
        {
            get
            {
                float val = BaseValue * ModMult + ModValue;
                return val < 0 ? 0 : val;
            }
            set => ModValue += value - Value;
        }

        protected float BaseValue
        {
            get => _baseValue;
            set
            {
                float val = Value;
                _baseValue = value;
                ValueChanged(val, Value);
            }
        }

        protected float ModValue
        {
            get => _modValue;
            set
            {
                float val = Value;
                _modValue = value;
                ValueChanged(val, Value);
            }
        }

        protected float ModMult
        {
            get => _modMult;
            set
            {
                float val = Value;
                _modMult = value;
                ValueChanged(val, Value);
            }
        }
    }
}