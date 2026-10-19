using System.Collections.Generic;
using UnityEngine;
using DopamineCleaner.Enemies;
using DopamineCleaner.Core;

namespace DopamineCleaner.Weapons
{
    public class DetoxMissile : MonoBehaviour
    {
        private float damage;
        private float explosionRadius;
        private bool isEvolved;
        private DetoxMissileWeapon ownerWeapon;
        private Transform target;

        private Vector2 currentVelocity;
        private float speed = 9.5f;
        private float turnSpeed = 480f;
        private float lifeTimer = 4.0f;
        private bool isInitialized = false;

        public void Setup(float dmg, float radius, bool evolved, Transform targetEnemy, DetoxMissileWeapon owner)
        {
            damage = dmg;
            explosionRadius = radius;
            isEvolved = evolved;
            target = targetEnemy;
            ownerWeapon = owner;
            isInitialized = true;

            // 초기 발사 속도: 위쪽/후방으로 솟구침
            float randomX = Random.Range(-4.5f, 4.5f);
            float randomY = Random.Range(6.0f, 9.5f);
            currentVelocity = new Vector2(randomX, randomY);

            // 미사일 외형 스프라이트 생성
            var sr = GetComponent<SpriteRenderer>();
            if (sr == null) sr = gameObject.AddComponent<SpriteRenderer>();
            sr.sprite = CreateMissileSprite(isEvolved);
            transform.localScale = isEvolved ? Vector3.one * 0.9f : Vector3.one * 0.6f;

            var col = GetComponent<CircleCollider2D>();
            if (col == null) col = gameObject.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.35f;

            SoundManager.Instance?.Play(SoundEffect.Missile, 0.4f);
        }

        private void Update()
        {
            if (!isInitialized) return;

            lifeTimer -= Time.deltaTime;
            if (lifeTimer <= 0f)
            {
                Explode();
                return;
            }

            // 타겟 유효성 검사 (타겟 사망 시 주변 가장 가까운 적으로 재타겟팅)
            if (target == null || !target.gameObject.activeInHierarchy)
            {
                FindNewTarget();
            }

            // 유도 비행 물리
            if (target != null)
            {
                Vector2 toTarget = ((Vector2)target.position - (Vector2)transform.position).normalized;
                currentVelocity = Vector2.MoveTowards(currentVelocity, toTarget * speed, turnSpeed * Time.deltaTime);
            }

            transform.position += (Vector3)(currentVelocity * Time.deltaTime);

            // 미사일 머리 방향 회전
            if (currentVelocity.sqrMagnitude > 0.01f)
            {
                float angle = Mathf.Atan2(currentVelocity.y, currentVelocity.x) * Mathf.Rad2Deg - 90f;
                transform.rotation = Quaternion.Euler(0, 0, angle);
            }
        }

        private void FindNewTarget()
        {
            Enemy[] enemies = FindObjectsByType<Enemy>();
            float minDist = 18f;
            Transform closest = null;

            foreach (var e in enemies)
            {
                if (e == null || !e.gameObject.activeInHierarchy) continue;
                float d = Vector2.Distance(transform.position, e.transform.position);
                if (d < minDist)
                {
                    minDist = d;
                    closest = e.transform;
                }
            }

            target = closest;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Enemy"))
            {
                Explode();
            }
        }

        private void Explode()
        {
            isInitialized = false;

            // 폭발 범위 피해
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
            foreach (var hit in hits)
            {
                if (hit.CompareTag("Enemy") && hit.TryGetComponent<Enemy>(out var enemy))
                {
                    float finalDmg = ownerWeapon != null ? ownerWeapon.CalculateDamage(damage, out bool isCrit) : damage;
                    Vector2 knockbackDir = ((Vector2)enemy.transform.position - (Vector2)transform.position).normalized;
                    enemy.TakeDamage(finalDmg, knockbackDir, isEvolved ? 1.6f : 0.9f);
                    ownerWeapon?.RegisterDamage(finalDmg);
                }
            }

            // 폭발 이펙트 및 사운드
            SpawnExplosionFX(transform.position, explosionRadius, isEvolved);
            SoundManager.Instance?.Play(SoundEffect.Explosion, 0.5f);

            Destroy(gameObject);
        }

