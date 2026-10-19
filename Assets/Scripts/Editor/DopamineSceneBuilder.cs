using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using DopamineCleaner.Core;
using DopamineCleaner.Characters;
using DopamineCleaner.Enemies;
using DopamineCleaner.Weapons;
using DopamineCleaner.Items;
using DopamineCleaner.UI;

namespace DopamineCleaner.Editor
{
    public static class DopamineSceneBuilder
    {
        private static Font koreanFont;

        [MenuItem("Dopamine Cleaner/✨ Build Complete Gameplay Scene (원클릭 자동 구성)")]
        public static void BuildScene()
        {
            EnsureTagsExist();
            LoadKoreanFont();

            // 1. 프리팹 생성
            GameObject projectilePrefab = CreateOrUpdateProjectilePrefab();
            GameObject satellitePrefab = CreateOrUpdateSatellitePrefab();
            GameObject gemPrefab = CreateOrUpdateGemPrefab();
            GameObject enemyPrefab = CreateOrUpdateEnemyPrefab();
            GameObject cardPrefab = CreateOrUpdateUpgradeCardPrefab();

            // 2. 새 씬 생성
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 3. 메인 카메라
            GameObject camObj = new GameObject("Main Camera");
            camObj.tag = "MainCamera";
            Camera cam = camObj.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 7.5f;
            cam.backgroundColor = new Color(0.92f, 0.96f, 0.94f); // 파스텔 민트그린
            cam.clearFlags = CameraClearFlags.SolidColor;
            camObj.AddComponent<AudioListener>();
            CameraFollow camFollow = camObj.AddComponent<CameraFollow>();

            // 4. 예쁜 2D 도트 잔디 타일맵 배경 (Background Tiled)
            GameObject bgObj = new GameObject("Background_MentalGarden");
            bgObj.transform.position = Vector3.zero;
            Sprite bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Environment/bg_mental_garden.png");
            if (bgSprite != null)
            {
                SpriteRenderer bgSr = bgObj.AddComponent<SpriteRenderer>();
                bgSr.sprite = bgSprite;
                bgSr.drawMode = SpriteDrawMode.Tiled;
                bgSr.size = new Vector2(120f, 120f); // 120x120 대형 파스텔 멘탈 가든
                bgSr.sortingOrder = -10;
            }

            // 5. 플레이어 생성
            GameObject playerObj = new GameObject("Player");
            playerObj.tag = "Player";
            playerObj.transform.position = Vector3.zero;

            Sprite playerSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Characters/player_brush.png");
            SpriteRenderer playerSr = playerObj.AddComponent<SpriteRenderer>();
            playerSr.sprite = playerSprite;
            playerSr.sortingOrder = 10;

            Rigidbody2D playerRb = playerObj.AddComponent<Rigidbody2D>();
            playerRb.gravityScale = 0f;
            playerRb.freezeRotation = true;
            playerRb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            CircleCollider2D playerCol = playerObj.AddComponent<CircleCollider2D>();
            playerCol.radius = 0.45f;

            PlayerController playerCtrl = playerObj.AddComponent<PlayerController>();

            // 플레이어 무기 장착
            GameObject weaponsObj = new GameObject("Weapons");
            weaponsObj.transform.SetParent(playerObj.transform);

            CleanBulletWeapon bulletWeapon = weaponsObj.AddComponent<CleanBulletWeapon>();
            SetPrivateField(bulletWeapon, "projectilePrefab", projectilePrefab);

            OrbitWeapon orbitWeapon = weaponsObj.AddComponent<OrbitWeapon>();
            SetPrivateField(orbitWeapon, "satellitePrefab", satellitePrefab);

            LaserBeamWeapon laserWeapon = weaponsObj.AddComponent<LaserBeamWeapon>();
            FirewallMineWeapon mineWeapon = weaponsObj.AddComponent<FirewallMineWeapon>();
            ShockAuraWeapon shockWeapon = weaponsObj.AddComponent<ShockAuraWeapon>();
            DetoxMissileWeapon missileWeapon = weaponsObj.AddComponent<DetoxMissileWeapon>();
            AlgorithmVortexWeapon vortexWeapon = weaponsObj.AddComponent<AlgorithmVortexWeapon>();
            SanitizerMistWeapon mistWeapon = weaponsObj.AddComponent<SanitizerMistWeapon>();
            CyberScytheWeapon scytheWeapon = weaponsObj.AddComponent<CyberScytheWeapon>();

            WeaponManager weaponMgr = weaponsObj.AddComponent<WeaponManager>();
            SetPrivateField(weaponMgr, "cleanBulletWeapon", bulletWeapon);
            SetPrivateField(weaponMgr, "orbitWeapon", orbitWeapon);
            SetPrivateField(weaponMgr, "laserBeamWeapon", laserWeapon);
            SetPrivateField(weaponMgr, "mineWeapon", mineWeapon);
            SetPrivateField(weaponMgr, "shockWeapon", shockWeapon);
            SetPrivateField(weaponMgr, "missileWeapon", missileWeapon);
            SetPrivateField(weaponMgr, "vortexWeapon", vortexWeapon);
            SetPrivateField(weaponMgr, "mistWeapon", mistWeapon);
            SetPrivateField(weaponMgr, "scytheWeapon", scytheWeapon);

            // 6. 게임 매니저 & 스포너 & 아이템 매니저 & 피버 매니저 & 데미지 텍스트 & 사운드 매니저 & 돌발 퀘스트 & 업적 매니저
            GameObject sysObj = new GameObject("[Systems]");
            GameManager gameMgr = sysObj.AddComponent<GameManager>();
            FeverManager feverMgr = sysObj.AddComponent<FeverManager>();
            DamageTextManager dmgTextMgr = sysObj.AddComponent<DamageTextManager>();
            SoundManager soundMgr = sysObj.AddComponent<SoundManager>();
            EmergencyQuestManager questMgr = sysObj.AddComponent<EmergencyQuestManager>();
            MentalSanctuaryManager sanctuaryMgr = sysObj.AddComponent<MentalSanctuaryManager>();
            Meta.AchievementManager achieveMgr = sysObj.AddComponent<Meta.AchievementManager>();

            // 7. 필드 럭키 슬롯 자판기
            GameObject slotMachineObj = new GameObject("LuckySlotMachine");
            slotMachineObj.transform.position = new Vector3(5.5f, 4.0f, 0f);
            LuckySlotVendingMachine slotMachine = slotMachineObj.AddComponent<LuckySlotVendingMachine>();

            EnemySpawner spawner = sysObj.AddComponent<EnemySpawner>();
            SetPrivateField(spawner, "enemyPrefab", enemyPrefab);

            ItemManager itemMgr = sysObj.AddComponent<ItemManager>();
            SetPrivateField(itemMgr, "gemPrefab", gemPrefab);

            // 8. UI 캔버스 및 HUD 생성 (한글 100% 완벽 출력 표준 uGUI Text)
            GameObject canvasObj = new GameObject("Canvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            canvasObj.AddComponent<GraphicRaycaster>();

            // EventSystem
            GameObject esObj = new GameObject("EventSystem");
            esObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

            // UI Manager
            UIManager uiMgr = canvasObj.AddComponent<UIManager>();
            BuildCanvasUI(canvasObj, uiMgr, cardPrefab, questMgr, sanctuaryMgr, slotMachine);

            // 8. 씬 저장
            Directory.CreateDirectory("Assets/Scenes");
            string scenePath = "Assets/Scenes/MainGame.unity";
            EditorSceneManager.SaveScene(scene, scenePath);

            EditorBuildSettingsScene[] scenes = new EditorBuildSettingsScene[]
            {
                new EditorBuildSettingsScene("Assets/Scenes/TownScene.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/MainGame.unity", true)
            };
            EditorBuildSettings.scenes = scenes;

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        [MenuItem("Dopamine Cleaner/🚀 Build Complete Game (Town + Dungeon)")]
        public static void BuildCompleteGame()
        {
            TownSceneBuilder.BuildTownScene();
            BuildScene();
            EditorSceneManager.OpenScene("Assets/Scenes/TownScene.unity");
            EditorUtility.DisplayDialog("도파민 클리너 (Dopamine Cleaner)", "🎉 도파민 본부(TownScene)와 액션 던전(MainGame) 원클릭 구축이 완료되었습니다!\n상단 [▶ 플레이] 버튼을 누르면 본부에서 요원을 고르고 보급소/창고 이용 후 정화 작전으로 출동할 수 있습니다!", "확인");
        }

        private static void LoadKoreanFont()
        {
            koreanFont = AssetDatabase.LoadAssetAtPath<Font>("Assets/Fonts/MalgunGothic.ttf");
            if (koreanFont == null)
            {
                koreanFont = Font.CreateDynamicFontFromOSFont(new string[] { "Malgun Gothic", "맑은 고딕", "Arial" }, 24);
            }
        }

        private static void BuildCanvasUI(GameObject canvasObj, UIManager uiMgr, GameObject cardPrefab, EmergencyQuestManager questMgr = null, MentalSanctuaryManager sanctuaryMgr = null, LuckySlotVendingMachine slotMachine = null)
        {
            // 화면 전체 도파민 과부하 비네트 (최하단 레이어)
            GameObject vignetteObj = CreateUIObject("OverloadVignette", canvasObj.transform);
            RectTransform vRt = vignetteObj.GetComponent<RectTransform>();
            vRt.anchorMin = Vector2.zero;
            vRt.anchorMax = Vector2.one;
            vRt.sizeDelta = Vector2.zero;
            Image vImg = vignetteObj.AddComponent<Image>();
            vImg.color = new Color(0.95f, 0.2f, 0.45f, 0f);
            vImg.raycastTarget = false;
            sanctuaryMgr?.BindVignette(vImg);

            // 업적 달성 토스트 배너 (상단 중앙 팝업)
            GameObject toastObj = CreateUIObject("AchievementToast", canvasObj.transform);
            RectTransform tRt = toastObj.GetComponent<RectTransform>();
            tRt.anchorMin = new Vector2(0.5f, 0.94f);
            tRt.anchorMax = new Vector2(0.5f, 0.94f);
            tRt.sizeDelta = new Vector2(620, 75);
            Image tBg = toastObj.AddComponent<Image>();
            tBg.color = new Color(0.12f, 0.22f, 0.18f, 0.96f);
            Outline tOut = toastObj.AddComponent<Outline>();
            tOut.effectColor = new Color(0.3f, 1f, 0.6f);
            tOut.effectDistance = new Vector2(2f, -2f);

            Text toastTxt = CreateText("ToastText", toastObj.transform, "🏆 [업적 달성!] 첫 소독의 첫걸음\n보상: +150 Gold", 17, TextAnchor.MiddleCenter, Vector2.zero, new Vector2(600, 65));
            toastTxt.color = new Color(1f, 0.95f, 0.5f);
            toastTxt.fontStyle = FontStyle.Bold;
            toastObj.SetActive(false);

            SetPrivateField(uiMgr, "achievementToastPanel", toastObj);
            SetPrivateField(uiMgr, "achievementToastText", toastTxt);

            // 상단 레이아웃 (어두운 반투명 HUD 글래스 패널)
            GameObject topHud = CreateUIObject("TopHUD", canvasObj.transform);
            RectTransform topRt = topHud.GetComponent<RectTransform>();
            topRt.anchorMin = new Vector2(0.5f, 1f);
            topRt.anchorMax = new Vector2(0.5f, 1f);
            topRt.pivot = new Vector2(0.5f, 1f);
            topRt.sizeDelta = new Vector2(920, 210);
            topRt.anchoredPosition = new Vector2(0, -15);

            Image topBg = topHud.AddComponent<Image>();
            topBg.color = new Color(0.12f, 0.09f, 0.18f, 0.88f);

            // 경험치 바 (최상단)
            Slider expSlider = CreateSimpleSlider("ExpBar", topHud.transform, new Color(1f, 0.46f, 0.56f), new Vector2(0, 75));
            SetPrivateField(uiMgr, "expSlider", expSlider);

            // 체력 바
            Slider hpSlider = CreateSimpleSlider("HpBar", topHud.transform, new Color(0.32f, 0.72f, 0.53f), new Vector2(0, 38));
            SetPrivateField(uiMgr, "hpSlider", hpSlider);

            // 피버 게이지 바
            Slider feverSlider = CreateSimpleSlider("FeverBar", topHud.transform, new Color(0.25f, 0.75f, 1f), new Vector2(0, 2));
            SetPrivateField(uiMgr, "feverSlider", feverSlider);

            // 레벨 텍스트
            Text lvlText = CreateText("LevelText", topHud.transform, "Lv.1", 24, TextAnchor.MiddleLeft, new Vector2(-380, 75));
            lvlText.color = new Color(1f, 0.78f, 0.35f);
            lvlText.fontStyle = FontStyle.Bold;
            Outline lvlOut = lvlText.gameObject.AddComponent<Outline>();
            lvlOut.effectColor = new Color(0f, 0f, 0f, 0.95f);
            lvlOut.effectDistance = new Vector2(1.5f, -1.5f);
            SetPrivateField(uiMgr, "levelText", lvlText);

            // 피버 게이지 텍스트
            Text feverTxt = CreateText("FeverText", topHud.transform, "⚡ 디톡스 피버 게이지: 0%", 18, TextAnchor.MiddleLeft, new Vector2(-380, 2));
            feverTxt.color = new Color(0.35f, 0.85f, 1f);
            feverTxt.fontStyle = FontStyle.Bold;
            Outline feverOut = feverTxt.gameObject.AddComponent<Outline>();
            feverOut.effectColor = new Color(0f, 0f, 0f, 0.95f);
            feverOut.effectDistance = new Vector2(1.2f, -1.2f);
            SetPrivateField(uiMgr, "feverText", feverTxt);

            // 콤보 텍스트
            Text comboTxt = CreateText("ComboText", topHud.transform, "⚡ 10 COMBO!", 20, TextAnchor.MiddleRight, new Vector2(380, 2));
            comboTxt.color = new Color(1f, 0.85f, 0.2f);
            comboTxt.fontStyle = FontStyle.Bold;
            Outline comboOut = comboTxt.gameObject.AddComponent<Outline>();
            comboOut.effectColor = new Color(0f, 0f, 0f, 0.95f);
            comboOut.effectDistance = new Vector2(1.5f, -1.5f);
            comboTxt.gameObject.SetActive(false);
            SetPrivateField(uiMgr, "comboText", comboTxt);

            // 타이머 텍스트 (화사한 골드 옐로우 + 또렷한 블랙 아웃라인)
            Text timerText = CreateText("TimerText", topHud.transform, "00:00", 34, TextAnchor.MiddleCenter, new Vector2(0, -42));
            timerText.color = new Color(1f, 0.88f, 0.32f);
            timerText.fontStyle = FontStyle.Bold;
            Outline timerOut = timerText.gameObject.AddComponent<Outline>();
            timerOut.effectColor = new Color(0f, 0f, 0f, 0.95f);
            timerOut.effectDistance = new Vector2(2f, -2f);
            SetPrivateField(uiMgr, "timerText", timerText);

            // 킬/스테이지 텍스트 (산뜻한 네온 민트 그린 + 블랙 아웃라인)
            Text killsText = CreateText("KillsText", topHud.transform, "STAGE 1 [0/35]  👾 0", 22, TextAnchor.MiddleCenter, new Vector2(0, -78));
            killsText.color = new Color(0.40f, 0.95f, 0.75f);
            killsText.fontStyle = FontStyle.Bold;
            Outline killsOut = killsText.gameObject.AddComponent<Outline>();
            killsOut.effectColor = new Color(0f, 0f, 0f, 0.95f);
            killsOut.effectDistance = new Vector2(1.5f, -1.5f);
            SetPrivateField(uiMgr, "stageKillsText", killsText);

            // 피버 배너 (화면 중앙 상단에 깜빡이는 배너)
            GameObject feverBannerObj = CreateUIObject("FeverBanner", canvasObj.transform);
            RectTransform fbRt = feverBannerObj.GetComponent<RectTransform>();
            fbRt.anchorMin = new Vector2(0.5f, 0.75f);
            fbRt.anchorMax = new Vector2(0.5f, 0.75f);
            fbRt.sizeDelta = new Vector2(700, 70);
            Image fbImg = feverBannerObj.AddComponent<Image>();
            fbImg.color = new Color(1f, 0.3f, 0.1f, 0.85f);
            Text fbTxt = CreateText("FbText", feverBannerObj.transform, "🔥 DETOX FEVER TIME! (연사속도 2배) 🔥", 26, TextAnchor.MiddleCenter, Vector2.zero);
            fbTxt.color = Color.white;
            fbTxt.fontStyle = FontStyle.Bold;
            feverBannerObj.SetActive(false);
            SetPrivateField(uiMgr, "feverBanner", feverBannerObj);

            // 긴급 돌발 퀘스트 배너 (피버 배너 위쪽)
            GameObject questBannerObj = CreateUIObject("QuestBanner", canvasObj.transform);
            RectTransform qbRt = questBannerObj.GetComponent<RectTransform>();
            qbRt.anchorMin = new Vector2(0.5f, 0.84f);
            qbRt.anchorMax = new Vector2(0.5f, 0.84f);
            qbRt.sizeDelta = new Vector2(820, 58);
            Image qbImg = questBannerObj.AddComponent<Image>();
            qbImg.color = new Color(0.12f, 0.08f, 0.20f, 0.94f);
            Outline qbOut = questBannerObj.AddComponent<Outline>();
            qbOut.effectColor = new Color(1f, 0.75f, 0.2f, 0.95f);
            qbOut.effectDistance = new Vector2(2f, -2f);

            Text qbTxt = CreateText("QbText", questBannerObj.transform, "⚠️ [긴급 작전] 미션 준비 중...", 19, TextAnchor.MiddleCenter, Vector2.zero, new Vector2(800, 50));
            qbTxt.color = new Color(1f, 0.88f, 0.35f);
            qbTxt.fontStyle = FontStyle.Bold;
            questBannerObj.SetActive(false);

            questMgr?.BindUI(questBannerObj, qbTxt);

            // 하단 대시 / 구급약 / 궁극기 텍스트
            GameObject bottomHud = CreateUIObject("BottomHUD", canvasObj.transform);
            RectTransform botRt = bottomHud.GetComponent<RectTransform>();
            botRt.anchorMin = new Vector2(0.5f, 0f);
            botRt.anchorMax = new Vector2(0.5f, 0f);
            botRt.pivot = new Vector2(0.5f, 0f);
            botRt.sizeDelta = new Vector2(960, 65);
            botRt.anchoredPosition = new Vector2(0, 15);

            Image botBg = bottomHud.AddComponent<Image>();
            botBg.color = new Color(0.12f, 0.09f, 0.18f, 0.90f);
            Outline botOut = bottomHud.AddComponent<Outline>();
            botOut.effectColor = new Color(0.2f, 0.85f, 1f, 0.5f);
            botOut.effectDistance = new Vector2(1.5f, -1.5f);

            Text dashText = CreateText("DashText", bottomHud.transform, "💨 대시 준비완료 (Space / Shift)", 20, TextAnchor.MiddleCenter, new Vector2(-220, 0), new Vector2(460, 50));
            dashText.color = new Color(0.70f, 0.90f, 1f);
            dashText.fontStyle = FontStyle.Bold;
            SetPrivateField(uiMgr, "dashCooldownText", dashText);

            Text ultimateText = CreateText("UltimateText", bottomHud.transform, "⚡ [Q] 궁극기 [READY!]", 20, TextAnchor.MiddleCenter, new Vector2(230, 0), new Vector2(440, 50));
            ultimateText.color = new Color(1f, 0.9f, 0.25f);
            ultimateText.fontStyle = FontStyle.Bold;
            SetPrivateField(uiMgr, "ultimateCooldownText", ultimateText);

            // 우측 상단 미니맵 레이더 & 나침반
            GameObject radarObj = CreateUIObject("RadarPanel", canvasObj.transform);
            RectTransform rRt = radarObj.GetComponent<RectTransform>();
            rRt.anchorMin = new Vector2(1f, 1f);
            rRt.anchorMax = new Vector2(1f, 1f);
            rRt.pivot = new Vector2(1f, 1f);
            rRt.sizeDelta = new Vector2(160, 160);
            rRt.anchoredPosition = new Vector2(-25, -25);
            Image rBg = radarObj.AddComponent<Image>();
            rBg.color = new Color(0.08f, 0.10f, 0.16f, 0.90f);
            Outline rOut = radarObj.AddComponent<Outline>();
            rOut.effectColor = new Color(0.2f, 0.85f, 1f, 0.8f);
            rOut.effectDistance = new Vector2(1.5f, -1.5f);

            CreateText("RadarTitle", radarObj.transform, "📡 RADAR", 14, TextAnchor.UpperCenter, new Vector2(0, -6), new Vector2(140, 20));

            GameObject ptrContainer = CreateUIObject("PointerContainer", canvasObj.transform);
            RectTransform ptrRt = ptrContainer.GetComponent<RectTransform>();
            ptrRt.anchorMin = Vector2.zero;
            ptrRt.anchorMax = Vector2.one;
            ptrRt.sizeDelta = Vector2.zero;

            MinimapRadar mm = radarObj.AddComponent<MinimapRadar>();
            SetPrivateField(mm, "radarRect", rRt);
            SetPrivateField(mm, "pointerContainer", ptrRt);

            // 럭키 슬롯 자판기 상호작용 프롬프트 & 모달
            GameObject slotPromptObj = CreateUIObject("SlotPrompt", canvasObj.transform);
            RectTransform spRt = slotPromptObj.GetComponent<RectTransform>();
            spRt.anchorMin = new Vector2(0.5f, 0.22f);
            spRt.anchorMax = new Vector2(0.5f, 0.22f);
            spRt.sizeDelta = new Vector2(480, 50);
            Image spBg = slotPromptObj.AddComponent<Image>();
            spBg.color = new Color(0.1f, 0.12f, 0.2f, 0.92f);
            Outline spOut = slotPromptObj.AddComponent<Outline>();
            spOut.effectColor = new Color(1f, 0.85f, 0.2f);
            Text spTxt = CreateText("SpText", slotPromptObj.transform, "🎰 [E] 도파민 럭키 자판기 이용 (50G)", 20, TextAnchor.MiddleCenter, Vector2.zero, new Vector2(460, 45));
            slotPromptObj.SetActive(false);

            GameObject slotModalObj = CreateUIObject("SlotModalPanel", canvasObj.transform);
            RectTransform smRt = slotModalObj.GetComponent<RectTransform>();
            smRt.anchorMin = new Vector2(0.5f, 0.5f);
            smRt.anchorMax = new Vector2(0.5f, 0.5f);
            smRt.sizeDelta = new Vector2(650, 520);
            Image smBg = slotModalObj.AddComponent<Image>();
            smBg.color = new Color(0.12f, 0.10f, 0.18f, 0.98f);
            Outline smOut = slotModalObj.AddComponent<Outline>();
            smOut.effectColor = new Color(1f, 0.3f, 0.8f);
            smOut.effectDistance = new Vector2(2f, -2f);

            CreateText("SmTitle", slotModalObj.transform, "🎰 도파민 럭키 슬롯 & 암거래", 30, TextAnchor.MiddleCenter, new Vector2(0, 220), new Vector2(580, 45));

            GameObject reelsObj = CreateUIObject("ReelsGroup", slotModalObj.transform);
            RectTransform rgRt = reelsObj.GetComponent<RectTransform>();
            rgRt.sizeDelta = new Vector2(460, 110);
            rgRt.anchoredPosition = new Vector2(0, 120);
            Image rgBg = reelsObj.AddComponent<Image>();
            rgBg.color = new Color(0.06f, 0.05f, 0.10f, 0.95f);

            Text r1 = CreateText("Reel1", reelsObj.transform, "7️⃣", 52, TextAnchor.MiddleCenter, new Vector2(-140, 0), new Vector2(100, 100));
            Text r2 = CreateText("Reel2", reelsObj.transform, "💎", 52, TextAnchor.MiddleCenter, new Vector2(0, 0), new Vector2(100, 100));
            Text r3 = CreateText("Reel3", reelsObj.transform, "🍒", 52, TextAnchor.MiddleCenter, new Vector2(140, 0), new Vector2(100, 100));

            Text resTxt = CreateText("ResText", slotModalObj.transform, "💰 50 골드로 레버를 당기세요!", 18, TextAnchor.MiddleCenter, new Vector2(0, 45), new Vector2(580, 35));

            GameObject spinBtnObj = CreateUIObject("SpinBtn", slotModalObj.transform);
            RectTransform sbRt = spinBtnObj.GetComponent<RectTransform>();
            sbRt.sizeDelta = new Vector2(240, 55);
            sbRt.anchoredPosition = new Vector2(0, -15);
            Image sbImg = spinBtnObj.AddComponent<Image>();
            sbImg.color = new Color(0.9f, 0.3f, 0.45f);
            Button spinBtn = spinBtnObj.AddComponent<Button>();
            CreateText("SbTxt", spinBtnObj.transform, "🎰 레버 당기기 (50G)", 20, TextAnchor.MiddleCenter, Vector2.zero, sbRt.sizeDelta);

            GameObject c1BtnObj = CreateUIObject("Contract1Btn", slotModalObj.transform);
            RectTransform c1Rt = c1BtnObj.GetComponent<RectTransform>();
            c1Rt.sizeDelta = new Vector2(260, 60);
            c1Rt.anchoredPosition = new Vector2(-140, -95);
            Image c1Img = c1BtnObj.AddComponent<Image>();
            c1Img.color = new Color(0.25f, 0.15f, 0.22f);
            Button c1Btn = c1BtnObj.AddComponent<Button>();
            Text c1Txt = CreateText("C1Txt", c1BtnObj.transform, "🩸 [피의 계약]\n공격력 +24%", 15, TextAnchor.MiddleCenter, Vector2.zero, c1Rt.sizeDelta);

            GameObject c2BtnObj = CreateUIObject("Contract2Btn", slotModalObj.transform);
            RectTransform c2Rt = c2BtnObj.GetComponent<RectTransform>();
            c2Rt.sizeDelta = new Vector2(260, 60);
            c2Rt.anchoredPosition = new Vector2(140, -95);
            Image c2Img = c2BtnObj.AddComponent<Image>();
            c2Img.color = new Color(0.15f, 0.22f, 0.28f);
            Button c2Btn = c2BtnObj.AddComponent<Button>();
            Text c2Txt = CreateText("C2Txt", c2BtnObj.transform, "⚡ [마하 계약]\n기동/공속 강화", 15, TextAnchor.MiddleCenter, Vector2.zero, c2Rt.sizeDelta);

            GameObject closeBtnObj = CreateUIObject("CloseBtn", slotModalObj.transform);
            RectTransform cbRt = closeBtnObj.GetComponent<RectTransform>();
            cbRt.sizeDelta = new Vector2(160, 42);
            cbRt.anchoredPosition = new Vector2(0, -180);
            Image cbImg = closeBtnObj.AddComponent<Image>();
            cbImg.color = new Color(0.35f, 0.35f, 0.45f);
            Button closeBtn = closeBtnObj.AddComponent<Button>();
            CreateText("CbTxt", closeBtnObj.transform, "✖️ 닫기", 18, TextAnchor.MiddleCenter, Vector2.zero, cbRt.sizeDelta);

            slotModalObj.SetActive(false);
            slotMachine?.BindUI(slotPromptObj, slotModalObj, r1, r2, r3, resTxt, spinBtn, closeBtn, c1Btn, c2Btn, c1Txt, c2Txt);

            // 레벨업 패널
            GameObject lvlPanel = CreateUIObject("LevelUpPanel", canvasObj.transform);
            RectTransform lvlRt = lvlPanel.GetComponent<RectTransform>();
            lvlRt.anchorMin = Vector2.zero;
            lvlRt.anchorMax = Vector2.one;
            lvlRt.sizeDelta = Vector2.zero;

            Image dimImg = lvlPanel.AddComponent<Image>();
            dimImg.color = new Color(0.1f, 0.08f, 0.15f, 0.88f);

            Text lvlTitle = CreateText("LvlUpTitle", lvlPanel.transform, "🎉 레벨 업!", 48, TextAnchor.MiddleCenter, new Vector2(0, 350));
            Text lvlSub = CreateText("LvlUpSub", lvlPanel.transform, "원하는 도파민 디톡스 능력을 선택하세요", 24, TextAnchor.MiddleCenter, new Vector2(0, 280));

            GameObject cardsContainer = CreateUIObject("CardsContainer", lvlPanel.transform);
            RectTransform cardContRt = cardsContainer.GetComponent<RectTransform>();
            cardContRt.anchorMin = new Vector2(0.5f, 0.5f);
            cardContRt.anchorMax = new Vector2(0.5f, 0.5f);
            cardContRt.sizeDelta = new Vector2(960, 480);
            cardContRt.anchoredPosition = new Vector2(0, -30);

            HorizontalLayoutGroup hlg = cardsContainer.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 25;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;

            SetPrivateField(uiMgr, "levelUpPanel", lvlPanel);
            SetPrivateField(uiMgr, "levelUpTitleText", lvlTitle);
            SetPrivateField(uiMgr, "levelUpSubText", lvlSub);
            SetPrivateField(uiMgr, "cardContainer", cardsContainer.transform);
            SetPrivateField(uiMgr, "cardPrefab", cardPrefab);

            // 비상 건너뛰기 버튼 (혹시 모를 UI 정지 방지용)
            GameObject skipBtnObj = CreateUIObject("SkipButton", lvlPanel.transform);
            RectTransform skipRt = skipBtnObj.GetComponent<RectTransform>();
            skipRt.anchorMin = new Vector2(0.5f, 0.5f);
            skipRt.anchorMax = new Vector2(0.5f, 0.5f);
            skipRt.sizeDelta = new Vector2(220, 50);
            skipRt.anchoredPosition = new Vector2(0, -310);
            Image skipImg = skipBtnObj.AddComponent<Image>();
            skipImg.color = new Color(0.25f, 0.22f, 0.32f, 0.85f);
            Button skipBtn = skipBtnObj.AddComponent<Button>();
            skipBtn.onClick.AddListener(uiMgr.OnSkipLevelUpClicked);
            CreateText("SkipText", skipBtnObj.transform, "⏩ 작전 계속하기", 18, TextAnchor.MiddleCenter, Vector2.zero);

            // 게임 오버 패널
            GameObject goPanel = CreateUIObject("GameOverPanel", canvasObj.transform);
            RectTransform goRt = goPanel.GetComponent<RectTransform>();
            goRt.anchorMin = Vector2.zero;
            goRt.anchorMax = Vector2.one;
            goRt.sizeDelta = Vector2.zero;

            Image goDim = goPanel.AddComponent<Image>();
            goDim.color = new Color(0.12f, 0.05f, 0.15f, 0.94f);

            CreateText("GoTitle", goPanel.transform, "📋 도파민 디톡스 종합 검진표", 44, TextAnchor.MiddleCenter, new Vector2(0, 370));
            Text goStats = CreateText("GoStats", goPanel.transform, "검진 데이터를 분석하는 중...", 22, TextAnchor.MiddleCenter, new Vector2(0, 20), new Vector2(980, 560));
            goStats.lineSpacing = 1.25f;

            GameObject restartBtnObj = CreateUIObject("RestartButton", goPanel.transform);
            RectTransform rBtnRt = restartBtnObj.GetComponent<RectTransform>();
            rBtnRt.sizeDelta = new Vector2(380, 75);
            rBtnRt.anchoredPosition = new Vector2(0, -360);
            Image btnImg = restartBtnObj.AddComponent<Image>();
            btnImg.color = new Color(0.35f, 0.75f, 0.65f);
            Button btn = restartBtnObj.AddComponent<Button>();
            btn.onClick.AddListener(uiMgr.OnRestartButtonClicked);

            CreateText("BtnText", restartBtnObj.transform, "🏠 본부로 귀환하기", 28, TextAnchor.MiddleCenter, Vector2.zero);

            SetPrivateField(uiMgr, "gameOverPanel", goPanel);
            SetPrivateField(uiMgr, "gameOverStatsText", goStats);

            // 9. 일시정지(Pause) 패널
            GameObject pausePanelObj = CreateUIObject("PausePanel", canvasObj.transform);
            RectTransform pRt = pausePanelObj.GetComponent<RectTransform>();
            pRt.anchorMin = Vector2.zero;
            pRt.anchorMax = Vector2.one;
            pRt.sizeDelta = Vector2.zero;

            Image pDim = pausePanelObj.AddComponent<Image>();
            pDim.color = new Color(0.08f, 0.06f, 0.12f, 0.94f);

            CreateText("PauseTitle", pausePanelObj.transform, "⏸️ 작전 일시정지 (PAUSED)", 44, TextAnchor.MiddleCenter, new Vector2(0, 370));

            Text pauseEquipTxt = CreateText("PauseEquipText", pausePanelObj.transform, "장비 정보를 불러오는 중...", 20, TextAnchor.MiddleCenter, new Vector2(0, 50), new Vector2(920, 520));
            pauseEquipTxt.lineSpacing = 1.3f;
            pauseEquipTxt.color = new Color(0.9f, 0.88f, 0.95f);

            // 효과음 토글 버튼
            GameObject muteBtnObj = CreateUIObject("MuteButton", pausePanelObj.transform);
            RectTransform mRt = muteBtnObj.GetComponent<RectTransform>();
            mRt.sizeDelta = new Vector2(200, 56);
            mRt.anchoredPosition = new Vector2(-220, -290);
            Image mImg = muteBtnObj.AddComponent<Image>();
            mImg.color = new Color(0.24f, 0.20f, 0.32f);
            Button mBtn = muteBtnObj.AddComponent<Button>();
            mBtn.onClick.AddListener(uiMgr.OnToggleMuteClicked);
            Text mMuteTxt = CreateText("MuteBtnText", muteBtnObj.transform, "🔊 효과음: ON", 19, TextAnchor.MiddleCenter, Vector2.zero, new Vector2(190, 48));

            // 창모드 / 전체화면 토글 버튼
            GameObject winBtnObj = CreateUIObject("WindowModeButton", pausePanelObj.transform);
            RectTransform wRt = winBtnObj.GetComponent<RectTransform>();
            wRt.sizeDelta = new Vector2(250, 56);
            wRt.anchoredPosition = new Vector2(20, -290);
            Image wImg = winBtnObj.AddComponent<Image>();
            wImg.color = new Color(0.20f, 0.26f, 0.38f);
            Button wBtn = winBtnObj.AddComponent<Button>();
            wBtn.onClick.AddListener(uiMgr.OnToggleWindowModeClicked);
            Text wTxt = CreateText("WindowBtnText", winBtnObj.transform, "🖥️ 화면 모드 (F11)", 18, TextAnchor.MiddleCenter, Vector2.zero, new Vector2(240, 48));

            // 계속하기 버튼
            GameObject resumeBtnObj = CreateUIObject("ResumeButton", pausePanelObj.transform);
            RectTransform resRt = resumeBtnObj.GetComponent<RectTransform>();
            resRt.sizeDelta = new Vector2(200, 56);
            resRt.anchoredPosition = new Vector2(260, -290);
            Image resImg = resumeBtnObj.AddComponent<Image>();
            resImg.color = new Color(0.32f, 0.72f, 0.55f);
            Button resBtn = resumeBtnObj.AddComponent<Button>();
            resBtn.onClick.AddListener(uiMgr.ResumeGame);
            CreateText("ResumeText", resumeBtnObj.transform, "▶ 계속하기", 20, TextAnchor.MiddleCenter, Vector2.zero, new Vector2(190, 48));

            // 조기 철수 버튼
            GameObject retreatBtnObj = CreateUIObject("RetreatButton", pausePanelObj.transform);
            RectTransform retRt = retreatBtnObj.GetComponent<RectTransform>();
            retRt.sizeDelta = new Vector2(360, 50);
            retRt.anchoredPosition = new Vector2(0, -365);
            Image retImg = retreatBtnObj.AddComponent<Image>();
            retImg.color = new Color(0.65f, 0.22f, 0.25f);
            Button retBtn = retreatBtnObj.AddComponent<Button>();
            retBtn.onClick.AddListener(uiMgr.OnRetreatButtonClicked);
            CreateText("RetreatText", retreatBtnObj.transform, "🏠 본부로 조기 철수 (60% 정산)", 18, TextAnchor.MiddleCenter, Vector2.zero, new Vector2(340, 45));

            SetPrivateField(uiMgr, "pausePanel", pausePanelObj);
            SetPrivateField(uiMgr, "pauseEquipmentText", pauseEquipTxt);
            SetPrivateField(uiMgr, "pauseMuteBtnText", mMuteTxt);
            SetPrivateField(uiMgr, "pauseWindowModeBtn", wBtn);
            SetPrivateField(uiMgr, "pauseWindowModeBtnText", wTxt);

            lvlPanel.SetActive(false);
            goPanel.SetActive(false);
            pausePanelObj.SetActive(false);
        }

        private static Slider CreateSimpleSlider(string name, Transform parent, Color fillColor, Vector2 pos)
        {
            GameObject sliderObj = CreateUIObject(name, parent);
            RectTransform rt = sliderObj.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(500, 24);
            rt.anchoredPosition = pos;

            Slider slider = sliderObj.AddComponent<Slider>();

            // 배경
            GameObject bgObj = CreateUIObject("Background", sliderObj.transform);
            RectTransform bgRt = bgObj.GetComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.sizeDelta = Vector2.zero;
            Image bgImg = bgObj.AddComponent<Image>();
            bgImg.color = new Color(0.9f, 0.88f, 0.86f);

            // Fill Area
            GameObject fillArea = CreateUIObject("Fill Area", sliderObj.transform);
            RectTransform faRt = fillArea.GetComponent<RectTransform>();
            faRt.anchorMin = Vector2.zero;
            faRt.anchorMax = Vector2.one;
            faRt.sizeDelta = Vector2.zero;

            GameObject fill = CreateUIObject("Fill", fillArea.transform);
            RectTransform fillRt = fill.GetComponent<RectTransform>();
            fillRt.anchorMin = Vector2.zero;
            fillRt.anchorMax = Vector2.one;
            fillRt.sizeDelta = Vector2.zero;
            Image fillImg = fill.AddComponent<Image>();
            fillImg.color = fillColor;

            slider.fillRect = fillRt;
            slider.targetGraphic = fillImg;
            slider.value = 1f;

            return slider;
        }

        private static Text CreateText(string name, Transform parent, string content, int size, TextAnchor align, Vector2 pos)
        {
            return CreateText(name, parent, content, size, align, pos, new Vector2(700, 60));
        }

        private static Text CreateText(string name, Transform parent, string content, int size, TextAnchor align, Vector2 pos, Vector2 sizeDelta)
        {
            GameObject obj = CreateUIObject(name, parent);
            RectTransform rt = obj.GetComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = sizeDelta;

            Text txt = obj.AddComponent<Text>();
            txt.text = content;
            txt.fontSize = size;
            txt.alignment = align;
            txt.color = Color.white;
            txt.font = koreanFont;
            return txt;
        }

        private static GameObject CreateUIObject(string name, Transform parent)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform));
            obj.transform.SetParent(parent, false);
            return obj;
        }

