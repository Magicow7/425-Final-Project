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
        _canHit = true;
       
    }

   

    private void OnTriggerEnter(Collider other)
    {
        if (!_canHit)
        {
            return;
        }

        if (other.gameObject.TryGetComponent<IDamageable>(out IDamageable damageable))
        {
            damageable.TakeDamage(life);
            Destroy(gameObject);
        }

        
    }

    void OnCollisionEnter(Collision collision)
    {
        
    }
}
