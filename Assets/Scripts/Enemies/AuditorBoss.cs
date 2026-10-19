using System.Collections;
using UnityEngine;
using DopamineCleaner.Characters;
using DopamineCleaner.Core;

namespace DopamineCleaner.Enemies
{
    [RequireComponent(typeof(CircleCollider2D))]
    public class AuditorBoss : MonoBehaviour
    {
        [Header("김감사 부장 스탯")]
        [SerializeField] private float maxHP = 650f;
        [SerializeField] private float currentHP;
        [SerializeField] private float moveSpeed = 2.2f;
        [SerializeField] private float damage = 18f;

        private SpriteRenderer spriteRenderer;
        private TextMesh nameplateText;
        private float patternTimer = 3.0f;
        private int patternIndex = 0;
        private bool isActing = false;

        public float HP => currentHP;
        public float MaxHP => maxHP;
        public bool IsAlive => currentHP > 0f;

        private void Awake()
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            if (spriteRenderer == null) spriteRenderer = gameObject.AddComponent<SpriteRenderer>();

            SetupVisual();
            SetupCollider();
            SetupNameplate();
        }

        public void Initialize(int stage, float survivalSec)
        {
            float stageMult = 1.0f + (stage - 1) * 0.35f;
            maxHP = (650f + survivalSec * 1.2f) * stageMult;
            currentHP = maxHP;
            isActing = false;
            patternTimer = 2.5f;

            gameObject.SetActive(true);
        }

        private void SetupVisual()
        {
            Texture2D tex = CreateAuditorTex();
            spriteRenderer.sprite = Sprite.Create(tex, new Rect(0, 0, 36, 36), new Vector2(0.5f, 0.5f), 16);
            spriteRenderer.sortingOrder = 25;
            transform.localScale = Vector3.one * 1.8f;
        }

        private void SetupCollider()
        {
            CircleCollider2D col = GetComponent<CircleCollider2D>();
            col.radius = 0.55f;
            col.isTrigger = false;
        }

        private void SetupNameplate()
        {
            GameObject npObj = new GameObject("AuditorNameplate");
            npObj.transform.SetParent(transform, false);
            npObj.transform.localPosition = new Vector3(0f, 0.9f, 0f);

            nameplateText = npObj.AddComponent<TextMesh>();
            nameplateText.text = "💼 [본사 특별감사팀장] 김감사 부장";
            nameplateText.fontSize = 22;
            nameplateText.characterSize = 0.12f;
            nameplateText.anchor = TextAnchor.MiddleCenter;
            nameplateText.alignment = TextAlignment.Center;
            nameplateText.fontStyle = FontStyle.Bold;
            nameplateText.color = new Color(1f, 0.3f, 0.3f);

            var mr = npObj.GetComponent<MeshRenderer>();
            if (mr != null) mr.sortingOrder = 45;
        }

        private void Update()
        {
            if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing)
                return;

            if (currentHP <= 0f) return;

            // 플레이어 추적
            if (!isActing && PlayerController.Instance != null)
            {
                Vector3 playerPos = PlayerController.Instance.transform.position;
                Vector3 dir = (playerPos - transform.position).normalized;
                transform.position += dir * (moveSpeed * Time.deltaTime);

                if (dir.x != 0 && spriteRenderer != null)
                {
                    spriteRenderer.flipX = dir.x < 0;
                }
            }

