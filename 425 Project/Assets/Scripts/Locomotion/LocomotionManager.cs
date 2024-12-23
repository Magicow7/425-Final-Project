using System;
using System.Collections.Generic;
using UnityEngine;
using Utils;
using Utils.Singleton;

namespace Locomotion
{
    /// <summary>
    ///     Manages all locomotion providers that are a child of the provider holder.
    /// </summary>
    [SingletonAttribute(SingletonCreationMode.Auto, false)]
    public class LocomotionManager : SingletonMonoBehaviour<LocomotionManager>, ICustomUpdate, ICustomFixedUpdate
    {
        [field: SerializeField] public Camera Camera { get; private set; }

        [SerializeField] private CharacterController _characterController = null!;

        [SerializeField, Tooltip("All Locomotion Providers should be a first level child of this transform.")]
        private Transform _providerHolder = null!;

        [SerializeField] private SphereCollider _groundCheckSphere = null!;
        [SerializeField] private SphereCollider _wallCheckSphere = null!;
        [SerializeField] private SphereCollider _ceilingCheckSphere = null!;

        [SerializeField] private LayerMask _groundLayer;
        [SerializeField] private LayerMask _wallLayer;

        private readonly List<LocomotionProvider> _providers = new();


        private RaycastNonAllocWrapper _groundRaycastWrapper = null!;

        private Vector3 _velocity;

        private Vector2 _velocityXZ;
        private RaycastNonAllocWrapper _wallRaycastWrapper = null!;
        private bool _wasGrounded = true;

        /// <summary>
        ///     A list of all providers found.
        /// </summary>
        public IReadOnlyList<LocomotionProvider> Providers => _providers;

        /// <summary>
        ///     The character controller that is used for movement.
        /// </summary>
        public CharacterController CharacterController => _characterController;

        /// <summary>
        ///     True if the player is currently grounded.
        /// </summary>
        public bool IsGrounded { get; private set; } = true;

        /// <summary>
        ///     True if the player is currently grounded.
        /// </summary>
        public bool IsTouchingWall { get; private set; }

        /// <summary>
        ///     True if the player is currently grounded.
        /// </summary>
        public bool IsTouchingCeiling { get; private set; }

        /// <summary>
        ///     The current velocity of the player.
        /// </summary>
        public Vector3 Velocity => new(_velocityXZ.x, VelocityY, _velocityXZ.y);

        /// <summary>
        ///     The current velocity of the player on the XZ plane.
        /// </summary>
        public Vector2 VelocityXZ => _velocityXZ;

        /// <summary>
        ///     The current velocity of the player on the Y axis.
        /// </summary>
        public float VelocityY { get; private set; }

        /// <summary>
        ///     The current rotation of the player.
        /// </summary>
        public float Rotation { get; private set; }

        /// <summary>
        ///     The current speed of the player (ignoring the Y axis).
        /// </summary>
        public float CurrentSpeed => _velocityXZ.magnitude;

        /// <summary>
        ///     The current movement state of the player.
        /// </summary>
        public MovementState State { get; private set; } = MovementState.Idle;

        protected override void Awake()
        {
            CollectProviders();
            InitializeProviders();
            base.Awake();
        }

        private void Start()
        {
            _groundRaycastWrapper = new RaycastNonAllocWrapper(_groundLayer);
            _wallRaycastWrapper = new RaycastNonAllocWrapper(_wallLayer);
            CustomUpdateManager.Register(this);
        }

        private void OnDestroy()
        {
            CustomUpdateManager.Unregister(this);
        }

        public void CustomFixedUpdate(float fixedDeltaTime)
        {
            ProcessFixedUpdate(fixedDeltaTime);
            ApplyMovement(fixedDeltaTime);
            ProcessAfterMovementUpdate(fixedDeltaTime);
        }

        public void CustomUpdate(float deltaTime)
        {
            HandleCollisions();
            ProcessUpdate(deltaTime);
        }

