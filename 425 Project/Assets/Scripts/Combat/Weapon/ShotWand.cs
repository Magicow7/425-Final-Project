using Combat.Weapon;
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
}