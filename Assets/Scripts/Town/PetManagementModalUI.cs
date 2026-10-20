using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DopamineCleaner.Characters;
using DopamineCleaner.Meta;

namespace DopamineCleaner.Town
{
    public class PetManagementModalUI : MonoBehaviour
    {
        public static PetManagementModalUI Instance { get; private set; }

        [SerializeField] private GameObject modalPanel;
        [SerializeField] private Transform cardContainer;
        [SerializeField] private Button closeButton;
        [SerializeField] private Text goldStatusText;

        private static Font koreanFont;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else { Destroy(gameObject); return; }

            EnsureFont();
            if (closeButton != null) closeButton.onClick.AddListener(CloseModal);
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

        public void ToggleModal()
        {
            if (modalPanel == null) return;
            bool nextState = !modalPanel.activeSelf;
            if (nextState) OpenModal();
            else CloseModal();
        }

        public void OpenModal()
        {
            if (modalPanel != null)
            {
                modalPanel.SetActive(true);
                RefreshUI();
            }
        }

        public void CloseModal()
        {
            if (modalPanel != null)
            {
                modalPanel.SetActive(false);
            }
        }

        public void RefreshUI()
        {
            if (GameDataManager.Instance == null) return;

            if (goldStatusText != null)
            {
                goldStatusText.font = koreanFont;
                goldStatusText.text = $"💰 보유 골드: {GameDataManager.Instance.Profile.gold:N0} G";
            }

            if (cardContainer == null) return;

            // Clear old cards
            for (int i = cardContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(cardContainer.GetChild(i).gameObject);
            }

            string equippedId = GameDataManager.Instance.Profile.equippedCompanionId ?? "pet_drone";
            var companions = CompanionDatabase.GetAll();

            foreach (var comp in companions)
            {
                CreatePetCard(comp, equippedId);
            }
        }

        private void CreatePetCard(CompanionInfo info, string equippedId)
        {
            int currentTier = GameDataManager.Instance.GetCompanionTier(info.id);
            bool isEquipped = (info.id == equippedId);
            int upgradeCost = currentTier switch
            {
                1 => 500,
                2 => 1200,
                _ => 0
            };

            GameObject card = new GameObject($"PetCard_{info.id}");
            card.transform.SetParent(cardContainer, false);

            Image bg = card.AddComponent<Image>();
            bg.color = isEquipped ? new Color(0.22f, 0.32f, 0.42f, 0.95f) : new Color(0.16f, 0.16f, 0.22f, 0.95f);

            RectTransform rt = card.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(280, 420);

            // 1. 헤더 (이모지 & 이름)
            CreateText("Title", card.transform, $"{info.icon} {info.name}", 22, TextAnchor.MiddleCenter, new Vector2(0, 160), new Vector2(260, 40), Color.white, FontStyle.Bold);

            // 2. 티어 배지
            string tierStars = currentTier switch
            {
                1 => "★☆☆ (Tier 1)",
                2 => "★★☆ (Tier 2)",
                _ => "★★★ (MAX Tier 3)"
            };
            Color tierCol = currentTier == 3 ? new Color(1f, 0.85f, 0.2f) : (currentTier == 2 ? new Color(0.4f, 0.9f, 1f) : new Color(0.8f, 0.8f, 0.8f));
            CreateText("Tier", card.transform, tierStars, 18, TextAnchor.MiddleCenter, new Vector2(0, 120), new Vector2(260, 30), tierCol, FontStyle.Bold);

            // 3. 설명문
            string effectDetail = GetTierEffectDetail(info.type, currentTier);
            CreateText("Desc", card.transform, $"{info.description}\n\n<color=#A7F3D0><b>[현재 효과]</b>\n{effectDetail}</color>", 15, TextAnchor.UpperLeft, new Vector2(0, 15), new Vector2(250, 150), new Color(0.85f, 0.88f, 0.92f));

            // 4. 출격 장착 버튼
            Button equipBtn = CreateButton("EquipBtn", card.transform, isEquipped ? "✅ 현재 동행 중" : "🐾 동행 출격", 16, isEquipped ? new Color(0.25f, 0.5f, 0.35f) : new Color(0.3f, 0.45f, 0.7f), new Vector2(0, -115), new Vector2(240, 42));
            if (!isEquipped)
            {
                string idCopy = info.id;
                equipBtn.onClick.AddListener(() =>
                {
                    GameDataManager.Instance.SelectCompanion(idCopy);
                    RefreshUI();
                });
            }

            // 5. 스킬 강화 버튼
            string upText = currentTier >= 3 ? "⭐ 최고 레벨 달성" : $"⚡ 스킬 강화 ({upgradeCost}G)";
            Color upCol = currentTier >= 3 ? new Color(0.3f, 0.3f, 0.35f) : new Color(0.7f, 0.45f, 0.2f);
            Button upBtn = CreateButton("UpgradeBtn", card.transform, upText, 16, upCol, new Vector2(0, -165), new Vector2(240, 42));
            if (currentTier < 3)
            {
                string idCopy = info.id;
                int costCopy = upgradeCost;
                upBtn.onClick.AddListener(() =>
                {
                    if (GameDataManager.Instance.UpgradeCompanionTier(idCopy, costCopy))
                    {
                        Core.SoundManager.Instance?.Play(Core.SoundEffect.LevelUp, 0.8f);
                        RefreshUI();
                    }
                });
            }
        }

        private string GetTierEffectDetail(CompanionType type, int tier)
        {
            return type switch
            {
                CompanionType.HappyDog => $"• 보석 수거 반경: {7f + tier * 2.5f:F1}m\n• 수거 쿨타임: {1.4f - tier * 0.2f:F1}초",
                CompanionType.NapCat => $"• 쉼터 치유량: +{16 + tier * 10} HP\n• 발동 주기: {15f - tier * 2f:F0}초",
                CompanionType.PomodoroDrone => $"• 사격 주기: {1.2f - tier * 0.25f:F2}초\n• 집중 시간: 25초 / 2배 경험치",
                CompanionType.BombHamster => $"• 폭격 피해: {55 + (tier - 1) * 45} DMG\n• 폭격 주기: {3.8f - tier * 0.7f:F1}초",
                _ => "특수 스킬 없음"
            };
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
