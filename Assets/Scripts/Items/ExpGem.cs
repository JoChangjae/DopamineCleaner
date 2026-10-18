using System.Collections.Generic;
using UnityEngine;
using DopamineCleaner.Characters;

namespace DopamineCleaner.Items
{
    public enum GemType
    {
        Green,   // 1~3 EXP
        Blue,    // 4~9 EXP
        Pink,    // 10~29 EXP
        MegaPink,// 30+ EXP
        Cake,    // HP 회복
        Bomb,    // 화면 일망타진 폭탄
        Magnet,  // 전역 자석
        Chest    // 황금 보물상자
    }

    [RequireComponent(typeof(Collider2D))]
    public class ExpGem : MonoBehaviour
    {
        [Header("보석 속성")]
        [SerializeField] private GemType gemType = GemType.Green;
        [SerializeField] private int expValue = 1;
        [SerializeField] private float magnetSpeed = 9f;

        [Header("아이템 스프라이트 (인스펙터 할당 또는 자동 로드)")]
        [SerializeField] private Sprite spriteGreen;
        [SerializeField] private Sprite spriteBlue;
        [SerializeField] private Sprite spritePink;
        [SerializeField] private Sprite spriteMega;
        [SerializeField] private Sprite spriteCake;
        [SerializeField] private Sprite spriteBomb;
        [SerializeField] private Sprite spriteMagnet;
        [SerializeField] private Sprite spriteChest;

        private SpriteRenderer spriteRenderer;
        private bool isAttracted = false;
        private float bobOffset;

        private static readonly Dictionary<GemType, Sprite> RuntimeSpriteCache = new();

        public GemType Type => gemType;
        public int ExpValue => expValue;

        private void Awake()
        {
            EnsureComponents();
            bobOffset = Random.Range(0f, 100f);
        }

        private void EnsureComponents()
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }
            if (spriteRenderer == null)
            {
                spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            }

