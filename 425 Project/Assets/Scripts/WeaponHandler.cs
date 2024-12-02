using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class WeaponHandler : MonoBehaviour
{
    // Start is called before the first frame update
    public static WeaponHandler instance;
    private GameObject _activeWeapon;

    [FormerlySerializedAs("weapons")] public List<GameObject> _weapons;
    [FormerlySerializedAs("weaponVisuals")] public List<GameObject> _weaponVisuals;
    void Start()
    {
        instance = this;
    }

    //activate the new weapon
    public void ActivateWeapon(int index){
        for(int i = 0; i < _weapons.Count; i++){
            _weapons[i].SetActive(false);
            _weaponVisuals[i].SetActive(false);
        }
        _weapons[index].SetActive(true);
        _weaponVisuals[index].SetActive(true);
    }
}
