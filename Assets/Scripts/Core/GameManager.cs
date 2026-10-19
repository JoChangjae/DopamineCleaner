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

                float targetSec = 300f;
                if (Meta.GameDataManager.Instance != null)
                {
                    targetSec = Meta.DungeonDatabase.Get(Meta.GameDataManager.Instance.SelectedMapId).targetSurvivalSec;
                }

                if (survivalTime >= targetSec)
                {
                    TriggerGameOver(true);
                }
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
            BonusGold = 0;

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
            FeverManager.Instance?.AddKill(isBoss);

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

        public bool IsVictory { get; private set; } = false;
        public int EarnedGold { get; private set; } = 0;
        public int EarnedKarma { get; private set; } = 0;
        public int BonusGold { get; private set; } = 0;

        public void AddBonusGold(int amount)
        {
            BonusGold += amount;
            EarnedGold += amount;
        }

        public void TriggerGameOver(bool victory = false)
        {
            if (currentState == GameState.GameOver) return;

            IsVictory = victory;
            CalculateSettlement(victory);
            SetState(GameState.GameOver);
        }

        private void CalculateSettlement(bool victory)
        {
            float targetSec = 300f;
            int baseGold = 600;
            int baseKarma = 1;
            float goldMult = 1.0f;
            float karmaMult = 1.0f;

            if (Meta.GameDataManager.Instance != null)
            {
                var map = Meta.DungeonDatabase.Get(Meta.GameDataManager.Instance.SelectedMapId);
                var diff = Meta.DungeonDatabase.GetDifficultyMultiplier(Meta.GameDataManager.Instance.SelectedDifficulty);
                targetSec = map.targetSurvivalSec;
                baseGold = map.baseGoldReward;
                baseKarma = map.baseKarmaReward;
                goldMult = diff.rewardGoldMult;
                karmaMult = diff.rewardKarmaMult;

                // 유물 황금 계산기 보유 시 추가 25% 보너스
                if (Meta.GameDataManager.Instance.Profile.inventory.Contains("artifact_golden_calc"))
                {
                    goldMult *= 1.25f;
                }
            }

            int killBonusGold = totalKills * 3;
            int survivalBonusGold = Mathf.FloorToInt(survivalTime * 1.5f);

            if (victory)
            {
                EarnedGold = Mathf.RoundToInt((baseGold + killBonusGold + survivalBonusGold) * goldMult) + BonusGold;
                EarnedKarma = Mathf.Max(1, Mathf.RoundToInt(baseKarma * karmaMult));
            }
            else
            {
                // 정화 실패(사망) 시 기본 지원금 지급 (40%)
                EarnedGold = Mathf.RoundToInt((baseGold * 0.4f + killBonusGold + survivalBonusGold) * goldMult) + BonusGold;
                EarnedKarma = survivalTime >= (targetSec * 0.6f) ? 1 : 0;
            }

            Meta.GameDataManager.Instance?.ApplyDungeonSettlement(EarnedGold, EarnedKarma, null);
        }
    }
}
