using System;
using System.Collections.Generic;
using UnityEngine;
using DopamineCleaner.Characters;
using DopamineCleaner.Core;

namespace DopamineCleaner.Weapons
{
    public enum SynergyType
    {
        OfficeFury,        // 💼 [사무직의 분노]: 클린 탄환 + 방화벽 지뢰 + 집중력 앰플
        DigitalShutdown,   // 🚫 [디지털 셧다운]: 디톡스 레이저 + 셧다운 단두대 + 블루라이트 차단경
        CapitalistBarrage, // 💰 [자본주의 폭격]: 디톡스 미사일 + 기가 개틀링/클린 탄환 + 황금 보너스
        SterileBioDome     // 🛡️ [완전 멸균 결계]: 소독 안개 + 가디언 위성 + 쇼크 펄스
    }

    [Serializable]
    public class SynergyInfo
    {
        public SynergyType type;
        public string title;
        public string description;
        public string icon;
        public bool isActive;
    }

    public class SynergyManager : MonoBehaviour
    {
        public static SynergyManager Instance { get; private set; }

        public static event Action<SynergyInfo> OnSynergyActivated;

        private readonly Dictionary<SynergyType, SynergyInfo> synergyMap = new();
        private float checkTimer = 0f;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            InitSynergyDefinitions();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void InitSynergyDefinitions()
        {
            synergyMap[SynergyType.OfficeFury] = new SynergyInfo
            {
                type = SynergyType.OfficeFury,
                title = "💼 [사무직의 분노]",
                description = "클린 탄환 + 방화벽 지뢰 + 공격력 앰플\n모든 넉백 2배, 폭발 범위 +40%, 지뢰 25% 연쇄 폭발!",
                icon = "🔥",
                isActive = false
            };

            synergyMap[SynergyType.DigitalShutdown] = new SynergyInfo
            {
                type = SynergyType.DigitalShutdown,
                title = "🚫 [디지털 셧다운]",
                description = "디톡스 레이저 + 셧다운 단두대 + 치명타경\n치명타 피격 시 적 1.5초 완전 정지(기절), 처형선 35%로 강화!",
                icon = "⚡",
                isActive = false
            };

            synergyMap[SynergyType.CapitalistBarrage] = new SynergyInfo
            {
                type = SynergyType.CapitalistBarrage,
                title = "💰 [자본주의 폭격]",
                description = "디톡스 미사일 + 클린 탄환 + 황금 보너스\n적 타격 시 2% 확률로 골드 드롭, 골드 획득 시 1초간 이속 +20%!",
                icon = "🪙",
                isActive = false
            };

            synergyMap[SynergyType.SterileBioDome] = new SynergyInfo
            {
                type = SynergyType.SterileBioDome,
                title = "🛡️ [완전 멸균 결계]",
                description = "소독 안개 + 가디언 위성 + 쇼크 펄스\n주변 4m 절대 보안 구역 형성: 적 이속 -50% & 초당 40 정화 벼락!",
                icon = "🔮",
                isActive = false
            };
        }

        private void Update()
        {
            checkTimer -= Time.deltaTime;
            if (checkTimer <= 0f)
            {
                checkTimer = 1.0f;
                EvaluateSynergies();
            }

            // 완전 멸균 결계 지속 효과 (활성화 시)
            if (HasSynergy(SynergyType.SterileBioDome) && PlayerController.Instance != null)
            {
                ApplySterileDomeTick();
            }
        }

        public bool HasSynergy(SynergyType type)
        {
            if (synergyMap.Count == 0) InitSynergyDefinitions();
            return synergyMap.TryGetValue(type, out var info) && info.isActive;
        }

        public List<SynergyInfo> GetActiveSynergies()
        {
            if (synergyMap.Count == 0) InitSynergyDefinitions();
            var list = new List<SynergyInfo>();
            foreach (var kvp in synergyMap)
            {
                if (kvp.Value.isActive) list.Add(kvp.Value);
            }
            return list;
        }

        public void EvaluateSynergies()
        {
            if (synergyMap.Count == 0) InitSynergyDefinitions();
            var wm = WeaponManager.Instance;
            if (wm == null) return;

            // 1. 사무직의 분노 체크
            bool bulletActive = wm.IsWeaponActive(WeaponType.CleanBullet);
            bool mineActive = wm.IsWeaponActive(WeaponType.FirewallMine);
            bool dmgBoost = wm.DamageBoostLevel >= 1;
            CheckAndActivate(SynergyType.OfficeFury, bulletActive && mineActive && dmgBoost);

            // 2. 디지털 셧다운 체크
            bool laserActive = wm.IsWeaponActive(WeaponType.LaserBeam);
            bool scytheActive = wm.IsWeaponActive(WeaponType.CyberScythe);
            bool critBoost = wm.CritChanceLevel >= 1;
            CheckAndActivate(SynergyType.DigitalShutdown, laserActive && scytheActive && critBoost);

            // 3. 자본주의 폭격 체크
            bool missileActive = wm.IsWeaponActive(WeaponType.DetoxMissile);
            bool hasGoldArtifact = Meta.GameDataManager.Instance != null && 
                                   Meta.GameDataManager.Instance.Profile.inventory.Contains("artifact_golden_calc");
            bool bonusGoldUnlocked = bulletActive && missileActive && (hasGoldArtifact || GameManager.Instance != null && GameManager.Instance.BonusGold > 0);
            CheckAndActivate(SynergyType.CapitalistBarrage, bonusGoldUnlocked);

            // 4. 완전 멸균 결계 체크
            bool mistActive = wm.IsWeaponActive(WeaponType.SanitizerMist);
            bool orbitActive = wm.IsWeaponActive(WeaponType.OrbitBarrier);
            bool shockActive = wm.IsWeaponActive(WeaponType.ShockPulse);
            CheckAndActivate(SynergyType.SterileBioDome, mistActive && orbitActive && shockActive);
        }

        private void CheckAndActivate(SynergyType type, bool conditionMet)
        {
            if (!synergyMap.TryGetValue(type, out var info)) return;

            if (conditionMet && !info.isActive)
            {
                info.isActive = true;
                SoundManager.Instance?.Play(SoundEffect.Fever, 0.8f);
                CameraFollow.Instance?.Shake(0.3f, 0.4f);
                OnSynergyActivated?.Invoke(info);
                Debug.Log($"✨ [세트 시너지 발동] {info.title}");
            }
        }

        private float domeTickTimer = 0f;
        private void ApplySterileDomeTick()
        {
            domeTickTimer -= Time.deltaTime;
            if (domeTickTimer > 0f) return;
            domeTickTimer = 0.5f;

            Vector3 center = PlayerController.Instance.transform.position;
            var hits = Physics2D.OverlapCircleAll(center, 4.0f);
            foreach (var hit in hits)
            {
                if (hit != null && hit.CompareTag("Enemy") && hit.TryGetComponent<Enemies.Enemy>(out var enemy))
                {
                    enemy.TakeDamage(20f);
                    // 넉백 밀쳐내기
                    Vector2 away = ((Vector2)enemy.transform.position - (Vector2)center).normalized;
                    enemy.transform.position += (Vector3)(away * 0.35f);
                }
            }
        }
    }
}
