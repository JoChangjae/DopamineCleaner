using System;
using UnityEngine;
using DopamineCleaner.Core;
using DopamineCleaner.Meta;

namespace DopamineCleaner.Characters
{
    public enum CharacterType
    {
        SmartBrush,    // 밸런스형, 기본 클린 탄환 연사
        EspressoShot,  // 고속 공격형, 카페인 펄스 충격파
        VitaminBunny,  // 고기동형, 번개 체인
        TomatoKnight   // 탱커형, 높은 체력 & 부활
    }

    [RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D))]
    public class PlayerController : MonoBehaviour
    {
        public static PlayerController Instance { get; private set; }

        [Header("캐릭터 기본 설정")]
        [SerializeField] private CharacterType characterType = CharacterType.SmartBrush;
        [SerializeField] private float moveSpeed = 5.5f;
        [SerializeField] private float currentHP = 100f;
        [SerializeField] private float maxHP = 100f;

        [Header("대시(Dash) 시스템")]
        [SerializeField] private float dashSpeedMultiplier = 2.8f;
        [SerializeField] private float dashDuration = 0.22f;
        [SerializeField] private float dashCooldown = 3.0f;
        private float dashTimer = 0f;
        private float dashCooldownTimer = 0f;
        private float worklifeSpeedBoostTimer = 0f;
        public bool IsDashReady => dashCooldownTimer <= 0f;
        public float DashRemainingCooldown => Mathf.Max(0f, dashCooldownTimer);

        [Header("궁극기(Ultimate Q) 시스템")]
        [SerializeField] private float ultimateCooldown = 18f;
        private float ultimateCooldownTimer = 0f;
        private string characterId = "char_intern";

        public event Action<float, float> OnUltimateCooldownChanged; // currentTimer, maxCooldown
        public string UltimateSkillName { get; private set; } = "전면 대청소";
        public string UltimateSkillIcon { get; private set; } = "🧹";
        public float UltimateCooldownRatio => ultimateCooldown > 0f ? Mathf.Clamp01(1f - (ultimateCooldownTimer / ultimateCooldown)) : 1f;
        public bool IsUltimateReady => ultimateCooldownTimer <= 0f;
        public float UltimateRemainingTime => Mathf.Max(0f, ultimateCooldownTimer);

        [Header("무적(Invincibility)")]
        [SerializeField] private float invincibilityDuration = 0.45f;
        private float invincibleTimer = 0f;

        [Header("경험치 및 레벨업")]
        [SerializeField] private int currentLevel = 1;
        [SerializeField] private float currentExp = 0f;
        [SerializeField] private float nextLevelExp = 10f;
        [SerializeField] private float magnetRadius = 3.5f;

        [Header("뽀모도로 부활 (Angel Revive)")]
        [SerializeField] private bool hasRevive = true;
        [SerializeField] private bool reviveUsed = false;

        [Header("정신과 전문의 나노 쉴드")]
        private bool hasNanoShield = false;
        private float nanoShieldTimer = 15f;

        [Header("컴포넌트 캐싱")]
        private Rigidbody2D rb;
        private SpriteRenderer spriteRenderer;
        private Vector2 movementInput;

        // 이벤트
        public event Action<float, float> OnHealthChanged; // currentHP, maxHP
        public event Action<float, float, int> OnExpChanged; // currentExp, nextExp, level
        public event Action<int> OnLevelUp;
        public event Action<float, float> OnDashCooldownChanged; // currentTimer, maxCooldown
        public event Action OnPlayerDied;

        public CharacterType Type => characterType;
        public float CurrentHP => currentHP;
        public float MaxHP => maxHP;
        public int Level => currentLevel;
        public float MagnetRadius => magnetRadius;
        public bool IsDashing => dashTimer > 0f;
        public bool IsInvincible => invincibleTimer > 0f || IsDashing;
        public float NextLevelExp => nextLevelExp;

        // 사원 승진 직급 및 런타임 버프 배율
        public CareerRank CurrentCareerRank { get; private set; } = CareerRank.Intern;
        public int CareerClearCount { get; private set; } = 0;
        public string CareerRankTitle { get; private set; } = "수습 사원";
        public float TotalDamageMultiplier { get; set; } = 1.0f;
        public float CooldownRateMultiplier { get; set; } = 1.0f;
        public float CritChanceBonus { get; set; } = 0f;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);

            rb = GetComponent<Rigidbody2D>();
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

            if (Meta.GameDataManager.Instance == null)
            {
                GameObject dm = new GameObject("[GameDataManager]");
                dm.AddComponent<Meta.GameDataManager>();
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void Start()
        {
            InitCharacterStats();
            OnHealthChanged?.Invoke(currentHP, maxHP);
            OnExpChanged?.Invoke(currentExp, nextLevelExp, currentLevel);
        }

        private void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing)
            {
                movementInput = Vector2.zero;
                return;
            }

            // 1. 입력 처리
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            movementInput = new Vector2(h, v).normalized;

            // 스프라이트 좌우 반전
            if (h != 0 && spriteRenderer != null)
            {
                spriteRenderer.flipX = h < 0;
            }

            // 2. 대시 입력 처리 (Space 또는 Shift)
            if (dashCooldownTimer > 0f)
            {
                dashCooldownTimer -= Time.deltaTime;
                OnDashCooldownChanged?.Invoke(Mathf.Max(0f, dashCooldownTimer), dashCooldown);
            }

            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.LeftShift))
            {
                PerformDash();
            }

            if (dashTimer > 0f)
            {
                dashTimer -= Time.deltaTime;
            }

            // 3. 궁극기(Q) 쿨타임 및 입력
            if (ultimateCooldownTimer > 0f)
            {
                ultimateCooldownTimer -= Time.deltaTime;
                OnUltimateCooldownChanged?.Invoke(Mathf.Max(0f, ultimateCooldownTimer), ultimateCooldown);
            }

            if (Input.GetKeyDown(KeyCode.Q))
            {
                PerformUltimateSkill();
            }

            // 3. 무적 타이머 및 피버 시각 효과 처리
            if (invincibleTimer > 0f)
            {
                invincibleTimer -= Time.deltaTime;
                // 피격 깜빡임 효과
                if (spriteRenderer != null)
                {
                    float alpha = Mathf.PingPong(Time.time * 12f, 1f);
                    Color c = spriteRenderer.color;
                    c.a = 0.4f + alpha * 0.6f;
                    spriteRenderer.color = c;
                }
            }
            else if (FeverManager.Instance != null && FeverManager.Instance.IsFever)
            {
                // 피버 중 황금/민트 네온 펄스 효과
                if (spriteRenderer != null)
                {
                    float hue = Mathf.PingPong(Time.time * 2f, 0.2f) + 0.12f; // 골드~옐로우
                    spriteRenderer.color = Color.HSVToRGB(hue, 0.8f, 1f);
                }
                // 네온 화염 아우라 파티클
                if (Time.frameCount % 5 == 0)
                {
                    Core.PixelParticleManager.Instance?.SpawnFeverAura(transform.position, FeverManager.Instance.IsOverdrive);
                }
            }
            else if (spriteRenderer != null && (spriteRenderer.color.a < 1f || spriteRenderer.color != Color.white))
            {
                spriteRenderer.color = Color.white;
            }

            // 4. 단축키 1: 응급 구급약 사용
            if ((Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1)) && currentHP < maxHP)
            {
                TryUseFirstAid();
            }

            // 5. 정신과 전문의(char_doctor) 나노 쉴드 자동 재충전
            if (characterId == "char_doctor" && !hasNanoShield)
            {
                nanoShieldTimer -= Time.deltaTime;
                if (nanoShieldTimer <= 0f)
                {
                    hasNanoShield = true;
                    nanoShieldTimer = 15f;
                    DamageTextManager.Instance?.SpawnCustomText("🛡️ 나노 쉴드 충전 완료!", transform.position + Vector3.up * 0.8f, new Color(0.3f, 0.95f, 1f));
                    SoundManager.Instance?.Play(SoundEffect.GemCollect, 0.6f);
                }
            }
        }

        private void FixedUpdate()
        {
            if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing)
            {
                rb.linearVelocity = Vector2.zero;
                return;
            }

            float speed = moveSpeed;
            if (IsDashing)
            {
                speed *= dashSpeedMultiplier;
            }
            if (FeverManager.Instance != null && FeverManager.Instance.IsFever)
            {
                speed *= 1.3f;
            }
            if (GameManager.Instance != null && GameManager.Instance.IsHyperMode)
            {
                speed *= 1.25f; // 하이퍼 모드 기동력 증폭
            }
            if (MentalSanctuaryManager.Instance != null)
            {
                if (MentalSanctuaryManager.Instance.IsInSanctuary)
                {
                    speed *= 1.2f; // 쉼터 내 쾌속 회복 기동
                }
                else if (MentalSanctuaryManager.Instance.OverloadPercent >= 75f)
                {
                    speed *= 0.88f; // 도파민 과부하 뇌 피로 둔화
                }
            }

            rb.linearVelocity = movementInput * speed;
        }

        public void PerformDash()
        {
            if (dashCooldownTimer <= 0f)
            {
                dashTimer = dashDuration;
                dashCooldownTimer = dashCooldown;
                SoundManager.Instance?.Play(SoundEffect.Dash, 0.4f);
                OnDashCooldownChanged?.Invoke(dashCooldownTimer, dashCooldown);
            }
        }

        public void TakeDamage(float amount)
        {
            if (IsInvincible || currentHP <= 0f) return;

            // 정신과 전문의 나노 쉴드 1회 흡수
            if (hasNanoShield)
            {
                hasNanoShield = false;
                nanoShieldTimer = 15f;
                invincibleTimer = 0.5f;
                DamageTextManager.Instance?.SpawnCustomText("🛡️ 나노 쉴드 방어! (피해 무효)", transform.position + Vector3.up * 0.8f, new Color(0.3f, 0.95f, 1f));
                SoundManager.Instance?.Play(SoundEffect.Whirlwind, 0.5f);
                return;
            }

            // 피버 모드 시 피격 피해 60% 경감
            if (FeverManager.Instance != null && FeverManager.Instance.IsFever)
            {
                amount *= 0.4f;
            }

            currentHP -= amount;
            invincibleTimer = invincibilityDuration;

            if (currentHP <= 0f)
            {
                currentHP = 0f;
                OnHealthChanged?.Invoke(currentHP, maxHP);

                if (hasRevive && !reviveUsed)
                {
                    // 사후 부활 증서 혜택 발동
                    reviveUsed = true;
                    if (Meta.GameDataManager.Instance != null)
                    {
                        Meta.GameDataManager.Instance.Profile.inventory.Remove("item_revive_policy");
                        Meta.GameDataManager.Instance.SaveProfile();
                    }
                    currentHP = maxHP * 0.5f;
                    invincibleTimer = 3.0f;
                    OnHealthChanged?.Invoke(currentHP, maxHP);
                }
                else
                {
                    OnPlayerDied?.Invoke();
                    GameManager.Instance?.TriggerGameOver(false);
                }
            }
            else
            {
                OnHealthChanged?.Invoke(currentHP, maxHP);
            }
        }

        public void Heal(float amount)
        {
            if (Meta.GameDataManager.Instance != null && Meta.GameDataManager.Instance.IsPactActive(Meta.PactType.AIPressure))
            {
                amount *= 0.5f; // AI 실적 압박 조약: 회복량 50% 삭감
            }

            currentHP = Mathf.Min(maxHP, currentHP + amount);
            OnHealthChanged?.Invoke(currentHP, maxHP);
        }

        public void TryUseFirstAid()
        {
            if (Meta.GameDataManager.Instance != null)
            {
                var profile = Meta.GameDataManager.Instance.Profile;
                if (profile.inventory.Contains("item_first_aid"))
                {
                    profile.inventory.Remove("item_first_aid");
                    Meta.GameDataManager.Instance.SaveProfile();
                    Heal(40f);
                }
            }
        }

        public void AddExp(float exp)
        {
            if (Meta.GameDataManager.Instance != null && Meta.GameDataManager.Instance.IsPactActive(Meta.PactType.NotificationBombard))
            {
                exp *= 1.25f; // 스팸 폭격 조약: 경험치 획득량 +25%
            }

            currentExp += exp;

            bool leveledUp = false;
            while (currentExp >= nextLevelExp)
            {
                currentExp -= nextLevelExp;
                currentLevel++;
                nextLevelExp = Mathf.Round(nextLevelExp * 1.35f + 5f);
                leveledUp = true;
                OnLevelUp?.Invoke(currentLevel);
                GameManager.Instance?.SetState(GameState.LevelUp);
            }

            if (leveledUp)
            {
                SoundManager.Instance?.Play(SoundEffect.LevelUp, 0.55f);
            }

            OnExpChanged?.Invoke(currentExp, nextLevelExp, currentLevel);
        }

        public void UpgradeMaxHP(float amount)
        {
            maxHP += amount;
            currentHP = Mathf.Min(maxHP, currentHP + amount);
            OnHealthChanged?.Invoke(currentHP, maxHP);
        }

        public void UpgradeMoveSpeed(float multiplier)
        {
            moveSpeed *= multiplier;
        }

        public void UpgradeMagnet(float radiusIncrease)
        {
            magnetRadius += radiusIncrease;
        }

        public void PerformUltimateSkill()
        {
            if (ultimateCooldownTimer > 0f) return;

            ultimateCooldownTimer = ultimateCooldown;
            OnUltimateCooldownChanged?.Invoke(ultimateCooldownTimer, ultimateCooldown);

            SoundManager.Instance?.Play(SoundEffect.Fever, 0.85f);
            CameraFollow.Instance?.Shake(0.35f, 0.4f);
            Meta.AchievementManager.Instance?.ReportUltimateUsed();

            switch (characterId)
            {
                case "char_intern":
                    ExecuteInternBroomSweep();
                    break;
                case "char_adjuster":
                case "char_auditor":
                    ExecuteAuditorImmunity();
                    break;
                case "char_cleaner":
                case "char_manager":
                    ExecuteManagerLockdown();
                    break;
                case "char_director":
                    ExecuteDirectorGoldenRain();
                    break;
                case "char_streamer":
                    ExecuteStreamerSuperChat();
                    break;
                case "char_doctor":
                    ExecuteDoctorMentalCare();
                    break;
                default:
                    ExecuteInternBroomSweep();
                    break;
            }
        }

        private void ExecuteInternBroomSweep()
        {
            // 김인턴: 전방 대청소 빗자루 넉백 및 투사체 정화
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 4.5f);
            foreach (var hit in hits)
            {
                if (hit != null && hit.CompareTag("Enemy") && hit.TryGetComponent<Enemies.Enemy>(out var enemy))
                {
                    Vector2 kb = ((Vector2)enemy.transform.position - (Vector2)transform.position).normalized;
                    enemy.TakeDamage(80f, kb, 3.2f);
                }
            }

            // 적 투사체 소멸
            var projs = FindObjectsByType<Enemies.EnemyProjectile>();
            foreach (var p in projs)
            {
                if (p != null) Destroy(p.gameObject);
            }

            DamageTextManager.Instance?.SpawnDamageText(80f, transform.position + Vector3.up * 0.8f, true);
        }

        private void ExecuteAuditorImmunity()
        {
            // 박사정: 4초간 완전 무적 및 주변 3.5m 감전 파동
            invincibleTimer = 4.0f;
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 3.8f);
            foreach (var hit in hits)
            {
                if (hit != null && hit.CompareTag("Enemy") && hit.TryGetComponent<Enemies.Enemy>(out var enemy))
                {
                    Vector2 kb = ((Vector2)enemy.transform.position - (Vector2)transform.position).normalized;
                    enemy.TakeDamage(95f, kb, 1.8f);
                }
            }
            DamageTextManager.Instance?.SpawnDamageText(95f, transform.position + Vector3.up * 0.8f, true);
        }

        private void ExecuteManagerLockdown()
        {
            // 강반장: 화면 전체 적 4.5초간 80% 이동속도 감속 & 45 데미지
            var enemies = FindObjectsByType<Enemies.Enemy>();
            foreach (var e in enemies)
            {
                if (e != null && e.gameObject.activeInHierarchy)
                {
                    e.TakeDamage(45f);
                    // 감속 밀쳐내기
                    Vector2 away = ((Vector2)e.transform.position - (Vector2)transform.position).normalized;
                    e.transform.position += (Vector3)(away * 0.8f);
                }
            }
            SoundManager.Instance?.Play(SoundEffect.Explosion, 0.7f);
        }

        private void ExecuteDirectorGoldenRain()
        {
            // 윤이사: 화면 전체 황금 코인비 폭발 + 즉시 50 Gold 적립 + 자석
            var enemies = FindObjectsByType<Enemies.Enemy>();
            foreach (var e in enemies)
            {
                if (e != null && e.gameObject.activeInHierarchy)
                {
                    e.TakeDamage(150f);
                }
            }

            GameManager.Instance?.AddBonusGold(50);
            Items.ItemManager.Instance?.TriggerMagnet();
            SoundManager.Instance?.Play(SoundEffect.GemCollect, 0.8f);
            DamageTextManager.Instance?.SpawnDamageText(150f, transform.position + Vector3.up * 0.8f, true);
        }

        private void ExecuteStreamerSuperChat()
        {
            // 유나 (인기 스트리머): 구독과 슈퍼챗 폭죽! 피버 강제 발동 + 110 대미지 + 45 골드 수금 + 전방위 픽셀 팝
            FeverManager.Instance?.AddFever(100f);
            GameManager.Instance?.AddBonusGold(45);
            SoundManager.Instance?.Play(SoundEffect.Fever, 1.0f);

            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 6.0f);
            foreach (var hit in hits)
            {
                if (hit != null && hit.CompareTag("Enemy") && hit.TryGetComponent<Enemies.Enemy>(out var enemy))
                {
                    enemy.TakeDamage(110f);
                }
            }

            DamageTextManager.Instance?.SpawnCustomText("🎙️ [슈퍼챗 100만 도파민 후원 폭발!]", transform.position + Vector3.up * 1.1f, new Color(1f, 0.4f, 0.85f));
            Core.PixelParticleManager.Instance?.SpawnNeonPop(transform.position, new Color(1f, 0.3f, 0.8f), 24);
        }

        private void ExecuteDoctorMentalCare()
        {
            // 오원장 (정신과 전문의): 즉각 체력 50% 회복 + 5초 완전 무적 및 나노 쉴드 재충전
            Heal(maxHP * 0.5f);
            invincibleTimer = 5.0f;
            hasNanoShield = true;
            SoundManager.Instance?.Play(SoundEffect.LevelUp, 0.9f);

            DamageTextManager.Instance?.SpawnCustomText("🩺 [특급 집중 멘탈 케어: HP 50% 회복 & 무적!]", transform.position + Vector3.up * 1.1f, new Color(0.2f, 0.95f, 0.65f));
            Core.PixelParticleManager.Instance?.SpawnNeonPop(transform.position, new Color(0.3f, 1f, 0.6f), 20);
        }

        public void InitCharacterStats()
        {
            if (Meta.GameDataManager.Instance != null)
            {
                var profile = Meta.GameDataManager.Instance.Profile;
                characterId = profile.selectedCharacterId;
                var def = Meta.CharacterDatabase.Get(profile.selectedCharacterId);

                maxHP = def.baseMaxHP + profile.contractHPLevel * 15f + profile.researchHpLevel * 25f;
                moveSpeed = (def.baseMoveSpeed + profile.contractSpeedLevel * 0.3f) * (1f + profile.researchSpdLevel * 0.06f);
                magnetRadius = def.baseMagnetRadius * (1f + profile.researchMagLevel * 0.18f);
                if (profile.researchRevLevel > 0) hasRevive = true;

                // 사옥 리모델링 복지 가구 영구 버프 적용
                float officeHpMult = 1f + Meta.GameDataManager.Instance.GetTotalFacilityBuff(Meta.FacilityBuffType.MaxHpPercent);
                float officeSpdMult = 1f + Meta.GameDataManager.Instance.GetTotalFacilityBuff(Meta.FacilityBuffType.MoveSpeed);
                float officeCdMult = Mathf.Max(0.5f, 1f - Meta.GameDataManager.Instance.GetTotalFacilityBuff(Meta.FacilityBuffType.CooldownReduction));
                maxHP *= officeHpMult;
                moveSpeed *= officeSpdMult;
                dashCooldown *= officeCdMult;
                ultimateCooldown *= officeCdMult;
                CooldownRateMultiplier *= (1f + Meta.GameDataManager.Instance.GetTotalFacilityBuff(Meta.FacilityBuffType.CooldownReduction));
                CritChanceBonus = Meta.GameDataManager.Instance.GetTotalFacilityBuff(Meta.FacilityBuffType.CritChance);
                if (Meta.GameDataManager.Instance.GetTotalFacilityBuff(Meta.FacilityBuffType.ReviveChance) > 0f) hasRevive = true;

                // 궁극기 및 고유 패시브 세팅
                switch (characterId)
                {
                    case "char_intern":
                        UltimateSkillName = "전면 대청소";
                        UltimateSkillIcon = "🧹";
                        ultimateCooldown = 16f;
                        break;
                    case "char_adjuster":
                    case "char_auditor":
                        UltimateSkillName = "법적 면책 특권";
                        UltimateSkillIcon = "🛡️";
                        ultimateCooldown = 20f;
                        break;
                    case "char_cleaner":
                    case "char_manager":
                        UltimateSkillName = "긴급 현장 통제";
                        UltimateSkillIcon = "🚨";
                        ultimateCooldown = 18f;
                        break;
                    case "char_director":
                        UltimateSkillName = "자본의 힘";
                        UltimateSkillIcon = "💼";
                        ultimateCooldown = 22f;
                        break;
                    case "char_streamer":
                        UltimateSkillName = "슈퍼챗 도파민 축제";
                        UltimateSkillIcon = "🎙️";
                        ultimateCooldown = 15f;
                        magnetRadius *= 2.0f; // 인기 폭발: 2배 자석 반경
                        break;
                    case "char_doctor":
                        UltimateSkillName = "집중 멘탈 케어";
                        UltimateSkillIcon = "🩺";
                        ultimateCooldown = 20f;
                        hasNanoShield = true; // 시작 시 나노 쉴드 보유
                        break;
                    default:
                        UltimateSkillName = "전면 대청소";
                        UltimateSkillIcon = "🧹";
                        ultimateCooldown = 18f;
                        break;
                }

                // 인벤토리 장비 & 유물 패시브 효과 적용
                foreach (string itemId in profile.inventory)
                {
                    if (itemId == "artifact_magnet_badge") magnetRadius *= 1.35f;
                    if (itemId == "equip_hazard_suit") maxHP += 30f;
                    if (itemId == "item_speed_booster") moveSpeed *= 1.15f;
                    if (itemId == "item_revive_policy") hasRevive = true;
                }

                // 사원 직급 전직(Career Rank) 특전 적용
                var career = Meta.GameDataManager.Instance.GetCareerData(characterId);
                CurrentCareerRank = (Meta.CareerRank)career.rankLevel;
                CareerClearCount = career.clearCount;
                var rankDef = Meta.CareerDatabase.Get(CurrentCareerRank);
                CareerRankTitle = rankDef.rankTitle;

                maxHP *= (1f + rankDef.hpBonusPercent);
                moveSpeed *= (1f + rankDef.spdBonusPercent);
                TotalDamageMultiplier *= (1f + rankDef.dmgBonusPercent);
                if (rankDef.cooldownReductionPercent > 0f)
                {
                    dashCooldown *= (1f - rankDef.cooldownReductionPercent);
                    ultimateCooldown *= (1f - rankDef.cooldownReductionPercent);
                    CooldownRateMultiplier *= (1f + rankDef.cooldownReductionPercent);
                }
            }
            else
            {
                maxHP = 100f;
                moveSpeed = 5.5f;
                magnetRadius = 3.0f;
                characterId = "char_intern";
                UltimateSkillName = "전면 대청소";
                UltimateSkillIcon = "🧹";
                ultimateCooldown = 16f;
                CurrentCareerRank = Meta.CareerRank.Intern;
                CareerClearCount = 0;
                CareerRankTitle = "수습 사원";
                TotalDamageMultiplier = 1.0f;
                CooldownRateMultiplier = 1.0f;
            }

            currentHP = maxHP;
        }
        private void TriggerWorkLifeDashBurst()
        {
            worklifeSpeedBoostTimer = 2.5f;
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 4.0f);
            foreach (var hit in hits)
            {
                if (hit != null && hit.TryGetComponent<Enemies.Enemy>(out var enemy))
                {
                    Vector2 dir = ((Vector2)enemy.transform.position - (Vector2)transform.position).normalized;
                    enemy.TakeDamage(80f, dir, 2.0f);
                }
            }
            SoundManager.Instance?.Play(SoundEffect.Explosion, 0.45f);
            PixelParticleManager.Instance?.SpawnNeonPop(transform.position, new Color(1f, 0.45f, 0.1f), 14);
            CameraFollow.Instance?.Shake(0.15f, 0.2f);
        }

        private void TriggerImmortalDetoxRevive()
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 9.0f);
            foreach (var hit in hits)
            {
                if (hit != null && hit.TryGetComponent<Enemies.Enemy>(out var enemy))
                {
                    Vector2 dir = ((Vector2)enemy.transform.position - (Vector2)transform.position).normalized;
                    enemy.TakeDamage(120f, dir, 3.5f);
                }
            }

            for (int i = 0; i < 5; i++)
            {
                Vector3 offset = (Vector3)UnityEngine.Random.insideUnitCircle * 2.0f;
                Items.ItemManager.Instance?.SpawnGem(transform.position + offset, Items.GemType.MegaPink, 50);
            }
            Items.ItemManager.Instance?.SpawnGem(transform.position, Items.GemType.Cake, 0);

            SoundManager.Instance?.Play(SoundEffect.LevelUp, 0.8f);
            PixelParticleManager.Instance?.SpawnNeonPop(transform.position, new Color(0.4f, 1f, 0.8f), 24);
            CameraFollow.Instance?.Shake(0.3f, 0.4f);
        }
    }
}