using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Chest : MonoBehaviour
{

    private bool playerInRange = false;
    private bool opened = false;
    public float chestSpeed = 40f;

    public GameObject lid;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log("RUNNING UPDATE");
        if (Input.GetKeyDown(KeyCode.E) && playerInRange && !opened)
        {
            opened = true;
            Debug.Log("chest opened!");
            StartCoroutine(setLid(true));
        }
    }

    private IEnumerator setLid(bool open)
    {
        while (lid.transform.rotation.eulerAngles.x <= 350)
        {
            lid.transform.localRotation *= Quaternion.Euler(new Vector3(-50 * Time.deltaTime, 0, 0));
            yield return null;
        }
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
