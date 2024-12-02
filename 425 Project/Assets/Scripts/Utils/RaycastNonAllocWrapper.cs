using UnityEngine;

namespace Utils
{
    /// <summary>
    /// A utility class that wraps *NonAlloc physics calls to make them a little more convenient to use.
    /// They always return true if at least one hit was found and provide the hits as out parameters.
    /// </summary>
    public class RaycastNonAllocWrapper
    {
        private readonly RaycastHit[] _hits;
        private readonly Collider[] _colliders;
        private readonly LayerMask _layerMask;
        private readonly QueryTriggerInteraction _triggerInteraction;
        
        public RaycastNonAllocWrapper(LayerMask layerMask, int maxHits = 1, QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore)
        {
            _hits = new RaycastHit[maxHits];
            _colliders = new Collider[maxHits];
            _layerMask = layerMask;
            _triggerInteraction = triggerInteraction;
        }
        
        public bool Raycast(Vector3 origin, Vector3 direction, float distance, out RaycastHit hit)
        {
            var hitCount = Physics.RaycastNonAlloc(origin, direction, _hits, distance, _layerMask, _triggerInteraction);
            hit = _hits[0];
            return hitCount > 0;
        }
        
        public bool RaycastAll(Vector3 origin, Vector3 direction, float distance, out RaycastHit[] hits, out int numHits)
        {
            numHits = Physics.RaycastNonAlloc(origin, direction, _hits, distance, _layerMask, _triggerInteraction);
            hits = _hits;
            return numHits > 0;
        }

        public bool OverlapSphere(Vector3 origin, float radius, out Collider[] colliders)
        {
            var hitCount = Physics.OverlapSphereNonAlloc(origin, radius, _colliders, _layerMask, _triggerInteraction);
            colliders = _colliders;
            return hitCount > 0;
        }
        
        public bool SphereCast(Vector3 origin, float radius, Vector3 direction, float distance, out RaycastHit hit)
        {
            var hitCount = Physics.SphereCastNonAlloc(origin, radius, direction, _hits, distance, _layerMask, _triggerInteraction);
            hit = _hits[0];
            return hitCount > 0;
        }

        public bool SphereCastAll(Vector3 origin, float radius, Vector3 direction, float distance, out RaycastHit[] hits, out int numHits)
        {
            numHits = Physics.SphereCastNonAlloc(origin, radius, direction, _hits, distance, _layerMask, _triggerInteraction);
            hits = _hits;
            return numHits > 0;
        }
        
        public bool OverlapCapsule(Vector3 point0, Vector3 point1, float radius, out Collider[] colliders)
        {
            var hitCount = Physics.OverlapCapsuleNonAlloc(point0, point1, radius, _colliders, _layerMask, _triggerInteraction);
            colliders = _colliders;
            return hitCount > 0;
        }
    }
}