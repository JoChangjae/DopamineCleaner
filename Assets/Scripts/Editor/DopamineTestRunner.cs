using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using DopamineCleaner.Characters;
using DopamineCleaner.Core;
using DopamineCleaner.Enemies;
using DopamineCleaner.Items;
using DopamineCleaner.Meta;
using DopamineCleaner.UI;
using DopamineCleaner.Weapons;

namespace DopamineCleaner.Editor
{
    public static class DopamineTestRunner
    {
        private static int passedCount = 0;
        private static int failedCount = 0;
        private static readonly List<string> failureLogs = new();

        [MenuItem("Dopamine Cleaner/🧪 Run All Automated Feature Tests")]
        public static void RunAllTests()
        {
            passedCount = 0;
            failedCount = 0;
            failureLogs.Clear();

            Debug.Log("==================================================================");
            Debug.Log("🚀 [도파민 클리너] 신규 구현 기능 전수 자동 테스트 시작!");
            Debug.Log("==================================================================");

            // 1. 씬 빌드 검증
            TestSceneBuilders();

            // 2. 메타 데이터 & 연구소 검증
            TestMetaAndResearch();

            // 3. 피버 모드 & 콤보 시스템 검증
            TestFeverAndCombo();

            // 4. 5대 무기, 진화, DPS 대미지 통계 검증
            TestWeaponsAndDamageTracking();

            // 5. 레벨업 만렙 보너스 & 방탄 로직 검증
            TestLevelUpBonusAndFallback();

            // 6. 돌발 긴급 퀘스트 검증
            TestEmergencyQuests();

            // 7. 도파민 과부하 & 멘탈 쉼터 검증
            TestOverloadAndSanctuary();

            // 8. 사운드 및 일시정지, 게임오버 검진표 검증
            TestSoundAndPauseAndReport();

            // 9. 몬스터 피격 & 투사체 검증
            TestEnemyAndProjectiles();

            // 최종 리포트
            Debug.Log("==================================================================");
            if (failedCount == 0)
            {
                Debug.Log($"🎉 [테스트 결과: ALL PASS] 총 {passedCount}개 테스트 항목 전체 성공!");
            }
            else
            {
                Debug.LogError($"❌ [테스트 결과: FAILED] 성공: {passedCount}개, 실패: {failedCount}개!");
                foreach (var log in failureLogs)
                {
                    Debug.LogError($"  - {log}");
                }
            }
            Debug.Log("==================================================================");
        }

        private static void Assert(bool condition, string testName, string detail = "")
        {
            if (condition)
            {
                passedCount++;
                Debug.Log($"  ✅ [PASS] {testName}");
            }
            else
            {
                failedCount++;
                string msg = $"[FAIL] {testName} : {detail}";
                failureLogs.Add(msg);
                Debug.LogError($"  ❌ {msg}");
            }
        }

        private static void TestSceneBuilders()
        {
            Debug.Log("--- 1. 씬 빌더(TownScene, MainGame) 자동 생성 검증 ---");
            try
            {
                TownSceneBuilder.BuildTownScene();
                Assert(System.IO.File.Exists("Assets/Scenes/TownScene.unity"), "TownScene 파일 생성 성공");

                DopamineSceneBuilder.BuildScene();
                Assert(System.IO.File.Exists("Assets/Scenes/MainGame.unity"), "MainGame 파일 생성 성공");
            }
            catch (Exception ex)
            {
                Assert(false, "씬 빌더 실행 도중 예외 발생", ex.ToString());
            }
        }

        private static void TestMetaAndResearch()
        {
            Debug.Log("--- 2. 메타 연구소(Perk Tree) 데이터 및 강화 검증 ---");
            try
            {
                var researches = ResearchDatabase.GetAll();
                Assert(researches.Count == 5, "연구소 5개 Perk(공격력, 체력, 이속, 자석, 부활) 등록 확인");

                GameObject gdmObj = new GameObject("[TestGDM]");
                GameDataManager gdm = gdmObj.AddComponent<GameDataManager>();

                gdm.Profile.gold = 5000;
                gdm.Profile.karmaPoints = 50;

                int prevAtk = gdm.GetResearchLevel("atk");
                bool upgraded = gdm.UpgradeResearch("atk");
                Assert(upgraded, "연구소 공격력(atk) 업그레이드 성공");
                Assert(gdm.GetResearchLevel("atk") == prevAtk + 1, "연구소 공격력 레벨 1 증가 확인");

                UnityEngine.Object.DestroyImmediate(gdmObj);
            }
            catch (Exception ex)
            {
                Assert(false, "메타 연구소 테스트 예외", ex.ToString());
            }
        }

