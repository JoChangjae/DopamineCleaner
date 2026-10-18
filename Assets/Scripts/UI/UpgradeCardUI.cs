using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DopamineCleaner.Weapons;

namespace DopamineCleaner.UI
{
    public class UpgradeCardUI : MonoBehaviour
    {
        [Header("UI 요소")]
        [SerializeField] private TextMeshProUGUI iconText;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI descriptionText;
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
