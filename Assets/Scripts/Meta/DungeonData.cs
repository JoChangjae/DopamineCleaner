using System;
using System.Collections.Generic;
using UnityEngine;

namespace DopamineCleaner.Meta
{
    [Serializable]
    public class DungeonMapDefinition
    {
        public string id;
        public string name;
        public string subTitle;
        public string iconEmoji;
        public Color themeColor;
        public float targetSurvivalSec;
        public int baseGoldReward;
        public int baseKarmaReward;
        public string description;

        public DungeonMapDefinition(string id, string name, string subTitle, string iconEmoji, Color themeColor, float targetSec, int gold, int karma, string desc)
        {
            this.id = id;
            this.name = name;
            this.subTitle = subTitle;
            this.iconEmoji = iconEmoji;
            this.themeColor = themeColor;
            this.targetSurvivalSec = targetSec;
            this.baseGoldReward = gold;
            this.baseKarmaReward = karma;
            this.description = desc;
        }
    }

    [Serializable]
    public class DifficultyMultiplier
    {
        public float enemyHpMult;
        public float enemyDmgMult;
        public float enemySpeedMult;
        public float spawnIntervalMult;
        public float rewardGoldMult;
        public float rewardKarmaMult;
    }

    public static class DungeonDatabase
    {
        private static readonly Dictionary<string, DungeonMapDefinition> maps = new();

        static DungeonDatabase()
        {
            Register(new DungeonMapDefinition(
                "map_hospital", "도파민 격리 병동", "중독 환자 집중 정화 구역", "🏥",
                new Color(0.85f, 0.94f, 0.92f), 300f, 600, 1,
                "좁고 음침한 병동 복도. 힐링 아이템 드랍 확률이 소폭 높습니다. (목표: 5분 생존)"));

            Register(new DungeonMapDefinition(
                "map_office", "번아웃 오피스 미궁", "야근 직장인 잡생각 대폭주", "🏢",
                new Color(0.92f, 0.88f, 0.95f), 420f, 1000, 2,
                "야근과 커피로 찌든 서류더미 미궁. 골드 드랍량이 매우 많습니다. (목표: 7분 생존)"));

            Register(new DungeonMapDefinition(
                "map_highway", "심야 숏폼 고속도로", "무한 스크롤 도파민 폭주 도로", "🛣️",
                new Color(0.95f, 0.85f, 0.85f), 600f, 1800, 4,
                "끝없이 맹렬히 돌진하는 최고 난이도의 고속도로. (목표: 10분 생존)"));
        }

        private static void Register(DungeonMapDefinition m)
        {
            maps[m.id] = m;
        }

        public static DungeonMapDefinition Get(string id)
        {
            return maps.TryGetValue(id, out var def) ? def : maps["map_hospital"];
        }

        public static List<DungeonMapDefinition> GetAll()
        {
            return new List<DungeonMapDefinition>(maps.Values);
        }

        public static DifficultyMultiplier GetDifficultyMultiplier(DungeonDifficulty diff)
        {
            return diff switch
            {
                DungeonDifficulty.Easy => new DifficultyMultiplier
                {
                    enemyHpMult = 0.8f, enemyDmgMult = 0.75f, enemySpeedMult = 0.9f,
                    spawnIntervalMult = 1.25f, rewardGoldMult = 0.8f, rewardKarmaMult = 1.0f
                },
                DungeonDifficulty.Normal => new DifficultyMultiplier
                {
                    enemyHpMult = 1.0f, enemyDmgMult = 1.0f, enemySpeedMult = 1.0f,
                    spawnIntervalMult = 1.0f, rewardGoldMult = 1.0f, rewardKarmaMult = 1.0f
                },
                DungeonDifficulty.Hard => new DifficultyMultiplier
                {
                    enemyHpMult = 1.45f, enemyDmgMult = 1.35f, enemySpeedMult = 1.12f,
                    spawnIntervalMult = 0.75f, rewardGoldMult = 1.6f, rewardKarmaMult = 1.5f
                },
                DungeonDifficulty.Nightmare => new DifficultyMultiplier
                {
                    enemyHpMult = 2.1f, enemyDmgMult = 1.8f, enemySpeedMult = 1.25f,
                    spawnIntervalMult = 0.55f, rewardGoldMult = 2.5f, rewardKarmaMult = 2.5f
                },
                _ => new DifficultyMultiplier
                {
                    enemyHpMult = 1f, enemyDmgMult = 1f, enemySpeedMult = 1f,
                    spawnIntervalMult = 1f, rewardGoldMult = 1f, rewardKarmaMult = 1f
                }
            };
        }
    }
}
