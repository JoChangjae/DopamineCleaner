using System;
using UnityEngine;
using UnityEngine.UI;
using DopamineCleaner.Characters;
using DopamineCleaner.Enemies;
using DopamineCleaner.Items;

namespace DopamineCleaner.Core
{
    public enum QuestType
    {
        RapidKills,     // 급속 킬 퀘스트
        EliteHunt,      // 쇼츠 러너 처치 퀘스트
        FeverTimeKeep   // 피버 돌입 퀘스트
    }

    public class EmergencyQuestManager : MonoBehaviour
    {
        public static EmergencyQuestManager Instance { get; private set; }

        [Header("퀘스트 상태")]
        [SerializeField] private bool isQuestActive = false;
        [SerializeField] private QuestType currentType;
        [SerializeField] private float timeRemaining = 0f;
        [SerializeField] private int targetProgress = 0;
        [SerializeField] private int currentProgress = 0;
        [SerializeField] private string questDescription = "";

        [Header("퀘스트 발동 스케줄 (초)")]
        private readonly float[] triggerTimes = { 65f, 155f, 245f };
        private int nextTriggerIndex = 0;

        [Header("UI 연결 (옵션)")]
        [SerializeField] private GameObject questBanner;
        [SerializeField] private Text questText;

        public bool IsQuestActive => isQuestActive;

        public event Action<string, float, int, int> OnQuestUpdated; // desc, time, cur, target
        public event Action<bool, string> OnQuestEnded;              // success, msg

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
            if (questBanner != null) questBanner.SetActive(false);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnKillsUpdated += OnKillReported;
            }
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnKillsUpdated -= OnKillReported;
            }
        }

        private void Update()
        {
            if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing)
                return;

            float survivalTime = GameManager.Instance.SurvivalTime;

            // 1. 스케줄된 긴급 퀘스트 발동 체크
            if (!isQuestActive && nextTriggerIndex < triggerTimes.Length)
            {
                if (survivalTime >= triggerTimes[nextTriggerIndex])
                {
                    StartEmergencyQuest(nextTriggerIndex);
                    nextTriggerIndex++;
                }
            }

            // 2. 진행 중인 퀘스트 타이머
            if (isQuestActive)
            {
                timeRemaining -= Time.deltaTime;
                UpdateUI();

                if (currentProgress >= targetProgress)
                {
                    CompleteQuest(true);
                }
                else if (timeRemaining <= 0f)
                {
                    CompleteQuest(false);
                }
            }
        }

        public void BindUI(GameObject banner, Text text)
        {
            questBanner = banner;
            questText = text;
            if (questBanner != null) questBanner.SetActive(isQuestActive);
        }

        private void StartEmergencyQuest(int index)
        {
            isQuestActive = true;
            currentProgress = 0;

            switch (index % 3)
            {
                case 0:
                    currentType = QuestType.RapidKills;
                    timeRemaining = 35f;
                    targetProgress = 22;
                    questDescription = "⚡ [긴급 정화] 35초 내에 잡생각 22마리 급속 소독!";
                    break;
                case 1:
                    currentType = QuestType.EliteHunt;
                    timeRemaining = 35f;
                    targetProgress = 6;
                    questDescription = "🎯 [알고리즘 차단] 35초 내에 숏폼 러너 6마리 퇴치!";
                    break;
                default:
                    currentType = QuestType.RapidKills;
                    timeRemaining = 40f;
                    targetProgress = 35;
                    questDescription = "🔥 [도파민 과부하 방어] 40초 내에 몬스터 35마리 격퇴!";
                    break;
            }

            SoundManager.Instance?.Play(SoundEffect.Fever, 0.7f);
            CameraFollow.Instance?.Shake(0.25f, 0.3f);

            if (questBanner != null) questBanner.SetActive(true);
            UpdateUI();
        }

        public void ReportEnemyKilled(EnemyType type)
        {
            if (!isQuestActive) return;

            if (currentType == QuestType.RapidKills)
            {
                currentProgress++;
            }
            else if (currentType == QuestType.EliteHunt && type == EnemyType.ShortsRunner)
            {
                currentProgress++;
            }

            UpdateUI();
        }

        private void OnKillReported(int stageKills, int totalKills)
        {
            // 범용 킬 이벤트 리스너
            if (isQuestActive && currentType == QuestType.RapidKills)
            {
                // ReportEnemyKilled가 직접 호출되지 않은 경우 대비
            }
        }

        private void UpdateUI()
        {
            int sec = Mathf.Max(0, Mathf.CeilToInt(timeRemaining));
            string statusStr = $"⚠️ {questDescription}  ({currentProgress}/{targetProgress})  ⏳ {sec}s";

            if (questText != null)
            {
                questText.text = statusStr;
            }

            OnQuestUpdated?.Invoke(questDescription, timeRemaining, currentProgress, targetProgress);
        }

        private void CompleteQuest(bool success)
        {
            isQuestActive = false;

            if (success)
            {
                SoundManager.Instance?.Play(SoundEffect.LevelUp, 0.8f);
                CameraFollow.Instance?.Shake(0.4f, 0.45f);

                // 1. 황금 보물상자 즉시 투하
                if (PlayerController.Instance != null && ItemManager.Instance != null)
                {
                    Vector3 dropPos = PlayerController.Instance.transform.position + Vector3.up * 1.5f;
                    ItemManager.Instance.SpawnGem(dropPos, GemType.Chest, 0);
                }

                // 2. 즉시 디톡스 피버 발동
                FeverManager.Instance?.ActivateFever();

                if (questText != null)
                {
                    questText.text = "🎉 [작전 대성공!] 황금 상자 보급 및 디톡스 피버 발동!";
                    questText.color = new Color(1f, 0.85f, 0.3f);
                }
            }
            else
            {
                if (questText != null)
                {
                    questText.text = "⚠️ [작전 시간 초과] 긴급 돌발 미션이 종료되었습니다.";
                    questText.color = new Color(0.8f, 0.8f, 0.8f);
                }
            }

            OnQuestEnded?.Invoke(success, success ? "작전 성공!" : "시간 초과");

            // 3초 후 배너 숨김
            CancelInvoke(nameof(HideBanner));
            Invoke(nameof(HideBanner), 3.0f);
        }

        private void HideBanner()
        {
            if (questBanner != null && !isQuestActive)
            {
                questBanner.SetActive(false);
            }
        }
    }
}
