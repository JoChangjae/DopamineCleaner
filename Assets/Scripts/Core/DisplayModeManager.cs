using System;
using UnityEngine;

namespace DopamineCleaner.Core
{
    public class DisplayModeManager : MonoBehaviour
    {
        public static DisplayModeManager Instance { get; private set; }

        public static event Action<bool> OnDisplayModeChanged;

        private const string PrefKeyWindowed = "DC_Display_Windowed";
        private const string PrefKeyWidth = "DC_Display_Width";
        private const string PrefKeyHeight = "DC_Display_Height";

        public bool IsWindowed { get; private set; } = false;
        public int WindowWidth { get; private set; } = 1600;
        public int WindowHeight { get; private set; } = 900;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoInitialize()
        {
            if (Instance == null)
            {
                GameObject obj = new GameObject("[DisplayModeManager]");
                obj.AddComponent<DisplayModeManager>();
            }
        }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                LoadSavedDisplayMode();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Update()
        {
            // Windows PC 표준 단축키: F11 또는 Alt+Enter
            bool isAltPressed = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
            if (Input.GetKeyDown(KeyCode.F11) || (isAltPressed && Input.GetKeyDown(KeyCode.Return)))
            {
                ToggleDisplayMode();
            }
        }

        public void ToggleDisplayMode()
        {
            SetDisplayMode(!IsWindowed);
        }

        public void SetDisplayMode(bool windowed, int width = 1600, int height = 900)
        {
            IsWindowed = windowed;
            WindowWidth = width;
            WindowHeight = height;

            if (windowed)
            {
                // 창모드 적용: 1600x900 윈도우 모드
                Screen.SetResolution(width, height, FullScreenMode.Windowed);
                Debug.Log($"🖥️ [화면 모드 전환] 창모드 ({width}x{height}) 적용 완료 (단축키: F11)");
            }
            else
            {
                // 전체화면 적용: 모니터 기본 최대 해상도 전체화면
                int fullW = Display.main.systemWidth > 0 ? Display.main.systemWidth : 1920;
                int fullH = Display.main.systemHeight > 0 ? Display.main.systemHeight : 1080;
                Screen.SetResolution(fullW, fullH, FullScreenMode.FullScreenWindow);
                Debug.Log($"🖥️ [화면 모드 전환] 전체화면 ({fullW}x{fullH}) 적용 완료 (단축키: F11)");
            }

            SaveDisplayMode();
            OnDisplayModeChanged?.Invoke(IsWindowed);
            SoundManager.Instance?.Play(SoundEffect.Dash, 0.35f);
        }

        public string GetDisplayModeLabel()
        {
            return IsWindowed ? "🖥️ 화면: 창모드 [1600x900] (F11)" : "🖥️ 화면: 전체화면 (F11)";
        }

        private void SaveDisplayMode()
        {
            PlayerPrefs.SetInt(PrefKeyWindowed, IsWindowed ? 1 : 0);
            PlayerPrefs.SetInt(PrefKeyWidth, WindowWidth);
            PlayerPrefs.SetInt(PrefKeyHeight, WindowHeight);
            PlayerPrefs.Save();
        }

        private void LoadSavedDisplayMode()
        {
            if (PlayerPrefs.HasKey(PrefKeyWindowed))
            {
                bool savedWindowed = PlayerPrefs.GetInt(PrefKeyWindowed, 0) == 1;
                int savedW = PlayerPrefs.GetInt(PrefKeyWidth, 1600);
                int savedH = PlayerPrefs.GetInt(PrefKeyHeight, 900);
                SetDisplayMode(savedWindowed, savedW, savedH);
            }
            else
            {
                // 최초 실행 시 현재 스크린 모드 감지
                IsWindowed = !Screen.fullScreen;
            }
        }
    }
}
