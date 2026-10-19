using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DopamineCleaner.Weapons;

namespace DopamineCleaner.UI
{
    public class UpgradeCardUI : MonoBehaviour
    {
        [Header("UI 배경 및 테두리")]
        [SerializeField] private Image cardBackground;
        [SerializeField] private Outline cardOutline;

        [Header("타입 뱃지")]
        [SerializeField] private GameObject evoBadge; // 레거시 호환
        [SerializeField] private Image typeBadgeBg;
        [SerializeField] private Text typeBadgeText;

        [Header("아이콘 및 타이틀")]
        [SerializeField] private Image iconImage;
        [SerializeField] private Text iconText;
        [SerializeField] private Text titleText;
        [SerializeField] private Text descriptionText;

        [Header("진화 족보 가이드 박스")]
        [SerializeField] private GameObject recipeBox;
        [SerializeField] private Text recipeGuideText;

        [Header("선택 버튼")]
        [SerializeField] private Button selectButton;
        [SerializeField] private Image selectButtonBg;
        [SerializeField] private Text selectButtonText;

        private static readonly Dictionary<UpgradeType, Sprite> iconCache = new();

        public static Sprite GetUpgradeSprite(UpgradeType type)
        {
            if (iconCache.TryGetValue(type, out var s) && s != null) return s;

            string resName = GetIconResourceName(type);
            Sprite loaded = Resources.Load<Sprite>("SkillIcons/" + resName);
            if (loaded != null)
            {
                iconCache[type] = loaded;
                return loaded;
            }
            return null;
        }

        public static string GetIconResourceName(UpgradeType type)
        {
            switch (type)
            {
                case UpgradeType.CleanBullet: return "icon_clean_bullet";
                case UpgradeType.OrbitBarrier: return "icon_orbit_barrier";
                case UpgradeType.LaserBeam: return "icon_laser_beam";
                case UpgradeType.FirewallMine: return "icon_firewall_mine";
                case UpgradeType.ShockAura: return "icon_shock_aura";
                case UpgradeType.DetoxMissile: return "icon_detox_missile";
                case UpgradeType.AlgorithmVortex: return "icon_algorithm_vortex";
                case UpgradeType.SanitizerMist: return "icon_sanitizer_mist";
                case UpgradeType.CyberScythe: return "icon_cyber_scythe";

                case UpgradeType.AttackSpeed: return "icon_attack_speed";
                case UpgradeType.MaxHP: return "icon_max_hp";
                case UpgradeType.DamageBoost: return "icon_damage_boost";
                case UpgradeType.MoveSpeed: return "icon_move_speed";
                case UpgradeType.Magnet: return "icon_magnet";
                case UpgradeType.CritChance: return "icon_crit_chance";

                case UpgradeType.EvoGigaGatling: return "icon_evo_giga_gatling";
                case UpgradeType.EvoGalacticCleanse: return "icon_evo_galactic_cleanse";
                case UpgradeType.EvoPrismCannon: return "icon_evo_prism_cannon";
                case UpgradeType.EvoSecurityMatrix: return "icon_evo_security_matrix";
                case UpgradeType.EvoTeslaDischarge: return "icon_evo_tesla_discharge";
                case UpgradeType.EvoAegisBarrage: return "icon_evo_aegis_barrage";
                case UpgradeType.EvoDetoxTyphoon: return "icon_evo_detox_typhoon";
                case UpgradeType.EvoPlasmaCloud: return "icon_evo_plasma_cloud";
                case UpgradeType.EvoShutdownBlade: return "icon_evo_shutdown_blade";

                case UpgradeType.BonusGold: return "icon_bonus_gold";
                case UpgradeType.BonusHeal: return "icon_bonus_heal";
                case UpgradeType.BonusBomb: return "icon_bonus_bomb";
                default: return "icon_clean_bullet";
            }
        }

