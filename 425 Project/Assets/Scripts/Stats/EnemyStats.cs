using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Stat;

namespace Stat
{
    public class EnemyStats
    {
        public ResourceStat Health { get; private set; }
        public ResourceStat Speed { get; private set; }
        public float Scale { get; private set; }
        public ResourceStat AttackDamage { get; private set; }

        // Base constructor
        public EnemyStats()
        {
            Health = new ResourceStat(10);
            Speed = new ResourceStat(2);
            Scale = 1.0f;
            AttackDamage = new ResourceStat(5);
        }

        // Custom constructor
        public EnemyStats(float health, float speed, float scale, float attackDamage)
        {
            Health = new ResourceStat(health);
            Speed = new ResourceStat(speed);
            Scale = scale;
            AttackDamage = new ResourceStat(attackDamage);
        }
    }
}
