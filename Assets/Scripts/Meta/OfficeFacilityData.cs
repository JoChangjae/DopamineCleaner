using System;
using System.Collections.Generic;
using UnityEngine;

namespace DopamineCleaner.Meta
{
    public enum FacilityBuffType
    {
        MaxHpPercent,       // 최대 체력 +%
        CooldownReduction,  // 모든 스킬/무기 쿨타임 -%
        CritChance,         // 치명타 확률 +%
        MoveSpeed,          // 이동 속도 +%
        ReviveChance,       // 사망 시 50% HP로 추가 부활 확률
        GoldBonus           // 정화 골드 획득 보너스 +%
    }

    [Serializable]
    public class OfficeFacilityDefinition
    {
        public string id;
        public string name;
        public string icon;
        public string category;
        public int baseCost;
        public float costMultiplier;
        public int maxLevel;
        public FacilityBuffType buffType;
        public float buffValuePerLevel;
        public string description;

        public OfficeFacilityDefinition(
            string id, string name, string icon, string category,
            int baseCost, float costMultiplier, int maxLevel,
            FacilityBuffType buffType, float buffValuePerLevel, string description)
        {
            this.id = id;
            this.name = name;
            this.icon = icon;
            this.category = category;
            this.baseCost = baseCost;
            this.costMultiplier = costMultiplier;
            this.maxLevel = maxLevel;
            this.buffType = buffType;
            this.buffValuePerLevel = buffValuePerLevel;
            this.description = description;
        }

        public int GetCost(int currentLevel)
        {
            if (currentLevel >= maxLevel) return 0;
            return Mathf.RoundToInt(baseCost * Mathf.Pow(costMultiplier, currentLevel));
        }

        public string GetBuffSummary(int level)
        {
            float total = level * buffValuePerLevel;
            return buffType switch
            {
                FacilityBuffType.MaxHpPercent => $"최대 체력 +{total * 100f:F0}%",
                FacilityBuffType.CooldownReduction => $"쿨타임 감소 -{total * 100f:F0}%",
                FacilityBuffType.CritChance => $"치명타 확률 +{total * 100f:F0}%",
                FacilityBuffType.MoveSpeed => $"이동 속도 +{total * 100f:F0}%",
                FacilityBuffType.ReviveChance => $"추가 부활 확률 +{total * 100f:F0}%",
                FacilityBuffType.GoldBonus => $"골드 획득량 +{total * 100f:F0}%",
                _ => ""
            };
        }
    }

    public static class OfficeFacilityDatabase
    {
        private static readonly Dictionary<string, OfficeFacilityDefinition> facilities = new();

        static OfficeFacilityDatabase()
        {
            Register(new OfficeFacilityDefinition(
                "fac_massage_chair", "최고급 무중력 안마의자", "🛋️", "신체 회복실",
                800, 1.45f, 5, FacilityBuffType.MaxHpPercent, 0.10f,
                "격무로 굳어진 목과 허리를 풀어주는 최첨단 안마의자. 요원들의 기본 생명력이 비약적으로 상승합니다."));

            Register(new OfficeFacilityDefinition(
                "fac_espresso_bar", "스페셜티 에스프레소 바", "☕", "카페테리아",
                1000, 1.5f, 5, FacilityBuffType.CooldownReduction, 0.04f,
                "갓 볶은 원두로 내린 고카페인 에스프레소. 요원들의 반응속도와 스킬 재사용 주기가 단축됩니다."));

            Register(new OfficeFacilityDefinition(
                "fac_arcade_machine", "네온 레트로 오락기", "🕹️", "휴게 오락실",
                1200, 1.5f, 5, FacilityBuffType.CritChance, 0.03f,
                "점심시간마다 랭킹 경쟁이 벌어지는 레트로 오락기. 요원들의 동체 시력과 급소 타격률이 상승합니다."));

            Register(new OfficeFacilityDefinition(
                "fac_standing_desk", "전동 스마트 스탠딩 데스크", "🖥️", "스마트 워크스페이스",
                900, 1.4f, 5, FacilityBuffType.MoveSpeed, 0.04f,
                "바른 자세를 유지해주는 전동 높낮이 책상. 가벼운 발걸음으로 필드 몬스터들의 공격을 회피합니다."));

            Register(new OfficeFacilityDefinition(
                "fac_sleep_capsule", "나노 힐링 수면 캡슐", "🛌", "수면 복사실",
                2000, 1.75f, 3, FacilityBuffType.ReviveChance, 0.25f,
                "치명상을 입었을 때 의식을 백업하고 급속 회복시키는 긴급 수면 장치입니다. (사망 시 50% HP 부활)"));

            Register(new OfficeFacilityDefinition(
                "fac_neon_aquarium", "사이버 네온 수족관", "🐠", "멘탈 케어 라운지",
                1100, 1.45f, 5, FacilityBuffType.GoldBonus, 0.10f,
                "은은한 마젠타 네온 빛 속에 헤엄치는 해파리들. 요원들의 정서 안정과 정화 성과급을 배가시킵니다."));
        }

        private static void Register(OfficeFacilityDefinition fac)
        {
            facilities[fac.id] = fac;
        }

        public static OfficeFacilityDefinition Get(string id)
        {
            if (!string.IsNullOrEmpty(id) && facilities.TryGetValue(id, out var def))
                return def;
            return null;
        }

        public static List<OfficeFacilityDefinition> GetAll()
        {
            return new List<OfficeFacilityDefinition>(facilities.Values);
        }
    }

    [Serializable]
    public class OfficeSynergySet
    {
        public string id;
        public string name;
        public string icon;
        public string requiredFacility1;
        public int requiredLevel1;
        public string requiredFacility2;
        public int requiredLevel2;
        public string effectDescription;

        public OfficeSynergySet(string id, string name, string icon, string f1, int l1, string f2, int l2, string desc)
        {
            this.id = id;
            this.name = name;
            this.icon = icon;
            this.requiredFacility1 = f1;
            this.requiredLevel1 = l1;
            this.requiredFacility2 = f2;
            this.requiredLevel2 = l2;
            this.effectDescription = desc;
        }

        public bool IsActive(Func<string, int> getLevelFunc)
        {
            return getLevelFunc(requiredFacility1) >= requiredLevel1 &&
                   getLevelFunc(requiredFacility2) >= requiredLevel2;
        }
    }

    public static class OfficeSynergyDatabase
    {
        private static readonly List<OfficeSynergySet> sets = new();

        static OfficeSynergyDatabase()
        {
            sets.Add(new OfficeSynergySet(
                "syn_overwork", "야근 전사 세트", "☕",
                "fac_massage_chair", 3, "fac_espresso_bar", 3,
                "피버 타임 지속 시간 +3.0초 & 피버 중 주변 5m 적들에게 1초마다 160 DMG 감전 펄스 방출"));

            sets.Add(new OfficeSynergySet(
                "syn_worklife", "MZ 워라밸 세트", "🏃",
                "fac_standing_desk", 3, "fac_arcade_machine", 3,
                "대시 후 2.5초간 이동속도 +30% & 대시 궤적에 화염 폭발 (80 DMG + 넉백)"));

            sets.Add(new OfficeSynergySet(
                "syn_immortal", "불멸의 디톡스 세트", "💖",
                "fac_sleep_capsule", 2, "fac_neon_aquarium", 3,
                "사망 부활 시 화면 전체 적 4초간 완전 기절 & 황금 보석 5개 즉시 폭발 생성"));
        }

        public static List<OfficeSynergySet> GetAll() => sets;

        public static OfficeSynergySet Get(string id) => sets.Find(s => s.id == id);
    }
}
