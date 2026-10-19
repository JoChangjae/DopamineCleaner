using UnityEngine;
using DopamineCleaner.Characters;

namespace DopamineCleaner.Enemies
{
    public class EnemyProjectile : MonoBehaviour
    {
        private Vector2 direction;
        private float speed = 5.0f;
        private float damage = 8.0f;
        private float lifeTime = 5.0f;

        public void Initialize(Vector2 dir, float spd, float dmg)
        {
            direction = dir.normalized;
            speed = spd;
            damage = dmg;

            var sr = GetComponent<SpriteRenderer>();
            if (sr == null) sr = gameObject.AddComponent<SpriteRenderer>();

            Texture2D tex = CreateBulletTex();
            sr.sprite = Sprite.Create(tex, new Rect(0, 0, 24, 24), new Vector2(0.5f, 0.5f));
            sr.color = new Color(1f, 0.2f, 0.35f);

            var col = GetComponent<CircleCollider2D>();
            if (col == null) col = gameObject.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.3f;
        }

        private void Update()
        {
            transform.position += (Vector3)(direction * (speed * Time.deltaTime));

            lifeTime -= Time.deltaTime;
            if (lifeTime <= 0f)
            {
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Player") || collision.GetComponent<PlayerController>() != null)
            {
                PlayerController.Instance?.TakeDamage(damage);
                Destroy(gameObject);
            }
        }

        private Texture2D CreateBulletTex()
        {
            Texture2D tex = new Texture2D(24, 24);
            Vector2 center = new Vector2(12, 12);
            for (int y = 0; y < 24; y++)
            {
                for (int x = 0; x < 24; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center);
                    tex.SetPixel(x, y, dist <= 10f ? Color.white : Color.clear);
                }
            }
            tex.Apply();
            return tex;
        }
    }
}
