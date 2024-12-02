using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerInteraction : MonoBehaviour
{
    [FormerlySerializedAs("notification")] public TextMeshProUGUI _notification;

    // Start is called before the first frame update
    private void Start() { }

    // Update is called once per frame
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E)) { }
    }

    private void OnTriggerEnter(Collider other)
    {
        _notification.text = "Press E to open";
    }

    private void OnTriggerExit(Collider other)
    {
        _notification.text = "";
    }
}