using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DopamineCleaner.Core;
using DopamineCleaner.Characters;
using DopamineCleaner.Weapons;

namespace DopamineCleaner.UI
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("상단 HUD (한글 호환 표준 Text)")]
        [SerializeField] private Slider hpSlider;
        [SerializeField] private Slider expSlider;
        [SerializeField] private Text levelText;
        [SerializeField] private Text timerText;
        [SerializeField] private Text stageKillsText;
        [SerializeField] private Text dashCooldownText;

        [Header("레벨업 모달")]
        [SerializeField] private GameObject levelUpPanel;
        [SerializeField] private Transform cardContainer;
        [SerializeField] private GameObject cardPrefab;

        [Header("게임오버 모달")]
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private Text gameOverStatsText;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
        }

        private void Start()
        {
            if (PlayerController.Instance != null)
            {
                PlayerController.Instance.OnHealthChanged += UpdateHealthHUD;
                PlayerController.Instance.OnExpChanged += UpdateExpHUD;
                PlayerController.Instance.OnLevelUp += ShowLevelUpUI;
                PlayerController.Instance.OnDashCooldownChanged += UpdateDashHUD;
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnKillsUpdated += UpdateKillsHUD;
                GameManager.Instance.OnStateChanged += HandleStateChanged;
            }

            if (levelUpPanel != null) levelUpPanel.SetActive(false);
            if (gameOverPanel != null) gameOverPanel.SetActive(false);
        }

        private void OnDestroy()
        {
            if (PlayerController.Instance != null)
            {
                PlayerController.Instance.OnHealthChanged -= UpdateHealthHUD;
                PlayerController.Instance.OnExpChanged -= UpdateExpHUD;
                PlayerController.Instance.OnLevelUp -= ShowLevelUpUI;
                PlayerController.Instance.OnDashCooldownChanged -= UpdateDashHUD;
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnKillsUpdated -= UpdateKillsHUD;
                GameManager.Instance.OnStateChanged -= HandleStateChanged;
            }
        }

        private void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Playing)
            {
                int totalSec = Mathf.FloorToInt(GameManager.Instance.SurvivalTime);
                if (timerText != null)
                {
                    timerText.text = $"{totalSec / 60:D2}:{totalSec % 60:D2}";
                }
            }
        }

        private void UpdateHealthHUD(float currentHP, float maxHP)
        {
            if (hpSlider != null)
            {
                hpSlider.maxValue = maxHP;
                hpSlider.value = currentHP;
            }
        }

        private void UpdateExpHUD(float currentExp, float nextExp, int level)
        {
            if (expSlider != null)
            {
                expSlider.maxValue = nextExp;
                expSlider.value = currentExp;
            }

            if (levelText != null)
            {
                levelText.text = $"Lv.{level}";
            }
        }

        private void UpdateKillsHUD(int stageKills, int totalKills)
        {
            if (stageKillsText != null && GameManager.Instance != null)
            {
                if (GameManager.Instance.IsBossActive)
                {
                    stageKillsText.text = $"👑 STAGE {GameManager.Instance.CurrentStage} BOSS 출현! (👾 {totalKills})";
                }
                else
                {
                    stageKillsText.text = $"STAGE {GameManager.Instance.CurrentStage} [{stageKills}/{GameManager.Instance.StageTargetKills}]  👾 {totalKills}";
                }
            }
        }

        private void UpdateDashHUD(float currentCooldown, float maxCooldown)
        {
            if (dashCooldownText != null)
            {
                dashCooldownText.text = currentCooldown <= 0f ? "💨 대시 준비완료 (Space)" : $"⏳ 대시 ({currentCooldown:F1}s)";
            }
        }

        private void ShowLevelUpUI(int newLevel)
        {
            if (levelUpPanel == null) return;

            levelUpPanel.SetActive(true);

            // 기존 카드 정리
            if (cardContainer != null)
            {
                foreach (Transform child in cardContainer)
                {
                    Destroy(child.gameObject);
                }

                // 3개 옵션 생성
                List<UpgradeCardInfo> options = WeaponManager.Instance.GetRandomUpgradeOptions(3);
                for (int i = 0; i < options.Count; i++)
                {
                    UpgradeCardInfo opt = options[i];
                    if (cardPrefab != null)
                    {
                        GameObject cardObj = Instantiate(cardPrefab, cardContainer);
                        UpgradeCardUI cardUI = cardObj.GetComponent<UpgradeCardUI>();
                        if (cardUI != null)
                        {
                            cardUI.Setup(opt, () =>
                            {
                                WeaponManager.Instance?.ApplyUpgrade(opt.type);
                                levelUpPanel.SetActive(false);
                            });
                        }
                    }
                }
            }
        }

        private void HandleStateChanged(GameState state)
        {
            if (state == GameState.GameOver)
            {
                ShowGameOverUI();
            }
        }

        private void ShowGameOverUI()
        {
            if (gameOverPanel == null || GameManager.Instance == null) return;

            gameOverPanel.SetActive(true);
            int sec = Mathf.FloorToInt(GameManager.Instance.SurvivalTime);

            if (gameOverStatsText != null)
            {
                gameOverStatsText.text =
                    $"도달 스테이지: STAGE {GameManager.Instance.CurrentStage}\n" +
                    $"생존 시간: {sec / 60:D2}:{sec % 60:D2}\n" +
                    $"정화한 잡생각 몬스터: {GameManager.Instance.TotalKills}마리\n" +
                    $"달성 레벨: Lv.{PlayerController.Instance?.Level ?? 1}";
            }
        }

        public void OnRestartButtonClicked()
        {
            if (gameOverPanel != null) gameOverPanel.SetActive(false);
            GameManager.Instance?.StartNewGame();
        }
    }
}
