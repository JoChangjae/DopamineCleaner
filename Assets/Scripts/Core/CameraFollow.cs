using UnityEngine;
using DopamineCleaner.Characters;

namespace DopamineCleaner.Core
{
    public class CameraFollow : MonoBehaviour
    {
        [Header("추적 대상 및 부드러움")]
        [SerializeField] private Transform target;
        [SerializeField] private float smoothSpeed = 0.12f;
        [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);

        public static CameraFollow Instance { get; private set; }

        private Vector3 velocity = Vector3.zero;
        private float shakeTimer = 0f;
        private float shakeMagnitude = 0.2f;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            if (target == null && PlayerController.Instance != null)
            {
                target = PlayerController.Instance.transform;
            }
        }

        public void Shake(float duration = 0.2f, float magnitude = 0.25f)
        {
            shakeTimer = duration;
            shakeMagnitude = magnitude;
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                if (PlayerController.Instance != null)
                    target = PlayerController.Instance.transform;
                else
                    return;
            }

            Vector3 desiredPosition = target.position + offset;
            Vector3 smoothed = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothSpeed);

            if (shakeTimer > 0f)
            {
                shakeTimer -= Time.deltaTime;
                Vector3 shakeOffset = (Vector3)(Random.insideUnitCircle * shakeMagnitude);
                transform.position = smoothed + shakeOffset;
            }
            else
            {
                transform.position = smoothed;
            }
        }
    }
}
