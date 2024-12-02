using System.Collections;
using Combat.Weapon;
using Stat;
using UnityEngine;

public class LaserWand : Weapon
{
    [SerializeField] private Transform _bulletSpawnPoint;

    [SerializeField] private Camera _mainCamera;

    [SerializeField] private LayerMask _bulletLayer;

    [SerializeField] private float _lineRemoveTime = 2f;

    [SerializeField] private int _bounces = 3;

    [SerializeField] private float _fireRate = 0.1f;

    [SerializeField] private float _explosionRadius;

    [SerializeField] private float _explosionDamage;

    [SerializeField] private float _explosionTime;

    [SerializeField] private float _explosionDamageOverTime;

    [SerializeField] private Explosion _explosion;

    private LineRenderer _lineRenderer;

    private float _nextFireTime;
    private Coroutine _removeLineCoroutine = null!;

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
            var ray = _mainCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
            RaycastHit hit;
            var hitList = new Vector3[_bounces + 2];

            if (_removeLineCoroutine != null)
            {
                StopCoroutine(_removeLineCoroutine);
            }

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

            int bounces = _bounces;
            var direction = (targetPoint - _bulletSpawnPoint.position).normalized;

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
            _removeLineCoroutine = StartCoroutine(_RemoveLine());

            return true;
        }

        return false;
    }

    private IEnumerator _RemoveLine()
    {
        yield return new WaitForSeconds(Mathf.Min(_explosionTime, _lineRemoveTime));
        _lineRenderer.positionCount = 0;
    }

    public override void SetStats()
    {
        float mult = Random.Range(0.8f, 1.3f);
        _manaPerShot = 85 * Mathf.Pow(mult, 2);
        _explosionRadius = Random.Range(1f, 3f) * mult;
        _explosionDamage = 10 * 1 + PlayerStats.Instance.WeaponPower.Value * mult;
        _explosionTime = 1;
        _fireRate = 1 / (1 + PlayerStats.Instance.WeaponPower.Value) / mult;
        _bounces = (int)(2 * (1 + PlayerStats.Instance.WeaponPower.Value) * mult);
        _explosionDamageOverTime = 0;

        int type = Random.Range(1, 6);
        if (type == 1)
        {
            _explosionDamage *= 1.5f;
        }
        else if (type == 2)
        {
            _manaPerShot /= 1.5f;
            _fireRate /= 1.5f;
        }
        else if (type == 3)
        {
            _explosionRadius *= 1.5f;
        }
        else if (type == 4)
        {
            _manaPerShot *= 1.5f;
            _bounces = 2 * _bounces + 1;
        }
        else if (type == 5)
        {
            _manaPerShot *= 1.5f;
            _explosionDamage = 0;
            _explosionTime = 2 * (1 + PlayerStats.Instance.WeaponPower.Value) * mult;
            _explosionDamageOverTime = 3f / _explosionRadius * 2 * (1 + PlayerStats.Instance.WeaponPower.Value) * mult;
        }
    }
}