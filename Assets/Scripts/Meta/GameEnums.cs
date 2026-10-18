namespace DopamineCleaner.Meta
{
    public enum DungeonDifficulty
    {
        Easy = 0,       // 일반 사고 (배율 0.8)
        Normal = 1,     // 특수 재해 (배율 1.0)
        Hard = 2,       // 영혼 폭주 (배율 1.5)
        Nightmare = 3   // 윤회 붕괴 (배율 2.2)
    }

    public enum ItemType
    {
        Consumable,     // 소모품 (구급약, 부활 증서, 각성제 등)
        Artifact,       // 유물 (황금 계산기, 행운의 사원증 등)
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
