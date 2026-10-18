using System.Collections.Generic;
using UnityEngine;
using DopamineCleaner.Enemies;
using DopamineCleaner.Meta;

namespace DopamineCleaner.Weapons
{
    public class CleanBulletWeapon : WeaponBase
    {
        [Header("투사체 프리팹 & 풀링")]
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private int poolSize = 30;
        private readonly List<Projectile> projectilePool = new();

        private void Awake()
        {
            weaponName = "클린 탄환 (Clean Bullet)";
            baseCooldown = 0.55f;

            if (projectilePrefab != null)
            {
                for (int i = 0; i < poolSize; i++)
                {
                    GameObject obj = Instantiate(projectilePrefab, transform);
                    obj.SetActive(false);
                    projectilePool.Add(obj.GetComponent<Projectile>());
                }
            }
        }

        public override void Upgrade()
        {
            if (currentLevel < maxLevel)
            {
                currentLevel++;
                baseCooldown = Mathf.Max(0.25f, 0.55f - (currentLevel - 1) * 0.06f);
            }
        }

        public override void Evolve()
        {
            isEvolved = true;
            weaponName = "✨ 기가 개틀링 (Giga Gatling)";
            baseCooldown = 0.22f; // 초고속 연사
        }

        protected override void Attack()
        {
            Enemy target = FindClosestEnemy();
            Vector2 targetDir = Vector2.right;

            if (target != null)
            {
                targetDir = (target.transform.position - transform.position).normalized;
            }

            float damageMult = 1f;
            if (Meta.GameDataManager.Instance != null)
            {
                damageMult = Meta.CharacterDatabase.Get(Meta.GameDataManager.Instance.Profile.selectedCharacterId).baseDamageMult;
            }

            if (isEvolved)
            {
                // 진화 개틀링: 8발 전방위 고속 산탄
                int bulletCount = 8;
                for (int i = 0; i < bulletCount; i++)
                {
                    float angle = Mathf.Atan2(targetDir.y, targetDir.x) + (i - 3.5f) * 0.22f;
                    Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                    FireBullet(dir, 14f, 65f * damageMult, 25, true);
                }
            }
            else
            {
                // 일반 클린 탄환: 레벨당 탄환 수 증가 (1~5발)
                int bulletCount = currentLevel + 1;
                for (int i = 0; i < bulletCount; i++)
                {
                    float angle = Mathf.Atan2(targetDir.y, targetDir.x) + (i - (bulletCount - 1) * 0.5f) * 0.18f;
                    Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                    FireBullet(dir, 10f, (20f + currentLevel * 5f) * damageMult, 1 + currentLevel, false);
                }
            }
        }

        private void FireBullet(Vector2 dir, float speed, float damage, int pierce, bool evolved)
        {
            Projectile p = GetAvailableProjectile();
            if (p != null)
            {
                p.transform.position = transform.position;
                p.Initialize(dir, speed, damage, pierce, evolved);
            }
        }

        private Projectile GetAvailableProjectile()
        {
            for (int i = 0; i < projectilePool.Count; i++)
            {
                if (!projectilePool[i].gameObject.activeSelf)
                    return projectilePool[i];
            }

            if (projectilePrefab != null)
            {
                GameObject obj = Instantiate(projectilePrefab, transform);
                obj.SetActive(false);
                Projectile p = obj.GetComponent<Projectile>();
                projectilePool.Add(p);
                return p;
            }

            return null;
        }

        private Enemy FindClosestEnemy()
        {
            Enemy[] enemies = FindObjectsByType<Enemy>();
            Enemy closest = null;
            float minDistSq = 144f; // 최대 12 유닛 내 탐색

            for (int i = 0; i < enemies.Length; i++)
            {
                if (enemies[i].gameObject.activeSelf && enemies[i].IsAlive)
                {
                    float dSq = (enemies[i].transform.position - transform.position).sqrMagnitude;
                    if (dSq < minDistSq)
                    {
                        minDistSq = dSq;
                        closest = enemies[i];
                    }
                }
            }

            return closest;
        }
    }
}
