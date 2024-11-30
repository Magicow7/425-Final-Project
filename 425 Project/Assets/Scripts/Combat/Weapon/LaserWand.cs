using System;
using System.Collections;
using System.Collections.Generic;
using Combat.Weapon;
using UnityEngine;
using UnityEngine.Serialization;

public class LaserWand : Weapon
{
    [SerializeField]
    private Transform _bulletSpawnPoint;
    [SerializeField]
    private Camera _mainCamera;
    [SerializeField]
    private LayerMask _bulletLayer;

    private float _nextFireTime = 0f;
    
    [SerializeField]
    private int _bounces = 3;
    [SerializeField]
    private float _fireRate = 0.1f;
    
    [SerializeField]
    private float _explosionRadius = 0f;
    [SerializeField]
    private float _explosionDamage = 0f;
    [SerializeField]
    private float _explosionTime = 0f;
    [SerializeField]
    private float _explosionDamageOverTime = 0f;
    
    [SerializeField]
    private Explosion _explosion;
    
    private LineRenderer _lineRenderer;

    private void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();
    }

    protected override bool Fire()
    {
        if (Time.time > _nextFireTime)
        {
            _nextFireTime = Time.time + _fireRate;
            SoundManager.PlaySound(SoundManager.Sound.LaserShoot);
            Ray ray = _mainCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
            RaycastHit hit;
            Vector3[] hitList = new Vector3[_bounces + 2];
            _lineRenderer.positionCount = hitList.Length;
            hitList[0] = _bulletSpawnPoint.position;

            Vector3 targetPoint;
            
            if (Physics.Raycast(ray, out hit, float.MaxValue, _bulletLayer))
            {
                targetPoint = hit.point;
            }
            else
            {
                targetPoint = ray.GetPoint(1000);
            }

            hitList[1] = targetPoint;
            SoundManager.PlaySound(SoundManager.Sound.LaserBlast, targetPoint);
            Instantiate(_explosion, targetPoint, Quaternion.identity)
                .Explode(_explosionRadius, _explosionDamage, _explosionTime, _explosionDamageOverTime);

            var bounces = _bounces;
            Vector3 direction = (targetPoint - _bulletSpawnPoint.position).normalized;

            while (bounces > 0)
            {
                direction = Vector3.Reflect(direction, hit.normal);
                if (!Physics.Raycast(targetPoint + direction * 0.1f, direction, out hit, float.MaxValue, _bulletLayer))
                {
                    break;
                }
                targetPoint = hit.point;
                hitList[_bounces - bounces + 2] = targetPoint;
                SoundManager.PlaySound(SoundManager.Sound.LaserBlast, targetPoint);
                Instantiate(_explosion, targetPoint, Quaternion.identity)
                    .Explode(_explosionRadius, _explosionDamage, _explosionTime, _explosionDamageOverTime);
                bounces--;
            }

            _lineRenderer.SetPositions(hitList);

            return true;
        }

        return false;
    }
}
