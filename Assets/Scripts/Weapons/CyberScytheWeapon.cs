using System.Collections.Generic;
using UnityEngine;
using DopamineCleaner.Enemies;
using DopamineCleaner.Core;

namespace DopamineCleaner.Weapons
{
    public class CyberScytheSlash : MonoBehaviour
    {
        private float damage;
        private float sweepAngle;
        private float radius;
        private float knockback;
        private bool isEvolved;
        private CyberScytheWeapon ownerWeapon;

        private float startAngle;
        private float currentAngle;
        private float sweepDuration = 0.22f;
        private float elapsed = 0f;
        private readonly HashSet<Enemy> hitEnemies = new();
        private bool isInitialized = false;

        public void Setup(float dmg, float rad, float angle, float kb, bool evolved, CyberScytheWeapon owner)
        {
            damage = dmg;
            radius = rad;
            sweepAngle = angle;
            knockback = kb;
            isEvolved = evolved;
            ownerWeapon = owner;

            startAngle = Random.Range(0f, 360f);
            currentAngle = startAngle;
            isInitialized = true;

            var sr = GetComponent<SpriteRenderer>();
            if (sr == null) sr = gameObject.AddComponent<SpriteRenderer>();
            sr.sprite = CreateBladeSprite(isEvolved);
            transform.localScale = Vector3.one * (radius * 1.3f);

            SoundManager.Instance?.Play(SoundEffect.Slash, 0.45f);
        }

        private void Update()
        {
            if (!isInitialized) return;

            elapsed += Time.deltaTime;
            float t = elapsed / sweepDuration;

            if (t >= 1f)
            {
                Destroy(gameObject);
                return;
            }

            // 부채꼴/원형 고속 회전 이동
            float angle = Mathf.Lerp(startAngle, startAngle + sweepAngle, t);
            transform.rotation = Quaternion.Euler(0, 0, angle);

            if (ownerWeapon != null)
            {
                transform.position = ownerWeapon.transform.position;
            }

            // 참격 궤적 내 적 판정
            CheckSlashCollisions();
        }

        private void CheckSlashCollisions()
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius);
            foreach (var hit in hits)
            {
                if (hit != null && hit.CompareTag("Enemy") && hit.TryGetComponent<Enemy>(out var enemy))
                {
                    if (hitEnemies.Add(enemy))
                    {
                        float finalDmg = ownerWeapon != null ? ownerWeapon.CalculateDamage(damage, out bool isCrit) : damage;

                        // 진화 특권: 일반 적 체력 30% 이하 시 즉사 단두대 판정
                        if (isEvolved && enemy.Type != EnemyType.DopamineBoss && enemy.HP <= enemy.MaxHP * 0.3f)
                        {
                            finalDmg = enemy.HP + 999f;
                            DamageTextManager.Instance?.SpawnDamageText(finalDmg, enemy.transform.position, true);
                        }

                        Vector2 kbDir = ((Vector2)enemy.transform.position - (Vector2)transform.position).normalized;
                        enemy.TakeDamage(finalDmg, kbDir, knockback);
                        ownerWeapon?.RegisterDamage(finalDmg);
                    }
                }
            }
        }

        private static Sprite CreateBladeSprite(bool evolved)
        {
            int size = 64;
            Texture2D tex = new Texture2D(size, size);
            tex.filterMode = FilterMode.Bilinear;
            Color bladeCol = evolved ? new Color(1f, 0.85f, 0.2f, 0.95f) : new Color(0.2f, 0.7f, 1f, 0.95f);
            Color glowCol = evolved ? new Color(1f, 0.4f, 0.1f, 0.6f) : new Color(0.6f, 0.2f, 1f, 0.6f);

            Vector2 center = new Vector2(size * 0.2f, size * 0.2f);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center);
                    // 초승달/호 형태
                    if (dist >= size * 0.4f && dist <= size * 0.85f && x >= y - 8)
                    {
                        float edge = 1f - Mathf.Abs(dist - size * 0.62f) / (size * 0.23f);
                        Color c = Color.Lerp(glowCol, bladeCol, edge);
                        c.a *= edge;
                        tex.SetPixel(x, y, c);
                    }
                    else
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.2f, 0.2f), size);
        }
    }

    public class CyberScytheWeapon : WeaponBase
    {
        [Header("단두대 스펙")]
        [SerializeField] private float slashDamage = 35f;
        [SerializeField] private float bladeRadius = 2.4f;
        [SerializeField] private float sweepAngle = 180f;
        [SerializeField] private float knockbackForce = 1.4f;
        [SerializeField] private int slashRounds = 1;

        private void Awake()
        {
            weaponName = "셧다운 단두대";
            baseCooldown = 1.95f;
            currentLevel = 0;
            maxLevel = 5;
        }

        public override void Upgrade()
        {
            currentLevel++;
            switch (currentLevel)
            {
                case 1:
                    slashDamage = 35f;
                    bladeRadius = 2.4f;
                    sweepAngle = 180f;
                    slashRounds = 1;
                    break;
                case 2:
                    sweepAngle = 270f;
                    slashDamage = 48f;
                    break;
                case 3:
                    sweepAngle = 360f;
                    bladeRadius = 3.0f;
                    baseCooldown = 1.9f;
                    break;
                case 4:
                    slashDamage = 68f;
                    knockbackForce = 2.0f;
                    break;
                case 5:
                    slashRounds = 2; // 2연속 참격
                    slashDamage = 85f;
                    bladeRadius = 3.6f;
                    break;
            }
        }

        public override void Evolve()
        {
            isEvolved = true;
            weaponName = "✨ 절대 셧다운 엑스칼리버";
            slashDamage = 160f;
            bladeRadius = 5.2f;
            sweepAngle = 720f; // 2바퀴 초고속 베기
            slashRounds = 3;
            baseCooldown = 1.3f;
            knockbackForce = 3.0f;
        }

        protected override void Attack()
        {
            for (int i = 0; i < slashRounds; i++)
            {
                Invoke(nameof(SpawnSlash), i * 0.12f);
            }
        }

        private void SpawnSlash()
        {
            GameObject slashObj = new GameObject("CyberScytheSlash");
            slashObj.transform.position = transform.position;

            var slash = slashObj.AddComponent<CyberScytheSlash>();
            slash.Setup(slashDamage, bladeRadius, sweepAngle, knockbackForce, isEvolved, this);
        }
    }
}
