namespace Combat
{
    public interface IDamageable
    {
        public abstract void TakeDamage(int damage);
        public abstract void Death();
    }
}