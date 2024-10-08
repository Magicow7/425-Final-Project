using System;
using UnityEngine;

namespace Combat
{
    public class Enemy : MonoBehaviour, IDamageable
    {
        public int Health { get; private set; } = 10;
        public int MaxHealth { get; private set; } = 10;

        private void Start()
        {
            Health = MaxHealth;
        }


        public void TakeDamage(int damage)
        {
            Health -= damage;
            if (Health <= 0)
            {
                Death();
            }
        }

        public void Death()
        {
            Destroy(gameObject);
        }
    }
}