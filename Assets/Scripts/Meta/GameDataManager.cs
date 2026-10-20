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

        // 최근 5회 작전 기록실
        public List<CombatRecord> combatRecords = new();

        // 사원 승진 및 직급 이력 데이터
        public List<CharacterCareerData> characterCareers = new();

        // 활성화된 블랙기업 근로 조약 목록
        public List<string> activePactIds = new();

        // 전술 동반자 드론 & 펫
        public string equippedCompanionId = "pet_drone";
        public List<string> unlockedCompanionIds = new() { "pet_dog", "pet_drone" };
        public List<string> companionTiers = new(); // e.g. "pet_dog:1"
    }

    [Serializable]
    public class CombatRecord
    {
        public string characterId = "char_intern";
        public int survivalSeconds = 0;
        public int kills = 0;
        public int goldEarned = 0;
        public int karmaEarned = 0;
        public bool isCleared = false;
        public string timestamp = "";
    }

    public class GameDataManager : MonoBehaviour
    {
        private static GameDataManager instance;
        public static GameDataManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindAnyObjectByType<GameDataManager>();
                    if (instance == null)
                    {
                        GameObject go = new GameObject("[GameDataManager]");
                        instance = go.AddComponent<GameDataManager>();
                        if (Application.isPlaying) DontDestroyOnLoad(go);
                    }
                }
                return instance;
            }
            private set => instance = value;
        }

        public PlayerProfileData Profile { get; private set; } = new();

        // 현재 던전 세션 설정 (마을에서 던전으로 전달)
        public string SelectedMapId { get; set; } = "map_hospital";
        public string SelectedCharacterId => Profile?.selectedCharacterId ?? "char_intern";
        public DungeonDifficulty SelectedDifficulty { get; set; } = DungeonDifficulty.Normal;
        public bool IsHyperMode { get; set; } = false;

        public event Action OnProfileUpdated;

        private string SaveFilePath => Path.Combine(Application.persistentDataPath, "player_profile.json");

        private void Awake()
        {
            if (instance == null || instance == this)
            {
                instance = this;
                if (Application.isPlaying) DontDestroyOnLoad(gameObject);
                LoadProfile();
            }
            else
            {
                if (Application.isPlaying)
                    Destroy(gameObject);
                else
                    DestroyImmediate(gameObject);
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

            if (Profile.combatRecords == null)
                Profile.combatRecords = new();

            if (Profile.characterCareers == null)
                Profile.characterCareers = new();

            if (Profile.activePactIds == null)
                Profile.activePactIds = new();

            OnProfileUpdated?.Invoke();
        }

        // ==========================================
        // 캐릭터 관리 및 승진 (Career Advancement)
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

        public CharacterCareerData GetCareerData(string charId)
        {
            if (Profile.characterCareers == null) Profile.characterCareers = new();

            var career = Profile.characterCareers.Find(c => c.characterId == charId);
            if (career == null)
            {
                career = new CharacterCareerData(charId, 0, 0);
                Profile.characterCareers.Add(career);
            }
            return career;
        }

        public void RecordCharacterClear(string charId)
        {
            var career = GetCareerData(charId);
            career.clearCount++;
            SaveProfile();
        }

        public bool CanPromoteCharacter(string charId)
        {
            var career = GetCareerData(charId);
            if (career.rankLevel >= 2) return false; // 최고 직급

            var nextDef = CareerDatabase.GetNext((CareerRank)career.rankLevel);
            if (nextDef == null) return false;

            return career.clearCount >= nextDef.requiredClears;
        }

        public bool PromoteCharacter(string charId)
        {
            if (!CanPromoteCharacter(charId)) return false;

            var career = GetCareerData(charId);
            career.rankLevel++;
            SaveProfile();
            OnProfileUpdated?.Invoke();
            return true;
        }

                public int GetCompanionTier(string companionId)
        {
            if (Profile?.companionTiers == null) return 1;
            foreach (var item in Profile.companionTiers)
            {
                var parts = item.Split(':');
                if (parts.Length == 2 && parts[0] == companionId && int.TryParse(parts[1], out int tier))
                    return tier;
            }
            return 1;
        }

        public bool UpgradeCompanionTier(string companionId, int cost)
        {
            if (Profile == null || Profile.gold < cost) return false;
            int currentTier = GetCompanionTier(companionId);
            if (currentTier >= 3) return false;

            Profile.gold -= cost;
            if (Profile.companionTiers == null) Profile.companionTiers = new List<string>();
            Profile.companionTiers.RemoveAll(item => item.StartsWith(companionId + ":"));
            Profile.companionTiers.Add(companionId + ":" + (currentTier + 1));
            SaveProfile();
            OnProfileUpdated?.Invoke();
            return true;
        }

        public void SelectCompanion(string companionId)
        {
            if (Profile == null) return;
            Profile.equippedCompanionId = companionId;
            SaveProfile();
            OnProfileUpdated?.Invoke();
        }

        public bool UnlockCompanion(string companionId, int cost)
        {
            if (Profile == null) return false;
            if (Profile.unlockedCompanionIds == null)
                Profile.unlockedCompanionIds = new List<string>();

            if (Profile.unlockedCompanionIds.Contains(companionId))
                return true;

            if (Profile.gold >= cost)
            {
                Profile.gold -= cost;
                Profile.unlockedCompanionIds.Add(companionId);
                Profile.equippedCompanionId = companionId;
                SaveProfile();
                OnProfileUpdated?.Invoke();
                return true;
            }
            return false;
        }

        // ==========================================
        // 블랙기업 근로 조약 (Pacts of Overwork)
        // ==========================================
        public bool IsPactActive(PactType type)
        {
            var def = PactDatabase.Get(type);
            return def != null && Profile.activePactIds != null && Profile.activePactIds.Contains(def.id);
        }

        public void SetPactActive(PactType type, bool active)
        {
            var def = PactDatabase.Get(type);
            if (def == null) return;
            if (Profile.activePactIds == null) Profile.activePactIds = new();

            if (active && !Profile.activePactIds.Contains(def.id))
            {
                Profile.activePactIds.Add(def.id);
            }
            else if (!active && Profile.activePactIds.Contains(def.id))
            {
                Profile.activePactIds.Remove(def.id);
            }
            SaveProfile();
            OnProfileUpdated?.Invoke();
        }

        public bool TogglePact(PactType type)
        {
            bool newState = !IsPactActive(type);
            SetPactActive(type, newState);
            return newState;
        }

        public void ClearActivePacts()
        {
            if (Profile.activePactIds != null)
            {
                Profile.activePactIds.Clear();
                SaveProfile();
                OnProfileUpdated?.Invoke();
            }
        }

        public float GetActivePactGoldMultiplier()
        {
            float bonus = 0f;
            foreach (PactType pt in Enum.GetValues(typeof(PactType)))
            {
                if (IsPactActive(pt))
                {
                    var def = PactDatabase.Get(pt);
                    if (def != null) bonus += def.goldBonus;
                }
            }
            return bonus;
        }

        public float GetActivePactKarmaMultiplier()
        {
            float bonus = 0f;
            foreach (PactType pt in Enum.GetValues(typeof(PactType)))
            {
                if (IsPactActive(pt))
                {
                    var def = PactDatabase.Get(pt);
                    if (def != null) bonus += def.karmaBonus;
                }
            }
            return bonus;
        }

        public float GetActivePactExpMultiplier()
        {
            float bonus = 0f;
            foreach (PactType pt in Enum.GetValues(typeof(PactType)))
            {
                if (IsPactActive(pt))
                {
                    var def = PactDatabase.Get(pt);
                    if (def != null) bonus += def.expBonus;
                }
            }
            return bonus;
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

        // ==========================================
        // 연구 100% 전액 환급 초기화
        // ==========================================
        public (int refundedGold, int refundedKarma) ResetAllResearch()
        {
            int totalRefundGold = 0;
            int totalRefundKarma = 0;

            string[] researchIds = { "atk", "hp", "spd", "mag", "rev" };
            foreach (var id in researchIds)
            {
                var def = ResearchDatabase.Get(id);
                if (def == null) continue;
                int currentLvl = GetResearchLevel(id);
                for (int lvl = 0; lvl < currentLvl; lvl++)
                {
                    totalRefundGold += def.GetGoldCost(lvl);
                    totalRefundKarma += def.GetKarmaCost(lvl);
                }
            }

            Profile.researchAtkLevel = 0;
            Profile.researchHpLevel = 0;
            Profile.researchSpdLevel = 0;
            Profile.researchMagLevel = 0;
            Profile.researchRevLevel = 0;

            Profile.gold += totalRefundGold;
            Profile.karmaPoints += totalRefundKarma;

            SaveProfile();
            OnProfileUpdated?.Invoke();

            return (totalRefundGold, totalRefundKarma);
        }

        // ==========================================
        // 최근 5회 작전 기록 관리
        // ==========================================
        public void AddCombatRecord(CombatRecord record)
        {
            if (record == null) return;
            if (Profile.combatRecords == null) Profile.combatRecords = new();

            Profile.combatRecords.Insert(0, record);
            while (Profile.combatRecords.Count > 5)
            {
                Profile.combatRecords.RemoveAt(Profile.combatRecords.Count - 1);
            }

            SaveProfile();
        }
    }
}
