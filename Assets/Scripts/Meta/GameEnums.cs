namespace DopamineCleaner.Meta
{
    public enum DungeonDifficulty
    {
        Easy = 0,       // 경미한 중독 (배율 0.8)
        Normal = 1,     // 주의 경보 (배율 1.0)
        Hard = 2,       // 도파민 과부하 (배율 1.5)
        Nightmare = 3   // 도파민 폭주 (배율 2.2)
    }

    public enum ItemType
    {
        Consumable,     // 소모품 (구급약, 비상 소생 키트, 각성제 등)
        Artifact,       // 유물 (황금 계산기, 행운의 요원증 등)
        Equipment       // 장비 (특수 방호복, 러닝화 등)
    }

    public enum ItemRarity
    {
        Common,
        Rare,
        Epic,
        Legendary
    }
}
