using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerInteraction : MonoBehaviour
{
    public TextMeshProUGUI notification;

    // Start is called before the first frame update
    void Start()
    {
       

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            
        }
    }

    void OnTriggerEnter(Collider other)
    {
        notification.text = "Press E to open";
    }

    void OnTriggerExit(Collider other)
    {
        notification.text = "";
    }
}
