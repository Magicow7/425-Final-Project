using Locomotion;
using UnityEngine;

namespace Locomotion
{
    public class MovementProvider : LocomotionProvider
    {
        [SerializeField, Tooltip("How fast the player moves.")]
        private float _walkSpeed = 5f;

        [SerializeField, Tooltip("How fast the player moves when sprinting.")]
        private float _sprintSpeed = 10f;
        
        private bool _isSprinting = false;
        private float _currentMoveSpeed;
        
        private Transform _characterTransform = null!;
        
        protected override void OnInitialize()
        {
            _currentMoveSpeed = _walkSpeed;
            _characterTransform = locomotionManager.CharacterController.transform;
        }

        public override void OnFixedUpdate(float deltaTime)
        {
            if (!locomotionManager.IsGrounded)
            {
                return;
            }

            HandleMovement();
        }

        private void HandleMovement()
        {
            float vertical = UnityEngine.Input.GetAxisRaw("Vertical");
            float horizontal = UnityEngine.Input.GetAxisRaw("Horizontal");
            
            var input = new Vector2(horizontal, vertical);
            
            if (_isSprinting && input == Vector2.zero)
            {
                ToggleSprint();
            }
            
            input.Normalize();

            if (UnityEngine.Input.GetKey(KeyCode.LeftShift) != _isSprinting)
            {
                ToggleSprint();
            }
            
            Move(LocomotionManager.Instance.Camera.transform, new Vector2(horizontal, vertical));
        }
        
        public void ToggleSprint()
        {
            _isSprinting = !_isSprinting;
            _currentMoveSpeed = _isSprinting ? _sprintSpeed : _walkSpeed;
        }
        
        public void Move(Transform relativeTo, Vector2 input)
        {
            var forward = relativeTo.forward;
            var right = relativeTo.right;
            
            forward.y = 0;
            right.y = 0;
            
            forward.Normalize();
            right.Normalize();
            input.Normalize();
            
            var targetVelocity = (forward * input.y + right * input.x) * _currentMoveSpeed;
            SetVelocityXZ(targetVelocity);

            SetMovementState(MovementState.Moving, input != Vector2.zero);
        }
    }
}