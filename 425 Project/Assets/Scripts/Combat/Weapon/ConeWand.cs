using System.Collections.Generic;
using Combat;
using Combat.Weapon;
using Stat;
using UnityEngine;
using Utils;

public class ConeWand : Weapon
{
    [SerializeField] private TriggerEvent _damageCone;

    [SerializeField] private float _rate = 0.05f;

    private readonly List<IDamageable> _enemies = new();
    private float _nextFireTime;
    private AudioSource _sfx;

    private void Start()
    {
        _sfx = gameObject.AddComponent<AudioSource>();
        _damageCone.OnTriggerEnterEvent += (o, c) =>
        {
            Debug.Log(c.name);
            if (c.TryGetComponent(out IDamageable damageable) && Player.Instance && c.gameObject != Player.Instance.gameObject)
            {
                _enemies.Add(damageable);
            }
        };
        _damageCone.OnTriggerExitEvent += (o, c) =>
        {
            if (c.TryGetComponent(out IDamageable damageable) && Player.Instance && c.gameObject != Player.Instance.gameObject)
            {
                _enemies.Remove(damageable);
            }
        };
    }

    protected override void FireStart()
    {
        _sfx.loop = true;
        SoundManager.PlaySound(SoundManager.Sound.FireSpellStart, _sfx);
        SpawnCone();
    }

    protected override void FireEnd()
    {
        _sfx.loop = false;
        _sfx.Stop();
        SoundManager.PlaySound(SoundManager.Sound.FireSpellStop);
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

    public override void SetStats()
    {
        float mult = Random.Range(0.8f, 1.3f);
        _manaPerShot = 3 * Mathf.Pow(mult, 2);
        _damage = 2 * (1 + PlayerStats.Instance.WeaponPower.Value) * mult;
        _rate = 0.05f;
    }
}