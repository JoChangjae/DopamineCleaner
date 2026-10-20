using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DopamineCleaner.Core;

namespace DopamineCleaner.Town
{
    /// <summary>
    /// 게임 접속 시 멋진 레트로 사이버펑크 CLI 터미널 부팅 시퀀스를 연출하는 UI.
    /// 실시간 타이머(WaitForSecondsRealtime)를 사용하여 어떤 환경에서도 멈춤 없이 안정적으로 실행되며,
    /// 아무 키나 클릭 시 즉시 스킵되어 본부로 진입합니다.
    /// </summary>
    public class CyberTerminalBootUI : MonoBehaviour
    {
        public static bool HasBootedThisSession = false;

        private GameObject bootPanel;
        private Text consoleText;
        private CanvasGroup canvasGroup;
        private bool isFinished = false;
        private bool isSkipped = false;

        private static Font koreanFont;

        private readonly string[] bootLogs = new string[]
        {
            "> [SYS_INIT] DOPAMINE CLEANER OS v1.4.0 INITIALIZING ... [OK]",
            "> [KERNEL] QUANTUM NEURAL INTERFACE LINKED ... [100% OK]",
            "> [SECURITY] SCANNING SHORT-FORM VIRUS CONTAMINATION ... [ALERT]",
            "> [SURVEILLANCE] 3 CRITICAL ZONES IDENTIFIED: [HIGHWAY] [OFFICE] [WARD]",
            "> [AGENTS] 6 ELITE DETOX AGENTS STANDING BY ... [ARMED]",
            "> [COMPANIONS] TACTICAL DRONES & PET BIO-MODULES SYNCHRONIZED",
            "> [FACILITIES] HQ SILICON-VALLEY REMODELING MATRIX ONLINE",
            "==================================================================",
            ">> ACCESS GRANTED. WELCOME TO DETOX HEADQUARTERS, AGENT."
        };

        private void Awake()
        {
            EnsureFont();
        }

        private static void EnsureFont()
        {
            if (koreanFont == null)
            {
#if UNITY_EDITOR
                koreanFont = UnityEditor.AssetDatabase.LoadAssetAtPath<Font>("Assets/Fonts/MalgunGothic.ttf");
#endif
                if (koreanFont == null)
                    koreanFont = Font.CreateDynamicFontFromOSFont(new string[] { "Malgun Gothic", "맑은 고딕", "Arial" }, 18);
            }
        }

        private void Start()
        {
            // 한 게임 세션 동안 첫 진입 시에만 1회 출력 (원할 경우 재실행 가능)
            if (HasBootedThisSession)
            {
                Destroy(gameObject);
                return;
            }

            HasBootedThisSession = true;
            CreateTerminalUI();
            StartCoroutine(BootSequenceRoutine());
        }

        private void Update()
        {
            if (!isFinished && !isSkipped)
            {
                if (Input.anyKeyDown || Input.GetMouseButtonDown(0))
                {
                    isSkipped = true;
                }
            }
        }

