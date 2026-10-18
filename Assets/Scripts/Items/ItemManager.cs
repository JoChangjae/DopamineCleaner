using System.Collections.Generic;
using UnityEngine;
using DopamineCleaner.Enemies;
using DopamineCleaner.Characters;
using DopamineCleaner.Weapons;

namespace DopamineCleaner.Items
{
    public class ItemManager : MonoBehaviour
    {
        public static ItemManager Instance { get; private set; }

        [Header("보석 프리팹 & 풀링")]
        [SerializeField] private GameObject gemPrefab;
        [SerializeField] private int initialPoolSize = 60;
        private readonly List<ExpGem> gemPool = new();

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);

            InitializePool();
        }

        private void InitializePool()
        {
            if (gemPrefab == null) return;

            for (int i = 0; i < initialPoolSize; i++)
            {
                CreateNewGemInPool();
            }
        }

        private ExpGem CreateNewGemInPool()
        {
            GameObject obj = Instantiate(gemPrefab, transform);
            obj.SetActive(false);
            ExpGem gem = obj.GetComponent<ExpGem>();
            gemPool.Add(gem);
            return gem;
        }

        public void SpawnGem(Vector3 position, GemType type, int exp)
        {
            Vector3 spawnPos = new Vector3(position.x, position.y, 0f);

            // 1. 특수 아이템(폭탄, 상자, 케이크, 자석)은 무조건 즉시 스폰
            if (type is GemType.Bomb or GemType.Chest or GemType.Cake or GemType.Magnet)
            {
                ExpGem item = GetAvailableGem();
                item.transform.position = spawnPos;
                item.Initialize(type, 0);
                return;
            }

            // 2. 일반 경험치 보석: 스마트 압축 병합 (Gem Merging)
            int activeCount = 0;
            ExpGem nearestGem = null;
            float minDistanceSq = 9999f;

            for (int i = 0; i < gemPool.Count; i++)
            {
                if (gemPool[i].gameObject.activeSelf)
                {
                    activeCount++;
                    if (gemPool[i].Type is GemType.Green or GemType.Blue or GemType.Pink or GemType.MegaPink)
                    {
                        float dSq = (gemPool[i].transform.position - spawnPos).sqrMagnitude;
                        if (dSq < minDistanceSq)
                        {
                            minDistanceSq = dSq;
                            nearestGem = gemPool[i];
                        }
                    }
                }
            }

            // 근처 1.8 유닛 내에 기존 보석이 있거나, 활성 보석이 45개 이상이면 병합!
            if (nearestGem != null && (minDistanceSq < 3.24f || activeCount >= 45))
            {
                nearestGem.AddExp(exp);
                return;
            }

            // 신규 보석 활성화
            ExpGem newGem = GetAvailableGem();
            newGem.transform.position = spawnPos;
            newGem.Initialize(type, exp);
        }

        private ExpGem GetAvailableGem()
        {
            for (int i = 0; i < gemPool.Count; i++)
            {
                if (!gemPool[i].gameObject.activeSelf)
                    return gemPool[i];
            }

            return CreateNewGemInPool();
        }

        public void TriggerScreenBomb()
        {
            Enemy[] allEnemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
            for (int i = 0; i < allEnemies.Length; i++)
            {
                if (allEnemies[i].gameObject.activeSelf && allEnemies[i].IsAlive)
                {
                    if (allEnemies[i].Type == EnemyType.DopamineBoss)
                    {
                        allEnemies[i].TakeDamage(350f, Vector2.zero);
                    }
                    else
                    {
                        allEnemies[i].TakeDamage(9999f, Vector2.zero); // 일반 몬스터 일망타진
                    }
                }
            }
        }

        public void AttractAllGems()
        {
            for (int i = 0; i < gemPool.Count; i++)
            {
                if (gemPool[i].gameObject.activeSelf)
                {
                    gemPool[i].ForceAttract();
                }
            }
        }

        public void OpenTreasureChest()
        {
            WeaponManager.Instance?.OpenTreasureChestJackpot();
        }
    }
}