        public event Action OnProvidersInitialized = delegate { };

        private void CollectProviders()
        {
            foreach (Transform child in _providerHolder)
            {
                if (!child.TryGetComponent(out LocomotionProvider provider))
                {
                    continue;
                }

                _providers.Add(provider);
            }
        }

        /// <summary>
        ///     Returns the provider of the specified type.
        /// </summary>
        public T GetProvider<T>() where T : LocomotionProvider
        {
            foreach (var provider in _providers)
            {
                if (provider is T tProvider)
                {
                    return tProvider;
                }
            }

            throw new ArgumentException("Provider of type " + typeof(T).Name + " not found!");
        }

        public void Teleport(Vector3 location)
        {
            CharacterController.enabled = false;
            CharacterController.transform.position = location;
            CharacterController.enabled = true;
        }

        private void ApplyMovement(float deltaTime)
        {
            _velocity = new Vector3(_velocityXZ.x, VelocityY, _velocityXZ.y);

            var totalVelocity = _velocity;

            if (CharacterController.enabled)
            {
                CharacterController.Move(totalVelocity * deltaTime);

                CharacterController.transform.Rotate(Vector3.up, Rotation * deltaTime);
            }
        }

        private void SetMovementState(MovementState state, bool on)
        {
            if ((State == state && on) || (State != state && !on))
            {
                //we can't activate the same state again, or deactivate a state that is not active.
                return;
            }

            var previousState = State;

            if (!on)
            {
                State = IsGrounded ? MovementState.Idle : MovementState.Airborne;
            }
            else
            {
                State = state;
            }

            foreach (var provider in _providers)
            {
                provider.OnMovementStateChanged(previousState, State);
            }
        }

        private void InitializeProviders()
        {
            foreach (var provider in _providers)
            {
                provider.Initialize(this, SetVelocityXZ, SetVelocityY, SetRotation, SetMovementState);
            }

            OnProvidersInitialized.Invoke();
        }

        private void SetVelocityXZ(Vector2 velocity)
        {
            _velocityXZ = velocity;
        }

        private void SetVelocityY(float velocity)
        {
            VelocityY = velocity;
        }

        private void SetRotation(float rotation)
        {
            Rotation = rotation;
        }

        private void HandleCollisions()
        {
            _wasGrounded = IsGrounded;
            IsGrounded = _groundRaycastWrapper.OverlapSphere(_groundCheckSphere.transform.position, _groundCheckSphere.radius, out _);
            IsTouchingWall = _wallRaycastWrapper.OverlapSphere(_wallCheckSphere.transform.position, _wallCheckSphere.radius, out _);
            IsTouchingCeiling = _groundRaycastWrapper.OverlapSphere(_ceilingCheckSphere.transform.position, _ceilingCheckSphere.radius, out _);

            if (IsTouchingCeiling)
            {
                VelocityY = MathF.Min(VelocityY, 0);
            }

            bool groundedChanged = _wasGrounded != IsGrounded;

            if (groundedChanged && State is MovementState.Idle or MovementState.Airborne)
            {
                SetMovementState(IsGrounded ? MovementState.Idle : MovementState.Airborne, true);
            }

            if (groundedChanged && IsGrounded)
            {
                // This means the player landed.
            }
        }

        private void ProcessUpdate(float deltaTime)
        {
            foreach (var provider in _providers)
            {
                provider.OnUpdate(deltaTime);
            }
        }

        private void ProcessFixedUpdate(float fixedDeltaTime)
        {
            foreach (var provider in _providers)
            {
                provider.OnFixedUpdate(fixedDeltaTime);
            }
        }

        private void ProcessAfterMovementUpdate(float fixedDeltaTime)
        {
            foreach (var provider in _providers)
            {
                provider.OnAfterMovementUpdate(fixedDeltaTime);
            }
        }
    }
}