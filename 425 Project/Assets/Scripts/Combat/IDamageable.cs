namespace Combat
{
    public interface IDamageable
    {
        public abstract bool TakeDamage(float damage);
        public abstract void Death();
    }
}