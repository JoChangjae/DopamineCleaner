using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DopamineCleaner.Meta;
using DopamineCleaner.Core;

namespace DopamineCleaner.Town
{
    public class OfficeRemodelUI : MonoBehaviour
    {
        [Header("상단 상태 표시")]
        [SerializeField] private Text headerStatusText;
        [SerializeField] private Text buffSummaryText;

        [Header("가구 카드 컨테이너")]
        [SerializeField] private Transform cardContainer;

        private static Font koreanFont;

        private void Awake()
        {
            EnsureFont();
        }

        private static void EnsureFont()
        {
            if (koreanFont == null)
            {
#if UNITY_EDITOR
                koreanFont = UnityEditor.AssetDatabase.LoadAssetAtPath<Font>("Assets/Fonts/MalgunGothic.ttf");
#endif
                if (koreanFont == null)
                    koreanFont = Font.CreateDynamicFontFromOSFont(new string[] { "Malgun Gothic", "맑은 고딕", "Arial" }, 20);
            }
        }

        public void RefreshUI()
        {
            EnsureFont();
            if (GameDataManager.Instance == null) return;

            int totalLv = GameDataManager.Instance.GetTotalOfficeDecoLevel();
            string tierTitle = GameDataManager.Instance.GetOfficeTierTitle(totalLv);
            int gold = GameDataManager.Instance.Profile.gold;

            if (headerStatusText != null)
            {
                headerStatusText.font = koreanFont;
                headerStatusText.text = $"🏢 {tierTitle} (복지 총합 Lv.{totalLv})  |  💰 보유 골드: {gold:N0} G";
            }

            if (buffSummaryText != null)
            {
                buffSummaryText.font = koreanFont;
                float hpB = GameDataManager.Instance.GetTotalFacilityBuff(FacilityBuffType.MaxHpPercent);
                float cdB = GameDataManager.Instance.GetTotalFacilityBuff(FacilityBuffType.CooldownReduction);
                float crB = GameDataManager.Instance.GetTotalFacilityBuff(FacilityBuffType.CritChance);
                float spB = GameDataManager.Instance.GetTotalFacilityBuff(FacilityBuffType.MoveSpeed);
                float revB = GameDataManager.Instance.GetTotalFacilityBuff(FacilityBuffType.ReviveChance);
                float gldB = GameDataManager.Instance.GetTotalFacilityBuff(FacilityBuffType.GoldBonus);

                buffSummaryText.text =
                    $"<color=#6EE7B7>❤️ 체력 +{hpB * 100:F0}%</color>  |  " +
                    $"<color=#93C5FD>⏱️ 쿨타임 -{cdB * 100:F0}%</color>  |  " +
                    $"<color=#FCD34D>🎯 치명타 +{crB * 100:F0}%</color>  |  " +
                    $"<color=#C4B5FD>🏃 이속 +{spB * 100:F0}%</color>  |  " +
                    $"<color=#F472B6>💖 부활 +{revB * 100:F0}%</color>  |  " +
                    $"<color=#FDE047>💰 골드 +{gldB * 100:F0}%</color>";
            }

            if (cardContainer == null) return;

            // 기존 카드 정리
            for (int i = cardContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(cardContainer.GetChild(i).gameObject);
            }

            var allFacilities = OfficeFacilityDatabase.GetAll();
            foreach (var fac in allFacilities)
            {
                CreateFacilityCard(fac, gold);
            }
        }

