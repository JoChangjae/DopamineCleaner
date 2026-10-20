using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DopamineCleaner.Core;
using DopamineCleaner.Meta;

namespace DopamineCleaner.UI
{
    public class VictoryCertificateUI : MonoBehaviour
    {
        public static VictoryCertificateUI Instance { get; private set; }

        [Header("UI 오브젝트")]
        private GameObject panelObj;
        private GameObject certificateFrame;
        private GameObject stampObj;
        private Text certContentText;
        private Button returnTownBtn;
        private Font uiFont;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            LoadFont();
            BuildCertificateUI();
        }

        private void LoadFont()
        {
            uiFont = Font.CreateDynamicFontFromOSFont(new string[] { "Malgun Gothic", "맑은 고딕", "Arial" }, 18);
            if (uiFont == null)
            {
                uiFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }
        }

        public void ShowVictoryCertificate()
        {
            if (panelObj == null) BuildCertificateUI();
            if (panelObj == null) return;

            panelObj.SetActive(true);
            Time.timeScale = 0f;

            // 데이터 취합
            int survivalSec = GameManager.Instance != null ? Mathf.FloorToInt(GameManager.Instance.SurvivalTime) : 0;
            int totalKills = GameManager.Instance != null ? GameManager.Instance.TotalKills : 0;
            int goldEarned = GameManager.Instance != null ? GameManager.Instance.EarnedGold : 1000;
            int karmaEarned = GameManager.Instance != null ? GameManager.Instance.EarnedKarma : 250;

            string charName = "도파민 디톡스 특수 요원";
            if (GameDataManager.Instance != null)
            {
                charName = GameDataManager.Instance.SelectedCharacterId switch
                {
                    "char_adjuster" => "특별 감사관 박사정",
                    "char_director" => "총괄 디렉터 최이사",
                    "char_cleaner" => "수석 베테랑 청소부",
                    _ => "열정 수습사원 김인턴"
                };
            }

            int preventedShorts = Mathf.Max(12, totalKills / 3);

            if (certContentText != null)
            {
                certContentText.text =
                    $"<b>【 도파민 디톡스 임무 완수 확인서 】</b>\n" +
                    $"────────────────────────────────────────\n" +
                    $"• 담당 요원 : <color=#60A5FA><b>{charName}</b></color>\n" +
                    $"• 사수한 집중 시간 : <color=#FBBF24><b>{survivalSec / 60:D2}분 {survivalSec % 60:D2}초</b></color>\n" +
                    $"• 정화한 잡생각 몬스터 : <color=#34D399><b>{totalKills:N0} 마리</b></color>\n" +
                    $"• 절약된 뇌 건강 가치 : <color=#A78BFA><b>숏폼 {preventedShorts}편 시청 방지 효과</b></color>\n" +
                    $"• 최종 정화 판정 : <color=#10B981><b>100.0% 완벽 정화 (MISSION CLEAR)</b></color>\n" +
                    $"────────────────────────────────────────\n" +
                    $"위 요원은 스마트폰 중독과 야근 압박을 완벽히 소독하였으므로,\n" +
                    $"사규 제14조에 의거하여 <b>[정규 칼퇴근 권리]</b>를 정식 승인함.\n\n" +
                    $"💰 수령 성과급: <b>+{goldEarned:N0} G</b>    ✨ 명예 카르마: <b>+{karmaEarned} P</b>";
            }

            // 도장 찍히는 펀치 애니메이션 시작
            StartCoroutine(StampDropRoutine());
            SoundManager.Instance?.Play(SoundEffect.LevelUp, 0.9f);
        }

