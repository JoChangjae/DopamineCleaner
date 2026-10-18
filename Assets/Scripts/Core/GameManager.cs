using System;
using UnityEngine;

namespace DopamineCleaner.Core
{
    public enum GameState
    {
        Ready,
        Playing,
        Paused,
        LevelUp,
        GameOver
    }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("게임 진행 상태")]
        [SerializeField] private GameState currentState = GameState.Ready;
        public GameState CurrentState => currentState;

        [Header("스테이지 및 진행도")]
        [SerializeField] private int currentStage = 1;
        [SerializeField] private int stageTargetKills = 35;
        [SerializeField] private int stageKills = 0;
        [SerializeField] private int totalKills = 0;
        [SerializeField] private float survivalTime = 0f;
        [SerializeField] private bool bossActive = false;

        public int CurrentStage => currentStage;
        public int StageTargetKills => stageTargetKills;
        public int StageKills => stageKills;
        public int TotalKills => totalKills;
        public float SurvivalTime => survivalTime;
        public bool IsBossActive => bossActive;

        // 이벤트 채널
        public event Action<GameState> OnStateChanged;
        public event Action<int, int> OnKillsUpdated; // currentStageKills, totalKills
        public event Action<int> OnStageChanged;
        public event Action OnBossSpawned;
        public event Action OnBossDefeated;

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
            StartNewGame();
        }

        private void Update()
        {
            if (currentState == GameState.Playing)
            {
                survivalTime += Time.deltaTime;
            }
        }

        public void StartNewGame()
        {
            currentStage = 1;
            stageTargetKills = 35;
            stageKills = 0;
            totalKills = 0;
            survivalTime = 0f;
            bossActive = false;

            Time.timeScale = 1f;
            SetState(GameState.Playing);
            OnKillsUpdated?.Invoke(stageKills, totalKills);
            OnStageChanged?.Invoke(currentStage);
        }

        public void SetState(GameState newState)
        {
            currentState = newState;
            switch (newState)
            {
                case GameState.Playing:
                    Time.timeScale = 1f;
                    break;
                case GameState.Paused:
                case GameState.LevelUp:
                case GameState.GameOver:
                    Time.timeScale = 0f;
                    break;
            }
            OnStateChanged?.Invoke(newState);
        }

        public void RegisterKill(bool isBoss)
        {
            totalKills++;

            if (isBoss)
            {
                bossActive = false;
                OnBossDefeated?.Invoke();
                AdvanceStage();
            }
            else
            {
                if (!bossActive)
                {
                    stageKills++;
                    if (stageKills >= stageTargetKills)
                    {
                        bossActive = true;
                        OnBossSpawned?.Invoke();
                    }
                }
            }

            OnKillsUpdated?.Invoke(stageKills, totalKills);
        }

        private void AdvanceStage()
        {
            currentStage++;
            stageKills = 0;
            stageTargetKills = 35 + (currentStage - 1) * 15;
            OnStageChanged?.Invoke(currentStage);
        }

        public void TriggerGameOver()
        {
            SetState(GameState.GameOver);
        }
    }
}
