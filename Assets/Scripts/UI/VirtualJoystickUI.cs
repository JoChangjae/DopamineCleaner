using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DopamineCleaner.Characters;

namespace DopamineCleaner.UI
{
    public class VirtualJoystickUI : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        public static VirtualJoystickUI Instance { get; private set; }
        public static Vector2 InputDirection { get; private set; } = Vector2.zero;

        [Header("조이스틱 UI 바인딩")]
        [SerializeField] private RectTransform joystickBackground;
        [SerializeField] private RectTransform joystickHandle;
        [SerializeField] private Button mobileDashButton;
        [SerializeField] private Button mobileUltimateButton;
        [SerializeField] private Text mobileDashCooldownText;
        [SerializeField] private Text mobileUltimateCooldownText;

        private float handleRange = 60f;
        private Canvas parentCanvas;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else { Destroy(gameObject); return; }

            parentCanvas = GetComponentInParent<Canvas>();
            if (mobileDashButton != null)
                mobileDashButton.onClick.AddListener(OnMobileDashClicked);
            if (mobileUltimateButton != null)
                mobileUltimateButton.onClick.AddListener(OnMobileUltimateClicked);
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
                InputDirection = Vector2.zero;
            }
        }

        private void Update()
        {
            if (PlayerController.Instance == null) return;

            // 대시 / 궁극기 모바일 버튼 쿨타임 오버레이 갱신
            if (mobileDashCooldownText != null)
            {
                mobileDashCooldownText.text = PlayerController.Instance.IsDashReady ? "" : $"{PlayerController.Instance.DashRemainingCooldown:F1}s";
            }

            if (mobileUltimateCooldownText != null)
            {
                mobileUltimateCooldownText.text = PlayerController.Instance.IsUltimateReady ? "" : $"{PlayerController.Instance.UltimateRemainingTime:F0}s";
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            OnDrag(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (joystickBackground == null || joystickHandle == null) return;

            Camera cam = parentCanvas != null && parentCanvas.renderMode == RenderMode.ScreenSpaceCamera ? parentCanvas.worldCamera : null;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(joystickBackground, eventData.position, cam, out Vector2 localPoint))
            {
                Vector2 clamped = Vector2.ClampMagnitude(localPoint, handleRange);
                joystickHandle.anchoredPosition = clamped;
                InputDirection = clamped / handleRange;
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (joystickHandle != null)
            {
                joystickHandle.anchoredPosition = Vector2.zero;
            }
            InputDirection = Vector2.zero;
        }

        private void OnMobileDashClicked()
        {
            PlayerController.Instance?.PerformDash();
        }

        private void OnMobileUltimateClicked()
        {
            PlayerController.Instance?.PerformUltimateSkill();
        }
    }
}
