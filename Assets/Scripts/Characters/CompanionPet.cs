using UnityEngine;
using DopamineCleaner.Core;
using DopamineCleaner.Enemies;
using DopamineCleaner.Items;

namespace DopamineCleaner.Characters
{
    public class CompanionPet : MonoBehaviour
    {
        public static CompanionPet Instance { get; private set; }

        [Header("펫 설정")]
        [SerializeField] private CompanionType companionType = CompanionType.HappyDog;
        [SerializeField] private float followSpeed = 4.5f;
        [SerializeField] private float followDistance = 1.3f;

        private float skillTimer = 0f;
        private float pomodoroCycleTimer = 0f;
        private bool isPomodoroFocus = true;
        private SpriteRenderer spriteRenderer;

        public CompanionType Type => companionType;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            SetupVisualSprite();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public void Initialize(CompanionType type)
        {
            companionType = type;
            skillTimer = 2.0f;
            pomodoroCycleTimer = 25.0f;
            isPomodoroFocus = true;
            SetupVisualSprite();
        }

        private void SetupVisualSprite()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null) spriteRenderer = gameObject.AddComponent<SpriteRenderer>();

            int w = 24, h = 24;
            Texture2D tex = new Texture2D(w, h);
            tex.filterMode = FilterMode.Point;
            Color baseColor = companionType switch
            {
                CompanionType.HappyDog => new Color(0.95f, 0.7f, 0.3f), // 황금빛 댕댕이
                CompanionType.NapCat => new Color(0.9f, 0.5f, 0.8f),   // 핑크 냥이
                CompanionType.PomodoroDrone => new Color(0.2f, 0.9f, 1f), // 네온 사이언 드론
                _ => Color.white
            };

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(w * 0.5f, h * 0.5f));
                    tex.SetPixel(x, y, dist < 10f ? baseColor : Color.clear);
                }
            }
            tex.Apply();
            spriteRenderer.sprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 24f);
            transform.localScale = Vector3.one * 0.75f;
        }

        private void Update()
        {
            if (PlayerController.Instance == null) return;
            if (companionType == CompanionType.None) return;

            // 1. 플레이어 부드럽게 추종
            Vector3 targetPos = PlayerController.Instance.transform.position + new Vector3(-0.9f, 0.7f, 0f);
            float dist = Vector3.Distance(transform.position, targetPos);
            if (dist > followDistance)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPos, followSpeed * Time.deltaTime * Mathf.Min(dist * 1.5f, 3.0f));
            }

            // 2. 고유 스킬 주기적 발동
            ExecuteCompanionSkill();
        }

        private void ExecuteCompanionSkill()
        {
            switch (companionType)
            {
                case CompanionType.HappyDog:
                    // 🐕 산책견 댕댕이: 1.2초마다 주변 8m 내 보석 자석 유도
                    skillTimer -= Time.deltaTime;
                    if (skillTimer <= 0f)
                    {
                        skillTimer = 1.2f;
                        CollectNearbyGems();
                    }
                    break;

                case CompanionType.NapCat:
                    // 🐱 단잠 냥이: 14초마다 힐링 쉼터 장판 (체력 22 회복 & 과부하 20% 정화)
                    skillTimer -= Time.deltaTime;
                    if (skillTimer <= 0f)
                    {
                        skillTimer = 14.0f;
                        DeployNapSanctuary();
                    }
                    break;

                case CompanionType.PomodoroDrone:
                    // 🤖 뽀모도로 드론: 25초 집중(1.2초마다 사격) / 5초 휴식(경험치 2배)
                    pomodoroCycleTimer -= Time.deltaTime;
                    if (pomodoroCycleTimer <= 0f)
                    {
                        isPomodoroFocus = !isPomodoroFocus;
                        pomodoroCycleTimer = isPomodoroFocus ? 25.0f : 5.0f;
                        SoundManager.Instance?.Play(SoundEffect.LevelUp, 0.4f);
                    }

                    if (isPomodoroFocus)
                    {
                        skillTimer -= Time.deltaTime;
                        if (skillTimer <= 0f)
                        {
                            skillTimer = 1.1f;
                            ShootPomodoroLaser();
                        }
                    }
                    break;
            }
        }

        private void CollectNearbyGems()
        {
            var gems = FindObjectsByType<ExpGem>();
            int collected = 0;
            foreach (var g in gems)
            {
                if (g != null && g.gameObject.activeInHierarchy && Vector2.Distance(transform.position, g.transform.position) <= 8.5f)
                {
                    // 플레이어에게 고속 이동 트리거
                    g.transform.position = Vector3.MoveTowards(g.transform.position, PlayerController.Instance.transform.position, 6.0f * Time.deltaTime);
                    collected++;
                    if (collected >= 5) break;
                }
            }
        }

        private void DeployNapSanctuary()
        {
            PlayerController.Instance?.Heal(22f);
            DamageTextManager.Instance?.SpawnDamageText(22f, transform.position + Vector3.up * 0.5f, false);
            SoundManager.Instance?.Play(SoundEffect.GemCollect, 0.7f);
        }

        private void ShootPomodoroLaser()
        {
            var enemies = FindObjectsByType<Enemy>();
            Enemy closest = null;
            float minDist = 9.0f;

            foreach (var e in enemies)
            {
                if (e != null && e.IsAlive)
                {
                    float d = Vector2.Distance(transform.position, e.transform.position);
                    if (d < minDist)
                    {
                        minDist = d;
                        closest = e;
                    }
                }
            }

            if (closest != null)
            {
                closest.TakeDamage(40f);
                DamageTextManager.Instance?.SpawnDamageText(40f, closest.transform.position, true);
                SoundManager.Instance?.Play(SoundEffect.Shoot, 0.3f);
            }
        }
    }
}
