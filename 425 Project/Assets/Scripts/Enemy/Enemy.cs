using System;
using System.Collections;
using System.Collections.Generic;
using Stat;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
namespace Combat
{
    public class Enemy : MonoBehaviour, IDamageable
    {
        private ResourceStat _playerhealth;
        [SerializeField]
        private DamageNumber _damageNumber;

        private bool _inRangeOfPlayer = false;

        [SerializeField]
        private Animator _enemyAnimatior;

        private NavMeshAgent _agent;
        private AudioSource _damageAudioSource;
        private AudioSource _ambientAudioSource;
        private Collider _collider;
        private bool _isCrossingLink = false;
        private bool _isDead = false;

        public EnemyStats EnemyStats { get; private set; }

        public static event Action EnemyDeath;

        private void Awake()
        {
            if (PlayerStats.Instance == null)
            {
                Debug.LogError("Player Stats Not Initialized?");
            }

            EnemyStats = new EnemyStats();
            transform.localScale = transform.localScale * EnemyStats.Scale;

            _playerhealth = PlayerStats.Instance.Health;
            _agent = GetComponent<NavMeshAgent>();
            _collider = GetComponent<Collider>();
            _damageNumber.Initialize(EnemyStats.Health);
            _damageAudioSource = gameObject.AddComponent<AudioSource>();
            _ambientAudioSource = gameObject.AddComponent<AudioSource>();
            _damageAudioSource.spatialBlend = 1.0f;
            _ambientAudioSource.spatialBlend = 1.0f;
            _damageAudioSource.rolloffMode = AudioRolloffMode.Custom;
            _ambientAudioSource.rolloffMode = AudioRolloffMode.Custom;
            _agent = transform.GetComponent<NavMeshAgent>();
            _agent.speed = EnemyStats.Speed.Value;
            StartCoroutine(AmbientSound());
        }

        private void Update()
        {
            // Check if the agent is on NavMeshLink
            if (_agent.isOnOffMeshLink && !_isCrossingLink)
            {
                // Start traversing the NavMeshLink
                StartCoroutine(CrossLink());
            }
        }
        private IEnumerator AmbientSound()
        {
            while (true)
            {
                if (!_damageAudioSource.isPlaying)
                {
                    if (UnityEngine.Random.value < 0.04f)
                    {
                        SoundManager.Sound[] items = { SoundManager.Sound.MobNoise1, SoundManager.Sound.MobNoise2, SoundManager.Sound.MobNoise3 };
                        SoundManager.PlaySound(items[UnityEngine.Random.Range(0, items.Length)], _ambientAudioSource, true);
                    }
                }
                yield return new WaitForSeconds(1);
            }
        }

        private IEnumerator CrossLink()
        {
            _isCrossingLink = true;

            // Get the link data (start and end points)
            OffMeshLinkData linkData = _agent.currentOffMeshLinkData;
            Vector3 startPos = _agent.transform.position;
            Vector3 endPos = linkData.endPos + new Vector3(0, _agent.transform.position.y, 0);

            // Get the total distance to travel across the link
            float distance = Vector3.Distance(startPos, endPos);
            float travelTime = distance / _agent.speed;  // Maintain the agent's speed

            float elapsedTime = 0f;

            // Move the agent manually across the link
            while (elapsedTime < travelTime)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / travelTime;

                // Linear interpolation from start to end position
                _agent.transform.position = Vector3.Lerp(startPos, endPos, t);
                yield return null;
            }

            // After crossing the link, complete the off-mesh link
            _agent.CompleteOffMeshLink();
            _isCrossingLink = false;
        }


        public bool TakeDamage(float damage)
        {
            _damageNumber.ShowDamageNumber(damage);

            SoundManager.PlaySound(SoundManager.Sound.MobHit, _damageAudioSource, true);

            if (!EnemyStats.Health.TrySpendResource(damage))
            {
                Death();
                return false;
            }

            return true;
        }

        public void Death()
        {
            _isDead = true;
            // Used for killCounter UI + enemySpawner wave spawn time calculation
            EnemyDeath?.Invoke();

            SoundManager.PlaySound(SoundManager.Sound.MobDeath, _agent.transform.position, true);
            // TODO: Death Animation
            if (_agent.isOnNavMesh)
            {
                _agent.isStopped = true;
                _collider.enabled = false;
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
            if (!_isDead && other.name == "PlayerModel" && !_isCrossingLink && !_agent.isOnOffMeshLink)
            {
                _inRangeOfPlayer = true;
                //agent.isStopped = true;

                StartCoroutine(AttackWhileNearby(other));

            }
        }

        private IEnumerator AttackWhileNearby(Collider player)
        {
            while (!_isDead && (player.transform.position - transform.position).magnitude <= 3)
            {
                // check if enemy is not moving much
                if (_agent.velocity.magnitude < 0.1f && _inRangeOfPlayer)
                {
                    _enemyAnimatior.SetTrigger("TrAttack");

                    bool isAlive = _playerhealth.TrySpendResource(EnemyStats.AttackDamage.Value);
                    if (!isAlive)
                    {
                        PlayerStats.Instance.Health.Value = 0;

                        TextUpdates.Instance.ShowDeathScreen();
                        // Player HP too low, cannot take another hit.
                        SoundManager.PlaySound(SoundManager.Sound.PlayerDeath);
                    }
                    else
                    {
                        SoundManager.PlaySound(SoundManager.Sound.PlayerHit);

                    }

                }
                yield return new WaitForSeconds(1f);
            }

            //agent.isStopped = false;
        }

        void OnTriggerExit(Collider other)
        {
            if (other.name == "PlayerModel")
            {
                _inRangeOfPlayer = false;

                _agent.isStopped = false;
            }
        }

        public void ConfigureStats(float health, float speed, float scale, float attackDamage)
        {
            EnemyStats = new EnemyStats(health, speed, scale, attackDamage);

            transform.localScale = transform.localScale * EnemyStats.Scale;
            _damageNumber.Initialize(EnemyStats.Health);
            _agent.speed = EnemyStats.Speed.Value;
        }
    }
}