using System.Collections.Generic;
using UnityEngine;
using DopamineCleaner.Core;
using DopamineCleaner.Characters;
using DopamineCleaner.UI;

namespace DopamineCleaner.Weapons
{
    public enum UpgradeType
    {
        CleanBullet,
        OrbitBarrier,
        LaserBeam,
        FirewallMine,
        ShockAura,
        DetoxMissile,
        AlgorithmVortex,
        SanitizerMist,
        CyberScythe,

        MoveSpeed,
        MaxHP,
        Magnet,
        AttackSpeed,
        DamageBoost,
        CritChance,

        EvoGigaGatling,
        EvoGalacticCleanse,
        EvoPrismCannon,
        EvoSecurityMatrix,
        EvoTeslaDischarge,
        EvoAegisBarrage,
        EvoDetoxTyphoon,
        EvoPlasmaCloud,
        EvoShutdownBlade,

        BonusGold,
        BonusHeal,
        BonusBomb
    }

    [System.Serializable]
    public struct UpgradeCardInfo
    {
        public UpgradeType type;
        public string title;
        public string description;
        public string iconText;
        public bool isEvolution;
    }

    public class WeaponManager : MonoBehaviour
    {
        public static WeaponManager Instance { get; private set; }

        [Header("무기 컴포넌트 9종")]
        [SerializeField] private CleanBulletWeapon cleanBulletWeapon;
        [SerializeField] private OrbitWeapon orbitWeapon;
        [SerializeField] private LaserBeamWeapon laserBeamWeapon;
        [SerializeField] private FirewallMineWeapon mineWeapon;
        [SerializeField] private ShockAuraWeapon shockWeapon;
        [SerializeField] private DetoxMissileWeapon missileWeapon;
        [SerializeField] private AlgorithmVortexWeapon vortexWeapon;
        [SerializeField] private SanitizerMistWeapon mistWeapon;
        [SerializeField] private CyberScytheWeapon scytheWeapon;

        [Header("패시브 레벨 6종")]
        [SerializeField] private int moveSpeedLevel = 0;
        [SerializeField] private int maxHPLevel = 0;
        [SerializeField] private int magnetLevel = 0;
        [SerializeField] private int attackSpeedLevel = 0;
        [SerializeField] private int damageBoostLevel = 0;
        [SerializeField] private int critChanceLevel = 0;

        public CleanBulletWeapon BulletWeapon => cleanBulletWeapon;
        public OrbitWeapon OrbitWeapon => orbitWeapon;
        public LaserBeamWeapon LaserWeapon => laserBeamWeapon;
        public FirewallMineWeapon MineWeapon => mineWeapon;
        public ShockAuraWeapon ShockWeapon => shockWeapon;
        public DetoxMissileWeapon MissileWeapon => missileWeapon;
        public AlgorithmVortexWeapon VortexWeapon => vortexWeapon;
        public SanitizerMistWeapon MistWeapon => mistWeapon;
        public CyberScytheWeapon ScytheWeapon => scytheWeapon;

        public int AttackSpeedLevel => attackSpeedLevel;
        public int MoveSpeedLevel => moveSpeedLevel;
        public int MaxHPLevel => maxHPLevel;
        public int MagnetLevel => magnetLevel;
        public int DamageBoostLevel => damageBoostLevel;
        public int CritChanceLevel => critChanceLevel;

        public string GetEquipmentSummary()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("⚔️ [장착 무기 현황 (총 9종 아스날)]");

            WeaponBase[] weapons = { cleanBulletWeapon, orbitWeapon, laserBeamWeapon, mineWeapon, shockWeapon, missileWeapon, vortexWeapon, mistWeapon, scytheWeapon };
            string[] icons = { "💧", "🛡️", "🔦", "💣", "⚡", "🚀", "🌪️", "🧪", "🪓" };