            spriteRenderer.sortingOrder = 5;
            spriteRenderer.color = Color.white;
        }

        public void Initialize(GemType type, int value)
        {
            EnsureComponents();
            gemType = type;
            expValue = value;
            isAttracted = false;

            gameObject.SetActive(true);
            UpdateVisual();
        }

        public void AddExp(int amount)
        {
            expValue += amount;
            if (expValue >= 30)
                gemType = GemType.MegaPink;
            else if (expValue >= 10)
                gemType = GemType.Pink;
            else if (expValue >= 4)
                gemType = GemType.Blue;

            UpdateVisual();
        }

        private void Update()
        {
            // 바닥에 정지해 있을 때 부드러운 둥둥 떠있는 픽셀 애니메이션
            if (!isAttracted && spriteRenderer != null)
            {
                float bobY = Mathf.Sin((Time.time * 4f) + bobOffset) * 0.08f;
                spriteRenderer.transform.localPosition = new Vector3(0, bobY, 0);
            }

            if (PlayerController.Instance == null) return;

            Vector3 playerPos = PlayerController.Instance.transform.position;
            float distSq = (playerPos - transform.position).sqrMagnitude;
            float magRadius = PlayerController.Instance.MagnetRadius;

            // 자석 흡수 반경 내 진입 체크
            if (distSq < magRadius * magRadius)
            {
                isAttracted = true;
                if (spriteRenderer != null)
                    spriteRenderer.transform.localPosition = Vector3.zero;
            }

            if (isAttracted)
            {
                transform.position = Vector3.MoveTowards(transform.position, playerPos, magnetSpeed * Time.deltaTime);

                // 획득 판정 (0.5 유닛 이내)
                if (distSq < 0.25f)
                {
                    Collect();
                }
            }
        }

        private void Collect()
        {
            switch (gemType)
            {
                case GemType.Cake:
                    PlayerController.Instance?.Heal(30f);
                    break;
                case GemType.Bomb:
                    ItemManager.Instance?.TriggerScreenBomb();
                    break;
                case GemType.Magnet:
                    ItemManager.Instance?.AttractAllGems();
                    break;
                case GemType.Chest:
                    ItemManager.Instance?.OpenTreasureChest();
                    break;
                default:
                    PlayerController.Instance?.AddExp(expValue);
                    break;
            }

            gameObject.SetActive(false);
        }

        public void ForceAttract()
        {
            isAttracted = true;
            if (spriteRenderer != null)
                spriteRenderer.transform.localPosition = Vector3.zero;
        }

        public void SetSprites(Sprite green, Sprite blue, Sprite pink, Sprite mega, Sprite cake, Sprite bomb, Sprite magnet, Sprite chest)
        {
            spriteGreen = green;
            spriteBlue = blue;
            spritePink = pink;
            spriteMega = mega;
            spriteCake = cake;
            spriteBomb = bomb;
            spriteMagnet = magnet;
            spriteChest = chest;
        }

        private void UpdateVisual()
        {
            EnsureComponents();

            Sprite sprite = GetSprite(gemType);
            if (sprite != null)
            {
                spriteRenderer.sprite = sprite;
            }

            spriteRenderer.color = Color.white;

            switch (gemType)
            {
                case GemType.Green:
                    transform.localScale = Vector3.one * 1.0f;
                    break;
                case GemType.Blue:
                    transform.localScale = Vector3.one * 1.1f;
                    break;
                case GemType.Pink:
                    transform.localScale = Vector3.one * 1.2f;
                    break;
                case GemType.MegaPink:
                    transform.localScale = Vector3.one * 1.35f;
                    break;
                case GemType.Cake:
                    transform.localScale = Vector3.one * 1.3f;
                    break;
                case GemType.Bomb:
                    transform.localScale = Vector3.one * 1.3f;
                    break;
                case GemType.Magnet:
                    transform.localScale = Vector3.one * 1.3f;
                    break;
                case GemType.Chest:
                    transform.localScale = Vector3.one * 1.4f;
                    break;
            }
        }

        private Sprite GetSprite(GemType type)
        {
            // 1. 인스펙터 직렬화 필드 확인
            Sprite s = type switch
            {
                GemType.Green => spriteGreen,
                GemType.Blue => spriteBlue,
                GemType.Pink => spritePink,
                GemType.MegaPink => spriteMega,
                GemType.Cake => spriteCake,
                GemType.Bomb => spriteBomb,
                GemType.Magnet => spriteMagnet,
                GemType.Chest => spriteChest,
                _ => null
            };

            if (s != null) return s;

            // 2. 런타임 캐시 확인
            if (RuntimeSpriteCache.TryGetValue(type, out var cached) && cached != null)
            {
                return cached;
            }

            // 3. Resources 폴더 로드 시도
            string resName = type switch
            {
                GemType.Green => "Items/gem_green",
                GemType.Blue => "Items/gem_blue",
                GemType.Pink => "Items/gem_pink",
                GemType.MegaPink => "Items/gem_mega",
                GemType.Cake => "Items/item_cake",
                GemType.Bomb => "Items/item_bomb",
                GemType.Magnet => "Items/item_magnet",
                GemType.Chest => "Items/item_chest",
                _ => "Items/gem_green"
            };

            s = Resources.Load<Sprite>(resName);
            if (s != null)
            {
                RuntimeSpriteCache[type] = s;
                return s;
            }

            // 4. 비상용 프로시저럴 픽셀 스프라이트 생성 (절대 투명해지지 않는 100% 안전장치)
            s = GenerateFallbackSprite(type);
            RuntimeSpriteCache[type] = s;
            return s;
        }

        private static Sprite GenerateFallbackSprite(GemType type)
        {
            int size = 24;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };

            Color clear = new Color(0, 0, 0, 0);
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                    tex.SetPixel(x, y, clear);

            Color primary = type switch
            {
                GemType.Green => new Color(0.28f, 0.85f, 0.55f),
                GemType.Blue => new Color(0.25f, 0.65f, 0.95f),
                GemType.Pink => new Color(1.0f, 0.42f, 0.62f),
                GemType.MegaPink => new Color(0.95f, 0.15f, 0.45f),
                GemType.Cake => new Color(1.0f, 0.85f, 0.65f),
                GemType.Bomb => new Color(0.95f, 0.22f, 0.22f),
                GemType.Magnet => new Color(0.92f, 0.25f, 0.25f),
                GemType.Chest => new Color(1.0f, 0.78f, 0.15f),
                _ => Color.white
            };

            Color accent = type switch
            {
                GemType.Cake => new Color(0.95f, 0.2f, 0.35f), // 딸기
                GemType.Magnet => new Color(0.25f, 0.55f, 0.95f), // 자석 파랑
                GemType.Chest => new Color(0.45f, 0.28f, 0.12f), // 상자 테두리
                _ => Color.white
            };

            int cx = size / 2;
            int cy = size / 2;
            int r = size / 3;

            for (int y = cy - r; y <= cy + r; y++)
            {
                for (int x = cx - r; x <= cx + r; x++)
                {
                    if (Mathf.Abs(x - cx) + Mathf.Abs(y - cy) <= r + 1)
                    {
                        tex.SetPixel(x, y, primary);
                    }
                }
            }

            // 하이라이트/악센트 점
            tex.SetPixel(cx, cy, accent);
            tex.SetPixel(cx - 1, cy + 1, Color.white);
            tex.Apply();

            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 24f);
        }
    }
}
