using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
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
        [MenuItem("Dopamine Cleaner/✨ Build Complete Gameplay Scene (원클릭 자동 구성)")]
        public static void BuildScene()
        {
            EnsureTagsExist();

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
            cam.backgroundColor = new Color(0.98f, 0.96f, 0.94f); // 포근한 바닐라 크림 배경
            cam.clearFlags = CameraClearFlags.SolidColor;
            camObj.AddComponent<AudioListener>();
            CameraFollow camFollow = camObj.AddComponent<CameraFollow>();

            // 4. 플레이어 생성
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

            // 5. 게임 매니저 & 스포너 & 아이템 매니저
            GameObject sysObj = new GameObject("[Systems]");
            GameManager gameMgr = sysObj.AddComponent<GameManager>();

            EnemySpawner spawner = sysObj.AddComponent<EnemySpawner>();
            SetPrivateField(spawner, "enemyPrefab", enemyPrefab);

            ItemManager itemMgr = sysObj.AddComponent<ItemManager>();
            SetPrivateField(itemMgr, "gemPrefab", gemPrefab);

            // 6. UI 캔버스 및 HUD 생성
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

            // 7. 씬 저장
            Directory.CreateDirectory("Assets/Scenes");
            string scenePath = "Assets/Scenes/MainGame.unity";
            EditorSceneManager.SaveScene(scene, scenePath);

            // Build Settings에 씬 등록
            EditorBuildSettingsScene[] scenes = new EditorBuildSettingsScene[]
            {
                new EditorBuildSettingsScene(scenePath, true)
            };
            EditorBuildSettings.scenes = scenes;

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("도파민 클리너", "✨ 게임플레이 씬(MainGame.unity) 및 전체 시스템 구축이 완료되었습니다!\n상단 [▶ 플레이] 버튼을 누르면 바로 게임을 플레이하실 수 있습니다.", "확인");
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
            TextMeshProUGUI lvlText = CreateText("LevelText", topHud.transform, "Lv.1", 28, TextAlignmentOptions.Left, new Vector2(-200, 40));
            SetPrivateField(uiMgr, "levelText", lvlText);

            // 타이머 텍스트
            TextMeshProUGUI timerText = CreateText("TimerText", topHud.transform, "00:00", 36, TextAlignmentOptions.Center, new Vector2(0, -60));
            SetPrivateField(uiMgr, "timerText", timerText);

            // 킬/스테이지 텍스트
            TextMeshProUGUI killsText = CreateText("KillsText", topHud.transform, "STAGE 1 [0/35]  👾 0", 26, TextAlignmentOptions.Center, new Vector2(0, -100));
            SetPrivateField(uiMgr, "stageKillsText", killsText);

            // 하단 대시 쿨타임 텍스트
            GameObject bottomHud = CreateUIObject("BottomHUD", canvasObj.transform);
            RectTransform botRt = bottomHud.GetComponent<RectTransform>();
            botRt.anchorMin = new Vector2(0f, 0f);
            botRt.anchorMax = new Vector2(1f, 0f);
            botRt.pivot = new Vector2(0.5f, 0f);
            botRt.sizeDelta = new Vector2(0, 80);
            botRt.anchoredPosition = Vector2.zero;

            TextMeshProUGUI dashText = CreateText("DashText", bottomHud.transform, "💨 대시 준비완료 (Space)", 26, TextAlignmentOptions.Center, new Vector2(0, 30));
            SetPrivateField(uiMgr, "dashCooldownText", dashText);

            // 레벨업 패널
            GameObject lvlPanel = CreateUIObject("LevelUpPanel", canvasObj.transform);
            RectTransform lvlRt = lvlPanel.GetComponent<RectTransform>();
            lvlRt.anchorMin = Vector2.zero;
            lvlRt.anchorMax = Vector2.one;
            lvlRt.sizeDelta = Vector2.zero;

            Image dimImg = lvlPanel.AddComponent<Image>();
            dimImg.color = new Color(0.1f, 0.08f, 0.15f, 0.85f);

            CreateText("LvlUpTitle", lvlPanel.transform, "🎉 레벨 업!", 52, TextAlignmentOptions.Center, new Vector2(0, 350));
            CreateText("LvlUpSub", lvlPanel.transform, "원하는 도파민 디톡스 능력을 선택하세요", 26, TextAlignmentOptions.Center, new Vector2(0, 280));

            GameObject cardsContainer = CreateUIObject("CardsContainer", lvlPanel.transform);
            RectTransform cardContRt = cardsContainer.GetComponent<RectTransform>();
            cardContRt.anchorMin = new Vector2(0.5f, 0.5f);
            cardContRt.anchorMax = new Vector2(0.5f, 0.5f);
            cardContRt.sizeDelta = new Vector2(900, 480);
            cardContRt.anchoredPosition = new Vector2(0, -40);

            HorizontalLayoutGroup hlg = cardsContainer.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 30;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;

            SetPrivateField(uiMgr, "levelUpPanel", lvlPanel);
            SetPrivateField(uiMgr, "cardContainer", cardsContainer.transform);
            SetPrivateField(uiMgr, "cardPrefab", cardPrefab);

            // 게임 오버 패널
            GameObject goPanel = CreateUIObject("GameOverPanel", canvasObj.transform);
            RectTransform goRt = goPanel.GetComponent<RectTransform>();
            goRt.anchorMin = Vector2.zero;
            goRt.anchorMax = Vector2.one;
            goRt.sizeDelta = Vector2.zero;

            Image goDim = goPanel.AddComponent<Image>();
            goDim.color = new Color(0.12f, 0.05f, 0.15f, 0.92f);

            CreateText("GoTitle", goPanel.transform, "디톡스 세션 종료", 56, TextAlignmentOptions.Center, new Vector2(0, 240));
            TextMeshProUGUI goStats = CreateText("GoStats", goPanel.transform, "통계 정보", 30, TextAlignmentOptions.Center, new Vector2(0, 60));

            GameObject restartBtnObj = CreateUIObject("RestartButton", goPanel.transform);
            RectTransform rBtnRt = restartBtnObj.GetComponent<RectTransform>();
            rBtnRt.sizeDelta = new Vector2(360, 80);
            rBtnRt.anchoredPosition = new Vector2(0, -160);
            Image btnImg = restartBtnObj.AddComponent<Image>();
            btnImg.color = new Color(1f, 0.46f, 0.56f);
            Button btn = restartBtnObj.AddComponent<Button>();
            btn.onClick.AddListener(uiMgr.OnRestartButtonClicked);

            CreateText("BtnText", restartBtnObj.transform, "🔄 다시 도전하기", 32, TextAlignmentOptions.Center, Vector2.zero);

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

        private static TextMeshProUGUI CreateText(string name, Transform parent, string text, float size, TextAlignmentOptions align, Vector2 pos)
        {
            GameObject obj = CreateUIObject(name, parent);
            RectTransform rt = obj.GetComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(700, 60);

            TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = size;
            tmp.alignment = align;
            tmp.color = Color.white;
            return tmp;
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
            col.radius = 0.3f;

            obj.AddComponent<ExpGem>();

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
            rt.sizeDelta = new Vector2(260, 420);

            Image cardBg = cardObj.AddComponent<Image>();
            cardBg.color = new Color(0.98f, 0.95f, 0.92f);

            Button btn = cardObj.AddComponent<Button>();
            UpgradeCardUI cardUI = cardObj.AddComponent<UpgradeCardUI>();

            TextMeshProUGUI icon = CreateText("CardIcon", cardObj.transform, "💧", 64, TextAlignmentOptions.Center, new Vector2(0, 110));
            TextMeshProUGUI title = CreateText("CardTitle", cardObj.transform, "스킬 이름", 26, TextAlignmentOptions.Center, new Vector2(0, 20));
            title.color = new Color(0.2f, 0.15f, 0.25f);

            TextMeshProUGUI desc = CreateText("CardDesc", cardObj.transform, "스킬 설명이 여기에 표시됩니다.", 18, TextAlignmentOptions.Center, new Vector2(0, -90));
            desc.color = new Color(0.45f, 0.4f, 0.5f);

            GameObject badgeObj = CreateUIObject("EvoBadge", cardObj.transform);
            RectTransform bRt = badgeObj.GetComponent<RectTransform>();
            bRt.sizeDelta = new Vector2(100, 30);
            bRt.anchoredPosition = new Vector2(0, 180);
            Image bImg = badgeObj.AddComponent<Image>();
            bImg.color = new Color(1f, 0.8f, 0.2f);
            TextMeshProUGUI bText = CreateText("BadgeText", badgeObj.transform, "EVOLUTION", 14, TextAlignmentOptions.Center, Vector2.zero);
            bText.color = new Color(0.3f, 0.15f, 0.0f);

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