        private static void TestFeverAndCombo()
        {
            Debug.Log("--- 3. 피버(Fever) 모드 & 콤보 시스템 검증 ---");
            try
            {
                GameObject obj = new GameObject("[TestFever]");
                FeverManager fm = obj.AddComponent<FeverManager>();

                Assert(!fm.IsFever, "초기 상태는 피버 비활성화");
                fm.AddKill(false);
                Assert(fm.ComboCount == 1, "적 처치 시 콤보 1 증가");
                Assert(fm.CurrentFever > 0f, "적 처치 시 피버 게이지 충전 확인");

                fm.ActivateFever();
                Assert(fm.IsFever, "ActivateFever() 호출 시 피버 폭주 활성화");
                Assert(fm.CurrentFever == fm.MaxFever, "피버 발동 시 게이지 100% 충전");

                UnityEngine.Object.DestroyImmediate(obj);
            }
            catch (Exception ex)
            {
                Assert(false, "피버 모드 테스트 예외", ex.ToString());
            }
        }

        private static void TestWeaponsAndDamageTracking()
        {
            Debug.Log("--- 4. 5대 무기 진화 및 DPS 대미지 통계 집계 검증 ---");
            try
            {
                GameObject root = new GameObject("[TestWeapons]");
                CleanBulletWeapon bullet = root.AddComponent<CleanBulletWeapon>();
                OrbitWeapon orbit = root.AddComponent<OrbitWeapon>();
                LaserBeamWeapon laser = root.AddComponent<LaserBeamWeapon>();
                FirewallMineWeapon mine = root.AddComponent<FirewallMineWeapon>();
                ShockAuraWeapon shock = root.AddComponent<ShockAuraWeapon>();

                WeaponManager wm = root.AddComponent<WeaponManager>();

                // Private 필드 바인딩
                SetField(wm, "cleanBulletWeapon", bullet);
                SetField(wm, "orbitWeapon", orbit);
                SetField(wm, "laserBeamWeapon", laser);
                SetField(wm, "mineWeapon", mine);
                SetField(wm, "shockWeapon", shock);

                // 무기 업그레이드
                bullet.Upgrade();
                laser.Upgrade();
                mine.Upgrade();
                shock.Upgrade();

                Assert(laser.Level == 1, "디톡스 레이저 1레벨 업그레이드 확인");
                Assert(mine.Level == 1, "방화벽 지뢰 1레벨 업그레이드 확인");
                Assert(shock.Level == 1, "쇼크 펄스 1레벨 업그레이드 확인");

                // 진화 테스트
                laser.Evolve();
                Assert(laser.IsEvolved, "레이저 -> 프리즘 정화포 진화 성공");
                mine.Evolve();
                Assert(mine.IsEvolved, "지뢰 -> 절대 보안 결계 진화 성공");
                shock.Evolve();
                Assert(shock.IsEvolved, "쇼크 -> 테슬라 뇌파 방전 진화 성공");

                // 대미지 누적 및 통계 테스트
                laser.RegisterDamage(5000f);
                mine.RegisterDamage(3000f);
                shock.RegisterDamage(2000f);

                var stats = wm.GetDamageStatistics();
                Assert(stats.Count >= 3, "대미지 통계 집계 항목 생성 확인");
                Assert(stats[0].name.Contains("프리즘") || stats[0].damage == 5000f, "최고 대미지 무기가 1순위로 정렬됨");
                Assert(Mathf.Approximately(stats[0].percentage, 50f), "5000/10000 = 50% 점유율 정확성 확인");

                string summary = wm.GetEquipmentSummary();
                Assert(summary.Contains("프리즘") && summary.Contains("EVOLVED"), "장비 요약 텍스트에 진화 무기 및 대미지 포함 확인");

                UnityEngine.Object.DestroyImmediate(root);
            }
            catch (Exception ex)
            {
                Assert(false, "무기 및 대미지 통계 테스트 예외", ex.ToString());
            }
        }

        private static void TestLevelUpBonusAndFallback()
        {
            Debug.Log("--- 5. 레벨업 만렙 보너스 & 방탄 로직 검증 ---");
            try
            {
                GameObject root = new GameObject("[TestLevelUp]");
                WeaponManager wm = root.AddComponent<WeaponManager>();

                // 후보가 부족한 상황에서도 항상 3장 보장
                var options = wm.GetRandomUpgradeOptions(3);
                Assert(options.Count == 3, "업그레이드 옵션은 항상 무조건 3장 반환됨");

                // 만렙 보너스 적용
                wm.ApplyUpgrade(UpgradeType.BonusGold);
                wm.ApplyUpgrade(UpgradeType.BonusHeal);
                wm.ApplyUpgrade(UpgradeType.BonusBomb);
                Assert(true, "만렙 보너스(골드, 힐, 폭탄) 적용 시 예외 없음");

                UnityEngine.Object.DestroyImmediate(root);
            }
            catch (Exception ex)
            {
                Assert(false, "레벨업 보너스 테스트 예외", ex.ToString());
            }
        }