        public void Setup(UpgradeCardInfo info, Action onSelect)
        {
            // 1. 카드 카테고리별 테마 적용 (색상, 테두리, 뱃지)
            ApplyCardTheme(info);

            // 2. 도트 아이콘 이미지 및 타이틀
            if (iconImage == null)
            {
                Transform plate = iconText != null ? iconText.transform.parent : transform.Find("IconPlate");
                if (plate != null)
                {
                    Transform existing = plate.Find("IconImage");
                    if (existing != null)
                    {
                        iconImage = existing.GetComponent<Image>();
                    }
                    else
                    {
                        GameObject imgObj = new GameObject("IconImage");
                        imgObj.transform.SetParent(plate, false);
                        RectTransform rt = imgObj.AddComponent<RectTransform>();
                        rt.sizeDelta = new Vector2(60, 60);
                        rt.anchoredPosition = Vector2.zero;
                        iconImage = imgObj.AddComponent<Image>();
                        iconImage.preserveAspect = true;
                    }
                }
            }

            Sprite dotSprite = info.iconSprite != null ? info.iconSprite : GetUpgradeSprite(info.type);

            if (iconImage != null)
            {
                if (dotSprite != null)
                {
                    iconImage.sprite = dotSprite;
                    iconImage.color = Color.white;
                    iconImage.preserveAspect = true;
                    iconImage.gameObject.SetActive(true);
                    if (iconText != null) iconText.gameObject.SetActive(false);
                }
                else
                {
                    iconImage.gameObject.SetActive(false);
                    if (iconText != null)
                    {
                        iconText.gameObject.SetActive(true);
                        iconText.text = string.IsNullOrEmpty(info.iconText) ? "✨" : info.iconText;
                        iconText.color = Color.white;
                    }
                }
            }
            else if (iconText != null)
            {
                iconText.gameObject.SetActive(true);
                iconText.text = string.IsNullOrEmpty(info.iconText) ? "✨" : info.iconText;
                iconText.color = Color.white;
            }

            if (titleText != null)
            {
                titleText.text = info.title;
                titleText.color = info.isEvolution ? new Color(1f, 0.92f, 0.45f) : Color.white;
            }

            // 3. 진화 족보 가이드 포맷팅 (초고대비 고시인성 컬러 적용)
            string rawGuide = WeaponManager.Instance != null ? WeaponManager.Instance.GetEvolutionRecipeGuide(info.type) : "";
            string formattedGuide = FormatRecipeGuide(rawGuide, info);

            bool hasGuide = !string.IsNullOrEmpty(rawGuide);

            if (recipeBox != null)
            {
                recipeBox.SetActive(hasGuide);
            }

            if (recipeGuideText != null)
            {
                recipeGuideText.text = formattedGuide;
                recipeGuideText.gameObject.SetActive(hasGuide);
                if (descriptionText != null)
                {
                    descriptionText.text = info.description;
                }
            }
            else
            {
                // 별도 족보 텍스트가 없는 경우 설명란 하단에 고대비 폰트로 추가
                if (descriptionText != null)
                {
                    descriptionText.text = hasGuide
                        ? $"{info.description}\n\n{formattedGuide}"
                        : info.description;
                }
            }

            // 레거시 뱃지 호환
            if (evoBadge != null)
            {
                evoBadge.SetActive(info.isEvolution);
            }

            // 4. 선택 버튼 리스너 바인딩
            if (selectButton == null)
            {
                selectButton = GetComponent<Button>() ?? GetComponentInChildren<Button>();
            }

            if (selectButton != null)
            {
                selectButton.onClick.RemoveAllListeners();
                selectButton.onClick.AddListener(() =>
                {
                    Core.SoundManager.Instance?.Play(Core.SoundEffect.LevelUp, 0.65f);
                    onSelect?.Invoke();
                });
            }
        }

