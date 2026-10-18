using System.Collections.Generic;
using UnityEngine;
using DopamineCleaner.Core;
using DopamineCleaner.Characters;

namespace DopamineCleaner.Weapons
{
    public enum UpgradeType
    {
        CleanBullet,
        OrbitBarrier,
        MoveSpeed,
        MaxHP,
        Magnet,
        AttackSpeed,
        EvoGigaGatling,
        EvoGalacticCleanse
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

        [Header("패시브 레벨")]
        [SerializeField] private int moveSpeedLevel = 0;
        [SerializeField] private int maxHPLevel = 0;
        [SerializeField] private int magnetLevel = 0;
        [SerializeField] private int attackSpeedLevel = 0;

        public CleanBulletWeapon BulletWeapon => cleanBulletWeapon;
        public OrbitWeapon OrbitWeapon => orbitWeapon;
        public int AttackSpeedLevel => attackSpeedLevel;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);

            if (cleanBulletWeapon == null) cleanBulletWeapon = GetComponentInChildren<CleanBulletWeapon>();
            if (orbitWeapon == null) orbitWeapon = GetComponentInChildren<OrbitWeapon>();
        }

        private void Start()
        {
            // 기본 무기 1레벨 시작
            cleanBulletWeapon?.Upgrade();

            // 윤이사 사원 특권: 시작부터 가디언 위성 1기 장착 출동
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
                case UpgradeType.EvoGigaGatling:
                    cleanBulletWeapon?.Evolve();
                    break;
                case UpgradeType.EvoGalacticCleanse:
                    orbitWeapon?.Evolve();
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
            }

            GameManager.Instance?.SetState(GameState.Playing);
        }

        public void OpenTreasureChestJackpot()
        {
            UIManager.Instance?.ShowChestRewardUI();
        }
    }
}
