using System;
using UnityEngine;

namespace Combat
{
    public class Enemy : MonoBehaviour, IDamageable
    {
        [field: SerializeField]
        public float Health { get; private set; } = 10;
        [field: SerializeField]
        public float MaxHealth { get; private set; } = 10;

        [SerializeField] 
        private DamageNumber _damageNumber;

        private void Start()
        {
            Health = MaxHealth;
        }


        public bool TakeDamage(float damage)
        {
            _damageNumber.ShowDamageNumber(damage);
            
            Health -= damage;
            
            if (Health <= 0)
            {
                Death();
                return false;
            }

            return true;
        }

        public void Death()
        {
            Destroy(gameObject);
        }
    }
}