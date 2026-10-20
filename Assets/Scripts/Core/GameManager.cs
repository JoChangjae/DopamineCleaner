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

        [Header("특수 모드 상태")]
        [SerializeField] private bool isEndlessMode = false;
        [SerializeField] private bool endlessPromptShown = false;

        public bool IsEndlessMode => isEndlessMode;
        public bool EndlessPromptShown => endlessPromptShown;
        public bool IsHyperMode => Meta.GameDataManager.Instance != null && Meta.GameDataManager.Instance.IsHyperMode;

        public event Action OnEndlessChoicePrompt;

        private void Update()
        {
            if (currentState == GameState.Playing)
            {
                survivalTime += Time.deltaTime;

                float targetSec = 300f;
                if (Meta.GameDataManager.Instance != null)
                {
                    var mapDef = Meta.DungeonDatabase.Get(Meta.GameDataManager.Instance.SelectedMapId);
                    if (mapDef != null) targetSec = mapDef.targetSurvivalSec;
                }

                if (survivalTime >= targetSec && !isEndlessMode && !endlessPromptShown)
                {
                    endlessPromptShown = true;
                    if (OnEndlessChoicePrompt != null)
                    {
                        SetState(GameState.Paused);
                        OnEndlessChoicePrompt.Invoke();
                    }
                    else
                    {
                        TriggerGameOver(true);
                    }
                }
            }
        }

        public void ChooseEndlessMode(bool goEndless)
        {
            if (goEndless)
            {
                isEndlessMode = true;
                AddBonusGold(100); // 야근 수당
                SetState(GameState.Playing);
                SoundManager.Instance?.Play(SoundEffect.Fever, 0.85f);
            }
            else
            {
                TriggerGameOver(true);
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
            isEndlessMode = false;
            endlessPromptShown = false;

            EnsureCompanionPet();
            EnsureStageHazards();

            Time.timeScale = 1f;
            SetState(GameState.Playing);
            OnKillsUpdated?.Invoke(stageKills, totalKills);
            OnStageChanged?.Invoke(currentStage);
        }

        private void EnsureCompanionPet()
        {
            if (Characters.CompanionPet.Instance == null && Characters.PlayerController.Instance != null)
            {
                GameObject petObj = new GameObject("TacticalCompanionPet");
                petObj.transform.position = Characters.PlayerController.Instance.transform.position + new Vector3(-1f, 1f, 0f);
                var pet = petObj.AddComponent<Characters.CompanionPet>();

                Characters.CompanionType cType = Characters.CompanionType.PomodoroDrone;
                if (Meta.GameDataManager.Instance != null && Meta.GameDataManager.Instance.Profile != null)
                {
                    string compId = Meta.GameDataManager.Instance.Profile.equippedCompanionId;
                    if (compId == "pet_dog") cType = Characters.CompanionType.HappyDog;
                    else if (compId == "pet_cat") cType = Characters.CompanionType.NapCat;
                    else if (compId == "pet_drone") cType = Characters.CompanionType.PomodoroDrone;
                }
                pet.Initialize(cType);
            }
        }

        private void EnsureStageHazards()
        {
            if (StageHazardManager.Instance == null)
            {
                GameObject hazObj = new GameObject("[StageHazardManager]");
                hazObj.AddComponent<StageHazardManager>();
            }
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
            Meta.AchievementManager.Instance?.ReportKill(totalKills, isBoss);

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
        public int CurrentGold => (Meta.GameDataManager.Instance?.Profile != null) ? Meta.GameDataManager.Instance.Profile.gold : BonusGold;

        public void AddBonusGold(int amount)
        {
            BonusGold += amount;
            EarnedGold += amount;
            if (Meta.GameDataManager.Instance?.Profile != null)
            {
                Meta.GameDataManager.Instance.Profile.gold += amount;
            }
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
                if (map != null)
                {
                    targetSec = map.targetSurvivalSec;
                    baseGold = map.baseGoldReward;
                    baseKarma = map.baseKarmaReward;
                }
                if (diff != null)
                {
                    goldMult = diff.rewardGoldMult;
                    karmaMult = diff.rewardKarmaMult;
                }

                // 유물 황금 계산기 보유 시 추가 25% 보너스
                var profile = Meta.GameDataManager.Instance.Profile;
                if (profile != null && profile.inventory != null && profile.inventory.Contains("artifact_golden_calc"))
                {
                    goldMult *= 1.25f;
                }

                // 하이퍼 모드 시 보상 2.0배 폭증
                if (IsHyperMode)
                {
                    goldMult *= 2.0f;
                    karmaMult *= 2.0f;
                }
            }

            int killBonusGold = totalKills * 3;
            int survivalBonusGold = Mathf.FloorToInt(survivalTime * 1.5f);

            // 철야 야근 모드: 5분 초과 생존 초당 4.5G 추가 성과급
            int overtimeGold = 0;
            if (isEndlessMode && survivalTime > targetSec)
            {
                overtimeGold = Mathf.FloorToInt((survivalTime - targetSec) * 4.5f);
                victory = true; // 야근 도전자는 사망하더라도 승리 정산
            }

            if (victory)
            {
                EarnedGold = Mathf.RoundToInt((baseGold + killBonusGold + survivalBonusGold) * goldMult) + BonusGold + overtimeGold;
                EarnedKarma = Mathf.Max(1, Mathf.RoundToInt(baseKarma * karmaMult));
            }
            else
            {
                // 정화 실패(사망) 시 기본 지원금 지급 (40%)
                EarnedGold = Mathf.RoundToInt((baseGold * 0.4f + killBonusGold + survivalBonusGold) * goldMult) + BonusGold;
                EarnedKarma = survivalTime >= (targetSec * 0.6f) ? 1 : 0;
            }

            Meta.GameDataManager.Instance?.ApplyDungeonSettlement(EarnedGold, EarnedKarma, null);
            Meta.GameDataManager.Instance?.AddCombatRecord(new Meta.CombatRecord
            {
                characterId = Meta.GameDataManager.Instance?.Profile?.selectedCharacterId ?? "char_intern",
                survivalSeconds = Mathf.FloorToInt(survivalTime),
                kills = totalKills,
                goldEarned = EarnedGold,
                karmaEarned = EarnedKarma,
                isCleared = victory,
                timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm")
            });
        }
    }
}