            int activeCount = 0;
            for (int i = 0; i < weapons.Length; i++)
            {
                if (weapons[i] != null && weapons[i].IsActive)
                {
                    activeCount++;
                    string status = weapons[i].IsEvolved ? "✨ EVOLVED (진화 완료)" : $"Lv.{weapons[i].Level}";
                    sb.AppendLine($"  {icons[i]} {weapons[i].WeaponName} [{status}] - 대미지: {weapons[i].TotalDamageDealt:N0}");
                }
            }
            if (activeCount == 0) sb.AppendLine("  (장착된 무기 없음)");

            sb.AppendLine();
            sb.AppendLine("🧬 [패시브 훈련 현황]");
            sb.AppendLine($"  🥾 달콤한 신발 (이동속도) : Lv.{moveSpeedLevel} / 5");
            sb.AppendLine($"  💖 하트 비타민 (최대체력) : Lv.{maxHPLevel} / 5");
            sb.AppendLine($"  🧲 도파민 자석 (흡수반경) : Lv.{magnetLevel} / 5");
            sb.AppendLine($"  ☕ 에스프레소 (공격속도) : Lv.{attackSpeedLevel} / 5");
            sb.AppendLine($"  💊 집중력 앰플 (전체공격력) : Lv.{damageBoostLevel} / 5");
            sb.AppendLine($"  👓 블루라이트 안경 (치명타율) : Lv.{critChanceLevel} / 5");

