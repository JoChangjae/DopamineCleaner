using UnityEngine;
using UnityEngine.UI;
using DopamineCleaner.Core;
using DopamineCleaner.Enemies;

namespace DopamineCleaner.UI
{
    public class BossHealthBarUI : MonoBehaviour
    {
        public static BossHealthBarUI Instance { get; private set; }

        private GameObject barRoot;
        private CanvasGroup barGroup;
        private Slider hpSlider;
        private Image fillImage;
        private Text bossNameText;
        private Text hpRatioText;
        private Text berserkBadgeText;

        private Enemy targetEnemyBoss;
        private AuditorBoss targetAuditorBoss;
        private bool isTracking = false;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            CreateHealthBarUI();
        }

        private void Start()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnBossSpawned += HandleBossSpawned;
                GameManager.Instance.OnBossDefeated += HandleBossDefeated;
            }
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnBossSpawned -= HandleBossSpawned;
                GameManager.Instance.OnBossDefeated -= HandleBossDefeated;
            }
        }

        private void HandleBossSpawned()
        {
            // 보스 인스턴스 검색 (0.3초 뒤에 탐색하거나 즉시 탐색)
            Invoke(nameof(FindAndBindBoss), 0.2f);
        }

        private void HandleBossDefeated()
        {
            targetEnemyBoss = null;
            targetAuditorBoss = null;
            isTracking = false;
        }

        public void BindBoss(Enemy boss)
        {
            targetEnemyBoss = boss;
            isTracking = true;
            UpdateBossInfo();
        }

        public void BindAuditorBoss(AuditorBoss boss)
        {
            targetAuditorBoss = boss;
            isTracking = true;
            UpdateBossInfo();
        }

        private void FindAndBindBoss()
        {
            var auditor = FindAnyObjectByType<AuditorBoss>();
            if (auditor != null && auditor.IsAlive)
            {
                BindAuditorBoss(auditor);
                return;
            }

            var enemies = FindObjectsByType<Enemy>();
            foreach (var e in enemies)
            {
                if (e.gameObject.activeSelf && e.IsAlive && (e.Type == EnemyType.DopamineBoss || e.Type == EnemyType.AuditorKim))
                {
                    BindBoss(e);
                    return;
                }
            }
        }

        private void CreateHealthBarUI()
        {
            Canvas canvas = GetComponentInParent<Canvas>() ?? FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            barRoot = new GameObject("BossHealthBarRoot");
            barRoot.transform.SetParent(canvas.transform, false);

            RectTransform rRt = barRoot.AddComponent<RectTransform>();
            rRt.anchorMin = new Vector2(0.5f, 1f);
            rRt.anchorMax = new Vector2(0.5f, 1f);
            rRt.pivot = new Vector2(0.5f, 1f);
            rRt.sizeDelta = new Vector2(680, 75);
            rRt.anchoredPosition = new Vector2(0, -58); // 상단 HUD 바로 아래

            barGroup = barRoot.AddComponent<CanvasGroup>();
            barGroup.alpha = 0f;
            barGroup.blocksRaycasts = false;

            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            // 체력바 배경 패널
            GameObject bgObj = new GameObject("BgFrame");
            bgObj.transform.SetParent(barRoot.transform, false);
            RectTransform bgRt = bgObj.AddComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.sizeDelta = Vector2.zero;

            Image bgImg = bgObj.AddComponent<Image>();
            bgImg.color = new Color(0.10f, 0.08f, 0.16f, 0.95f);
            var outl = bgObj.AddComponent<Outline>();
            outl.effectColor = new Color(0.9f, 0.25f, 0.35f, 0.85f);
            outl.effectDistance = new Vector2(2f, -2f);

            // 상단 보스 명칭
            GameObject nameObj = new GameObject("BossName");
            nameObj.transform.SetParent(barRoot.transform, false);
            RectTransform nRt = nameObj.AddComponent<RectTransform>();
            nRt.anchorMin = new Vector2(0f, 1f);
            nRt.anchorMax = new Vector2(1f, 1f);
            nRt.sizeDelta = new Vector2(-20, 30);
            nRt.anchoredPosition = new Vector2(0, -18);

            bossNameText = nameObj.AddComponent<Text>();
            bossNameText.font = font;
            bossNameText.text = "👑 [BOSS] 김감사 부장 - 극대 도파민 변이체";
            bossNameText.fontSize = 17;
            bossNameText.fontStyle = FontStyle.Bold;
            bossNameText.color = new Color(1f, 0.85f, 0.4f);
            bossNameText.alignment = TextAnchor.MiddleLeft;

            // 광폭화 뱃지
            GameObject bzObj = new GameObject("BerserkBadge");
            bzObj.transform.SetParent(barRoot.transform, false);
            RectTransform bzRt = bzObj.AddComponent<RectTransform>();
            bzRt.anchorMin = new Vector2(0f, 1f);
            bzRt.anchorMax = new Vector2(1f, 1f);
            bzRt.sizeDelta = new Vector2(-20, 30);
            bzRt.anchoredPosition = new Vector2(0, -18);

            berserkBadgeText = bzObj.AddComponent<Text>();
            berserkBadgeText.font = font;
            berserkBadgeText.text = "🔥 [PHASE 2 BERSERK] 🔥";
            berserkBadgeText.fontSize = 16;
            berserkBadgeText.fontStyle = FontStyle.Bold;
            berserkBadgeText.color = new Color(1f, 0.3f, 0.2f);
            berserkBadgeText.alignment = TextAnchor.MiddleRight;
            berserkBadgeText.gameObject.SetActive(false);

            // 슬라이더 바
            GameObject sldObj = new GameObject("HpSlider");
            sldObj.transform.SetParent(barRoot.transform, false);
            RectTransform sldRt = sldObj.AddComponent<RectTransform>();
            sldRt.anchorMin = new Vector2(0f, 0f);
            sldRt.anchorMax = new Vector2(1f, 0f);
            sldRt.sizeDelta = new Vector2(-20, 24);
            sldRt.anchoredPosition = new Vector2(0, 18);

            hpSlider = sldObj.AddComponent<Slider>();
            hpSlider.minValue = 0f;
            hpSlider.maxValue = 1f;
            hpSlider.value = 1f;

            // 슬라이더 배경
            GameObject sBgObj = new GameObject("Background");
            sBgObj.transform.SetParent(sldObj.transform, false);
            RectTransform sbRt = sBgObj.AddComponent<RectTransform>();
            sbRt.anchorMin = Vector2.zero;
            sbRt.anchorMax = Vector2.one;
            sbRt.sizeDelta = Vector2.zero;
            Image sbImg = sBgObj.AddComponent<Image>();
            sbImg.color = new Color(0.2f, 0.15f, 0.25f, 1f);

            // 슬라이더 Fill
            GameObject fillObj = new GameObject("Fill");
            fillObj.transform.SetParent(sldObj.transform, false);
            RectTransform fRt = fillObj.AddComponent<RectTransform>();
            fRt.anchorMin = Vector2.zero;
            fRt.anchorMax = Vector2.one;
            fRt.sizeDelta = Vector2.zero;
            fillImage = fillObj.AddComponent<Image>();
            fillImage.color = new Color(0.85f, 0.2f, 0.35f); // 딥 네온 레드

            hpSlider.fillRect = fRt;

            // 체력 수치 텍스트
            GameObject ratioObj = new GameObject("HpRatioText");
            ratioObj.transform.SetParent(sldObj.transform, false);
            RectTransform rTxtRt = ratioObj.AddComponent<RectTransform>();
            rTxtRt.anchorMin = Vector2.zero;
            rTxtRt.anchorMax = Vector2.one;
            rTxtRt.sizeDelta = Vector2.zero;

            hpRatioText = ratioObj.AddComponent<Text>();
            hpRatioText.font = font;
            hpRatioText.fontSize = 15;
            hpRatioText.fontStyle = FontStyle.Bold;
            hpRatioText.color = Color.white;
            hpRatioText.alignment = TextAnchor.MiddleCenter;
            hpRatioText.text = "100.0%";
        }

        private void UpdateBossInfo()
        {
            if (bossNameText == null) return;
            int stage = GameManager.Instance != null ? GameManager.Instance.CurrentStage : 1;
            bossNameText.text = stage switch
            {
                1 => "💼 [STAGE 1 BOSS] 김감사 부장 - 극대 도파민 변이체",
                2 => "🤖 [STAGE 2 BOSS] 숏폼 알고리즘 코어",
                _ => "👑 [STAGE 3 BOSS] 절대 탐닉의 거신"
            };
        }

        private void Update()
        {
            bool hasLiveBoss = false;
            float curHp = 0f;
            float maxHp = 1f;

            if (targetEnemyBoss != null && targetEnemyBoss.gameObject.activeSelf && targetEnemyBoss.IsAlive)
            {
                hasLiveBoss = true;
                curHp = targetEnemyBoss.HP;
                maxHp = targetEnemyBoss.MaxHP;
            }
            else if (targetAuditorBoss != null && targetAuditorBoss.gameObject.activeSelf && targetAuditorBoss.IsAlive)
            {
                hasLiveBoss = true;
                curHp = targetAuditorBoss.HP;
                maxHp = targetAuditorBoss.MaxHP;
            }
            else if (isTracking)
            {
                // 보스를 잃어버렸을 경우 다시 검색
                FindAndBindBoss();
            }

            // UI 페이드 표시
            if (barGroup != null)
            {
                float targetAlpha = hasLiveBoss ? 1f : 0f;
                barGroup.alpha = Mathf.MoveTowards(barGroup.alpha, targetAlpha, Time.unscaledDeltaTime * 3.5f);
            }

            if (!hasLiveBoss) return;

            float ratio = Mathf.Clamp01(curHp / maxHp);
            if (hpSlider != null)
            {
                hpSlider.value = Mathf.MoveTowards(hpSlider.value, ratio, Time.deltaTime * 2.5f);
            }

            if (hpRatioText != null)
            {
                hpRatioText.text = $"{curHp:F0} / {maxHp:F0} ({ratio * 100f:F1}%)";
            }

            // 50% 이하 2페이즈 광폭화 연출
            bool isBerserk = ratio <= 0.5f;
            if (berserkBadgeText != null)
            {
                berserkBadgeText.gameObject.SetActive(isBerserk);
            }

            if (fillImage != null)
            {
                if (isBerserk)
                {
                    // 붉은색-주황색 펄스 점멸
                    float ping = Mathf.PingPong(Time.time * 6f, 1f);
                    fillImage.color = Color.Lerp(new Color(1f, 0.2f, 0.1f), new Color(1f, 0.65f, 0.1f), ping);
                }
                else
                {
                    fillImage.color = new Color(0.85f, 0.2f, 0.35f);
                }
            }
        }
    }
}
