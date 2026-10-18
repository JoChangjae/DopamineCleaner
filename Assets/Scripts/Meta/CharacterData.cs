using System;
using System.Collections.Generic;

namespace DopamineCleaner.Meta
{
    [Serializable]
    public class CharacterDefinition
    {
        public string id;
        public string name;
        public string jobTitle;
        public string iconEmoji;
        public float baseMaxHP;
        public float baseMoveSpeed;
        public float baseDamageMult;
        public float baseMagnetRadius;
        public string passiveDesc;
        public int unlockGoldCost;
        public int unlockKarmaCost;
        public bool isDefaultUnlocked;

        public CharacterDefinition(
            string id, string name, string jobTitle, string iconEmoji,
            float maxHP, float moveSpeed, float damageMult, float magnetRadius,
            string passiveDesc, int goldCost, int karmaCost, bool defaultUnlocked)
        {
            this.id = id;
            this.name = name;
            this.jobTitle = jobTitle;
            this.iconEmoji = iconEmoji;
            this.baseMaxHP = maxHP;
            this.baseMoveSpeed = moveSpeed;
            this.baseDamageMult = damageMult;
            this.baseMagnetRadius = magnetRadius;
            this.passiveDesc = passiveDesc;
            this.unlockGoldCost = goldCost;
            this.unlockKarmaCost = karmaCost;
            this.isDefaultUnlocked = defaultUnlocked;
        }
    }

    public static class CharacterDatabase
    {
        private static readonly Dictionary<string, CharacterDefinition> characters = new();

        static CharacterDatabase()
        {
            Register(new CharacterDefinition(
                "char_intern", "김인턴", "신입 수습사원", "🧑‍💼",
                100f, 5.0f, 1.0f, 2.5f,
                "열정 넘치는 신입: 모든 능력치가 균형 잡혀 있습니다.",
                0, 0, true));

            Register(new CharacterDefinition(
                "char_adjuster", "박사정", "손해사정 전문가", "🕵️‍♂️",
                90f, 5.8f, 1.15f, 3.2f,
                "칼같은 손익 계산: 기동성이 빠르고, 골드 획득량이 +20% 증가합니다.",
                800, 0, false));

            Register(new CharacterDefinition(
                "char_cleaner", "강반장", "특수현장 청소부", "🧹",
                145f, 4.4f, 1.25f, 2.8f,
                "산전수전 베테랑: 체력이 매우 높고, 피격 무적 시간이 0.5초 증가합니다.",
                1500, 2, false));

            Register(new CharacterDefinition(
                "char_director", "윤이사", "윤회 집행이사", "👑",
                120f, 5.5f, 1.4f, 4.0f,
                "윤회의 권능: 시작부터 위성 궤도 1개를 기본 장착하고 출동합니다.",
                3000, 5, false));
        }

        private static void Register(CharacterDefinition c)
        {
            characters[c.id] = c;
        }

        public static CharacterDefinition Get(string id)
        {
            return characters.TryGetValue(id, out var def) ? def : characters["char_intern"];
        }

        public static List<CharacterDefinition> GetAll()
        {
            return new List<CharacterDefinition>(characters.Values);
        }
    }
}
