using System;
using UnityEngine;
using DopamineCleaner.Core;

namespace DopamineCleaner.Characters
{
    public enum CharacterType
    {
        SmartBrush,    // 밸런스형, 기본 클린 탄환 연사
        EspressoShot,  // 고속 공격형, 카페인 펄스 충격파
        VitaminBunny,  // 고기동형, 번개 체인
        TomatoKnight   // 탱커형, 높은 체력 & 부활
    }

    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
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

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);

            rb = GetComponent<Rigidbody2D>();
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
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

            // 3. 무적 타이머 처리
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
            else if (spriteRenderer != null && spriteRenderer.color.a < 1f)
            {
                Color c = spriteRenderer.color;
                c.a = 1f;
                spriteRenderer.color = c;
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

            rb.linearVelocity = movementInput * speed;
        }

        private void PerformDash()
        {
            if (dashCooldownTimer <= 0f)
            {
                dashTimer = dashDuration;
                dashCooldownTimer = dashCooldown;
                OnDashCooldownChanged?.Invoke(dashCooldownTimer, dashCooldown);
            }
        }

        public void TakeDamage(float amount)
        {
            if (IsInvincible || currentHP <= 0f) return;

            currentHP -= amount;
            invincibleTimer = invincibilityDuration;

            if (currentHP <= 0f)
            {
                currentHP = 0f;
                OnHealthChanged?.Invoke(currentHP, maxHP);

                if (hasRevive && !reviveUsed)
                {
                    // 천사 부활
                    reviveUsed = true;
                    currentHP = maxHP;
                    invincibleTimer = 2.5f;
                    OnHealthChanged?.Invoke(currentHP, maxHP);
                }
                else
                {
                    OnPlayerDied?.Invoke();
                    GameManager.Instance?.TriggerGameOver();
                }
            }
            else
            {
                OnHealthChanged?.Invoke(currentHP, maxHP);
            }
        }

        public void Heal(float amount)
        {
            currentHP = Mathf.Min(maxHP, currentHP + amount);
            OnHealthChanged?.Invoke(currentHP, maxHP);
        }

        public void AddExp(float exp)
        {
            currentExp += exp;

            while (currentExp >= nextLevelExp)
            {
                currentExp -= nextLevelExp;
                currentLevel++;
                nextLevelExp = Mathf.Round(nextLevelExp * 1.35f + 5f);
                OnLevelUp?.Invoke(currentLevel);
                GameManager.Instance?.SetState(GameState.LevelUp);
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

        private void InitCharacterStats()
        {
            switch (characterType)
            {
                case CharacterType.SmartBrush:
                    maxHP = 100f;
                    moveSpeed = 5.5f;
                    break;
                case CharacterType.EspressoShot:
                    maxHP = 85f;
                    moveSpeed = 5.8f;
                    break;
                case CharacterType.VitaminBunny:
                    maxHP = 90f;
                    moveSpeed = 6.4f;
                    dashCooldown = 2.2f;
                    break;
                case CharacterType.TomatoKnight:
                    maxHP = 140f;
                    moveSpeed = 4.8f;
                    hasRevive = true;
                    break;
            }
            currentHP = maxHP;
        }
    }
}
