using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Chest : MonoBehaviour
{

    private bool playerInRange = false;
    private bool opened = false;

    // Start is called before the first frame update
    void Start()
    {
     
    }

    // Update is called once per frame
    void Update()
    {
       
       if (Input.GetKeyDown(KeyCode.E) && playerInRange && !opened)
        {
            opened = true;
            Debug.Log("chest opened!");
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
