using System;
using UnityEngine;
using UnityEngine.UI;
using DopamineCleaner.Core;

namespace DopamineCleaner.UI
{
    /// <summary>
    /// 통합 환경설정 모달:
    /// - BGM / SFX 실시간 볼륨 조절 슬라이더 및 효과음 테스트
    /// - 전체화면 / 창모드 및 해상도(1600x900, 1280x720, 960x540) 원클릭 변경
    /// - 화면 흔들림(Camera Shake) & 스피드런 타이머 토글
    /// - 전체 조작키 가이드 시각화
    /// </summary>
    public class SettingsModalUI : MonoBehaviour
    {
        public static SettingsModalUI Instance { get; private set; }

        [Header("모달 패널")]
        [SerializeField] private GameObject modalPanel;

        [Header("오디오 컨트롤")]
        [SerializeField] private Slider bgmSlider;
        [SerializeField] private Slider sfxSlider;
        [SerializeField] private Text bgmValText;
        [SerializeField] private Text sfxValText;
        [SerializeField] private Button muteToggleBtn;
        [SerializeField] private Text muteText;
        [SerializeField] private Button sfxTestBtn;

        [Header("게임플레이 옵션")]
        [SerializeField] private Toggle shakeToggle;
        [SerializeField] private Toggle speedrunTimerToggle;

        [Header("화면 디스플레이 옵션")]
        [SerializeField] private Button windowModeBtn;
        [SerializeField] private Text windowModeText;
        [SerializeField] private Button res1600Btn;
        [SerializeField] private Button res1280Btn;
        [SerializeField] private Button res960Btn;

        [Header("버튼")]
        [SerializeField] private Button closeButton;

        public static bool CameraShakeEnabled { get; set; } = true;
        public static bool SpeedrunTimerEnabled { get; set; } = false;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            // 모달 패널이 없으면 자체 동적 생성
            if (modalPanel == null)
            {
                BuildDynamicModalPanel();
            }

            if (modalPanel != null) modalPanel.SetActive(false);
            InitListeners();
        }

        private void Start()
        {
            LoadSavedSettings();
        }

        public static void OpenSettings()
        {
            if (Instance == null)
            {
                Canvas canvas = FindAnyObjectByType<Canvas>();
                if (canvas != null)
                {
                    GameObject obj = new GameObject("[SettingsModalUI]");
                    obj.transform.SetParent(canvas.transform, false);
                    Instance = obj.AddComponent<SettingsModalUI>();
                }
            }

            Instance?.Open();
        }

        private void InitListeners()
        {
            if (bgmSlider != null)
            {
                bgmSlider.value = SoundManager.Instance != null ? SoundManager.Instance.BGMVolume : 0.35f;
                bgmSlider.onValueChanged.AddListener(OnBGMChanged);
            }

            if (sfxSlider != null)
            {
                sfxSlider.value = SoundManager.Instance != null ? SoundManager.Instance.SFXVolume : 1.0f;
                sfxSlider.onValueChanged.AddListener(OnSFXChanged);
            }

            if (muteToggleBtn != null)
            {
                muteToggleBtn.onClick.AddListener(OnMuteClicked);
            }

            if (sfxTestBtn != null)
            {
                sfxTestBtn.onClick.AddListener(() =>
                {
                    SoundManager.Instance?.Play(SoundEffect.ButtonClick, 0.9f);
                });
            }

            if (shakeToggle != null)
            {
                shakeToggle.isOn = CameraShakeEnabled;
                shakeToggle.onValueChanged.AddListener((val) =>
                {
                    CameraShakeEnabled = val;
                    PlayerPrefs.SetInt("DC_Opt_Shake", val ? 1 : 0);
                    PlayerPrefs.Save();
                });
            }

            if (speedrunTimerToggle != null)
            {
                speedrunTimerToggle.isOn = SpeedrunTimerEnabled;
                speedrunTimerToggle.onValueChanged.AddListener((val) =>
                {
                    SpeedrunTimerEnabled = val;
                    PlayerPrefs.SetInt("DC_Opt_Speedrun", val ? 1 : 0);
                    PlayerPrefs.Save();
                });
            }

            if (windowModeBtn != null)
            {
                windowModeBtn.onClick.AddListener(OnWindowModeClicked);
            }

            if (res1600Btn != null)
            {
                res1600Btn.onClick.AddListener(() => DisplayModeManager.Instance?.SetWindowResolution(1600, 900));
            }

            if (res1280Btn != null)
            {
                res1280Btn.onClick.AddListener(() => DisplayModeManager.Instance?.SetWindowResolution(1280, 720));
            }

            if (res960Btn != null)
            {
                res960Btn.onClick.AddListener(() => DisplayModeManager.Instance?.SetWindowResolution(960, 540));
            }

            DisplayModeManager.OnDisplayModeChanged += HandleDisplayModeChanged;

            if (closeButton != null)
            {
                closeButton.onClick.AddListener(Close);
            }

            UpdateLabels();
        }

