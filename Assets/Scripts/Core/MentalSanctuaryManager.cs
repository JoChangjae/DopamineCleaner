using System;
using UnityEngine;
using UnityEngine.UI;
using DopamineCleaner.Characters;

namespace DopamineCleaner.Core
{
    public class MentalSanctuaryManager : MonoBehaviour
    {
        public static MentalSanctuaryManager Instance { get; private set; }

        [Header("도파민 과부하 (Brain Fog)")]
        [SerializeField] private float overloadPercent = 0f;
        [SerializeField] private float overloadStartSec = 130f;
        [SerializeField] private float overloadAccumRate = 1.2f; // 초당 증가량
        [SerializeField] private Image overloadVignette;

        [Header("멘탈 쉼터 (Sanctuary Zone)")]
        [SerializeField] private GameObject sanctuaryObj;
        [SerializeField] private float sanctuaryRadius = 2.4f;
        [SerializeField] private float sanctuaryDuration = 14f;
        [SerializeField] private float sanctuaryRespawnDelay = 45f;
        private float sanctuaryTimer = 0f;
        private float sanctuaryStayTimer = 0f;
        private bool isSanctuaryActive = false;

        public float OverloadPercent => overloadPercent;
        public bool IsInSanctuary { get; private set; } = false;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            overloadPercent = 0f;
            sanctuaryTimer = 25f; // 첫 쉼터는 25초 후 스폰
            CreateSanctuaryObject();
        }

        private void Update()
        {
            if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing)
                return;

            float survivalTime = GameManager.Instance.SurvivalTime;

            // 1. 피버 모드 중이면 과부하 즉시 완전 정화
            if (FeverManager.Instance != null && FeverManager.Instance.IsFever)
            {
                overloadPercent = Mathf.Max(0f, overloadPercent - Time.deltaTime * 35f);
            }
            else if (IsInSanctuary)
            {
                // 쉼터 안에서는 급속 정화
                overloadPercent = Mathf.Max(0f, overloadPercent - Time.deltaTime * 40f);
            }
            else if (survivalTime >= overloadStartSec)
            {
                // 130초 이후 점진적 과부하 누적
                overloadPercent = Mathf.Min(100f, overloadPercent + Time.deltaTime * overloadAccumRate);
            }

            // 비네트 알파값 맥동 연출
            if (overloadVignette != null)
            {
                float baseAlpha = (overloadPercent / 100f) * 0.38f;
                float pulse = baseAlpha > 0.05f ? Mathf.PingPong(Time.time * 2.5f, 0.08f) : 0f;
                Color col = overloadVignette.color;
                col.a = baseAlpha + pulse;
                overloadVignette.color = col;
            }

            // 2. 멘탈 쉼터 스폰 및 라이프사이클 관리
            UpdateSanctuaryLifecycle();
        }

        public void BindVignette(Image vignette)
        {
            overloadVignette = vignette;
            if (overloadVignette != null)
            {
                Color c = overloadVignette.color;
                c.a = 0f;
                overloadVignette.color = c;
            }
        }

        private void CreateSanctuaryObject()
        {
            sanctuaryObj = new GameObject("MentalSanctuaryZone");
            sanctuaryObj.transform.position = new Vector3(9999f, 9999f, 0f);

            var sr = sanctuaryObj.AddComponent<SpriteRenderer>();
            sr.sprite = CreateSanctuarySprite();
            sr.color = new Color(0.35f, 0.95f, 0.75f, 0.45f);
            sr.sortingOrder = 1;

            var col = sanctuaryObj.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = sanctuaryRadius;

            sanctuaryObj.SetActive(false);
        }

        private void UpdateSanctuaryLifecycle()
        {
            if (PlayerController.Instance == null || sanctuaryObj == null) return;

            if (!isSanctuaryActive)
            {
                sanctuaryTimer -= Time.deltaTime;
                if (sanctuaryTimer <= 0f)
                {
                    SpawnSanctuary();
                }
            }
            else
            {
                // 쉼터 내 플레이어 거리 판정
                float dist = Vector2.Distance(PlayerController.Instance.transform.position, sanctuaryObj.transform.position);
                if (dist <= sanctuaryRadius)
                {
                    IsInSanctuary = true;
                    // 초당 체력 회복
                    PlayerController.Instance.Heal(12f * Time.deltaTime);

                    // 쉼터 체류 시간 누적
                    sanctuaryStayTimer += Time.deltaTime;
                    if (sanctuaryStayTimer >= sanctuaryDuration)
                    {
                        DespawnSanctuary();
                    }
                }
                else
                {
                    IsInSanctuary = false;
                }
            }
        }

        private void SpawnSanctuary()
        {
            if (PlayerController.Instance == null) return;

            isSanctuaryActive = true;
            sanctuaryStayTimer = 0f;

            // 플레이어 주변 4~8 유닛 거리 무작위 위치
            Vector2 offset = UnityEngine.Random.insideUnitCircle.normalized * UnityEngine.Random.Range(4.5f, 8.0f);
            Vector3 pos = PlayerController.Instance.transform.position + new Vector3(offset.x, offset.y, 0f);

            sanctuaryObj.transform.position = pos;
            sanctuaryObj.SetActive(true);

            SoundManager.Instance?.Play(SoundEffect.GemCollect, 0.6f);
        }

        private void DespawnSanctuary()
        {
            isSanctuaryActive = false;
            IsInSanctuary = false;
            sanctuaryTimer = sanctuaryRespawnDelay;
            if (sanctuaryObj != null) sanctuaryObj.SetActive(false);
        }

        private Sprite CreateSanctuarySprite()
        {
            int size = 128;
            Texture2D tex = new Texture2D(size, size);
            Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
            float outerR = size * 0.48f;
            float innerR = size * 0.40f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center);
                    if (dist <= outerR)
                    {
                        float edgeAlpha = Mathf.Clamp01((outerR - dist) / 6f);
                        float fillAlpha = dist <= innerR ? 0.25f : 0.85f;
                        tex.SetPixel(x, y, new Color(1f, 1f, 1f, edgeAlpha * fillAlpha));
                    }
                    else
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 24f);
        }
    }
}
