using System.Collections.Generic;
using UnityEngine;
using DopamineCleaner.Enemies;

namespace DopamineCleaner.Weapons
{
    public class LaserBeamWeapon : WeaponBase
    {
        [Header("레이저 설정")]
        [SerializeField] private float damage = 25f;
        [SerializeField] private float beamRange = 12f;
        [SerializeField] private float beamWidth = 0.35f;
        [SerializeField] private LayerMask enemyLayer;

        private LineRenderer lineRenderer;
        private float beamDisplayTimer = 0f;

        private void Awake()
        {
            weaponName = "디톡스 레이저";
            baseCooldown = 1.4f;
            maxLevel = 5;

            // 라인 렌더러 생성
            lineRenderer = gameObject.AddComponent<LineRenderer>();
            lineRenderer.startWidth = beamWidth;
            lineRenderer.endWidth = beamWidth;
            lineRenderer.positionCount = 2;
            lineRenderer.useWorldSpace = true;
            lineRenderer.enabled = false;

            // 핑크/시안 네온 머티리얼 적용
            Shader shader = Shader.Find("Sprites/Default");
            if (shader != null)
            {
                lineRenderer.material = new Material(shader);
            }
            lineRenderer.startColor = new Color(0.2f, 0.9f, 1f, 0.95f);
            lineRenderer.endColor = new Color(1f, 0.3f, 0.8f, 0.95f);
        }

        protected override void Update()
        {
            base.Update();

            if (beamDisplayTimer > 0f)
            {
                beamDisplayTimer -= Time.deltaTime;
                if (beamDisplayTimer <= 0f && lineRenderer != null)
                {
                    lineRenderer.enabled = false;
                }
            }
        }

        public override void Upgrade()
        {
            currentLevel++;
            damage += 12f;
            baseCooldown = Mathf.Max(0.5f, baseCooldown - 0.15f);
            beamRange += 1.5f;
            beamWidth += 0.08f;
            if (lineRenderer != null)
            {
                lineRenderer.startWidth = beamWidth;
                lineRenderer.endWidth = beamWidth;
            }
        }

        public override void Evolve()
        {
            isEvolved = true;
            weaponName = "프리즘 정화포";
            damage *= 2.2f;
            baseCooldown = 0.55f;
            beamRange = 18f;
            beamWidth = 0.65f;
            if (lineRenderer != null)
            {
                lineRenderer.startWidth = beamWidth;
                lineRenderer.endWidth = beamWidth;
                lineRenderer.startColor = new Color(1f, 0.9f, 0.2f, 1f);
                lineRenderer.endColor = new Color(1f, 0.2f, 0.4f, 1f);
            }
        }

        protected override void Attack()
        {
            Vector2 origin = transform.position;

            if (isEvolved)
            {
                // 프리즘 정화포: 십자 4방향 동시 관통 레이저
                Vector2[] directions = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };
                foreach (var dir in directions)
                {
                    FireRay(origin, dir, beamRange);
                }
            }
            else
            {
                // 가장 가까운 적 타겟팅
                Enemy target = FindClosestEnemy();
                Vector2 dir = target != null ? ((Vector2)target.transform.position - origin).normalized : Vector2.right;
                FireRay(origin, dir, beamRange);
            }
        }

        private void FireRay(Vector2 origin, Vector2 dir, float range)
        {
            Vector2 end = origin + dir * range;

            // 광선 렌더링
            if (lineRenderer != null)
            {
                lineRenderer.SetPosition(0, origin);
                lineRenderer.SetPosition(1, end);
                lineRenderer.enabled = true;
                beamDisplayTimer = 0.14f;
            }

            // 관통 대미지 판정
            RaycastHit2D[] hits = Physics2D.CircleCastAll(origin, beamWidth * 1.5f, dir, range);
            HashSet<Enemy> hitEnemies = new();

            foreach (var hit in hits)
            {
                if (hit.collider != null && hit.collider.TryGetComponent<Enemy>(out var enemy))
                {
                    if (hitEnemies.Add(enemy))
                    {
                        enemy.TakeDamage(damage);
                        RegisterDamage(damage);
                    }
                }
            }
        }

        private Enemy FindClosestEnemy()
        {
            Enemy[] allEnemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
            Enemy closest = null;
            float minDist = beamRange;

            foreach (var e in allEnemies)
            {
                if (e == null || !e.gameObject.activeInHierarchy) continue;
                float d = Vector2.Distance(transform.position, e.transform.position);
                if (d < minDist)
                {
                    minDist = d;
                    closest = e;
                }
            }
            return closest;
        }
    }
}
