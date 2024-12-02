namespace Stat
{
    public class EnemyStats
    {
        // Base constructor
        public EnemyStats()
        {
            Health = new ResourceStat(10);
            Speed = new AttributeStat(2);
            Scale = 1.0f;
            AttackDamage = new AttributeStat(5);
            AttackSpeed = new AttributeStat(1);
        }

        // Custom constructor
        public EnemyStats(float health, float speed, float scale, float attackDamage, float attackTime)
        {
            Health = new ResourceStat(health);
            Speed = new AttributeStat(speed);
            Scale = scale;
            AttackDamage = new AttributeStat(attackDamage);
            AttackSpeed = new AttributeStat(attackTime);
        }

        public ResourceStat Health { get; private set; }
        public AttributeStat Speed { get; private set; }
        public float Scale { get; private set; }
        public AttributeStat AttackDamage { get; private set; }
        public AttributeStat AttackSpeed { get; private set; }
    }
}