        private void CreateFacilityCard(OfficeFacilityDefinition fac, int currentGold)
        {
            int currentLv = GameDataManager.Instance.GetFacilityLevel(fac.id);
            bool isMax = currentLv >= fac.maxLevel;
            int cost = fac.GetCost(currentLv);
            bool canAfford = currentGold >= cost;

            GameObject card = new GameObject($"Card_{fac.id}");
            card.transform.SetParent(cardContainer, false);

            Image cardBg = card.AddComponent<Image>();
            cardBg.color = isMax ? new Color(0.24f, 0.22f, 0.35f, 0.95f) : new Color(0.16f, 0.17f, 0.24f, 0.95f);

            RectTransform rt = card.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(490, 240);

            // 1. 헤더: 아이콘 & 이름
            CreateText("Title", card.transform, $"{fac.icon} {fac.name}", 20, TextAnchor.MiddleLeft, new Vector2(20, 85), new Vector2(340, 32), Color.white, FontStyle.Bold);

            // 2. 카테고리 배지
            CreateText("Category", card.transform, $"[{fac.category}]", 14, TextAnchor.MiddleRight, new Vector2(-20, 85), new Vector2(120, 26), new Color(0.55f, 0.85f, 1f));

            // 3. 레벨 별점 표시
            string stars = "";
            for (int s = 1; s <= fac.maxLevel; s++)
            {
                stars += (s <= currentLv) ? "★" : "☆";
            }
            string levelText = isMax ? $"<color=#FCD34D>{stars} (Lv.{currentLv} MAX)</color>" : $"{stars} (Lv.{currentLv}/{fac.maxLevel})";
            CreateText("LevelStars", card.transform, levelText, 16, TextAnchor.MiddleLeft, new Vector2(20, 52), new Vector2(300, 28), new Color(1f, 0.9f, 0.4f), FontStyle.Bold);

            // 4. 설명문
            CreateText("Desc", card.transform, fac.description, 14, TextAnchor.UpperLeft, new Vector2(20, 10), new Vector2(450, 48), new Color(0.85f, 0.85f, 0.9f));

            // 5. 현재 효과 및 다음 레벨 효과
            string currentBuffStr = currentLv > 0 ? fac.GetBuffSummary(currentLv) : "미설치";
            string nextBuffStr = isMax ? "최대 단계 달성" : fac.GetBuffSummary(currentLv + 1);
            string buffComparison = $"<b>[효과]</b> <color=#6EE7B7>{currentBuffStr}</color> " + (isMax ? "" : $"➜ <color=#93C5FD>{nextBuffStr}</color>");
            CreateText("BuffText", card.transform, buffComparison, 15, TextAnchor.MiddleLeft, new Vector2(20, -42), new Vector2(450, 28), Color.white);

            // 6. 업그레이드 버튼
            string btnLabel = isMax ? "⭐ 최대 복지 달성" : $"🔨 리모델링 ({cost:N0} G)";
            Color btnBgColor = isMax ? new Color(0.3f, 0.3f, 0.35f) : (canAfford ? new Color(0.25f, 0.6f, 0.4f) : new Color(0.45f, 0.25f, 0.25f));

            Button upBtn = CreateButton("UpgradeBtn", card.transform, btnLabel, 16, btnBgColor, new Vector2(0, -85), new Vector2(450, 40));
            if (!isMax)
            {
                string facIdCopy = fac.id;
                upBtn.onClick.AddListener(() =>
                {
                    if (GameDataManager.Instance.UpgradeFacility(facIdCopy))
                    {
                        SoundManager.Instance?.Play(SoundEffect.LevelUp, 0.85f);
                        RefreshUI();
                        // 본부 상단 골드 및 정보 갱신
                        TownManager tm = FindAnyObjectByType<TownManager>();
                        tm?.RefreshHeader();
                    }
                    else
                    {
                        SoundManager.Instance?.Play(SoundEffect.Hit, 0.5f);
                    }
                });
            }
        }

        private static Text CreateText(string name, Transform parent, string text, int fontSize, TextAnchor alignment, Vector2 pos, Vector2 size, Color color, FontStyle style = FontStyle.Normal)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            RectTransform rt = obj.AddComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            Text txt = obj.AddComponent<Text>();
            txt.font = koreanFont;
            txt.text = text;
            txt.fontSize = fontSize;
            txt.alignment = alignment;
            txt.color = color;
            txt.fontStyle = style;
            txt.supportRichText = true;
            return txt;
        }

        private static Button CreateButton(string name, Transform parent, string text, int fontSize, Color bgColor, Vector2 pos, Vector2 size)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            RectTransform rt = obj.AddComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            Image img = obj.AddComponent<Image>();
            img.color = bgColor;

            Button btn = obj.AddComponent<Button>();
            CreateText("BtnText", obj.transform, text, fontSize, TextAnchor.MiddleCenter, Vector2.zero, size, Color.white, FontStyle.Bold);
            return btn;
        }
    }
}
