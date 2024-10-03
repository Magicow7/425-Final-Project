using UnityEngine;

namespace Locomotion
{
    public class GravityProvider : LocomotionProvider
    {
        [SerializeField, Tooltip("How fast gravity will pull you down.")]
        private float _gravityAcceleration = 9.8f;

        private bool _enabled = false;
        
        protected override void OnInitialize()
        {
            _enabled = true;
        }

        public override void OnFixedUpdate(float deltaTime)
        {
            if (!_enabled || locomotionManager.IsGrounded)
            {
                return;
            }

            if (locomotionManager.IsGrounded)
            {
                SetVelocityY(0);
            }
            else
            {
                AddVelocityY(-_gravityAcceleration * deltaTime);
            }
        }
    }
}