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

            // 최근 5회 작전 기록실 섹션 생성
            CreateCombatHistorySection();

            // 업적 목록 생성
            foreach (var ach in list)
            {
                CreateAchievementCard(ach);
            }
        }

        private void CreateCombatHistorySection()
        {
            var records = GameDataManager.Instance?.Profile?.combatRecords;

            // 섹션 헤더
            GameObject secHeader = new GameObject("CombatHistoryHeader");
            secHeader.transform.SetParent(cardContainer, false);
            var leH = secHeader.AddComponent<LayoutElement>();
            leH.minHeight = 44;
            leH.preferredHeight = 44;
            leH.flexibleWidth = 1;

            var imgH = secHeader.AddComponent<Image>();
            imgH.color = new Color(0.25f, 0.20f, 0.35f, 0.9f);

            var hTxtObj = new GameObject("HeaderTxt");
            hTxtObj.transform.SetParent(secHeader.transform, false);
            var hTxt = hTxtObj.AddComponent<Text>();
            hTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            hTxt.text = "📜 [최근 5회 정화 작전 일지 (Combat Record)]";
            hTxt.fontSize = 18;
            hTxt.fontStyle = FontStyle.Bold;
            hTxt.alignment = TextAnchor.MiddleCenter;
            hTxt.color = new Color(1f, 0.88f, 0.4f);
            var rtH = hTxtObj.GetComponent<RectTransform>();
            rtH.anchorMin = Vector2.zero;
            rtH.anchorMax = Vector2.one;
            rtH.sizeDelta = Vector2.zero;

            if (records == null || records.Count == 0)
            {
                GameObject emptyCard = new GameObject("CombatHistoryEmpty");
                emptyCard.transform.SetParent(cardContainer, false);
                var leE = emptyCard.AddComponent<LayoutElement>();
                leE.minHeight = 48;
                leE.preferredHeight = 48;
                leE.flexibleWidth = 1;
                var imgE = emptyCard.AddComponent<Image>();
                imgE.color = new Color(0.14f, 0.15f, 0.20f, 0.85f);

                var eTxtObj = new GameObject("EmptyTxt");
                eTxtObj.transform.SetParent(emptyCard.transform, false);
                var eTxt = eTxtObj.AddComponent<Text>();
                eTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                eTxt.text = "  (최근 출동 기록이 없습니다. 본부 출동처에서 첫 정화 작전을 시작하세요!)";
                eTxt.fontSize = 15;
                eTxt.alignment = TextAnchor.MiddleCenter;
                eTxt.color = new Color(0.7f, 0.7f, 0.8f);
                var rtE = eTxtObj.GetComponent<RectTransform>();
                rtE.anchorMin = Vector2.zero;
                rtE.anchorMax = Vector2.one;
                rtE.sizeDelta = Vector2.zero;
            }
            else
            {
                foreach (var rec in records)
                {
                    CreateCombatRecordCard(rec);
                }
            }

            // 업적 섹션 헤더 구분선
            GameObject achHeader = new GameObject("AchHeader");
            achHeader.transform.SetParent(cardContainer, false);
            var leAh = achHeader.AddComponent<LayoutElement>();
            leAh.minHeight = 44;
            leAh.preferredHeight = 44;
            leAh.flexibleWidth = 1;

            var imgAh = achHeader.AddComponent<Image>();
            imgAh.color = new Color(0.18f, 0.25f, 0.28f, 0.9f);

            var ahTxtObj = new GameObject("AchHeaderTxt");
            ahTxtObj.transform.SetParent(achHeader.transform, false);
            var ahTxt = ahTxtObj.AddComponent<Text>();
            ahTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            ahTxt.text = "🏆 [도파민 디톡스 실적 및 업적 (Achievements)]";
            ahTxt.fontSize = 18;
            ahTxt.fontStyle = FontStyle.Bold;
            ahTxt.alignment = TextAnchor.MiddleCenter;
            ahTxt.color = new Color(0.4f, 1f, 0.75f);
            var rtAh = ahTxtObj.GetComponent<RectTransform>();
            rtAh.anchorMin = Vector2.zero;
            rtAh.anchorMax = Vector2.one;
            rtAh.sizeDelta = Vector2.zero;
        }

        private void CreateCombatRecordCard(CombatRecord rec)
        {
            GameObject card = new GameObject("CombatCard");
            card.transform.SetParent(cardContainer, false);

            var le = card.AddComponent<LayoutElement>();
            le.minHeight = 60;
            le.preferredHeight = 60;
            le.flexibleWidth = 1;

            var img = card.AddComponent<Image>();
            img.color = rec.isCleared ? new Color(0.15f, 0.28f, 0.22f, 0.9f) : new Color(0.24f, 0.16f, 0.18f, 0.9f);

            var hlg = card.AddComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(16, 16, 6, 6);
            hlg.spacing = 15;
            hlg.childControlWidth = false;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = true;

            // 결과 뱃지
            GameObject badgeObj = new GameObject("ResultBadge");
            badgeObj.transform.SetParent(card.transform, false);
            var badgeTxt = badgeObj.AddComponent<Text>();
            badgeTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            badgeTxt.text = rec.isCleared ? "🏆 승리" : "💀 작전종료";
            badgeTxt.fontSize = 20;
            badgeTxt.fontStyle = FontStyle.Bold;
            badgeTxt.alignment = TextAnchor.MiddleCenter;
            badgeTxt.color = rec.isCleared ? new Color(0.4f, 1f, 0.6f) : new Color(1f, 0.45f, 0.45f);
            badgeObj.AddComponent<LayoutElement>().preferredWidth = 110;

            // 상세 내역
            GameObject infoObj = new GameObject("Info");
            infoObj.transform.SetParent(card.transform, false);
            var infoTxt = infoObj.AddComponent<Text>();
            infoTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            string charName = CharacterDatabase.Get(rec.characterId)?.name ?? rec.characterId;
            infoTxt.text = $"요원: {charName}  |  ⏱️ 생존: {rec.survivalSeconds / 60:D2}:{rec.survivalSeconds % 60:D2}  |  👾 소독: {rec.kills}마리\n💰 지원금: +{rec.goldEarned:N0} G   ✨ 디톡스: +{rec.karmaEarned} P";
            infoTxt.fontSize = 15;
            infoTxt.alignment = TextAnchor.MiddleLeft;
            infoTxt.color = Color.white;
            infoObj.AddComponent<LayoutElement>().flexibleWidth = 1;

            // 일시
            GameObject timeObj = new GameObject("Timestamp");
            timeObj.transform.SetParent(card.transform, false);
            var timeTxt = timeObj.AddComponent<Text>();
            timeTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            timeTxt.text = rec.timestamp;
            timeTxt.fontSize = 14;
            timeTxt.alignment = TextAnchor.MiddleRight;
            timeTxt.color = new Color(0.75f, 0.8f, 0.9f);
            timeObj.AddComponent<LayoutElement>().preferredWidth = 140;
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
