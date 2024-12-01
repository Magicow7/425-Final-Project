using System;
using UnityEngine;
using UnityEngine.Serialization;
using Utils.Singleton;

namespace Stat
{
    public class PlayerStats : SingletonMonoBehaviour<PlayerStats>, ICustomUpdate
    {
        [SerializeField]
        private int _maxHealth = 100;
        [SerializeField]
        private int _maxMana = 500;
        
        
        private int _healthRegen = 5;

        [SerializeField]
        private int _manaRegen = 20;
        
        public ResourceStat Mana { get; private set; }
        public ResourceStat Health { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            
            Mana = new ResourceStat(_maxMana);
            Health = new ResourceStat(_maxHealth);
            
            CustomUpdateManager.RegisterUpdate(this);
        }

        public void CustomUpdate(float deltaTime)
        {
          
            Mana.Value += _manaRegen * deltaTime;

            if (Health.Value > 0) {
                Health.Value += _healthRegen * deltaTime;
            }
            
        }
    }
}