            // 특수 패턴 타이머
            patternTimer -= Time.deltaTime;
            if (patternTimer <= 0f && !isActing)
            {
                ExecuteNextPattern();
                patternTimer = Random.Range(3.5f, 5.0f);
            }
        }

        private void ExecuteNextPattern()
        {
            patternIndex = (patternIndex + 1) % 3;
            switch (patternIndex)
            {
                case 0:
                    // 서류 뭉치 8방향 폭격
                    StartCoroutine(DocumentBarrageRoutine());
                    break;
                case 1:
                    // KPI 감사 광선
                    StartCoroutine(KPIAuditLaserRoutine());
                    break;
                case 2:
                    // 경위서 폭풍
                    StartCoroutine(ReportPaperStormRoutine());
                    break;
            }
        }

        private IEnumerator DocumentBarrageRoutine()
        {
            isActing = true;
            SoundManager.Instance?.Play(SoundEffect.Warning, 0.7f);

            yield return new WaitForSeconds(0.4f);

            int count = 10;
            for (int i = 0; i < count; i++)
            {
                float angle = i * (360f / count) * Mathf.Deg2Rad;
                Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

                GameObject projObj = new GameObject("AuditDocument");
                projObj.transform.position = transform.position;
                EnemyProjectile ep = projObj.AddComponent<EnemyProjectile>();
                ep.Initialize(dir, 5.0f, damage * 0.9f);
            }

            SoundManager.Instance?.Play(SoundEffect.Shoot, 0.6f);
            yield return new WaitForSeconds(0.3f);
            isActing = false;
        }

        private IEnumerator KPIAuditLaserRoutine()
        {
            isActing = true;
            if (PlayerController.Instance == null)
            {
                isActing = false;
                yield break;
            }

            Vector3 targetPos = PlayerController.Instance.transform.position;
            BossTelegraphManager.Instance?.SpawnLineTelegraph(transform.position, targetPos, 1.2f, 0.7f);
            SoundManager.Instance?.Play(SoundEffect.Laser, 0.65f);

            yield return new WaitForSeconds(0.7f);

            Vector2 dir = (targetPos - transform.position).normalized;
            GameObject projObj = new GameObject("KPILaserBullet");
            projObj.transform.position = transform.position;
            EnemyProjectile ep = projObj.AddComponent<EnemyProjectile>();
            ep.Initialize(dir, 9.0f, damage * 1.5f);

            yield return new WaitForSeconds(0.4f);
            isActing = false;
        }

        private IEnumerator ReportPaperStormRoutine()
        {
            isActing = true;
            SoundManager.Instance?.Play(SoundEffect.Fever, 0.6f);

            for (int wave = 0; wave < 3; wave++)
            {
                float baseAngle = wave * 25f * Mathf.Deg2Rad;
                for (int i = 0; i < 6; i++)
                {
                    float angle = baseAngle + i * (360f / 6) * Mathf.Deg2Rad;
                    Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

                    GameObject projObj = new GameObject("PaperStorm");
                    projObj.transform.position = transform.position;
                    EnemyProjectile ep = projObj.AddComponent<EnemyProjectile>();
                    ep.Initialize(dir, 4.2f, damage * 0.75f);
                }
                yield return new WaitForSeconds(0.35f);
            }

            isActing = false;
        }

        public void TakeDamage(float dmg)
        {
            if (currentHP <= 0f) return;

            currentHP -= dmg;
            StartCoroutine(FlashWhiteRoutine());

            DamageTextManager.Instance?.SpawnDamageText(dmg, transform.position + Vector3.up * 0.5f, false);

            if (currentHP <= 0f)
            {
                Die();
            }
        }

        private IEnumerator FlashWhiteRoutine()
        {
            if (spriteRenderer != null)
            {
                Color prev = spriteRenderer.color;
                spriteRenderer.color = Color.white;
                yield return new WaitForSeconds(0.08f);
                if (spriteRenderer != null) spriteRenderer.color = prev;
            }
        }

        private void Die()
        {
            currentHP = 0f;
            SoundManager.Instance?.Play(SoundEffect.Explosion, 0.9f);

            // 황금 퇴직금 상자 필드 드랍
            SpawnGoldenSeveranceBox(transform.position);

            // 대형 경험치 젬 드랍
            if (Items.ItemManager.Instance != null)
            {
                for (int i = 0; i < 5; i++)
                {
                    Items.ItemManager.Instance.SpawnGem(transform.position + (Vector3)(Random.insideUnitCircle * 1.5f), Items.GemType.Pink, 15);
                }
            }

            if (HRAuditManager.Instance != null)
            {
                HRAuditManager.Instance.OnAuditorDefeated();
            }

            gameObject.SetActive(false);
            Destroy(gameObject, 0.1f);
        }

        private void SpawnGoldenSeveranceBox(Vector3 pos)
        {
            GameObject boxObj = new GameObject("GoldenSeveranceBox");
            boxObj.transform.position = pos;

            SpriteRenderer sr = boxObj.AddComponent<SpriteRenderer>();
            Texture2D tex = CreateBoxTex();
            sr.sprite = Sprite.Create(tex, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 16);
            sr.sortingOrder = 22;
            boxObj.transform.localScale = Vector3.one * 1.6f;

            CircleCollider2D col = boxObj.AddComponent<CircleCollider2D>();
            col.radius = 1.2f;
            col.isTrigger = true;

            boxObj.AddComponent<SeveranceBoxTrigger>();
        }

        private Texture2D CreateAuditorTex()
        {
            Texture2D tex = new Texture2D(36, 36);
            tex.filterMode = FilterMode.Point;
            Color suitColor = new Color(0.12f, 0.15f, 0.25f);
            Color shirtWhite = new Color(0.95f, 0.95f, 0.95f);
            Color redTie = new Color(0.9f, 0.15f, 0.15f);
            Color hairBlack = new Color(0.1f, 0.1f, 0.15f);

            for (int y = 0; y < 36; y++)
            {
                for (int x = 0; x < 36; x++)
                {
                    if (x >= 8 && x <= 28 && y >= 6 && y <= 30)
                    {
                        if (y >= 26) tex.SetPixel(x, y, hairBlack); // 머리
                        else if (x >= 16 && x <= 20 && y >= 14 && y <= 24) tex.SetPixel(x, y, redTie); // 붉은 넥타이
                        else if (x >= 14 && x <= 22 && y >= 14 && y <= 25) tex.SetPixel(x, y, shirtWhite); // 와이셔츠
                        else tex.SetPixel(x, y, suitColor); // 양복 수트
                    }
                    else
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                }
            }
            tex.Apply();
            return tex;
        }

        private Texture2D CreateBoxTex()
        {
            Texture2D tex = new Texture2D(32, 32);
            tex.filterMode = FilterMode.Point;
            Color gold = new Color(1f, 0.85f, 0.2f);
            Color darkGold = new Color(0.7f, 0.55f, 0.1f);
            Color ribbon = new Color(0.95f, 0.2f, 0.25f);

            for (int y = 0; y < 32; y++)
            {
                for (int x = 0; x < 32; x++)
                {
                    if (x >= 6 && x <= 26 && y >= 6 && y <= 26)
                    {
                        if (x == 15 || x == 16 || y == 15 || y == 16) tex.SetPixel(x, y, ribbon);
                        else if (x == 6 || x == 26 || y == 6 || y == 26) tex.SetPixel(x, y, darkGold);
                        else tex.SetPixel(x, y, gold);
                    }
                    else
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                }
            }
            tex.Apply();
            return tex;
        }
    }

    public class SeveranceBoxTrigger : MonoBehaviour
    {
        private bool collected = false;

        public void OnTriggerEnter2D(Collider2D collision)
        {
            if (collected) return;
            if (collision != null && (collision.CompareTag("Player") || collision.GetComponent<PlayerController>() != null || collision.GetComponentInParent<PlayerController>() != null))
            {
                Collect();
            }
        }

        public void Collect()
        {
            if (collected) return;
            collected = true;

            // 1. 위로금 +500 Gold
            GameManager.Instance?.AddBonusGold(500);

            // 2. 체력 100% 완전 회복
            PlayerController.Instance?.Heal(9999f);

            // 3. 감사 통과 확인서 (+30% All Weapon Damage 영구 버프)
            if (PlayerController.Instance != null)
            {
                PlayerController.Instance.TotalDamageMultiplier *= 1.30f;
            }

            SoundManager.Instance?.Play(SoundEffect.Fever, 1.0f);
            SoundManager.Instance?.Play(SoundEffect.LevelUp, 0.9f);

            try
            {
                if (DamageTextManager.Instance != null && DamageTextManager.Instance.gameObject != null)
                {
                    DamageTextManager.Instance.SpawnDamageText(500f, transform.position + Vector3.up * 1f, true);
                }
            }
            catch { }

            if (Application.isPlaying) Destroy(gameObject);
            else DestroyImmediate(gameObject);
        }
    }
}
