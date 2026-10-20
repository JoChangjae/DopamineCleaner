using System.Collections;
using UnityEngine;
using DopamineCleaner.Core;
using DopamineCleaner.Characters;

namespace DopamineCleaner.Items
{
    [RequireComponent(typeof(CircleCollider2D))]
    public class GoldenChest : MonoBehaviour
    {
        private SpriteRenderer spriteRenderer;
        private CircleCollider2D circleCollider;
        private bool isCollected = false;
        private float floatTimer = 0f;
        private Vector3 startPos;

                private void Reset()
        {
            circleCollider = GetComponent<CircleCollider2D>();
            if (circleCollider != null)
            {
                circleCollider.radius = 0.8f;
                circleCollider.isTrigger = true;
            }
        }

        private void Awake()
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            if (spriteRenderer == null) spriteRenderer = gameObject.AddComponent<SpriteRenderer>();

            circleCollider = GetComponent<CircleCollider2D>();
            circleCollider.radius = 0.8f;
            circleCollider.isTrigger = true;

            SetupVisual();
        }

        private void Start()
        {
            startPos = transform.position;
            StartCoroutine(SpawnBounceRoutine());
        }

        private void SetupVisual()
        {
            Texture2D tex = CreateChestTexture();
            spriteRenderer.sprite = Sprite.Create(tex, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 16);
            spriteRenderer.sortingOrder = 22;
            transform.localScale = Vector3.one * 1.5f;
        }

        private void Update()
        {
            if (isCollected) return;

            // 황금 상자 공중 부유 및 회전/반짝임 연출
            floatTimer += Time.deltaTime * 3.5f;
            transform.position = startPos + new Vector3(0f, Mathf.Sin(floatTimer) * 0.15f, 0f);

            // 0.4초마다 황금 픽셀 스파클 방출
            if (Random.value < 0.12f)
            {
                PixelParticleManager.Instance?.SpawnDeathPop(transform.position + (Vector3)(Random.insideUnitCircle * 0.4f), new Color(1f, 0.85f, 0.2f), 2);
            }

            // 플레이어 근접 자동 흡수
            if (PlayerController.Instance != null)
            {
                float dist = Vector2.Distance(transform.position, PlayerController.Instance.transform.position);
                if (dist <= 1.8f)
                {
                    CollectChest();
                }
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (isCollected) return;
            if (collision.CompareTag("Player") || collision.GetComponent<PlayerController>() != null)
            {
                CollectChest();
            }
        }

        private void CollectChest()
        {
            if (isCollected) return;
            isCollected = true;

            // 1. 극적 팡파레 효과음 및 파티클
            SoundManager.Instance?.Play(SoundEffect.Fever, 1.0f);
            SoundManager.Instance?.Play(SoundEffect.LevelUp, 0.9f);
            PixelParticleManager.Instance?.SpawnDeathPop(transform.position, new Color(1f, 0.85f, 0.2f), 35);
            PixelParticleManager.Instance?.SpawnDeathPop(transform.position, new Color(0.3f, 0.9f, 1f), 20);

            // 2. 화면 데미지/획득 텍스트
            DamageTextManager.Instance?.SpawnCustomText("👑 [전설 도파민 황금 상자 습득!] 👑", transform.position + Vector3.up * 1.2f, new Color(1f, 0.85f, 0.2f));

            // 3. 골드 대량 지급 (+500G)
            if (Meta.GameDataManager.Instance != null)
            {
                Meta.GameDataManager.Instance.AddGold(500);
            }

            // 4. 피버 게이지 100% 즉시 완충
            FeverManager.Instance?.AddFever(100f);

            // 5. 즉시 레벨업 3연속 폭풍 성장 발동!
            if (PlayerController.Instance != null)
            {
                // 플레이어의 다음 레벨 필요 경험치의 3배를 주입하여 3연속 레벨업 유도
                float expNeeded = PlayerController.Instance.NextLevelExp;
                PlayerController.Instance.AddExp(expNeeded * 3.2f);
            }

            Destroy(gameObject);
        }

        private IEnumerator SpawnBounceRoutine()
        {
            Vector3 origin = transform.position;
            float duration = 0.5f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float yOffset = Mathf.Sin(t * Mathf.PI) * 1.4f;
                transform.position = origin + new Vector3(0f, yOffset, 0f);
                yield return null;
            }

            startPos = origin;
            transform.position = origin;
            PixelParticleManager.Instance?.SpawnDeathPop(transform.position, new Color(1f, 0.85f, 0.2f), 15);
        }

        private Texture2D CreateChestTexture()
        {
            Texture2D tex = new Texture2D(32, 32, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            Color clear = new Color(0, 0, 0, 0);
            Color goldBase = new Color(0.95f, 0.75f, 0.15f);
            Color goldShine = new Color(1f, 0.95f, 0.5f);
            Color goldDark = new Color(0.65f, 0.45f, 0.05f);
            Color ironTrim = new Color(0.25f, 0.22f, 0.35f);
            Color lockRuby = new Color(1f, 0.15f, 0.35f);

            for (int y = 0; y < 32; y++)
            {
                for (int x = 0; x < 32; x++)
                {
                    tex.SetPixel(x, y, clear);
                }
            }

            // 상자 본체 (8~24 x 6~22)
            for (int y = 6; y <= 22; y++)
            {
                for (int x = 6; x <= 25; x++)
                {
                    bool isBorder = (x == 6 || x == 25 || y == 6 || y == 22 || y == 15);
                    if (isBorder)
                    {
                        tex.SetPixel(x, y, ironTrim);
                    }
                    else if (y > 15)
                    {
                        // 뚜껑
                        tex.SetPixel(x, y, y == 21 ? goldShine : goldBase);
                    }
                    else
                    {
                        // 몸통
                        tex.SetPixel(x, y, x % 4 == 0 ? goldDark : goldBase);
                    }
                }
            }

            // 루비 보석 자물쇠 (중앙)
            for (int y = 13; y <= 16; y++)
            {
                for (int x = 14; x <= 17; x++)
                {
                    tex.SetPixel(x, y, lockRuby);
                }
            }

            tex.Apply();
            return tex;
        }
    }
}
