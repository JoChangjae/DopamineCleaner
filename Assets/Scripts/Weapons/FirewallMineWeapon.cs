using System.Collections.Generic;
using UnityEngine;
using DopamineCleaner.Enemies;

namespace DopamineCleaner.Weapons
{
    public class FirewallMine : MonoBehaviour
    {
        private float damage;
        private float explosionRadius;
        private bool isEvolved;
        private float lifeTimer = 3.5f;
        private bool hasExploded = false;
        private FirewallMineWeapon ownerWeapon;

        public void Setup(float dmg, float radius, bool evolved, FirewallMineWeapon owner = null)
        {
            damage = dmg;
            explosionRadius = radius;
            isEvolved = evolved;
            ownerWeapon = owner;
            lifeTimer = 4.0f;
            hasExploded = false;

            // 시각 표시 (작은 원형 스프라이트)
            var sr = GetComponent<SpriteRenderer>();
            if (sr == null) sr = gameObject.AddComponent<SpriteRenderer>();

            Texture2D tex = CreateCircleTexture(32, isEvolved ? new Color(1f, 0.4f, 0.1f) : new Color(0.2f, 0.8f, 1f));
            sr.sprite = Sprite.Create(tex, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f));
            transform.localScale = Vector3.one * (isEvolved ? 0.7f : 0.45f);

            var col = GetComponent<CircleCollider2D>();
            if (col == null) col = gameObject.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.5f;
        }

        private void Update()
        {
            if (hasExploded) return;

            lifeTimer -= Time.deltaTime;
            // 지뢰 깜빡임
            var sr = GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                float a = Mathf.PingPong(Time.time * 6f, 0.5f) + 0.5f;
                sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, a);
            }

            if (lifeTimer <= 0f)
            {
                Explode();
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (hasExploded) return;
            if (collision.CompareTag("Enemy") || collision.GetComponent<Enemy>() != null)
            {
                Explode();
            }
        }

        private void Explode()
        {
            if (hasExploded) return;
            hasExploded = true;

            // 폭발 범위 내 모든 적 대미지
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
            foreach (var col in colliders)
            {
                if (col.TryGetComponent<Enemy>(out var enemy))
                {
                    enemy.TakeDamage(damage);
                    ownerWeapon?.RegisterDamage(damage);
                }
            }

            // 폭발 시각 효과 (임시 스케일 확장 후 소멸)
            StartCoroutine(ExplosionVisual());
        }

        private System.Collections.IEnumerator ExplosionVisual()
        {
            var sr = GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.color = isEvolved ? new Color(1f, 0.6f, 0.1f, 0.8f) : new Color(0.3f, 0.9f, 1f, 0.8f);
            }

            float elapsed = 0f;
            float dur = isEvolved ? 0.5f : 0.2f;
            Vector3 startScale = transform.localScale;
            Vector3 endScale = Vector3.one * (explosionRadius * 2f);

            while (elapsed < dur)
            {
                elapsed += Time.deltaTime;
                transform.localScale = Vector3.Lerp(startScale, endScale, elapsed / dur);
                if (sr != null)
                {
                    Color c = sr.color;
                    c.a = Mathf.Lerp(0.8f, 0f, elapsed / dur);
                    sr.color = c;
                }
                yield return null;
            }

            Destroy(gameObject);
        }

        private Texture2D CreateCircleTexture(int size, Color color)
        {
            Texture2D tex = new Texture2D(size, size);
            Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
            float radius = size * 0.45f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center);
                    if (dist <= radius)
                    {
                        tex.SetPixel(x, y, color);
                    }
                    else
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                }
            }
            tex.Apply();
            return tex;
        }
    }

    public class FirewallMineWeapon : WeaponBase
    {
        [Header("지뢰 설정")]
        [SerializeField] private float damage = 40f;
        [SerializeField] private float explosionRadius = 2.4f;

        private void Awake()
        {
            weaponName = "방화벽 지뢰";
            baseCooldown = 1.8f;
            maxLevel = 5;
        }

        public override void Upgrade()
        {
            currentLevel++;
            damage += 20f;
            baseCooldown = Mathf.Max(0.8f, baseCooldown - 0.25f);
            explosionRadius += 0.4f;
        }

        public override void Evolve()
        {
            isEvolved = true;
            weaponName = "절대 보안 결계";
            damage *= 2.5f;
            baseCooldown = 0.9f;
            explosionRadius = 4.8f;
        }

        protected override void Attack()
        {
            // 플레이어 발 밑에 지뢰 배치
            GameObject mineObj = new GameObject("FirewallMine");
            mineObj.transform.position = transform.position;

            FirewallMine mine = mineObj.AddComponent<FirewallMine>();
            mine.Setup(damage, explosionRadius, isEvolved, this);
        }
    }
}
