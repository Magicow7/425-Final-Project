using System;
using UnityEngine;

namespace Locomotion
{
    public abstract class LocomotionProvider : MonoBehaviour
    {
        private Action<Vector2> _setVelocityXZ = null!;
        private Action<float> _setVelocityY = null!;
        private Action<float> _setRotation = null!;
        private Action<MovementState, bool> _setMovementState = null!;
        
        protected LocomotionManager locomotionManager = null!;
        
        public void Initialize(LocomotionManager locomotionManager, Action<Vector2> setVelocityXZ, Action<float> setVelocityY, Action<float> setRotation, Action<MovementState, bool> setMovementState)
        {
            _setVelocityXZ = setVelocityXZ;
            _setVelocityY = setVelocityY;
            _setRotation = setRotation;
            _setMovementState = setMovementState;
            
            this.locomotionManager = locomotionManager;

            OnInitialize();
        }

        protected abstract void OnInitialize();
        
        protected void SetVelocityXZ(Vector2 velocity) => _setVelocityXZ(velocity);
        protected void SetVelocityY(float velocity) => _setVelocityY(velocity);
        protected void SetVelocityXZ(Vector3 velocity) => _setVelocityXZ(new Vector2(velocity.x, velocity.z));
        protected void SetRotation(float rotation) => _setRotation(rotation);
        protected void SetVelocityY(Vector3 velocity) => _setVelocityY(velocity.y);
        protected void SetVelocity(Vector3 velocity)
        {
            _setVelocityXZ(new Vector2(velocity.x, velocity.z));
            _setVelocityY(velocity.y);
        }
        
        protected void AddVelocityXZ(Vector2 velocity) => SetVelocityXZ(locomotionManager.VelocityXZ + velocity);
        protected void AddVelocityXZ(Vector3 velocity) => SetVelocityXZ(locomotionManager.VelocityXZ + new Vector2(velocity.x, velocity.z));
        protected void AddVelocityY(float velocity) => SetVelocityY(locomotionManager.VelocityY + velocity);
        protected void AddVelocityY(Vector3 velocity) => SetVelocityY(locomotionManager.VelocityY + velocity.y);
        protected void AddVelocity(Vector3 velocity)
        {
            AddVelocityXZ(velocity);
            AddVelocityY(velocity.y);
        }
        protected void AddRotation(float rotation) => SetRotation(locomotionManager.Rotation + rotation);
        
        protected void SetMovementState(MovementState state, bool on) => _setMovementState(state, on);
        
        public virtual void OnMovementStateChanged(MovementState previousState, MovementState newState) { }

        public virtual void OnUpdate(float deltaTime) { }
        public virtual void OnFixedUpdate(float fixedDeltaTime) { }
        public virtual void OnAfterMovementUpdate(float fixedDeltaTime) { }
    }
}