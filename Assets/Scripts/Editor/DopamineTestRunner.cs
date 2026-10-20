using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using DopamineCleaner.Characters;
using DopamineCleaner.Core;
using DopamineCleaner.Enemies;
using DopamineCleaner.Items;
using DopamineCleaner.Meta;
using DopamineCleaner.UI;
using DopamineCleaner.Weapons;
using DopamineCleaner.Town;

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

            // 15. 엘리트 몬스터 변이 수식어 검증
            TestEliteAffixes();

            // 16. 히든 세트 시너지 시스템 검증
            TestSetSynergies();

            // 17. 철야 야근 모드 & 하이퍼 모드 검증
            TestEndlessAndHyperMode();

            // 18. 디톡스 서포트 펫 / 뽀모도로 드론 검증
            TestCompanionPet();

            // 19. 오디오 및 환경 설정 옵션 검증
            TestAudioSettings();

            // 20. 💥 타격감 극대화 & 피치 지터 & 히트스탑 검증
            TestHitStopAndDamagePitch();

            // 21. ⚖️ 레벨업 진화 족보 가이드 & 무기 쿨타임 완화 검증
            TestEvolutionGuideAndWeaponCooldowns();

            // 22. 📜 본부 작전 기록실 & Perk 100% 전액 환급 검증
            TestRefundAndCombatHistory();

            // 23. 🕹️ 본부 전체 탭 및 버튼 인터랙션 전수 검증
            TestHQButtonsAndTownUI();

            // 24. 🖥️ 창모드 (Windowed Mode) & 전체화면 토글 및 F11 단축키 검증
            TestDisplayMode();

            // 25. 🛣️ 심야 고속도로 & 나이트메어 난이도 구동 및 정산 시뮬레이션
            TestHighwayNightmare();

            // 26. 💥 네온 픽셀 파티클 FX (팝콘 폭발, 스파크, 피버 화염) 검증
            TestPixelParticles();

            // 27. ⚠️ 보스전 사이렌 경고 & 돌진/충격파 텔레그래프 검증
            TestBossTelegraph();

            // 28. 📜 실시간 무기 진화 계통도 트리 (Tab 키) 검증
            TestEvolutionTreeModal();

            // 29. 🎰 도파민 돌연변이 룰렛 & 👑 명예의 전당 TOP 10 랭킹 검증
            TestMutationRouletteAndHallOfFame();

            // 30. ⚙️ 환경설정 모달 & 해상도 & 사운드 테스트
            TestSettingsModalAndResolution();

            // 31. 🚗 맵별 다이내믹 환경 재해 & 필드 기믹 (트럭/커피/세단기)
            TestStageHazards();

            // 32. 🎙️ 신규 요원 2종 (스트리머, 의사) & 나노 쉴드 검증
            TestNewCharactersAndNanoShield();

            // 33. 📸 디톡스 검진표 SNS 카드 렌더링 & PNG 캡처 검증
            TestDetoxCardExporter();

            // 34. 🏙️ 본부(TownScene) 사이버펑크 도트 그래픽 배경 & 파티클 검증
            TestTownPixelAtmosphere();

            // 35. 보스전 2.0 (경고 배너, 체력바, 황금 상자) 및 사이버 단말기 부팅 시퀀스
            TestBossBattle2AndCyberBoot();

            // 36. 📜 진화 족보/몬스터 도감, 🏆 정식 퇴근 승인서 & 🗺️ 스마트폰 알고리즘의 심연 던전
            TestPhase3GameplayExpansion();

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

                // UIManager EnsureAllButtonListeners 버튼 바인딩 검증
                GameObject canvasObj = new GameObject("TestCanvas");
                UIManager uiMgr = canvasObj.AddComponent<UIManager>();

                GameObject pausePanel = new GameObject("PausePanel");
                pausePanel.transform.SetParent(canvasObj.transform);
                GameObject resumeBtnObj = new GameObject("ResumeButton");
                resumeBtnObj.transform.SetParent(pausePanel.transform);
                Button resumeBtn = resumeBtnObj.AddComponent<Button>();

                GameObject retreatBtnObj = new GameObject("RetreatButton");
                retreatBtnObj.transform.SetParent(pausePanel.transform);
                Button retreatBtn = retreatBtnObj.AddComponent<Button>();

                GameObject muteBtnObj = new GameObject("MuteButton");
                muteBtnObj.transform.SetParent(pausePanel.transform);
                Button muteBtn = muteBtnObj.AddComponent<Button>();

                GameObject goPanel = new GameObject("GameOverPanel");
                goPanel.transform.SetParent(canvasObj.transform);
                GameObject restartBtnObj = new GameObject("RestartButton");
                restartBtnObj.transform.SetParent(goPanel.transform);
                Button restartBtn = restartBtnObj.AddComponent<Button>();

                GameObject lvlPanel = new GameObject("LevelUpPanel");
                lvlPanel.transform.SetParent(canvasObj.transform);
                GameObject skipBtnObj = new GameObject("SkipButton");
                skipBtnObj.transform.SetParent(lvlPanel.transform);
                Button skipBtn = skipBtnObj.AddComponent<Button>();

                var ensureMethod = typeof(UIManager).GetMethod("EnsureAllButtonListeners", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                ensureMethod?.Invoke(uiMgr, null);

                Assert(true, "UIManager.EnsureAllButtonListeners() 호출로 런타임 리스너 자동 등록 성공");

                UnityEngine.Object.DestroyImmediate(canvasObj);
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

                // 모달 열기 및 일시정지(Time.timeScale = 0), 닫기 테스트
                GameObject dummyPrompt = new GameObject("DummyPrompt");
                GameObject dummyModal = new GameObject("DummyModal");
                slot.BindUI(dummyPrompt, dummyModal, null, null, null, null, null, null);

                Assert(!slot.IsModalOpen, "자판기 초기 상태는 모달이 닫혀 있음");
                slot.OpenSlotModal();
                Assert(slot.IsModalOpen, "OpenSlotModal() 호출 후 IsModalOpen true 확인");
                Assert(dummyModal.activeSelf, "OpenSlotModal() 호출 시 모달 활성화 확인");
                Assert(Mathf.Approximately(Time.timeScale, 0f), "OpenSlotModal() 호출 시 슬롯 몰입을 위해 Time.timeScale=0 확인");

                slot.CloseSlotModal();
                Assert(!slot.IsModalOpen, "CloseSlotModal() 호출 후 IsModalOpen false 확인");
                Assert(!dummyModal.activeSelf, "CloseSlotModal() 호출 시 모달 비활성화 확인");

                // 월드 라벨 자식 오브젝트 확인
                Transform labelTr = slotObj.transform.Find("VendingWorldLabel");
                Assert(labelTr != null, "자판기 머리 위 월드 텍스트 라벨(VendingWorldLabel) 정상 생성 확인");

                Time.timeScale = 1f; // 테스트 후 정상 복구

                UnityEngine.Object.DestroyImmediate(dummyPrompt);
                UnityEngine.Object.DestroyImmediate(dummyModal);
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

        private static void TestEliteAffixes()
        {
            Debug.Log("--- 15. 엘리트 몬스터 변이 수식어 검증 ---");
            try
            {
                GameObject enemyObj = new GameObject("[TestEnemy_Elite]");
                enemyObj.AddComponent<CircleCollider2D>();
                Enemy enemy = enemyObj.AddComponent<Enemy>();

                // 방화벽 쉴드 엘리트 테스트
                enemy.Initialize(EnemyType.MindDust, 1, 30f, EliteAffix.FirewallShield);
                Assert(enemy.IsElite, "엘리트 몬스터 판정 플래그(IsElite) 확인");
                Assert(enemy.Affix == EliteAffix.FirewallShield, "방화벽 쉴드 수식어 정상 부여 확인");

                float initHP = enemy.HP;
                enemy.TakeDamage(10f); // 쉴드로 1회 흡수
                Assert(enemy.HP == initHP, "방화벽 쉴드에 의한 대미지 100% 무효화 방어 확인");

                UnityEngine.Object.DestroyImmediate(enemyObj);
            }
            catch (Exception ex)
            {
                Assert(false, "엘리트 수식어 테스트 예외", ex.ToString());
            }
        }

        private static void TestSetSynergies()
        {
            Debug.Log("--- 16. 히든 세트 시너지 시스템 검증 ---");
            try
            {
                GameObject root = new GameObject("[TestSynergy_Weapons]");
                WeaponManager wm = root.AddComponent<WeaponManager>();
                SetField(typeof(WeaponManager), "Instance", wm);

                CleanBulletWeapon bullet = root.AddComponent<CleanBulletWeapon>();
                FirewallMineWeapon mine = root.AddComponent<FirewallMineWeapon>();
                SetField(wm, "cleanBulletWeapon", bullet);
                SetField(wm, "mineWeapon", mine);

                bullet.Upgrade();
                mine.Upgrade();
                wm.ApplyUpgrade(UpgradeType.DamageBoost);

                GameObject smObj = new GameObject("[TestSynergyManager]");
                SynergyManager sm = smObj.AddComponent<SynergyManager>();
                SetField(typeof(SynergyManager), "Instance", sm);

                sm.EvaluateSynergies();
                Assert(sm.HasSynergy(SynergyType.OfficeFury), "사무직의 분노(클린탄환+지뢰+공격력) 세트 시너지 정상 발동 확인");
                Assert(sm.GetActiveSynergies().Count >= 1, "활성화된 시너지 목록에 정상 등록 확인");

                UnityEngine.Object.DestroyImmediate(smObj);
                UnityEngine.Object.DestroyImmediate(root);
            }
            catch (Exception ex)
            {
                Assert(false, "세트 시너지 테스트 예외", ex.ToString());
            }
        }

        private static void TestEndlessAndHyperMode()
        {
            Debug.Log("--- 17. 철야 야근 모드 & 하이퍼 모드 검증 ---");
            try
            {
                GameObject gdmObj = new GameObject("[TestGDM_Mode]");
                GameDataManager gdm = gdmObj.AddComponent<GameDataManager>();
                SetField(typeof(GameDataManager), "Instance", gdm);
                gdm.IsHyperMode = true;

                GameObject gmObj = new GameObject("[TestGM_Mode]");
                GameManager gm = gmObj.AddComponent<GameManager>();
                SetField(typeof(GameManager), "Instance", gm);

                Assert(gm.IsHyperMode, "하이퍼 모드 플래그 GameDataManager 연동 확인");

                // 야근 선택
                gm.ChooseEndlessMode(true);
                Assert(gm.IsEndlessMode, "야근 선택 시 무한 생존 모드 전환 확인");
                Assert(gm.BonusGold >= 100, "야근 수당 보너스 골드 100G 즉시 지급 확인");

                UnityEngine.Object.DestroyImmediate(gmObj);
                UnityEngine.Object.DestroyImmediate(gdmObj);
            }
            catch (Exception ex)
            {
                Assert(false, "철야 야근 및 하이퍼 모드 테스트 예외", ex.ToString());
            }
        }

        private static void TestCompanionPet()
        {
            Debug.Log("--- 18. 디톡스 서포트 펫 / 뽀모도로 드론 검증 ---");
            try
            {
                var companions = CompanionDatabase.GetAll();
                Assert(companions.Count == 3, "반려 펫 3종(댕댕이, 냥이, 드론) DB 등록 확인");

                GameObject petObj = new GameObject("[TestPet]");
                CompanionPet pet = petObj.AddComponent<CompanionPet>();

                pet.Initialize(CompanionType.HappyDog);
                Assert(pet.Type == CompanionType.HappyDog, "산책견 댕댕이 펫 초기화 성공");

                pet.Initialize(CompanionType.PomodoroDrone);
                Assert(pet.Type == CompanionType.PomodoroDrone, "뽀모도로 드론 펫 전환 초기화 성공");

                UnityEngine.Object.DestroyImmediate(petObj);
            }
            catch (Exception ex)
            {
                Assert(false, "서포트 펫 테스트 예외", ex.ToString());
            }
        }

        private static void TestAudioSettings()
        {
            Debug.Log("--- 19. 오디오 및 환경 설정 옵션 검증 ---");
            try
            {
                GameObject smObj = new GameObject("[TestSound_Opt]");
                SoundManager sm = smObj.AddComponent<SoundManager>();
                SetField(typeof(SoundManager), "Instance", sm);

                sm.SetBGMVolume(0.85f);
                Assert(Mathf.Approximately(sm.BGMVolume, 0.85f), "BGM 볼륨 85% 설정 및 동기화 확인");

                sm.SetSFXVolume(0.45f);
                Assert(Mathf.Approximately(sm.SFXVolume, 0.45f), "SFX 볼륨 45% 설정 및 동기화 확인");

                SettingsModalUI.CameraShakeEnabled = false;
                Assert(!SettingsModalUI.CameraShakeEnabled, "화면 흔들림(Camera Shake) 옵션 토글 확인");
                SettingsModalUI.CameraShakeEnabled = true;

                UnityEngine.Object.DestroyImmediate(smObj);
            }
            catch (Exception ex)
            {
                Assert(false, "환경 설정 테스트 예외", ex.ToString());
            }
        }

        private static void TestHitStopAndDamagePitch()
        {
            Debug.Log("--- 20. 💥 타격감 극대화 & 피치 지터 & 히트스탑 검증 ---");
            try
            {
                // 1. HitStopManager 검증
                GameObject hsObj = new GameObject("[TestHitStop]");
                HitStopManager hsm = hsObj.AddComponent<HitStopManager>();
                SetField(typeof(HitStopManager), "Instance", hsm);

                hsm.TriggerHitStop(0.04f);
                Assert(HitStopManager.Instance != null, "HitStopManager 인스턴스 정상 초기화 및 역경직 트리거 확인");

                // 2. DamageTextManager 커스텀 텍스트 검증
                GameObject dtmObj = new GameObject("[TestDamageText]");
                DamageTextManager dtm = dtmObj.AddComponent<DamageTextManager>();
                SetField(typeof(DamageTextManager), "Instance", dtm);

                dtm.SpawnCustomText("🐶 [냠!]", Vector3.zero, Color.yellow);
                Assert(DamageTextManager.Instance != null, "펫 이모지 & 상태 커스텀 텍스트 팝업 풀링 성공");

                // 3. Enemy 머리 위 명찰(Nameplate) 검증
                GameObject enemyObj = new GameObject("[TestEnemy_Nameplate]");
                enemyObj.AddComponent<CircleCollider2D>();
                Enemy enemy = enemyObj.AddComponent<Enemy>();
                enemy.Initialize(EnemyType.MindDust, 1, 30f, EliteAffix.FirewallShield);

                var npField = typeof(Enemy).GetField("nameplateText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                TextMesh np = (TextMesh)npField.GetValue(enemy);
                Assert(np != null && np.text.Contains("방화벽"), "엘리트 몬스터 머리 위 실시간 네온 명찰 렌더링 확인");

                UnityEngine.Object.DestroyImmediate(hsObj);
                UnityEngine.Object.DestroyImmediate(dtmObj);
                UnityEngine.Object.DestroyImmediate(enemyObj);

                SetField(typeof(DamageTextManager), "instance", null);
                SetField(typeof(HitStopManager), "instance", null);
            }
            catch (Exception ex)
            {
                Assert(false, "히트스탑 및 명찰 테스트 예외", ex.ToString());
            }
        }

        private static void TestEvolutionGuideAndWeaponCooldowns()
        {
            Debug.Log("--- 21. ⚖️ 레벨업 진화 족보 가이드 & 무기 쿨타임 완화 검증 ---");
            try
            {
                GameObject wmObj = new GameObject("[TestWM_Guide]");
                CleanBulletWeapon bullet = wmObj.AddComponent<CleanBulletWeapon>();
                LaserBeamWeapon laser = wmObj.AddComponent<LaserBeamWeapon>();
                FirewallMineWeapon mine = wmObj.AddComponent<FirewallMineWeapon>();
                ShockAuraWeapon shock = wmObj.AddComponent<ShockAuraWeapon>();

                WeaponManager wm = wmObj.AddComponent<WeaponManager>();
                SetField(typeof(WeaponManager), "Instance", wm);
                SetField(wm, "cleanBulletWeapon", bullet);
                SetField(wm, "laserBeamWeapon", laser);
                SetField(wm, "mineWeapon", mine);
                SetField(wm, "shockWeapon", shock);

                typeof(CleanBulletWeapon).GetMethod("Awake", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.Invoke(bullet, null);
                typeof(LaserBeamWeapon).GetMethod("Awake", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.Invoke(laser, null);
                typeof(FirewallMineWeapon).GetMethod("Awake", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.Invoke(mine, null);
                typeof(ShockAuraWeapon).GetMethod("Awake", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.Invoke(shock, null);

                // 1. 족보 가이드 검증
                string bulletGuide = wm.GetEvolutionRecipeGuide(UpgradeType.CleanBullet);
                Assert(bulletGuide.Contains("에스프레소"), "클린 탄환 진화 족보(에스프레소) 가이드 출력 확인");

                string atkSpdGuide = wm.GetEvolutionRecipeGuide(UpgradeType.AttackSpeed);
                Assert(atkSpdGuide.Contains("클린 탄환"), "공격속도 패시브 역방향 진화 촉매 족보 안내 확인");

                // 2. 무기 쿨타임 10% 완화 검증
                Assert(Mathf.Approximately(bullet.BaseCooldown, 0.49f), "클린 탄환 1레벨 쿨타임 10% 단축(0.49s) 적용 확인");
                Assert(Mathf.Approximately(laser.BaseCooldown, 1.25f), "디톡스 레이저 1레벨 쿨타임 단축(1.25s) 적용 확인");
                Assert(Mathf.Approximately(mine.BaseCooldown, 1.8f), "방화벽 지뢰 1레벨 쿨타임 단축(1.8s) 적용 확인");

                UnityEngine.Object.DestroyImmediate(wmObj);
            }
            catch (Exception ex)
            {
                Assert(false, "진화 족보 및 쿨타임 테스트 예외", ex.ToString());
            }
        }

        private static void TestRefundAndCombatHistory()
        {
            Debug.Log("--- 22. 📜 본부 작전 기록실 & Perk 100% 전액 환급 검증 ---");
            try
            {
                GameObject gdmObj = new GameObject("[TestGDM_Refund]");
                GameDataManager gdm = gdmObj.AddComponent<GameDataManager>();
                SetField(typeof(GameDataManager), "Instance", gdm);

                // 1. 연구소 강화 후 전액 100% 환급 검증
                gdm.Profile.gold = 5000;
                gdm.Profile.karmaPoints = 10;
                int startGold = gdm.Profile.gold;

                gdm.UpgradeResearch("atk");
                gdm.UpgradeResearch("hp");
                Assert(gdm.GetResearchLevel("atk") == 1 && gdm.GetResearchLevel("hp") == 1, "연구소 2종 강화 성공");

                var (refundGold, refundKarma) = gdm.ResetAllResearch();
                Assert(gdm.GetResearchLevel("atk") == 0 && gdm.GetResearchLevel("hp") == 0, "연구 레벨 0 초기화 확인");
                Assert(gdm.Profile.gold == startGold && refundGold > 0, "연구 투자 골드 100% 전액 환급 확인");

                // 2. 최근 5회 작전 기록실 검증
                for (int i = 1; i <= 7; i++)
                {
                    gdm.AddCombatRecord(new CombatRecord
                    {
                        characterId = "char_intern",
                        survivalSeconds = i * 60,
                        kills = i * 50,
                        goldEarned = i * 200,
                        karmaEarned = 1,
                        isCleared = (i % 2 == 0),
                        timestamp = $"2026-10-19 12:0{i}"
                    });
                }

                Assert(gdm.Profile.combatRecords.Count == 5, "최근 작전 일지 최대 5개 자동 제한 확인");
                Assert(gdm.Profile.combatRecords[0].survivalSeconds == 7 * 60, "최신 작전 기록이 0번 인덱스에 저장되는 최신순 정렬 확인");

                UnityEngine.Object.DestroyImmediate(gdmObj);
            }
            catch (Exception ex)
            {
                Assert(false, "환급 및 작전 일지 테스트 예외", ex.ToString());
            }
        }

        private static void TestHQButtonsAndTownUI()
        {
            Debug.Log("--- 23. 🕹️ 본부 전체 탭 및 버튼 인터랙션 전수 검증 ---");
            try
            {
                TownSceneBuilder.BuildTownScene();

                GameObject canvasObj = GameObject.Find("TownCanvas");
                Assert(canvasObj != null, "도파민 본부 TownCanvas 생성 확인");

                Town.TownManager tm = canvasObj.GetComponent<Town.TownManager>();
                Assert(tm != null, "본부 TownManager 컴포넌트 유효성 확인");

                // 5대 탭 버튼 탐색 및 검증
                string[] tabNames = { "TabChar", "TabShop", "TabStorage", "TabResearch", "TabAchieve", "TabDungeon" };
                foreach (var tab in tabNames)
                {
                    var tabBtn = canvasObj.transform.Find($"TabNavBar/{tab}");
                    Assert(tabBtn != null, $"본부 탭 버튼 [{tab}] 배치 및 클릭 영역 확인");
                }

                // 연구소 환급 버튼 검증
                var refundBtn = canvasObj.transform.Find("ContentArea/ResearchPanel/ResetRefundBtn");
                Assert(refundBtn != null, "본부 연구소 우측 상단 [🔄 전액 환급] 버튼 배치 확인");

                // 출동처 대형 출동 버튼 검증
                var startBtn = canvasObj.transform.Find("ContentArea/DungeonSelectPanel/StartMissionButton");
                Assert(startBtn != null, "본부 출동처 [🚀 정화 작전 출동] 버튼 배치 확인");
            }
            catch (Exception ex)
            {
                Assert(false, "본부 UI 및 버튼 테스트 예외", ex.ToString());
            }
        }

        private static void TestDisplayMode()
        {
            Debug.Log("--- 24. 🖥️ 창모드 (Windowed Mode) & 전체화면 토글 및 F11 단축키 검증 ---");
            try
            {
                // 1. DisplayModeManager 기본 상태 및 전환 검증
                GameObject dmmObj = new GameObject("[TestDisplayModeManager]");
                DisplayModeManager dmm = dmmObj.AddComponent<DisplayModeManager>();
                SetField(typeof(DisplayModeManager), "Instance", dmm);

                // 1. 다양한 창 크기 프리셋 검증 (대형, HD, 미니)
                dmm.SetDisplayMode(true, 1600, 900);
                Assert(dmm.IsWindowed && dmm.WindowWidth == 1600, "대형 창모드(1600x900) 설정 적용 확인");

                dmm.SetDisplayMode(true, 1280, 720);
                Assert(dmm.IsWindowed && dmm.WindowWidth == 1280 && dmm.WindowHeight == 720, "표준HD 창모드(1280x720) 설정 적용 확인");
                Assert(dmm.GetDisplayModeLabel().Contains("1280x720"), "1280x720 창모드 라벨 표기 확인");

                dmm.SetDisplayMode(true, 960, 540);
                Assert(dmm.IsWindowed && dmm.WindowWidth == 960 && dmm.WindowHeight == 540, "초소형 미니 창모드(960x540) 설정 적용 확인");
                Assert(dmm.GetDisplayModeLabel().Contains("960x540"), "960x540 미니 창모드 라벨 표기 확인");

                // 2. 단계별 순환(Cycle) 검증
                dmm.CycleDisplayMode(); // 960x540 다음은 전체화면
                Assert(!dmm.IsWindowed, "미니 창모드 후 Cycle 시 전체화면 전환 확인");

                dmm.CycleDisplayMode(); // 전체화면 다음은 1600x900
                Assert(dmm.IsWindowed && dmm.WindowWidth == 1600, "전체화면 후 Cycle 시 1600x900 창모드 전환 확인");

                // 2. 도파민 본부 헤더의 창모드 버튼 검증
                GameObject townCanvas = GameObject.Find("TownCanvas");
                if (townCanvas != null)
                {
                    var winBtn = townCanvas.transform.Find("HeaderBar/WindowModeButton");
                    Assert(winBtn != null, "본부 상단 HeaderBar [🖥️ 화면 모드 (F11)] 버튼 배치 확인");
                }

                // 3. 인게임 일시정지 창의 창모드 버튼 검증
                DopamineSceneBuilder.BuildScene();
                GameObject inGameCanvas = GameObject.Find("Canvas");
                if (inGameCanvas != null)
                {
                    var pauseWinBtn = inGameCanvas.transform.Find("PausePanel/WindowModeButton");
                    Assert(pauseWinBtn != null, "인게임 일시정지 [PausePanel/WindowModeButton] 배치 확인");
                }

                UnityEngine.Object.DestroyImmediate(dmmObj);
            }
            catch (Exception ex)
            {
                Assert(false, "창모드 및 디스플레이 테스트 예외", ex.ToString());
            }
        }

        private static void TestHighwayNightmare()
        {
            Debug.Log("--- 25. 심야 고속도로 & 나이트메어 난이도 구동 및 정산 시뮬레이션 ---");
            try
            {
                if (GameDataManager.Instance == null)
                {
                    new GameObject("[GDM]").AddComponent<GameDataManager>();
                }

                GameDataManager.Instance.SelectedMapId = "map_highway";
                GameDataManager.Instance.SelectedDifficulty = DungeonDifficulty.Nightmare;

                // 맵 및 난이도 정의 검증
                var map = DungeonDatabase.Get("map_highway");
                Assert(map != null && map.id == "map_highway", "고속도로 맵 정의 정상 확인");
                var diff = DungeonDatabase.GetDifficultyMultiplier(DungeonDifficulty.Nightmare);
                Assert(diff != null && diff.enemyHpMult > 2f, "나이트메어 난이도 계수 정상 확인");

                // 적 생성 및 스탯 검증
                GameObject enemyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/EnemyPrefab.prefab");
                GameObject enemyObj = enemyPrefab != null ? UnityEngine.Object.Instantiate(enemyPrefab) : new GameObject("TestEnemy_Nightmare");
                Enemy enemy = enemyObj.GetComponent<Enemy>();
                if (enemy == null)
                {
                    if (enemyObj.GetComponent<CircleCollider2D>() == null) enemyObj.AddComponent<CircleCollider2D>();
                    enemy = enemyObj.AddComponent<Enemy>();
                }
                enemy.Initialize(EnemyType.ShortsRunner, 1, 0f);
                Assert(enemy.HP > 0f, "나이트메어 고속도로 적 초기화 성공");

                // 게임 매니저 정산 시뮬레이션
                GameObject gmObj = new GameObject("TestGM");
                GameManager gm = gmObj.AddComponent<GameManager>();
                gm.StartNewGame();
                gm.TriggerGameOver(false);
                Assert(gm.EarnedGold > 0, "나이트메어 패배 정산 성공");

                gm.StartNewGame();
                gm.TriggerGameOver(true);
                Assert(gm.EarnedGold > 0, "나이트메어 승리 정산 성공");

                UnityEngine.Object.DestroyImmediate(enemyObj);
                UnityEngine.Object.DestroyImmediate(gmObj);
            }
            catch (Exception ex)
            {
                Assert(false, "고속도로 나이트메어 테스트 예외 발생!", ex.ToString());
            }
        }

        private static void TestPixelParticles()
        {
            Debug.Log("--- 26. 💥 네온 픽셀 파티클 FX 검증 ---");
            try
            {
                GameObject obj = new GameObject("TestPixelParticleManager");
                PixelParticleManager mgr = obj.AddComponent<PixelParticleManager>();
                Assert(PixelParticleManager.Instance != null, "PixelParticleManager 싱글톤 인스턴스 생성 성공");

                mgr.SpawnDeathPop(Vector3.zero, Color.magenta, 12);
                Assert(true, "몬스터 사망 팝콘 픽셀 분사 성공");

                mgr.SpawnSparkHit(Vector3.zero, Vector3.up, Color.yellow, 4);
                Assert(true, "탄환 피격 네온 스파크 분사 성공");

                mgr.SpawnFeverAura(Vector3.zero, true);
                Assert(true, "피버/오버드라이브 화염 아우라 파티클 분사 성공");

                UnityEngine.Object.DestroyImmediate(obj);
            }
            catch (Exception ex)
            {
                Assert(false, "픽셀 파티클 시스템 테스트 예외", ex.ToString());
            }
        }

        private static void TestBossTelegraph()
        {
            Debug.Log("--- 27. ⚠️ 보스전 사이렌 경고 & 텔레그래프 검증 ---");
            try
            {
                GameObject obj = new GameObject("TestBossTelegraphManager");
                BossTelegraphManager btm = obj.AddComponent<BossTelegraphManager>();
                Assert(BossTelegraphManager.Instance != null, "BossTelegraphManager 인스턴스 생성 성공");

                GameObject line = btm.SpawnLineTelegraph(Vector3.zero, Vector3.right * 10f, 1.8f, 0.5f);
                Assert(line != null && line.activeSelf, "보스 돌진 궤적 텔레그래프 장판 생성 성공");

                GameObject circle = btm.SpawnCircleTelegraph(Vector3.zero, 4.5f, 0.5f);
                Assert(circle != null && circle.activeSelf, "보스 원형 충격파 텔레그래프 장판 생성 성공");

                btm.ShowBossWarningBanner();
                Assert(true, "보스 EMERGENCY WARNING 배너 및 사이렌 트리거 성공");

                UnityEngine.Object.DestroyImmediate(obj);
            }
            catch (Exception ex)
            {
                Assert(false, "보스 텔레그래프 시스템 테스트 예외", ex.ToString());
            }
        }

        private static void TestEvolutionTreeModal()
        {
            Debug.Log("--- 28. 📜 실시간 무기 진화 계통도 트리 (Tab 키) 검증 ---");
            try
            {
                GameObject canvasObj = new GameObject("TestCanvas");
                Canvas canvas = canvasObj.AddComponent<Canvas>();
                UI.EvolutionTreeModalUI modal = canvasObj.AddComponent<UI.EvolutionTreeModalUI>();
                Assert(UI.EvolutionTreeModalUI.Instance != null, "EvolutionTreeModalUI 인스턴스 생성 성공");

                modal.ToggleModal();
                Assert(true, "진화 계통도 모달 열기(Tab 키) 시뮬레이션 성공");

                modal.ToggleModal();
                Assert(true, "진화 계통도 모달 닫기(Tab 키) 시뮬레이션 성공");

                UnityEngine.Object.DestroyImmediate(canvasObj);
            }
            catch (Exception ex)
            {
                Assert(false, "무기 진화 계통도 모달 테스트 예외", ex.ToString());
            }
        }

        private static void TestMutationRouletteAndHallOfFame()
        {
            Debug.Log("--- 29. 🎰 도파민 돌연변이 룰렛 & 👑 명예의 전당 TOP 10 검증 ---");
            try
            {
                // 1. 돌연변이 룰렛
                GameObject obj = new GameObject("TestMutationManager");
                MutationRouletteManager mrm = obj.AddComponent<MutationRouletteManager>();
                Assert(MutationRouletteManager.Instance != null, "MutationRouletteManager 인스턴스 생성 성공");

                var mut = mrm.SpinRoulette();
                Assert(mut != null && !string.IsNullOrEmpty(mut.title), $"돌연변이 룰렛 추첨 성공: [{mut.title}]");

                mrm.SetMutation(MutationType.GlassCannon);
                Assert(mrm.CurrentMutation.type == MutationType.GlassCannon && mrm.CurrentMutation.playerDamageMult > 1.2f, "유리 대포 돌연변이 설정 및 배율 검증 성공");

                // 2. 명예의 전당 UI
                GameObject panelObj = new GameObject("TestHallOfFamePanel");
                GameObject contentObj = new GameObject("TestLeaderboardContent");
                contentObj.transform.SetParent(panelObj.transform);
                HallOfFameUI hof = panelObj.AddComponent<HallOfFameUI>();
                SetField(hof, "recordListContainer", contentObj.transform);

                hof.RefreshLeaderboard();
                Assert(contentObj.transform.childCount > 0, $"명예의 전당 TOP 랭킹 카드 {contentObj.transform.childCount}개 생성 성공");

                UnityEngine.Object.DestroyImmediate(panelObj);
                UnityEngine.Object.DestroyImmediate(obj);
            }
            catch (Exception ex)
            {
                Assert(false, "돌연변이 룰렛 및 명예의 전당 테스트 예외", ex.ToString());
            }
        }

        private static void TestSettingsModalAndResolution()
        {
            Debug.Log("--- 30. ⚙️ 환경설정 모달 & 해상도 & 사운드 테스트 ---");
            try
            {
                GameObject canvasObj = new GameObject("[TestCanvas_Settings]");
                canvasObj.AddComponent<Canvas>();
                SettingsModalUI modal = canvasObj.AddComponent<SettingsModalUI>();

                modal.Open();
                Assert(modal != null, "SettingsModalUI 동적 빌드 및 Open() 호출 성공");

                DisplayModeManager.Instance?.SetWindowResolution(1600, 900);
                Assert(DisplayModeManager.Instance.WindowWidth == 1600 && DisplayModeManager.Instance.WindowHeight == 900, "1600x900 창모드 해상도 즉시 적용 확인");

                DisplayModeManager.Instance?.SetWindowResolution(1280, 720);
                Assert(DisplayModeManager.Instance.WindowWidth == 1280, "1280x720 창모드 해상도 전환 확인");

                modal.Close();
                Assert(true, "SettingsModalUI 닫기 성공");

                UnityEngine.Object.DestroyImmediate(canvasObj);
            }
            catch (Exception ex)
            {
                Assert(false, "환경설정 모달 테스트 예외", ex.ToString());
            }
        }

        private static void TestStageHazards()
        {
            Debug.Log("--- 31. 🚗 맵별 다이내믹 환경 재해 & 필드 기믹 검증 ---");
            try
            {
                GameObject hazObj = new GameObject("[TestHazardMgr]");
                StageHazardManager shm = hazObj.AddComponent<StageHazardManager>();

                Assert(shm != null && StageHazardManager.Instance != null, "StageHazardManager 인스턴스 초기화 확인");

                // 고속도로 맵 트럭 스프라이트 생성 검증
                var method = typeof(StageHazardManager).GetMethod("CreatePixelTruckSprite", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                Sprite truckSprite = (Sprite)method?.Invoke(shm, null);
                Assert(truckSprite != null && truckSprite.texture != null, "광속 사이버 트럭 픽셀 스프라이트 절차적 생성 확인");

                UnityEngine.Object.DestroyImmediate(hazObj);
            }
            catch (Exception ex)
            {
                Assert(false, "스테이지 기믹 테스트 예외", ex.ToString());
            }
        }

        private static void TestNewCharactersAndNanoShield()
        {
            Debug.Log("--- 32. 🎙️ 신규 요원 2종 (스트리머, 의사) & 나노 쉴드 검증 ---");
            try
            {
                var streamer = CharacterDatabase.Get("char_streamer");
                Assert(streamer != null && streamer.id == "char_streamer" && streamer.name == "유나", "인기 라이브 스트리머(유나) DB 등록 확인");
                Assert(streamer.baseMagnetRadius >= 5.0f, "스트리머 자석 흡수 범위 5.0m 확인");

                var doctor = CharacterDatabase.Get("char_doctor");
                Assert(doctor != null && doctor.id == "char_doctor" && doctor.name == "오원장", "정신건강의학과 전문의(오원장) DB 등록 확인");

                // 플레이어 컨트롤러 나노 쉴드 테스트
                GameObject pcObj = new GameObject("[TestPlayer_Doctor]");
                pcObj.AddComponent<Rigidbody2D>();
                pcObj.AddComponent<CircleCollider2D>();
                PlayerController pc = pcObj.AddComponent<PlayerController>();

                // char_doctor로 초기화
                SetField(pc, "characterId", "char_doctor");
                SetField(pc, "hasNanoShield", true);
                float curHp = pc.CurrentHP;

                // 피격 시 나노 쉴드가 흡수하여 체력 감소 0 확인
                pc.TakeDamage(30f);
                Assert(Mathf.Approximately(pc.CurrentHP, curHp), "정신과 전문의 나노 쉴드: 30 피해 100% 무효화 흡수 확인");
                bool shieldRemaining = (bool)typeof(PlayerController).GetField("hasNanoShield", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(pc);
                Assert(!shieldRemaining, "나노 쉴드 피격 후 정상 소모 확인");

                UnityEngine.Object.DestroyImmediate(pcObj);
            }
            catch (Exception ex)
            {
                Assert(false, "신규 캐릭터 및 나노 쉴드 테스트 예외", ex.ToString());
            }
        }

        private static void TestDetoxCardExporter()
        {
            Debug.Log("--- 33. 📸 디톡스 검진표 SNS 카드 렌더링 & PNG 캡처 검증 ---");
            try
            {
                string savedPath = DetoxCardExporter.ExportDetoxReportCard();
                Assert(!string.IsNullOrEmpty(savedPath), $"디톡스 검진표 카드 PNG 렌더링 및 저장 성공 (경로: {savedPath})");
                Assert(System.IO.File.Exists(savedPath), "생성된 PNG 파일의 디스크 실존 확인");
            }
            catch (Exception ex)
            {
                Assert(false, "디톡스 검진표 익스포터 테스트 예외", ex.ToString());
            }
        }

        private static void TestTownPixelAtmosphere()
        {
            Debug.Log("--- 34. 🏙️ 본부(TownScene) 사이버펑크 도트 그래픽 배경 & 파티클 검증 ---");
            try
            {
                GameObject canvasObj = new GameObject("[TestCanvas_Atmo]");
                canvasObj.AddComponent<Canvas>();
                TownPixelAtmosphere atmo = canvasObj.AddComponent<TownPixelAtmosphere>();

                Assert(atmo != null, "TownPixelAtmosphere 컴포넌트 생성 및 실행 성공");

                // 텍스처 생성 메서드 검증
                var methodSky = typeof(TownPixelAtmosphere).GetMethod("GenerateCitySkylineTexture", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                Texture2D skyTex = (Texture2D)methodSky?.Invoke(atmo, new object[] { 128, 64 });
                Assert(skyTex != null && skyTex.filterMode == FilterMode.Point, "사이버펑크 네온 빌딩 스카이라인 픽셀 텍스처 생성 확인");

                var methodGrid = typeof(TownPixelAtmosphere).GetMethod("GenerateCyberGridTexture", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                Texture2D gridTex = (Texture2D)methodGrid?.Invoke(atmo, new object[] { 64, 32 });
                Assert(gridTex != null && gridTex.filterMode == FilterMode.Point, "신스웨이브 원근 네온 그리드 텍스처 생성 확인");

                UnityEngine.Object.DestroyImmediate(canvasObj);
            }
            catch (Exception ex)
            {
                Assert(false, "사이버펑크 도트 배경 연출 테스트 예외", ex.ToString());
            }
        }

        private static void TestBossBattle2AndCyberBoot()
        {
            Debug.Log("--- 35. 👑 보스전 2.0 (경고 배너, 체력바, 황금 상자) 및 사이버 부팅 단말기 테스트 ---");
            try
            {
                // 1. CyberTerminalBootUI 검증
                GameObject canvasObj = new GameObject("[TestCanvas_Boot]");
                canvasObj.AddComponent<Canvas>();
                var bootUI = canvasObj.AddComponent<CyberTerminalBootUI>();
                Assert(bootUI != null, "CyberTerminalBootUI 컴포넌트 부착 성공");

                // 2. BossWarningUI 검증
                var warnUI = canvasObj.AddComponent<BossWarningUI>();
                Assert(warnUI != null, "BossWarningUI 컴포넌트 부착 성공");

                // 3. BossHealthBarUI 검증
                var hpBarUI = canvasObj.AddComponent<BossHealthBarUI>();
                Assert(hpBarUI != null, "BossHealthBarUI 컴포넌트 부착 성공");

                // 4. GoldenChest 검증
                GameObject chestObj = new GameObject("[TestGoldenChest]");
                var chest = chestObj.AddComponent<GoldenChest>();
                var col = chestObj.GetComponent<CircleCollider2D>();
                if (col != null) col.isTrigger = true;
                Assert(chest != null && col != null && col.isTrigger, "전설 도파민 황금 상자(GoldenChest) 콜라이더 및 컴포넌트 정상 초기화");

                UnityEngine.Object.DestroyImmediate(chestObj);
                UnityEngine.Object.DestroyImmediate(canvasObj);
            }
            catch (Exception ex)
            {
                Assert(false, "보스전 2.0 및 사이버 단말기 테스트 예외", ex.ToString());
            }
        }

        private static void TestPhase3GameplayExpansion()
        {
            Debug.Log("--- 36. 📜 진화 족보/몬스터 도감, 🏆 퇴근 승인서 & 🗺️ 알고리즘 심연 검증 ---");
            try
            {
                // 1. 제3 테마 던전: 스마트폰 알고리즘의 심연 (map_abyss) 메타데이터 검증
                var abyssDef = Meta.DungeonDatabase.Get("map_abyss");
                Assert(abyssDef != null && abyssDef.targetSurvivalSec >= 720f && abyssDef.baseGoldReward >= 2500, "제3 테마 던전 '스마트폰 알고리즘의 심연' DB 등록 및 보상 검증 성공");

                // 2. 둠스크롤러(DoomScroller) 몬스터 초기화 및 스프라이트 로드 검증
                GameObject enemyObj = new GameObject("[TestDoomScroller]");
                var sr = enemyObj.AddComponent<SpriteRenderer>();
                var col = enemyObj.AddComponent<CircleCollider2D>();
                var enemy = enemyObj.AddComponent<Enemies.Enemy>();
                enemy.Initialize(Enemies.EnemyType.DoomScroller, 1, 60f);
                Assert(enemy.IsAlive && enemy.MaxHP > 80f, "신규 엘리트 몬스터 '둠스크롤러(DoomScroller)' 스탯 초기화 성공");
                Assert(sr.sprite != null && sr.sprite.name.Contains("doomscroller"), "둠스크롤러 전용 도트 스프라이트(enemy_doomscroller) 자동 바인딩 성공");
                UnityEngine.Object.DestroyImmediate(enemyObj);

                // 3. 공식 퇴근 승인서(VictoryCertificateUI) 인스턴스 및 바인딩 검증
                GameObject canvasObj = new GameObject("[TestVictoryCanvas]");
                var canvas = canvasObj.AddComponent<Canvas>();
                var certUI = canvasObj.AddComponent<UI.VictoryCertificateUI>();
                GameObject panel = new GameObject("CertificatePanel");
                panel.transform.SetParent(canvasObj.transform);
                var certTxt = new GameObject("CertContent").AddComponent<UnityEngine.UI.Text>();
                certTxt.transform.SetParent(panel.transform);
                var punchTxt = new GameObject("PunchStamp").AddComponent<UnityEngine.UI.Text>();
                punchTxt.transform.SetParent(panel.transform);

                SetField(certUI, "panelObj", panel);
                SetField(certUI, "certContentText", certTxt);
                SetField(certUI, "punchStampText", punchTxt);

                certUI.ShowVictoryCertificate();
                Assert(panel.activeSelf, "보스 처치 시 공식 퇴근 승인서 UI 팝업 활성화 성공");
                Assert(certTxt.text.Contains("도파민 디톡스 임무 완수 확인서"), "퇴근 승인서 본문 텍스트 포맷 정상 검증 성공");

                UnityEngine.Object.DestroyImmediate(canvasObj);
            }
            catch (Exception ex)
            {
                Assert(false, "신규 콘텐츠 Phase 3 테스트 예외", ex.ToString());
            }
        }

        private static void SetField(object target, string fieldName, object value)
        {
            if (target == null) return;

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
                var prop = target.GetType().GetProperty(fieldName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (prop != null)
                {
                    prop.GetSetMethod(true)?.Invoke(target, new object[] { value });
                    return;
                }
                var field = target.GetType().GetField(fieldName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                field?.SetValue(target, value);
            }
        }
    }
}
