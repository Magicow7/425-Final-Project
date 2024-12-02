using System.Collections.Generic;
using Combat.Weapon;
using UnityEngine;
using UnityEngine.Serialization;

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