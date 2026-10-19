using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DopamineCleaner.Enemies;

namespace DopamineCleaner.Weapons
{
    public class ShockAuraWeapon : WeaponBase
    {
        [Header("쇼크 펄스 설정")]
        [SerializeField] private float damage = 20f;
        [SerializeField] private float shockRadius = 5.5f;
        [SerializeField] private int targetCount = 3;

        private void Awake()
        {
            weaponName = "쇼크 펄스";
            baseCooldown = 1.4f;
            maxLevel = 5;
        }

        public override void Upgrade()
        {
            currentLevel++;
            damage += 12f;
            baseCooldown = Mathf.Max(0.6f, baseCooldown - 0.2f);
            shockRadius += 0.8f;
            targetCount++;
        }

        public override void Evolve()
        {
            isEvolved = true;
            weaponName = "테슬라 뇌파 방전";
            damage *= 2.3f;
            baseCooldown = 0.7f;
            shockRadius = 10f;
            targetCount = 8;
        }

        protected override void Attack()
        {
            Vector2 playerPos = transform.position;
            Collider2D[] colliders = Physics2D.OverlapCircleAll(playerPos, shockRadius);

            List<Enemy> candidates = new();
            foreach (var col in colliders)
            {
                if (col.TryGetComponent<Enemy>(out var enemy) && enemy.gameObject.activeInHierarchy)
                {
                    candidates.Add(enemy);
                }
            }

            int strikeCount = Mathf.Min(targetCount, candidates.Count);
            for (int i = 0; i < strikeCount; i++)
            {
                Enemy target = candidates[i];
                if (target == null) continue;

                target.TakeDamage(damage);
                RegisterDamage(damage);
                StartCoroutine(DrawLightningArc(playerPos, target.transform.position));
            }
        }

        private IEnumerator DrawLightningArc(Vector2 start, Vector2 end)
        {
            GameObject arcObj = new GameObject("LightningArc");
            LineRenderer lr = arcObj.AddComponent<LineRenderer>();
            lr.startWidth = isEvolved ? 0.35f : 0.18f;
            lr.endWidth = isEvolved ? 0.25f : 0.12f;
            lr.positionCount = 4;
            lr.useWorldSpace = true;

            Shader shader = Shader.Find("Sprites/Default");
            if (shader != null) lr.material = new Material(shader);

            Color boltColor = isEvolved ? new Color(1f, 0.9f, 0.3f, 1f) : new Color(0.4f, 0.9f, 1f, 1f);
            lr.startColor = boltColor;
            lr.endColor = boltColor;

            // 지그재그 번개 중간 지점 계산
            Vector2 mid1 = Vector2.Lerp(start, end, 0.33f) + Random.insideUnitCircle * 0.4f;
            Vector2 mid2 = Vector2.Lerp(start, end, 0.66f) + Random.insideUnitCircle * 0.4f;

            lr.SetPosition(0, start);
            lr.SetPosition(1, mid1);
            lr.SetPosition(2, mid2);
            lr.SetPosition(3, end);

            yield return new WaitForSeconds(0.1f);
            Destroy(arcObj);
        }
    }
}