        private void LoadSavedSettings()
        {
            CameraShakeEnabled = PlayerPrefs.GetInt("DC_Opt_Shake", 1) == 1;
            SpeedrunTimerEnabled = PlayerPrefs.GetInt("DC_Opt_Speedrun", 0) == 1;

            if (shakeToggle != null) shakeToggle.isOn = CameraShakeEnabled;
            if (speedrunTimerToggle != null) speedrunTimerToggle.isOn = SpeedrunTimerEnabled;

            if (PlayerPrefs.HasKey("DC_BGM_Vol") && SoundManager.Instance != null)
            {
                float bgm = PlayerPrefs.GetFloat("DC_BGM_Vol", 0.35f);
                SoundManager.Instance.SetBGMVolume(bgm);
                if (bgmSlider != null) bgmSlider.value = bgm;
            }

            if (PlayerPrefs.HasKey("DC_SFX_Vol") && SoundManager.Instance != null)
            {
                float sfx = PlayerPrefs.GetFloat("DC_SFX_Vol", 1.0f);
                SoundManager.Instance.SetSFXVolume(sfx);
                if (sfxSlider != null) sfxSlider.value = sfx;
            }

            UpdateLabels();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
            DisplayModeManager.OnDisplayModeChanged -= HandleDisplayModeChanged;
        }

        private void HandleDisplayModeChanged(bool isWindowed)
        {
            UpdateLabels();
        }

        public void Open()
        {
            SoundManager.Instance?.Play(SoundEffect.ButtonClick, 0.6f);
            if (modalPanel != null)
            {
                modalPanel.SetActive(true);
                modalPanel.transform.SetAsLastSibling();
            }
            UpdateLabels();
        }

        public void Close()
        {
            SoundManager.Instance?.Play(SoundEffect.ButtonClick, 0.6f);
            if (modalPanel != null) modalPanel.SetActive(false);
        }

        private void OnBGMChanged(float val)
        {
            SoundManager.Instance?.SetBGMVolume(val);
            PlayerPrefs.SetFloat("DC_BGM_Vol", val);
            PlayerPrefs.Save();
            UpdateLabels();
        }

        private void OnSFXChanged(float val)
        {
            SoundManager.Instance?.SetSFXVolume(val);
            PlayerPrefs.SetFloat("DC_SFX_Vol", val);
            PlayerPrefs.Save();
            UpdateLabels();
        }