        private void ApplyCardTheme(UpgradeCardInfo info)
        {
            if (cardBackground != null)
            {
                cardBackground.color = new Color(0.11f, 0.10f, 0.17f, 0.98f);
            }

            Color themeColor;
            Color badgeBgColor;
            string badgeText;
            string btnText;

            if (info.isEvolution || (info.type >= UpgradeType.EvoGigaGatling && info.type <= UpgradeType.EvoShutdownBlade))
            {
                // 👑 초월 진화
                themeColor = new Color(1f, 0.84f, 0.15f, 1f); // Radiant Gold
                badgeBgColor = new Color(0.38f, 0.28f, 0.05f, 0.95f);
                badgeText = "👑 초월 진화";
                btnText = "초월 각성";
            }
            else if (info.type >= UpgradeType.CleanBullet && info.type <= UpgradeType.CyberScythe)
            {
                // ⚔️ 공격 무기
                themeColor = new Color(0.68f, 0.42f, 1f, 1f); // Electric Purple
                badgeBgColor = new Color(0.24f, 0.14f, 0.40f, 0.95f);
                badgeText = "⚔️ 공격 무기";
                btnText = "무기 강화";
            }
            else if (info.type >= UpgradeType.AttackSpeed && info.type <= UpgradeType.CritChance)
            {
                // 🛡️ 패시브 보조 능력
                themeColor = new Color(0.0f, 0.80f, 1.0f, 1f); // Neon Cyan
                badgeBgColor = new Color(0.06f, 0.22f, 0.35f, 0.95f);
                badgeText = "🛡️ 패시브 보조";
                btnText = "능력 강화";
            }
            else
            {
                // 🎁 만렙 보너스 등
                themeColor = new Color(0.0f, 0.92f, 0.48f, 1f); // Emerald Green
                badgeBgColor = new Color(0.08f, 0.28f, 0.18f, 0.95f);
                badgeText = "🎁 특별 보너스";
                btnText = "보너스 획득";
            }

            if (cardOutline != null)
            {
                cardOutline.effectColor = themeColor;
            }

            if (typeBadgeBg != null)
            {
                typeBadgeBg.color = badgeBgColor;
            }

            if (typeBadgeText != null)
            {
                typeBadgeText.text = badgeText;
                typeBadgeText.color = themeColor;
            }

            if (selectButtonBg != null)
            {
                selectButtonBg.color = themeColor * 0.82f;
            }

            if (selectButtonText != null)
            {
                selectButtonText.text = btnText;
            }
        }

        private string FormatRecipeGuide(string rawGuide, UpgradeCardInfo info)
        {
            if (string.IsNullOrEmpty(rawGuide)) return "";

            // 미보유 항목인 경우 눈에 띄는 선명한 황금 오렌지색(#FFAB00)으로 표시
            if (rawGuide.Contains("미보유"))
            {
                string highlighted = rawGuide.Replace("❌미보유", "<color=#FF5252><b>❌미보유</b></color>");
                return $"<color=#FFB74D><b>{highlighted}</b></color>";
            }

            // 보유중인 경우 즉시 진화 가능한 산뜻한 네온 라임그린(#00E676)
            if (rawGuide.Contains("보유중"))
            {
                string highlighted = rawGuide.Replace("✅보유중", "<color=#00E676><b>✅보유중 (진화 준비!)</b></color>");
                return $"<color=#69F0AE><b>{highlighted}</b></color>";
            }

            // 패시브 진화 촉매 가이드인 경우 청명한 스카이블루(#38BDF8)
            if (rawGuide.Contains("촉매"))
            {
                return $"<color=#38BDF8><b>{rawGuide}</b></color>";
            }

            // 초월 진화인 경우 골드(#FFD54F)
            if (rawGuide.Contains("궁극") || info.isEvolution)
            {
                return $"<color=#FFE082><b>{rawGuide}</b></color>";
            }

            return $"<color=#E2E8F0><b>{rawGuide}</b></color>";
        }
    }
}