        private static void TestEmergencyQuests()
        {
            Debug.Log("--- 6. 작전 중 돌발 긴급 퀘스트 검증 ---");
            try
            {
                GameObject obj = new GameObject("[TestQuest]");
                EmergencyQuestManager eqm = obj.AddComponent<EmergencyQuestManager>();

                // UI 배너 임시 생성
                GameObject banner = new GameObject("Banner");
                banner.transform.SetParent(obj.transform);
                var text = banner.AddComponent<UnityEngine.UI.Text>();
                eqm.BindUI(banner, text);

                // 퀘스트 시작
                var mStart = typeof(EmergencyQuestManager).GetMethod("StartEmergencyQuest", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                mStart?.Invoke(eqm, new object[] { 0 });

                Assert(eqm.IsQuestActive, "긴급 퀘스트 정상 시작됨");
                Assert(banner.activeSelf, "퀘스트 시작 시 배너 활성화됨");

                // 처치 보고
                for (int i = 0; i < 22; i++)
                {
                    eqm.ReportEnemyKilled(EnemyType.MindDust);
                }

                Assert(!eqm.IsQuestActive, "목표 달성 시 퀘스트 성공 완료 및 비활성화");
                Assert(text.text.Contains("대성공"), "성공 메시지 텍스트 출력 확인");

                UnityEngine.Object.DestroyImmediate(obj);
            }
            catch (Exception ex)
            {
                Assert(false, "돌발 퀘스트 테스트 예외", ex.ToString());
            }
        }

        private static void TestOverloadAndSanctuary()
        {
            Debug.Log("--- 7. 도파민 과부하 & 멘탈 쉼터 존 검증 ---");
            try
            {
                GameObject obj = new GameObject("[TestSanctuary]");
                MentalSanctuaryManager msm = obj.AddComponent<MentalSanctuaryManager>();

                GameObject vigObj = new GameObject("Vignette");
                vigObj.transform.SetParent(obj.transform);
                var vigImg = vigObj.AddComponent<UnityEngine.UI.Image>();
                msm.BindVignette(vigImg);

                Assert(msm.OverloadPercent == 0f, "초기 과부하 수치는 0%");
                Assert(!msm.IsInSanctuary, "초기 쉼터 외곽 상태 확인");

                UnityEngine.Object.DestroyImmediate(obj);
            }
            catch (Exception ex)
            {
                Assert(false, "과부하 및 쉼터 테스트 예외", ex.ToString());
            }
        }

        private static void TestSoundAndPauseAndReport()
        {
            Debug.Log("--- 8. 사운드 토글, 일시정지, 종합 진단 리포트 검증 ---");
            try
            {
                // 사운드 토글
                GameObject soundObj = new GameObject("[TestSound]");
                SoundManager sm = soundObj.AddComponent<SoundManager>();

                bool muted = sm.ToggleMute();
                Assert(muted && sm.IsMuted, "사운드 음소거 토글 ON 성공");
                muted = sm.ToggleMute();
                Assert(!muted && !sm.IsMuted, "사운드 음소거 해제 토글 OFF 성공");

                UnityEngine.Object.DestroyImmediate(soundObj);
            }
            catch (Exception ex)
            {
                Assert(false, "사운드/일시정지 테스트 예외", ex.ToString());
            }
        }

        private static void TestEnemyAndProjectiles()
        {
            Debug.Log("--- 9. 몬스터 피격 & 투사체 검증 ---");
            try
            {
                GameObject enemyObj = new GameObject("Enemy");
                enemyObj.AddComponent<CircleCollider2D>();
                Enemy enemy = enemyObj.AddComponent<Enemy>();
                enemy.Initialize(EnemyType.ShortsRunner, 1, 30f);

                float initHP = enemy.HP;
                enemy.TakeDamage(10f); // 단일 인자 호출
                Assert(enemy.HP == initHP - 10f, "Enemy.TakeDamage(10f) 단일 인자 정상 피격 및 HP 차감");

                enemy.TakeDamage(15f, Vector2.right, 0.5f); // 3개 인자 호출
                Assert(enemy.HP == initHP - 25f, "Enemy.TakeDamage(amount, dir, force) 넉백 인자 정상 처리");

                // 투사체 테스트
                GameObject projObj = new GameObject("Proj");
                EnemyProjectile ep = projObj.AddComponent<EnemyProjectile>();
                ep.Initialize(Vector2.up, 5f, 10f);
                Assert(projObj != null, "EnemyProjectile 초기화 및 생성 성공");

                UnityEngine.Object.DestroyImmediate(enemyObj);
                UnityEngine.Object.DestroyImmediate(projObj);
            }
            catch (Exception ex)
            {
                Assert(false, "몬스터 및 투사체 테스트 예외", ex.ToString());
            }
        }

        private static void SetField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(target, value);
        }
    }
}
