using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Serialization;

public class PlayerInteraction : MonoBehaviour
{
    [FormerlySerializedAs("notification")] public TextMeshProUGUI _notification;

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
        _notification.text = "Press E to open";
    }

    void OnTriggerExit(Collider other)
    {
        _notification.text = "";
    }
}
