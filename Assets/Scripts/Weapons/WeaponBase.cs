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
    }
}
