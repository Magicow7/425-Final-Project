using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class Chest : MonoBehaviour
{
    public TextMeshProUGUI taskText;
    private bool playerInRange = false;
    private bool opened = false;
    private bool canFire = false;
    private bool hasWand = false;
    
    public float chestSpeed = 40f;
    public GameObject wand;

    public GameObject lid;

    // Start is called before the first frame update
    void Start()
    {

        TextUpdates.Instance.UpdateTaskText("Use WASD keys to move to the chest.");

    }

    // Update is called once per frame
    void Update()
    {
        
        if (playerInRange && !opened)
        {
            opened = true;
            
            StartCoroutine(setLid(true));
            StartCoroutine(moveWand());
        }

        if (Input.GetKeyDown(KeyCode.E) && playerInRange && opened && !hasWand)
        {
            hasWand = true;
            Debug.LogWarning("Presed e");
            TextUpdates.Instance.UpdateTaskText("Now practice shooting your wand using left click.");
            
            canFire = true;
        }

        if (Input.GetKeyDown(KeyCode.Mouse0) && canFire && opened)
        {
            canFire = false;
            TextUpdates.Instance.UpdateTaskText("Search the dungeon for chests & beware of monsters.");
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
        TextUpdates.Instance.UpdateTaskText("Press 'E' to collect your wand.");
        
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
