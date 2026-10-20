using System.Collections;
using UnityEngine;
using DopamineCleaner.Core;
using DopamineCleaner.Characters;
using DopamineCleaner.Items;

namespace DopamineCleaner.Enemies
{
    public enum EnemyType
    {
        MindDust,          // 잡생각 먼지 (약하지만 떼지어 몰려옴)
        HeavySloth,        // 쏟아지는 졸음 (탱커형)
        NotificationGhost, // 알림 유령 (스마트폰 알림, 고속 이동)
        ShortsRunner,      // 숏폼 알고리즘 (초고속 지그재그 돌진)
        SpamShooter,       // 스팸/악플러 (원거리 투사체 사격)
        DoomScroller,      // 둠스크롤러 (심연 중력 골렘)
        DopamineBoss,      // 도파민 거대 보스
        AuditorKim         // 불시 특별 감사 김감사 부장
    }

    [RequireComponent(typeof(CircleCollider2D))]
    public class Enemy : MonoBehaviour
    {
        [Header("몬스터 스탯")]
        [SerializeField] private EnemyType enemyType = EnemyType.MindDust;
        [SerializeField] private EliteAffix eliteAffix = EliteAffix.None;
        [SerializeField] private float maxHP = 15f;
        [SerializeField] private float currentHP;
        [SerializeField] private float moveSpeed = 2.5f;
        [SerializeField] private float damage = 5f;
        [SerializeField] private int baseExp = 1;

        [Header("엘리트 상태")]
        private int shieldCount = 0;
        private int stolenGemsCount = 0;
        private float affixActionTimer = 0f;

        [Header("컴포넌트")]
        private SpriteRenderer spriteRenderer;
        private Color originalColor;
        private Coroutine flashCoroutine;
        private float specialActionTimer = 0f;
        private float bossPatternTimer = 4.0f;
        private bool isBossSpecialActing = false;
        private bool isBossBerserk = false;
        private int bossPatternIndex = 0;
        private TextMesh nameplateText;

        public EnemyType Type => enemyType;
        public EliteAffix Affix => eliteAffix;
        public bool IsElite => eliteAffix != EliteAffix.None;
        public float HP => currentHP;
        public float MaxHP => maxHP;
        public float Damage => damage;
        public bool IsAlive => currentHP > 0f;

        private void Awake()
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            if (spriteRenderer != null)
                originalColor = spriteRenderer.color;
        }

        private void EnsureNameplate()
        {
            if (nameplateText == null)
            {
                GameObject npObj = new GameObject("EliteNameplate");
                npObj.transform.SetParent(transform, false);
                npObj.transform.localPosition = new Vector3(0f, 0.75f, 0f);
                nameplateText = npObj.AddComponent<TextMesh>();
                nameplateText.fontSize = 20;
                nameplateText.characterSize = 0.12f;
                nameplateText.anchor = TextAnchor.MiddleCenter;
                nameplateText.alignment = TextAlignment.Center;
                nameplateText.fontStyle = FontStyle.Bold;
                var mr = npObj.GetComponent<MeshRenderer>();
                if (mr != null) mr.sortingOrder = 45;
            }
        }

        public void UpdateNameplate()
        {
            if (!IsElite)
            {
                if (nameplateText != null) nameplateText.gameObject.SetActive(false);
                return;
            }

            EnsureNameplate();
            nameplateText.gameObject.SetActive(true);

            string title = eliteAffix.GetDisplayName();
            if (eliteAffix == EliteAffix.FirewallShield)
            {
                title = shieldCount > 0 ? $"🛡️ [방화벽: {shieldCount}]" : "💥 [실드 파괴!]";
                nameplateText.color = shieldCount > 0 ? new Color(0.2f, 0.95f, 1f) : Color.gray;
            }
            else if (eliteAffix == EliteAffix.GemThief)
            {
                title = $"🧲 [도둑: +{stolenGemsCount}]";
                nameplateText.color = new Color(0.95f, 0.4f, 1f);
            }
            else if (eliteAffix == EliteAffix.HasteSprinter)
            {
                nameplateText.color = new Color(1f, 0.95f, 0.2f);
            }
            else if (eliteAffix == EliteAffix.SpamReflector)
            {
                nameplateText.color = new Color(1f, 0.35f, 0.35f);
            }

            nameplateText.text = title;
        }

