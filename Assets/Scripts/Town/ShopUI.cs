using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DopamineCleaner.Meta;

namespace DopamineCleaner.Town
{
    public class ShopUI : MonoBehaviour
    {
        [Header("상품 목록 컨테이너")]
        [SerializeField] private Transform shopItemContainer;
        [SerializeField] private GameObject shopItemPrefab;

        [Header("소지품 판매 목록 컨테이너")]
        [SerializeField] private Transform inventorySellContainer;
        [SerializeField] private GameObject sellItemPrefab;

        public void RefreshUI()
        {
            if (GameDataManager.Instance == null) return;

            PopulateShopItems();
            PopulateSellItems();
        }

        private void PopulateShopItems()
        {
            if (shopItemContainer == null || shopItemPrefab == null) return;

            foreach (Transform child in shopItemContainer)
            {
                Destroy(child.gameObject);
            }

            var allItems = ItemDatabase.GetAll();
            var profile = GameDataManager.Instance.Profile;

            foreach (var item in allItems)
            {
                GameObject card = Instantiate(shopItemPrefab, shopItemContainer);

                Text iconText = card.transform.Find("Icon")?.GetComponent<Text>();
                Text nameText = card.transform.Find("Name")?.GetComponent<Text>();
                Text priceText = card.transform.Find("Price")?.GetComponent<Text>();
                Text descText = card.transform.Find("Desc")?.GetComponent<Text>();
                Button buyBtn = card.transform.Find("BuyButton")?.GetComponent<Button>();

                if (iconText != null) iconText.text = item.iconEmoji;
                if (nameText != null) nameText.text = $"{item.name} ({item.rarity})";
                if (priceText != null) priceText.text = $"💰 {item.buyPrice} G";
                if (descText != null) descText.text = item.description;

                if (buyBtn != null)
                {
                    bool canAfford = profile.gold >= item.buyPrice;
                    bool hasSpace = profile.inventory.Count < 6 || profile.stashStorage.Count < 24;
                    buyBtn.interactable = canAfford && hasSpace;

                    buyBtn.onClick.RemoveAllListeners();
                    buyBtn.onClick.AddListener(() =>
                    {
                        if (GameDataManager.Instance.BuyItem(item.id))
                        {
                            Core.SoundManager.Instance?.Play(Core.SoundEffect.GemCollect, 0.7f);
                            RefreshUI();
                        }
                    });
                }
            }
        }

        private void PopulateSellItems()
        {
            if (inventorySellContainer == null || sellItemPrefab == null) return;

            foreach (Transform child in inventorySellContainer)
            {
                Destroy(child.gameObject);
            }

            var profile = GameDataManager.Instance.Profile;
            for (int i = 0; i < profile.inventory.Count; i++)
            {
                int index = i;
                string itemId = profile.inventory[i];
                ItemDefinition item = ItemDatabase.Get(itemId);
                if (item == null) continue;

                GameObject row = Instantiate(sellItemPrefab, inventorySellContainer);
                Text infoText = row.transform.Find("Info")?.GetComponent<Text>();
                Button sellBtn = row.transform.Find("SellButton")?.GetComponent<Button>();
                Text sellBtnText = sellBtn?.GetComponentInChildren<Text>();

                if (infoText != null)
                    infoText.text = $"{item.iconEmoji} {item.name}";

                if (sellBtnText != null)
                    sellBtnText.text = $"환급 (+{item.sellPrice}G)";

                if (sellBtn != null)
                {
                    sellBtn.onClick.RemoveAllListeners();
                    sellBtn.onClick.AddListener(() =>
                    {
                        if (GameDataManager.Instance.SellItemFromInventory(index))
                        {
                            Core.SoundManager.Instance?.Play(Core.SoundEffect.Dash, 0.5f);
                            RefreshUI();
                        }
                    });
                }
            }
        }
    }
}
