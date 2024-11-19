using System.Collections;
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

        [SerializeField, Tooltip("How fast friction decelerates the player.")]
        private float _frictionSpeed = 2f;

        [SerializeField, Tooltip("The max speed someone can move at.")]
        private float _maxSpeed = 30f;

        [SerializeField]
        private float _jumpSpeed = 3f;
        
        [SerializeField]
        private float _wallJumpCooldown = 2f;
        
        [SerializeField]
        private float _wallJumpDistanceThreshold = 5f;
        
        private bool _isSprinting = false;
        private float _currentMoveSpeed;
        private float _currentMaxSpeed;
        
        private Transform _characterTransform = null!;

        private bool _wallJump = true;
        private bool _canJump = true;
        
        private Vector3 _lastWallJumpPos = Vector3.zero;

        private GravityProvider _gravityProvider;
        
        protected override void OnInitialize()
        {
            _currentMoveSpeed = 0;
            _currentMaxSpeed = _walkSpeed;
            _characterTransform = locomotionManager.CharacterController.transform;
            _gravityProvider = locomotionManager.GetProvider<GravityProvider>();
        }

        public override void OnFixedUpdate(float deltaTime)
        {
            if (locomotionManager.IsGrounded)
            {
                _wallJump = true;
            }

            HandleMovement();
        }

        private void HandleMovement()
        {
            float vertical = UnityEngine.Input.GetAxisRaw("Vertical");
            float horizontal = UnityEngine.Input.GetAxisRaw("Horizontal");
            
            var input = new Vector2(horizontal, vertical);
            
            if (_isSprinting && input == Vector2.zero && vertical > 0)
            {
                ToggleSprint();
            }
            
            input.Normalize();

            if (UnityEngine.Input.GetKey(KeyCode.Space) && _canJump)
            {
                if (locomotionManager.IsGrounded)
                {
                    Jump(_jumpSpeed);
                    _lastWallJumpPos = Vector3.zero;
                }
                else if (locomotionManager.IsTouchingWall && _wallJump && 
                         (locomotionManager.CharacterController.transform.position - _lastWallJumpPos).magnitude >= _wallJumpDistanceThreshold)
                {
                    _lastWallJumpPos = locomotionManager.CharacterController.transform.position;
                    Jump(_jumpSpeed / 2);
                    StartCoroutine(_WallJumpCooldown());
                }
            }

            if (UnityEngine.Input.GetKey(KeyCode.LeftShift) != _isSprinting)
            {
                ToggleSprint();
            }
            
            Move(LocomotionManager.Instance.Camera.transform, new Vector2(horizontal, vertical));
        }
        
        public void ToggleSprint()
        {
            _isSprinting = !_isSprinting;
            _currentMaxSpeed = _isSprinting ? _sprintSpeed : _walkSpeed;
        }
        
        public void Jump(float speed)
        {
            StartCoroutine(_JumpCooldown());
            SetVelocityY(speed);
        }

        private IEnumerator _JumpCooldown()
        {
            _gravityProvider.SetActive(false);
            _canJump = false;
            yield return new WaitForSeconds(0.2f);
            _canJump = true;
            _gravityProvider.SetActive(true);
        }

        private IEnumerator _WallJumpCooldown()
        {
            _wallJump = false;
            yield return new WaitForSeconds(_wallJumpCooldown);
            _wallJump = true;
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
            
            var targetVelocity = (forward * input.y + right * input.x);
            if (targetVelocity == Vector3.zero)
            {
                _currentMoveSpeed = 0;
            }
            else if (locomotionManager.IsGrounded)
            {
                if (locomotionManager.VelocityXZ.magnitude > _currentMaxSpeed)
                {
                    _currentMoveSpeed -= Mathf.Max(
                        _currentMaxSpeed * _frictionSpeed * Time.fixedDeltaTime, 
                        locomotionManager.VelocityXZ.magnitude / 100);
                }
                else
                {
                    _currentMoveSpeed = _currentMaxSpeed;
                }
            }
            else
            {
                if (_isSprinting)
                {
                    _currentMoveSpeed += _currentMaxSpeed * Time.fixedDeltaTime;
                }

                if (_currentMoveSpeed > _maxSpeed)
                {
                    _currentMoveSpeed = _maxSpeed;
                }
            }

            float horizontalSpeed = _currentMoveSpeed > _currentMaxSpeed ? _currentMaxSpeed : _currentMoveSpeed;
            SetVelocityXZ(forward * (input.y * _currentMoveSpeed) + right * (input.x * horizontalSpeed));

            SetMovementState(MovementState.Moving, input != Vector2.zero);
        }
    }
}