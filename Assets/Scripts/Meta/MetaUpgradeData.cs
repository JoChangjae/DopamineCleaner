using System;
using System.Collections.Generic;

namespace DopamineCleaner.Meta
{
    [Serializable]
    public class ResearchDefinition
    {
        public string id;
        public string title;
        public string description;
        public string iconEmoji;
        public int maxLevel;
        public int baseGoldCost;
        public int baseKarmaCost;
        public float valuePerLevel;

        public ResearchDefinition(string id, string title, string description, string iconEmoji, int maxLevel, int baseGold, int baseKarma, float value)
        {
            this.id = id;
            this.title = title;
            this.description = description;
            this.iconEmoji = iconEmoji;
            this.maxLevel = maxLevel;
            this.baseGoldCost = baseGold;
            this.baseKarmaCost = baseKarma;
            this.valuePerLevel = value;
        }

        public int GetGoldCost(int currentLevel) => baseGoldCost * (currentLevel + 1);
        public int GetKarmaCost(int currentLevel) => baseKarmaCost * (currentLevel + 1);
    }

    public static class ResearchDatabase
    {
        private static readonly Dictionary<string, ResearchDefinition> researches = new();

        static ResearchDatabase()
        {
            Register(new ResearchDefinition("atk", "뇌파 집중 (공격력)", "모든 무기의 기본 공격력이 상승합니다. (레벨당 +10%)", "💥", 5, 250, 1, 0.10f));
            Register(new ResearchDefinition("hp", "멘탈 강화 (최대 체력)", "요원의 기본 최대 체력이 상승합니다. (레벨당 +25 HP)", "❤️", 5, 200, 1, 25f));
            Register(new ResearchDefinition("spd", "경쾌한 발걸음 (이속)", "요원의 기본 이동속도가 상승합니다. (레벨당 +6%)", "👟", 5, 200, 1, 0.06f));
            Register(new ResearchDefinition("mag", "도파민 흡인력 (자석)", "경험치 및 아이템 흡수 반경이 넓어집니다. (레벨당 +18%)", "🧲", 5, 180, 0, 0.18f));
            Register(new ResearchDefinition("rev", "비상 리셋 칩 (부활)", "던전 진입 시 추가 부활 횟수를 기본 제공합니다.", "✨", 2, 800, 3, 1f));
        }

        private static void Register(ResearchDefinition def)
        {
            researches[def.id] = def;
        }

        public static ResearchDefinition Get(string id)
        {
            return researches.TryGetValue(id, out var def) ? def : null;
        }

        public static List<ResearchDefinition> GetAll()
        {
            return new List<ResearchDefinition>(researches.Values);
        }
    }
}