        private static GameObject CreateOrUpdateProjectilePrefab()
        {
            GameObject obj = new GameObject("Projectile");
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Weapons/bullet_clean.png");
            SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = 5;

            CircleCollider2D col = obj.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.35f;

            obj.AddComponent<Projectile>();

            string path = "Assets/Prefabs/ProjectilePrefab.prefab";
            Directory.CreateDirectory("Assets/Prefabs");
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(obj, path);
            Object.DestroyImmediate(obj);
            return prefab;
        }

        private static GameObject CreateOrUpdateSatellitePrefab()
        {
            GameObject obj = new GameObject("Satellite");
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Weapons/satellite_orbit.png");
            SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = 6;

            CircleCollider2D col = obj.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.45f;

            obj.AddComponent<OrbitSatelliteContact>();

            string path = "Assets/Prefabs/SatellitePrefab.prefab";
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(obj, path);
            Object.DestroyImmediate(obj);
            return prefab;
        }

        private static GameObject CreateOrUpdateGemPrefab()
        {
            GameObject obj = new GameObject("ExpGem");
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Items/gem_green.png");
            SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = 3;

            CircleCollider2D col = obj.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.35f;

            ExpGem gemComp = obj.AddComponent<ExpGem>();

            Sprite sGreen = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Items/gem_green.png");
            Sprite sBlue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Items/gem_blue.png");
            Sprite sPink = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Items/gem_pink.png");
            Sprite sMega = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Items/gem_mega.png");
            Sprite sCake = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Items/item_cake.png");
            Sprite sBomb = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Items/item_bomb.png");
            Sprite sMagnet = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Items/item_magnet.png");
            Sprite sChest = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Items/item_chest.png");

            gemComp.SetSprites(sGreen, sBlue, sPink, sMega, sCake, sBomb, sMagnet, sChest);

            string path = "Assets/Prefabs/GemPrefab.prefab";
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(obj, path);
            Object.DestroyImmediate(obj);
            return prefab;
        }

