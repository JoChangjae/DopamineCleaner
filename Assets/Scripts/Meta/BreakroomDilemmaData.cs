using System;
using System.Collections.Generic;
using UnityEngine;

namespace DopamineCleaner.Meta
{
    public enum BreakroomOptionType
    {
        TripleEspresso,       // 탕비실 믹스커피 3봉: 60초간 공격 및 이동 속도 +40%, 이후 10초간 카페인 방전(-20%)
        RooftopPowerNap,      // 옥상 벤치 꿀잠: 체력 100% 완전 회복 + 15초간 무적 방어막, 30초간 몬스터 스폰량 +30%
        CorporateCardSwipe,   // 비품실 법인카드 긁기: 즉시 +350G 및 전 무기 쿨타임 -20%, 최대 체력 -20 영구 삭감
        AnonymousBlindLeak    // 사내 익명 블라인드 폭로: 화면 내 모든 일반 몬스터 소멸 및 젬 흡수, 즉시 엘리트 2마리 소환
    }

    public class BreakroomDilemmaOption
    {
        public BreakroomOptionType type;
        public string title;
        public string iconEmoji;
        public string benefitDesc;
        public string penaltyDesc;
        public string buttonLabel;

        public BreakroomDilemmaOption(BreakroomOptionType type, string title, string iconEmoji,
                                      string benefitDesc, string penaltyDesc, string buttonLabel)
        {
            this.type = type;
            this.title = title;
            this.iconEmoji = iconEmoji;
            this.benefitDesc = benefitDesc;
            this.penaltyDesc = penaltyDesc;
            this.buttonLabel = buttonLabel;
        }
    }

    public static class BreakroomDilemmaDatabase
    {
        private static readonly List<BreakroomDilemmaOption> allOptions = new()
        {
            new BreakroomDilemmaOption(
                BreakroomOptionType.TripleEspresso,
                "☕ 탕비실 믹스커피 3봉 몰래 타먹기",
                "☕",
                "즉시 공격 속도 및 이동 속도 +40% (60초간 지속)",
                "60초 지속 후 10초간 심한 카페인 탈진 발생 (이속 -20%)",
                "믹스커피 마시기"),

            new BreakroomDilemmaOption(
                BreakroomOptionType.RooftopPowerNap,
                "🛌 옥상 휴게실 벤치 꿀잠 자기",
                "🛌",
                "체력 100% 즉시 완전 회복 + 15초간 절대 무적 쉴드 획득",
                "잠든 사이 보고서 지연: 다음 30초간 몬스터 스폰량 +30% 증원",
                "옥상에서 낮잠자기"),

            new BreakroomDilemmaOption(
                BreakroomOptionType.CorporateCardSwipe,
                "💳 비품실 법인카드로 야식 결제",
                "💳",
                "즉시 +350 Gold 획득 + 모든 무기 쿨타임 -20% 영구 단축",
                "양심의 가책 및 감사 압박: 최대 체력 -20 영구 삭감 (최소 20 유지)",
                "법카로 시원하게 결제"),

            new BreakroomDilemmaOption(
                BreakroomOptionType.AnonymousBlindLeak,
                "🗣️ 블라인드 사내 익명 폭로글 작성",
                "🗣️",
                "속이 뻥! 화면 내 모든 일반 몬스터 즉시 소멸 + 대량의 경험치 흡수",
                "인사팀 발칵: 즉시 강력한 변이 엘리트 몬스터 2마리 급습 소환",
                "사내 폭로글 게시")
        };

        public static List<BreakroomDilemmaOption> GetRandomDilemmas(int count = 3)
        {
            var pool = new List<BreakroomDilemmaOption>(allOptions);
            // 셔플
            for (int i = 0; i < pool.Count; i++)
            {
                int rnd = UnityEngine.Random.Range(i, pool.Count);
                (pool[i], pool[rnd]) = (pool[rnd], pool[i]);
            }

            int pickCount = Mathf.Min(count, pool.Count);
            return pool.GetRange(0, pickCount);
        }
    }
}