        private void CreateTerminalUI()
        {
            EnsureFont();
            Canvas canvas = GetComponentInParent<Canvas>() ?? FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            bootPanel = new GameObject("CyberTerminalBootOverlay");
            bootPanel.transform.SetParent(canvas.transform, false);
            bootPanel.transform.SetAsLastSibling(); // UI 최상단에 렌더링

            RectTransform pRt = bootPanel.AddComponent<RectTransform>();
            pRt.anchorMin = Vector2.zero;
            pRt.anchorMax = Vector2.one;
            pRt.sizeDelta = Vector2.zero;

            canvasGroup = bootPanel.AddComponent<CanvasGroup>();

            // 1. CRT 모니터 어두운 배경
            Image bg = bootPanel.AddComponent<Image>();
            bg.color = new Color(0.04f, 0.04f, 0.07f, 0.98f);

            // 2. 터미널 윈도우 프레임 박스
            GameObject boxObj = new GameObject("TerminalBox");
            boxObj.transform.SetParent(bootPanel.transform, false);
            RectTransform bRt = boxObj.AddComponent<RectTransform>();
            bRt.anchorMin = new Vector2(0.5f, 0.5f);
            bRt.anchorMax = new Vector2(0.5f, 0.5f);
            bRt.sizeDelta = new Vector2(1100, 600);

            Image boxBg = boxObj.AddComponent<Image>();
            boxBg.color = new Color(0.06f, 0.08f, 0.12f, 0.96f);

            var outl = boxObj.AddComponent<Outline>();
            outl.effectColor = new Color(0.2f, 0.85f, 1f, 0.9f);
            outl.effectDistance = new Vector2(2f, -2f);

            // 3. 상단 헤더 바
            GameObject headerObj = new GameObject("Header");
            headerObj.transform.SetParent(boxObj.transform, false);
            RectTransform hRt = headerObj.AddComponent<RectTransform>();
            hRt.anchorMin = new Vector2(0f, 1f);
            hRt.anchorMax = new Vector2(1f, 1f);
            hRt.pivot = new Vector2(0.5f, 1f);
            hRt.sizeDelta = new Vector2(0, 48);
            hRt.anchoredPosition = Vector2.zero;

            Image hBg = headerObj.AddComponent<Image>();
            hBg.color = new Color(0.12f, 0.18f, 0.28f, 1f);

            // 타이틀 텍스트
            GameObject hTxtObj = new GameObject("HeaderText");
            hTxtObj.transform.SetParent(headerObj.transform, false);
            RectTransform htRt = hTxtObj.AddComponent<RectTransform>();
            htRt.anchorMin = Vector2.zero;
            htRt.anchorMax = Vector2.one;
            htRt.offsetMin = new Vector2(20, 0);
            htRt.offsetMax = new Vector2(-220, 0);

            Text hTxt = hTxtObj.AddComponent<Text>();
            hTxt.font = koreanFont;
            hTxt.text = "⚡ [TERMINAL_BOOT_SYS] NEURAL COMMAND PROMPT v1.4.0";
            hTxt.fontSize = 17;
            hTxt.fontStyle = FontStyle.Bold;
            hTxt.color = new Color(0.35f, 0.95f, 1f);
            hTxt.alignment = TextAnchor.MiddleLeft;

            // 스킵 안내 버튼 텍스트
            GameObject skipObj = new GameObject("SkipGuide");
            skipObj.transform.SetParent(headerObj.transform, false);
            RectTransform sRt = skipObj.AddComponent<RectTransform>();
            sRt.anchorMin = new Vector2(1f, 0f);
            sRt.anchorMax = new Vector2(1f, 1f);
            sRt.pivot = new Vector2(1f, 0.5f);
            sRt.sizeDelta = new Vector2(240, 0);
            sRt.anchoredPosition = new Vector2(-15, 0);

            Text sTxt = skipObj.AddComponent<Text>();
            sTxt.font = koreanFont;
            sTxt.text = "[클릭 시 즉시 스킵 >>]";
            sTxt.fontSize = 14;
            sTxt.color = new Color(0.8f, 0.85f, 0.95f, 0.85f);
            sTxt.alignment = TextAnchor.MiddleRight;

            // 4. 콘솔 텍스트 바디
            GameObject bodyObj = new GameObject("ConsoleBody");
            bodyObj.transform.SetParent(boxObj.transform, false);
            RectTransform bodyRt = bodyObj.AddComponent<RectTransform>();
            bodyRt.anchorMin = Vector2.zero;
            bodyRt.anchorMax = Vector2.one;
            bodyRt.offsetMin = new Vector2(35, 25);
            bodyRt.offsetMax = new Vector2(-35, -60);

            consoleText = bodyObj.AddComponent<Text>();
            consoleText.font = koreanFont;
            consoleText.fontSize = 18;
            consoleText.lineSpacing = 1.35f;
            consoleText.color = new Color(0.3f, 1f, 0.65f); // 네온 레트로 그린
            consoleText.alignment = TextAnchor.UpperLeft;
            consoleText.horizontalOverflow = HorizontalWrapMode.Wrap;
            consoleText.verticalOverflow = VerticalWrapMode.Overflow;
            consoleText.text = "";
        }

        private IEnumerator BootSequenceRoutine()
        {
            SoundManager.Instance?.Play(SoundEffect.ButtonClick, 0.4f);

            string currentFullText = "";
            for (int i = 0; i < bootLogs.Length; i++)
            {
                if (isSkipped)
                {
                    // 스킵 시 전체 텍스트 즉각 표시
                    currentFullText = string.Join("\n", bootLogs);
                    if (consoleText != null) consoleText.text = currentFullText;
                    break;
                }

                currentFullText += (i == 0 ? "" : "\n") + bootLogs[i];
                if (consoleText != null)
                {
                    consoleText.text = currentFullText;
                }

                SoundManager.Instance?.Play(SoundEffect.Shoot, 0.2f);
                // 실시간 초 기반 대기 (어떤 Time.timeScale 상황에서도 100% 정상 작동)
                yield return new WaitForSecondsRealtime(0.12f);
            }

            // 부팅 완료 대기
            SoundManager.Instance?.Play(SoundEffect.LevelUp, 0.45f);
            yield return new WaitForSecondsRealtime(isSkipped ? 0.15f : 0.4f);

            // 페이드아웃 및 CRT 셔터 오픈 효과
            isFinished = true;
            float elapsed = 0f;
            float duration = 0.35f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                if (canvasGroup != null)
                {
                    canvasGroup.alpha = Mathf.Lerp(1f, 0f, t);
                }

                if (bootPanel != null)
                {
                    // 수직 셔터 오픈
                    bootPanel.transform.localScale = new Vector3(1f, Mathf.Lerp(1f, 0.02f, t), 1f);
                }

                yield return null;
            }

            if (bootPanel != null)
            {
                Destroy(bootPanel);
            }

            Destroy(gameObject);
        }
    }
}
