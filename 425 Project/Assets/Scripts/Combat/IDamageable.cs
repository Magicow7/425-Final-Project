namespace Combat
{
    public interface IDamageable
    {
        public bool TakeDamage(float damage);
        public void Death();
    }
}