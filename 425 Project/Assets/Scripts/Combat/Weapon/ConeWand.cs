using System;
using System.Collections;
using System.Collections.Generic;
using Combat;
using Combat.Weapon;
using UnityEngine;
using UnityEngine.Serialization;
using Utils;

public class ConeWand : Weapon
{
    [SerializeField]
    private TriggerEvent _damageCone;

    [SerializeField]
    private float _rate = 0.05f;
    private float _nextFireTime = 0f;
    
    private List<IDamageable> _enemies = new();

    private void Start()
    {
        _damageCone.OnTriggerEnterEvent += (o, c) =>
        {
            Debug.Log(c.name);
            if (c.TryGetComponent(out IDamageable damageable)) 
            {
                _enemies.Add(damageable);
            }
        };
        _damageCone.OnTriggerExitEvent += (o, c) =>
        {
            if (c.TryGetComponent(out IDamageable damageable)) 
            {
                _enemies.Remove(damageable);
            }
        };
    }

    protected override void FireStart()
    {
        SpawnCone();
    }
    
    protected override void FireEnd()
    {
        DespawnCone();
    }

    private void SpawnCone()
    {
        _damageCone.gameObject.SetActive(true);
    }

    private void DespawnCone()
    {
        _damageCone.gameObject.SetActive(false);
        _enemies.Clear();
    }

    protected override bool Fire()
    {
        if (Time.time > _nextFireTime)
        {
            _nextFireTime = Time.time + _rate;

            for (int i = _enemies.Count - 1; i >= 0; i--)
            {
                if (!_enemies[i].TakeDamage(_damage))
                {
                    _enemies.RemoveAt(i);
                }
            }

            return true;
        }

        return false;
    }
}