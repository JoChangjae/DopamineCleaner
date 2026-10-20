using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DopamineCleaner.Core;

namespace DopamineCleaner.Town
{
    public class CyberTerminalBootUI : MonoBehaviour
    {
        public static bool HasBootedThisSession = false;

        private GameObject bootPanel;
        private Text consoleText;
        private CanvasGroup canvasGroup;
        private bool isFinished = false;
        private bool isSkipped = false;

        private readonly string[] bootLogs = new string[]
        {
            "💻 [SYSTEM] INITIALIZING DOPAMINE CLEANER OS v1.3.0...",
            "⚡ [HARDWARE] SYNCHRONIZING NEURAL RECEPTORS ... [100% OK]",
            "🛡️ [SECURITY] ACTIVATING SHORT-FORM VIRUS FIREWALL ... [ONLINE]",
            "🚨 [SURVEILLANCE] 3 CONTAMINATED ZONES DETECTED: [HIGHWAY] [OFFICE] [WARD]",
            "👥 [PERSONNEL] 6 ELITE CLEANERS STANDING BY. TACTICAL PET/DRONE ARMED.",
            "📡 [UPLINK] HEADQUARTERS QUANTUM COMM LINK ESTABLISHED.",
            "==================================================================",
            "✨ >> ACCESS GRANTED. WELCOME TO DETOX HEADQUARTERS, AGENT."
        };

        private void Start()
        {
            // 한 세션에 한 번만 실행 (원할 경우 강제 재생 가능)
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
            Canvas canvas = GetComponentInParent<Canvas>() ?? FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            bootPanel = new GameObject("CyberTerminalBootOverlay");
            bootPanel.transform.SetParent(canvas.transform, false);
            bootPanel.transform.SetAsLastSibling(); // 최상단에 렌더링

            RectTransform pRt = bootPanel.AddComponent<RectTransform>();
            pRt.anchorMin = Vector2.zero;
            pRt.anchorMax = Vector2.one;
            pRt.sizeDelta = Vector2.zero;

            canvasGroup = bootPanel.AddComponent<CanvasGroup>();

            // 검은 CRT 배경
            Image bg = bootPanel.AddComponent<Image>();
            bg.color = new Color(0.04f, 0.04f, 0.07f, 0.98f);

            // CRT 모니터 테두리 박스
            GameObject boxObj = new GameObject("TerminalBox");
            boxObj.transform.SetParent(bootPanel.transform, false);
            RectTransform bRt = boxObj.AddComponent<RectTransform>();
            bRt.anchorMin = new Vector2(0.5f, 0.5f);
            bRt.anchorMax = new Vector2(0.5f, 0.5f);
            bRt.sizeDelta = new Vector2(1050, 580);

            Image boxBg = boxObj.AddComponent<Image>();
            boxBg.color = new Color(0.06f, 0.08f, 0.12f, 0.95f);

            var outl = boxObj.AddComponent<Outline>();
            outl.effectColor = new Color(0.2f, 0.85f, 1f, 0.85f);
            outl.effectDistance = new Vector2(2f, -2f);

            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            // 상단 타이틀 바
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

            GameObject hTxtObj = new GameObject("HeaderText");
            hTxtObj.transform.SetParent(headerObj.transform, false);
            RectTransform htRt = hTxtObj.AddComponent<RectTransform>();
            htRt.anchorMin = Vector2.zero;
            htRt.anchorMax = Vector2.one;
            htRt.sizeDelta = new Vector2(-20, 0);

            Text hTxt = hTxtObj.AddComponent<Text>();
            hTxt.font = font;
            hTxt.text = "⚡ [TERMINAL_BOOT_SYS] NEURAL COMMAND PROMPT v1.3.0";
            hTxt.fontSize = 17;
            hTxt.fontStyle = FontStyle.Bold;
            hTxt.color = new Color(0.35f, 0.95f, 1f);
            hTxt.alignment = TextAnchor.MiddleLeft;

            // 스킵 안내 텍스트
            GameObject skipObj = new GameObject("SkipGuide");
            skipObj.transform.SetParent(headerObj.transform, false);
            RectTransform sRt = skipObj.AddComponent<RectTransform>();
            sRt.anchorMin = Vector2.zero;
            sRt.anchorMax = Vector2.one;
            sRt.sizeDelta = new Vector2(-25, 0);

            Text sTxt = skipObj.AddComponent<Text>();
            sTxt.font = font;
            sTxt.text = "[Click or Any Key to Skip >>]";
            sTxt.fontSize = 14;
            sTxt.color = new Color(0.7f, 0.7f, 0.8f, 0.8f);
            sTxt.alignment = TextAnchor.MiddleRight;

            // 콘솔 텍스트 출력 영역
            GameObject bodyObj = new GameObject("ConsoleBody");
            bodyObj.transform.SetParent(boxObj.transform, false);
            RectTransform bodyRt = bodyObj.AddComponent<RectTransform>();
            bodyRt.anchorMin = Vector2.zero;
            bodyRt.anchorMax = Vector2.one;
            bodyRt.offsetMin = new Vector2(30, 25);
            bodyRt.offsetMax = new Vector2(-30, -55);

            consoleText = bodyObj.AddComponent<Text>();
            consoleText.font = font;
            consoleText.fontSize = 18;
            consoleText.lineSpacing = 1.35f;
            consoleText.color = new Color(0.3f, 1f, 0.65f); // 레트로 터미널 형광 그린/민트
            consoleText.alignment = TextAnchor.UpperLeft;
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
                    // 스킵 시 전체 텍스트 즉각 완성
                    currentFullText = string.Join("\n", bootLogs);
                    if (consoleText != null) consoleText.text = currentFullText;
                    break;
                }

                currentFullText += (i == 0 ? "" : "\n") + bootLogs[i];
                if (consoleText != null)
                {
                    consoleText.text = currentFullText;
                }

                SoundManager.Instance?.Play(SoundEffect.Shoot, 0.25f);
                yield return new WaitForSeconds(0.18f);
            }

            // 부팅 완료 대기 (0.35초)
            SoundManager.Instance?.Play(SoundEffect.LevelUp, 0.5f);
            yield return new WaitForSeconds(0.35f);

            // 페이드아웃 및 셔터 오픈 효과
            isFinished = true;
            float elapsed = 0f;
            float duration = 0.4f;

            Vector3 origScale = bootPanel.transform.localScale;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / duration;

                if (canvasGroup != null)
                {
                    canvasGroup.alpha = Mathf.Lerp(1f, 0f, t);
                }

                if (bootPanel != null)
                {
                    // 수직으로 샤라락 열리는 CRT 셔터 연출
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
