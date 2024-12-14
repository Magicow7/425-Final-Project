using System.Collections.Generic;
using Combat.Weapon;
using UnityEngine;
using UnityEngine.Serialization;
using Stat;

public class WeaponHandler : MonoBehaviour
{
    // Start is called before the first frame update
    public static WeaponHandler instance;

    [FormerlySerializedAs("weapons")] public List<Weapon> _weapons;

    [FormerlySerializedAs("weaponVisuals")]
    public List<GameObject> _weaponVisuals;

    private GameObject _activeWeapon;

    private void Start()
    {
        instance = this;
    }

    private void Update(){
        if(Input.GetKeyDown("1")){
            ActivateWeapon(3);
        }
        if(Input.GetKeyDown("2")){
            ActivateWeapon(2);
        }
        if(Input.GetKeyDown("3")){
            ActivateWeapon(1);
        }
        if(Input.GetKeyDown("4")){
            ActivateWeapon(0);
        }
        if(Input.GetKeyDown("5")) {
            PlayerStats.Instance.WeaponPower.Value += 0.1f;
        }
        if(Input.GetKeyDown("6")) {
            PlayerStats.Instance.WeaponPower.Value += 1f;
        }
        if(Input.GetKeyDown("7")) {
            PlayerStats.Instance.WeaponPower.Value += 10f;
        }
    }

    //activate the new weapon
    public void ActivateWeapon(int index)
    {
        for (var i = 0; i < _weapons.Count; i++)
        {
            _weapons[i].gameObject.SetActive(false);
            _weapons[i].SetStats();
            _weaponVisuals[i].SetActive(false);
        }

        _weapons[index].gameObject.SetActive(true);
        _weaponVisuals[index].SetActive(true);
    }
}