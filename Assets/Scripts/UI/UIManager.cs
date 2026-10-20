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
        [SerializeField] private Text ultimateCooldownText;

        [Header("업적 달성 토스트")]
        [SerializeField] private GameObject achievementToastPanel;
        [SerializeField] private Text achievementToastText;

        [Header("세트 시너지 좌측 HUD")]
        [SerializeField] private GameObject synergyHudPanel;
        [SerializeField] private Text synergyHudText;

        [Header("피버 & 콤보 HUD")]
        [SerializeField] private Slider feverSlider;
        [SerializeField] private Text feverText;
        [SerializeField] private Text comboText;
        [SerializeField] private GameObject feverBanner;

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

        [Header("일시정지(Pause) 모달")]
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private Text pauseEquipmentText;
        [SerializeField] private Text pauseMuteBtnText;
        [SerializeField] private Button pauseWindowModeBtn;
        [SerializeField] private Text pauseWindowModeBtnText;

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
                PlayerController.Instance.OnUltimateCooldownChanged += UpdateUltimateHUD;
                UpdateUltimateHUD(PlayerController.Instance.UltimateRemainingTime, 20f);
            }

            Meta.AchievementManager.OnAchievementUnlocked += ShowAchievementToast;

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnKillsUpdated += UpdateKillsHUD;
                GameManager.Instance.OnStateChanged += HandleStateChanged;
            }

            SynergyManager.OnSynergyActivated += HandleSynergyActivated;
            EnsureSynergyHudUI();
            UpdateSynergyHUD();

            DisplayModeManager.OnDisplayModeChanged += HandleDisplayModeChanged;
            EnsureAllButtonListeners();
            UpdateWindowModeButtonText();

            if (FeverManager.Instance != null)
            {
                FeverManager.Instance.OnFeverGaugeChanged += UpdateFeverHUD;
                FeverManager.Instance.OnFeverStateChanged += HandleFeverStateChanged;
                FeverManager.Instance.OnComboChanged += UpdateComboHUD;
                UpdateFeverHUD(FeverManager.Instance.CurrentFever, FeverManager.Instance.MaxFever);
                UpdateComboHUD(FeverManager.Instance.ComboCount);
            }

            if (levelUpPanel != null) levelUpPanel.SetActive(false);
            if (gameOverPanel != null) gameOverPanel.SetActive(false);
            if (feverBanner != null) feverBanner.SetActive(false);
            if (pausePanel != null) pausePanel.SetActive(false);

            // HUD 텍스트 시인성 극대화 (골드/민트 색상 + 블랙 아웃라인)
            if (timerText != null)
            {
                timerText.color = new Color(1f, 0.88f, 0.32f); // 화사한 골드 옐로우
                if (!timerText.TryGetComponent<Outline>(out _))
                {
                    var outl = timerText.gameObject.AddComponent<Outline>();
                    outl.effectColor = new Color(0f, 0f, 0f, 0.95f);
                    outl.effectDistance = new Vector2(2f, -2f);
                }
            }

            if (stageKillsText != null)
            {
                stageKillsText.color = new Color(0.40f, 0.95f, 0.75f); // 산뜻한 네온 민트
                if (!stageKillsText.TryGetComponent<Outline>(out _))
                {
                    var outl = stageKillsText.gameObject.AddComponent<Outline>();
                    outl.effectColor = new Color(0f, 0f, 0f, 0.95f);
                    outl.effectDistance = new Vector2(1.5f, -1.5f);
                }
            }

            if (levelText != null)
            {
                levelText.color = new Color(1f, 0.78f, 0.35f); // 웜 피치
                if (!levelText.TryGetComponent<Outline>(out _))
                {
                    var outl = levelText.gameObject.AddComponent<Outline>();
                    outl.effectColor = new Color(0f, 0f, 0f, 0.95f);
                    outl.effectDistance = new Vector2(1.5f, -1.5f);
                }
            }

            if (dashCooldownText != null)
            {
                dashCooldownText.color = new Color(0.70f, 0.90f, 1f); // 아이스 시안
                if (!dashCooldownText.TryGetComponent<Outline>(out _))
                {
                    var outl = dashCooldownText.gameObject.AddComponent<Outline>();
                    outl.effectColor = new Color(0f, 0f, 0f, 0.95f);
                    outl.effectDistance = new Vector2(1.5f, -1.5f);
                }
            }

            if (ultimateCooldownText != null)
            {
                ultimateCooldownText.color = new Color(1f, 0.9f, 0.25f); // 골드 네온
                if (!ultimateCooldownText.TryGetComponent<Outline>(out _))
                {
                    var outl = ultimateCooldownText.gameObject.AddComponent<Outline>();
                    outl.effectColor = new Color(0f, 0f, 0f, 0.95f);
                    outl.effectDistance = new Vector2(1.5f, -1.5f);
                }
            }
        }

        private void OnDestroy()
        {
            if (PlayerController.Instance != null)
            {
                PlayerController.Instance.OnHealthChanged -= UpdateHealthHUD;
                PlayerController.Instance.OnExpChanged -= UpdateExpHUD;
                PlayerController.Instance.OnLevelUp -= ShowLevelUpUI;
                PlayerController.Instance.OnDashCooldownChanged -= UpdateDashHUD;
                PlayerController.Instance.OnUltimateCooldownChanged -= UpdateUltimateHUD;
            }
            Meta.AchievementManager.OnAchievementUnlocked -= ShowAchievementToast;
            SynergyManager.OnSynergyActivated -= HandleSynergyActivated;
            DisplayModeManager.OnDisplayModeChanged -= HandleDisplayModeChanged;

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnKillsUpdated -= UpdateKillsHUD;
                GameManager.Instance.OnStateChanged -= HandleStateChanged;
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (DopamineCleaner.Core.LuckySlotVendingMachine.Instance != null &&
                    DopamineCleaner.Core.LuckySlotVendingMachine.Instance.IsModalOpen)
                {
                    DopamineCleaner.Core.LuckySlotVendingMachine.Instance.CloseSlotModal();
                    return;
                }

                TogglePauseMenu();
            }

            if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Playing)
            {
                int totalSec = Mathf.FloorToInt(GameManager.Instance.SurvivalTime);
                if (timerText != null)
                {
                    timerText.text = $"{totalSec / 60:D2}:{totalSec % 60:D2}";
                }
            }
        }

        private void UpdateFeverHUD(float current, float max)
        {
            if (feverSlider != null)
            {
                feverSlider.maxValue = max;
                feverSlider.value = current;
            }

            if (feverText != null)
            {
                if (FeverManager.Instance != null && FeverManager.Instance.IsFever)
                {
                    feverText.text = "🔥 FEVER TIME! (연사속도 2배!) 🔥";
                    feverText.color = new Color(1f, 0.45f, 0.2f);
                }
                else
                {
                    int pct = Mathf.RoundToInt((current / Mathf.Max(1f, max)) * 100f);
                    feverText.text = $"⚡ 디톡스 피버 게이지: {pct}%";
                    feverText.color = new Color(0.35f, 0.85f, 1f);
                }
            }
        }

        private void HandleFeverStateChanged(bool isFever)
        {
            if (feverBanner != null)
            {
                feverBanner.SetActive(isFever);
            }
            if (feverSlider != null)
            {
                var fill = feverSlider.fillRect?.GetComponent<Image>();
                if (fill != null)
                {
                    fill.color = isFever ? new Color(1f, 0.45f, 0.2f) : new Color(0.35f, 0.85f, 1f);
                }
            }
        }

        private void UpdateComboHUD(int combo)
        {
            if (comboText != null)
            {
                if (combo > 1)
                {
                    comboText.gameObject.SetActive(true);
                    comboText.text = $"⚡ {combo} COMBO!";
                }
                else
                {
                    comboText.gameObject.SetActive(false);
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
                    stageKillsText.text = $"⚠️ STAGE {GameManager.Instance.CurrentStage} BOSS 출현! (👾 {totalKills})";
                    stageKillsText.color = new Color(1f, 0.35f, 0.45f); // 보스 경고 네온 레드
                }
                else
                {
                    stageKillsText.text = $"STAGE {GameManager.Instance.CurrentStage} [{stageKills}/{GameManager.Instance.StageTargetKills}]  👾 {totalKills}";
                    stageKillsText.color = new Color(0.40f, 0.95f, 0.75f); // 일반 스테이지 네온 민트
                }
            }
        }

        private void UpdateDashHUD(float currentCooldown, float maxCooldown)
        {
            if (dashCooldownText != null)
            {
                string dashStr = currentCooldown <= 0f ? "💨 대시 준비완료 (Space)" : $"⏳ 대시 ({currentCooldown:F1}s)";

                int aidCount = 0;
                if (Meta.GameDataManager.Instance != null)
                {
                    foreach (string id in Meta.GameDataManager.Instance.Profile.inventory)
                    {
                        if (id == "item_first_aid") aidCount++;
                    }
                }

                if (aidCount > 0)
                {
                    dashCooldownText.text = $"{dashStr}  |  🩹 구급약 [1] (보유: {aidCount}개)";
                }
                else
                {
                    dashCooldownText.text = dashStr;
                }
            }
        }

        private void UpdateUltimateHUD(float currentCooldown, float maxCooldown)
        {
            if (ultimateCooldownText != null && PlayerController.Instance != null)
            {
                if (currentCooldown <= 0f)
                {
                    ultimateCooldownText.text = $"⚡ [Q] {PlayerController.Instance.UltimateSkillName} [READY!]";
                    ultimateCooldownText.color = new Color(1f, 0.9f, 0.2f);
                }
                else
                {
                    ultimateCooldownText.text = $"⏳ [Q] {PlayerController.Instance.UltimateSkillName} ({currentCooldown:F1}s)";
                    ultimateCooldownText.color = new Color(0.7f, 0.75f, 0.85f);
                }
            }
        }

        public void ShowAchievementToast(Meta.AchievementInfo ach)
        {
            if (achievementToastPanel == null || achievementToastText == null) return;
            achievementToastText.text = $"🏆 [업적 달성!] {ach.title}\n{ach.description} (+{ach.goldReward}G)";
            achievementToastPanel.SetActive(true);
            CancelInvoke(nameof(HideAchievementToast));
            Invoke(nameof(HideAchievementToast), 3.5f);
        }

        private void HideAchievementToast()
        {
            if (achievementToastPanel != null) achievementToastPanel.SetActive(false);
        }

        private void EnsureSynergyHudUI()
        {
            if (synergyHudPanel == null)
            {
                var canvas = GetComponentInParent<Canvas>();
                if (canvas != null)
                {
                    synergyHudPanel = new GameObject("SynergyHUDPanel");
                    synergyHudPanel.transform.SetParent(canvas.transform, false);

                    var rt = synergyHudPanel.AddComponent<RectTransform>();
                    rt.anchorMin = new Vector2(0f, 1f);
                    rt.anchorMax = new Vector2(0f, 1f);
                    rt.pivot = new Vector2(0f, 1f);
                    rt.sizeDelta = new Vector2(320, 130);
                    rt.anchoredPosition = new Vector2(25, -230);

                    var img = synergyHudPanel.AddComponent<Image>();
                    img.color = new Color(0.10f, 0.08f, 0.16f, 0.85f);

                    var txtObj = new GameObject("SynergyText");
                    txtObj.transform.SetParent(synergyHudPanel.transform, false);
                    synergyHudText = txtObj.AddComponent<Text>();
                    synergyHudText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                    synergyHudText.fontSize = 15;
                    synergyHudText.color = new Color(0.4f, 0.95f, 1f);
                    synergyHudText.lineSpacing = 1.15f;
                    synergyHudText.alignment = TextAnchor.UpperLeft;

                    var txtRt = txtObj.GetComponent<RectTransform>();
                    txtRt.anchorMin = Vector2.zero;
                    txtRt.anchorMax = Vector2.one;
                    txtRt.offsetMin = new Vector2(12, 10);
                    txtRt.offsetMax = new Vector2(-12, -10);

                    var outl = txtObj.AddComponent<Outline>();
                    outl.effectColor = new Color(0f, 0f, 0f, 0.95f);
                    outl.effectDistance = new Vector2(1f, -1f);
                }
            }
        }

        public void HandleSynergyActivated(SynergyInfo info)
        {
            UpdateSynergyHUD();

            if (achievementToastPanel != null && achievementToastText != null)
            {
                achievementToastText.text = $"✨ [세트 시너지 개화!] {info.title}\n{info.description}";
                achievementToastPanel.SetActive(true);
                CancelInvoke(nameof(HideAchievementToast));
                Invoke(nameof(HideAchievementToast), 4.0f);
            }
        }

        public void UpdateSynergyHUD()
        {
            if (SynergyManager.Instance == null)
            {
                if (synergyHudPanel != null) synergyHudPanel.SetActive(false);
                return;
            }

            var activeList = SynergyManager.Instance.GetActiveSynergies();
            if (activeList == null || activeList.Count == 0)
            {
                if (synergyHudPanel != null) synergyHudPanel.SetActive(false);
                return;
            }

            if (synergyHudPanel != null) synergyHudPanel.SetActive(true);

            if (synergyHudText != null)
            {
                var sb = new System.Text.StringBuilder();
                sb.AppendLine("<color=#FFE082>✨ [활성 세트 시너지]</color>");
                foreach (var syn in activeList)
                {
                    sb.AppendLine($"{syn.icon} <color=#64FFDA>{syn.title}</color>");
                }
                synergyHudText.text = sb.ToString().TrimEnd();
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
                List<UpgradeCardInfo> options = WeaponManager.Instance != null
                    ? WeaponManager.Instance.GetRandomUpgradeOptions(3)
                    : new List<UpgradeCardInfo>();

                if (options == null || options.Count == 0)
                {
                    // 후보가 전혀 없을 때 게임 정지 먹통 방지 (즉시 플레이 재개)
                    pendingLevelUps = 0;
                    levelUpPanel.SetActive(false);
                    GameManager.Instance?.SetState(GameState.Playing);
                    return;
                }

                // 만렙 보너스 카드인 경우 타이틀 변경
                if (options.Count > 0 && (options[0].type == UpgradeType.BonusGold || options[0].type == UpgradeType.BonusHeal || options[0].type == UpgradeType.BonusBomb))
                {
                    if (levelUpTitleText != null) levelUpTitleText.text = "👑 만렙 달성! 특별 보너스";
                    if (levelUpSubText != null) levelUpSubText.text = "모든 능력을 마스터했습니다! 원하는 성과급을 선택하세요.";
                }

                int spawnedCount = 0;
                for (int i = 0; i < options.Count; i++)
                {
                    UpgradeCardInfo opt = options[i];
                    GameObject cardObj = null;

                    if (cardPrefab != null)
                    {
                        cardObj = Instantiate(cardPrefab, cardContainer);
                    }
                    else
                    {
                        // 프리팹 누락 시 동적 폴백 카드 생성
                        cardObj = CreateFallbackCard(cardContainer, opt);
                    }

                    if (cardObj != null)
                    {
                        UpgradeCardUI cardUI = cardObj.GetComponent<UpgradeCardUI>();
                        if (cardUI != null)
                        {
                            spawnedCount++;
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

                // 만약 생성된 카드가 0개라면 절대 멈추지 않고 즉시 닫고 플레이 재개
                if (spawnedCount == 0)
                {
                    pendingLevelUps = 0;
                    levelUpPanel.SetActive(false);
                    GameManager.Instance?.SetState(GameState.Playing);
                }
            }
            else
            {
                // cardContainer가 없더라도 먹통 방지
                pendingLevelUps = 0;
                levelUpPanel.SetActive(false);
                GameManager.Instance?.SetState(GameState.Playing);
            }
        }

        public void OnSkipLevelUpClicked()
        {
            pendingLevelUps = 0;
            if (levelUpPanel != null) levelUpPanel.SetActive(false);
            GameManager.Instance?.SetState(GameState.Playing);
        }

        private GameObject CreateFallbackCard(Transform parent, UpgradeCardInfo info)
        {
            GameObject cardObj = new GameObject("FallbackCard");
            cardObj.transform.SetParent(parent, false);
            RectTransform rt = cardObj.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(280, 420);

            Image img = cardObj.AddComponent<Image>();
            img.color = new Color(0.96f, 0.94f, 0.98f);

            Button btn = cardObj.AddComponent<Button>();
            UpgradeCardUI cardUI = cardObj.AddComponent<UpgradeCardUI>();

            Text icon = CreateFallbackText("Icon", cardObj.transform, info.iconText, 48, new Vector2(0, 90), new Vector2(100, 80));
            Text title = CreateFallbackText("Title", cardObj.transform, info.title, 20, new Vector2(0, 20), new Vector2(260, 40));
            title.color = new Color(0.15f, 0.12f, 0.22f);
            title.fontStyle = FontStyle.Bold;

            Text desc = CreateFallbackText("Desc", cardObj.transform, info.description, 15, new Vector2(0, -80), new Vector2(240, 120));
            desc.color = new Color(0.35f, 0.3f, 0.4f);

            var fIcon = typeof(UpgradeCardUI).GetField("iconText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            fIcon?.SetValue(cardUI, icon);
            var fTitle = typeof(UpgradeCardUI).GetField("titleText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            fTitle?.SetValue(cardUI, title);
            var fDesc = typeof(UpgradeCardUI).GetField("descriptionText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            fDesc?.SetValue(cardUI, desc);
            var fBtn = typeof(UpgradeCardUI).GetField("selectButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            fBtn?.SetValue(cardUI, btn);

            return cardObj;
        }

        private Text CreateFallbackText(string name, Transform parent, string content, int fontSize, Vector2 pos, Vector2 size)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            RectTransform rt = obj.AddComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            Text txt = obj.AddComponent<Text>();
            txt.text = content;
            txt.fontSize = fontSize;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.font = Font.CreateDynamicFontFromOSFont(new string[] { "Malgun Gothic", "맑은 고딕", "Arial" }, fontSize);
            return txt;
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
            int totalKills = GameManager.Instance.TotalKills;
            int gold = GameManager.Instance.EarnedGold;
            int karma = GameManager.Instance.EarnedKarma;

            // 1. 디톡스 등급 산정
            string grade;
            Color gradeColor;
            if (isVictory || (sec >= 300 && totalKills >= 350))
            {
                grade = "👑 S등급 (디톡스 마스터)";
                gradeColor = new Color(1f, 0.85f, 0.25f);
            }
            else if (sec >= 180 && totalKills >= 180)
            {
                grade = "🌟 A등급 (우수 멘탈 요원)";
                gradeColor = new Color(0.4f, 0.95f, 0.65f);
            }
            else if (sec >= 90)
            {
                grade = "🌱 B등급 (수습 디톡스 요원)";
                gradeColor = new Color(0.35f, 0.8f, 1f);
            }
            else
            {
                grade = "📱 C등급 (도파민 과부하 주의)";
                gradeColor = new Color(1f, 0.45f, 0.45f);
            }

            Text titleText = gameOverPanel.transform.Find("GoTitle")?.GetComponent<Text>();
            if (titleText != null)
            {
                titleText.text = isVictory ? "📋 도파민 디톡스 종합 검진표 (작전 완료)" : "📋 도파민 디톡스 종합 검진표 (작전 종료)";
                titleText.color = gradeColor;
            }

            // 2. 무기별 DPS 통계 텍스트 구성
            string weaponReport = "⚔️ [무기별 정화 기여도 (DPS)]\n";
            if (WeaponManager.Instance != null)
            {
                var stats = WeaponManager.Instance.GetDamageStatistics();
                if (stats.Count > 0)
                {
                    foreach (var ws in stats)
                    {
                        int barLength = Mathf.RoundToInt(ws.percentage / 10f);
                        string bar = new string('■', barLength) + new string('□', Mathf.Max(0, 10 - barLength));
                        weaponReport += $"{ws.icon} {ws.name,-12}: {bar} {ws.damage:N0} ({ws.percentage:F1}%)\n";
                    }
                }
                else
                {
                    weaponReport += "  - 기록된 무기 대미지 데이터 없음\n";
                }
            }

            int savedShorts = Mathf.Max(1, totalKills / 4);

            if (gameOverStatsText != null)
            {
                gameOverStatsText.text =
                    $"【 종합 진단 결과: {grade} 】\n" +
                    $"─────────────────────────────────────\n" +
                    $"⏱️ 사수한 집중 시간: {sec / 60:D2}:{sec % 60:D2}  |  👾 소독한 잡생각: {totalKills}마리\n" +
                    $"📱 도파민 절약 효과: 오늘 숏폼 {savedShorts}편 시청 방지 효과 달성!\n" +
                    $"💰 정산 지원금: +{gold:N0} G   |   ✨ 디톡스 포인트: +{karma} P\n" +
                    $"─────────────────────────────────────\n" +
                    $"{weaponReport}";
            }
        }

        public void OnRestartButtonClicked()
        {
            if (gameOverPanel != null) gameOverPanel.SetActive(false);
            Time.timeScale = 1f;
            UnityEngine.SceneManagement.SceneManager.LoadScene("TownScene");
        }

        public void TogglePauseMenu()
        {
            if (GameManager.Instance == null) return;

            // 게임오버나 레벨업 중에는 일시정지 불가
            if (GameManager.Instance.CurrentState == GameState.GameOver || GameManager.Instance.CurrentState == GameState.LevelUp)
                return;

            if (GameManager.Instance.CurrentState == GameState.Paused)
            {
                ResumeGame();
            }
            else if (GameManager.Instance.CurrentState == GameState.Playing)
            {
                PauseGame();
            }
        }

        public void PauseGame()
        {
            if (pausePanel == null || GameManager.Instance == null) return;

            GameManager.Instance.SetState(GameState.Paused);
            pausePanel.SetActive(true);

            if (pauseEquipmentText != null && WeaponManager.Instance != null)
            {
                pauseEquipmentText.text = WeaponManager.Instance.GetEquipmentSummary();
            }

            UpdateMuteButtonText();
            UpdateWindowModeButtonText();
        }

        public void ResumeGame()
        {
            SoundManager.Instance?.Play(SoundEffect.Dash, 0.4f);
            if (pausePanel != null) pausePanel.SetActive(false);
            GameManager.Instance?.SetState(GameState.Playing);
        }

        public void OnToggleMuteClicked()
        {
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.ToggleMute();
                UpdateMuteButtonText();
            }
        }

        private void UpdateMuteButtonText()
        {
            if (pauseMuteBtnText != null && SoundManager.Instance != null)
            {
                pauseMuteBtnText.text = SoundManager.Instance.IsMuted ? "🔈 효과음: OFF (음소거)" : "🔊 효과음: ON";
            }
        }

        public void OnToggleWindowModeClicked()
        {
            DisplayModeManager.Instance?.CycleDisplayMode();
            UpdateWindowModeButtonText();
        }

        private void HandleDisplayModeChanged(bool isWindowed)
        {
            UpdateWindowModeButtonText();
        }

        private void UpdateWindowModeButtonText()
        {
            if (pauseWindowModeBtnText != null && DisplayModeManager.Instance != null)
            {
                pauseWindowModeBtnText.text = DisplayModeManager.Instance.GetDisplayModeLabel();
            }
        }

        private void EnsureAllButtonListeners()
        {
            if (pausePanel == null) pausePanel = GameObject.Find("PausePanel");
            if (gameOverPanel == null) gameOverPanel = GameObject.Find("GameOverPanel");
            if (levelUpPanel == null) levelUpPanel = GameObject.Find("LevelUpPanel");

            EnsurePauseWindowModeButton();

            // 1. 일시정지 패널 버튼들
            if (pausePanel != null)
            {
                var resumeBtn = pausePanel.transform.Find("ResumeButton")?.GetComponent<Button>();
                if (resumeBtn != null)
                {
                    resumeBtn.onClick.RemoveAllListeners();
                    resumeBtn.onClick.AddListener(ResumeGame);
                }

                var retreatBtn = pausePanel.transform.Find("RetreatButton")?.GetComponent<Button>();
                if (retreatBtn != null)
                {
                    retreatBtn.onClick.RemoveAllListeners();
                    retreatBtn.onClick.AddListener(OnRetreatButtonClicked);
                }

                var muteBtn = pausePanel.transform.Find("MuteButton")?.GetComponent<Button>();
                if (muteBtn != null)
                {
                    muteBtn.onClick.RemoveAllListeners();
                    muteBtn.onClick.AddListener(OnToggleMuteClicked);
                }

                EnsurePauseSettingsButton();
            }

            // 2. 게임오버 패널 버튼들
            if (gameOverPanel != null)
            {
                var restartBtn = gameOverPanel.transform.Find("RestartButton")?.GetComponent<Button>();
                if (restartBtn != null)
                {
                    restartBtn.onClick.RemoveAllListeners();
                    restartBtn.onClick.AddListener(OnRestartButtonClicked);
                }

                EnsureSaveReportCardButton();
            }

            // 3. 레벨업 패널 비상 건너뛰기 버튼
            if (levelUpPanel != null)
            {
                var skipBtn = levelUpPanel.transform.Find("SkipButton")?.GetComponent<Button>();
                if (skipBtn != null)
                {
                    skipBtn.onClick.RemoveAllListeners();
                    skipBtn.onClick.AddListener(OnSkipLevelUpClicked);
                }
            }
        }

        private void EnsurePauseSettingsButton()
        {
            if (pausePanel == null) return;
            var settingsBtn = pausePanel.transform.Find("SettingsButton")?.GetComponent<Button>();
            if (settingsBtn == null)
            {
                GameObject sBtnObj = new GameObject("SettingsButton");
                sBtnObj.transform.SetParent(pausePanel.transform, false);
                RectTransform sRt = sBtnObj.AddComponent<RectTransform>();
                sRt.anchoredPosition = new Vector2(0, -90);
                sRt.sizeDelta = new Vector2(280, 42);

                Image sImg = sBtnObj.AddComponent<Image>();
                sImg.color = new Color(0.24f, 0.28f, 0.45f, 0.95f);

                settingsBtn = sBtnObj.AddComponent<Button>();
                var outl = sBtnObj.AddComponent<Outline>();
                outl.effectColor = new Color(0f, 0f, 0f, 0.8f);
                outl.effectDistance = new Vector2(1.5f, -1.5f);

                GameObject txtObj = new GameObject("Text");
                txtObj.transform.SetParent(sBtnObj.transform, false);
                RectTransform txtRt = txtObj.AddComponent<RectTransform>();
                txtRt.anchorMin = Vector2.zero;
                txtRt.anchorMax = Vector2.one;
                txtRt.sizeDelta = Vector2.zero;

                Text t = txtObj.AddComponent<Text>();
                t.text = "⚙️ 환경설정 (오디오/화면)";
                t.fontSize = 16;
                t.alignment = TextAnchor.MiddleCenter;
                t.color = Color.white;
                t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }

            if (settingsBtn != null)
            {
                settingsBtn.onClick.RemoveAllListeners();
                settingsBtn.onClick.AddListener(() => SettingsModalUI.OpenSettings());
            }
        }

        private void EnsureSaveReportCardButton()
        {
            if (gameOverPanel == null) return;
            Transform existing = gameOverPanel.transform.Find("SaveCardButton");
            Button saveBtn;
            Text btnText;
            if (existing == null)
            {
                GameObject btnObj = new GameObject("SaveCardButton");
                btnObj.transform.SetParent(gameOverPanel.transform, false);
                RectTransform rt = btnObj.AddComponent<RectTransform>();
                rt.anchoredPosition = new Vector2(0, -220);
                rt.sizeDelta = new Vector2(340, 48);

                Image img = btnObj.AddComponent<Image>();
                img.color = new Color(0.18f, 0.45f, 0.35f, 0.95f);

                saveBtn = btnObj.AddComponent<Button>();
                var outl = btnObj.AddComponent<Outline>();
                outl.effectColor = new Color(0f, 0f, 0f, 0.85f);
                outl.effectDistance = new Vector2(1.5f, -1.5f);

                GameObject txtObj = new GameObject("Text");
                txtObj.transform.SetParent(btnObj.transform, false);
                RectTransform txtRt = txtObj.AddComponent<RectTransform>();
                txtRt.anchorMin = Vector2.zero;
                txtRt.anchorMax = Vector2.one;
                txtRt.sizeDelta = Vector2.zero;

                btnText = txtObj.AddComponent<Text>();
                btnText.text = "📸 [검진표 카드 바탕화면 저장]";
                btnText.fontSize = 16;
                btnText.alignment = TextAnchor.MiddleCenter;
                btnText.color = Color.white;
                btnText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }
            else
            {
                saveBtn = existing.GetComponent<Button>();
                btnText = existing.GetComponentInChildren<Text>();
            }

            if (saveBtn != null)
            {
                saveBtn.onClick.RemoveAllListeners();
                saveBtn.onClick.AddListener(() =>
                {
                    SoundManager.Instance?.Play(SoundEffect.ButtonClick, 1.0f);
                    string savedPath = DetoxCardExporter.ExportDetoxReportCard();
                    if (btnText != null)
                    {
                        btnText.text = "✅ [바탕화면에 저장 완료!]";
                        btnText.color = new Color(0.4f, 1f, 0.6f);
                    }
                });
            }
        }

        private void EnsurePauseWindowModeButton()
        {
            if (pauseWindowModeBtn == null && pausePanel != null)
            {
                var existingBtn = pausePanel.transform.Find("WindowModeButton")?.GetComponent<Button>();
                if (existingBtn != null)
                {
                    pauseWindowModeBtn = existingBtn;
                    pauseWindowModeBtnText = existingBtn.GetComponentInChildren<Text>();
                }
            }

            if (pauseWindowModeBtn != null)
            {
                pauseWindowModeBtn.onClick.RemoveAllListeners();
                pauseWindowModeBtn.onClick.AddListener(OnToggleWindowModeClicked);
            }
        }

        public void OnRetreatButtonClicked()
        {
            SoundManager.Instance?.Play(SoundEffect.Dash, 0.5f);
            if (pausePanel != null) pausePanel.SetActive(false);
            GameManager.Instance?.TriggerGameOver(false);
        }
    }
}
