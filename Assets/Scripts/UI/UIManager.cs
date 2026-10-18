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
        [SerializeField] private Text levelUpTitleText;
        [SerializeField] private Text levelUpSubText;
        [SerializeField] private Transform cardContainer;
        [SerializeField] private GameObject cardPrefab;

        private int pendingLevelUps = 0;

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

        public void ShowLevelUpUI(int newLevel)
        {
            pendingLevelUps++;
            if (levelUpPanel != null && !levelUpPanel.activeSelf)
            {
                PresentNextLevelUp(false);
            }
        }

        public void ShowChestRewardUI()
        {
            pendingLevelUps++;
            if (levelUpPanel != null && !levelUpPanel.activeSelf)
            {
                PresentNextLevelUp(true);
            }
        }

        private void PresentNextLevelUp(bool isChest = false)
        {
            if (levelUpPanel == null) return;

            if (pendingLevelUps <= 0)
            {
                levelUpPanel.SetActive(false);
                GameManager.Instance?.SetState(GameState.Playing);
                return;
            }

            GameManager.Instance?.SetState(GameState.LevelUp);
            levelUpPanel.SetActive(true);

            if (levelUpTitleText != null)
            {
                levelUpTitleText.text = isChest ? "🎁 황금 보물상자 발견!" : "🎉 레벨 업!";
            }

            if (levelUpSubText != null)
            {
                levelUpSubText.text = isChest ? "행운의 보너스 능력을 선택하세요!" : "원하는 도파민 디톡스 능력을 선택하세요";
            }

            // 기존 카드 정리
            if (cardContainer != null)
            {
                foreach (Transform child in cardContainer)
                {
                    Destroy(child.gameObject);
                }

                // 3개 옵션 생성 (보물상자면 진화 무기 우선 배치)
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
                                pendingLevelUps--;

                                if (pendingLevelUps > 0)
                                {
                                    PresentNextLevelUp(false);
                                }
                                else
                                {
                                    levelUpPanel.SetActive(false);
                                    GameManager.Instance?.SetState(GameState.Playing);
                                }
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
            bool isVictory = GameManager.Instance.IsVictory;
            int gold = GameManager.Instance.EarnedGold;
            int karma = GameManager.Instance.EarnedKarma;

            Text titleText = gameOverPanel.transform.Find("GoTitle")?.GetComponent<Text>();
            if (titleText != null)
            {
                titleText.text = isVictory ? "🏆 출장 임무 완료 (생존)" : "💀 현장 순직 (사후 보험금 정산)";
                titleText.color = isVictory ? new Color(1f, 0.85f, 0.25f) : new Color(1f, 0.4f, 0.45f);
            }

            if (gameOverStatsText != null)
            {
                gameOverStatsText.text =
                    $"출장 현장: {Meta.DungeonDatabase.Get(Meta.GameDataManager.Instance?.SelectedMapId ?? "map_hospital").name}\n" +
                    $"생존 시간: {sec / 60:D2}:{sec % 60:D2}  |  처치: {GameManager.Instance.TotalKills}마리\n\n" +
                    $"[정산 회수 내역]\n" +
                    $"💰 수령 보험금: +{gold:N0} G\n" +
                    $"✨ 윤회 포인트: +{karma} P";
            }
        }

        public void OnRestartButtonClicked()
        {
            if (gameOverPanel != null) gameOverPanel.SetActive(false);
            Time.timeScale = 1f;
            UnityEngine.SceneManagement.SceneManager.LoadScene("TownScene");
        }
    }
}
