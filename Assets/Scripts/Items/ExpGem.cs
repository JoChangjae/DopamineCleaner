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

        private SpriteRenderer spriteRenderer;
        private bool isAttracted = false;

        public GemType Type => gemType;
        public int ExpValue => expValue;

        private void Awake()
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
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
            if (PlayerController.Instance == null) return;

            Vector3 playerPos = PlayerController.Instance.transform.position;
            float distSq = (playerPos - transform.position).sqrMagnitude;
            float magRadius = PlayerController.Instance.MagnetRadius;

            // 자석 흡수 반경 내 진입 체크
            if (distSq < magRadius * magRadius)
            {
                isAttracted = true;
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
        }

        private void UpdateVisual()
        {
            if (spriteRenderer == null) return;

            switch (gemType)
            {
                case GemType.Green:
                    spriteRenderer.color = new Color(0.32f, 0.72f, 0.53f);
                    transform.localScale = Vector3.one * 0.45f;
                    break;
                case GemType.Blue:
                    spriteRenderer.color = new Color(0.31f, 0.66f, 0.87f);
                    transform.localScale = Vector3.one * 0.55f;
                    break;
                case GemType.Pink:
                    spriteRenderer.color = new Color(1.0f, 0.46f, 0.56f);
                    transform.localScale = Vector3.one * 0.65f;
                    break;
                case GemType.MegaPink:
                    spriteRenderer.color = new Color(1.0f, 0.25f, 0.45f);
                    transform.localScale = Vector3.one * 0.9f;
                    break;
                case GemType.Cake:
                    spriteRenderer.color = new Color(1.0f, 0.5f, 0.65f);
                    transform.localScale = Vector3.one * 0.75f;
                    break;
                case GemType.Bomb:
                    spriteRenderer.color = new Color(1.0f, 0.2f, 0.2f);
                    transform.localScale = Vector3.one * 0.8f;
                    break;
                case GemType.Magnet:
                    spriteRenderer.color = new Color(0.88f, 0.62f, 0.24f);
                    transform.localScale = Vector3.one * 0.75f;
                    break;
                case GemType.Chest:
                    spriteRenderer.color = new Color(1.0f, 0.82f, 0.4f);
                    transform.localScale = Vector3.one * 0.85f;
                    break;
            }
        }
    }
}