        private static Sprite CreateMissileSprite(bool evolved)
        {
            int w = 16, h = 32;
            Texture2D tex = new Texture2D(w, h);
            tex.filterMode = FilterMode.Point;
            Color bodyCol = evolved ? new Color(1f, 0.85f, 0.2f) : new Color(0.2f, 0.85f, 1f);
            Color tipCol = evolved ? new Color(1f, 0.2f, 0.2f) : new Color(0.9f, 0.2f, 0.5f);

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    if (y >= h - 8 && Mathf.Abs(x - w / 2) < (h - y))
                        tex.SetPixel(x, y, tipCol);
                    else if (y >= 4 && x >= 4 && x <= w - 5)
                        tex.SetPixel(x, y, bodyCol);
                    else if (y < 4 && (x < 4 || x > w - 5))
                        tex.SetPixel(x, y, tipCol);
                    else
                        tex.SetPixel(x, y, Color.clear);
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 32);
        }

        private static void SpawnExplosionFX(Vector2 pos, float radius, bool evolved)
        {
            GameObject fx = new GameObject("MissileExplosionFX");
            fx.transform.position = pos;
            var sr = fx.AddComponent<SpriteRenderer>();

            Texture2D tex = new Texture2D(32, 32);
            tex.filterMode = FilterMode.Point;
            Color centerCol = evolved ? new Color(1f, 0.8f, 0.3f, 0.9f) : new Color(0.3f, 0.95f, 1f, 0.9f);

            for (int y = 0; y < 32; y++)
            {
                for (int x = 0; x < 32; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(16, 16));
                    if (dist <= 15f)
                    {
                        float alpha = Mathf.Clamp01(1f - dist / 15f);
                        tex.SetPixel(x, y, new Color(centerCol.r, centerCol.g, centerCol.b, alpha));
                    }
                    else
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                }
            }
            tex.Apply();
            sr.sprite = Sprite.Create(tex, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f));
            fx.transform.localScale = Vector3.one * (radius * 2.2f);
            Destroy(fx, 0.22f);
        }
    }

    public class DetoxMissileWeapon : WeaponBase
    {
        [Header("미사일 스펙")]
        [SerializeField] private float baseDamage = 22f;
        [SerializeField] private float blastRadius = 2.0f;
        [SerializeField] private int missileCount = 1;

        private void Awake()
        {
            weaponName = "디톡스 미사일";
            baseCooldown = 1.8f;
            currentLevel = 0;
            maxLevel = 5;
        }

        public override void Upgrade()
        {
            currentLevel++;
            switch (currentLevel)
            {
                case 1:
                    missileCount = 1;
                    baseDamage = 22f;
                    blastRadius = 2.0f;
                    break;
                case 2:
                    missileCount = 2;
                    baseCooldown = 1.6f;
                    break;
                case 3:
                    baseDamage = 30f;
                    blastRadius = 2.6f;
                    break;
                case 4:
                    missileCount = 3;
                    baseCooldown = 1.4f;
                    break;
                case 5:
                    missileCount = 4;
                    baseDamage = 42f;
                    blastRadius = 3.2f;
                    break;
            }
        }

        public override void Evolve()
        {
            isEvolved = true;
            weaponName = "✨ 궤도 폭격 이지스";
            baseDamage = 70f;
            missileCount = 6;
            blastRadius = 4.0f;
            baseCooldown = 0.8f;
        }

        protected override void Attack()
        {
            Enemy[] enemies = FindObjectsByType<Enemy>();
            List<Enemy> activeEnemies = new();
            foreach (var e in enemies)
            {
                if (e != null && e.gameObject.activeInHierarchy) activeEnemies.Add(e);
            }

            for (int i = 0; i < missileCount; i++)
            {
                GameObject mObj = new GameObject("DetoxMissile");
                mObj.transform.position = transform.position + new Vector3(Random.Range(-0.3f, 0.3f), Random.Range(-0.1f, 0.3f), 0);

                Transform targetT = null;
                if (activeEnemies.Count > 0)
                {
                    targetT = activeEnemies[Random.Range(0, activeEnemies.Count)].transform;
                }

                var missile = mObj.AddComponent<DetoxMissile>();
                missile.Setup(baseDamage, blastRadius, isEvolved, targetT, this);
            }
        }
    }
}
