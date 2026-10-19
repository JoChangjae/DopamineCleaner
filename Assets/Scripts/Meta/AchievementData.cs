using System;
using System.Collections.Generic;
using UnityEngine;
using DopamineCleaner.Core;

namespace DopamineCleaner.Meta
{
    public enum AchievementRewardType
    {
        Gold,
        Karma,
        Both
    }

    [Serializable]
    public class AchievementInfo
    {
        public string id;
        public string title;
        public string description;
        public string icon;
        public AchievementRewardType rewardType;
        public int goldReward;
        public int karmaReward;

        public bool IsUnlocked => GameDataManager.Instance != null && 
                                  GameDataManager.Instance.Profile.unlockedAchievements.Contains(id);
    }

    public static class AchievementDatabase
    {
        private static readonly List<AchievementInfo> allAchievements = new()
        {
            new AchievementInfo
            {
                id = "ach_first_clean",
                title = "🌱 첫 소독의 첫걸음",
                description = "잡생각 몬스터를 처음으로 1마리 소독했습니다.",
                icon = "🧹",
                rewardType = AchievementRewardType.Gold,
                goldReward = 150,
                karmaReward = 0
            },
            new AchievementInfo
            {
                id = "ach_hundred_kills",
                title = "🎯 잡생각 정리 전문가",
                description = "누적 몬스터 100마리를 소독 완료했습니다.",
                icon = "🎯",
                rewardType = AchievementRewardType.Gold,
                goldReward = 350,
                karmaReward = 0
            },
            new AchievementInfo
            {
                id = "ach_thousand_kills",
                title = "👑 디지털 도파민 청소부",
                description = "누적 몬스터 1,000마리를 소독하여 뇌 건강을 되찾았습니다.",
                icon = "✨",
                rewardType = AchievementRewardType.Both,
                goldReward = 1000,
                karmaReward = 2
            },
            new AchievementInfo
            {
                id = "ach_first_fever",
                title = "🔥 초집중의 시작",
                description = "초집중 디톡스 피버 모드를 1회 발동시켰습니다.",
                icon = "⚡",
                rewardType = AchievementRewardType.Gold,
                goldReward = 250,
                karmaReward = 0
            },
            new AchievementInfo
            {
                id = "ach_fever_master",
                title = "💥 피버 폭주 마니아",
                description = "누적 10회의 디톡스 피버 모드를 발동했습니다.",
                icon = "🔥",
                rewardType = AchievementRewardType.Both,
                goldReward = 800,
                karmaReward = 1
            },
            new AchievementInfo
            {
                id = "ach_boss_slayer",
                title = "👾 알고리즘의 지배자",
                description = "도파민 거대 보스를 격퇴하고 확정 황금 상자를 획득했습니다.",
                icon = "👑",
                rewardType = AchievementRewardType.Both,
                goldReward = 600,
                karmaReward = 1
            },
            new AchievementInfo
            {
                id = "ach_first_evo",
                title = "🌈 진화의 미학",
                description = "무기 1종을 최종 진화 형태로 조합 완성했습니다.",
                icon = "🔮",
                rewardType = AchievementRewardType.Gold,
                goldReward = 400,
                karmaReward = 0
            },
            new AchievementInfo
            {
                id = "ach_triple_evo",
                title = "⚔️ 궁극의 도파민 아스날",
                description = "한 출동에서 3종 이상의 무기를 동시에 최종 진화시켰습니다.",
                icon = "⚔️",
                rewardType = AchievementRewardType.Both,
                goldReward = 1200,
                karmaReward = 2
            },
            new AchievementInfo
            {
                id = "ach_sanctuary_rest",
                title = "⛲ 오아시스의 휴식",
                description = "멘탈 쉼터(디톡스 분수대)에 진입하여 도파민 과부하를 정화했습니다.",
                icon = "⛲",
                rewardType = AchievementRewardType.Gold,
                goldReward = 200,
                karmaReward = 0
            },
            new AchievementInfo
            {
                id = "ach_lucky_slot",
                title = "🎰 럭키 잭팟!",
                description = "도파민 럭키 자판기에서 대박 슬롯을 터뜨렸습니다.",
                icon = "🎰",
                rewardType = AchievementRewardType.Gold,
                goldReward = 777,
                karmaReward = 1
            },
            new AchievementInfo
            {
                id = "ach_ultimate_q",
                title = "⚡ 필살기 전개!",
                description = "요원 고유 궁극기(Q 스킬)를 1회 이상 사용했습니다.",
                icon = "💥",
                rewardType = AchievementRewardType.Gold,
                goldReward = 200,
                karmaReward = 0
            },
            new AchievementInfo
            {
                id = "ach_all_arsenal",
                title = "🚀 전천후 클리너",
                description = "9종의 모든 무기 중 5종 이상을 활성화 장착했습니다.",
                icon = "🛸",
                rewardType = AchievementRewardType.Both,
                goldReward = 1500,
                karmaReward = 3
            }
        };