        private IEnumerator StampDropRoutine()
        {
            if (stampObj == null) yield break;

            stampObj.SetActive(false);
            RectTransform srt = stampObj.GetComponent<RectTransform>();
            srt.localScale = Vector3.one * 3.5f;

            yield return new WaitForSecondsRealtime(0.65f);

            stampObj.SetActive(true);
            SoundManager.Instance?.Play(SoundEffect.ButtonClick, 1.0f);

            float elapsed = 0f;
            float dur = 0.22f;
            while (elapsed < dur)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / dur);
                srt.localScale = Vector3.Lerp(Vector3.one * 3.5f, Vector3.one, t);
                yield return null;
            }

            srt.localScale = Vector3.one;

            // 도장 충격 파티클/사운드
            if (PixelParticleManager.Instance != null)
            {
                PixelParticleManager.Instance.SpawnDeathPop(Vector3.zero, new Color(1f, 0.2f, 0.2f));
            }

            // 복귀 버튼 활성화
            if (returnTownBtn != null)
            {
                returnTownBtn.gameObject.SetActive(true);
            }
        }

        private void BuildCertificateUI()
        {
            Canvas canvas = GetComponentInParent<Canvas>() ?? FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            panelObj = new GameObject("VictoryCertificatePanel");
            panelObj.transform.SetParent(canvas.transform, false);

            RectTransform prt = panelObj.AddComponent<RectTransform>();
            prt.anchorMin = Vector2.zero;
            prt.anchorMax = Vector2.one;
            prt.offsetMin = Vector2.zero;
            prt.offsetMax = Vector2.zero;

            Image dim = panelObj.AddComponent<Image>();
            dim.color = new Color(0.05f, 0.04f, 0.09f, 0.94f);

            // 상단 긴급 승리 배너
            GameObject bannerObj = new GameObject("VictoryBanner");
            bannerObj.transform.SetParent(panelObj.transform, false);
            RectTransform bnRt = bannerObj.AddComponent<RectTransform>();
            bnRt.anchorMin = new Vector2(0.5f, 1f);
            bnRt.anchorMax = new Vector2(0.5f, 1f);
            bnRt.anchoredPosition = new Vector2(0, -60);
            bnRt.sizeDelta = new Vector2(900, 56);
            Image bnImg = bannerObj.AddComponent<Image>();
            bnImg.color = new Color(0.20f, 0.45f, 0.28f, 0.95f);
            var bnOutl = bannerObj.AddComponent<Outline>();
            bnOutl.effectColor = new Color(0.2f, 1f, 0.6f);
            bnOutl.effectDistance = new Vector2(2, -2);

            Text bnTxt = CreateText(bannerObj.transform, "🎉 OVERTIME TERMINATED: 정규 칼퇴근 승인! 🎉", 24, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter);
            RectTransform bnTxtRt = bnTxt.GetComponent<RectTransform>();
            bnTxtRt.anchorMin = Vector2.zero;
            bnTxtRt.anchorMax = Vector2.one;
            bnTxtRt.offsetMin = Vector2.zero;
            bnTxtRt.offsetMax = Vector2.zero;

            // 서류 결재판 프레임
            certificateFrame = new GameObject("CertificateFrame");
            certificateFrame.transform.SetParent(panelObj.transform, false);
            RectTransform frt = certificateFrame.AddComponent<RectTransform>();
            frt.anchorMin = new Vector2(0.5f, 0.5f);
            frt.anchorMax = new Vector2(0.5f, 0.5f);
            frt.anchoredPosition = new Vector2(0, -15);
            frt.sizeDelta = new Vector2(860, 580);

            Image fBg = certificateFrame.AddComponent<Image>();
            fBg.color = new Color(0.12f, 0.11f, 0.18f, 0.98f);
            var fOutl = certificateFrame.AddComponent<Outline>();
            fOutl.effectColor = new Color(0.95f, 0.80f, 0.25f, 0.95f); // 골드 테두리
            fOutl.effectDistance = new Vector2(3, -3);

            // 결재 서류 본문 내용
            GameObject txtObj = new GameObject("CertContent");
            txtObj.transform.SetParent(certificateFrame.transform, false);
            RectTransform trt = txtObj.AddComponent<RectTransform>();
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.offsetMin = new Vector2(50, 95);
            trt.offsetMax = new Vector2(-50, -35);

            certContentText = txtObj.AddComponent<Text>();
            certContentText.font = uiFont;
            certContentText.fontSize = 20;
            certContentText.alignment = TextAnchor.UpperLeft;
            certContentText.supportRichText = true;
            certContentText.lineSpacing = 1.32f;
            certContentText.color = new Color(0.92f, 0.92f, 0.98f);

            // 쾅! 찍히는 결재 도장 (Stamp)
            stampObj = new GameObject("ApprovalStamp");
            stampObj.transform.SetParent(certificateFrame.transform, false);
            RectTransform srt = stampObj.AddComponent<RectTransform>();
            srt.anchorMin = new Vector2(1f, 0f);
            srt.anchorMax = new Vector2(1f, 0f);
            srt.pivot = new Vector2(0.5f, 0.5f);
            srt.anchoredPosition = new Vector2(-155, 175);
            srt.sizeDelta = new Vector2(210, 85);
            srt.localRotation = Quaternion.Euler(0, 0, -12f); // 자연스러운 사선 각도

            Image sImg = stampObj.AddComponent<Image>();
            sImg.color = new Color(0.85f, 0.15f, 0.20f, 0.25f);
            var sOutl = stampObj.AddComponent<Outline>();
            sOutl.effectColor = new Color(1.0f, 0.2f, 0.25f, 0.95f);
            sOutl.effectDistance = new Vector2(3, -3);

            Text sTxt = CreateText(stampObj.transform, "【 칼퇴근 승인 】\n<size=12>APPROVED BY HR</size>", 22, FontStyle.Bold, new Color(1f, 0.35f, 0.35f), TextAnchor.MiddleCenter);
            RectTransform stRt = sTxt.GetComponent<RectTransform>();
            stRt.anchorMin = Vector2.zero;
            stRt.anchorMax = Vector2.one;
            stRt.offsetMin = Vector2.zero;
            stRt.offsetMax = Vector2.zero;

            // 하단 본부 복귀 버튼
            GameObject btnObj = new GameObject("ReturnTownButton");
            btnObj.transform.SetParent(certificateFrame.transform, false);
            RectTransform brt = btnObj.AddComponent<RectTransform>();
            brt.anchorMin = new Vector2(0.5f, 0f);
            brt.anchorMax = new Vector2(0.5f, 0f);
            brt.anchoredPosition = new Vector2(0, 48);
            brt.sizeDelta = new Vector2(380, 56);

            Image bImg = btnObj.AddComponent<Image>();
            bImg.color = new Color(0.20f, 0.65f, 0.40f);
            var bOutl = btnObj.AddComponent<Outline>();
            bOutl.effectColor = new Color(0.4f, 1f, 0.6f);
            bOutl.effectDistance = new Vector2(2, -2);

            returnTownBtn = btnObj.AddComponent<Button>();
            returnTownBtn.onClick.AddListener(() =>
            {
                Time.timeScale = 1f;
                SceneManager.LoadScene("TownScene");
            });

            Text bTxt = CreateText(btnObj.transform, "🏠 당당하게 정시 퇴근하기 (본부 복귀)", 20, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter);
            RectTransform btRt = bTxt.GetComponent<RectTransform>();
            btRt.anchorMin = Vector2.zero;
            btRt.anchorMax = Vector2.one;
            btRt.offsetMin = Vector2.zero;
            btRt.offsetMax = Vector2.zero;

            panelObj.SetActive(false);
        }

        private Text CreateText(Transform parent, string content, int size, FontStyle style, Color color, TextAnchor align)
        {
            GameObject obj = new GameObject("Text");
            obj.transform.SetParent(parent, false);
            Text t = obj.AddComponent<Text>();
            t.font = uiFont;
            t.text = content;
            t.fontSize = size;
            t.fontStyle = style;
            t.color = color;
            t.alignment = align;
            t.supportRichText = true;
            return t;
        }
    }
}
