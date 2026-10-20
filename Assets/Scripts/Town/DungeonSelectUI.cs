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
        [SerializeField] private Image mapPreviewImage;
        [SerializeField] private Text mapTitleText;
        [SerializeField] private Text mapDescText;
        [SerializeField] private Text difficultySummaryText;
        [SerializeField] private Text rewardPreviewText;

        [Header("블랙기업 근로 조약 버튼 목록")]
        [SerializeField] private Button pactOverworkBtn;
        [SerializeField] private Button pactAiPressureBtn;
        [SerializeField] private Button pactNotificationBtn;
        [SerializeField] private Button pactBlackoutBtn;
        [SerializeField] private Text pactSummaryText;

        [Header("하이퍼 모드 & 출동 버튼")]
        [SerializeField] private Button hyperModeToggleBtn;
        [SerializeField] private Text hyperModeText;
        [SerializeField] private Button startMissionButton;

        [Header("도파민 돌연변이 룰렛")]
        [SerializeField] private Button mutationSpinButton;
        [SerializeField] private Text mutationStatusText;

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

            if (pactOverworkBtn != null) pactOverworkBtn.onClick.AddListener(() => TogglePact(PactType.WeekendOverwork));
            if (pactAiPressureBtn != null) pactAiPressureBtn.onClick.AddListener(() => TogglePact(PactType.AIPressure));
            if (pactNotificationBtn != null) pactNotificationBtn.onClick.AddListener(() => TogglePact(PactType.NotificationBombard));
            if (pactBlackoutBtn != null) pactBlackoutBtn.onClick.AddListener(() => TogglePact(PactType.ServerBlackout));

            if (hyperModeToggleBtn != null)
                hyperModeToggleBtn.onClick.AddListener(ToggleHyperMode);

            if (mutationSpinButton != null)
                mutationSpinButton.onClick.AddListener(SpinMutationRoulette);

            if (startMissionButton != null)
                startMissionButton.onClick.AddListener(OnStartMissionClicked);

            RefreshUI();
        }

        private void TogglePact(PactType type)
        {
            if (GameDataManager.Instance != null)
            {
                GameDataManager.Instance.TogglePact(type);
                Core.SoundManager.Instance?.Play(Core.SoundEffect.ButtonClick, 0.6f);
                UpdatePreview();
            }
        }

        private void SpinMutationRoulette()
        {
            if (MutationRouletteManager.Instance != null)
            {
                var mut = MutationRouletteManager.Instance.SpinRoulette();
                UpdateMutationUI(mut);
            }
        }

        private void UpdateMutationUI(MutationDefinition mut)
        {
            if (mutationStatusText != null && mut != null)
            {
                mutationStatusText.text = $"🎰 [출격 돌연변이 룰렛]: {mut.icon} <b>{mut.title}</b> ({mut.benefit} / {mut.penalty})";
                mutationStatusText.color = mut.themeColor;
            }
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

            if (mapPreviewImage != null)
            {
                string spriteName = mapId switch
                {
                    "map_office" => "bg_office_carpet",
                    "map_highway" => "bg_highway_road",
                    _ => "bg_hospital_floor"
                };
                Sprite sp = Resources.Load<Sprite>($"Environment/{spriteName}") ??
#if UNITY_EDITOR
                    UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Sprites/Environment/{spriteName}.png");
#else
                    null;
#endif
                if (sp != null)
                {
                    mapPreviewImage.sprite = sp;
                    mapPreviewImage.gameObject.SetActive(true);
                }
            }

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

            // 근로 조약 UI 및 보너스 계산
            float pactGoldBonus = GameDataManager.Instance != null ? GameDataManager.Instance.GetActivePactGoldMultiplier() : 0f;
            float pactKarmaBonus = GameDataManager.Instance != null ? GameDataManager.Instance.GetActivePactKarmaMultiplier() : 0f;

            UpdatePactButtonVisual(pactOverworkBtn, PactType.WeekendOverwork, "⏰ 주말 특근 (+25%G)");
            UpdatePactButtonVisual(pactAiPressureBtn, PactType.AIPressure, "🤖 AI 압박 (+25%P)");
            UpdatePactButtonVisual(pactNotificationBtn, PactType.NotificationBombard, "📨 스팸 폭탄 (+25%Exp)");
            UpdatePactButtonVisual(pactBlackoutBtn, PactType.ServerBlackout, "🔌 블랙아웃 (+35%G)");

            int activePactCount = 0;
            if (GameDataManager.Instance != null)
            {
                foreach (PactType pt in System.Enum.GetValues(typeof(PactType)))
                {
                    if (GameDataManager.Instance.IsPactActive(pt)) activePactCount++;
                }
            }

            if (pactSummaryText != null)
            {
                pactSummaryText.text = activePactCount > 0
                    ? $"📜 [근로 조약]: {activePactCount}건 체결 중 (보너스 골드: +{pactGoldBonus * 100:F0}% / 카르마: +{pactKarmaBonus * 100:F0}%)"
                    : "📜 [근로 조약]: 미체결 (조약을 맺어 난이도를 올리고 추가 성과금을 획득하세요)";
                pactSummaryText.color = activePactCount > 0 ? new Color(1f, 0.45f, 0.4f) : new Color(0.7f, 0.75f, 0.85f);
            }

            if (rewardPreviewText != null)
            {
                float hyperMult = isHyper ? 2.0f : 1.0f;
                float totalGoldMult = mult.rewardGoldMult * hyperMult * (1f + pactGoldBonus);
                float totalKarmaMult = mult.rewardKarmaMult * hyperMult * (1f + pactKarmaBonus);

                int finalGold = Mathf.RoundToInt(map.baseGoldReward * totalGoldMult);
                int finalKarma = Mathf.RoundToInt(map.baseKarmaReward * totalKarmaMult);

                string bonusNote = "";
                if (isHyper) bonusNote += " 🔥 하이퍼 2배";
                if (pactGoldBonus > 0f) bonusNote += $" 📜 조약 +{pactGoldBonus * 100:F0}%";

                rewardPreviewText.text = $"🎯 클리어 성공 예상 보상:  💰 +{finalGold:N0} G  |  ✨ +{finalKarma} P" + bonusNote;
            }
        }

        private void UpdatePactButtonVisual(Button btn, PactType type, string label)
        {
            if (btn == null) return;
            bool isActive = GameDataManager.Instance != null && GameDataManager.Instance.IsPactActive(type);

            var img = btn.GetComponent<Image>();
            if (img != null)
            {
                img.color = isActive ? new Color(0.85f, 0.25f, 0.25f) : new Color(0.22f, 0.20f, 0.28f);
            }

            var txt = btn.GetComponentInChildren<Text>();
            if (txt != null)
            {
                txt.text = isActive ? $"✅ {label}" : $"⬛ {label}";
                txt.color = isActive ? Color.white : new Color(0.7f, 0.7f, 0.7f);
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
