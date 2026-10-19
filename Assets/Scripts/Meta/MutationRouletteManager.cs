using System;
using System.Collections.Generic;
using UnityEngine;

namespace DopamineCleaner.Meta
{
    public enum MutationType
    {
        None,             // 표준 모드
        Overheated,       // 과열 알고리즘 (적 이속 +25%, EXP +40%)
        GlassCannon,      // 유리 대포 (플레이어 공격력 +50%, 최대 체력 -30%)
        GoldJackpot,      // 도파민 잭팟 (골드 드랍 +100%, 엘리트 확률 2배)
        HyperCaffeine,    // 무중력 카페인 (이속 +30%, 대시 쿨타임 -40%)
        StormSurge        // 폭풍전야 (스폰 주기 30% 단축, 황금상자 드랍률 3배)
    }

    [Serializable]
    public class MutationDefinition
    {
        public MutationType type;
        public string title;
        public string icon;
        public string benefit;
        public string penalty;
        public Color themeColor;

        public float playerDamageMult = 1.0f;
        public float playerHpMult = 1.0f;
        public float playerSpeedMult = 1.0f;
        public float dashCooldownMult = 1.0f;
        public float enemySpeedMult = 1.0f;
        public float expMult = 1.0f;
        public float goldMult = 1.0f;
        public float spawnIntervalMult = 1.0f;
        public float chestRateMult = 1.0f;
        public float eliteRateMult = 1.0f;
    }

    public class MutationRouletteManager : MonoBehaviour
    {
        private static MutationRouletteManager instance;
        public static MutationRouletteManager Instance
        {
            get
            {
                if (instance == null) instance = FindAnyObjectByType<MutationRouletteManager>();
                return instance;
            }
            private set => instance = value;
        }

        public MutationDefinition CurrentMutation { get; private set; }

        private readonly List<MutationDefinition> mutations = new()
        {
            new MutationDefinition
            {
                type = MutationType.None,
                title = "안정적 표준 모드",
                icon = "🛡️",
                benefit = "특이 변이 없음",
                penalty = "기본 난이도 유지",
                themeColor = new Color(0.7f, 0.8f, 0.9f)
            },
            new MutationDefinition
            {
                type = MutationType.Overheated,
                title = "과열 알고리즘",
                icon = "🔥",
                benefit = "경험치(EXP) 획득량 +40%",
                penalty = "적 이동속도 +25%",
                themeColor = new Color(1f, 0.45f, 0.2f),
                expMult = 1.4f,
                enemySpeedMult = 1.25f
            },
            new MutationDefinition
            {
                type = MutationType.GlassCannon,
                title = "유리 대포",
                icon = "⚡",
                benefit = "플레이어 공격력 +50%",
                penalty = "플레이어 최대 체력 -30%",
                themeColor = new Color(1f, 0.25f, 0.35f),
                playerDamageMult = 1.5f,
                playerHpMult = 0.7f
            },
            new MutationDefinition
            {
                type = MutationType.GoldJackpot,
                title = "도파민 잭팟",
                icon = "🎰",
                benefit = "정산 골드 획득량 +100%",
                penalty = "엘리트 변종 출현 확률 2배",
                themeColor = new Color(1f, 0.85f, 0.15f),
                goldMult = 2.0f,
                eliteRateMult = 2.0f
            },
            new MutationDefinition
            {
                type = MutationType.HyperCaffeine,
                title = "무중력 카페인",
                icon = "☕",
                benefit = "플레이어 이동속도 +30%, 대시 쿨타임 -40%",
                penalty = "집중력 과민 상태",
                themeColor = new Color(0.3f, 0.95f, 0.85f),
                playerSpeedMult = 1.3f,
                dashCooldownMult = 0.6f
            },
            new MutationDefinition
            {
                type = MutationType.StormSurge,
                title = "폭풍전야",
                icon = "🌪️",
                benefit = "황금 보물상자 드랍률 3배",
                penalty = "몬스터 스폰 주기 30% 가속",
                themeColor = new Color(0.85f, 0.4f, 1f),
                chestRateMult = 3.0f,
                spawnIntervalMult = 0.7f
            }
        };

        private void Awake()
        {
            if (instance == null || instance == this)
            {
                instance = this;
                if (Application.isPlaying) DontDestroyOnLoad(gameObject);
                CurrentMutation = mutations[0]; // 기본 None
            }
            else
            {
                if (Application.isPlaying) Destroy(gameObject);
                else DestroyImmediate(gameObject);
            }
        }

        /// <summary>
        /// 무작위 돌연변이 룰렛 추첨
        /// </summary>
        public MutationDefinition SpinRoulette()
        {
            int index = UnityEngine.Random.Range(0, mutations.Count);
            CurrentMutation = mutations[index];
            return CurrentMutation;
        }

        public void SetMutation(MutationType type)
        {
            CurrentMutation = mutations.Find(m => m.type == type) ?? mutations[0];
        }

        public List<MutationDefinition> GetAllMutations() => mutations;
    }
}