            return sb.ToString();
        }

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);

            if (cleanBulletWeapon == null) cleanBulletWeapon = GetComponentInChildren<CleanBulletWeapon>();
            if (orbitWeapon == null) orbitWeapon = GetComponentInChildren<OrbitWeapon>();
            if (laserBeamWeapon == null) laserBeamWeapon = GetComponentInChildren<LaserBeamWeapon>();
            if (mineWeapon == null) mineWeapon = GetComponentInChildren<FirewallMineWeapon>();
            if (shockWeapon == null) shockWeapon = GetComponentInChildren<ShockAuraWeapon>();
            if (missileWeapon == null) missileWeapon = GetComponentInChildren<DetoxMissileWeapon>();
            if (vortexWeapon == null) vortexWeapon = GetComponentInChildren<AlgorithmVortexWeapon>();
            if (mistWeapon == null) mistWeapon = GetComponentInChildren<SanitizerMistWeapon>();
            if (scytheWeapon == null) scytheWeapon = GetComponentInChildren<CyberScytheWeapon>();

            // 없으면 동적 추가
            if (laserBeamWeapon == null) laserBeamWeapon = gameObject.AddComponent<LaserBeamWeapon>();
            if (mineWeapon == null) mineWeapon = gameObject.AddComponent<FirewallMineWeapon>();
            if (shockWeapon == null) shockWeapon = gameObject.AddComponent<ShockAuraWeapon>();
            if (missileWeapon == null) missileWeapon = gameObject.AddComponent<DetoxMissileWeapon>();
            if (vortexWeapon == null) vortexWeapon = gameObject.AddComponent<AlgorithmVortexWeapon>();
            if (mistWeapon == null) mistWeapon = gameObject.AddComponent<SanitizerMistWeapon>();
            if (scytheWeapon == null) scytheWeapon = gameObject.AddComponent<CyberScytheWeapon>();
        }

        private void Start()
        {
            // 기본 무기 1레벨 시작
            cleanBulletWeapon?.Upgrade();

            // 윤이사(도파민 총괄) 요원 특권: 시작부터 가디언 위성 1기 장착 출동
            if (Meta.GameDataManager.Instance != null && Meta.GameDataManager.Instance.Profile.selectedCharacterId == "char_director")
            {
                orbitWeapon?.Upgrade();
            }
        }

        public List<UpgradeCardInfo> GetRandomUpgradeOptions(int count = 3)
        {
            List<UpgradeCardInfo> candidates = new();

            // 1. 진화 후보 체크 (총 9종)
            if (cleanBulletWeapon != null && cleanBulletWeapon.Level >= 5 && !cleanBulletWeapon.IsEvolved && attackSpeedLevel >= 1)
            {
                candidates.Add(new UpgradeCardInfo
                {
                    type = UpgradeType.EvoGigaGatling,
                    title = "✨ 기가 개틀링",
                    description = "8방향 고속 연사 탄막으로 전방을 융단 폭격합니다!",
                    iconText = "🔫",
                    isEvolution = true
                });
            }

            if (orbitWeapon != null && orbitWeapon.Level >= 5 && !orbitWeapon.IsEvolved && maxHPLevel >= 1)
            {
                candidates.Add(new UpgradeCardInfo
                {
                    type = UpgradeType.EvoGalacticCleanse,
                    title = "✨ 은하계 정화 결계",
                    description = "6개의 초거대 황금 위성이 모든 잡생각을 갈아버립니다!",
                    iconText = "🪐",
                    isEvolution = true
                });
            }

            if (laserBeamWeapon != null && laserBeamWeapon.Level >= 5 && !laserBeamWeapon.IsEvolved && magnetLevel >= 1)
            {
                candidates.Add(new UpgradeCardInfo
                {
                    type = UpgradeType.EvoPrismCannon,
                    title = "✨ 프리즘 정화포",
                    description = "십자 4방향으로 거대 관통 프리즘 레이저를 일제 사격합니다!",
                    iconText = "🌈",
                    isEvolution = true
                });
            }

            if (mineWeapon != null && mineWeapon.Level >= 5 && !mineWeapon.IsEvolved && moveSpeedLevel >= 1)
            {
                candidates.Add(new UpgradeCardInfo
                {
                    type = UpgradeType.EvoSecurityMatrix,
                    title = "✨ 절대 보안 결계",
                    description = "발밑에 초대형 폭발 지뢰를 맹렬히 투하합니다!",
                    iconText = "💥",
                    isEvolution = true
                });
            }

            if (shockWeapon != null && shockWeapon.Level >= 5 && !shockWeapon.IsEvolved && attackSpeedLevel >= 1)
            {
                candidates.Add(new UpgradeCardInfo
                {
                    type = UpgradeType.EvoTeslaDischarge,
                    title = "✨ 테슬라 뇌파 방전",
                    description = "화면 내 다수의 적들에게 강력한 연쇄 벼락 폭풍을 내리꽂습니다!",
                    iconText = "⚡",
                    isEvolution = true
                });
            }

            if (missileWeapon != null && missileWeapon.Level >= 5 && !missileWeapon.IsEvolved && (attackSpeedLevel >= 1 || damageBoostLevel >= 1))
            {
                candidates.Add(new UpgradeCardInfo
                {
                    type = UpgradeType.EvoAegisBarrage,
                    title = "✨ 궤도 폭격 이지스",
                    description = "위성 궤도에서 대량의 스마트 탄도 미사일이 화면 전체를 맹폭합니다!",
                    iconText = "🚀",
                    isEvolution = true
                });
            }

            if (vortexWeapon != null && vortexWeapon.Level >= 5 && !vortexWeapon.IsEvolved && moveSpeedLevel >= 1)
            {
                candidates.Add(new UpgradeCardInfo
                {
                    type = UpgradeType.EvoDetoxTyphoon,
                    title = "✨ 하이퍼 디톡스 타이푼",
                    description = "황금빛 거대 회오리가 화면을 휩쓸며 몬스터를 영구 흡입 분쇄합니다!",
                    iconText = "🌪️",
                    isEvolution = true
                });
            }

            if (mistWeapon != null && mistWeapon.Level >= 5 && !mistWeapon.IsEvolved && magnetLevel >= 1)
            {
                candidates.Add(new UpgradeCardInfo
                {
                    type = UpgradeType.EvoPlasmaCloud,
                    title = "✨ 절대 멸균 플라즈마",
                    description = "플레이어 주위에 거대한 청록색 플라즈마 멸균 영역이 상시 유지됩니다!",
                    iconText = "🧪",
                    isEvolution = true
                });
            }

            if (scytheWeapon != null && scytheWeapon.Level >= 5 && !scytheWeapon.IsEvolved && (maxHPLevel >= 1 || critChanceLevel >= 1))
            {
                candidates.Add(new UpgradeCardInfo
                {
                    type = UpgradeType.EvoShutdownBlade,
                    title = "✨ 절대 셧다운 엑스칼리버",
                    description = "화면 전체를 360도로 베어 넘기며 약화된 일반 몬스터를 즉사시킵니다!",
                    iconText = "🪓",
                    isEvolution = true
                });
            }

            // 2. 일반 무기 후보 (총 9종)
            if (cleanBulletWeapon != null && cleanBulletWeapon.Level < 5 && !cleanBulletWeapon.IsEvolved)
            {
                candidates.Add(new UpgradeCardInfo
                {
                    type = UpgradeType.CleanBullet,
                    title = cleanBulletWeapon.Level == 0 ? "클린 탄환 (신규)" : $"클린 탄환 Lv.{cleanBulletWeapon.Level + 1}",
                    description = "가장 가까운 적을 자동 조준하여 탄환을 발사합니다.",
                    iconText = "💧",
                    isEvolution = false
                });
            }

            if (orbitWeapon != null && orbitWeapon.Level < 5 && !orbitWeapon.IsEvolved)
            {
                candidates.Add(new UpgradeCardInfo
                {
                    type = UpgradeType.OrbitBarrier,
                    title = orbitWeapon.Level == 0 ? "가디언 위성 (신규)" : $"가디언 위성 Lv.{orbitWeapon.Level + 1}",
                    description = "주변을 회전하며 접근하는 적을 밀어내고 피해를 줍니다.",
                    iconText = "🛡️",
                    isEvolution = false
                });
            }

            if (laserBeamWeapon != null && laserBeamWeapon.Level < 5 && !laserBeamWeapon.IsEvolved)
            {
                candidates.Add(new UpgradeCardInfo
                {
                    type = UpgradeType.LaserBeam,
                    title = laserBeamWeapon.Level == 0 ? "디톡스 레이저 (신규)" : $"디톡스 레이저 Lv.{laserBeamWeapon.Level + 1}",
                    description = "가장 가까운 적을 관통하는 고출력 정화 레이저를 쏩니다.",
                    iconText = "🔦",
                    isEvolution = false
                });
            }

            if (mineWeapon != null && mineWeapon.Level < 5 && !mineWeapon.IsEvolved)
            {
                candidates.Add(new UpgradeCardInfo
                {
                    type = UpgradeType.FirewallMine,
                    title = mineWeapon.Level == 0 ? "방화벽 지뢰 (신규)" : $"방화벽 지뢰 Lv.{mineWeapon.Level + 1}",
                    description = "이동 경로에 지뢰를 설치하여 밟은 적을 폭파시킵니다.",
                    iconText = "💣",
                    isEvolution = false
                });
            }

            if (shockWeapon != null && shockWeapon.Level < 5 && !shockWeapon.IsEvolved)
            {
                candidates.Add(new UpgradeCardInfo
                {
                    type = UpgradeType.ShockAura,
                    title = shockWeapon.Level == 0 ? "쇼크 펄스 (신규)" : $"쇼크 펄스 Lv.{shockWeapon.Level + 1}",
                    description = "주변의 여러 적에게 연쇄 번개 전류를 방전합니다.",
                    iconText = "⚡",
                    isEvolution = false
                });
            }

            if (missileWeapon != null && missileWeapon.Level < 5 && !missileWeapon.IsEvolved)
            {
                candidates.Add(new UpgradeCardInfo
                {
                    type = UpgradeType.DetoxMissile,
                    title = missileWeapon.Level == 0 ? "디톡스 미사일 (신규)" : $"디톡스 미사일 Lv.{missileWeapon.Level + 1}",
                    description = "스마트 패킷 미사일을 발사하여 적을 자동 추적 폭격합니다.",
                    iconText = "🚀",
                    isEvolution = false
                });
            }

            if (vortexWeapon != null && vortexWeapon.Level < 5 && !vortexWeapon.IsEvolved)
            {
                candidates.Add(new UpgradeCardInfo
                {
                    type = UpgradeType.AlgorithmVortex,
                    title = vortexWeapon.Level == 0 ? "알고리즘 회오리 (신규)" : $"알고리즘 회오리 Lv.{vortexWeapon.Level + 1}",
                    description = "적들을 중심으로 끌어당기며 지속 피해를 주는 소용돌이를 날립니다.",
                    iconText = "🌪️",
                    isEvolution = false
                });
            }

            if (mistWeapon != null && mistWeapon.Level < 5 && !mistWeapon.IsEvolved)
            {
                candidates.Add(new UpgradeCardInfo
                {
                    type = UpgradeType.SanitizerMist,
                    title = mistWeapon.Level == 0 ? "멸균 소독 안개 (신규)" : $"멸균 소독 안개 Lv.{mistWeapon.Level + 1}",
                    description = "이동 경로에 소독 연무를 뿌려 적을 감속시키고 도트 피해를 줍니다.",
                    iconText = "🧪",
                    isEvolution = false
                });
            }

            if (scytheWeapon != null && scytheWeapon.Level < 5 && !scytheWeapon.IsEvolved)
            {
                candidates.Add(new UpgradeCardInfo
                {
                    type = UpgradeType.CyberScythe,
                    title = scytheWeapon.Level == 0 ? "셧다운 단두대 (신규)" : $"셧다운 단두대 Lv.{scytheWeapon.Level + 1}",
                    description = "주변을 넓게 회전 베기하여 강력한 넉백과 피해를 줍니다.",
                    iconText = "🪓",
                    isEvolution = false
                });
            }

            // 3. 패시브 스킬 후보 (총 6종)
            if (moveSpeedLevel < 5)
            {
                candidates.Add(new UpgradeCardInfo
                {
                    type = UpgradeType.MoveSpeed,
                    title = $"달콤한 신발 Lv.{moveSpeedLevel + 1}",
                    description = "이동 속도가 15% 영구 증가합니다.",
                    iconText = "🥾",
                    isEvolution = false
                });
            }

            if (maxHPLevel < 5)
            {
                candidates.Add(new UpgradeCardInfo
                {
                    type = UpgradeType.MaxHP,
                    title = $"하트 비타민 Lv.{maxHPLevel + 1}",
                    description = "최대 체력이 +20 증가하고 즉시 회복됩니다.",
                    iconText = "💖",
                    isEvolution = false
                });
            }

            if (magnetLevel < 5)
            {
                candidates.Add(new UpgradeCardInfo
                {
                    type = UpgradeType.Magnet,
                    title = $"도파민 자석 Lv.{magnetLevel + 1}",
                    description = "아이템 및 경험치 흡수 반경이 30% 증가합니다.",
                    iconText = "🧲",
                    isEvolution = false
                });
            }

            if (attackSpeedLevel < 5)
            {
                candidates.Add(new UpgradeCardInfo
                {
                    type = UpgradeType.AttackSpeed,
                    title = $"에스프레소 부스터 Lv.{attackSpeedLevel + 1}",
                    description = "모든 무기의 공격 주기가 15% 빨라집니다.",
                    iconText = "☕",
                    isEvolution = false
                });
            }

            if (damageBoostLevel < 5)
            {
                candidates.Add(new UpgradeCardInfo
                {
                    type = UpgradeType.DamageBoost,
                    title = $"집중력 앰플 Lv.{damageBoostLevel + 1}",
                    description = "모든 무기의 피해량이 12% 영구 증가합니다.",
                    iconText = "💊",
                    isEvolution = false
                });
            }

            if (critChanceLevel < 5)
            {
                candidates.Add(new UpgradeCardInfo
                {
                    type = UpgradeType.CritChance,
                    title = $"블루라이트 차단경 Lv.{critChanceLevel + 1}",
                    description = "치명타 확률이 +8% 증가하고 200% 피해를 입힙니다.",
                    iconText = "👓",
                    isEvolution = false
                });
            }

            // 4. 업그레이드 후보가 부족하거나 만렙일 때 제공되는 '한계 돌파 / 보너스 카드' 보충
            if (candidates.Count < count)
            {
                var bonusCards = new List<UpgradeCardInfo>
                {
                    new UpgradeCardInfo
                    {
                        type = UpgradeType.BonusGold,
                        title = "💰 디톡스 정화 보너스 (+100G)",
                        description = "정산금에 즉시 추가 골드 100G를 적립합니다.",
                        iconText = "💰",
                        isEvolution = false
                    },
                    new UpgradeCardInfo
                    {
                        type = UpgradeType.BonusHeal,
                        title = "💖 긴급 영양 보충 (+40 HP)",
                        description = "도파민 피로를 씻어내고 체력을 40 즉시 회복합니다.",
                        iconText = "💖",
                        isEvolution = false
                    },
                    new UpgradeCardInfo
                    {
                        type = UpgradeType.BonusBomb,
                        title = "💣 전면 방역 살포 (화면 폭탄)",
                        description = "화면 내의 모든 일반 몬스터를 일망타진 소독합니다.",
                        iconText = "💣",
                        isEvolution = false
                    }
                };

                for (int b = 0; b < bonusCards.Count && candidates.Count < count; b++)
                {
                    candidates.Add(bonusCards[b]);
                }
            }

            // 셔플 및 count개 추출
            for (int i = 0; i < candidates.Count; i++)
            {
                int r = Random.Range(i, candidates.Count);
                (candidates[i], candidates[r]) = (candidates[r], candidates[i]);
            }

            int returnCount = Mathf.Min(count, candidates.Count);
            return candidates.GetRange(0, returnCount);
        }

        public void ApplyUpgrade(UpgradeType type)
        {
            switch (type)
            {
                case UpgradeType.CleanBullet:
                    cleanBulletWeapon?.Upgrade();
                    break;
                case UpgradeType.OrbitBarrier:
                    orbitWeapon?.Upgrade();
                    break;
                case UpgradeType.LaserBeam:
                    laserBeamWeapon?.Upgrade();
                    break;
                case UpgradeType.FirewallMine:
                    mineWeapon?.Upgrade();
                    break;
                case UpgradeType.ShockAura:
                    shockWeapon?.Upgrade();
                    break;
                case UpgradeType.DetoxMissile:
                    missileWeapon?.Upgrade();
                    break;
                case UpgradeType.AlgorithmVortex:
                    vortexWeapon?.Upgrade();
                    break;
                case UpgradeType.SanitizerMist:
                    mistWeapon?.Upgrade();
                    break;
                case UpgradeType.CyberScythe:
                    scytheWeapon?.Upgrade();
                    break;

                case UpgradeType.EvoGigaGatling:
                    cleanBulletWeapon?.Evolve();
                    CheckEvolutionAchievements();
                    break;
                case UpgradeType.EvoGalacticCleanse:
                    orbitWeapon?.Evolve();
                    CheckEvolutionAchievements();
                    break;
                case UpgradeType.EvoPrismCannon:
                    laserBeamWeapon?.Evolve();
                    CheckEvolutionAchievements();
                    break;
                case UpgradeType.EvoSecurityMatrix:
                    mineWeapon?.Evolve();
                    CheckEvolutionAchievements();
                    break;
                case UpgradeType.EvoTeslaDischarge:
                    shockWeapon?.Evolve();
                    CheckEvolutionAchievements();
                    break;
                case UpgradeType.EvoAegisBarrage:
                    missileWeapon?.Evolve();
                    CheckEvolutionAchievements();
                    break;
                case UpgradeType.EvoDetoxTyphoon:
                    vortexWeapon?.Evolve();
                    CheckEvolutionAchievements();
                    break;
                case UpgradeType.EvoPlasmaCloud:
                    mistWeapon?.Evolve();
                    CheckEvolutionAchievements();
                    break;
                case UpgradeType.EvoShutdownBlade:
                    scytheWeapon?.Evolve();
                    CheckEvolutionAchievements();
                    break;

                case UpgradeType.MoveSpeed:
                    moveSpeedLevel++;
                    PlayerController.Instance?.UpgradeMoveSpeed(1.15f);
                    break;
                case UpgradeType.MaxHP:
                    maxHPLevel++;
                    PlayerController.Instance?.UpgradeMaxHP(20f);
                    break;
                case UpgradeType.Magnet:
                    magnetLevel++;
                    PlayerController.Instance?.UpgradeMagnet(1.2f);
                    break;
                case UpgradeType.AttackSpeed:
                    attackSpeedLevel++;
                    break;
                case UpgradeType.DamageBoost:
                    damageBoostLevel++;
                    break;
                case UpgradeType.CritChance:
                    critChanceLevel++;
                    break;

                case UpgradeType.BonusGold:
                    GameManager.Instance?.AddBonusGold(100);
                    break;
                case UpgradeType.BonusHeal:
                    PlayerController.Instance?.Heal(40f);
                    break;
                case UpgradeType.BonusBomb:
                    Items.ItemManager.Instance?.TriggerScreenBomb();
                    break;
            }
        }

        private void CheckEvolutionAchievements()
        {
            WeaponBase[] weapons = { cleanBulletWeapon, orbitWeapon, laserBeamWeapon, mineWeapon, shockWeapon, missileWeapon, vortexWeapon, mistWeapon, scytheWeapon };
            int evolvedCount = 0;
            foreach (var w in weapons)
            {
                if (w != null && w.IsEvolved) evolvedCount++;
            }
            Meta.AchievementManager.Instance?.ReportEvolution(evolvedCount);
        }

        public void OpenTreasureChestJackpot()
        {
            UIManager.Instance?.ShowChestRewardUI();
        }

        public struct WeaponDamageStat
        {
            public string name;
            public string icon;
            public float damage;
            public float percentage;
        }

        public List<WeaponDamageStat> GetDamageStatistics()
        {
            var list = new List<WeaponDamageStat>();
            float total = 0f;

            WeaponBase[] weapons = { cleanBulletWeapon, orbitWeapon, laserBeamWeapon, mineWeapon, shockWeapon, missileWeapon, vortexWeapon, mistWeapon, scytheWeapon };
            string[] icons = { "💧", "🛡️", "🔦", "💣", "⚡", "🚀", "🌪️", "🧪", "🪓" };

            for (int i = 0; i < weapons.Length; i++)
            {
                if (weapons[i] != null && weapons[i].IsActive)
                {
                    total += weapons[i].TotalDamageDealt;
                }
            }

            for (int i = 0; i < weapons.Length; i++)
            {
                if (weapons[i] != null && weapons[i].IsActive)
                {
                    float pct = total > 0f ? (weapons[i].TotalDamageDealt / total) * 100f : 0f;
                    list.Add(new WeaponDamageStat
                    {
                        name = weapons[i].WeaponName,
                        icon = icons[i],
                        damage = weapons[i].TotalDamageDealt,
                        percentage = pct
                    });
                }
            }

            list.Sort((a, b) => b.damage.CompareTo(a.damage));
            return list;
        }
    }
}
