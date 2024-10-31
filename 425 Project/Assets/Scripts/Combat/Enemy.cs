using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Combat
{
    public class Enemy : MonoBehaviour, IDamageable
    {
        [field: SerializeField]
        public float Health { get; private set; } = 10;
        [field: SerializeField]
        public float MaxHealth { get; private set; } = 10;

        [SerializeField] 
        private DamageNumber _damageNumber;

        private NavMeshAgent agent;
        private bool isCrossingLink = false;

        private void Start()
        {
            agent = transform.GetComponent<NavMeshAgent>();
            Health = MaxHealth;
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
            
            Health -= damage;
            
            if (Health <= 0)
            {
                Death();
                return false;
            }

            return true;
        }

        public void Death()
        {
            Destroy(gameObject);
        }
    }
}