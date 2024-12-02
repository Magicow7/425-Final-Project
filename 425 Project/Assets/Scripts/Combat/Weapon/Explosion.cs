using System;
using System.Collections;
using System.Collections.Generic;
using Combat;
using UnityEngine;
using Utils;

public class Explosion : MonoBehaviour
{
    private float _explosionRadius = 0;
    private float _explosionDamage = 0;
    private float _explosionDamageOverTime = 0;
    private float _explosionTime = 0;
    private bool _canHit = false;
    
    [SerializeField] 
    private ParticleSystem _explosionVFX;

    private void Awake()
    {
        _canHit = true;
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
        foreach (Collider other in colliders)
        {
            if (other.gameObject.TryGetComponent(out IDamageable damageable) && Player.Instance && other.gameObject != Player.Instance.gameObject)
            {
                damageable.TakeDamage(_explosionDamage);
            }
        }
        
        Destroy(gameObject, _explosionTime);
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
}
