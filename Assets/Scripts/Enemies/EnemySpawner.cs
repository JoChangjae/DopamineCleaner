using System.Collections.Generic;
using UnityEngine;
using DopamineCleaner.Core;
using DopamineCleaner.Characters;

namespace DopamineCleaner.Enemies
{
    public class EnemySpawner : MonoBehaviour
    {
        public static EnemySpawner Instance { get; private set; }

        [Header("몬스터 프리팹 & 풀링")]
        [SerializeField] private GameObject enemyPrefab;
        [SerializeField] private int poolSize = 100;
        [SerializeField] private int maxActiveEnemies = 90; // 렉 방지 Mob Density Cap

        [Header("스폰 타이머")]
        [SerializeField] private float baseSpawnInterval = 0.5f;
        private float spawnTimer = 0f;
        private int lastWaveTriggerSec = 0;

        private readonly List<Enemy> enemyPool = new();

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);

            InitializePool();
        }

        private void Start()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnBossSpawned += SpawnBoss;
            }
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnBossSpawned -= SpawnBoss;
            }
        }

        private void InitializePool()
        {
            if (enemyPrefab == null) return;

            for (int i = 0; i < poolSize; i++)
            {
                GameObject obj = Instantiate(enemyPrefab, transform);
                obj.SetActive(false);
                enemyPool.Add(obj.GetComponent<Enemy>());
            }
        }

        private void Update()
        {
            if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing)
                return;

            float survivalSec = GameManager.Instance.SurvivalTime;
            int currentStage = GameManager.Instance.CurrentStage;

            // 1. 일반 스폰 주기
            spawnTimer += Time.deltaTime;
            float interval = Mathf.Max(0.28f, baseSpawnInterval - survivalSec * 0.002f);

            if (spawnTimer >= interval)
            {
                spawnTimer = 0f;
                int batch = survivalSec < 30f ? Random.Range(2, 4) : Random.Range(3, 7);

                for (int b = 0; b < batch; b++)
                {
                    EnemyType type = DetermineEnemyType(survivalSec);
                    SpawnSingleEnemy(type, currentStage, survivalSec);
                }
            }

            // 2. 15초 주기 대규모 웨이브 (Rush Wave)
            int sec = Mathf.FloorToInt(survivalSec);
            if (sec > 0 && sec % 15 == 0 && sec != lastWaveTriggerSec)
            {
                lastWaveTriggerSec = sec;
                TriggerWaveRush(sec, currentStage, survivalSec);
            }
        }

        private EnemyType DetermineEnemyType(float survivalSec)
        {
            if (survivalSec < 20f) return EnemyType.MindDust;
            if (survivalSec < 45f)
            {
                float r = Random.value;
                if (r < 0.6f) return EnemyType.MindDust;
                if (r < 0.85f) return EnemyType.HeavySloth;
                return EnemyType.ShortsRunner;
            }

            float roll = Random.value;
            if (roll < 0.30f) return EnemyType.MindDust;
            if (roll < 0.55f) return EnemyType.HeavySloth;
            if (roll < 0.75f) return EnemyType.NotificationGhost;
            if (roll < 0.90f) return EnemyType.ShortsRunner;
            return EnemyType.SpamShooter;
        }

        private void TriggerWaveRush(int sec, int stage, float survivalSec)
        {
            int waveCount = Mathf.Min(20, 10 + stage * 3);
            EnemyType waveType = (sec % 30 == 0) ? EnemyType.HeavySloth : EnemyType.NotificationGhost;

            for (int i = 0; i < waveCount; i++)
            {
                SpawnSingleEnemy(waveType, stage, survivalSec);
            }
        }

        private void SpawnSingleEnemy(EnemyType type, int stage, float survivalSec)
        {
            int activeCount = GetActiveEnemyCount();

            // 몬스터 밀도 상한선: 90마리 도달 시 신규 스폰 대신 기존 몬스터 강화
            if (activeCount >= maxActiveEnemies && type != EnemyType.DopamineBoss)
            {
                return;
            }

            Enemy enemy = GetAvailableEnemy();
            if (enemy != null && PlayerController.Instance != null)
            {
                Vector3 spawnPos = GetPerimeterSpawnPosition();
                enemy.transform.position = spawnPos;
                enemy.Initialize(type, stage, survivalSec);
            }
        }

        public void SpawnSpecialWave(int count)
        {
            if (GameManager.Instance == null) return;
            int stage = GameManager.Instance.CurrentStage;
            float time = GameManager.Instance.SurvivalTime;
            for (int i = 0; i < count; i++)
            {
                SpawnSingleEnemy(EnemyType.SpamShooter, stage, time);
            }
        }

        private void SpawnBoss()
        {
            if (GameManager.Instance == null) return;

            Enemy boss = GetAvailableEnemy();
            if (boss != null && PlayerController.Instance != null)
            {
                Vector3 spawnPos = GetPerimeterSpawnPosition();
                boss.transform.position = spawnPos;
                boss.Initialize(EnemyType.DopamineBoss, GameManager.Instance.CurrentStage, GameManager.Instance.SurvivalTime);
            }
        }

        private Vector3 GetPerimeterSpawnPosition()
        {
            Vector3 center = PlayerController.Instance.transform.position;
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float distance = Random.Range(13f, 16f); // 카메라 바깥 원형 둘레

            return center + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * distance;
        }

        private int GetActiveEnemyCount()
        {
            int count = 0;
            for (int i = 0; i < enemyPool.Count; i++)
            {
                if (enemyPool[i].gameObject.activeSelf && enemyPool[i].IsAlive)
                    count++;
            }
            return count;
        }

        private Enemy GetAvailableEnemy()
        {
            for (int i = 0; i < enemyPool.Count; i++)
            {
                if (!enemyPool[i].gameObject.activeSelf)
                    return enemyPool[i];
            }

            if (enemyPrefab != null)
            {
                GameObject obj = Instantiate(enemyPrefab, transform);
                obj.SetActive(false);
                Enemy e = obj.GetComponent<Enemy>();
                enemyPool.Add(e);
                return e;
            }

            return null;
        }
    }
}
