using UnityEngine;

namespace Utils
{
    public class TriggerEvent : MonoBehaviour
    {
        public delegate void OnTriggerDelegate(GameObject gameObject, Collider collider);

        private void OnTriggerEnter(Collider other)
        {
            OnTriggerEnterEvent?.Invoke(gameObject, other);
        }

        private void OnTriggerExit(Collider other)
        {
            OnTriggerExitEvent?.Invoke(gameObject, other);
        }

        private void OnTriggerStay(Collider other)
        {
            OnTriggerStayEvent?.Invoke(gameObject, other);
        }

        public event OnTriggerDelegate? OnTriggerEnterEvent;

        public event OnTriggerDelegate? OnTriggerStayEvent;

        public event OnTriggerDelegate? OnTriggerExitEvent;
    }
}