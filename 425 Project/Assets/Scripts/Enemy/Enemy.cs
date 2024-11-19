using System;
using System.Collections;
using System.Collections.Generic;
using Stat;
using UnityEngine;
using UnityEngine.AI;

namespace Combat
{
    public class Enemy : MonoBehaviour, IDamageable
    {
        [SerializeField] 
        private float _health = 10f;

        [SerializeField] 
        private DamageNumber _damageNumber;
        
        [SerializeField] 
        private Animator _enemyAnimatior;

        private NavMeshAgent agent;
        private Collider collider;
        private bool isCrossingLink = false;
        
        public ResourceStat Health { get; private set; }

        private void Awake()
        {
            Health = new ResourceStat(_health);
            agent = GetComponent<NavMeshAgent>();
            collider = GetComponent<Collider>();
            _damageNumber.Initialize(Health);
        }

        private void Update(){
            // Check if the agent is on NavMeshLink
            if (agent.isOnOffMeshLink && !isCrossingLink)
            {
                // Start traversing the NavMeshLink
                StartCoroutine(CrossLink());
            }
        }

        private IEnumerator CrossLink()
        {
            isCrossingLink = true;

            // Get the link data (start and end points)
            OffMeshLinkData linkData = agent.currentOffMeshLinkData;
            Vector3 startPos = agent.transform.position;
            Vector3 endPos = linkData.endPos + new Vector3(0,agent.transform.position.y,0);

            // Get the total distance to travel across the link
            float distance = Vector3.Distance(startPos, endPos);
            float travelTime = distance / agent.speed;  // Maintain the agent's speed

            float elapsedTime = 0f;

            // Move the agent manually across the link
            while (elapsedTime < travelTime)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / travelTime;

                // Linear interpolation from start to end position
                agent.transform.position = Vector3.Lerp(startPos, endPos, t);
                yield return null;
            }

            // After crossing the link, complete the off-mesh link
            agent.CompleteOffMeshLink();
            isCrossingLink = false;
        }


        public bool TakeDamage(float damage)
        {
            _damageNumber.ShowDamageNumber(damage);
            
            Health.Value -= damage;
            
            if (Health.Value <= 0)
            {
                Death();
                return false;
            }

            return true;
        }

        public void Death()
        {
            // TODO: Death Animation
            if (agent.isOnNavMesh)
            {
                agent.isStopped = true;
                collider.enabled = false;
                _enemyAnimatior.gameObject.SetActive(false);
                Destroy(gameObject, 3f);
            }
            else
            {
                // TODO: Death on the mesh link is a pain
                Destroy(gameObject);
            }
        }
    }
}