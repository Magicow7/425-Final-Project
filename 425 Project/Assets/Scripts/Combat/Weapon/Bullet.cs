using System.Collections;
using Combat;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [field: SerializeField] public Rigidbody Rigidbody { get; private set; } = null!;

    [SerializeField] private Explosion _explosion;

    [SerializeField] private LayerMask _layerMask;

    private bool _canHit;

    private float _damage;
    private float _explosionDamage;
    private float _explosionDamageOverTime;
    private float _explosionRadius;
    private float _explosionTime;

    private void Awake()
    {
        _canHit = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!_canHit)
        {
            return;
        }

        if (gameObject.name == "Bullet(Clone)")
        {
            SoundManager.PlaySound(SoundManager.Sound.GrenadeBoom, gameObject.transform.position);
        }

        if (other.gameObject.TryGetComponent(out IDamageable damageable) && Player.Instance && other.gameObject != Player.Instance.gameObject)
        {
            damageable.TakeDamage(_damage);
            ExplodeAndDestroy();
            return;
        }

        if ((_layerMask & (1 << other.gameObject.layer)) != 0)
        {
            ExplodeAndDestroy();
        }
    }

    public void Setup(float damage, float explosionRadius, float explosionDamage, float explosionTime, float explosionDamageOverTime, float lifespan)
    {
        _damage = damage;
        _explosionRadius = explosionRadius;
        _explosionDamage = explosionDamage;
        _explosionDamageOverTime = explosionDamageOverTime;
        _explosionTime = explosionTime;
        StartCoroutine(_Lifetime(lifespan));
    }

    private IEnumerator _Lifetime(float time)
    {
        yield return new WaitForSeconds(time);
        ExplodeAndDestroy();
    }

    private void ExplodeAndDestroy()
    {
        if (_explosionRadius > 0)
        {
            Debug.Log(_explosionRadius);
            Instantiate(_explosion, transform.position, Quaternion.identity)
                .Explode(_explosionRadius, _explosionDamage, _explosionTime, _explosionDamageOverTime);
        }

        Destroy(gameObject);
    }
}