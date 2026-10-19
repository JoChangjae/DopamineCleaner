using System;
using UnityEngine;
using UnityEngine.UI;
using DopamineCleaner.Weapons;

namespace DopamineCleaner.UI
{
    public class UpgradeCardUI : MonoBehaviour
    {
        [Header("UI 요소 (한글 호환 표준 Text)")]
        [SerializeField] private Text iconText;
        [SerializeField] private Text titleText;
        [SerializeField] private Text descriptionText;
        [SerializeField] private GameObject evoBadge;
        [SerializeField] private Button selectButton;
        [SerializeField] private Text recipeGuideText;

        public void Setup(UpgradeCardInfo info, Action onSelect)
        {
            if (iconText != null) iconText.text = info.iconText;
            if (titleText != null) titleText.text = info.title;

            string guide = WeaponManager.Instance != null ? WeaponManager.Instance.GetEvolutionRecipeGuide(info.type) : "";

            if (recipeGuideText != null)
            {
                recipeGuideText.text = guide;
                recipeGuideText.gameObject.SetActive(!string.IsNullOrEmpty(guide));
                if (descriptionText != null) descriptionText.text = info.description;
            }
            else
            {
                if (descriptionText != null)
                {
                    descriptionText.text = string.IsNullOrEmpty(guide)
                        ? info.description
                        : $"{info.description}\n<color=#5df0c2>{guide}</color>";
                }
            }

            if (evoBadge != null) evoBadge.SetActive(info.isEvolution);

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
    }
}
