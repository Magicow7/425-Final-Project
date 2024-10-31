using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    public Transform bulletSpawnPoint;
    public GameObject bulletPrefab;
    public Camera mainCamera;
    public ManaBar manaBar;

    // TEMPORARY
    public HealthBar healthBar;

    public LayerMask bulletLayer;
    public GameObject player;

    private float rate = 0.1f;
    private float nextFireTime = 0f;
    private float bulletSpeed = 10f;
    private const int MANA_PER_SHOT = 20;

    void Update()
    {
        if (Input.GetButton("Fire1") && Time.time > nextFireTime)
        {
            nextFireTime = Time.time + rate;
            healthBar.AddHealth(5);
            if (manaBar.UseMana(MANA_PER_SHOT))
            {

                Ray ray = mainCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
                RaycastHit hit;

                Vector3 targetPoint;


                if (Physics.Raycast(ray, out hit, ~bulletLayer))
                {
                    targetPoint = hit.point;
                }
                else
                {
                    targetPoint = ray.GetPoint(1000);
                }


                Vector3 direction = (targetPoint - bulletSpawnPoint.position).normalized;

                var bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);

                bullet.GetComponent<Rigidbody>().velocity = direction * bulletSpeed;
            }
            else
            {
                // not enough mana to shoot
            }

        }
    }
}
