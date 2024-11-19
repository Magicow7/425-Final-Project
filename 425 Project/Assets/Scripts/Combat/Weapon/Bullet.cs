using System;
using System.Collections;
using System.Collections.Generic;
using Combat;
using UnityEngine;
using Utils;

public class Bullet : MonoBehaviour
{
    [field: SerializeField] 
    public Rigidbody Rigidbody { get; private set; } = null!;

    [SerializeField] 
    private Explosion _explosion;

    private float _damage = 0;
    private float _explosionRadius = 0;
    private float _explosionDamage = 0;
    private float _explosionTime = 0;
    private float _explosionDamageOverTime = 0;
    private bool _canHit = false;

    private void Awake()
    {
        _canHit = true;
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

    private void OnTriggerEnter(Collider other)
    {
        if (!_canHit)
        {
            return;
        }

        if (other.gameObject.TryGetComponent(out IDamageable damageable))
        {
            damageable.TakeDamage(_damage);
            ExplodeAndDestroy();
        }
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