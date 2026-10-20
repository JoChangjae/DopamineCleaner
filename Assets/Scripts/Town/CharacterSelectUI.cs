using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DopamineCleaner.Characters;
using DopamineCleaner.Meta;

namespace DopamineCleaner.Town
{
    public class CharacterSelectUI : MonoBehaviour
    {
        [Header("캐릭터 카드 컨테이너")]
        [SerializeField] private Transform cardContainer;
        [SerializeField] private GameObject characterCardPrefab;

        private GameObject companionBar;

        public void RefreshUI()
        {
            if (GameDataManager.Instance == null) return;

            RefreshCharacterCards();
            RefreshCompanionBar();
        }

        private void RefreshCharacterCards()
        {
            if (cardContainer == null || characterCardPrefab == null) return;

            foreach (Transform child in cardContainer)
            {
                Destroy(child.gameObject);
            }

            var allChars = CharacterDatabase.GetAll();
            var profile = GameDataManager.Instance.Profile;

            foreach (var charDef in allChars)
            {
                CreateCard(charDef, profile);
            }
        }

        private void CreateCard(CharacterDefinition def, PlayerProfileData profile)
        {
            GameObject card = Instantiate(characterCardPrefab, cardContainer);

            Text iconText = card.transform.Find("Icon")?.GetComponent<Text>();
            Text nameText = card.transform.Find("Name")?.GetComponent<Text>();
            Text titleText = card.transform.Find("Title")?.GetComponent<Text>();
            Text statsText = card.transform.Find("Stats")?.GetComponent<Text>();
            Text passiveText = card.transform.Find("Passive")?.GetComponent<Text>();
            Button actionBtn = card.transform.Find("ActionButton")?.GetComponent<Button>();
            Text btnText = actionBtn?.GetComponentInChildren<Text>();

            if (iconText != null) iconText.text = def.iconEmoji;
            if (nameText != null) nameText.text = def.name;
            if (titleText != null) titleText.text = def.jobTitle;

            if (statsText != null)
            {
                statsText.text = $"체력: {def.baseMaxHP:F0}  |  이속: {def.baseMoveSpeed:F1}  |  공격력: x{def.baseDamageMult:F2}";
            }

            if (passiveText != null)
            {
                passiveText.text = $"[고유 특성]\n{def.passiveDesc}";
            }

            bool isUnlocked = profile.unlockedCharacterIds.Contains(def.id);
            bool isCurrent = profile.selectedCharacterId == def.id;

            if (actionBtn != null && btnText != null)
            {
                actionBtn.onClick.RemoveAllListeners();

                if (isCurrent)
                {
                    btnText.text = "✅ 출동 중인 요원";
                    actionBtn.interactable = false;
                }
                else if (isUnlocked)
                {
                    btnText.text = "요원 선택";
                    actionBtn.interactable = true;
                    actionBtn.onClick.AddListener(() =>
                    {
                        Core.SoundManager.Instance?.Play(Core.SoundEffect.Dash, 0.5f);
                        GameDataManager.Instance.SelectCharacter(def.id);
                        RefreshUI();
                    });
                }
                else
                {
                    string costStr = $"{def.unlockGoldCost}G";
                    if (def.unlockKarmaCost > 0) costStr += $" + {def.unlockKarmaCost}P";
                    btnText.text = $"🔓 영입 ({costStr})";

                    bool canAfford = profile.gold >= def.unlockGoldCost && profile.karmaPoints >= def.unlockKarmaCost;
                    actionBtn.interactable = canAfford;

                    actionBtn.onClick.AddListener(() =>
                    {
                        if (GameDataManager.Instance.UnlockCharacter(def.id))
                        {
                            Core.SoundManager.Instance?.Play(Core.SoundEffect.LevelUp, 0.8f);
                            RefreshUI();
                        }
                    });
                }
            }
        }

