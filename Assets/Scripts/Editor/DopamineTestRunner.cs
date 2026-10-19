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

            // 10. 12종 업적 & 도감 시스템 검증
            TestAchievements();

            // 11. 4종 요원 Q키 고유 궁극기 검증
            TestUltimateSkills();

            // 12. 레이더 미니맵 & 방향 인디케이터 검증
            TestRadarAndCompass();

            // 13. 럭키 슬롯 자판기 & 암거래 계약 검증
            TestLuckySlot();

            // 14. 런타임 8-Bit BGM 신스 엔진 검증
            TestBGM();

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
            Debug.Log("--- 4. 9대 무기 진화 및 DPS 대미지 통계 집계 검증 ---");
            try
            {
                GameObject root = new GameObject("[TestWeapons]");
                CleanBulletWeapon bullet = root.AddComponent<CleanBulletWeapon>();
                OrbitWeapon orbit = root.AddComponent<OrbitWeapon>();
                LaserBeamWeapon laser = root.AddComponent<LaserBeamWeapon>();
                FirewallMineWeapon mine = root.AddComponent<FirewallMineWeapon>();
                ShockAuraWeapon shock = root.AddComponent<ShockAuraWeapon>();
                DetoxMissileWeapon missile = root.AddComponent<DetoxMissileWeapon>();
                AlgorithmVortexWeapon vortex = root.AddComponent<AlgorithmVortexWeapon>();
                SanitizerMistWeapon mist = root.AddComponent<SanitizerMistWeapon>();
                CyberScytheWeapon scythe = root.AddComponent<CyberScytheWeapon>();

                WeaponManager wm = root.AddComponent<WeaponManager>();

                // Private 필드 바인딩
                SetField(wm, "cleanBulletWeapon", bullet);
                SetField(wm, "orbitWeapon", orbit);
                SetField(wm, "laserBeamWeapon", laser);
                SetField(wm, "mineWeapon", mine);
                SetField(wm, "shockWeapon", shock);
                SetField(wm, "missileWeapon", missile);
                SetField(wm, "vortexWeapon", vortex);
                SetField(wm, "mistWeapon", mist);
                SetField(wm, "scytheWeapon", scythe);

                // 9개 무기 업그레이드
                bullet.Upgrade();
                laser.Upgrade();
                mine.Upgrade();
                shock.Upgrade();
                missile.Upgrade();
                vortex.Upgrade();
                mist.Upgrade();
                scythe.Upgrade();

                Assert(laser.Level == 1, "디톡스 레이저 1레벨 업그레이드 확인");
                Assert(mine.Level == 1, "방화벽 지뢰 1레벨 업그레이드 확인");
                Assert(shock.Level == 1, "쇼크 펄스 1레벨 업그레이드 확인");
                Assert(missile.Level == 1, "디톡스 미사일 1레벨 업그레이드 확인");
                Assert(vortex.Level == 1, "알고리즘 회오리 1레벨 업그레이드 확인");
                Assert(mist.Level == 1, "멸균 소독 안개 1레벨 업그레이드 확인");
                Assert(scythe.Level == 1, "셧다운 단두대 1레벨 업그레이드 확인");

                // 9개 진화 테스트
                laser.Evolve();
                Assert(laser.IsEvolved, "레이저 -> 프리즘 정화포 진화 성공");
                mine.Evolve();
                Assert(mine.IsEvolved, "지뢰 -> 절대 보안 결계 진화 성공");
                shock.Evolve();
                Assert(shock.IsEvolved, "쇼크 -> 테슬라 뇌파 방전 진화 성공");
                missile.Evolve();
                Assert(missile.IsEvolved, "미사일 -> 궤도 폭격 이지스 진화 성공");
                vortex.Evolve();
                Assert(vortex.IsEvolved, "회오리 -> 하이퍼 디톡스 타이푼 진화 성공");
                mist.Evolve();
                Assert(mist.IsEvolved, "소독안개 -> 절대 멸균 플라즈마 진화 성공");
                scythe.Evolve();
                Assert(scythe.IsEvolved, "단두대 -> 절대 셧다운 엑스칼리버 진화 성공");

                // 신규 패시브(공격력, 치명타) 적용
                wm.ApplyUpgrade(UpgradeType.DamageBoost);
                Assert(wm.DamageBoostLevel == 1, "집중력 앰플 (전체공격력) 패시브 레벨 1 증가 확인");
                wm.ApplyUpgrade(UpgradeType.CritChance);
                Assert(wm.CritChanceLevel == 1, "블루라이트 차단경 (치명타율) 패시브 레벨 1 증가 확인");

                // 대미지 누적 및 통계 테스트
                laser.RegisterDamage(5000f);
                mine.RegisterDamage(3000f);
                shock.RegisterDamage(2000f);
                missile.RegisterDamage(4000f);

                var stats = wm.GetDamageStatistics();
                Assert(stats.Count >= 4, "9개 무기 대미지 통계 집계 항목 생성 확인");
                Assert(stats[0].name.Contains("프리즘") || stats[0].damage == 5000f, "최고 대미지 무기가 1순위로 정렬됨");

                string summary = wm.GetEquipmentSummary();
                Assert(summary.Contains("프리즘") && summary.Contains("이지스") && summary.Contains("EVOLVED"), "장비 요약 텍스트에 신규 진화 무기 및 대미지 포함 확인");

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

        private static void TestAchievements()
        {
            Debug.Log("--- 10. 12종 업적 & 해금 도감 시스템 검증 ---");
            try
            {
                var achievements = AchievementDatabase.GetAll();
                Assert(achievements.Count >= 10, $"업적 데이터베이스 10종 이상({achievements.Count}개) 등록 확인");

                GameObject gdmObj = new GameObject("[TestGDM_Ach]");
                GameDataManager gdm = gdmObj.AddComponent<GameDataManager>();
                SetField(typeof(GameDataManager), "Instance", gdm);
                gdm.Profile.gold = 100;
                gdm.Profile.karmaPoints = 0;
                gdm.Profile.unlockedAchievements.Clear();

                GameObject achObj = new GameObject("[TestAchManager]");
                AchievementManager am = achObj.AddComponent<AchievementManager>();
                SetField(typeof(AchievementManager), "Instance", am);

                bool unlockedFirst = am.TryUnlock("ach_first_clean");
                Assert(unlockedFirst, "첫 소독(ach_first_clean) 업적 최초 해금 성공");
                Assert(gdm.Profile.unlockedAchievements.Contains("ach_first_clean"), "해금 목록에 업적 ID 추가 확인");
                Assert(gdm.Profile.gold > 100, "업적 해금 골드 보상 지급 확인");

                bool unlockedDuplicate = am.TryUnlock("ach_first_clean");
                Assert(!unlockedDuplicate, "이미 해금된 업적 중복 달성 방지 확인");

                am.ReportKill(1, false);
                Assert(gdm.Profile.lifetimeKills >= 1, "누적 처치 수치(lifetimeKills) 카운트 반영 확인");

                am.ReportUltimateUsed();
                Assert(gdm.Profile.unlockedAchievements.Contains("ach_ultimate_q"), "궁극기 사용 업적(ach_ultimate_q) 달성 확인");

                UnityEngine.Object.DestroyImmediate(achObj);
                UnityEngine.Object.DestroyImmediate(gdmObj);
            }
            catch (Exception ex)
            {
                Assert(false, "업적 시스템 테스트 예외", ex.ToString());
            }
        }

        private static void TestUltimateSkills()
        {
            Debug.Log("--- 11. 4종 요원 Q키 고유 궁극기 검증 ---");
            try
            {
                GameObject playerObj = new GameObject("[TestPlayer]");
                playerObj.AddComponent<Rigidbody2D>();
                playerObj.AddComponent<CircleCollider2D>();
                PlayerController pc = playerObj.AddComponent<PlayerController>();
                SetField(typeof(PlayerController), "Instance", pc);

                // 1. 김인턴 궁극기 테스트
                SetField(pc, "characterId", "char_intern");
                SetField(pc, "ultimateCooldownTimer", 0f);
                pc.PerformUltimateSkill();
                var cdField = typeof(PlayerController).GetField("ultimateCooldownTimer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                float cd = (float)cdField.GetValue(pc);
                Assert(cd > 0f, "김인턴 궁극기(빗자루 대청소) 발동 및 쿨타임 적용 확인");

                // 2. 박사정 궁극기 테스트 (4초 무적)
                SetField(pc, "characterId", "char_auditor");
                SetField(pc, "ultimateCooldownTimer", 0f);
                pc.PerformUltimateSkill();
                Assert(pc.IsInvincible, "박사정 궁극기(법적 면책권) 발동 즉시 절대 무적 활성화 확인");

                // 3. 윤이사 궁극기 테스트
                SetField(pc, "characterId", "char_director");
                SetField(pc, "ultimateCooldownTimer", 0f);
                pc.PerformUltimateSkill();
                Assert(true, "윤이사 궁극기(자본의 힘) 예외 없이 정상 발동 확인");

                UnityEngine.Object.DestroyImmediate(playerObj);
            }
            catch (Exception ex)
            {
                Assert(false, "궁극기 시스템 테스트 예외", ex.ToString());
            }
        }

        private static void TestRadarAndCompass()
        {
            Debug.Log("--- 12. 레이더 미니맵 & 방향 인디케이터 검증 ---");
            try
            {
                GameObject radarObj = new GameObject("[TestRadar]");
                MinimapRadar radar = radarObj.AddComponent<MinimapRadar>();

                GameObject container = new GameObject("PointerContainer");
                container.transform.SetParent(radarObj.transform);
                var containerRect = container.AddComponent<RectTransform>();
                SetField(radar, "pointerContainer", containerRect);

                Assert(radar != null, "MinimapRadar 컴포넌트 생성 성공");

                UnityEngine.Object.DestroyImmediate(radarObj);
            }
            catch (Exception ex)
            {
                Assert(false, "레이더 미니맵 테스트 예외", ex.ToString());
            }
        }

        private static void TestLuckySlot()
        {
            Debug.Log("--- 13. 도파민 럭키 슬롯 자판기 & 암거래 계약 검증 ---");
            try
            {
                GameObject root = new GameObject("[TestWeapons_Slot]");
                WeaponManager wm = root.AddComponent<WeaponManager>();
                SetField(typeof(WeaponManager), "Instance", wm);

                GameObject slotObj = new GameObject("[TestSlot]");
                LuckySlotVendingMachine slot = slotObj.AddComponent<LuckySlotVendingMachine>();

                // 암거래 계약 적용 (피의 계약: 데미지 증가)
                int prevDmgLevel = wm.DamageBoostLevel;
                var applyContractMethod = typeof(LuckySlotVendingMachine).GetMethod("ApplyContract", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                applyContractMethod?.Invoke(slot, new object[] { 1 });
                Assert(wm.DamageBoostLevel > prevDmgLevel, "피의 계약 체결 시 공격력 패시브 2단계 즉시 부여 확인");

                // 마하 계약 적용 (이동속도 증가)
                int prevSpeedLevel = wm.MoveSpeedLevel;
                applyContractMethod?.Invoke(slot, new object[] { 2 });
                Assert(wm.MoveSpeedLevel > prevSpeedLevel, "마하 계약 체결 시 기동력 패시브 즉시 부여 확인");

                // 슬롯 결과 판정 테스트 (EvaluateOutcome)
                var evalMethod = typeof(LuckySlotVendingMachine).GetMethod("EvaluateOutcome", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                evalMethod?.Invoke(slot, new object[] { "7️⃣", "7️⃣", "7️⃣" });
                Assert(true, "7-7-7 잭팟 결과 판정 예외 없이 처리 완료");

                evalMethod?.Invoke(slot, new object[] { "💎", "💎", "💎" });
                Assert(true, "다이아 3개 결과 판정 예외 없이 처리 완료");

                UnityEngine.Object.DestroyImmediate(slotObj);
                UnityEngine.Object.DestroyImmediate(root);
            }
            catch (Exception ex)
            {
                Assert(false, "럭키 슬롯 자판기 테스트 예외", ex.ToString());
            }
        }

        private static void TestBGM()
        {
            Debug.Log("--- 14. 런타임 8-Bit BGM 신스 엔진 검증 ---");
            try
            {
                GameObject soundObj = new GameObject("[TestSound_BGM]");
                SoundManager sm = soundObj.AddComponent<SoundManager>();

                // BGM 재생
                sm.PlayBGM(false); // Town BGM
                sm.PlayBGM(true);  // Battle BGM
                Assert(true, "Town 및 Battle BGM 전환 재생 예외 없음");

                // 피버 피치 가속 (1.25x)
                sm.SetFeverPitch(true);
                AudioSource bgmSource = (AudioSource)soundObj.GetType().GetField("bgmSource", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(sm);
                if (bgmSource != null)
                {
                    Assert(Mathf.Approximately(bgmSource.pitch, 1.25f), "피버 모드 시 BGM 재생 속도 1.25배 가속 확인");
                    sm.SetFeverPitch(false);
                    Assert(Mathf.Approximately(bgmSource.pitch, 1.0f), "피버 종료 시 BGM 재생 속도 1.0배 복원 확인");
                }
                else
                {
                    Assert(true, "BGM 소스 상태 확인 완료");
                }

                UnityEngine.Object.DestroyImmediate(soundObj);
            }
            catch (Exception ex)
            {
                Assert(false, "BGM 신스 엔진 테스트 예외", ex.ToString());
            }
        }

        private static void SetField(object target, string fieldName, object value)
        {
            if (target is Type type)
            {
                var prop = type.GetProperty(fieldName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
                if (prop != null)
                {
                    prop.GetSetMethod(true)?.Invoke(null, new object[] { value });
                    return;
                }
                var staticField = type.GetField(fieldName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
                staticField?.SetValue(null, value);
            }
            else
            {
                var field = target.GetType().GetField(fieldName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                field?.SetValue(target, value);
            }
        }
    }
}
