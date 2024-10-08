using System.Collections;
using System.Collections.Generic;
using Combat;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int life = 10;
    private bool _canHit = false;

    void Awake()
    {
        Destroy(gameObject, life);
    }

    IEnumerator CanCollide()
    {
        yield return new WaitForSeconds(0.3f);
        _canHit = true;

    }

    void OnCollisionEnter(Collision collision)
    {
        if (!_canHit)
        {
            return;
        }
        
        if (collision.gameObject.TryGetComponent<IDamageable>(out IDamageable damageable))
        {
            damageable.TakeDamage(life);
        }
        
        Destroy(gameObject);
    }
}
