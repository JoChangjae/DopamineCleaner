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

        [Header("출동 버튼")]
        [SerializeField] private Button startMissionButton;

        private string currentMapId = "map_hospital";
        private DungeonDifficulty currentDiff = DungeonDifficulty.Normal;

        private void Start()
        {
            mapHospitalBtn.onClick.AddListener(() => SelectMap("map_hospital"));
            mapOfficeBtn.onClick.AddListener(() => SelectMap("map_office"));
            mapHighwayBtn.onClick.AddListener(() => SelectMap("map_highway"));

            diffEasyBtn.onClick.AddListener(() => SelectDifficulty(DungeonDifficulty.Easy));
            diffNormalBtn.onClick.AddListener(() => SelectDifficulty(DungeonDifficulty.Normal));
            diffHardBtn.onClick.AddListener(() => SelectDifficulty(DungeonDifficulty.Hard));
            diffNightmareBtn.onClick.AddListener(() => SelectDifficulty(DungeonDifficulty.Nightmare));

            startMissionButton.onClick.AddListener(OnStartMissionClicked);

            RefreshUI();
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

            if (rewardPreviewText != null)
            {
                int finalGold = Mathf.RoundToInt(map.baseGoldReward * mult.rewardGoldMult);
                int finalKarma = Mathf.RoundToInt(map.baseKarmaReward * mult.rewardKarmaMult);
                rewardPreviewText.text = $"🎯 클리어 성공 예상 보상:  💰 +{finalGold:N0} G  |  ✨ +{finalKarma} P";
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
