using System;
using UnityEngine;
using DopamineCleaner.Characters;
using DopamineCleaner.Weapons;

namespace DopamineCleaner.Core
{
    public class FeverManager : MonoBehaviour
    {
        public static FeverManager Instance { get; private set; }

        [Header("피버 게이지 설정")]
        [SerializeField] private float maxFever = 100f;
        [SerializeField] private float currentFever = 0f;
        [SerializeField] private float feverDuration = 8.0f;
        [SerializeField] private bool isFever = false;

        [Header("콤보 시스템")]
        [SerializeField] private int comboCount = 0;
        [SerializeField] private float comboTimeout = 2.2f;
        private float comboTimer = 0f;

        private float feverTimer = 0f;

        public float CurrentFever => currentFever;
        public float MaxFever => maxFever;
        public bool IsFever => isFever;
        public bool IsOverdrive => isFever && comboCount >= 25;
        public int ComboCount => comboCount;
        public float FeverRatio => maxFever > 0f ? Mathf.Clamp01(currentFever / maxFever) : 0f;

        public event Action<float, float> OnFeverGaugeChanged; // current, max
        public event Action<bool> OnFeverStateChanged;         // isFever
        public event Action<int> OnComboChanged;              // comboCount

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            currentFever = 0f;
            isFever = false;
            comboCount = 0;
            OnFeverGaugeChanged?.Invoke(currentFever, maxFever);
        }

        private void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing)
                return;

            // 콤보 타이머 감소
            if (comboCount > 0)
            {
                comboTimer -= Time.deltaTime;
                if (comboTimer <= 0f)
                {
                    comboCount = 0;
                    OnComboChanged?.Invoke(comboCount);
                }
            }

            // 피버 모드 진행
            if (isFever)
            {
                feverTimer -= Time.deltaTime;
                currentFever = Mathf.Max(0f, (feverTimer / feverDuration) * maxFever);
                OnFeverGaugeChanged?.Invoke(currentFever, maxFever);

                if (feverTimer <= 0f)
                {
                    EndFever();
                }
            }
        }

        public void AddKill(bool isBoss)
        {
            if (isFever) return; // 피버 도중에는 재충전되지 않음

            // 콤보 갱신
            comboCount++;
            comboTimer = comboTimeout;
            OnComboChanged?.Invoke(comboCount);

            // 피버 게이지 충전 (콤보가 높을수록 추가 획득)
            float baseGain = isBoss ? 35f : 2.5f;
            float comboBonus = Mathf.Min(comboCount * 0.15f, 3f);
            float totalGain = baseGain + comboBonus;

            currentFever = Mathf.Min(maxFever, currentFever + totalGain);
            OnFeverGaugeChanged?.Invoke(currentFever, maxFever);

            if (currentFever >= maxFever)
            {
                ActivateFever();
            }
        }

        public void AddFever(float amount)
        {
            if (isFever) return;
            currentFever = Mathf.Min(maxFever, currentFever + amount);
            OnFeverGaugeChanged?.Invoke(currentFever, maxFever);
            if (currentFever >= maxFever)
            {
                ActivateFever();
            }
        }

        public void ActivateFever()
        {
            if (isFever) return;

            isFever = true;
            feverTimer = feverDuration;
            currentFever = maxFever;

            OnFeverStateChanged?.Invoke(true);
            OnFeverGaugeChanged?.Invoke(currentFever, maxFever);

            // 카메라 쉐이크 & 피버 사운드
            CameraFollow.Instance?.Shake(0.35f, 0.35f);
            SoundManager.Instance?.Play(SoundEffect.Fever, 0.6f);
            SoundManager.Instance?.SetFeverPitch(true);
            Meta.AchievementManager.Instance?.ReportFever();

            // 1. 플레이어 강화 (자석 대폭 흡수)
            if (PlayerController.Instance != null)
            {
                PlayerController.Instance.UpgradeMagnet(15f);
            }

            // 2. 화면 내 모든 보석 즉시 플레이어에게 견인
            Items.ItemManager.Instance?.TriggerMagnet();
        }

        private void EndFever()
        {
            if (!isFever) return;

            isFever = false;
            currentFever = 0f;
            feverTimer = 0f;

            SoundManager.Instance?.SetFeverPitch(false);
            OnFeverStateChanged?.Invoke(false);
            OnFeverGaugeChanged?.Invoke(currentFever, maxFever);

            // 자석 범위 원복
            if (PlayerController.Instance != null)
            {
                PlayerController.Instance.UpgradeMagnet(-15f);
            }
        }
    }
}
