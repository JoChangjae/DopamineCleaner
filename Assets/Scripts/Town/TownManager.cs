using UnityEngine;
using UnityEngine.UI;
using DopamineCleaner.Meta;

namespace DopamineCleaner.Town
{
    public class TownManager : MonoBehaviour
    {
        public static TownManager Instance { get; private set; }

        [Header("상단 재화 및 정보 헤더")]
        [SerializeField] private Text goldText;
        [SerializeField] private Text karmaText;
        [SerializeField] private Text activeCharacterText;

        [Header("탭 패널")]
        [SerializeField] private GameObject characterPanel;
        [SerializeField] private GameObject shopPanel;
        [SerializeField] private GameObject storagePanel;
        [SerializeField] private GameObject researchPanel;
        [SerializeField] private GameObject achievementPanel;
        [SerializeField] private GameObject dungeonPanel;

        [Header("탭 버튼")]
        [SerializeField] private Button tabCharBtn;
        [SerializeField] private Button tabShopBtn;
        [SerializeField] private Button tabStorageBtn;
        [SerializeField] private Button tabResearchBtn;
        [SerializeField] private Button tabAchievementBtn;
        [SerializeField] private Button tabDungeonBtn;

        [Header("화면 모드 버튼")]
        [SerializeField] private Button windowModeBtn;
        [SerializeField] private Text windowModeBtnText;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            // GameDataManager가 없을 경우 생성 (직접 TownScene을 실행했을 때 대응)
            if (GameDataManager.Instance == null)
            {
                GameObject dm = new GameObject("GameDataManager");
                dm.AddComponent<GameDataManager>();
            }

            GameDataManager.Instance.OnProfileUpdated += RefreshHeader;

            tabCharBtn.onClick.AddListener(() => SwitchTab(0));
            tabShopBtn.onClick.AddListener(() => SwitchTab(1));
            tabStorageBtn.onClick.AddListener(() => SwitchTab(2));
            if (tabResearchBtn != null) tabResearchBtn.onClick.AddListener(() => SwitchTab(3));
            if (tabAchievementBtn != null) tabAchievementBtn.onClick.AddListener(() => SwitchTab(4));
            tabDungeonBtn.onClick.AddListener(() => SwitchTab(5));

            if (windowModeBtn != null)
            {
                windowModeBtn.onClick.AddListener(OnWindowModeToggleClicked);
            }
            Core.DisplayModeManager.OnDisplayModeChanged += HandleDisplayModeChanged;
            UpdateWindowModeText();

            RefreshHeader();
            SwitchTab(0); // 기본 탭: 캐릭터 선택소
        }

        private void OnDestroy()
        {
            if (GameDataManager.Instance != null)
            {
                GameDataManager.Instance.OnProfileUpdated -= RefreshHeader;
            }
            Core.DisplayModeManager.OnDisplayModeChanged -= HandleDisplayModeChanged;
        }

        public void OnWindowModeToggleClicked()
        {
            Core.DisplayModeManager.Instance?.CycleDisplayMode();
            UpdateWindowModeText();
        }

        private void HandleDisplayModeChanged(bool isWindowed)
        {
            UpdateWindowModeText();
        }

        private void UpdateWindowModeText()
        {
            if (windowModeBtnText != null && Core.DisplayModeManager.Instance != null)
            {
                windowModeBtnText.text = Core.DisplayModeManager.Instance.GetDisplayModeLabel();
            }
        }

        public void RefreshHeader()
        {
            if (GameDataManager.Instance == null) return;

            var profile = GameDataManager.Instance.Profile;
            var curChar = CharacterDatabase.Get(profile.selectedCharacterId);

            if (goldText != null)
                goldText.text = $"💰 보유 골드: {profile.gold:N0} G";

            if (karmaText != null)
                karmaText.text = $"✨ 디톡스 포인트: {profile.karmaPoints} P";

            if (activeCharacterText != null)
                activeCharacterText.text = $"요원증: {curChar.iconEmoji} {curChar.name} ({curChar.jobTitle})";
        }

        public void SwitchTab(int tabIndex)
        {
            if (characterPanel != null) characterPanel.SetActive(tabIndex == 0);
            if (shopPanel != null) shopPanel.SetActive(tabIndex == 1);
            if (storagePanel != null) storagePanel.SetActive(tabIndex == 2);
            if (researchPanel != null) researchPanel.SetActive(tabIndex == 3);
            if (achievementPanel != null) achievementPanel.SetActive(tabIndex == 4);
            if (dungeonPanel != null) dungeonPanel.SetActive(tabIndex == 5);

            // 탭 열릴 때 해당 UI 갱신
            if (tabIndex == 0) GetComponentInChildren<CharacterSelectUI>(true)?.RefreshUI();
            if (tabIndex == 1) GetComponentInChildren<ShopUI>(true)?.RefreshUI();
            if (tabIndex == 2) GetComponentInChildren<StorageUI>(true)?.RefreshUI();
            if (tabIndex == 3) GetComponentInChildren<ResearchUI>(true)?.RefreshUI();
            if (tabIndex == 4) GetComponentInChildren<AchievementUI>(true)?.RefreshUI();
            if (tabIndex == 5) GetComponentInChildren<DungeonSelectUI>(true)?.RefreshUI();
        }
    }
}
