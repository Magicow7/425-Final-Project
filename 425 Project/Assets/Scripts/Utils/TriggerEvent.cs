using UnityEngine;
using UnityEngine.Events;

namespace Utils
{
    public class TriggerEvent : MonoBehaviour
    {
        public event OnTriggerDelegate? OnTriggerEnterEvent;
        
        public event OnTriggerDelegate? OnTriggerStayEvent;
        
        public event OnTriggerDelegate? OnTriggerExitEvent;
        
        private void OnTriggerEnter(Collider other)
        {
            OnTriggerEnterEvent?.Invoke(gameObject, other);
        }
        
        private void OnTriggerStay(Collider other)
        {
            OnTriggerStayEvent?.Invoke(gameObject, other);
        }
        
        private void OnTriggerExit(Collider other)
        {
            OnTriggerExitEvent?.Invoke(gameObject, other);
        }

        public delegate void OnTriggerDelegate(GameObject gameObject, Collider collider);
    }
}