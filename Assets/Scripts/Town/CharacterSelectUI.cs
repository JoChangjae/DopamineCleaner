using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DopamineCleaner.Meta;

namespace DopamineCleaner.Town
{
    public class CharacterSelectUI : MonoBehaviour
    {
        [Header("캐릭터 카드 컨테이너")]
        [SerializeField] private Transform cardContainer;
        [SerializeField] private GameObject characterCardPrefab;

        public void RefreshUI()
        {
            if (cardContainer == null || GameDataManager.Instance == null) return;

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
                passiveText.text = $"[특약 패시브]\n{def.passiveDesc}";
            }

            bool isUnlocked = profile.unlockedCharacterIds.Contains(def.id);
            bool isCurrent = profile.selectedCharacterId == def.id;

            if (actionBtn != null && btnText != null)
            {
                actionBtn.onClick.RemoveAllListeners();

                if (isCurrent)
                {
                    btnText.text = "✅ 현장 출동 사원";
                    actionBtn.interactable = false;
                }
                else if (isUnlocked)
                {
                    btnText.text = "사원 선택";
                    actionBtn.interactable = true;
                    actionBtn.onClick.AddListener(() =>
                    {
                        GameDataManager.Instance.SelectCharacter(def.id);
                        RefreshUI();
                    });
                }
                else
                {
                    string costStr = $"{def.unlockGoldCost}G";
                    if (def.unlockKarmaCost > 0) costStr += $" + {def.unlockKarmaCost}P";
                    btnText.text = $"🔓 채용 ({costStr})";

                    bool canAfford = profile.gold >= def.unlockGoldCost && profile.karmaPoints >= def.unlockKarmaCost;
                    actionBtn.interactable = canAfford;

                    actionBtn.onClick.AddListener(() =>
                    {
                        if (GameDataManager.Instance.UnlockCharacter(def.id))
                        {
                            RefreshUI();
                        }
                    });
                }
            }
        }
    }
}
