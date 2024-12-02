using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class Chest : MonoBehaviour
{
    [FormerlySerializedAs("taskText")] public TextMeshProUGUI _taskText;

    [FormerlySerializedAs("timerText")] public TextMeshPro _timerText;

    [FormerlySerializedAs("chestSpeed")] public float _chestSpeed = 40f;
    [FormerlySerializedAs("wand")] public GameObject _wand;

    [FormerlySerializedAs("lid")] public GameObject _lid;

    [FormerlySerializedAs("weaponIndex")] public int _weaponIndex;

    [FormerlySerializedAs("chestWeaponEffects")]
    public List<GameObject> _chestWeaponEffects;

    [FormerlySerializedAs("tutorialChest")]
    public bool _tutorialChest;

    [FormerlySerializedAs("tutorialStartWeaponForceIndex")]
    public int _tutorialStartWeaponForceIndex = 3;

    [FormerlySerializedAs("normalOpenTime")]
    public float _normalOpenTime = 60;

    [FormerlySerializedAs("tutorialOpenTime")]
    public float _tutorialOpenTime = 5;

    [FormerlySerializedAs("dechargeDistance")]
    public float _dechargeDistance = 5;

    private bool _canFire;

    private bool _charging;
    private bool _hasWand;

    private Camera _mainCam;

    private float _maxTimeRemaining;
    private bool _opened;

    private bool _playerInChargeRadius = true;
    private bool _playerInRange;
    private float _timeRemaining;
    private bool _uniqueFail = true;

    // Start is called before the first frame update
    private void Start()
    {
        _mainCam = Camera.main;

        if (_tutorialChest)
        {
            _weaponIndex = _tutorialStartWeaponForceIndex;
        }
        else
        {
            _weaponIndex = Random.Range(0, WeaponHandler.instance._weapons.Count);
        }

        Debug.Log("weaponIndex is" + _weaponIndex);
        if (_tutorialChest)
        {
            TextUpdates.Instance.UpdateTaskText("Use WASD keys to move to the chest.");
            _maxTimeRemaining = _tutorialOpenTime;
        }
        else
        {
            _maxTimeRemaining = _normalOpenTime;
        }

        _timeRemaining = _maxTimeRemaining;
    }

    // Update is called once per frame
    private void Update()
    {
        if (_charging && !_opened)
        {
            _playerInChargeRadius = Vector3.Distance(transform.position, _mainCam.transform.position) < _dechargeDistance;
            if (_playerInChargeRadius)
            {
                _uniqueFail = true;
                _timerText.color = new Color(1, 1, 0, 1);
                _timeRemaining -= Time.deltaTime;
                if (_timeRemaining < 0)
                {
                    SoundManager.PlaySound(SoundManager.Sound.StoodGroundSuccess);
                    _opened = true;
                    var chestWandRender = _wand.GetComponent<MeshRenderer>();
                    chestWandRender.enabled = true;
                    StartCoroutine(SetLid(true));
                    StartCoroutine(MoveWand());
                    _chestWeaponEffects[_weaponIndex].SetActive(true);
                }
            }
            else
            {
                if (_uniqueFail)
                {
                    SoundManager.PlaySound(SoundManager.Sound.StoodGroundFail);
                    _uniqueFail = false;
                }

                _timerText.color = new Color(1, 0, 0, 1);
                _timeRemaining += Time.deltaTime;
                if (_timeRemaining > _maxTimeRemaining)
                {
                    _timeRemaining = _maxTimeRemaining;
                }
            }

            _timerText.text = Mathf.Round(_timeRemaining).ToString();
            _timerText.transform.LookAt(_mainCam.transform.position);
        }
        else
        {
            _timerText.text = "";
        }

        if (_playerInRange && !_opened && !_charging)
        {
            if (_tutorialChest)
            {
                TextUpdates.Instance.UpdateTaskText("Defend the chest until it opens.");
            }

            _charging = true;
        }

        if (Input.GetKeyDown(KeyCode.E) && _playerInRange && _opened && !_hasWand)
        {
            _hasWand = true;
            Debug.LogWarning("Pressed e");
            if (_tutorialChest)
            {
                TextUpdates.Instance.UpdateTaskText("Now practice shooting your wand using left click.");
            }

            var playerwand = GameObject.FindWithTag("MainCamera")?.transform.Find("wand")?.gameObject;
            var chestWandRender = _wand.GetComponent<MeshRenderer>();
            chestWandRender.enabled = false;
            _chestWeaponEffects[_weaponIndex].SetActive(false);
            if (playerwand != null)
            {
                Debug.LogWarning("found");
                var meshRenderer = playerwand.GetComponent<MeshRenderer>();
                if (meshRenderer != null)
                {
                    meshRenderer.enabled = true;
                }
            }

            WeaponHandler.instance.ActivateWeapon(_weaponIndex);
            _canFire = true;
        }

        if (Input.GetKeyDown(KeyCode.Mouse0) && _canFire && _opened)
        {
            _canFire = false;
            if (_tutorialChest)
            {
                TextUpdates.Instance.UpdateTaskText("Search the dungeon for chests & beware of monsters.");
            }

            StartCoroutine(ClearTextAfterSeconds(5f));
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.name == "PlayerModel")
        {
            _playerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.name == "PlayerModel")
        {
            _playerInRange = false;
        }
    }

    private IEnumerator SetLid(bool open)
    {
        SoundManager.PlaySound(SoundManager.Sound.OpenChest, gameObject.transform.position);
        while (_lid.transform.rotation.eulerAngles.x <= 350)
        {
            _lid.transform.localRotation *= Quaternion.Euler(new Vector3(-80 * Time.deltaTime, 0, 0));

            yield return null;
        }
    }

    private IEnumerator MoveWand()
    {
        var targetWandPos = transform.position + new Vector3(0, .3f, 0);
        yield return new WaitForSeconds(0.3f);
        while (_wand.transform.position.y < targetWandPos.y)
        {
            _wand.transform.position = Vector3.MoveTowards(_wand.transform.position, targetWandPos, .3f * Time.deltaTime);
            yield return null;
        }

        if (_tutorialChest)
        {
            TextUpdates.Instance.UpdateTaskText("Press 'E' to collect your wand.");
        }
    }

    private IEnumerator ClearTextAfterSeconds(float s)
    {
        yield return new WaitForSeconds(s);
        Debug.Log("MAKING EMPYTY");
        TextUpdates.Instance.UpdateTaskText("");
    }
}