        private void OnMuteClicked()
        {
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.ToggleMute();
                UpdateLabels();
            }
        }

        private void OnWindowModeClicked()
        {
            DisplayModeManager.Instance?.CycleDisplayMode();
            UpdateLabels();
        }

        private void UpdateLabels()
        {
            if (bgmValText != null && SoundManager.Instance != null)
                bgmValText.text = $"{Mathf.RoundToInt(SoundManager.Instance.BGMVolume * 100f)}%";

            if (sfxValText != null && SoundManager.Instance != null)
                sfxValText.text = $"{Mathf.RoundToInt(SoundManager.Instance.SFXVolume * 100f)}%";

            if (muteText != null && SoundManager.Instance != null)
                muteText.text = SoundManager.Instance.IsMuted ? "🔇 [전체 음소거 ON]" : "🔊 [전체 음소거 OFF]";

            if (windowModeText != null && DisplayModeManager.Instance != null)
                windowModeText.text = DisplayModeManager.Instance.GetDisplayModeLabel();
        }

        /// <summary>
        /// 인스펙터에 수동 할당되지 않았을 경우, 완전히 아름다운 사이버펑크 픽셀 스타일 설정 모달을 코드에서 동적 빌드
        /// </summary>
        private void BuildDynamicModalPanel()
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null) canvas = FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            Font font = Font.CreateDynamicFontFromOSFont(new string[] { "Malgun Gothic", "맑은 고딕", "Arial" }, 16);

            // 1. 전체 암전 배경
            modalPanel = new GameObject("SettingsModalPanel");
            modalPanel.transform.SetParent(canvas.transform, false);
            RectTransform pRt = modalPanel.AddComponent<RectTransform>();
            pRt.anchorMin = Vector2.zero;
            pRt.anchorMax = Vector2.one;
            pRt.offsetMin = Vector2.zero;
            pRt.offsetMax = Vector2.zero;

            Image darkBg = modalPanel.AddComponent<Image>();
            darkBg.color = new Color(0.04f, 0.03f, 0.08f, 0.88f); // 딥 다크 오버레이

            // 2. 모달 팝업 창 본체 (900 x 680)
            GameObject box = new GameObject("SettingsBox");
            box.transform.SetParent(modalPanel.transform, false);
            RectTransform bRt = box.AddComponent<RectTransform>();
            bRt.sizeDelta = new Vector2(900, 680);
            bRt.anchoredPosition = Vector2.zero;

            Image boxImg = box.AddComponent<Image>();
            boxImg.color = new Color(0.12f, 0.10f, 0.18f, 0.98f);
            var boxOutl = box.AddComponent<Outline>();
            boxOutl.effectColor = new Color(0.35f, 0.85f, 1f, 0.8f); // 네온 사이언 테두리
            boxOutl.effectDistance = new Vector2(2f, -2f);

            // 타이틀 헤더
            CreateText(box.transform, "⚙️ 게임 시스템 & 오디오 환경설정", 24, TextAnchor.MiddleCenter, new Vector2(0, 305), new Vector2(850, 40), new Color(1f, 0.88f, 0.35f), font);

            // 구분선
            CreateDivider(box.transform, new Vector2(0, 275), 840);

            // [섹션 1: 오디오 설정] (Y: 230 ~ 110)
            CreateText(box.transform, "🎵 사운드 볼륨", 18, TextAnchor.MiddleLeft, new Vector2(-230, 240), new Vector2(360, 30), new Color(0.4f, 0.95f, 1f), font);

            // BGM 슬라이더
            CreateText(box.transform, "배경음악 (BGM):", 15, TextAnchor.MiddleLeft, new Vector2(-250, 205), new Vector2(140, 25), Color.white, font);
            bgmSlider = CreateSlider(box.transform, new Vector2(-30, 205), new Vector2(260, 20));
            bgmValText = CreateText(box.transform, "35%", 15, TextAnchor.MiddleCenter, new Vector2(140, 205), new Vector2(60, 25), new Color(1f, 0.85f, 0.3f), font);

            // SFX 슬라이더
            CreateText(box.transform, "효과음 (SFX):", 15, TextAnchor.MiddleLeft, new Vector2(-250, 165), new Vector2(140, 25), Color.white, font);
            sfxSlider = CreateSlider(box.transform, new Vector2(-30, 165), new Vector2(260, 20));
            sfxValText = CreateText(box.transform, "100%", 15, TextAnchor.MiddleCenter, new Vector2(140, 165), new Vector2(60, 25), new Color(1f, 0.85f, 0.3f), font);

            // 음소거 및 효과음 테스트 버튼
            muteToggleBtn = CreateSimpleButton(box.transform, "muteBtn", "🔊 [전체 음소거 OFF]", new Vector2(-80, 120), new Vector2(240, 38), new Color(0.24f, 0.22f, 0.35f), font);
            muteText = muteToggleBtn.GetComponentInChildren<Text>();

            sfxTestBtn = CreateSimpleButton(box.transform, "sfxTestBtn", "🔔 SFX 테스트", new Vector2(120, 120), new Vector2(130, 38), new Color(0.25f, 0.38f, 0.32f), font);

            // 구분선
            CreateDivider(box.transform, new Vector2(0, 85), 840);

            // [섹션 2: 화면 디스플레이] (Y: 55 ~ -30)
            CreateText(box.transform, "🖥️ 디스플레이 & 해상도", 18, TextAnchor.MiddleLeft, new Vector2(-230, 55), new Vector2(360, 30), new Color(0.4f, 0.95f, 1f), font);

            windowModeBtn = CreateSimpleButton(box.transform, "winModeBtn", "🖥️ 창모드 / 전체화면 (F11)", new Vector2(-150, 15), new Vector2(300, 38), new Color(0.22f, 0.28f, 0.42f), font);
            windowModeText = windowModeBtn.GetComponentInChildren<Text>();

            res1600Btn = CreateSimpleButton(box.transform, "res1600Btn", "1600x900", new Vector2(60, 15), new Vector2(110, 38), new Color(0.20f, 0.25f, 0.35f), font);
            res1280Btn = CreateSimpleButton(box.transform, "res1280Btn", "1280x720", new Vector2(180, 15), new Vector2(110, 38), new Color(0.20f, 0.25f, 0.35f), font);
            res960Btn = CreateSimpleButton(box.transform, "res960Btn", "960x540", new Vector2(300, 15), new Vector2(110, 38), new Color(0.20f, 0.25f, 0.35f), font);

            // 구분선
            CreateDivider(box.transform, new Vector2(0, -35), 840);

            // [섹션 3: 조작키 안내 (Keybindings Reference)] (Y: -65 ~ -240)
            CreateText(box.transform, "🎮 조작키 가이드 (KEY CONTROLS)", 18, TextAnchor.MiddleLeft, new Vector2(-230, -65), new Vector2(360, 30), new Color(0.4f, 0.95f, 1f), font);

            string keyGuide =
                "🕹️ [WASD / 방향키] 이동        💨 [Space] 긴급 대시 회피\n" +
                "⚡ [Q] 캐릭터 고유 궁극기         📋 [Tab] 무기 진화 계통도 트리\n" +
                "🎰 [E] 자판기/기믹 상호작용       🩹 [1] 응급 구급약 사용\n" +
                "⏸️ [ESC] 일시정지 / 설정        🖥️ [F11] 창모드/전체화면 전환";

            Text keyTxt = CreateText(box.transform, keyGuide, 15, TextAnchor.MiddleLeft, new Vector2(0, -145), new Vector2(800, 120), new Color(0.85f, 0.88f, 0.95f), font);
            keyTxt.lineSpacing = 1.35f;

            // 닫기 버튼 (X / 적용 완료)
            closeButton = CreateSimpleButton(box.transform, "closeBtn", "💾 저장 및 닫기 (ESC)", new Vector2(0, -285), new Vector2(280, 48), new Color(0.25f, 0.55f, 0.38f), font);
            closeButton.GetComponentInChildren<Text>().fontSize = 17;
        }

        private Text CreateText(Transform parent, string text, int size, TextAnchor anchor, Vector2 pos, Vector2 delta, Color col, Font font)
        {
            GameObject obj = new GameObject("Text");
            obj.transform.SetParent(parent, false);
            RectTransform rt = obj.AddComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = delta;

            Text t = obj.AddComponent<Text>();
            t.text = text;
            t.fontSize = size;
            t.alignment = anchor;
            t.color = col;
            t.font = font;

            var outl = obj.AddComponent<Outline>();
            outl.effectColor = new Color(0f, 0f, 0f, 0.9f);
            outl.effectDistance = new Vector2(1f, -1f);
            return t;
        }

        private void CreateDivider(Transform parent, Vector2 pos, float width)
        {
            GameObject div = new GameObject("Divider");
            div.transform.SetParent(parent, false);
            RectTransform rt = div.AddComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(width, 2);
            Image img = div.AddComponent<Image>();
            img.color = new Color(0.25f, 0.22f, 0.35f, 0.8f);
        }

        private Slider CreateSlider(Transform parent, Vector2 pos, Vector2 size)
        {
            GameObject sliderObj = new GameObject("Slider");
            sliderObj.transform.SetParent(parent, false);
            RectTransform rt = sliderObj.AddComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            Slider slider = sliderObj.AddComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;

            // 슬라이더 배경
            GameObject bg = new GameObject("Background");
            bg.transform.SetParent(sliderObj.transform, false);
            RectTransform bgRt = bg.AddComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.sizeDelta = Vector2.zero;
            Image bgImg = bg.AddComponent<Image>();
            bgImg.color = new Color(0.18f, 0.16f, 0.24f);

            // Fill Area
            GameObject fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(sliderObj.transform, false);
            RectTransform faRt = fillArea.AddComponent<RectTransform>();
            faRt.anchorMin = new Vector2(0f, 0.25f);
            faRt.anchorMax = new Vector2(1f, 0.75f);
            faRt.sizeDelta = Vector2.zero;

            GameObject fill = new GameObject("Fill");
            fill.transform.SetParent(fillArea.transform, false);
            RectTransform fillRt = fill.AddComponent<RectTransform>();
            fillRt.sizeDelta = Vector2.zero;
            Image fillImg = fill.AddComponent<Image>();
            fillImg.color = new Color(0.35f, 0.85f, 1f); // 네온 사이언

            slider.fillRect = fillRt;
            slider.targetGraphic = fillImg;

            return slider;
        }

        private Button CreateSimpleButton(Transform parent, string name, string text, Vector2 pos, Vector2 size, Color bgColor, Font font)
        {
            GameObject btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent, false);
            RectTransform rt = btnObj.AddComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            Image img = btnObj.AddComponent<Image>();
            img.color = bgColor;

            Button btn = btnObj.AddComponent<Button>();
            ColorBlock cb = btn.colors;
            cb.highlightedColor = bgColor * 1.25f;
            cb.pressedColor = bgColor * 0.8f;
            btn.colors = cb;

            var outl = btnObj.AddComponent<Outline>();
            outl.effectColor = new Color(0f, 0f, 0f, 0.8f);
            outl.effectDistance = new Vector2(1.5f, -1.5f);

            Text t = CreateText(btnObj.transform, text, 15, TextAnchor.MiddleCenter, Vector2.zero, size, Color.white, font);
            t.rectTransform.anchorMin = Vector2.zero;
            t.rectTransform.anchorMax = Vector2.one;
            t.rectTransform.sizeDelta = Vector2.zero;

            return btn;
        }
    }
}
