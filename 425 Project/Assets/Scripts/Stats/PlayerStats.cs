using System.Collections;
using UnityEngine;
using Utils.Singleton;

namespace Stat
{
    [SingletonAttribute(SingletonCreationMode.Auto, false)]
    public class PlayerStats : SingletonMonoBehaviour<PlayerStats>, ICustomUpdate
    {
        [SerializeField] private int _maxHealth = 100;

        [SerializeField] private int _maxMana = 500;

        [SerializeField] private int _manaRegen = 20;

        public ResourceStat Mana { get; private set; }
        public ResourceStat Health { get; private set; }

        private float _healthRegen = 0;

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
            if (Health.Value > 0)
            {
                Health.Value += _healthRegen * deltaTime;
            }
        }

        public void RegenerateHealth(float healthRegen, int time)
        {
            _healthRegen = healthRegen;
            StartCoroutine(_StopRegen(time));
        }

        private IEnumerator _StopRegen(int time)
        {
            yield return new WaitForSeconds(time);
            _healthRegen = 0;
        }
    }
}