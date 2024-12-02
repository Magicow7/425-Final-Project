using Combat.Weapon;
using Stat;
using UnityEngine;

public class ShotWand : Weapon
{
    [SerializeField] private Transform _bulletSpawnPoint;

    [SerializeField] private Bullet _bulletPrefab;

    [SerializeField] private Camera _mainCamera;

    [SerializeField] private LayerMask _bulletLayer;

    [SerializeField] private float _bulletSpeed = 10f;

    [SerializeField] private float _bulletLifespan = 10f;

    [SerializeField] private float _fireRate = 0.1f;

    [SerializeField] private float _explosionRadius;

    [SerializeField] private float _explosionDamage;

    [SerializeField] private float _explosionTime;

    [SerializeField] private float _explosionDamageOverTime;

    [SerializeField] private bool _isExplode;

    private float _nextFireTime;

    protected override bool Fire()
    {
        if (Time.time > _nextFireTime)
        {
            if (_bulletPrefab.name == "Bullet 1")
            {
                SoundManager.PlaySound(SoundManager.Sound.ShotSpellCast);
            }
            else
            {
                SoundManager.PlaySound(SoundManager.Sound.GrenadeThrow);
            }

            _nextFireTime = Time.time + _fireRate;

            var ray = _mainCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
            RaycastHit hit;

            Vector3 targetPoint;

            if (Physics.Raycast(ray, out hit, ~_bulletLayer))
            {
                targetPoint = hit.point;
            }
            else
            {
                targetPoint = ray.GetPoint(1000);
            }

            var direction = (targetPoint - _bulletSpawnPoint.position).normalized;

            var bullet = Instantiate(_bulletPrefab, _bulletSpawnPoint.position, _bulletSpawnPoint.rotation);
            bullet.Setup(_damage, _explosionRadius, _explosionDamage, _explosionTime, _explosionDamageOverTime, _bulletLifespan);
            if (bullet.Rigidbody != null)
            {
                bullet.Rigidbody.velocity = direction * _bulletSpeed;
            }

            return true;
        }

        return false;
    }
    
    public override void SetStats()
    {
        float mult = Random.Range(0.8f, 1.3f);
        _manaPerShot = 30 * Mathf.Pow(mult, 2);
        _damage = 45 * Mathf.Pow(1 + PlayerStats.Instance.WeaponPower.Value, 2) * mult;
        _fireRate = 1 / (1 + PlayerStats.Instance.WeaponPower.Value) / mult;
        _bulletSpeed = 5 * mult;
        _bulletLifespan = 10 * mult;

        if (_isExplode)
        {
            int type = Random.Range(1, 4);
            if (type == 1)
            {
                _manaPerShot *= 1.5f;
                _explosionRadius = Random.Range(1f, 3f) * mult;
                _explosionDamage = 3f / _explosionRadius * 10 * (1 + PlayerStats.Instance.WeaponPower.Value) * mult;
                _explosionTime = 1;
                _explosionDamageOverTime = 0;
            }
            else if (type == 2)
            {
                _manaPerShot *= 1.5f;
                _explosionRadius = Random.Range(1f, 3f) * mult;
                _explosionDamage = 0;
                _explosionTime = 2 * (1 + PlayerStats.Instance.WeaponPower.Value) * mult;
                _explosionDamageOverTime = 3f / _explosionRadius * 2 * (1 + PlayerStats.Instance.WeaponPower.Value) * mult;
            }
            else if (type == 3)
            {
                _manaPerShot *= 1.5f;
                _explosionRadius = Random.Range(1f, 3f) * mult;
                _explosionDamage = 3f / _explosionRadius * 5 * (1 + PlayerStats.Instance.WeaponPower.Value) * mult;
                _explosionTime = 2 * (1 + PlayerStats.Instance.WeaponPower.Value) * mult;
                _explosionDamageOverTime = 3f / _explosionRadius * 1 * (1 + PlayerStats.Instance.WeaponPower.Value) * mult;
            }
        }
        else
        {
            int type = Random.Range(1, 3);
            if (type == 1)
            {
                _damage *= 2;
                _manaPerShot *= 1;
                _explosionRadius = 0;
                _explosionDamage = 0;
                _explosionTime = 0;
                _explosionDamageOverTime = 0;
            }
            else if (type == 2)
            {
                _fireRate /= 2;
                _manaPerShot /= 2;
                _explosionRadius = 0;
                _explosionDamage = 0;
                _explosionTime = 0;
                _explosionDamageOverTime = 0;
            }
        }
    }
}