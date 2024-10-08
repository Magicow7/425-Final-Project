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
        

        [Header("Settings")]
        public Vector2 clampInDegrees = new Vector2(360, 180);
        [Space]
        private Vector2 sensitivity = new Vector2(2, 2);
        [Space]
        public Vector2 smoothing = new Vector2(3, 3);

        [Header("First Person")]

        private Vector2 targetDirection;
        private Vector2 targetCharacterDirection;

        private Vector2 _mouseAbsolute;
        private Vector2 _smoothMouse;

        private Vector2 mouseDelta;
        
        protected override void OnInitialize()
        {
            // Set target direction to the camera's initial orientation.
            targetDirection = transform.localRotation.eulerAngles;

            _characterTransform = locomotionManager.CharacterController.transform;
            
            // Set target direction for the character body to its inital state.
            if (_characterTransform)
                targetCharacterDirection = _characterTransform.transform.localRotation.eulerAngles;
        
            Cursor.lockState = CursorLockMode.Locked;
        }

        public override void OnUpdate(float deltaTime)
        {
            // Allow the script to clamp based on a desired target value.
            var targetOrientation = Quaternion.Euler(targetDirection);
            var targetCharacterOrientation = Quaternion.Euler(targetCharacterDirection);

            // Get raw mouse input for a cleaner reading on more sensitive mice.
            mouseDelta = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));

            // Scale input against the sensitivity setting and multiply that against the smoothing value.
            mouseDelta = Vector2.Scale(mouseDelta, new Vector2(sensitivity.x * smoothing.x, sensitivity.y * smoothing.y));

            // Interpolate mouse movement over time to apply smoothing delta.
            _smoothMouse.x = Mathf.Lerp(_smoothMouse.x, mouseDelta.x, 1f / smoothing.x);
            _smoothMouse.y = Mathf.Lerp(_smoothMouse.y, mouseDelta.y, 1f / smoothing.y);

            // Find the absolute mouse movement value from point zero.
            _mouseAbsolute += _smoothMouse;

            // Clamp and apply the local x value first, so as not to be affected by world transforms.
            if (clampInDegrees.x < 360)
                _mouseAbsolute.x = Mathf.Clamp(_mouseAbsolute.x, -clampInDegrees.x * 0.5f, clampInDegrees.x * 0.5f);

            // Then clamp and apply the global y value.
            if (clampInDegrees.y < 360)
                _mouseAbsolute.y = Mathf.Clamp(_mouseAbsolute.y, -clampInDegrees.y * 0.5f, clampInDegrees.y * 0.5f);

            LocomotionManager.Instance.Camera.transform.localRotation = Quaternion.AngleAxis(-_mouseAbsolute.y, targetOrientation * Vector3.right) * targetOrientation;

            // If there's a character body that acts as a parent to the camera
            if (_characterTransform)
            {
                var yRotation = Quaternion.AngleAxis(_mouseAbsolute.x, Vector3.up);
                _characterTransform.transform.localRotation = yRotation * targetCharacterOrientation;

                yRotation = Quaternion.AngleAxis(_mouseAbsolute.x, LocomotionManager.Instance.Camera.transform.InverseTransformDirection(Vector3.up));
                LocomotionManager.Instance.Camera.transform.localRotation *= yRotation;
            }
            else
            {
                var yRotation = Quaternion.AngleAxis(_mouseAbsolute.x, LocomotionManager.Instance.Camera.transform.InverseTransformDirection(Vector3.up));
                LocomotionManager.Instance.Camera.transform.localRotation *= yRotation;
            }
        }
    }
}