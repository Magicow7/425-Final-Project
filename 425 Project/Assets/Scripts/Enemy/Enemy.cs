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
        private ResourceStat _playerhealth;
        [SerializeField] 
        private DamageNumber _damageNumber;

        private bool inRangeOfPlayer = false;
        
        [SerializeField] 
        private Animator _enemyAnimatior;

        private NavMeshAgent agent;
        private AudioSource damageAudioSource;
        private AudioSource ambientAudioSource;
        private Collider collider;
        private bool isCrossingLink = false;
        
        public ResourceStat Health { get; private set; }

        public static event Action EnemyDeath;

        private void Awake()
        {
            if (PlayerStats.Instance == null)
            {
                Debug.LogError("Player Stats Not Initialized?");
            }
            _playerhealth = PlayerStats.Instance.Health;
            Health = new ResourceStat(_health);
            agent = GetComponent<NavMeshAgent>();
            collider = GetComponent<Collider>();
            _damageNumber.Initialize(Health);
            damageAudioSource = gameObject.AddComponent<AudioSource>();
            ambientAudioSource = gameObject.AddComponent<AudioSource>();
            damageAudioSource.spatialBlend = 1.0f;
            ambientAudioSource.spatialBlend = 1.0f;
            damageAudioSource.rolloffMode = AudioRolloffMode.Custom;
            ambientAudioSource.rolloffMode = AudioRolloffMode.Custom;
            agent = transform.GetComponent<NavMeshAgent>();
            StartCoroutine(AmbientSound());
        }

        private void Update(){
            // Check if the agent is on NavMeshLink
            if (agent.isOnOffMeshLink && !isCrossingLink)
            {
                // Start traversing the NavMeshLink
                StartCoroutine(CrossLink());
            }
        }
        private IEnumerator AmbientSound()
        {
            while (true)
            {
                if (!damageAudioSource.isPlaying)
                {
                    if (UnityEngine.Random.value < 0.04f)
                    {
                        SoundManager.Sound[] items = { SoundManager.Sound.MobNoise1, SoundManager.Sound.MobNoise2, SoundManager.Sound.MobNoise3 };
                        SoundManager.PlaySound(items[UnityEngine.Random.Range(0, items.Length)], ambientAudioSource, true);
                    }
                }
                yield return new WaitForSeconds(1);
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
            SoundManager.PlaySound(SoundManager.Sound.MobHit, damageAudioSource, true);

            if (Health.Value <= 0)
            {
                Death();
                return false;
            }

            return true;
        }

        public void Death()
        {
            // Used for killCounter UI + enemySpawner wave spawn time calculation
            EnemyDeath?.Invoke();
            
            SoundManager.PlaySound(SoundManager.Sound.MobDeath, agent.transform.position, true);
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

        void OnTriggerEnter(Collider other)
        {
            if (other.name == "PlayerModel")
            {
                inRangeOfPlayer = true;
                Debug.LogWarning("ENTERED!");
                agent.isStopped = true;
                
                StartCoroutine(AttackWhileNearby(other));
                
            }
        }

        private IEnumerator AttackWhileNearby(Collider player)
        {
            while ((player.transform.position - transform.position).magnitude <= 3)
            {
                // check if enemy is not moving much
                if (agent.velocity.magnitude < 0.1f && inRangeOfPlayer) {
                    _enemyAnimatior.SetTrigger("TrAttack");
                    _playerhealth.TrySpendResource(20);
                }
                yield return new WaitForSeconds(1f); 
            }

            agent.isStopped = false;
        }

        void OnTriggerExit(Collider other)
        {
            if (other.name == "PlayerModel")
            {
                inRangeOfPlayer = false;

                agent.isStopped = false;
            }
        }
    }
}