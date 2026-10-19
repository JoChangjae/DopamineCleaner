using System.Collections.Generic;
using UnityEngine;
using DopamineCleaner.Enemies;
using DopamineCleaner.Core;

namespace DopamineCleaner.Weapons
{
    public class AlgorithmVortex : MonoBehaviour
    {
        private float damagePerTick;
        private float pullRadius;
        private float pullForce;
        private float duration;
        private bool isEvolved;
        private AlgorithmVortexWeapon ownerWeapon;

        private Vector2 moveDirection;
        private float moveSpeed = 2.0f;
        private float lifeTimer;
        private float tickTimer = 0f;
        private const float TICK_INTERVAL = 0.25f;
        private bool isInitialized = false;

        public void Setup(float dmg, float radius, float force, float dur, Vector2 dir, bool evolved, AlgorithmVortexWeapon owner)
        {
            damagePerTick = dmg;
            pullRadius = radius;
            pullForce = force;
            duration = dur;
            lifeTimer = dur;
            moveDirection = dir.normalized;
            isEvolved = evolved;
            ownerWeapon = owner;
            isInitialized = true;

            var sr = GetComponent<SpriteRenderer>();
            if (sr == null) sr = gameObject.AddComponent<SpriteRenderer>();
            sr.sprite = CreateVortexSprite(isEvolved);
            transform.localScale = Vector3.one * (pullRadius * 0.9f);

            SoundManager.Instance?.Play(SoundEffect.Whirlwind, 0.4f);
        }

        private void Update()
        {
            if (!isInitialized) return;

            lifeTimer -= Time.deltaTime;
            if (lifeTimer <= 0f)
            {
                Destroy(gameObject);
                return;
            }

            // 고속 회전 시각 연출
            transform.Rotate(0, 0, (isEvolved ? -520f : -340f) * Time.deltaTime);

            // 이동
            if (!isEvolved)
            {
                transform.position += (Vector3)(moveDirection * (moveSpeed * Time.deltaTime));
            }
            else if (ownerWeapon != null)
            {
                // 진화형: 플레이어 주변을 궤도 선회
                transform.RotateAround(ownerWeapon.transform.position, Vector3.forward, 90f * Time.deltaTime);
            }

            // 적 흡입 (CC)
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, pullRadius);
            foreach (var hit in hits)
            {
                if (hit.CompareTag("Enemy") && hit.TryGetComponent<Enemy>(out var enemy))
                {
                    if (enemy.Type != EnemyType.DopamineBoss)
                    {
                        Vector2 toCenter = ((Vector2)transform.position - (Vector2)enemy.transform.position).normalized;
                        enemy.transform.position += (Vector3)(toCenter * (pullForce * Time.deltaTime));
                    }
                }
            }

            // 틱 대미지
            tickTimer -= Time.deltaTime;
            if (tickTimer <= 0f)
            {
                tickTimer = TICK_INTERVAL;
                ApplyTickDamage(hits);
            }
        }

        private void ApplyTickDamage(Collider2D[] hits)
        {
            foreach (var hit in hits)
            {
                if (hit != null && hit.CompareTag("Enemy") && hit.TryGetComponent<Enemy>(out var enemy))
                {
                    float dmg = ownerWeapon != null ? ownerWeapon.CalculateDamage(damagePerTick, out bool isCrit) : damagePerTick;
                    enemy.TakeDamage(dmg);
                    ownerWeapon?.RegisterDamage(dmg);
                }
            }
        }

        private static Sprite CreateVortexSprite(bool evolved)
        {
            int size = 64;
            Texture2D tex = new Texture2D(size, size);
            tex.filterMode = FilterMode.Bilinear;
            Color baseCol = evolved ? new Color(1f, 0.85f, 0.2f) : new Color(0.2f, 0.9f, 0.8f);

            Vector2 center = new Vector2(size / 2f, size / 2f);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Vector2 pos = new Vector2(x, y);
                    float dist = Vector2.Distance(pos, center);
                    if (dist > size * 0.48f)
                    {
                        tex.SetPixel(x, y, Color.clear);
                        continue;
                    }

                    float angle = Mathf.Atan2(y - center.y, x - center.x);
                    float spiral = Mathf.Sin(angle * 3f + dist * 0.4f);
                    float alpha = Mathf.Clamp01((1f - dist / (size * 0.48f)) * (0.4f + spiral * 0.6f));

                    Color c = new Color(baseCol.r, baseCol.g, baseCol.b, alpha * 0.8f);
                    tex.SetPixel(x, y, c);
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }
    }

    public class AlgorithmVortexWeapon : WeaponBase
    {
        [Header("회오리 스펙")]
        [SerializeField] private float damagePerTick = 8f;
        [SerializeField] private float pullRadius = 2.2f;
        [SerializeField] private float pullForce = 3.5f;
        [SerializeField] private float vortexDuration = 4.0f;
        [SerializeField] private int vortexCount = 1;

        private void Awake()
        {
            weaponName = "알고리즘 회오리";
            baseCooldown = 2.6f;
            currentLevel = 0;
            maxLevel = 5;
        }

        public override void Upgrade()
        {
            currentLevel++;
            switch (currentLevel)
            {
                case 1:
                    vortexCount = 1;
                    damagePerTick = 8f;
                    pullRadius = 2.2f;
                    break;
                case 2:
                    pullRadius = 2.8f;
                    pullForce = 4.5f;
                    break;
                case 3:
                    vortexCount = 2;
                    damagePerTick = 12f;
                    break;
                case 4:
                    vortexDuration = 5.5f;
                    baseCooldown = 2.2f;
                    break;
                case 5:
                    vortexCount = 3;
                    damagePerTick = 18f;
                    pullRadius = 3.4f;
                    break;
            }
        }

        public override void Evolve()
        {
            isEvolved = true;
            weaponName = "✨ 하이퍼 디톡스 타이푼";
            damagePerTick = 32f;
            pullRadius = 4.8f;
            pullForce = 7.5f;
            vortexDuration = 10f;
            vortexCount = 2;
            baseCooldown = 3.5f;
        }

        protected override void Attack()
        {
            float angleStep = 360f / vortexCount;
            float baseAngle = Random.Range(0f, 360f);

            for (int i = 0; i < vortexCount; i++)
            {
                float angle = (baseAngle + i * angleStep) * Mathf.Deg2Rad;
                Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

                GameObject vObj = new GameObject("AlgorithmVortex");
                vObj.transform.position = transform.position + (Vector3)(dir * 0.8f);

                var vortex = vObj.AddComponent<AlgorithmVortex>();
                vortex.Setup(damagePerTick, pullRadius, pullForce, vortexDuration, dir, isEvolved, this);
            }
        }
    }
}