        public void Initialize(EnemyType type, int stage, float survivalSec, EliteAffix affix = EliteAffix.None)
        {
            enemyType = type;
            eliteAffix = affix;
            shieldCount = 0;
            stolenGemsCount = 0;
            affixActionTimer = 0.5f;
            specialActionTimer = Random.Range(1.0f, 2.5f);
            isBossBerserk = false;
            if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            float stageMult = 1.0f + (stage - 1) * 0.32f;

            switch (type)
            {
                case EnemyType.MindDust:
                    maxHP = (15f + survivalSec * 0.22f) * stageMult;
                    moveSpeed = Random.Range(2.4f, 3.0f);
                    damage = 7f + (stage - 1) * 2.5f;
                    baseExp = 1;
                    SetVisualColor(new Color(0.85f, 0.78f, 0.92f)); // 연보라
                    transform.localScale = Vector3.one * 0.9f;
                    break;

                case EnemyType.HeavySloth:
                    maxHP = (36f + survivalSec * 0.38f) * stageMult;
                    moveSpeed = Random.Range(1.6f, 2.0f);
                    damage = 13f + (stage - 1) * 3.5f;
                    baseExp = 2;
                    SetVisualColor(new Color(0.75f, 0.88f, 0.92f)); // 포근한 하늘빛
                    transform.localScale = Vector3.one * 1.3f;
                    break;

                case EnemyType.NotificationGhost:
                    maxHP = (52f + survivalSec * 0.50f) * stageMult;
                    moveSpeed = Random.Range(3.4f, 4.2f);
                    damage = 16f + (stage - 1) * 4.5f;
                    baseExp = 3;
                    SetVisualColor(new Color(1.0f, 0.76f, 0.82f)); // 핑크
                    transform.localScale = Vector3.one * 1.1f;
                    break;

                case EnemyType.ShortsRunner:
                    maxHP = (42f + survivalSec * 0.40f) * stageMult;
                    moveSpeed = Random.Range(4.5f, 5.5f);
                    damage = 14f + (stage - 1) * 3.5f;
                    baseExp = 2;
                    SetVisualColor(new Color(1.0f, 0.45f, 0.2f)); // 번갯빛 주황
                    transform.localScale = Vector3.one * 0.85f;
                    break;

                case EnemyType.SpamShooter:
                    maxHP = (38f + survivalSec * 0.35f) * stageMult;
                    moveSpeed = 2.0f;
                    damage = 13f + (stage - 1) * 3.5f;
                    baseExp = 3;
                    SetVisualColor(new Color(0.95f, 0.25f, 0.35f)); // 핏빛 빨강
                    transform.localScale = Vector3.one * 1.0f;
                    break;

                case EnemyType.DoomScroller:
                    maxHP = (110f + survivalSec * 0.85f) * stageMult;
                    moveSpeed = 1.35f;
                    damage = 22f + (stage - 1) * 5f;
                    baseExp = 6;
                    SetVisualColor(new Color(0.7f, 0.45f, 0.95f)); // 심연의 암자색
                    transform.localScale = Vector3.one * 1.55f;
                    break;

                case EnemyType.DopamineBoss:
                    maxHP = (480f + (stage - 1) * 320f) * stageMult;
                    moveSpeed = 1.8f + Mathf.Min(0.6f, (stage - 1) * 0.12f);
                    damage = 26f + (stage - 1) * 7f;
                    baseExp = 35;
                    SetVisualColor(GetBossStageColor(stage));
                    transform.localScale = Vector3.one * 2.8f;
                    break;

                case EnemyType.AuditorKim:
                    maxHP = (550f + (stage - 1) * 200f + survivalSec * 0.8f) * stageMult;
                    moveSpeed = 2.2f;
                    damage = 18f + (stage - 1) * 4f;
                    baseExp = 45;
                    SetVisualColor(new Color(0.95f, 0.25f, 0.25f)); // 특별 감사 붉은 정장
                    transform.localScale = Vector3.one * 1.8f;
                    EnsureNameplate();
                    nameplateText.gameObject.SetActive(true);
                    nameplateText.text = "💼 [본사 감사팀장] 김감사 부장";
                    nameplateText.color = new Color(1f, 0.3f, 0.3f);
                    break;
            }

            string spriteName = type switch
            {
                EnemyType.HeavySloth => "enemy_sloth",
                EnemyType.NotificationGhost => "enemy_ghost",
                EnemyType.ShortsRunner => "enemy_shorts",
                EnemyType.SpamShooter => "enemy_spam",
                EnemyType.DoomScroller => "enemy_doomscroller",
                EnemyType.DopamineBoss => "enemy_boss",
                EnemyType.AuditorKim => "enemy_boss",
                _ => "enemy_dust"
            };
            if (spriteRenderer != null)
            {
                Sprite sp = Resources.Load<Sprite>($"Enemies/{spriteName}");
#if UNITY_EDITOR
                if (sp == null)
                    sp = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Sprites/Enemies/{spriteName}.png");
#endif
                if (sp != null)
                {
                    spriteRenderer.sprite = sp;
                }
            }

            if (Meta.GameDataManager.Instance != null)
            {
                var diff = Meta.DungeonDatabase.GetDifficultyMultiplier(Meta.GameDataManager.Instance.SelectedDifficulty);
                maxHP *= diff.enemyHpMult;
                damage *= diff.enemyDmgMult;
                moveSpeed *= diff.enemySpeedMult;

                if (Meta.GameDataManager.Instance.IsPactActive(Meta.PactType.WeekendOverwork))
                {
                    moveSpeed *= 1.25f; // 주말 특근 조약: 몬스터 이동속도 +25%
                }
            }

            if (IsElite)
            {
                maxHP *= 2.2f;
                damage *= 1.35f;
                transform.localScale *= 1.3f;

                switch (eliteAffix)
                {
                    case EliteAffix.FirewallShield:
                        shieldCount = 3;
                        SetVisualColor(new Color(0.2f, 0.95f, 1f)); // 네온 시안
                        break;
                    case EliteAffix.HasteSprinter:
                        moveSpeed *= 1.55f;
                        SetVisualColor(new Color(1f, 0.95f, 0.2f)); // 네온 옐로우
                        break;
                    case EliteAffix.GemThief:
                        SetVisualColor(new Color(0.9f, 0.2f, 0.9f)); // 네온 퍼플
                        break;
                    case EliteAffix.SpamReflector:
                        SetVisualColor(new Color(1f, 0.3f, 0.3f)); // 네온 레드
                        break;
                }
            }

            currentHP = maxHP;
            UpdateNameplate();
            gameObject.SetActive(true);
        }