        private void RefreshCompanionBar()
        {
            if (companionBar == null)
            {
                companionBar = new GameObject("CompanionSelectorBar");
                companionBar.transform.SetParent(transform, false);

                RectTransform rt = companionBar.AddComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.5f, 0f);
                rt.anchorMax = new Vector2(0.5f, 0f);
                rt.pivot = new Vector2(0.5f, 0f);
                rt.sizeDelta = new Vector2(1650, 65);
                rt.anchoredPosition = new Vector2(0, 5);

                Image bg = companionBar.AddComponent<Image>();
                bg.color = new Color(0.14f, 0.11f, 0.22f, 0.95f);
                var outl = companionBar.AddComponent<Outline>();
                outl.effectColor = new Color(0.35f, 0.85f, 1f, 0.7f);
                outl.effectDistance = new Vector2(1f, -1f);

                HorizontalLayoutGroup hlg = companionBar.AddComponent<HorizontalLayoutGroup>();
                hlg.spacing = 20;
                hlg.childAlignment = TextAnchor.MiddleCenter;
                hlg.childControlWidth = false;
                hlg.childControlHeight = false;
            }

            foreach (Transform child in companionBar.transform)
            {
                Destroy(child.gameObject);
            }

            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var profile = GameDataManager.Instance.Profile;

            // 라벨 텍스트
            GameObject lblObj = new GameObject("CompLabel");
            lblObj.transform.SetParent(companionBar.transform, false);
            RectTransform lRt = lblObj.AddComponent<RectTransform>();
            lRt.sizeDelta = new Vector2(280, 50);
            Text lTxt = lblObj.AddComponent<Text>();
            lTxt.text = "🐾 [전술 동반자 드론/펫]:";
            lTxt.fontSize = 18;
            lTxt.alignment = TextAnchor.MiddleCenter;
            lTxt.color = new Color(1f, 0.85f, 0.3f);
            lTxt.font = font;

            var companions = CompanionDatabase.GetAll();
            foreach (var comp in companions)
            {
                GameObject btnObj = new GameObject($"Comp_{comp.id}");
                btnObj.transform.SetParent(companionBar.transform, false);
                RectTransform bRt = btnObj.AddComponent<RectTransform>();
                bRt.sizeDelta = new Vector2(380, 50);

                Image img = btnObj.AddComponent<Image>();
                bool isEquipped = profile.equippedCompanionId == comp.id;
                bool isUnlocked = profile.unlockedCompanionIds != null && profile.unlockedCompanionIds.Contains(comp.id);

                img.color = isEquipped ? new Color(0.25f, 0.55f, 0.4f) : new Color(0.20f, 0.18f, 0.28f);

                Button btn = btnObj.AddComponent<Button>();
                var bOut = btnObj.AddComponent<Outline>();
                bOut.effectColor = isEquipped ? new Color(0.4f, 1f, 0.6f) : new Color(0f, 0f, 0f, 0.6f);
                bOut.effectDistance = new Vector2(1.5f, -1.5f);

                GameObject txtObj = new GameObject("Text");
                txtObj.transform.SetParent(btnObj.transform, false);
                RectTransform tRt = txtObj.AddComponent<RectTransform>();
                tRt.anchorMin = Vector2.zero;
                tRt.anchorMax = Vector2.one;
                tRt.sizeDelta = Vector2.zero;

                Text t = txtObj.AddComponent<Text>();
                string status = isEquipped ? "✅ [동행 중]" : (isUnlocked ? "동행 선택" : "500G 영입");
                t.text = $"{comp.icon} {comp.name} {status}";
                t.fontSize = 15;
                t.alignment = TextAnchor.MiddleCenter;
                t.color = Color.white;
                t.font = font;

                btn.onClick.AddListener(() =>
                {
                    if (isUnlocked)
                    {
                        Core.SoundManager.Instance?.Play(Core.SoundEffect.Dash, 0.5f);
                        GameDataManager.Instance.SelectCompanion(comp.id);
                        RefreshUI();
                    }
                    else
                    {
                        if (GameDataManager.Instance.UnlockCompanion(comp.id, 500))
                        {
                            Core.SoundManager.Instance?.Play(Core.SoundEffect.LevelUp, 0.7f);
                            RefreshUI();
                        }
                    }
                });
            }
        }
    }
}
