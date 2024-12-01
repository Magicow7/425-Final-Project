using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class Chest : MonoBehaviour
{
    public TextMeshProUGUI taskText;

    public TextMeshPro timerText;
    private bool playerInRange = false;

    private bool playerInChargeRadius = true;
    private bool opened = false;

    private bool charging = false;
    private bool canFire = false;
    private bool hasWand = false;
    
    public float chestSpeed = 40f;
    public GameObject wand;

    public GameObject lid;

    public int weaponIndex = 0;

    public List<GameObject> chestWeaponEffects;

    public bool tutorialChest = false;

    public int tutorialStartWeaponForceIndex = 3;

    public float normalOpenTime = 60;

    public float tutorialOpenTime = 5;

    public float dechargeDistance = 5;

    private float maxTimeRemaining;
    private float timeRemaining;

    private Camera mainCam;

    // Start is called before the first frame update
    void Start()
    {
        mainCam = Camera.main;
        
        if(tutorialChest){
            weaponIndex = tutorialStartWeaponForceIndex;
        }else{
            weaponIndex = Random.Range(0, WeaponHandler.instance.weapons.Count);
        }
        Debug.Log("weaponIndex is" + weaponIndex);
        if(tutorialChest){
            TextUpdates.Instance.UpdateTaskText("Use WASD keys to move to the chest.");
            maxTimeRemaining = tutorialOpenTime;
        }else{
            maxTimeRemaining = normalOpenTime;
        }
        timeRemaining = maxTimeRemaining;

    }

    // Update is called once per frame
    void Update()
    {
        if(charging && !opened){
            playerInChargeRadius = Vector3.Distance(transform.position, mainCam.transform.position) < dechargeDistance;
            if(playerInChargeRadius){
                timerText.color = new Color(1,1,0,1);
                timeRemaining -= Time.deltaTime;
                if(timeRemaining < 0){
                    opened = true;
                    MeshRenderer chestWandRender = wand.GetComponent<MeshRenderer>();
                    chestWandRender.enabled = true;
                    StartCoroutine(setLid(true));
                    StartCoroutine(moveWand());
                    chestWeaponEffects[weaponIndex].SetActive(true);
                }
            }else{
                timerText.color = new Color(1,0,0,1);
                timeRemaining += Time.deltaTime;
                if(timeRemaining > maxTimeRemaining){
                    timeRemaining = maxTimeRemaining;
                }
            }
            timerText.text = Mathf.Round(timeRemaining).ToString();
            timerText.transform.LookAt(mainCam.transform.position);
        }else{
            timerText.text = "";
        }
        
        if (playerInRange && !opened && !charging)
        {            
            if(tutorialChest){
                TextUpdates.Instance.UpdateTaskText("Defend the chest untill it opens.");
            }
            charging = true; 
        }

        if (Input.GetKeyDown(KeyCode.E) && playerInRange && opened && !hasWand)
        {
            hasWand = true;
            Debug.LogWarning("Presed e");
            if(tutorialChest){
                TextUpdates.Instance.UpdateTaskText("Now practice shooting your wand using left click.");
            }

            GameObject playerwand = GameObject.FindWithTag("MainCamera")?.transform.Find("wand")?.gameObject;
            MeshRenderer chestWandRender = wand.GetComponent<MeshRenderer>();
            chestWandRender.enabled = false;
            chestWeaponEffects[weaponIndex].SetActive(false);
            if (playerwand != null)
            {
                Debug.LogWarning("found");
                MeshRenderer meshRenderer = playerwand.GetComponent<MeshRenderer>();
                if (meshRenderer != null)
                {
                    meshRenderer.enabled = true; 
                }
            }
            WeaponHandler.instance.ActivateWeapon(weaponIndex);
            canFire = true;
        }

        if (Input.GetKeyDown(KeyCode.Mouse0) && canFire && opened)
        {
            canFire = false;
            if(tutorialChest){
                TextUpdates.Instance.UpdateTaskText("Search the dungeon for chests & beware of monsters.");
            }
            StartCoroutine(ClearTextAfterSeconds(5f));

        }
    }

    private IEnumerator setLid(bool open)
    {
        while (lid.transform.rotation.eulerAngles.x <= 350)
        {
            lid.transform.localRotation *= Quaternion.Euler(new Vector3(-80 * Time.deltaTime, 0, 0));
           
            yield return null;
        }
    }

    private IEnumerator moveWand()
    {
        Vector3 targetWandPos = transform.position + new Vector3(0, .3f, 0);
        yield return new WaitForSeconds(0.3f);
        while (wand.transform.position.y < targetWandPos.y)
        {
            wand.transform.position = Vector3.MoveTowards(wand.transform.position, targetWandPos, .3f * Time.deltaTime);
            yield return null;
        }
        if(tutorialChest){
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
            playerInRange = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.name == "PlayerModel")
        {
            playerInRange = false;
        }
    }

   

}
