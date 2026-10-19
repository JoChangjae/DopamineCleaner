using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace DopamineCleaner.Meta
{
    [Serializable]
    public class PlayerProfileData
    {
        public int gold = 600;
        public int karmaPoints = 1;
        public string selectedCharacterId = "char_intern";
        public List<string> unlockedCharacterIds = new() { "char_intern" };

        // 가방 (출동 시 들고 가는 아이템 슬롯, 최대 6칸)
        public List<string> inventory = new() { "item_first_aid" };

        // 사물함 창고 (영구 보관 슬롯, 최대 24칸)
        public List<string> stashStorage = new() { "artifact_golden_calc", "item_speed_booster" };

        // 영구 계약 특약 레벨
        public int contractHPLevel = 0;
        public int contractSpeedLevel = 0;
        public int contractDamageLevel = 0;
        public int contractGoldLevel = 0;

        // 본부 연구소 영구 강화 레벨
        public int researchAtkLevel = 0;
        public int researchHpLevel = 0;
        public int researchSpdLevel = 0;
        public int researchMagLevel = 0;
        public int researchRevLevel = 0;

        // 업적 및 누적 플레이 통계
        public List<string> unlockedAchievements = new();
        public int lifetimeKills = 0;
        public int lifetimeFevers = 0;
        public int lifetimeBossKills = 0;
        public int lifetimeGamesPlayed = 0;
        public int lifetimeGoldEarned = 0;
    }

    public class GameDataManager : MonoBehaviour
    {
        public static GameDataManager Instance { get; private set; }

        public PlayerProfileData Profile { get; private set; } = new();

        // 현재 던전 세션 설정 (마을에서 던전으로 전달)
        public string SelectedMapId { get; set; } = "map_hospital";
        public DungeonDifficulty SelectedDifficulty { get; set; } = DungeonDifficulty.Normal;
        public bool IsHyperMode { get; set; } = false;

        public event Action OnProfileUpdated;

        private string SaveFilePath => Path.Combine(Application.persistentDataPath, "player_profile.json");

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                LoadProfile();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void SaveProfile()
        {
            try
            {
                string json = JsonUtility.ToJson(Profile, true);
                File.WriteAllText(SaveFilePath, json);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"세이브 실패: {ex.Message}");
            }

            OnProfileUpdated?.Invoke();
        }

        public void LoadProfile()
        {
            if (File.Exists(SaveFilePath))
            {
                try
                {
                    string json = File.ReadAllText(SaveFilePath);
                    Profile = JsonUtility.FromJson<PlayerProfileData>(json);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"세이브 로드 오류, 기본값 사용: {ex.Message}");
                    Profile = new PlayerProfileData();
                }
            }
            else
            {
                Profile = new PlayerProfileData();
                SaveProfile();
            }

            if (Profile.unlockedCharacterIds == null)
                Profile.unlockedCharacterIds = new() { "char_intern" };

            if (!Profile.unlockedCharacterIds.Contains("char_intern"))
                Profile.unlockedCharacterIds.Add("char_intern");

            if (string.IsNullOrEmpty(Profile.selectedCharacterId))
                Profile.selectedCharacterId = "char_intern";

            if (Profile.inventory == null)
                Profile.inventory = new() { "item_first_aid" };

            if (Profile.stashStorage == null)
                Profile.stashStorage = new() { "artifact_golden_calc", "item_speed_booster" };

            OnProfileUpdated?.Invoke();
        }

        // ==========================================
        // 캐릭터 관리
        // ==========================================
        public bool SelectCharacter(string charId)
        {
            if (Profile.unlockedCharacterIds.Contains(charId))
            {
                Profile.selectedCharacterId = charId;
                SaveProfile();
                return true;
            }
            return false;
        }

        public bool UnlockCharacter(string charId)
        {
            CharacterDefinition def = CharacterDatabase.Get(charId);
            if (def == null || Profile.unlockedCharacterIds.Contains(charId)) return false;

            if (Profile.gold >= def.unlockGoldCost && Profile.karmaPoints >= def.unlockKarmaCost)
            {
                Profile.gold -= def.unlockGoldCost;
                Profile.karmaPoints -= def.unlockKarmaCost;
                Profile.unlockedCharacterIds.Add(charId);
                Profile.selectedCharacterId = charId;
                SaveProfile();
                return true;
            }
            return false;
        }

        public int GetResearchLevel(string researchId)
        {
            return researchId switch
            {
                "atk" => Profile.researchAtkLevel,
                "hp" => Profile.researchHpLevel,
                "spd" => Profile.researchSpdLevel,
                "mag" => Profile.researchMagLevel,
                "rev" => Profile.researchRevLevel,
                _ => 0
            };
        }

        public bool UpgradeResearch(string researchId)
        {
            var def = ResearchDatabase.Get(researchId);
            if (def == null) return false;

            int curLevel = GetResearchLevel(researchId);
            if (curLevel >= def.maxLevel) return false;

            int goldCost = def.GetGoldCost(curLevel);
            int karmaCost = def.GetKarmaCost(curLevel);

            if (Profile.gold < goldCost || Profile.karmaPoints < karmaCost)
                return false;

            Profile.gold -= goldCost;
            Profile.karmaPoints -= karmaCost;

            switch (researchId)
            {
                case "atk": Profile.researchAtkLevel++; break;
                case "hp": Profile.researchHpLevel++; break;
                case "spd": Profile.researchSpdLevel++; break;
                case "mag": Profile.researchMagLevel++; break;
                case "rev": Profile.researchRevLevel++; break;
            }

            SaveProfile();
            OnProfileUpdated?.Invoke();
            return true;
        }

        // ==========================================
        // 창고 & 인벤토리 슬롯 이동
        // ==========================================
        public bool MoveInventoryToStash(int invIndex)
        {
            if (invIndex < 0 || invIndex >= Profile.inventory.Count) return false;
            if (Profile.stashStorage.Count >= 24) return false; // 창고 최대 24칸

            string itemId = Profile.inventory[invIndex];
            Profile.inventory.RemoveAt(invIndex);
            Profile.stashStorage.Add(itemId);
            SaveProfile();
            return true;
        }

        public bool MoveStashToInventory(int stashIndex)
        {
            if (stashIndex < 0 || stashIndex >= Profile.stashStorage.Count) return false;
            if (Profile.inventory.Count >= 6) return false; // 가방 최대 6칸

            string itemId = Profile.stashStorage[stashIndex];
            Profile.stashStorage.RemoveAt(stashIndex);
            Profile.inventory.Add(itemId);
            SaveProfile();
            return true;
        }

        // ==========================================
        // 상점 구매 및 판매
        // ==========================================
        public bool BuyItem(string itemId)
        {
            ItemDefinition item = ItemDatabase.Get(itemId);
            if (item == null) return false;

            if (Profile.gold >= item.buyPrice)
            {
                if (Profile.inventory.Count < 6)
                {
                    Profile.gold -= item.buyPrice;
                    Profile.inventory.Add(itemId);
                    SaveProfile();
                    return true;
                }
                else if (Profile.stashStorage.Count < 24)
                {
                    Profile.gold -= item.buyPrice;
                    Profile.stashStorage.Add(itemId);
                    SaveProfile();
                    return true;
                }
            }
            return false;
        }

        public bool SellItemFromInventory(int invIndex)
        {
            if (invIndex < 0 || invIndex >= Profile.inventory.Count) return false;

            string itemId = Profile.inventory[invIndex];
            ItemDefinition item = ItemDatabase.Get(itemId);
            if (item == null) return false;

            Profile.gold += item.sellPrice;
            Profile.inventory.RemoveAt(invIndex);
            SaveProfile();
            return true;
        }

        public void AddGold(int amount)
        {
            Profile.gold += amount;
            SaveProfile();
        }

        // ==========================================
        // 던전 보상 정산
        // ==========================================
        public void ApplyDungeonSettlement(int earnedGold, int earnedKarma, List<string> lootedItemIds)
        {
            Profile.gold += earnedGold;
            Profile.karmaPoints += earnedKarma;

            if (lootedItemIds != null)
            {
                foreach (string item in lootedItemIds)
                {
                    if (Profile.stashStorage.Count < 24)
                        Profile.stashStorage.Add(item);
                    else if (Profile.inventory.Count < 6)
                        Profile.inventory.Add(item);
                }
            }

            SaveProfile();
        }
    }
}
