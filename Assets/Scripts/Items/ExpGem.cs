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

        [Header("아이템 스프라이트")]
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

        public GemType Type => gemType;
        public int ExpValue => expValue;

        private void Awake()
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            bobOffset = Random.Range(0f, 100f);
        }

        public void Initialize(GemType type, int value)
        {
            gemType = type;
            expValue = value;
            isAttracted = false;

            UpdateVisual();
            gameObject.SetActive(true);
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
            if (spriteRenderer == null) return;

            spriteRenderer.color = Color.white;

            switch (gemType)
            {
                case GemType.Green:
                    if (spriteGreen != null) spriteRenderer.sprite = spriteGreen;
                    transform.localScale = Vector3.one * 1.0f;
                    break;
                case GemType.Blue:
                    if (spriteBlue != null) spriteRenderer.sprite = spriteBlue;
                    transform.localScale = Vector3.one * 1.1f;
                    break;
                case GemType.Pink:
                    if (spritePink != null) spriteRenderer.sprite = spritePink;
                    transform.localScale = Vector3.one * 1.2f;
                    break;
                case GemType.MegaPink:
                    if (spriteMega != null) spriteRenderer.sprite = spriteMega;
                    transform.localScale = Vector3.one * 1.35f;
                    break;
                case GemType.Cake:
                    if (spriteCake != null) spriteRenderer.sprite = spriteCake;
                    transform.localScale = Vector3.one * 1.25f;
                    break;
                case GemType.Bomb:
                    if (spriteBomb != null) spriteRenderer.sprite = spriteBomb;
                    transform.localScale = Vector3.one * 1.25f;
                    break;
                case GemType.Magnet:
                    if (spriteMagnet != null) spriteRenderer.sprite = spriteMagnet;
                    transform.localScale = Vector3.one * 1.25f;
                    break;
                case GemType.Chest:
                    if (spriteChest != null) spriteRenderer.sprite = spriteChest;
                    transform.localScale = Vector3.one * 1.35f;
                    break;
            }
        }
    }
}
