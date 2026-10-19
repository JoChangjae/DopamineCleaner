using UnityEngine;
using UnityEngine.UI;
using DopamineCleaner.Meta;

namespace DopamineCleaner.Town
{
    public class StorageUI : MonoBehaviour
    {
        [Header("가방 (인벤토리)")]
        [SerializeField] private Text inventoryCapacityText;
        [SerializeField] private Transform inventoryContainer;
        [SerializeField] private GameObject storageItemPrefab;

        [Header("창고 (사물함)")]
        [SerializeField] private Text stashCapacityText;
        [SerializeField] private Transform stashContainer;

        public void RefreshUI()
        {
            if (GameDataManager.Instance == null) return;

            var profile = GameDataManager.Instance.Profile;

            if (inventoryCapacityText != null)
                inventoryCapacityText.text = $"🎒 출동 가방 ({profile.inventory.Count} / 6)";

            if (stashCapacityText != null)
                stashCapacityText.text = $"🗄️ 본부 창고 ({profile.stashStorage.Count} / 24)";

            // 1. 가방 아이템 목록 생성
            if (inventoryContainer != null && storageItemPrefab != null)
            {
                foreach (Transform child in inventoryContainer) Destroy(child.gameObject);

                for (int i = 0; i < profile.inventory.Count; i++)
                {
                    int index = i;
                    string itemId = profile.inventory[i];
                    ItemDefinition item = ItemDatabase.Get(itemId);
                    if (item == null) continue;

                    GameObject obj = Instantiate(storageItemPrefab, inventoryContainer);
                    Text title = obj.transform.Find("Title")?.GetComponent<Text>();
                    Text desc = obj.transform.Find("Desc")?.GetComponent<Text>();
                    Button actionBtn = obj.transform.Find("ActionButton")?.GetComponent<Button>();
                    Text btnText = actionBtn?.GetComponentInChildren<Text>();

                    if (title != null) title.text = $"{item.iconEmoji} {item.name}";
                    if (desc != null) desc.text = item.description;
                    if (btnText != null) btnText.text = "창고로 보관 ➔";

                    if (actionBtn != null)
                    {
                        actionBtn.interactable = profile.stashStorage.Count < 24;
                        actionBtn.onClick.RemoveAllListeners();
                        actionBtn.onClick.AddListener(() =>
                        {
                            if (GameDataManager.Instance.MoveInventoryToStash(index))
                            {
                                Core.SoundManager.Instance?.Play(Core.SoundEffect.Dash, 0.45f);
                                RefreshUI();
                            }
                        });
                    }
                }
            }

            // 2. 창고 아이템 목록 생성
            if (stashContainer != null && storageItemPrefab != null)
            {
                foreach (Transform child in stashContainer) Destroy(child.gameObject);

                for (int i = 0; i < profile.stashStorage.Count; i++)
                {
                    int index = i;
                    string itemId = profile.stashStorage[i];
                    ItemDefinition item = ItemDatabase.Get(itemId);
                    if (item == null) continue;

                    GameObject obj = Instantiate(storageItemPrefab, stashContainer);
                    Text title = obj.transform.Find("Title")?.GetComponent<Text>();
                    Text desc = obj.transform.Find("Desc")?.GetComponent<Text>();
                    Button actionBtn = obj.transform.Find("ActionButton")?.GetComponent<Button>();
                    Text btnText = actionBtn?.GetComponentInChildren<Text>();

                    if (title != null) title.text = $"{item.iconEmoji} {item.name}";
                    if (desc != null) desc.text = item.description;
                    if (btnText != null) btnText.text = "➔ 가방으로 꺼내기";

                    if (actionBtn != null)
                    {
                        actionBtn.interactable = profile.inventory.Count < 6;
                        actionBtn.onClick.RemoveAllListeners();
                        actionBtn.onClick.AddListener(() =>
                        {
                            if (GameDataManager.Instance.MoveStashToInventory(index))
                            {
                                Core.SoundManager.Instance?.Play(Core.SoundEffect.Dash, 0.45f);
                                RefreshUI();
                            }
                        });
                    }
                }
            }
        }
    }
}
