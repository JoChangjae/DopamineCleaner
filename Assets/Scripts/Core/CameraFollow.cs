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

        private Vector3 velocity = Vector3.zero;

        private void Start()
        {
            if (target == null && PlayerController.Instance != null)
            {
                target = PlayerController.Instance.transform;
            }
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
            transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothSpeed);
        }
    }
}
