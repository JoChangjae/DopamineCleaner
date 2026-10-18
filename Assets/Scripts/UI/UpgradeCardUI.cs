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

        public void Setup(UpgradeCardInfo info, Action onSelect)
        {
            if (iconText != null) iconText.text = info.iconText;
            if (titleText != null) titleText.text = info.title;
            if (descriptionText != null) descriptionText.text = info.description;
            if (evoBadge != null) evoBadge.SetActive(info.isEvolution);

            if (selectButton != null)
            {
                selectButton.onClick.RemoveAllListeners();
                selectButton.onClick.AddListener(() => onSelect?.Invoke());
            }
        }
    }
}
