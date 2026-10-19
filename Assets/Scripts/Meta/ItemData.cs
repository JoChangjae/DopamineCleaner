using System;
using System.Collections.Generic;

namespace DopamineCleaner.Meta
{
    [Serializable]
    public class ItemDefinition
    {
        public string id;
        public string name;
        public ItemType type;
        public ItemRarity rarity;
        public int buyPrice;
        public int sellPrice;
        public string iconEmoji;
        public string description;
        public float effectValue;

        public ItemDefinition(string id, string name, ItemType type, ItemRarity rarity, int buyPrice, int sellPrice, string iconEmoji, string description, float effectValue)
        {
            this.id = id;
            this.name = name;
            this.type = type;
            this.rarity = rarity;
            this.buyPrice = buyPrice;
            this.sellPrice = sellPrice;
            this.iconEmoji = iconEmoji;
            this.description = description;
            this.effectValue = effectValue;
        }
    }

    public static class ItemDatabase
    {
        private static readonly Dictionary<string, ItemDefinition> items = new();

        static ItemDatabase()
        {
            Register(new ItemDefinition(
                "item_first_aid", "응급 구급약", ItemType.Consumable, ItemRarity.Common,
                150, 75, "🩹", "현장에서 즉시 체력 40을 회복합니다.", 40f));

            Register(new ItemDefinition(
                "item_revive_policy", "비상 소생 키트", ItemType.Consumable, ItemRarity.Legendary,
                650, 300, "💉", "사망 시 긴급 리셋 프로토콜이 발동되어 체력 50%로 1회 부활합니다.", 0.5f));

            Register(new ItemDefinition(
                "item_speed_booster", "고카페인 각성제", ItemType.Consumable, ItemRarity.Rare,
                220, 100, "⚡", "세션 동안 이동속도를 15% 영구 상승시킵니다.", 1.15f));

            Register(new ItemDefinition(
                "artifact_golden_calc", "황금 계산기", ItemType.Artifact, ItemRarity.Epic,
                500, 250, "🧮", "정화 구역에서 획득하는 골드가 25% 증가합니다.", 1.25f));

            Register(new ItemDefinition(
                "artifact_magnet_badge", "도파민 견인 배지", ItemType.Artifact, ItemRarity.Rare,
                350, 160, "🧲", "바닥의 모든 보석 흡수 반경이 35% 넓어집니다.", 1.35f));

            Register(new ItemDefinition(
                "equip_hazard_suit", "특수 방호복", ItemType.Equipment, ItemRarity.Common,
                280, 130, "🦺", "최대 체력이 30 증가합니다.", 30f));

            Register(new ItemDefinition(
                "equip_coffee_thermos", "철야 보온병", ItemType.Equipment, ItemRarity.Rare,
                380, 180, "☕", "무기 공격 속도가 15% 빨라집니다.", 0.85f));
        }

        private static void Register(ItemDefinition item)
        {
            items[item.id] = item;
        }

        public static ItemDefinition Get(string id)
        {
            return items.TryGetValue(id, out var def) ? def : null;
        }

        public static List<ItemDefinition> GetAll()
        {
            return new List<ItemDefinition>(items.Values);
        }
    }
}
