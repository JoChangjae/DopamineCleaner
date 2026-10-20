using System;
using System.Collections.Generic;

namespace DopamineCleaner.Characters
{
    public enum CompanionType
    {
        None,
        HappyDog,       // 🐕 [산책견 댕댕이]: 보석 자동 수거 전문
        NapCat,         // 🐱 [단잠 냥이]: 15초마다 힐링 쉼터 장판 생성
        PomodoroDrone,  // 🤖 [뽀모도로 드론]: 25초 집중 사격 + 5초 2배 경험치
        BombHamster     // 🐹 [폭격 햄스터]: 해바라기씨 폭탄 투하 광역 폭격
    }

    [Serializable]
    public class CompanionInfo
    {
        public CompanionType type;
        public string id;
        public string name;
        public string description;
        public string icon;
    }

    public static class CompanionDatabase
    {
        private static readonly List<CompanionInfo> allCompanions = new()
        {
            new CompanionInfo
            {
                type = CompanionType.HappyDog,
                id = "pet_dog",
                name = "산책견 댕댕이",
                description = "주변 8m 내의 경험치 보석과 상자를 플레이어 대신 잽싸게 물어다 줍니다.",
                icon = "🐕"
            },
            new CompanionInfo
            {
                type = CompanionType.NapCat,
                id = "pet_cat",
                name = "단잠 냥이",
                description = "15초마다 발밑에 힐링 쉼터 장판을 깔아 HP를 회복하고 과부하를 정화합니다.",
                icon = "🐱"
            },
            new CompanionInfo
            {
                type = CompanionType.PomodoroDrone,
                id = "pet_drone",
                name = "뽀모도로 드론",
                description = "25초간 주기적 레이저 사격을 가하고, 5초간 경험치 획득량을 2배로 증폭시킵니다.",
                icon = "🤖"
            },
            new CompanionInfo
            {
                type = CompanionType.BombHamster,
                id = "pet_hamster",
                name = "폭격 햄스터",
                description = "주기적으로 해바라기씨 도파민 폭탄을 투하하여 적들을 한 방에 일망타진합니다.",
                icon = "🐹"
            }
        };

        public static List<CompanionInfo> GetAll() => allCompanions;

        public static CompanionInfo Get(CompanionType type)
        {
            return allCompanions.Find(c => c.type == type);
        }
    }
}
