using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DopamineCleaner.Meta;

namespace DopamineCleaner.Town
{
    public class DungeonSelectUI : MonoBehaviour
    {
        [Header("맵 선택 버튼 목록")]
        [SerializeField] private Button mapHospitalBtn;
        [SerializeField] private Button mapOfficeBtn;
        [SerializeField] private Button mapHighwayBtn;

        [Header("난이도 선택 버튼 목록")]
        [SerializeField] private Button diffEasyBtn;
        [SerializeField] private Button diffNormalBtn;
        [SerializeField] private Button diffHardBtn;
        [SerializeField] private Button diffNightmareBtn;

        [Header("정보 표시 영역")]
        [SerializeField] private Text mapTitleText;
        [SerializeField] private Text mapDescText;
        [SerializeField] private Text difficultySummaryText;
        [SerializeField] private Text rewardPreviewText;

        [Header("하이퍼 모드 & 출동 버튼")]
        [SerializeField] private Button hyperModeToggleBtn;
        [SerializeField] private Text hyperModeText;
        [SerializeField] private Button startMissionButton;

        private string currentMapId = "map_hospital";
        private DungeonDifficulty currentDiff = DungeonDifficulty.Normal;

        private void Start()
        {
            if (mapHospitalBtn != null) mapHospitalBtn.onClick.AddListener(() => SelectMap("map_hospital"));
            if (mapOfficeBtn != null) mapOfficeBtn.onClick.AddListener(() => SelectMap("map_office"));
            if (mapHighwayBtn != null) mapHighwayBtn.onClick.AddListener(() => SelectMap("map_highway"));

            if (diffEasyBtn != null) diffEasyBtn.onClick.AddListener(() => SelectDifficulty(DungeonDifficulty.Easy));
            if (diffNormalBtn != null) diffNormalBtn.onClick.AddListener(() => SelectDifficulty(DungeonDifficulty.Normal));
            if (diffHardBtn != null) diffHardBtn.onClick.AddListener(() => SelectDifficulty(DungeonDifficulty.Hard));
            if (diffNightmareBtn != null) diffNightmareBtn.onClick.AddListener(() => SelectDifficulty(DungeonDifficulty.Nightmare));

            if (hyperModeToggleBtn != null)
                hyperModeToggleBtn.onClick.AddListener(ToggleHyperMode);

            if (startMissionButton != null)
                startMissionButton.onClick.AddListener(OnStartMissionClicked);

            RefreshUI();
        }

        private void ToggleHyperMode()
        {
            if (GameDataManager.Instance != null)
            {
                GameDataManager.Instance.IsHyperMode = !GameDataManager.Instance.IsHyperMode;
                UpdatePreview();
            }
        }

        public void RefreshUI()
        {
            SelectMap(currentMapId);
            SelectDifficulty(currentDiff);
        }

        private void SelectMap(string mapId)
        {
            currentMapId = mapId;
            var map = DungeonDatabase.Get(mapId);

            if (mapTitleText != null)
                mapTitleText.text = $"{map.iconEmoji} {map.name} - {map.subTitle}";

            if (mapDescText != null)
                mapDescText.text = $"{map.description}\n[목표 생존 시간: {map.targetSurvivalSec / 60f:F0}분]";

            UpdatePreview();
        }

        private void SelectDifficulty(DungeonDifficulty diff)
        {
            currentDiff = diff;
            UpdatePreview();
        }

        private void UpdatePreview()
        {
            var map = DungeonDatabase.Get(currentMapId);
            var mult = DungeonDatabase.GetDifficultyMultiplier(currentDiff);

            if (difficultySummaryText != null)
            {
                difficultySummaryText.text = $"선택 난이도: [{currentDiff}]\n" +
                    $"• 적 체력: x{mult.enemyHpMult:F2}  |  공격력: x{mult.enemyDmgMult:F2}  |  이동속도: x{mult.enemySpeedMult:F2}\n" +
                    $"• 몬스터 스폰 주기: x{mult.spawnIntervalMult:F2}";
            }

            bool isHyper = GameDataManager.Instance != null && GameDataManager.Instance.IsHyperMode;
            if (hyperModeText != null)
            {
                hyperModeText.text = isHyper ? "⚡ [하이퍼 모드: ON] (보상 200% / 속도 1.25x)" : "⚡ [하이퍼 모드: OFF]";
                hyperModeText.color = isHyper ? new Color(1f, 0.85f, 0.2f) : Color.white;
            }

            if (rewardPreviewText != null)
            {
                float hyperMult = isHyper ? 2.0f : 1.0f;
                int finalGold = Mathf.RoundToInt(map.baseGoldReward * mult.rewardGoldMult * hyperMult);
                int finalKarma = Mathf.RoundToInt(map.baseKarmaReward * mult.rewardKarmaMult * hyperMult);
                rewardPreviewText.text = $"🎯 클리어 성공 예상 보상:  💰 +{finalGold:N0} G  |  ✨ +{finalKarma} P" + (isHyper ? " 🔥 (하이퍼 2배 적용)" : "");
            }
        }

        private void OnStartMissionClicked()
        {
            if (GameDataManager.Instance != null)
            {
                GameDataManager.Instance.SelectedMapId = currentMapId;
                GameDataManager.Instance.SelectedDifficulty = currentDiff;
            }

            SceneManager.LoadScene("MainGame");
        }
    }
}
