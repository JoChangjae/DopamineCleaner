using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DopamineCleaner.Characters;
using DopamineCleaner.Enemies;
using DopamineCleaner.Meta;

namespace DopamineCleaner.Core
{
    /// <summary>
    /// 스테이지별 고유 다이내믹 환경 재해 및 필드 인터랙티브 기믹 매니저.
    /// - 심야 고속도로: 주기적으로 경적과 함께 궤적 예고 후 도로를 질주하는 광속 트럭 (유저/몬스터 관통 타격)
    /// - 오피스 미궁: E키로 마시는 에스프레소 커피머신(공속/이속 부스트) 및 잡생각 분쇄 세단기 트랩
    /// - 격리 병동: 상시 힐링 링거 거치대 존 및 주기적 감전 제세동기 트랩
    /// </summary>
    public class StageHazardManager : MonoBehaviour
    {
        private static StageHazardManager instance;
        public static StageHazardManager Instance
        {
            get
            {
                if (instance == null) instance = FindAnyObjectByType<StageHazardManager>();
                return instance;
            }
            private set => instance = value;
        }

        private string currentDungeonId = "map_hospital";
        private float truckTimer = 15f;
        private readonly List<InteractiveGimmick> activeGimmicks = new();

        public class InteractiveGimmick
        {
            public GameObject gameObject;
            public string type;
            public Vector2 position;
            public float cooldownTimer;
            public float effectTimer;
        }

        private void Awake()
        {
            if (instance == null || instance == this) instance = this;
            else
            {
                if (Application.isPlaying) Destroy(gameObject);
                else DestroyImmediate(gameObject);
            }
        }

        private void OnDestroy()
        {
            if (instance == this) instance = null;
        }

        private void Start()
        {
            if (GameDataManager.Instance != null && !string.IsNullOrEmpty(GameDataManager.Instance.SelectedMapId))
            {
                currentDungeonId = GameDataManager.Instance.SelectedMapId;
            }

            SetupStageHazards();
        }

        private void SetupStageHazards()
        {
            switch (currentDungeonId)
            {
                case "map_highway":
                    truckTimer = 18f;
                    break;

                case "map_office":
                    SpawnOfficeGimmicks();
                    break;

                case "map_hospital":
                default:
                    SpawnHospitalGimmicks();
                    break;
            }
        }

        private void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing)
                return;

            if (PlayerController.Instance == null) return;

            // 1. 고속도로 맵: 광속 트럭 돌진 타이머
            if (currentDungeonId == "map_highway")
            {
                truckTimer -= Time.deltaTime;
                if (truckTimer <= 0f)
                {
                    truckTimer = Random.Range(18f, 24f);
                    StartCoroutine(SpawnHighwayTruckRoutine());
                }
            }

