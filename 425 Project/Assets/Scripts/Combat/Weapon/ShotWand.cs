using System;
using System.Collections;
using System.Collections.Generic;
using Combat.Weapon;
using UnityEngine;
using UnityEngine.Serialization;

public class ShotWand : Weapon
{
    [SerializeField]
    private Transform _bulletSpawnPoint;
    [SerializeField]
    private Bullet _bulletPrefab;
    [SerializeField]
    private Camera _mainCamera;
    [SerializeField]
    private LayerMask _bulletLayer;

    private float _nextFireTime = 0f;
    
    [SerializeField]
    private float _bulletSpeed = 10f;
    [SerializeField]
    private float _bulletLifespan = 10f;
    [SerializeField]
    private float _fireRate = 0.1f;

    protected override bool Fire()
    {
        if (Time.time > _nextFireTime)
        {
            _nextFireTime = Time.time + _fireRate;
            
            Ray ray = _mainCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
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

            Vector3 direction = (targetPoint - _bulletSpawnPoint.position).normalized;

            var bullet = Instantiate(_bulletPrefab, _bulletSpawnPoint.position, _bulletSpawnPoint.rotation);
            bullet.Setup(_damage, _bulletLifespan);
            bullet.Rigidbody.velocity = direction * _bulletSpeed;

            return true;
        }

        return false;
    }
}
