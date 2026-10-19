using UnityEngine;
using UnityEngine.UI;
using DopamineCleaner.Core;

namespace DopamineCleaner.UI
{
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

        [Header("게임플레이 옵션")]
        [SerializeField] private Toggle shakeToggle;
        [SerializeField] private Toggle speedrunTimerToggle;

        [Header("버튼")]
        [SerializeField] private Button closeButton;

        public static bool CameraShakeEnabled { get; set; } = true;
        public static bool SpeedrunTimerEnabled { get; set; } = false;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            if (modalPanel != null) modalPanel.SetActive(false);
            InitListeners();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
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

            if (shakeToggle != null)
            {
                shakeToggle.isOn = CameraShakeEnabled;
                shakeToggle.onValueChanged.AddListener((val) => CameraShakeEnabled = val);
            }

            if (speedrunTimerToggle != null)
            {
                speedrunTimerToggle.isOn = SpeedrunTimerEnabled;
                speedrunTimerToggle.onValueChanged.AddListener((val) => SpeedrunTimerEnabled = val);
            }

            if (closeButton != null)
            {
                closeButton.onClick.AddListener(Close);
            }

            UpdateLabels();
        }

        public void Open()
        {
            if (modalPanel != null) modalPanel.SetActive(true);
            UpdateLabels();
        }

        public void Close()
        {
            if (modalPanel != null) modalPanel.SetActive(false);
        }

        private void OnBGMChanged(float val)
        {
            SoundManager.Instance?.SetBGMVolume(val);
            UpdateLabels();
        }

        private void OnSFXChanged(float val)
        {
            SoundManager.Instance?.SetSFXVolume(val);
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

        private void UpdateLabels()
        {
            if (bgmValText != null && SoundManager.Instance != null)
                bgmValText.text = $"{Mathf.RoundToInt(SoundManager.Instance.BGMVolume * 100f)}%";

            if (sfxValText != null && SoundManager.Instance != null)
                sfxValText.text = $"{Mathf.RoundToInt(SoundManager.Instance.SFXVolume * 100f)}%";

            if (muteText != null && SoundManager.Instance != null)
                muteText.text = SoundManager.Instance.IsMuted ? "🔇 [전체 음소거 ON]" : "🔊 [전체 음소거 OFF]";
        }
    }
}
