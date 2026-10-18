using UnityEngine;
using DopamineCleaner.Enemies;

namespace DopamineCleaner.Weapons
{
    [RequireComponent(typeof(Collider2D))]
    public class Projectile : MonoBehaviour
    {
        [Header("투사체 스탯")]
        [SerializeField] private float speed = 12f;
        [SerializeField] private float damage = 20f;
        [SerializeField] private int pierce = 1;
        [SerializeField] private float lifetime = 2.5f;

        private Vector2 direction;
        private float lifeTimer;
        private SpriteRenderer spriteRenderer;

        private void Awake()
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        public void Initialize(Vector2 dir, float projSpeed, float projDamage, int projPierce, bool isEvolved)
        {
            direction = dir.normalized;
            speed = projSpeed;
            damage = projDamage;
            pierce = projPierce;
            lifeTimer = lifetime;

            if (spriteRenderer != null)
            {
                // 일반 탄환: 산뜻한 핑크, 진화 개틀링: 빛나는 골드
                spriteRenderer.color = isEvolved ? new Color(1f, 0.85f, 0.35f) : new Color(1f, 0.46f, 0.56f);
                transform.localScale = isEvolved ? Vector3.one * 0.65f : Vector3.one * 0.45f;
            }

            gameObject.SetActive(true);
        }

        private void Update()
        {
            transform.position += (Vector3)(direction * (speed * Time.deltaTime));

            lifeTimer -= Time.deltaTime;
            if (lifeTimer <= 0f)
            {
                gameObject.SetActive(false);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent<Enemy>(out var enemy) && enemy.IsAlive)
            {
                enemy.TakeDamage(damage, direction, 0.2f);
                pierce--;
                if (pierce <= 0)
                {
                    gameObject.SetActive(false);
                }
            }
        }
    }
}