            // 2. 필드 기믹들 인터랙션 및 주기적 틱 처리
            UpdateActiveGimmicks();
        }

        #region Highway Hazard: 광속 숏폼 트럭
        private IEnumerator SpawnHighwayTruckRoutine()
        {
            Vector3 playerPos = PlayerController.Instance.transform.position;
            float spawnY = playerPos.y + Random.Range(-2.5f, 2.5f);
            float startX = playerPos.x - 25f;
            float endX = playerPos.x + 25f;

            // 1. 화면 사이렌 및 경고 텍스트
            SoundManager.Instance?.Play(SoundEffect.Explosion, 0.4f);
            DamageTextManager.Instance?.SpawnCustomText("🚨 [주의: 광속 숏폼 트럭 돌진!] 🚨", new Vector3(playerPos.x, spawnY + 0.8f, 0f), new Color(1f, 0.2f, 0.3f));

            // 2. 붉은 경고 궤적선 (1.2초간 깜빡임)
            GameObject telegraph = new GameObject("TruckTelegraph");
            LineRenderer lr = telegraph.AddComponent<LineRenderer>();
            lr.startWidth = 1.4f;
            lr.endWidth = 1.4f;
            lr.positionCount = 2;
            lr.SetPosition(0, new Vector3(startX, spawnY, 0));
            lr.SetPosition(1, new Vector3(endX, spawnY, 0));
            lr.material = new Material(Shader.Find("Sprites/Default"));
            lr.startColor = new Color(1f, 0.1f, 0.2f, 0.45f);
            lr.endColor = new Color(1f, 0.1f, 0.2f, 0.45f);

            float warnTime = 1.1f;
            while (warnTime > 0f)
            {
                warnTime -= Time.deltaTime;
                float alpha = (Mathf.Sin(warnTime * 20f) + 1f) * 0.25f + 0.2f;
                lr.startColor = new Color(1f, 0.1f, 0.2f, alpha);
                lr.endColor = new Color(1f, 0.1f, 0.2f, alpha);
                yield return null;
            }

            Destroy(telegraph);

            // 3. 광속 트럭 질주
            GameObject truck = new GameObject("HighwayTruck");
            truck.transform.position = new Vector3(startX, spawnY, 0);

            SpriteRenderer sr = truck.AddComponent<SpriteRenderer>();
            sr.sprite = CreatePixelTruckSprite();
            sr.sortingOrder = 10;
            truck.transform.localScale = new Vector3(2.5f, 1.6f, 1f);

            float speed = 36f;
            float currentX = startX;

            SoundManager.Instance?.Play(SoundEffect.Dash, 0.9f);
            CameraFollow.Instance?.Shake(0.4f, 0.35f);

            HashSet<Enemy> hitEnemies = new();
            bool hitPlayer = false;

            while (currentX < endX)
            {
                currentX += speed * Time.deltaTime;
                truck.transform.position = new Vector3(currentX, spawnY, 0);

                // 유저 충돌 판정
                if (!hitPlayer && PlayerController.Instance != null && !PlayerController.Instance.IsInvincible)
                {
                    if (Vector2.Distance(truck.transform.position, PlayerController.Instance.transform.position) < 1.8f)
                    {
                        hitPlayer = true;
                        PlayerController.Instance.TakeDamage(25f);
                        DamageTextManager.Instance?.SpawnCustomText("💥 교통사고!", PlayerController.Instance.transform.position + Vector3.up, Color.red);
                    }
                }

                // 몬스터 충돌 판정 (대량 압사)
                Collider2D[] hits = Physics2D.OverlapCircleAll(truck.transform.position, 1.8f);
                foreach (var h in hits)
                {
                    if (h != null && h.CompareTag("Enemy") && h.TryGetComponent<Enemy>(out var e))
                    {
                        if (!hitEnemies.Contains(e))
                        {
                            hitEnemies.Add(e);
                            Vector2 launchDir = new Vector2(1f, Random.Range(-0.5f, 0.5f)).normalized;
                            e.TakeDamage(350f, launchDir, 4.5f);
                            PixelParticleManager.Instance?.SpawnNeonPop(e.transform.position, new Color(1f, 0.4f, 0.2f), 8);
                        }
                    }
                }

                yield return null;
            }

            Destroy(truck);
        }

        private Sprite CreatePixelTruckSprite()
        {
            int w = 32, h = 18;
            Texture2D tex = new Texture2D(w, h);
            tex.filterMode = FilterMode.Point;
            Color body = new Color(0.9f, 0.2f, 0.3f);      // 사이버 레드 트럭 바디
            Color window = new Color(0.2f, 0.85f, 1f);    // 네온 유리창
            Color wheel = new Color(0.12f, 0.12f, 0.15f); // 휠

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    if (y < 4 && (x is >= 4 and <= 9 || x is >= 22 and <= 27))
                        tex.SetPixel(x, y, wheel);
                    else if (y is >= 7 and <= 14 && x is >= 20 and <= 30)
                        tex.SetPixel(x, y, window);
                    else
                        tex.SetPixel(x, y, body);
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 16f);
        }
        #endregion

        #region Office Gimmicks: 커피 머신 & 문서 세단기
        private void SpawnOfficeGimmicks()
        {
            Vector3 center = PlayerController.Instance != null ? PlayerController.Instance.transform.position : Vector3.zero;

            // 1. 에스프레소 커피 머신 2대
            CreateGimmickObject("CoffeeMachine_1", center + new Vector3(-8f, 5f, 0), "coffee", new Color(0.6f, 0.35f, 0.15f), "☕ [에스프레소 머신]");
            CreateGimmickObject("CoffeeMachine_2", center + new Vector3(8f, -6f, 0), "coffee", new Color(0.6f, 0.35f, 0.15f), "☕ [에스프레소 머신]");

            // 2. 문서 세단기 트랩 2대
            CreateGimmickObject("Shredder_1", center + new Vector3(7f, 6f, 0), "shredder", new Color(0.35f, 0.45f, 0.55f), "⚙️ [초고속 문서 세단기]");
            CreateGimmickObject("Shredder_2", center + new Vector3(-7f, -5f, 0), "shredder", new Color(0.35f, 0.45f, 0.55f), "⚙️ [초고속 문서 세단기]");
        }

        private void SpawnHospitalGimmicks()
        {
            Vector3 center = PlayerController.Instance != null ? PlayerController.Instance.transform.position : Vector3.zero;

            // 1. 정맥 링거 거치대 힐링 존
            CreateGimmickObject("IV_Station_1", center + new Vector3(-7f, 4f, 0), "iv_heal", new Color(0.2f, 0.95f, 0.65f), "🩺 [정맥 수액 힐링 존]");

            // 2. 심장 제세동기 전기 트랩
            CreateGimmickObject("Defibrillator_1", center + new Vector3(8f, -4f, 0), "defibrillator", new Color(1f, 0.9f, 0.25f), "⚡ [심장 제세동기]");
        }

        private void CreateGimmickObject(string name, Vector3 pos, string type, Color color, string label)
        {
            GameObject obj = new GameObject(name);
            obj.transform.position = pos;

            SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = CreateBoxSprite(color);
            sr.sortingOrder = 2;
            obj.transform.localScale = Vector3.one * 1.35f;

            CircleCollider2D col = obj.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 1.2f;

            activeGimmicks.Add(new InteractiveGimmick
            {
                gameObject = obj,
                type = type,
                position = pos,
                cooldownTimer = 0f,
                effectTimer = 0f
            });
        }

        private Sprite CreateBoxSprite(Color col)
        {
            int s = 24;
            Texture2D tex = new Texture2D(s, s);
            tex.filterMode = FilterMode.Point;
            for (int y = 0; y < s; y++)
            {
                for (int x = 0; x < s; x++)
                {
                    bool isBorder = x == 0 || x == s - 1 || y == 0 || y == s - 1;
                    tex.SetPixel(x, y, isBorder ? Color.white : col);
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, s, s), new Vector2(0.5f, 0.5f), 20f);
        }

        private void UpdateActiveGimmicks()
        {
            if (PlayerController.Instance == null) return;
            Vector2 pPos = PlayerController.Instance.transform.position;

            foreach (var g in activeGimmicks)
            {
                if (g.gameObject == null) continue;

                if (g.cooldownTimer > 0f)
                {
                    g.cooldownTimer -= Time.deltaTime;
                }

                float distToPlayer = Vector2.Distance(pPos, g.position);

                switch (g.type)
                {
                    case "coffee":
                        // 플레이어가 가까이 오면 상호작용 안내
                        if (distToPlayer < 2.0f && g.cooldownTimer <= 0f)
                        {
                            if (Input.GetKeyDown(KeyCode.E))
                            {
                                g.cooldownTimer = 30f;
                                SoundManager.Instance?.Play(SoundEffect.GemCollect, 0.9f);
                                DamageTextManager.Instance?.SpawnCustomText("☕ 카페인 폭발! (이속+40%, 공속+50%!)", PlayerController.Instance.transform.position + Vector3.up * 0.9f, new Color(1f, 0.75f, 0.2f));
                                FeverManager.Instance?.AddFever(35f);
                                PixelParticleManager.Instance?.SpawnNeonPop((Vector3)g.position, new Color(0.9f, 0.6f, 0.2f), 12);
                            }
                        }
                        break;

                    case "shredder":
                        // 몬스터 흡입 분쇄
                        Collider2D[] shHits = Physics2D.OverlapCircleAll(g.position, 3.2f);
                        foreach (var hit in shHits)
                        {
                            if (hit != null && hit.CompareTag("Enemy") && hit.TryGetComponent<Enemy>(out var e))
                            {
                                e.transform.position = Vector3.MoveTowards(e.transform.position, (Vector3)g.position, 2.5f * Time.deltaTime);
                                e.TakeDamage(40f * Time.deltaTime);
                            }
                        }
                        break;

                    case "iv_heal":
                        // 플레이어가 존 안에 머물면 지속 치유
                        if (distToPlayer < 2.5f)
                        {
                            PlayerController.Instance.Heal(8f * Time.deltaTime);
                            g.effectTimer += Time.deltaTime;
                            if (g.effectTimer >= 1.0f)
                            {
                                g.effectTimer = 0f;
                                DamageTextManager.Instance?.SpawnCustomText("💚 수액 치유 (+8)", PlayerController.Instance.transform.position + Vector3.up * 0.6f, new Color(0.3f, 1f, 0.6f));
                            }
                        }
                        break;

                    case "defibrillator":
                        // 6초마다 전기 충격 방출
                        g.effectTimer += Time.deltaTime;
                        if (g.effectTimer >= 6.0f)
                        {
                            g.effectTimer = 0f;
                            SoundManager.Instance?.Play(SoundEffect.Whirlwind, 0.6f);
                            Collider2D[] zapHits = Physics2D.OverlapCircleAll(g.position, 4.2f);
                            foreach (var hit in zapHits)
                            {
                                if (hit != null && hit.CompareTag("Enemy") && hit.TryGetComponent<Enemy>(out var e))
                                {
                                    e.TakeDamage(120f);
                                }
                            }
                            DamageTextManager.Instance?.SpawnCustomText("⚡ 제세동 펄스!", (Vector3)g.position + Vector3.up, new Color(1f, 0.9f, 0.3f));
                            PixelParticleManager.Instance?.SpawnNeonPop((Vector3)g.position, new Color(1f, 0.9f, 0.2f), 15);
                        }
                        break;
                }
            }
        }
        #endregion
    }
}
