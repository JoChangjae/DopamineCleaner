using System;
using System.Collections.Generic;

namespace DopamineCleaner.Meta
{
    public enum CareerRank
    {
        Intern = 0,            // 수습 사원 (기본)
        AssistantManager = 1,  // 주임 / 대리 (2회 클리어 달성 시 승진)
        Manager = 2            // 과장 / 팀장 (5회 클리어 달성 시 승진)
    }

    [Serializable]
    public class CareerRankDefinition
    {
        public CareerRank rank;
        public string rankTitle;
        public string badgeEmoji;
        public int requiredClears;
        public string perksDesc;
        public float hpBonusPercent;
        public float spdBonusPercent;
        public float dmgBonusPercent;
        public float cooldownReductionPercent;

        public CareerRankDefinition(CareerRank rank, string rankTitle, string badgeEmoji,
                                    int requiredClears, string perksDesc,
                                    float hpBonus, float spdBonus, float dmgBonus, float cdr)
        {
            this.rank = rank;
            this.rankTitle = rankTitle;
            this.badgeEmoji = badgeEmoji;
            this.requiredClears = requiredClears;
            this.perksDesc = perksDesc;
            this.hpBonusPercent = hpBonus;
            this.spdBonusPercent = spdBonus;
            this.dmgBonusPercent = dmgBonus;
            this.cooldownReductionPercent = cdr;
        }
    }

    [Serializable]
    public class CharacterCareerData
    {
        public string characterId = "char_intern";
        public int clearCount = 0;
        public int rankLevel = 0; // 0: Intern, 1: AssistantManager, 2: Manager

        public CharacterCareerData() { }

        public CharacterCareerData(string charId, int clears = 0, int rank = 0)
        {
            characterId = charId;
            clearCount = clears;
            rankLevel = rank;
        }
    }

    public static class CareerDatabase
    {
        private static readonly Dictionary<CareerRank, CareerRankDefinition> ranks = new();

        static CareerDatabase()
        {
            ranks[CareerRank.Intern] = new CareerRankDefinition(
                CareerRank.Intern, "수습 사원", "🌱",
                0, "표준 기본 스탯",
                0f, 0f, 0f, 0f);

            ranks[CareerRank.AssistantManager] = new CareerRankDefinition(
                CareerRank.AssistantManager, "선임 대리", "🎖️",
                2, "정규직 전환: 최대 체력 +15%, 이동 속도 +10%",
                0.15f, 0.10f, 0f, 0f);

            ranks[CareerRank.Manager] = new CareerRankDefinition(
                CareerRank.Manager, "총괄 팀장", "👑",
                5, "임원진 승진: 최대 체력 +15%, 이동 속도 +10%, 전 무기 공격력 +20%, 모든 쿨타임 -15%",
                0.15f, 0.10f, 0.20f, 0.15f);
        }

        public static CareerRankDefinition Get(CareerRank rank)
        {
            return ranks.TryGetValue(rank, out var def) ? def : ranks[CareerRank.Intern];
        }

        public static CareerRankDefinition Get(int rankLevel)
        {
            CareerRank rank = (CareerRank)Math.Clamp(rankLevel, 0, 2);
            return Get(rank);
        }

        public static CareerRankDefinition GetNext(CareerRank current)
        {
            if (current == CareerRank.Intern) return ranks[CareerRank.AssistantManager];
            if (current == CareerRank.AssistantManager) return ranks[CareerRank.Manager];
            return null; // 최고 직급 만렙
        }
    }
}
