using Combat;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    [SerializeField] private ParticleSystem _explosionVFX;

    private bool _canHit;
    private float _explosionDamage;
    private float _explosionDamageOverTime;
    private float _explosionRadius;
    private float _explosionTime;

    private void Awake()
    {
        _canHit = true;
    }

    private void OnTriggerStay(Collider other)
    {
        if (!_canHit)
        {
            return;
        }

        if (other.gameObject.TryGetComponent(out IDamageable damageable) && Player.Instance && other.gameObject != Player.Instance.gameObject)
        {
            damageable.TakeDamage(_explosionDamageOverTime * Time.deltaTime);
        }
    }

    public void Explode(float explosionRadius, float explosionDamage, float explosionTime, float explosionDamageOverTime)
    {
        _explosionRadius = explosionRadius;
        _explosionDamage = explosionDamage;
        _explosionDamageOverTime = explosionDamageOverTime;
        _explosionTime = explosionTime;

        transform.localScale = new Vector3(_explosionRadius, _explosionRadius, _explosionRadius);

        // TODO: Play explosion animation
        // Instantiate(_explosionVFX, transform.position, Quaternion.identity);
        Collider[] colliders = Physics.OverlapSphere(transform.position, _explosionRadius / 2);
        foreach (var other in colliders)
        {
            if (other.gameObject.TryGetComponent(out IDamageable damageable) && Player.Instance && other.gameObject != Player.Instance.gameObject)
            {
                damageable.TakeDamage(_explosionDamage);
            }
        }

        Destroy(gameObject, _explosionTime);
    }
}