using System.Collections.Generic;
using UnityEngine;
using DopamineCleaner.Characters;
using DopamineCleaner.Enemies;
using DopamineCleaner.Meta;

namespace DopamineCleaner.Weapons
{
    public abstract class WeaponBase : MonoBehaviour
    {
        [Header("무기 기본 정보")]
        [SerializeField] protected string weaponName = "Weapon";
        [SerializeField] protected int currentLevel = 0;
        [SerializeField] protected int maxLevel = 5;
        [SerializeField] protected bool isEvolved = false;
        [SerializeField] protected float baseCooldown = 1.0f;
        protected float cooldownTimer = 0f;

        public string WeaponName => weaponName;
        public int Level => currentLevel;
        public int MaxLevel => maxLevel;
        public bool IsEvolved => isEvolved;
        public bool IsActive => currentLevel > 0;
        public float TotalDamageDealt { get; protected set; } = 0f;

        public virtual void RegisterDamage(float damage)
        {
            TotalDamageDealt += damage;
        }

        public abstract void Upgrade();
        public abstract void Evolve();
        protected abstract void Attack();

        protected virtual void Update()
        {
            if (!IsActive) return;

            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0f)
            {
                Attack();
                cooldownTimer = GetEffectiveCooldown();
            }
        }

        public virtual float GetEffectiveCooldown()
        {
            float rate = 1f;
            if (WeaponManager.Instance != null)
            {
                rate += WeaponManager.Instance.AttackSpeedLevel * 0.15f;
            }
            if (Meta.GameDataManager.Instance != null && Meta.GameDataManager.Instance.Profile.inventory.Contains("equip_coffee_thermos"))
            {
                rate += 0.15f;
            }

            if (Core.FeverManager.Instance != null && Core.FeverManager.Instance.IsFever)
            {
                rate *= 1.8f;
            }

            return Mathf.Max(0.08f, baseCooldown / rate);
        }

        public virtual float CalculateDamage(float rawDamage, out bool isCrit)
        {
            float dmg = rawDamage;

            // 1. 패시브 공격력 증가
            if (WeaponManager.Instance != null)
            {
                dmg *= (1f + WeaponManager.Instance.DamageBoostLevel * 0.12f);
            }

            // 2. 본부 영구 연구소 공격력 보너스
            if (Meta.GameDataManager.Instance != null)
            {
                dmg *= (1f + Meta.GameDataManager.Instance.Profile.researchAtkLevel * 0.08f);
            }

            // 3. 디톡스 피버 모드 데미지 증폭 (+40%)
            if (Core.FeverManager.Instance != null && Core.FeverManager.Instance.IsFever)
            {
                dmg *= 1.4f;
            }

            // 4. 치명타 확률 계산 (기본 5% + 패시브 레벨당 8%)
            float critChance = 0.05f;
            if (WeaponManager.Instance != null)
            {
                critChance += WeaponManager.Instance.CritChanceLevel * 0.08f;
            }

            isCrit = Random.value < critChance;
            if (isCrit)
            {
                dmg *= 2.0f;
            }

            return dmg;
        }
    }
}
