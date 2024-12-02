using System;
using System.Collections;
using Stat;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace Combat
{
    public class Enemy : MonoBehaviour, IDamageable
    {
        [SerializeField] private DamageNumber _damageNumber;

        [SerializeField] private Animator _enemyAnimatior;

        private NavMeshAgent _agent;
        private AudioSource _ambientAudioSource;
        private Collider _collider;
        private AudioSource _damageAudioSource;

        private bool _inRangeOfPlayer = false;
        private bool _isCrossingLink;
        private bool _isDead;
        private float _lastPlayerTime = -1;
        private ResourceStat _playerhealth;

        public EnemyStats EnemyStats { get; private set; }

        private void Awake()
        {
            if (PlayerStats.Instance == null)
            {
                Debug.LogError("Player Stats Not Initialized?");
            }

            EnemyStats = new EnemyStats();
            transform.localScale *= EnemyStats.Scale;

            if (PlayerStats.Instance != null)
            {
                _playerhealth = PlayerStats.Instance.Health;
            }

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

            if (!Player.Instance)
            {
                return;
            }

            float playerDistance = (Player.Instance.transform.position - transform.position).magnitude;

            if (Player.Instance && playerDistance <= 1)
            {
                _lastPlayerTime = _lastPlayerTime < 0 ? 0 : _lastPlayerTime + Time.deltaTime;
                _enemyAnimatior.SetTrigger("TrAttack");
            }
            else if (Player.Instance && playerDistance <= 2)
            {
                _lastPlayerTime = _lastPlayerTime < 0 ? _lastPlayerTime : _lastPlayerTime + Time.deltaTime;
            }
            else
            {
                _lastPlayerTime = -1;
            }

            if (_lastPlayerTime > EnemyStats.AttackSpeed.Value)
            {
                Player.Instance.TakeDamage(EnemyStats.AttackDamage.Value);
                _lastPlayerTime = -1;
            }
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

        public static event Action EnemyDeath;

        private IEnumerator AmbientSound()
        {
            while (true)
            {
                if (!_damageAudioSource.isPlaying)
                {
                    if (Random.value < 0.04f)
                    {
                        SoundManager.Sound[] items = { SoundManager.Sound.MobNoise1, SoundManager.Sound.MobNoise2, SoundManager.Sound.MobNoise3 };
                        SoundManager.PlaySound(items[Random.Range(0, items.Length)], _ambientAudioSource, true);
                    }
                }

                yield return new WaitForSeconds(1);
            }
        }

        private IEnumerator CrossLink()
        {
            _isCrossingLink = true;

            // Get the link data (start and end points)
            var linkData = _agent.currentOffMeshLinkData;
            var startPos = _agent.transform.position;
            var endPos = linkData.endPos + new Vector3(0, _agent.transform.position.y, 0);

            // Get the total distance to travel across the link
            float distance = Vector3.Distance(startPos, endPos);
            float travelTime = distance / _agent.speed; // Maintain the agent's speed

            var elapsedTime = 0f;

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

        public void ConfigureStats(float health, float speed, float scale, float attackDamage, float attackSpeed)
        {
            EnemyStats = new EnemyStats(health, speed, scale, attackDamage, attackSpeed);

            transform.localScale = transform.localScale * EnemyStats.Scale;
            _damageNumber.Initialize(EnemyStats.Health);
            _agent.speed = EnemyStats.Speed.Value;
        }
    }
}