        public static List<AchievementInfo> GetAll() => allAchievements;

        public static AchievementInfo Get(string id)
        {
            return allAchievements.Find(a => a.id == id);
        }
    }

    public class AchievementManager : MonoBehaviour
    {
        public static AchievementManager Instance { get; private set; }

        public static event Action<AchievementInfo> OnAchievementUnlocked;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public bool TryUnlock(string achievementId)
        {
            if (GameDataManager.Instance == null) return false;

            var profile = GameDataManager.Instance.Profile;
            if (profile.unlockedAchievements.Contains(achievementId))
            {
                return false; // 이미 해금됨
            }

            var ach = AchievementDatabase.Get(achievementId);
            if (ach == null) return false;

            profile.unlockedAchievements.Add(achievementId);
            profile.gold += ach.goldReward;
            profile.karmaPoints += ach.karmaReward;
            GameDataManager.Instance.SaveProfile();

            SoundManager.Instance?.Play(SoundEffect.LevelUp, 0.7f);
            OnAchievementUnlocked?.Invoke(ach);
            Debug.Log($"[ACHIEVEMENT UNLOCKED] {ach.title} (+{ach.goldReward}G, +{ach.karmaReward}P)");

            return true;
        }

        // 실시간 게임 이벤트 검증 헬퍼
        public void ReportKill(int sessionKills, bool isBoss)
        {
            TryUnlock("ach_first_clean");

            if (GameDataManager.Instance != null)
            {
                GameDataManager.Instance.Profile.lifetimeKills++;
                if (isBoss) GameDataManager.Instance.Profile.lifetimeBossKills++;

                if (GameDataManager.Instance.Profile.lifetimeKills >= 100)
                    TryUnlock("ach_hundred_kills");
                if (GameDataManager.Instance.Profile.lifetimeKills >= 1000)
                    TryUnlock("ach_thousand_kills");
            }

            if (isBoss)
            {
                TryUnlock("ach_boss_slayer");
            }
        }

        public void ReportFever()
        {
            TryUnlock("ach_first_fever");
            if (GameDataManager.Instance != null)
            {
                GameDataManager.Instance.Profile.lifetimeFevers++;
                if (GameDataManager.Instance.Profile.lifetimeFevers >= 10)
                {
                    TryUnlock("ach_fever_master");
                }
            }
        }

        public void ReportEvolution(int activeEvolutionsCount)
        {
            TryUnlock("ach_first_evo");
            if (activeEvolutionsCount >= 3)
            {
                TryUnlock("ach_triple_evo");
            }
        }

        public void ReportSanctuaryHeal()
        {
            TryUnlock("ach_sanctuary_rest");
        }

        public void ReportJackpot()
        {
            TryUnlock("ach_lucky_slot");
        }

        public void ReportUltimateUsed()
        {
            TryUnlock("ach_ultimate_q");
        }
    }
}
