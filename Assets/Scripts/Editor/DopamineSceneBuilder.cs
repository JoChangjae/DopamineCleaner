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

            WeaponManager weaponMgr = weaponsObj.AddComponent<WeaponManager>();
            SetPrivateField(weaponMgr, "cleanBulletWeapon", bulletWeapon);
            SetPrivateField(weaponMgr, "orbitWeapon", orbitWeapon);

            // 6. 게임 매니저 & 스포너 & 아이템 매니저
            GameObject sysObj = new GameObject("[Systems]");
            GameManager gameMgr = sysObj.AddComponent<GameManager>();

            EnemySpawner spawner = sysObj.AddComponent<EnemySpawner>();
            SetPrivateField(spawner, "enemyPrefab", enemyPrefab);

            ItemManager itemMgr = sysObj.AddComponent<ItemManager>();
            SetPrivateField(itemMgr, "gemPrefab", gemPrefab);

            // 7. UI 캔버스 및 HUD 생성 (한글 100% 완벽 출력 표준 uGUI Text)
            GameObject canvasObj = new GameObject("Canvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            canvasObj.AddComponent<GraphicRaycaster>();

            // EventSystem
            GameObject esObj = new GameObject("EventSystem");
            esObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

            // UI Manager
            UIManager uiMgr = canvasObj.AddComponent<UIManager>();
            BuildCanvasUI(canvasObj, uiMgr, cardPrefab);

            // 8. 씬 저장
            Directory.CreateDirectory("Assets/Scenes");
            string scenePath = "Assets/Scenes/MainGame.unity";
            EditorSceneManager.SaveScene(scene, scenePath);

            EditorBuildSettingsScene[] scenes = new EditorBuildSettingsScene[]
            {
                new EditorBuildSettingsScene(scenePath, true)
            };
            EditorBuildSettings.scenes = scenes;

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("도파민 클리너", "✨ 도트 배경 및 한글 폰트가 완벽히 적용된 씬(MainGame.unity) 구축이 완료되었습니다!\n상단 [▶ 플레이] 버튼을 누르면 바로 게임을 즐기실 수 있습니다.", "확인");
        }

        private static void LoadKoreanFont()
        {
            koreanFont = AssetDatabase.LoadAssetAtPath<Font>("Assets/Fonts/MalgunGothic.ttf");
            if (koreanFont == null)
            {
                koreanFont = Font.CreateDynamicFontFromOSFont(new string[] { "Malgun Gothic", "맑은 고딕", "Arial" }, 24);
            }
        }

        private static void BuildCanvasUI(GameObject canvasObj, UIManager uiMgr, GameObject cardPrefab)
        {
            // 상단 레이아웃
            GameObject topHud = CreateUIObject("TopHUD", canvasObj.transform);
            RectTransform topRt = topHud.GetComponent<RectTransform>();
            topRt.anchorMin = new Vector2(0f, 1f);
            topRt.anchorMax = new Vector2(1f, 1f);
            topRt.pivot = new Vector2(0.5f, 1f);
            topRt.sizeDelta = new Vector2(-40, 140);
            topRt.anchoredPosition = new Vector2(0, -20);

            // 경험치 바 (최상단)
            Slider expSlider = CreateSimpleSlider("ExpBar", topHud.transform, new Color(1f, 0.46f, 0.56f), new Vector2(0, 40));
            SetPrivateField(uiMgr, "expSlider", expSlider);

            // 체력 바
            Slider hpSlider = CreateSimpleSlider("HpBar", topHud.transform, new Color(0.32f, 0.72f, 0.53f), new Vector2(0, 0));
            SetPrivateField(uiMgr, "hpSlider", hpSlider);

            // 레벨 텍스트
            Text lvlText = CreateText("LevelText", topHud.transform, "Lv.1", 24, TextAnchor.MiddleLeft, new Vector2(-200, 40));
            SetPrivateField(uiMgr, "levelText", lvlText);

            // 타이머 텍스트
            Text timerText = CreateText("TimerText", topHud.transform, "00:00", 34, TextAnchor.MiddleCenter, new Vector2(0, -60));
            SetPrivateField(uiMgr, "timerText", timerText);

            // 킬/스테이지 텍스트
            Text killsText = CreateText("KillsText", topHud.transform, "STAGE 1 [0/35]  👾 0", 24, TextAnchor.MiddleCenter, new Vector2(0, -100));
            SetPrivateField(uiMgr, "stageKillsText", killsText);

            // 하단 대시 쿨타임 텍스트
            GameObject bottomHud = CreateUIObject("BottomHUD", canvasObj.transform);
            RectTransform botRt = bottomHud.GetComponent<RectTransform>();
            botRt.anchorMin = new Vector2(0f, 0f);
            botRt.anchorMax = new Vector2(1f, 0f);
            botRt.pivot = new Vector2(0.5f, 0f);
            botRt.sizeDelta = new Vector2(0, 80);
            botRt.anchoredPosition = Vector2.zero;

            Text dashText = CreateText("DashText", bottomHud.transform, "💨 대시 준비완료 (Space / Shift)", 24, TextAnchor.MiddleCenter, new Vector2(0, 30));
            SetPrivateField(uiMgr, "dashCooldownText", dashText);

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

            // 게임 오버 패널
            GameObject goPanel = CreateUIObject("GameOverPanel", canvasObj.transform);
            RectTransform goRt = goPanel.GetComponent<RectTransform>();
            goRt.anchorMin = Vector2.zero;
            goRt.anchorMax = Vector2.one;
            goRt.sizeDelta = Vector2.zero;

            Image goDim = goPanel.AddComponent<Image>();
            goDim.color = new Color(0.12f, 0.05f, 0.15f, 0.94f);

            CreateText("GoTitle", goPanel.transform, "디톡스 세션 종료", 52, TextAnchor.MiddleCenter, new Vector2(0, 240));
            Text goStats = CreateText("GoStats", goPanel.transform, "통계 정보", 28, TextAnchor.MiddleCenter, new Vector2(0, 60));

            GameObject restartBtnObj = CreateUIObject("RestartButton", goPanel.transform);
            RectTransform rBtnRt = restartBtnObj.GetComponent<RectTransform>();
            rBtnRt.sizeDelta = new Vector2(360, 80);
            rBtnRt.anchoredPosition = new Vector2(0, -160);
            Image btnImg = restartBtnObj.AddComponent<Image>();
            btnImg.color = new Color(1f, 0.46f, 0.56f);
            Button btn = restartBtnObj.AddComponent<Button>();
            btn.onClick.AddListener(uiMgr.OnRestartButtonClicked);

            CreateText("BtnText", restartBtnObj.transform, "🔄 다시 도전하기", 30, TextAnchor.MiddleCenter, Vector2.zero);

            SetPrivateField(uiMgr, "gameOverPanel", goPanel);
            SetPrivateField(uiMgr, "gameOverStatsText", goStats);

            lvlPanel.SetActive(false);
            goPanel.SetActive(false);
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
            GameObject obj = CreateUIObject(name, parent);
            RectTransform rt = obj.GetComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(700, 60);

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
