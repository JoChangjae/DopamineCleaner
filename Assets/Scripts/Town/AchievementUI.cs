using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DopamineCleaner.Meta;

namespace DopamineCleaner.Town
{
    public class AchievementUI : MonoBehaviour
    {
        [Header("UI 컴포넌트")]
        [SerializeField] private Text progressSummaryText;
        [SerializeField] private Slider progressSlider;
        [SerializeField] private Transform cardContainer;
        [SerializeField] private GameObject cardPrefab;

        private void Start()
        {
            RefreshUI();
        }

        public void RefreshUI()
        {
            if (GameDataManager.Instance == null) return;

            var list = AchievementDatabase.GetAll();
            int unlockedCount = 0;
            foreach (var a in list)
            {
                if (a.IsUnlocked) unlockedCount++;
            }

            if (progressSummaryText != null)
            {
                float pct = list.Count > 0 ? (float)unlockedCount / list.Count * 100f : 0f;
                progressSummaryText.text = $"🏆 디톡스 실적 달성 현황: {unlockedCount} / {list.Count} ({pct:F0}%)";
            }

            if (progressSlider != null)
            {
                progressSlider.maxValue = list.Count;
                progressSlider.value = unlockedCount;
            }

            if (cardContainer == null) return;

            // 기존 카드 정리
            foreach (Transform child in cardContainer)
            {
                Destroy(child.gameObject);
            }

            // 업적 목록 생성
            foreach (var ach in list)
            {
                CreateAchievementCard(ach);
            }
        }

        private void CreateAchievementCard(AchievementInfo ach)
        {
            GameObject card = new GameObject($"AchCard_{ach.id}");
            card.transform.SetParent(cardContainer, false);

            var le = card.AddComponent<LayoutElement>();
            le.minHeight = 72;
            le.preferredHeight = 72;
            le.flexibleWidth = 1;

            var img = card.AddComponent<Image>();
            img.color = ach.IsUnlocked ? new Color(0.12f, 0.28f, 0.24f, 0.95f) : new Color(0.18f, 0.20f, 0.26f, 0.85f);

            var hlg = card.AddComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(16, 16, 8, 8);
            hlg.spacing = 14;
            hlg.childControlWidth = false;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = true;

            // 1. 아이콘
            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(card.transform, false);
            var iconText = iconObj.AddComponent<Text>();
            iconText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            iconText.text = ach.icon;
            iconText.fontSize = 32;
            iconText.alignment = TextAnchor.MiddleCenter;
            iconObj.AddComponent<LayoutElement>().preferredWidth = 48;

            // 2. 설명 텍스트
            GameObject textObj = new GameObject("TextGroup");
            textObj.transform.SetParent(card.transform, false);
            var vlg = textObj.AddComponent<VerticalLayoutGroup>();
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.spacing = 3;
            textObj.AddComponent<LayoutElement>().flexibleWidth = 1;

            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(textObj.transform, false);
            var titleTxt = titleObj.AddComponent<Text>();
            titleTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            titleTxt.text = ach.title;
            titleTxt.fontSize = 18;
            titleTxt.fontStyle = FontStyle.Bold;
            titleTxt.color = ach.IsUnlocked ? new Color(1f, 0.9f, 0.3f) : new Color(0.9f, 0.9f, 0.9f);

            GameObject descObj = new GameObject("Desc");
            descObj.transform.SetParent(textObj.transform, false);
            var descTxt = descObj.AddComponent<Text>();
            descTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            descTxt.text = ach.description;
            descTxt.fontSize = 14;
            descTxt.color = new Color(0.75f, 0.85f, 0.85f);

            // 3. 보상 및 상태 뱃지
            GameObject badgeObj = new GameObject("Badge");
            badgeObj.transform.SetParent(card.transform, false);
            var badgeTxt = badgeObj.AddComponent<Text>();
            badgeTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            string rewardStr = ach.karmaReward > 0 ? $"+{ach.goldReward}G  +{ach.karmaReward}P" : $"+{ach.goldReward}G";
            badgeTxt.text = ach.IsUnlocked ? $"✅ 달성 완료\n({rewardStr})" : $"🔒 미달성\n({rewardStr})";
            badgeTxt.fontSize = 14;
            badgeTxt.alignment = TextAnchor.MiddleRight;
            badgeTxt.color = ach.IsUnlocked ? new Color(0.4f, 1f, 0.6f) : new Color(0.7f, 0.7f, 0.7f);
            badgeObj.AddComponent<LayoutElement>().preferredWidth = 130;
        }
    }
}