        private void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing)
                return;

            if (PlayerController.Instance == null || !IsAlive) return;

            Vector3 playerPos = PlayerController.Instance.transform.position;
            Vector3 dir = (playerPos - transform.position).normalized;
            float distToPlayer = Vector2.Distance(transform.position, playerPos);

            // 특수 몬스터 행동 분기
            if (enemyType == EnemyType.ShortsRunner)
            {
                // 지그재그 전광석화 돌진
                Vector3 perp = new Vector3(-dir.y, dir.x, 0f) * Mathf.Sin(Time.time * 9f) * 1.2f;
                transform.position += (dir + perp).normalized * (moveSpeed * Time.deltaTime);
            }
            else if (enemyType == EnemyType.SpamShooter)
            {
                // 원거리 유지 및 투사체 발사
                if (distToPlayer > 5.5f)
                {
                    transform.position += dir * (moveSpeed * Time.deltaTime);
                }
                else
                {
                    // 5.5m 이내에선 사격
                    specialActionTimer -= Time.deltaTime;
                    if (specialActionTimer <= 0f)
                    {
                        specialActionTimer = 2.6f;
                        ShootProjectile(dir);
                    }
                }
            }
            else if (enemyType == EnemyType.DopamineBoss)
            {
                if (!isBossSpecialActing)
                {
                    // 일반 추적 이동
                    transform.position += dir * (moveSpeed * Time.deltaTime);

                    // 1. 기본 8방향 탄막 사격
                    specialActionTimer -= Time.deltaTime;
                    if (specialActionTimer <= 0f)
                    {
                        specialActionTimer = isBossBerserk ? 1.8f : 3.6f;
                        if (isBossBerserk)
                            FireBossSpiralBurst(16);
                        else
                            FireBossRadialBurst(8);
                    }

                    // 2. 6초 주기 특수 텔레그래프 필살기 패턴 (돌진 vs 원형 충격파 교대)
                    bossPatternTimer -= Time.deltaTime;
                    if (bossPatternTimer <= 0f)
                    {
                        bossPatternTimer = 6.5f;
                        bossPatternIndex++;
                        if (bossPatternIndex % 2 == 1)
                        {
                            StartCoroutine(BossTelegraphDashRoutine(playerPos));
                        }
                        else
                        {
                            StartCoroutine(BossTelegraphShockwaveRoutine());
                        }
                    }
                }
            }
            else if (enemyType == EnemyType.AuditorKim)
            {
                if (!isBossSpecialActing)
                {
                    transform.position += dir * (moveSpeed * Time.deltaTime);

                    // 1. 서류 뭉치 8방향 사격
                    specialActionTimer -= Time.deltaTime;
                    if (specialActionTimer <= 0f)
                    {
                        specialActionTimer = isBossBerserk ? 1.6f : 3.2f;
                        if (isBossBerserk)
                            FireBossSpiralBurst(16);
                        else
                            FireBossRadialBurst(10);
                    }

                    // 2. 5초 주기 돌진 및 충격파 필살기
                    bossPatternTimer -= Time.deltaTime;
                    if (bossPatternTimer <= 0f)
                    {
                        bossPatternTimer = 5.2f;
                        bossPatternIndex++;
                        if (bossPatternIndex % 2 == 1)
                        {
                            StartCoroutine(BossTelegraphDashRoutine(playerPos));
                        }
                        else
                        {
                            StartCoroutine(BossTelegraphShockwaveRoutine());
                        }
                    }
                }
            }
            else
            {
                // 기본 추적 이동
                transform.position += dir * (moveSpeed * Time.deltaTime);
            }

            // 스프라이트 반전
            if (spriteRenderer != null && dir.x != 0)
            {
                spriteRenderer.flipX = dir.x < 0;
            }

            // 엘리트: 보석 강탈자(GemThief) 주변 경험치 보석 흡수
            if (IsElite && eliteAffix == EliteAffix.GemThief)
            {
                affixActionTimer -= Time.deltaTime;
                if (affixActionTimer <= 0f)
                {
                    affixActionTimer = 0.75f;
                    TryStealNearbyGems();
                }
            }
        }

        private void TryStealNearbyGems()
        {
            var gems = FindObjectsByType<ExpGem>();
            foreach (var g in gems)
            {
                if (g != null && g.gameObject.activeInHierarchy && Vector2.Distance(transform.position, g.transform.position) <= 2.6f)
                {
                    stolenGemsCount++;
                    currentHP = Mathf.Min(maxHP, currentHP + 6f);
                    transform.localScale *= 1.015f;
                    UpdateNameplate();
                    g.gameObject.SetActive(false);
                    break; // 1틱당 1개 흡수
                }
            }
        }

        private void ShootProjectile(Vector2 dir)
        {
            GameObject projObj = new GameObject("SpamBullet");
            projObj.transform.position = transform.position;
            EnemyProjectile ep = projObj.AddComponent<EnemyProjectile>();
            ep.Initialize(dir, 4.5f, damage);
        }

        private void FireBossRadialBurst(int bulletCount = 8)
        {
            for (int i = 0; i < bulletCount; i++)
            {
                float angle = i * (360f / bulletCount) * Mathf.Deg2Rad;
                Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

                GameObject projObj = new GameObject("BossBullet");
                projObj.transform.position = transform.position;
                EnemyProjectile ep = projObj.AddComponent<EnemyProjectile>();
                ep.Initialize(dir, 5.0f, damage * 0.8f);
            }
        }

        private void FireBossSpiralBurst(int bulletCount = 16)
        {
            float baseAngleOffset = Time.time * 65f;
            for (int i = 0; i < bulletCount; i++)
            {
                float angle = (i * (360f / bulletCount) + baseAngleOffset) * Mathf.Deg2Rad;
                Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

                GameObject projObj = new GameObject("BossSpiralBullet");
                projObj.transform.position = transform.position;
                EnemyProjectile ep = projObj.AddComponent<EnemyProjectile>();
                ep.Initialize(dir, 5.5f, damage * 0.85f);
            }
            Core.SoundManager.Instance?.Play(Core.SoundEffect.Laser, 0.5f);
        }

        private void TriggerBossBerserk()
        {
            isBossBerserk = true;
            moveSpeed *= 1.35f;
            damage *= 1.25f;

            Core.SoundManager.Instance?.Play(Core.SoundEffect.Warning, 1.0f);
            Core.HitStopManager.Instance?.TriggerHitStop(0.35f);
            Core.PixelParticleManager.Instance?.SpawnDeathPop(transform.position, new Color(1f, 0.2f, 0.1f), 35);
            Core.DamageTextManager.Instance?.SpawnDamageText(0f, transform.position + Vector3.up * 1.5f, true);

            if (nameplateText != null)
            {
                nameplateText.text = "🔥 [광폭화 각성] " + nameplateText.text;
                nameplateText.color = new Color(1f, 0.2f, 0.1f);
            }
        }

        private IEnumerator BossTelegraphDashRoutine(Vector3 targetPos)
        {
            isBossSpecialActing = true;
            // 1. 텔레그래프 장판 표시 (0.8초)
            Core.BossTelegraphManager.Instance?.SpawnLineTelegraph(transform.position, targetPos, 1.8f, 0.8f);
            Core.SoundManager.Instance?.Play(Core.SoundEffect.Dash, 0.7f);

            if (spriteRenderer != null) spriteRenderer.color = new Color(1f, 0.35f, 0.35f);
            yield return new WaitForSeconds(0.8f);
            if (spriteRenderer != null) spriteRenderer.color = originalColor;

            // 2. 고속 돌진 (0.28초간 4.2배 속도)
            Vector3 dashDir = (targetPos - transform.position).normalized;
            float dashTime = 0.28f;
            float elapsed = 0f;
            while (elapsed < dashTime && IsAlive)
            {
                elapsed += Time.deltaTime;
                transform.position += dashDir * (moveSpeed * 4.2f * Time.deltaTime);
                yield return null;
            }

            Core.PixelParticleManager.Instance?.SpawnDeathPop(transform.position, new Color(1f, 0.3f, 0.3f), 15);
            Core.SoundManager.Instance?.Play(Core.SoundEffect.Explosion, 0.6f);

            yield return new WaitForSeconds(0.35f);
            isBossSpecialActing = false;
        }

        private IEnumerator BossTelegraphShockwaveRoutine()
        {
            isBossSpecialActing = true;
            float radius = 4.5f;
            // 1. 원형 텔레그래프 장판 표시 (1.0초)
            Core.BossTelegraphManager.Instance?.SpawnCircleTelegraph(transform.position, radius, 1.0f);

            Vector3 baseScale = transform.localScale;
            float t = 0f;
            while (t < 1.0f && IsAlive)
            {
                t += Time.deltaTime;
                float s = 1.0f + Mathf.Sin(t * 15f) * 0.12f;
                transform.localScale = baseScale * s;
                yield return null;
            }
            transform.localScale = baseScale;

            // 2. 충격파 발동: 14방향 탄막 + 광역 폭발
            FireBossRadialBurst(14);
            Core.PixelParticleManager.Instance?.SpawnDeathPop(transform.position, new Color(1f, 0.85f, 0.2f), 28);
            Core.SoundManager.Instance?.Play(Core.SoundEffect.Explosion, 0.85f);

            if (PlayerController.Instance != null)
            {
                float dist = Vector2.Distance(transform.position, PlayerController.Instance.transform.position);
                if (dist <= radius)
                {
                    PlayerController.Instance.TakeDamage(damage * 1.5f);
                }
            }

            yield return new WaitForSeconds(0.4f);
            isBossSpecialActing = false;
        }

        private void FireSpamRadialReflection()
        {
            int bulletCount = 6;
            for (int i = 0; i < bulletCount; i++)
            {
                float angle = i * (360f / bulletCount) * Mathf.Deg2Rad;
                Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

                GameObject projObj = new GameObject("ReflectSpamBullet");
                projObj.transform.position = transform.position;
                EnemyProjectile ep = projObj.AddComponent<EnemyProjectile>();
                ep.Initialize(dir, 4.2f, damage * 0.7f);
            }
            Core.SoundManager.Instance?.Play(Core.SoundEffect.Shoot, 0.4f);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (other.CompareTag("Player") && IsAlive)
            {
                PlayerController.Instance?.TakeDamage(damage);
            }
        }

        public void TakeDamage(float amount)
        {
            TakeDamage(amount, Vector2.zero, 0f);
        }

        public void TakeDamage(float amount, Vector2 knockbackDir, float knockbackForce = 0f)
        {
            if (!IsAlive) return;

            // 엘리트: 방화벽 쉴드 피해 무효화
            if (IsElite && eliteAffix == EliteAffix.FirewallShield && shieldCount > 0)
            {
                shieldCount--;
                UpdateNameplate();
                Core.DamageTextManager.Instance?.SpawnDamageText(0f, transform.position + Vector3.up * 0.4f, false);
                Core.SoundManager.Instance?.Play(Core.SoundEffect.Hit, 0.35f);
                return;
            }

            currentHP -= amount;

            if ((enemyType == EnemyType.DopamineBoss || enemyType == EnemyType.AuditorKim) && !isBossBerserk && currentHP > 0f && currentHP <= maxHP * 0.5f)
            {
                TriggerBossBerserk();
            }

            // 중타격/엘리트 피격 시 0.04초 역경직(Hit-Stop) 발동
            if (amount >= 35f || IsElite || enemyType == EnemyType.DopamineBoss)
            {
                Core.HitStopManager.Instance?.TriggerHitStop(0.04f);
            }

            // 엘리트: 알림 폭풍 반사 사격
            if (IsElite && eliteAffix == EliteAffix.SpamReflector && Random.value < 0.35f)
            {
                FireSpamRadialReflection();
            }

            // 넉백 적용
            if (knockbackForce > 0f)
            {
                transform.position += (Vector3)(knockbackDir.normalized * knockbackForce);
            }

            // 데미지 텍스트 팝업 및 피격음, 네온 스파크 분사
            Core.DamageTextManager.Instance?.SpawnDamageText(amount, transform.position, amount >= 35f);
            Core.SoundManager.Instance?.Play(Core.SoundEffect.Hit, 0.4f);
            Core.PixelParticleManager.Instance?.SpawnSparkHit(transform.position, knockbackDir, new Color(1f, 0.9f, 0.2f), 4);

            // 피격 플래시
            if (Application.isPlaying && gameObject.activeInHierarchy)
            {
                if (flashCoroutine != null) StopCoroutine(flashCoroutine);
                flashCoroutine = StartCoroutine(FlashRoutine());
            }

            if (currentHP <= 0f)
            {
                Die();
            }
        }

        private IEnumerator FlashRoutine()
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.color = Color.white;
                yield return new WaitForSeconds(0.08f);
                spriteRenderer.color = originalColor;
            }
        }

        private void Die()
        {
            currentHP = 0f;
            bool isBoss = (enemyType == EnemyType.DopamineBoss);

            // 킬 카운트 등록
            GameManager.Instance?.RegisterKill(isBoss);
            Core.EmergencyQuestManager.Instance?.ReportEnemyKilled(enemyType);

            // 팝콘 네온 파티클 폭발 연출
            int popCount = isBoss ? 45 : (IsElite ? 22 : 12);
            Core.PixelParticleManager.Instance?.SpawnDeathPop(transform.position, originalColor, popCount);

            // 아이템 및 경험치 드랍
            DropLoot(isBoss);

            gameObject.SetActive(false);
        }

        private void DropLoot(bool isBoss)
        {
            if (isBoss)
            {
                // 보스 격파 시 확정 황금상자 & 대량 EXP 드랍 (단일 황금상자로 밸런스 조정)
                Core.HitStopManager.Instance?.TriggerHitStop(0.5f);
                GameObject gChest = new GameObject("GoldenChest");
                gChest.transform.position = transform.position;
                gChest.AddComponent<Items.GoldenChest>();
                ItemManager.Instance?.SpawnGem(transform.position + Vector3.left * 1.2f, GemType.Cake, 0);
                ItemManager.Instance?.SpawnGem(transform.position + Vector3.right * 1.2f, GemType.Magnet, 0);
                ItemManager.Instance?.SpawnGem(transform.position + Vector3.up * 1.0f, GemType.Pink, 35);
            }
            else if (IsElite)
            {
                // 엘리트 몬스터 격파 시 확정 황금상자 + 보너스 골드 드랍! (단일 황금상자 유지)
                Core.HitStopManager.Instance?.TriggerHitStop(0.5f);
                GameObject gChest = new GameObject("GoldenChest");
                gChest.transform.position = transform.position;
                gChest.AddComponent<Items.GoldenChest>();
                GameManager.Instance?.AddBonusGold(Random.Range(30, 55));
                Core.SoundManager.Instance?.Play(Core.SoundEffect.LevelUp, 0.6f);

                // 보석 강탈자였을 경우 훔친 보석 환급
                if (eliteAffix == EliteAffix.GemThief && stolenGemsCount > 0)
                {
                    int returnGems = Mathf.Min(16, stolenGemsCount * 2);
                    for (int g = 0; g < returnGems; g++)
                    {
                        Vector3 jitter = (Vector3)Random.insideUnitCircle * 1.6f;
                        ItemManager.Instance?.SpawnGem(transform.position + jitter, GemType.Blue, 5);
                    }
                }
            }
            else
            {
                // 일반 적 드랍
                GemType gType = GemType.Green;
                if (enemyType == EnemyType.HeavySloth) gType = GemType.Blue;
                else if (enemyType == EnemyType.NotificationGhost) gType = GemType.Pink;

                ItemManager.Instance?.SpawnGem(transform.position, gType, baseExp);

                // 무작위 특수 아이템 드랍 (2% 케이크, 2% 폭탄 - 일반 몹 상자 남발 방지)
                int roll = Random.Range(0, 1000);
                if (roll < 20)
                    ItemManager.Instance?.SpawnGem(transform.position + Vector3.up * 0.4f, GemType.Cake, 0);
                else if (roll < 40)
                    ItemManager.Instance?.SpawnGem(transform.position + Vector3.up * 0.4f, GemType.Bomb, 0);
            }

            if (enemyType == EnemyType.AuditorKim)
            {
                SpawnSeveranceLoot(transform.position);
                Core.HRAuditManager.Instance?.OnAuditorDefeated();
            }
        }

        private void SpawnSeveranceLoot(Vector3 pos)
        {
            GameObject boxObj = new GameObject("GoldenSeveranceBox");
            boxObj.transform.position = pos;

            SpriteRenderer sr = boxObj.AddComponent<SpriteRenderer>();
            Texture2D tex = CreateSeveranceBoxTex();
            sr.sprite = Sprite.Create(tex, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 16);
            sr.sortingOrder = 22;
            boxObj.transform.localScale = Vector3.one * 1.6f;

            CircleCollider2D col = boxObj.AddComponent<CircleCollider2D>();
            col.radius = 1.2f;
            col.isTrigger = true;

            boxObj.AddComponent<SeveranceBoxTrigger>();

            for (int i = 0; i < 4; i++)
            {
                ItemManager.Instance?.SpawnGem(pos + (Vector3)(Random.insideUnitCircle * 1.5f), GemType.Pink, 10);
            }
        }

        private Texture2D CreateSeveranceBoxTex()
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

        private void SetVisualColor(Color c)
        {
            originalColor = c;
            if (spriteRenderer != null)
                spriteRenderer.color = c;
        }

        private Color GetBossStageColor(int stage)
        {
            return stage switch
            {
                1 => new Color(0.48f, 0.49f, 0.86f), // 1스테이지: 라벤더 퍼플
                2 => new Color(0.23f, 0.53f, 1.0f),  // 2스테이지: 오션 블루
                3 => new Color(1.0f, 0.0f, 0.43f),   // 3스테이지: 네온 마젠타
                4 => new Color(1.0f, 0.35f, 0.37f),  // 4스테이지: 분노의 코랄 레드
                _ => new Color(0.51f, 0.22f, 0.93f)  // 5스테이지+: 다크 사이버 퍼플
            };
        }
    }
}
