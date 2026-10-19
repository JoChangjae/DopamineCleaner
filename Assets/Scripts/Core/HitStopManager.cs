using System.Collections;
using UnityEngine;

namespace DopamineCleaner.Core
{
    public class HitStopManager : MonoBehaviour
    {
        public static HitStopManager Instance { get; private set; }

        private Coroutine hitStopRoutine;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public void TriggerHitStop(float duration = 0.05f)
        {
            if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing) return;

            if (hitStopRoutine != null) StopCoroutine(hitStopRoutine);
            if (gameObject.activeInHierarchy)
            {
                hitStopRoutine = StartCoroutine(HitStopRoutine(duration));
            }
        }

        private IEnumerator HitStopRoutine(float duration)
        {
            Time.timeScale = 0.05f;
            yield return new WaitForSecondsRealtime(duration);
            if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Playing)
            {
                Time.timeScale = 1.0f;
            }
            hitStopRoutine = null;
        }
    }
}
