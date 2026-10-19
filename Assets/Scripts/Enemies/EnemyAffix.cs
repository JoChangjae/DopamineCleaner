namespace DopamineCleaner.Enemies
{
    public enum EliteAffix
    {
        None,
        GemThief,       // 🧲 [도파민 도둑]: 주변 바닥의 경험치 보석을 빨아들여 흡수 (처치 시 2배 환급 및 보너스 골드)
        SpamReflector,  // 💬 [알림 폭풍]: 피격 시 일정 확률로 주변 6방향 스팸 탄환 반사 방출
        FirewallShield, // 🛡️ [디지털 방화벽]: 3회의 피해를 완전 무효화하는 보안 쉴드
        HasteSprinter   // ⚡ [알고리즘 과속]: 이동속도 1.55배 및 초고속 돌진
    }

    public static class EliteAffixExtensions
    {
        public static string GetDisplayName(this EliteAffix affix)
        {
            return affix switch
            {
                EliteAffix.GemThief => "🧲 [보석 강탈자]",
                EliteAffix.SpamReflector => "💬 [알림 폭풍]",
                EliteAffix.FirewallShield => "🛡️ [방화벽 보호]",
                EliteAffix.HasteSprinter => "⚡ [알고리즘 과속]",
                _ => ""
            };
        }
    }
}
