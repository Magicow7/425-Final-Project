using System;
using System.Collections;
using System.Collections.Generic;
using Combat;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [field: SerializeField] 
    public Rigidbody Rigidbody { get; private set; }

    private float _damage = 0;
    private bool _canHit = false;

    private void Reset()
    {
        Rigidbody = GetComponent<Rigidbody>();
    }

    private void Awake()
    {
        _canHit = true;
    }

    public void Setup(float damage, float lifespan)
    {
        _damage = damage;
        Destroy(gameObject, lifespan);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!_canHit)
        {
            return;
        }

        if (other.gameObject.TryGetComponent<IDamageable>(out IDamageable damageable))
        {
            damageable.TakeDamage(_damage);
            Destroy(gameObject);
        }
    }
}