        private static GameObject CreateOrUpdateEnemyPrefab()
        {
            GameObject obj = new GameObject("Enemy");
            obj.tag = "Enemy";
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Enemies/enemy_dust.png");
            SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = 4;

            CircleCollider2D col = obj.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.45f;

            obj.AddComponent<Enemy>();

            string path = "Assets/Prefabs/EnemyPrefab.prefab";
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(obj, path);
            Object.DestroyImmediate(obj);
            return prefab;
        }

        private static GameObject CreateOrUpdateUpgradeCardPrefab()
        {
            GameObject cardObj = CreateUIObject("UpgradeCard", null);
            RectTransform rt = cardObj.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(280, 420);

            Image cardBg = cardObj.AddComponent<Image>();
            cardBg.color = new Color(0.98f, 0.96f, 0.94f);

            Button btn = cardObj.AddComponent<Button>();
            UpgradeCardUI cardUI = cardObj.AddComponent<UpgradeCardUI>();

            // 아이콘
            Text icon = CreateText("CardIcon", cardObj.transform, "💧", 52, TextAnchor.MiddleCenter, new Vector2(0, 95));
            RectTransform iconRt = icon.GetComponent<RectTransform>();
            iconRt.sizeDelta = new Vector2(80, 80);

            // 스킬 이름
            Text title = CreateText("CardTitle", cardObj.transform, "스킬 이름", 22, TextAnchor.MiddleCenter, new Vector2(0, 25));
            RectTransform titleRt = title.GetComponent<RectTransform>();
            titleRt.sizeDelta = new Vector2(250, 40);
            title.color = new Color(0.18f, 0.12f, 0.24f);
            title.fontStyle = FontStyle.Bold;

            // 스킬 설명 (카드 너비 280 안에서 여유있게 줄바꿈되도록 240 너비 지정)
            Text desc = CreateText("CardDesc", cardObj.transform, "스킬 설명이 여기에 표시됩니다.", 16, TextAnchor.UpperCenter, new Vector2(0, -90));
            RectTransform descRt = desc.GetComponent<RectTransform>();
            descRt.sizeDelta = new Vector2(240, 130);
            desc.color = new Color(0.32f, 0.28f, 0.38f);
            desc.horizontalOverflow = HorizontalWrapMode.Wrap;
            desc.verticalOverflow = VerticalWrapMode.Truncate;
            desc.lineSpacing = 1.2f;

            // 진화 뱃지
            GameObject badgeObj = CreateUIObject("EvoBadge", cardObj.transform);
            RectTransform bRt = badgeObj.GetComponent<RectTransform>();
            bRt.sizeDelta = new Vector2(120, 28);
            bRt.anchoredPosition = new Vector2(0, 165);
            Image bImg = badgeObj.AddComponent<Image>();
            bImg.color = new Color(1f, 0.78f, 0.22f);
            Text bText = CreateText("BadgeText", badgeObj.transform, "EVOLUTION", 12, TextAnchor.MiddleCenter, Vector2.zero);
            RectTransform bTextRt = bText.GetComponent<RectTransform>();
            bTextRt.sizeDelta = new Vector2(115, 26);
            bText.color = new Color(0.35f, 0.18f, 0.02f);
            bText.fontStyle = FontStyle.Bold;

            SetPrivateField(cardUI, "iconText", icon);
            SetPrivateField(cardUI, "titleText", title);
            SetPrivateField(cardUI, "descriptionText", desc);
            SetPrivateField(cardUI, "evoBadge", badgeObj);
            SetPrivateField(cardUI, "selectButton", btn);

            string path = "Assets/Prefabs/UpgradeCardPrefab.prefab";
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(cardObj, path);
            Object.DestroyImmediate(cardObj);
            return prefab;
        }

        private static void EnsureTagsExist()
        {
            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty tagsProp = tagManager.FindProperty("tags");

            AddTagIfNotExists(tagsProp, "Enemy");
            tagManager.ApplyModifiedProperties();
        }

        private static void AddTagIfNotExists(SerializedProperty tagsProp, string tag)
        {
            for (int i = 0; i < tagsProp.arraySize; i++)
            {
                if (tagsProp.GetArrayElementAtIndex(i).stringValue.Equals(tag))
                    return;
            }
            tagsProp.InsertArrayElementAtIndex(tagsProp.arraySize);
            tagsProp.GetArrayElementAtIndex(tagsProp.arraySize - 1).stringValue = tag;
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
                field.SetValue(target, value);
        }
    }
}
