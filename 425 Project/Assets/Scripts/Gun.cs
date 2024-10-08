using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    public Transform bulletSpawnPoint;
    public GameObject bulletPrefab;
    public float bulletSpeed ;
    public Camera mainCamera;  // Reference to the main camera
    public ManaBar manaBar;

    private const int MANA_PER_SHOT = 20;

    private void Start()
    {
        //manaBar = manaBar.GetComponent<ManaBar>();
    }

    void Update()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            if (manaBar.UseMana(MANA_PER_SHOT) == true)
            {
                // Ray from the center of the screen
                Ray ray = mainCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
                RaycastHit hit;

                Vector3 targetPoint;


                if (Physics.Raycast(ray, out hit))
                {
                    targetPoint = hit.point;
                }
                else
                {
                    targetPoint = ray.GetPoint(1000);
                }


                Vector3 direction = (targetPoint - bulletSpawnPoint.position).normalized;

                // Instantiate the bullet
                var bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);

                // Set bullet velocity towards the target point
                bullet.GetComponent<Rigidbody>().velocity = direction * bulletSpeed;
            } else
            {
                // not enough mana to shoot
            }
            
        }
    }
}
