using System;
using System.Collections.Generic;

namespace DopamineCleaner.Meta
{
    public enum PactType
    {
        WeekendOverwork,      // 주말 특근 계약: 적 이동속도 +25% & 스폰 빈도 +20% / 골드 보너스 +25%
        AIPressure,           // AI 실적 압박 계약: 체력 회복량 50% 삭감 / 디톡스 카르마 보너스 +25% (최소 +1P)
        NotificationBombard,  // 스팸 알림 폭탄: 적 탄환 속도 +25% & 사격 빈도 증가 / 획득 경험치 +25%
        ServerBlackout        // 전산망 블랙아웃: 시야 제한(다크 비네트 포그) / 골드 보너스 +35%
    }

    [Serializable]
    public class PactDefinition
    {
        public PactType type;
        public string id;
        public string title;
        public string iconEmoji;
        public string riskDesc;
        public string rewardDesc;
        public float goldBonus;
        public float karmaBonus;
        public float expBonus;

        public PactDefinition(PactType type, string id, string title, string iconEmoji,
                              string riskDesc, string rewardDesc,
                              float goldBonus, float karmaBonus, float expBonus)
        {
            this.type = type;
            this.id = id;
            this.title = title;
            this.iconEmoji = iconEmoji;
            this.riskDesc = riskDesc;
            this.rewardDesc = rewardDesc;
            this.goldBonus = goldBonus;
            this.karmaBonus = karmaBonus;
            this.expBonus = expBonus;
        }
    }

    public static class PactDatabase
    {
        private static readonly Dictionary<PactType, PactDefinition> pacts = new();

        static PactDatabase()
        {
            Register(new PactDefinition(
                PactType.WeekendOverwork, "pact_overwork", "주말 특근 계약", "⏰",
                "적 이동속도 +25% 증가 및 스폰 빈도 20% 단축",
                "정산 골드 획득량 +25% 보너스",
                0.25f, 0f, 0f));

            Register(new PactDefinition(
                PactType.AIPressure, "pact_ai_pressure", "AI 실적 압박 계약", "🤖",
                "구급상자/쉼터 등 모든 체력 회복 효과 -50% 삭감",
                "정산 디톡스 카르마 +25% 보너스 (최소 +1 P)",
                0f, 0.25f, 0f));

            Register(new PactDefinition(
                PactType.NotificationBombard, "pact_notification", "스팸 알림 폭격 계약", "📨",
                "적 원거리 투사체 비행 속도 +25% 가속 및 사격 주기 단축",
                "몬스터 처치 시 획득 경험치 +25% 보너스",
                0f, 0f, 0.25f));

            Register(new PactDefinition(
                PactType.ServerBlackout, "pact_blackout", "전산망 블랙아웃 계약", "🔌",
                "모니터 시야 제한 (다크 비네트 포그 적용)",
                "정산 골드 획득량 +35% 보너스",
                0.35f, 0f, 0f));
        }

        private static void Register(PactDefinition pact)
        {
            pacts[pact.type] = pact;
        }

        public static PactDefinition Get(PactType type)
        {
            return pacts.TryGetValue(type, out var def) ? def : null;
        }

        public static PactDefinition GetById(string id)
        {
            foreach (var p in pacts.Values)
            {
                if (p.id == id) return p;
            }
            return null;
        }

        public static List<PactDefinition> GetAll()
        {
            return new List<PactDefinition>(pacts.Values);
        }
    }
}
