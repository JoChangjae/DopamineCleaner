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

        public struct WindowPreset
        {
            public int width;
            public int height;
            public string name;

            public WindowPreset(int w, int h, string n)
            {
                width = w;
                height = h;
                name = n;
            }
        }

        public static readonly WindowPreset[] Presets = new WindowPreset[]
        {
            new WindowPreset(1600, 900, "1600x900 (대형)"),
            new WindowPreset(1280, 720, "1280x720 (HD)"),
            new WindowPreset(960, 540, "960x540 (미니)")
        };

        public bool IsWindowed { get; private set; } = false;
        public int WindowWidth { get; private set; } = 1280;
        public int WindowHeight { get; private set; } = 720;
        public int CurrentPresetIndex { get; private set; } = 1; // 기본 1280x720

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
            // Windows PC 표준 단축키: F11 또는 Alt+Enter (전체화면 <-> 현재 창모드 토글)
            bool isAltPressed = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
            if (Input.GetKeyDown(KeyCode.F11) || (isAltPressed && Input.GetKeyDown(KeyCode.Return)))
            {
                ToggleDisplayMode();
            }

            // F10: 창모드 해상도 단계별 순환 (1600x900 -> 1280x720 -> 960x540)
            if (Input.GetKeyDown(KeyCode.F10))
            {
                CycleWindowPreset();
            }
        }

        public void ToggleDisplayMode()
        {
            SetDisplayMode(!IsWindowed, WindowWidth, WindowHeight);
        }

        /// <summary>
        /// 화면 모드 버튼 클릭 시: 전체화면 -> 1600x900 -> 1280x720 -> 960x540 순환
        /// </summary>
        public void CycleDisplayMode()
        {
            if (!IsWindowed)
            {
                CurrentPresetIndex = 0;
                var p = Presets[CurrentPresetIndex];
                SetDisplayMode(true, p.width, p.height);
            }
            else
            {
                CurrentPresetIndex++;
                if (CurrentPresetIndex < Presets.Length)
                {
                    var p = Presets[CurrentPresetIndex];
                    SetDisplayMode(true, p.width, p.height);
                }
                else
                {
                    CurrentPresetIndex = 0;
                    SetDisplayMode(false);
                }
            }
        }

        public void CycleWindowPreset()
        {
            CurrentPresetIndex = (CurrentPresetIndex + 1) % Presets.Length;
            var p = Presets[CurrentPresetIndex];
            SetDisplayMode(true, p.width, p.height);
        }

        public void SetDisplayMode(bool windowed, int width = 1280, int height = 720)
        {
            IsWindowed = windowed;
            WindowWidth = width;
            WindowHeight = height;

            for (int i = 0; i < Presets.Length; i++)
            {
                if (Presets[i].width == width && Presets[i].height == height)
                {
                    CurrentPresetIndex = i;
                    break;
                }
            }

            if (windowed)
            {
                Screen.SetResolution(width, height, FullScreenMode.Windowed);
                Debug.Log($"🖥️ [화면 모드 전환] 창모드 ({width}x{height}) 적용 완료 (F11:전체화면, F10:창크기)");
            }
            else
            {
                int fullW = Display.main.systemWidth > 0 ? Display.main.systemWidth : 1920;
                int fullH = Display.main.systemHeight > 0 ? Display.main.systemHeight : 1080;
                Screen.SetResolution(fullW, fullH, FullScreenMode.FullScreenWindow);
                Debug.Log($"🖥️ [화면 모드 전환] 전체화면 ({fullW}x{fullH}) 적용 완료 (F11:창모드)");
            }

            SaveDisplayMode();
            OnDisplayModeChanged?.Invoke(IsWindowed);
            SoundManager.Instance?.Play(SoundEffect.Dash, 0.35f);
        }

        public void SetWindowResolution(int width, int height)
        {
            SetDisplayMode(true, width, height);
        }

        public string GetDisplayModeLabel()
        {
            if (!IsWindowed)
            {
                return "🖥️ 화면: 전체화면 (F11)";
            }

            string presetName = Presets[Mathf.Clamp(CurrentPresetIndex, 0, Presets.Length - 1)].name;
            return $"🖥️ 창모드: {presetName}";
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
                int savedW = PlayerPrefs.GetInt(PrefKeyWidth, 1280);
                int savedH = PlayerPrefs.GetInt(PrefKeyHeight, 720);
                SetDisplayMode(savedWindowed, savedW, savedH);
            }
            else
            {
                IsWindowed = !Screen.fullScreen;
                if (IsWindowed)
                {
                    WindowWidth = 1280;
                    WindowHeight = 720;
                    CurrentPresetIndex = 1;
                }
            }
        }
    }
}
