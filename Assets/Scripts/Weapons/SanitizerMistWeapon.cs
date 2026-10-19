using System.Collections.Generic;
using UnityEngine;
using DopamineCleaner.Enemies;
using DopamineCleaner.Core;

namespace DopamineCleaner.Weapons
{
    public class SanitizerMistCloud : MonoBehaviour
    {
        private float damagePerTick;
        private float radius;
        private float duration;
        private float slowFactor;
        private bool isEvolved;
        private SanitizerMistWeapon ownerWeapon;

        private float lifeTimer;
        private float tickTimer = 0f;
        private const float TICK_INTERVAL = 0.3f;
        private SpriteRenderer spriteRenderer;

        public void Setup(float dmg, float rad, float dur, float slow, bool evolved, SanitizerMistWeapon owner)
        {
            damagePerTick = dmg;
            radius = rad;
            duration = dur;
            lifeTimer = dur;
            slowFactor = slow;
            isEvolved = evolved;
            ownerWeapon = owner;

            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null) spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = CreateMistSprite(isEvolved);
            transform.localScale = Vector3.one * (radius * 2f);

            var col = GetComponent<CircleCollider2D>();
            if (col == null) col = gameObject.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.5f;

            SoundManager.Instance?.Play(SoundEffect.Mist, 0.35f);
        }

        private void Update()
        {
            if (isEvolved && ownerWeapon != null)
            {
                // 진화형은 플레이어에 상시 부착
                transform.position = ownerWeapon.transform.position;
            }
            else
            {
                lifeTimer -= Time.deltaTime;
                if (lifeTimer <= 0f)
                {
                    Destroy(gameObject);
                    return;
                }

                // 페이드 아웃 연출
                if (spriteRenderer != null)
                {
                    float a = Mathf.Clamp01(lifeTimer / duration);
                    Color c = spriteRenderer.color;
                    spriteRenderer.color = new Color(c.r, c.g, c.b, a * 0.7f);
                }
            }

            tickTimer -= Time.deltaTime;
            if (tickTimer <= 0f)
            {
                tickTimer = TICK_INTERVAL;
                ApplyMistEffect();
            }
        }

        private void ApplyMistEffect()
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius);
            foreach (var hit in hits)
            {
                if (hit != null && hit.CompareTag("Enemy") && hit.TryGetComponent<Enemy>(out var enemy))
                {
                    float dmg = ownerWeapon != null ? ownerWeapon.CalculateDamage(damagePerTick, out bool isCrit) : damagePerTick;
                    enemy.TakeDamage(dmg);
                    ownerWeapon?.RegisterDamage(dmg);

                    // 슬로우 디버프 (적의 이동을 일시적으로 감속)
                    Vector2 away = ((Vector2)enemy.transform.position - (Vector2)transform.position).normalized;
                    enemy.transform.position -= (Vector3)(away * (slowFactor * 0.4f * Time.deltaTime));
                }
            }
        }

        private static Sprite CreateMistSprite(bool evolved)
        {
            int size = 64;
            Texture2D tex = new Texture2D(size, size);
            tex.filterMode = FilterMode.Bilinear;
            Color baseCol = evolved ? new Color(0.2f, 1f, 0.5f) : new Color(0.4f, 0.9f, 0.7f);

            Vector2 center = new Vector2(size / 2f, size / 2f);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center);
                    if (dist > size * 0.48f)
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                    else
                    {
                        float alpha = Mathf.Pow(1f - (dist / (size * 0.48f)), 1.8f);
                        tex.SetPixel(x, y, new Color(baseCol.r, baseCol.g, baseCol.b, alpha * 0.75f));
                    }
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }
    }

    public class SanitizerMistWeapon : WeaponBase
    {
        [Header("소독 연무 스펙")]
        [SerializeField] private float damagePerTick = 6f;
        [SerializeField] private float cloudRadius = 2.0f;
        [SerializeField] private float cloudDuration = 3.5f;
        [SerializeField] private float slowFactor = 0.3f;
        [SerializeField] private int sprayCount = 1;

        private SanitizerMistCloud permanentCloud;

        private void Awake()
        {
            weaponName = "멸균 소독 안개";
            baseCooldown = 1.4f;
            currentLevel = 0;
            maxLevel = 5;
        }

        public override void Upgrade()
        {
            currentLevel++;
            switch (currentLevel)
            {
                case 1:
                    sprayCount = 1;
                    damagePerTick = 6f;
                    cloudRadius = 2.0f;
                    break;
                case 2:
                    cloudRadius = 2.6f;
                    damagePerTick = 9f;
                    break;
                case 3:
                    sprayCount = 2;
                    slowFactor = 0.45f;
                    break;
                case 4:
                    damagePerTick = 15f;
                    cloudDuration = 4.8f;
                    break;
                case 5:
                    sprayCount = 3;
                    damagePerTick = 22f;
                    cloudRadius = 3.2f;
                    slowFactor = 0.55f;
                    break;
            }
        }

        public override void Evolve()
        {
            isEvolved = true;
            weaponName = "✨ 절대 멸균 플라즈마";
            damagePerTick = 42f;
            cloudRadius = 4.5f;
            slowFactor = 0.7f;
            baseCooldown = 0.5f;

            // 상시 플라즈마 오라 생성
            if (permanentCloud == null)
            {
                GameObject auraObj = new GameObject("PermanentPlasmaAura");
                auraObj.transform.position = transform.position;
                permanentCloud = auraObj.AddComponent<SanitizerMistCloud>();
                permanentCloud.Setup(damagePerTick, cloudRadius, 99999f, slowFactor, true, this);
            }
        }

        protected override void Attack()
        {
            if (isEvolved) return; // 진화 시 permanentCloud가 상시 작동

            for (int i = 0; i < sprayCount; i++)
            {
                Vector3 spawnOffset = (Vector3)Random.insideUnitCircle * (0.8f + i * 0.5f);
                GameObject cloudObj = new GameObject("SanitizerMistCloud");
                cloudObj.transform.position = transform.position + spawnOffset;

                var cloud = cloudObj.AddComponent<SanitizerMistCloud>();
                cloud.Setup(damagePerTick, cloudRadius, cloudDuration, slowFactor, false, this);
            }
        }
    }
}
