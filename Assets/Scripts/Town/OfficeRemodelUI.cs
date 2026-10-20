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

        [Header("복지 기금 수령")]
        [SerializeField] private Text welfareFundInfoText;
        [SerializeField] private Button welfareClaimBtn;
        [SerializeField] private Text welfareClaimBtnText;

        [Header("세트 시너지 컨테이너")]
        [SerializeField] private Transform synergyContainer;

        [Header("사옥 디오라마 뷰")]
        [SerializeField] private TownOfficeDiorama officeDiorama;

        [Header("가구 카드 컨테이너")]
        [SerializeField] private Transform cardContainer;

        private static Font koreanFont;

        private void Awake()
        {
            EnsureFont();
        }

        private void Start()
        {
            if (welfareClaimBtn != null)
            {
                welfareClaimBtn.onClick.AddListener(OnClaimWelfareClicked);
            }
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

        private void OnClaimWelfareClicked()
        {
            if (GameDataManager.Instance == null) return;
            if (GameDataManager.Instance.ClaimWelfareFund(out int gold, out int karma))
            {
                SoundManager.Instance?.Play(SoundEffect.LevelUp, 0.9f);
                RefreshUI();

                // 타운 헤더 갱신
                TownManager tm = FindAnyObjectByType<TownManager>();
                tm?.RefreshHeader();
            }
            else
            {
                SoundManager.Instance?.Play(SoundEffect.Hit, 0.4f);
            }
        }

        public void RefreshUI()
        {
            EnsureFont();
            if (GameDataManager.Instance == null) return;

            int totalLv = GameDataManager.Instance.GetTotalOfficeDecoLevel();
            string tierTitle = GameDataManager.Instance.GetOfficeTierTitle(totalLv);
            int gold = GameDataManager.Instance.Profile.gold;

            // 1. 헤더 갱신
            if (headerStatusText != null)
            {
                headerStatusText.font = koreanFont;
                headerStatusText.text = $"🏢 {tierTitle} (복지 총합 Lv.{totalLv})  |  💰 보유 골드: {gold:N0} G";
            }

            // 2. 버프 요약
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

            // 3. 방치형 사내 복지 기금 갱신
            int pendingGold = GameDataManager.Instance.GetPendingWelfareGold();
            int pendingKarma = GameDataManager.Instance.GetPendingWelfareKarma();
            int goldPerMin = 10 + totalLv * 6;
            int maxCap = 2500 + totalLv * 350;

            if (welfareFundInfoText != null)
            {
                welfareFundInfoText.font = koreanFont;
                string karmaStr = totalLv >= 5 ? $" + <color=#93C5FD>{pendingKarma} P</color>" : "";
                welfareFundInfoText.text =
                    $"🎁 <b>[사내 복지 기금 보관소]</b> 미수령 기금: <color=#FDE047>{pendingGold:N0} G</color>{karmaStr} " +
                    $"<color=#94A3B8>(분당 +{goldPerMin} G 누적 중 / 최대 {maxCap:N0} G)</color>";
            }

            if (welfareClaimBtn != null && welfareClaimBtnText != null)
            {
                welfareClaimBtnText.font = koreanFont;
                bool hasFund = (pendingGold > 0 || pendingKarma > 0);
                welfareClaimBtn.interactable = hasFund;
                if (hasFund)
                {
                    string kStr = pendingKarma > 0 ? $", +{pendingKarma} P" : "";
                    welfareClaimBtnText.text = $"💰 기금 일괄 수령 (+{pendingGold:N0} G{kStr})";
                    welfareClaimBtn.GetComponent<Image>().color = new Color(0.2f, 0.6f, 0.45f);
                }
                else
                {
                    welfareClaimBtnText.text = "📦 복지 기금 적립 대기 중";
                    welfareClaimBtn.GetComponent<Image>().color = new Color(0.25f, 0.28f, 0.35f);
                }
            }

            // 4. 복지 세트 시너지 갱신
            RefreshSynergyBadges();

            // 5. 디오라마 뷰 갱신
            if (officeDiorama != null)
            {
                officeDiorama.RefreshDiorama();
            }

            // 6. 가구 카드 목록 갱신
            if (cardContainer != null)
            {
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
        }

        private void RefreshSynergyBadges()
        {
            if (synergyContainer == null) return;

            for (int i = synergyContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(synergyContainer.GetChild(i).gameObject);
            }

            var synergies = OfficeSynergyDatabase.GetAll();
            foreach (var syn in synergies)
            {
                bool isActive = syn.IsActive(GameDataManager.Instance.GetFacilityLevel);

                GameObject badgeObj = new GameObject($"Synergy_{syn.id}");
                badgeObj.transform.SetParent(synergyContainer, false);

                RectTransform bRt = badgeObj.AddComponent<RectTransform>();
                bRt.sizeDelta = new Vector2(480f, 42f);

                Image bg = badgeObj.AddComponent<Image>();
                bg.color = isActive ? new Color(0.24f, 0.20f, 0.38f, 0.95f) : new Color(0.14f, 0.15f, 0.20f, 0.85f);

                // 테두리
                GameObject outlineObj = new GameObject("Outline");
                outlineObj.transform.SetParent(badgeObj.transform, false);
                RectTransform oRt = outlineObj.AddComponent<RectTransform>();
                oRt.anchorMin = Vector2.zero;
                oRt.anchorMax = Vector2.one;
                oRt.sizeDelta = new Vector2(2, 2);
                Image oImg = outlineObj.AddComponent<Image>();
                oImg.color = isActive ? new Color(1f, 0.85f, 0.3f, 0.85f) : new Color(0.3f, 0.35f, 0.45f, 0.35f);
                outlineObj.transform.SetAsFirstSibling();

                // 텍스트 라벨
                string statusTag = isActive ? "<color=#FCD34D>✨ [활성]</color>" : "<color=#64748B>🔒 [미달성]</color>";
                string displayText = $"{statusTag} <b>{syn.name}</b>: {syn.effectDescription}";

                Text txt = CreateText("BadgeText", badgeObj.transform, displayText, 13, TextAnchor.MiddleLeft, new Vector2(12, 0), new Vector2(456, 36), isActive ? Color.white : new Color(0.7f, 0.75f, 0.85f));
                txt.font = koreanFont;
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
            rt.sizeDelta = new Vector2(490, 225);

            // 1. 헤더: 아이콘 & 이름
            CreateText("Title", card.transform, $"{fac.icon} {fac.name}", 19, TextAnchor.MiddleLeft, new Vector2(20, 80), new Vector2(340, 30), Color.white, FontStyle.Bold);

            // 2. 카테고리 배지
            CreateText("Category", card.transform, $"[{fac.category}]", 13, TextAnchor.MiddleRight, new Vector2(-20, 80), new Vector2(120, 26), new Color(0.55f, 0.85f, 1f));

            // 3. 레벨 별점 표시
            string stars = "";
            for (int s = 1; s <= fac.maxLevel; s++)
            {
                stars += (s <= currentLv) ? "★" : "☆";
            }
            string levelText = isMax ? $"<color=#FCD34D>{stars} (Lv.{currentLv} MAX)</color>" : $"{stars} (Lv.{currentLv}/{fac.maxLevel})";
            CreateText("LevelStars", card.transform, levelText, 15, TextAnchor.MiddleLeft, new Vector2(20, 48), new Vector2(300, 26), new Color(1f, 0.9f, 0.4f), FontStyle.Bold);

            // 4. 설명문
            CreateText("Desc", card.transform, fac.description, 13, TextAnchor.UpperLeft, new Vector2(20, 10), new Vector2(450, 42), new Color(0.85f, 0.85f, 0.9f));

            // 5. 현재 효과 및 다음 레벨 효과
            string currentBuffStr = currentLv > 0 ? fac.GetBuffSummary(currentLv) : "미설치";
            string nextBuffStr = isMax ? "최대 단계 달성" : fac.GetBuffSummary(currentLv + 1);
            string buffComparison = $"<b>[효과]</b> <color=#6EE7B7>{currentBuffStr}</color> " + (isMax ? "" : $"➜ <color=#93C5FD>{nextBuffStr}</color>");
            CreateText("BuffText", card.transform, buffComparison, 14, TextAnchor.MiddleLeft, new Vector2(20, -36), new Vector2(450, 26), Color.white);

            // 6. 업그레이드 버튼
            string btnLabel = isMax ? "⭐ 최대 복지 달성" : $"🔨 리모델링 ({cost:N0} G)";
            Color btnBgColor = isMax ? new Color(0.3f, 0.3f, 0.35f) : (canAfford ? new Color(0.25f, 0.6f, 0.4f) : new Color(0.45f, 0.25f, 0.25f));

            Button upBtn = CreateButton("UpgradeBtn", card.transform, btnLabel, 15, btnBgColor, new Vector2(0, -78), new Vector2(450, 38));
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
