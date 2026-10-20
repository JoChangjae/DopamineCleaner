using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DopamineCleaner.Core;

namespace DopamineCleaner.UI
{
    public class BossWarningUI : MonoBehaviour
    {
        public static BossWarningUI Instance { get; private set; }

        private GameObject warningRoot;
        private CanvasGroup warningGroup;
        private Text titleText;
        private Text subText;
        private Image vignetteImage;
        private RectTransform topStripeRt;
        private RectTransform bottomStripeRt;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            CreateWarningUI();
        }

        private void Start()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnBossSpawned += HandleBossSpawned;
            }
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnBossSpawned -= HandleBossSpawned;
            }
        }

        private void HandleBossSpawned()
        {
            int stage = GameManager.Instance != null ? GameManager.Instance.CurrentStage : 1;
            string bossName = stage switch
            {
                1 => "💼 [본사 감사실] 김감사 부장",
                2 => "🤖 [알고리즘 폭주체] 숏폼 알고리즘 코어",
                _ => "👑 [도파민 과다 분비체] 절대 탐닉의 거신"
            };

            string bossQuote = stage switch
            {
                1 => "“업무 중 딴짓 300회 적발! 즉각 시말서를 작성하라!”",
                2 => "“무한 스크롤 루프 가동… 현실로 돌아갈 수 없다…”",
                _ => "“도파민 폭포수를 맞이하라! 영원한 쾌락 속에 잠들라!”"
            };

            ShowWarning(bossName, bossQuote);
        }

        public void ShowWarning(string name, string quote)
        {
            StopAllCoroutines();
            StartCoroutine(WarningRoutine(name, quote));
        }

        private void CreateWarningUI()
        {
            Canvas canvas = GetComponentInParent<Canvas>() ?? FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            warningRoot = new GameObject("BossWarningRoot");
            warningRoot.transform.SetParent(canvas.transform, false);
            warningRoot.transform.SetAsLastSibling();

            RectTransform rRt = warningRoot.AddComponent<RectTransform>();
            rRt.anchorMin = Vector2.zero;
            rRt.anchorMax = Vector2.one;
            rRt.sizeDelta = Vector2.zero;

            warningGroup = warningRoot.AddComponent<CanvasGroup>();
            warningGroup.alpha = 0f;
            warningGroup.blocksRaycasts = false;

            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            // 붉은 비네트 펄스 배경
            GameObject vigObj = new GameObject("Vignette");
            vigObj.transform.SetParent(warningRoot.transform, false);
            RectTransform vRt = vigObj.AddComponent<RectTransform>();
            vRt.anchorMin = Vector2.zero;
            vRt.anchorMax = Vector2.one;
            vRt.sizeDelta = Vector2.zero;
            vignetteImage = vigObj.AddComponent<Image>();
            vignetteImage.color = new Color(1f, 0.05f, 0.05f, 0f);

            // 상단 경고 테이프 스트라이프
            GameObject topObj = new GameObject("TopStripe");
            topObj.transform.SetParent(warningRoot.transform, false);
            topStripeRt = topObj.AddComponent<RectTransform>();
            topStripeRt.anchorMin = new Vector2(0f, 1f);
            topStripeRt.anchorMax = new Vector2(1f, 1f);
            topStripeRt.pivot = new Vector2(0.5f, 1f);
            topStripeRt.sizeDelta = new Vector2(0, 42);
            topStripeRt.anchoredPosition = new Vector2(0, 0);

            Image topImg = topObj.AddComponent<Image>();
            topImg.color = new Color(0.9f, 0.15f, 0.15f, 0.95f);
            Text topTxt = CreateStripeText(topObj, font, "⚠ WARNING ⚠ DANGER ⚠ WARNING ⚠ DANGER ⚠ WARNING ⚠ DANGER ⚠ WARNING ⚠");

            // 하단 경고 테이프 스트라이프
            GameObject btmObj = new GameObject("BottomStripe");
            btmObj.transform.SetParent(warningRoot.transform, false);
            bottomStripeRt = btmObj.AddComponent<RectTransform>();
            bottomStripeRt.anchorMin = new Vector2(0f, 0f);
            bottomStripeRt.anchorMax = new Vector2(1f, 0f);
            bottomStripeRt.pivot = new Vector2(0.5f, 0f);
            bottomStripeRt.sizeDelta = new Vector2(0, 42);
            bottomStripeRt.anchoredPosition = new Vector2(0, 0);

            Image btmImg = btmObj.AddComponent<Image>();
            btmImg.color = new Color(0.9f, 0.15f, 0.15f, 0.95f);
            Text btmTxt = CreateStripeText(btmObj, font, "⚠ CRITICAL HAZARD ⚠ HIGH VOLTAGE ⚠ DOPAMINE ANOMALY ⚠ CRITICAL HAZARD ⚠");

            // 중앙 경고 배너 박스
            GameObject bannerObj = new GameObject("BannerBox");
            bannerObj.transform.SetParent(warningRoot.transform, false);
            RectTransform bRt = bannerObj.AddComponent<RectTransform>();
            bRt.anchorMin = new Vector2(0.5f, 0.5f);
            bRt.anchorMax = new Vector2(0.5f, 0.5f);
            bRt.sizeDelta = new Vector2(1000, 220);
            bRt.anchoredPosition = new Vector2(0, 40);

            Image bannerBg = bannerObj.AddComponent<Image>();
            bannerBg.color = new Color(0.08f, 0.05f, 0.12f, 0.95f);
            var outl = bannerObj.AddComponent<Outline>();
            outl.effectColor = new Color(1f, 0.2f, 0.2f, 0.9f);
            outl.effectDistance = new Vector2(3f, -3f);

            // 긴급 경보 상단 헤더
            GameObject hObj = new GameObject("EmgHeader");
            hObj.transform.SetParent(bannerObj.transform, false);
            RectTransform hRt = hObj.AddComponent<RectTransform>();
            hRt.anchorMin = new Vector2(0f, 1f);
            hRt.anchorMax = new Vector2(1f, 1f);
            hRt.sizeDelta = new Vector2(0, 45);
            hRt.anchoredPosition = new Vector2(0, -28);

            Text hTxt = hObj.AddComponent<Text>();
            hTxt.font = font;
            hTxt.text = "🚨 EMERGENCY HAZARD DETECTED 🚨";
            hTxt.fontSize = 24;
            hTxt.fontStyle = FontStyle.Bold;
            hTxt.alignment = TextAnchor.MiddleCenter;
            hTxt.color = new Color(1f, 0.3f, 0.3f);

            // 보스 명칭 텍스트
            GameObject tObj = new GameObject("BossTitle");
            tObj.transform.SetParent(bannerObj.transform, false);
            RectTransform tRt = tObj.AddComponent<RectTransform>();
            tRt.anchorMin = new Vector2(0.5f, 0.5f);
            tRt.anchorMax = new Vector2(0.5f, 0.5f);
            tRt.sizeDelta = new Vector2(950, 60);
            tRt.anchoredPosition = new Vector2(0, 5);

            titleText = tObj.AddComponent<Text>();
            titleText.font = font;
            titleText.text = "💼 [본사 감사실] 김감사 부장 출현!";
            titleText.fontSize = 32;
            titleText.fontStyle = FontStyle.Bold;
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.color = new Color(1f, 0.95f, 0.4f);

            // 대사/서브 텍스트
            GameObject sObj = new GameObject("BossQuote");
            sObj.transform.SetParent(bannerObj.transform, false);
            RectTransform sRt = sObj.AddComponent<RectTransform>();
            sRt.anchorMin = new Vector2(0.5f, 0.5f);
            sRt.anchorMax = new Vector2(0.5f, 0.5f);
            sRt.sizeDelta = new Vector2(950, 40);
            sRt.anchoredPosition = new Vector2(0, -50);

            subText = sObj.AddComponent<Text>();
            subText.font = font;
            subText.text = "“업무 중 딴짓 300회 적발! 즉각 시말서를 작성하라!”";
            subText.fontSize = 18;
            subText.fontStyle = FontStyle.Italic;
            subText.alignment = TextAnchor.MiddleCenter;
            subText.color = new Color(0.85f, 0.85f, 0.9f);
        }

        private Text CreateStripeText(GameObject parent, Font font, string content)
        {
            GameObject txtObj = new GameObject("Text");
            txtObj.transform.SetParent(parent.transform, false);
            RectTransform rt = txtObj.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;

            Text txt = txtObj.AddComponent<Text>();
            txt.font = font;
            txt.text = content;
            txt.fontSize = 16;
            txt.fontStyle = FontStyle.Bold;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = Color.white;
            return txt;
        }

        private IEnumerator WarningRoutine(string bossName, string quote)
        {
            if (titleText != null) titleText.text = $"{bossName} 강림!";
            if (subText != null) subText.text = quote;

            SoundManager.Instance?.Play(SoundEffect.Warning, 0.95f);

            // 1. 페이드 인 (0.2초)
            float elapsed = 0f;
            while (elapsed < 0.2f)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / 0.2f;
                if (warningGroup != null) warningGroup.alpha = t;
                yield return null;
            }
            if (warningGroup != null) warningGroup.alpha = 1f;

            // 2. 2.0초간 붉은 비네트 펄스 및 경보 스트라이프 애니메이션
            float timer = 0f;
            while (timer < 2.0f)
            {
                timer += Time.unscaledDeltaTime;
                float pulse = Mathf.PingPong(timer * 4.5f, 1f);

                if (vignetteImage != null)
                {
                    vignetteImage.color = new Color(1f, 0.05f, 0.05f, pulse * 0.35f);
                }

                if (topStripeRt != null)
                {
                    topStripeRt.anchoredPosition = new Vector2((timer * 60f) % 100f - 50f, 0f);
                }
                if (bottomStripeRt != null)
                {
                    bottomStripeRt.anchoredPosition = new Vector2(-(timer * 60f) % 100f + 50f, 0f);
                }

                yield return null;
            }

            // 두 번째 경고 비프음
            SoundManager.Instance?.Play(SoundEffect.Warning, 0.75f);

            // 3. 페이드 아웃 (0.35초)
            elapsed = 0f;
            while (elapsed < 0.35f)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / 0.35f;
                if (warningGroup != null) warningGroup.alpha = 1f - t;
                yield return null;
            }
            if (warningGroup != null) warningGroup.alpha = 0f;
        }
    }
}
