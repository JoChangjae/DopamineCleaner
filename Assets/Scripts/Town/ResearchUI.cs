using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DopamineCleaner.Meta;

namespace DopamineCleaner.Town
{
    public class ResearchUI : MonoBehaviour
    {
        [SerializeField] private Transform researchContainer;
        [SerializeField] private GameObject researchCardPrefab;
        [SerializeField] private Button refundAllButton;

        private void Awake()
        {
            if (refundAllButton == null)
            {
                refundAllButton = transform.Find("ResetRefundBtn")?.GetComponent<Button>();
            }

            if (refundAllButton != null)
            {
                refundAllButton.onClick.RemoveAllListeners();
                refundAllButton.onClick.AddListener(OnResetRefundClicked);
            }
        }

        private void OnEnable()
        {
            if (refundAllButton == null)
            {
                refundAllButton = transform.Find("ResetRefundBtn")?.GetComponent<Button>();
                if (refundAllButton != null)
                {
                    refundAllButton.onClick.RemoveAllListeners();
                    refundAllButton.onClick.AddListener(OnResetRefundClicked);
                }
            }
            RefreshUI();
        }

        public void OnResetRefundClicked()
        {
            if (GameDataManager.Instance == null) return;
            var (refundGold, refundKarma) = GameDataManager.Instance.ResetAllResearch();
            Core.SoundManager.Instance?.Play(Core.SoundEffect.Fever, 0.4f);
            RefreshUI();
        }

        public void RefreshUI()
        {
            if (GameDataManager.Instance == null || researchContainer == null || researchCardPrefab == null)
                return;

            foreach (Transform child in researchContainer)
            {
                Destroy(child.gameObject);
            }

            var allResearches = ResearchDatabase.GetAll();
            foreach (var res in allResearches)
            {
                int curLevel = GameDataManager.Instance.GetResearchLevel(res.id);
                bool isMax = curLevel >= res.maxLevel;

                GameObject obj = Instantiate(researchCardPrefab, researchContainer);
                Text icon = obj.transform.Find("Icon")?.GetComponent<Text>();
                Text title = obj.transform.Find("Title")?.GetComponent<Text>();
                Text desc = obj.transform.Find("Desc")?.GetComponent<Text>();
                Text levelText = obj.transform.Find("LevelText")?.GetComponent<Text>();
                Button upgradeBtn = obj.transform.Find("UpgradeButton")?.GetComponent<Button>();
                Text btnText = upgradeBtn?.GetComponentInChildren<Text>();

                if (icon != null) icon.text = res.iconEmoji;
                if (title != null) title.text = res.title;
                if (desc != null) desc.text = res.description;

                if (levelText != null)
                {
                    levelText.text = isMax ? $"Lv.MAX ({curLevel}/{res.maxLevel})" : $"Lv.{curLevel} / {res.maxLevel}";
                    levelText.color = isMax ? new Color(1f, 0.85f, 0.3f) : new Color(0.6f, 0.9f, 1f);
                }

                if (upgradeBtn != null && btnText != null)
                {
                    if (isMax)
                    {
                        btnText.text = "연구 완료 (MAX)";
                        upgradeBtn.interactable = false;
                    }
                    else
                    {
                        int goldCost = res.GetGoldCost(curLevel);
                        int karmaCost = res.GetKarmaCost(curLevel);
                        string costStr = $"{goldCost:N0}G";
                        if (karmaCost > 0) costStr += $" + {karmaCost}P";

                        btnText.text = $"연구 강화 ({costStr})";

                        bool canAfford = GameDataManager.Instance.Profile.gold >= goldCost &&
                                         GameDataManager.Instance.Profile.karmaPoints >= karmaCost;
                        upgradeBtn.interactable = canAfford;

                        upgradeBtn.onClick.RemoveAllListeners();
                        upgradeBtn.onClick.AddListener(() =>
                        {
                            if (GameDataManager.Instance.UpgradeResearch(res.id))
                            {
                                Core.SoundManager.Instance?.Play(Core.SoundEffect.LevelUp, 0.4f);
                                RefreshUI();
                            }
                        });
                    }
                }
            }
        }
    }
}
