using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponHandler : MonoBehaviour
{
    // Start is called before the first frame update
    public static WeaponHandler instance;
    private GameObject activeWeapon;

    public List<GameObject> weapons;
    public List<GameObject> weaponVisuals;
    void Start()
    {
        instance = this;
    }

    //activate the new weapon
    public void ActivateWeapon(int index){
        for(int i = 0; i < weapons.Count; i++){
            weapons[i].SetActive(false);
            weaponVisuals[i].SetActive(false);
        }
        weapons[index].SetActive(true);
        weaponVisuals[index].SetActive(true);
    }
}
