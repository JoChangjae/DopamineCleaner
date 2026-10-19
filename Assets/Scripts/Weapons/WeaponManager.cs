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
        MoveSpeed,
        MaxHP,
        Magnet,
        AttackSpeed,
        EvoGigaGatling,
        EvoGalacticCleanse,
        EvoPrismCannon,
        EvoSecurityMatrix,
        EvoTeslaDischarge,
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

        [Header("무기 컴포넌트")]
        [SerializeField] private CleanBulletWeapon cleanBulletWeapon;
        [SerializeField] private OrbitWeapon orbitWeapon;
        [SerializeField] private LaserBeamWeapon laserBeamWeapon;
        [SerializeField] private FirewallMineWeapon mineWeapon;
        [SerializeField] private ShockAuraWeapon shockWeapon;

        [Header("패시브 레벨")]
        [SerializeField] private int moveSpeedLevel = 0;
        [SerializeField] private int maxHPLevel = 0;
        [SerializeField] private int magnetLevel = 0;
        [SerializeField] private int attackSpeedLevel = 0;

        public CleanBulletWeapon BulletWeapon => cleanBulletWeapon;
        public OrbitWeapon OrbitWeapon => orbitWeapon;
        public LaserBeamWeapon LaserWeapon => laserBeamWeapon;
        public FirewallMineWeapon MineWeapon => mineWeapon;
        public ShockAuraWeapon ShockWeapon => shockWeapon;
        public int AttackSpeedLevel => attackSpeedLevel;

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

            // 없으면 동적 추가
            if (laserBeamWeapon == null) laserBeamWeapon = gameObject.AddComponent<LaserBeamWeapon>();
            if (mineWeapon == null) mineWeapon = gameObject.AddComponent<FirewallMineWeapon>();
            if (shockWeapon == null) shockWeapon = gameObject.AddComponent<ShockAuraWeapon>();
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

            // 1. 진화 후보 체크
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

            // 2. 일반 무기 후보
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

            // 3. 패시브 스킬 후보
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

            // 4. 모든 스킬 만렙 시 제공되는 '한계 돌파 / 만렙 보너스'
            if (candidates.Count == 0)
            {
                candidates.Add(new UpgradeCardInfo
                {
                    type = UpgradeType.BonusGold,
                    title = "💰 디톡스 정화 보너스 (+100G)",
                    description = "모든 능력을 마스터했습니다! 즉시 추가 골드 100G를 정산금에 적립합니다.",
                    iconText = "💰",
                    isEvolution = false
                });

                candidates.Add(new UpgradeCardInfo
                {
                    type = UpgradeType.BonusHeal,
                    title = "💖 긴급 영양 보충 (+40 HP)",
                    description = "도파민 피로를 씻어내고 체력을 40 즉시 회복합니다.",
                    iconText = "💖",
                    isEvolution = false
                });

                candidates.Add(new UpgradeCardInfo
                {
                    type = UpgradeType.BonusBomb,
                    title = "💣 전면 방역 살포 (화면 폭탄)",
                    description = "화면 내의 모든 일반 몬스터를 일망타진 소독합니다.",
                    iconText = "💣",
                    isEvolution = false
                });
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
                case UpgradeType.EvoGigaGatling:
                    cleanBulletWeapon?.Evolve();
                    break;
                case UpgradeType.EvoGalacticCleanse:
                    orbitWeapon?.Evolve();
                    break;
                case UpgradeType.EvoPrismCannon:
                    laserBeamWeapon?.Evolve();
                    break;
                case UpgradeType.EvoSecurityMatrix:
                    mineWeapon?.Evolve();
                    break;
                case UpgradeType.EvoTeslaDischarge:
                    shockWeapon?.Evolve();
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

            GameManager.Instance?.SetState(GameState.Playing);
        }

        public void OpenTreasureChestJackpot()
        {
            UIManager.Instance?.ShowChestRewardUI();
        }
    }
}
