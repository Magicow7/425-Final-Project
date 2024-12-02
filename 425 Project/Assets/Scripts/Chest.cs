using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Serialization;

public class Chest : MonoBehaviour
{
    [FormerlySerializedAs("taskText")] public TextMeshProUGUI _taskText;

    [FormerlySerializedAs("timerText")] public TextMeshPro _timerText;
    private bool _playerInRange = false;

    private bool _playerInChargeRadius = true;
    private bool _opened = false;

    private bool _charging = false;
    private bool _canFire = false;
    private bool _hasWand = false;
    
    [FormerlySerializedAs("chestSpeed")] public float _chestSpeed = 40f;
    [FormerlySerializedAs("wand")] public GameObject _wand;

    [FormerlySerializedAs("lid")] public GameObject _lid;

    [FormerlySerializedAs("weaponIndex")] public int _weaponIndex = 0;

    [FormerlySerializedAs("chestWeaponEffects")] public List<GameObject> _chestWeaponEffects;

    [FormerlySerializedAs("tutorialChest")] public bool _tutorialChest = false;

    [FormerlySerializedAs("tutorialStartWeaponForceIndex")] public int _tutorialStartWeaponForceIndex = 3;

    [FormerlySerializedAs("normalOpenTime")] public float _normalOpenTime = 60;

    [FormerlySerializedAs("tutorialOpenTime")] public float _tutorialOpenTime = 5;

    [FormerlySerializedAs("dechargeDistance")] public float _dechargeDistance = 5;

    private float _maxTimeRemaining;
    private float _timeRemaining;
    private bool _uniqueFail = true;

    private Camera _mainCam;

    // Start is called before the first frame update
    void Start()
    {
        _mainCam = Camera.main;
        
        if(_tutorialChest){
            _weaponIndex = _tutorialStartWeaponForceIndex;
        }else{
            _weaponIndex = Random.Range(0, WeaponHandler.instance._weapons.Count);
        }
        Debug.Log("weaponIndex is" + _weaponIndex);
        if(_tutorialChest){
            TextUpdates.Instance.UpdateTaskText("Use WASD keys to move to the chest.");
            _maxTimeRemaining = _tutorialOpenTime;
        }else{
            _maxTimeRemaining = _normalOpenTime;
        }
        _timeRemaining = _maxTimeRemaining;

    }

    // Update is called once per frame
    void Update()
    {
        if(_charging && !_opened){
            _playerInChargeRadius = Vector3.Distance(transform.position, _mainCam.transform.position) < _dechargeDistance;
            if(_playerInChargeRadius){
                _uniqueFail = true;
                _timerText.color = new Color(1,1,0,1);
                _timeRemaining -= Time.deltaTime;
                if(_timeRemaining < 0){
                    SoundManager.PlaySound(SoundManager.Sound.StoodGroundSuccess);
                    _opened = true;
                    MeshRenderer chestWandRender = _wand.GetComponent<MeshRenderer>();
                    chestWandRender.enabled = true;
                    StartCoroutine(SetLid(true));
                    StartCoroutine(MoveWand());
                    _chestWeaponEffects[_weaponIndex].SetActive(true);
                }
            }else{
                if (_uniqueFail)
                {
                    SoundManager.PlaySound(SoundManager.Sound.StoodGroundFail);
                    _uniqueFail = false;
                }
                _timerText.color = new Color(1,0,0,1);
                _timeRemaining += Time.deltaTime;
                if(_timeRemaining > _maxTimeRemaining){
                    _timeRemaining = _maxTimeRemaining;
                }
            }
            _timerText.text = Mathf.Round(_timeRemaining).ToString();
            _timerText.transform.LookAt(_mainCam.transform.position);
        }else{
            _timerText.text = "";
        }
        
        if (_playerInRange && !_opened && !_charging)
        {            
            if(_tutorialChest){
                TextUpdates.Instance.UpdateTaskText("Defend the chest until it opens.");
            }
            _charging = true; 
        }

        if (Input.GetKeyDown(KeyCode.E) && _playerInRange && _opened && !_hasWand)
        {
            _hasWand = true;
            Debug.LogWarning("Presed e");
            if(_tutorialChest){
                TextUpdates.Instance.UpdateTaskText("Now practice shooting your wand using left click.");
            }

            GameObject playerwand = GameObject.FindWithTag("MainCamera")?.transform.Find("wand")?.gameObject;
            MeshRenderer chestWandRender = _wand.GetComponent<MeshRenderer>();
            chestWandRender.enabled = false;
            _chestWeaponEffects[_weaponIndex].SetActive(false);
            if (playerwand != null)
            {
                Debug.LogWarning("found");
                MeshRenderer meshRenderer = playerwand.GetComponent<MeshRenderer>();
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
            if(_tutorialChest){
                TextUpdates.Instance.UpdateTaskText("Search the dungeon for chests & beware of monsters.");
            }
            StartCoroutine(ClearTextAfterSeconds(5f));

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
        Vector3 targetWandPos = transform.position + new Vector3(0, .3f, 0);
        yield return new WaitForSeconds(0.3f);
        while (_wand.transform.position.y < targetWandPos.y)
        {
            _wand.transform.position = Vector3.MoveTowards(_wand.transform.position, targetWandPos, .3f * Time.deltaTime);
            yield return null;
        }
        if(_tutorialChest){
            TextUpdates.Instance.UpdateTaskText("Press 'E' to collect your wand.");
        }
        
    }

    private IEnumerator ClearTextAfterSeconds(float s)
    {
        yield return new WaitForSeconds(s);
        Debug.Log("MAKING EMPYTY");
        TextUpdates.Instance.UpdateTaskText("");
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.name == "PlayerModel")
        {
            _playerInRange = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.name == "PlayerModel")
        {
            _playerInRange = false;
        }
    }

   

}
