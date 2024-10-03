using Locomotion;
using UnityEngine;

namespace Locomotion
{
    public class RotationProvider : LocomotionProvider
    {
        [SerializeField] private float _sensitivity = 1f;
        [SerializeField] private bool _invertVertical;
        [SerializeField] private Transform _headTransform;

        private Transform _characterTransform;
        
        protected override void OnInitialize()
        {
            _characterTransform = locomotionManager.CharacterController.transform;
            Cursor.lockState = CursorLockMode.Locked;
        }

        public override void OnUpdate(float deltaTime)
        {
            var deltaMouse = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
            
            Vector2 deltaRotation = deltaMouse * _sensitivity;
            deltaRotation.y *= _invertVertical ? 1.0f : -1.0f;
    
            float pitchAngle = _headTransform.localEulerAngles.x;
            
            if (pitchAngle > 180)
                pitchAngle -= 360;
    
            pitchAngle = Mathf.Clamp(pitchAngle + deltaRotation.y, -90.0f, 90.0f);
    
            SetRotation(_characterTransform.eulerAngles.x + deltaRotation.x * 100);
            _headTransform.localRotation = Quaternion.Euler(pitchAngle, 0.0f, 0.0f);
        }
    }
}