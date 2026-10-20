using System.Collections.Generic;
using UnityEngine;

namespace DopamineCleaner.Core
{
    public class PixelParticleManager : MonoBehaviour
    {
        private static PixelParticleManager instance;
        public static PixelParticleManager Instance
        {
            get
            {
                if (instance == null) instance = FindAnyObjectByType<PixelParticleManager>();
                return instance;
            }
            private set => instance = value;
        }

        private class PixelParticle
        {
            public GameObject gameObject;
            public SpriteRenderer renderer;
            public Vector3 velocity;
            public float lifetime;
            public float maxLifetime;
            public float drag;
            public float gravity;
            public Color baseColor;
            public float initialScale;
            public float rotationSpeed;
        }

        [Header("파티클 풀 설정")]
        [SerializeField] private int maxPoolSize = 100;
        private readonly List<PixelParticle> pool = new();
        private readonly List<PixelParticle> activeParticles = new();

        private Sprite defaultPixelSprite;

        private void Awake()
        {
            if (instance == null || instance == this)
            {
                instance = this;
            }
            else
            {
                if (Application.isPlaying) Destroy(gameObject);
                else DestroyImmediate(gameObject);
                return;
            }

            CreateDefaultPixelSprite();
            InitializePool();
        }

        private void CreateDefaultPixelSprite()
        {
            Texture2D tex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
            Color[] colors = new Color[16];
            for (int i = 0; i < 16; i++) colors[i] = Color.white;
            tex.SetPixels(colors);
            tex.filterMode = FilterMode.Point;
            tex.Apply();

            defaultPixelSprite = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 16f);
        }

        private void InitializePool()
        {
            for (int i = 0; i < maxPoolSize; i++)
            {
                GameObject obj = new GameObject($"PixelParticle_{i}");
                obj.transform.SetParent(transform);
                SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
                sr.sprite = defaultPixelSprite;
                sr.sortingOrder = 40;
                obj.SetActive(false);

                pool.Add(new PixelParticle
                {
                    gameObject = obj,
                    renderer = sr
                });
            }
        }

        private PixelParticle GetParticle()
        {
            for (int i = 0; i < pool.Count; i++)
            {
                if (!pool[i].gameObject.activeSelf)
                    return pool[i];
            }

            // 풀 부족 시 가장 오래된 파티클 재활용
            if (activeParticles.Count > 0)
            {
                PixelParticle oldest = activeParticles[0];
                activeParticles.RemoveAt(0);
                return oldest;
            }

            return null;
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            for (int i = activeParticles.Count - 1; i >= 0; i--)
            {
                PixelParticle p = activeParticles[i];
                p.lifetime += dt;

                if (p.lifetime >= p.maxLifetime)
                {
                    p.gameObject.SetActive(false);
                    activeParticles.RemoveAt(i);
                    continue;
                }

                // 이동 및 감속
                p.velocity = Vector3.Lerp(p.velocity, Vector3.zero, p.drag * dt);
                p.velocity.y -= p.gravity * dt;
                p.gameObject.transform.position += p.velocity * dt;
                p.gameObject.transform.Rotate(0f, 0f, p.rotationSpeed * dt);

                // 페이드 및 스케일 축소
                float progress = p.lifetime / p.maxLifetime;
                float alpha = 1.0f - progress;
                Color c = p.baseColor;
                c.a = alpha;
                p.renderer.color = c;

                float scale = Mathf.Lerp(p.initialScale, 0f, progress * progress);
                p.gameObject.transform.localScale = Vector3.one * scale;
            }
        }

        /// <summary>
        /// 몬스터 처치 시 팝콘처럼 사방으로 튀어나가는 네온 도파민 픽셀 파티클
        /// </summary>
        public void SpawnDeathPop(Vector3 position, Color themeColor, int count = 12)
        {
            for (int i = 0; i < count; i++)
            {
                PixelParticle p = GetParticle();
                if (p == null) break;

                p.gameObject.transform.position = position;
                p.gameObject.SetActive(true);

                float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                float speed = Random.Range(3.5f, 7.5f);
                p.velocity = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * speed;

                p.lifetime = 0f;
                p.maxLifetime = Random.Range(0.35f, 0.65f);
                p.drag = Random.Range(3.0f, 5.0f);
                p.gravity = 1.2f;
                p.baseColor = Color.Lerp(themeColor, Color.white, Random.Range(0.1f, 0.4f));
                p.initialScale = Random.Range(0.6f, 1.2f);
                p.rotationSpeed = Random.Range(-360f, 360f);

                activeParticles.Add(p);
            }
        }

        public void SpawnNeonPop(Vector3 position, Color themeColor, int count = 12)
        {
            SpawnDeathPop(position, themeColor, count);
        }

        /// <summary>
        /// 탄환 피격 시 미세 네온 스파크 분사
        /// </summary>
        public void SpawnSparkHit(Vector3 position, Vector3 hitDirection, Color sparkColor, int count = 4)
        {
            for (int i = 0; i < count; i++)
            {
                PixelParticle p = GetParticle();
                if (p == null) break;

                p.gameObject.transform.position = position;
                p.gameObject.SetActive(true);

                Vector3 spreadDir = Quaternion.Euler(0, 0, Random.Range(-45f, 45f)) * (-hitDirection.normalized);
                float speed = Random.Range(4.0f, 9.0f);
                p.velocity = spreadDir * speed;

                p.lifetime = 0f;
                p.maxLifetime = Random.Range(0.12f, 0.25f);
                p.drag = 6.0f;
                p.gravity = 0f;
                p.baseColor = sparkColor;
                p.initialScale = Random.Range(0.35f, 0.65f);
                p.rotationSpeed = Random.Range(-500f, 500f);

                activeParticles.Add(p);
            }
        }

        /// <summary>
        /// 피버 / 오버드라이브 시 플레이어 주변 화염 네온 아우라
        /// </summary>
        public void SpawnFeverAura(Vector3 playerPos, bool isOverdrive)
        {
            int count = isOverdrive ? 4 : 2;
            for (int i = 0; i < count; i++)
            {
                PixelParticle p = GetParticle();
                if (p == null) break;

                Vector3 offset = (Vector3)Random.insideUnitCircle * 0.7f;
                p.gameObject.transform.position = playerPos + offset;
                p.gameObject.SetActive(true);

                p.velocity = new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(1.5f, 3.5f), 0f);
                p.lifetime = 0f;
                p.maxLifetime = Random.Range(0.25f, 0.45f);
                p.drag = 1.0f;
                p.gravity = -2.5f; // 위로 솟구침 (화염 효과)
                p.baseColor = isOverdrive ? new Color(1f, 0.2f, 0.9f) : new Color(1f, 0.85f, 0.1f);
                p.initialScale = Random.Range(0.5f, 0.9f);
                p.rotationSpeed = Random.Range(-180f, 180f);

                activeParticles.Add(p);
            }
        }
    }
}
