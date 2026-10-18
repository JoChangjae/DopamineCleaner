using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using DopamineCleaner.Meta;
using DopamineCleaner.Town;

namespace DopamineCleaner.Editor
{
    public static class TownSceneBuilder
    {
        private static Font koreanFont;

        [MenuItem("Dopamine Cleaner/🏢 Build R.I.P Town Scene")]
        public static void BuildTownScene()
        {
            LoadKoreanFont();

            // 1. 프리팹 생성/갱신
            GameObject charCardPrefab = CreateOrUpdateCharacterCardPrefab();
            GameObject shopItemPrefab = CreateOrUpdateShopItemPrefab();
            GameObject sellItemPrefab = CreateOrUpdateSellItemPrefab();
            GameObject storageItemPrefab = CreateOrUpdateStorageItemPrefab();

            // 2. 새 씬 생성
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 3. 카메라 생성
            GameObject camObj = new GameObject("Main Camera");
            camObj.tag = "MainCamera";
            Camera cam = camObj.AddComponent<Camera>();
            cam.orthographic = true;
            cam.backgroundColor = new Color(0.11f, 0.09f, 0.15f); // 딥 다크 오피스 퍼플
            cam.clearFlags = CameraClearFlags.SolidColor;
            camObj.AddComponent<AudioListener>();

            // 4. GameDataManager 생성
            GameObject gdmObj = new GameObject("[GameDataManager]");
            gdmObj.AddComponent<GameDataManager>();

            // 5. 캔버스 및 UI 생성
            GameObject canvasObj = new GameObject("TownCanvas");
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

            // TownManager
            TownManager townMgr = canvasObj.AddComponent<TownManager>();

            // 배경 패널
            GameObject bgObj = CreateUIObject("TownBackground", canvasObj.transform);
            RectTransform bgRt = bgObj.GetComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.sizeDelta = Vector2.zero;
            Image bgImg = bgObj.AddComponent<Image>();
            bgImg.color = new Color(0.12f, 0.10f, 0.16f);

            // 상단 헤더 (회사명 및 재화/사원증)
            GameObject headerObj = CreateUIObject("HeaderBar", canvasObj.transform);
            RectTransform hRt = headerObj.GetComponent<RectTransform>();
            hRt.anchorMin = new Vector2(0f, 1f);
            hRt.anchorMax = new Vector2(1f, 1f);
            hRt.pivot = new Vector2(0.5f, 1f);
            hRt.sizeDelta = new Vector2(-40, 200);
            hRt.anchoredPosition = new Vector2(0, -20);

            Image hBg = headerObj.AddComponent<Image>();
            hBg.color = new Color(0.18f, 0.15f, 0.24f);

            CreateText("CorpTitle", headerObj.transform, "🏢 윤회 보험사 R.I.P (본사)", 36, TextAnchor.MiddleLeft, new Vector2(-20, 50), new Vector2(500, 50));
            Text goldTxt = CreateText("GoldText", headerObj.transform, "💰 보유 골드: 600 G", 26, TextAnchor.MiddleRight, new Vector2(20, 50), new Vector2(400, 50));
            goldTxt.color = new Color(1f, 0.85f, 0.3f);

            Text karmaTxt = CreateText("KarmaText", headerObj.transform, "✨ 윤회 포인트: 1 P", 26, TextAnchor.MiddleRight, new Vector2(20, -5), new Vector2(400, 50));
            karmaTxt.color = new Color(0.6f, 0.85f, 1f);

            Text activeCharTxt = CreateText("ActiveCharText", headerObj.transform, "사원증: 🧑‍💼 김인턴 (신입 수습사원)", 24, TextAnchor.MiddleLeft, new Vector2(-20, -5), new Vector2(550, 50));
            activeCharTxt.color = new Color(0.85f, 0.8f, 0.95f);

            SetPrivateField(townMgr, "goldText", goldTxt);
            SetPrivateField(townMgr, "karmaText", karmaTxt);
            SetPrivateField(townMgr, "activeCharacterText", activeCharTxt);

            // 상단 탭 네비게이션 버튼 (4개)
            GameObject tabNavObj = CreateUIObject("TabNavBar", canvasObj.transform);
            RectTransform tabRt = tabNavObj.GetComponent<RectTransform>();
            tabRt.anchorMin = new Vector2(0.5f, 1f);
            tabRt.anchorMax = new Vector2(0.5f, 1f);
            tabRt.pivot = new Vector2(0.5f, 1f);
            tabRt.sizeDelta = new Vector2(1000, 70);
            tabRt.anchoredPosition = new Vector2(0, -235);

            HorizontalLayoutGroup tabHlg = tabNavObj.AddComponent<HorizontalLayoutGroup>();
            tabHlg.spacing = 15;
            tabHlg.childAlignment = TextAnchor.MiddleCenter;
            tabHlg.childControlWidth = true;
            tabHlg.childControlHeight = true;

            Button tabChar = CreateButton("TabChar", tabNavObj.transform, "🧑‍💼 사원 선택소", 24, new Color(0.28f, 0.24f, 0.38f));
            Button tabShop = CreateButton("TabShop", tabNavObj.transform, "🛒 사내 매점", 24, new Color(0.28f, 0.24f, 0.38f));
            Button tabStorage = CreateButton("TabStorage", tabNavObj.transform, "🗄️ 개인 사물함", 24, new Color(0.28f, 0.24f, 0.38f));
            Button tabDungeon = CreateButton("TabDungeon", tabNavObj.transform, "🚀 현장 출장 접수처", 24, new Color(0.85f, 0.35f, 0.38f));

            SetPrivateField(townMgr, "tabCharBtn", tabChar);
            SetPrivateField(townMgr, "tabShopBtn", tabShop);
            SetPrivateField(townMgr, "tabStorageBtn", tabStorage);
            SetPrivateField(townMgr, "tabDungeonBtn", tabDungeon);

            // 메인 콘텐츠 영역 (중앙)
            GameObject contentArea = CreateUIObject("ContentArea", canvasObj.transform);
            RectTransform contentRt = contentArea.GetComponent<RectTransform>();
            contentRt.anchorMin = new Vector2(0f, 0f);
            contentRt.anchorMax = new Vector2(1f, 1f);
            contentRt.offsetMin = new Vector2(40, 40);
            contentRt.offsetMax = new Vector2(-40, -320);

            // 1. 캐릭터 선택 패널
            GameObject charPanel = CreateCharacterSelectPanel(contentArea.transform, charCardPrefab);
            SetPrivateField(townMgr, "characterPanel", charPanel);

            // 2. 상점 패널
            GameObject shopPanel = CreateShopPanel(contentArea.transform, shopItemPrefab, sellItemPrefab);
            SetPrivateField(townMgr, "shopPanel", shopPanel);

            // 3. 창고 패널
            GameObject storagePanel = CreateStoragePanel(contentArea.transform, storageItemPrefab);
            SetPrivateField(townMgr, "storagePanel", storagePanel);

            // 4. 던전 출장 접수처 패널
            GameObject dungeonPanel = CreateDungeonSelectPanel(contentArea.transform);
            SetPrivateField(townMgr, "dungeonPanel", dungeonPanel);

            // 씬 저장
            Directory.CreateDirectory("Assets/Scenes");
            string scenePath = "Assets/Scenes/TownScene.unity";
            EditorSceneManager.SaveScene(scene, scenePath);

            // Build Settings에 등록 (TownScene이 0번, MainGame이 1번)
            EditorBuildSettingsScene[] scenes = new EditorBuildSettingsScene[]
            {
                new EditorBuildSettingsScene("Assets/Scenes/TownScene.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/MainGame.unity", true)
            };
            EditorBuildSettings.scenes = scenes;

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("윤회 보험사 R.I.P", "🏢 'TownScene.unity'(마을 본사 씬) 구축이 성공적으로 완료되었습니다!\n상단 [▶ 플레이] 버튼을 누르면 사원 선택/상점/창고/출장 시스템을 즐기실 수 있습니다.", "확인");
        }

        private static GameObject CreateCharacterSelectPanel(Transform parent, GameObject cardPrefab)
        {
            GameObject panel = CreateUIObject("CharacterSelectPanel", parent);
            RectTransform rt = panel.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;

            CharacterSelectUI ui = panel.AddComponent<CharacterSelectUI>();

            GameObject container = CreateUIObject("CardContainer", panel.transform);
            RectTransform cRt = container.GetComponent<RectTransform>();
            cRt.anchorMin = new Vector2(0.5f, 0.5f);
            cRt.anchorMax = new Vector2(0.5f, 0.5f);
            cRt.sizeDelta = new Vector2(980, 1100);

            GridLayoutGroup glg = container.AddComponent<GridLayoutGroup>();
            glg.cellSize = new Vector2(460, 520);
            glg.spacing = new Vector2(40, 40);
            glg.childAlignment = TextAnchor.MiddleCenter;

            SetPrivateField(ui, "cardContainer", container.transform);
            SetPrivateField(ui, "characterCardPrefab", cardPrefab);

            return panel;
        }

        private static GameObject CreateShopPanel(Transform parent, GameObject buyPrefab, GameObject sellPrefab)
        {
            GameObject panel = CreateUIObject("ShopPanel", parent);
            RectTransform rt = panel.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;

            ShopUI ui = panel.AddComponent<ShopUI>();

            // 상품 목록 (상단/중앙)
            CreateText("ShopTitle", panel.transform, "🛒 사내 보급 매점 (구매 가능 품목)", 28, TextAnchor.MiddleLeft, new Vector2(0, 520), new Vector2(900, 40));

            GameObject shopCont = CreateUIObject("ShopItemContainer", panel.transform);
            RectTransform sRt = shopCont.GetComponent<RectTransform>();
            sRt.anchorMin = new Vector2(0.5f, 0.5f);
            sRt.anchorMax = new Vector2(0.5f, 0.5f);
            sRt.sizeDelta = new Vector2(980, 720);
            sRt.anchoredPosition = new Vector2(0, 120);

            GridLayoutGroup sGlg = shopCont.AddComponent<GridLayoutGroup>();
            sGlg.cellSize = new Vector2(460, 220);
            sGlg.spacing = new Vector2(30, 25);
            sGlg.childAlignment = TextAnchor.UpperCenter;

            // 판매 목록 (하단)
            CreateText("SellTitle", panel.transform, "💰 소지품 매각 환급 (가방 아이템)", 26, TextAnchor.MiddleLeft, new Vector2(0, -280), new Vector2(900, 40));

            GameObject sellCont = CreateUIObject("SellItemContainer", panel.transform);
            RectTransform sellRt = sellCont.GetComponent<RectTransform>();
            sellRt.anchorMin = new Vector2(0.5f, 0.5f);
            sellRt.anchorMax = new Vector2(0.5f, 0.5f);
            sellRt.sizeDelta = new Vector2(980, 220);
            sellRt.anchoredPosition = new Vector2(0, -420);

            HorizontalLayoutGroup sellHlg = sellCont.AddComponent<HorizontalLayoutGroup>();
            sellHlg.spacing = 20;
            sellHlg.childAlignment = TextAnchor.MiddleCenter;
            sellHlg.childControlWidth = false;
            sellHlg.childControlHeight = false;

            SetPrivateField(ui, "shopItemContainer", shopCont.transform);
            SetPrivateField(ui, "shopItemPrefab", buyPrefab);
            SetPrivateField(ui, "inventorySellContainer", sellCont.transform);
            SetPrivateField(ui, "sellItemPrefab", sellPrefab);

            return panel;
        }

        private static GameObject CreateStoragePanel(Transform parent, GameObject itemPrefab)
        {
            GameObject panel = CreateUIObject("StoragePanel", parent);
            RectTransform rt = panel.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;

            StorageUI ui = panel.AddComponent<StorageUI>();

            // 좌측: 출격 가방
            Text invCap = CreateText("InvCap", panel.transform, "🎒 출격 가방 (0 / 6)", 28, TextAnchor.MiddleCenter, new Vector2(-260, 520), new Vector2(440, 40));
            GameObject invCont = CreateUIObject("InvContainer", panel.transform);
            RectTransform iRt = invCont.GetComponent<RectTransform>();
            iRt.anchorMin = new Vector2(0.5f, 0.5f);
            iRt.anchorMax = new Vector2(0.5f, 0.5f);
            iRt.sizeDelta = new Vector2(460, 1000);
            iRt.anchoredPosition = new Vector2(-250, -20);

            VerticalLayoutGroup iVlg = invCont.AddComponent<VerticalLayoutGroup>();
            iVlg.spacing = 15;
            iVlg.childAlignment = TextAnchor.UpperCenter;
            iVlg.childControlWidth = true;
            iVlg.childControlHeight = false;

            // 우측: 개인 사물함 창고
            Text stashCap = CreateText("StashCap", panel.transform, "🗄️ 개인 사물함 (0 / 24)", 28, TextAnchor.MiddleCenter, new Vector2(260, 520), new Vector2(440, 40));
            GameObject stashCont = CreateUIObject("StashContainer", panel.transform);
            RectTransform sRt = stashCont.GetComponent<RectTransform>();
            sRt.anchorMin = new Vector2(0.5f, 0.5f);
            sRt.anchorMax = new Vector2(0.5f, 0.5f);
            sRt.sizeDelta = new Vector2(460, 1000);
            sRt.anchoredPosition = new Vector2(250, -20);

            VerticalLayoutGroup sVlg = stashCont.AddComponent<VerticalLayoutGroup>();
            sVlg.spacing = 15;
            sVlg.childAlignment = TextAnchor.UpperCenter;
            sVlg.childControlWidth = true;
            sVlg.childControlHeight = false;

            SetPrivateField(ui, "inventoryCapacityText", invCap);
            SetPrivateField(ui, "inventoryContainer", invCont.transform);
            SetPrivateField(ui, "storageItemPrefab", itemPrefab);
            SetPrivateField(ui, "stashCapacityText", stashCap);
            SetPrivateField(ui, "stashContainer", stashCont.transform);

            return panel;
        }

        private static GameObject CreateDungeonSelectPanel(Transform parent)
        {
            GameObject panel = CreateUIObject("DungeonSelectPanel", parent);
            RectTransform rt = panel.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;

            DungeonSelectUI ui = panel.AddComponent<DungeonSelectUI>();

            // 1. 맵 선택 헤더 & 버튼들
            CreateText("MapHeader", panel.transform, "📍 출장 사고 현장 선택", 30, TextAnchor.MiddleCenter, new Vector2(0, 510), new Vector2(900, 45));

            GameObject mapBtnGroup = CreateUIObject("MapBtnGroup", panel.transform);
            RectTransform mRt = mapBtnGroup.GetComponent<RectTransform>();
            mRt.anchorMin = new Vector2(0.5f, 0.5f);
            mRt.anchorMax = new Vector2(0.5f, 0.5f);
            mRt.sizeDelta = new Vector2(960, 110);
            mRt.anchoredPosition = new Vector2(0, 410);

            HorizontalLayoutGroup mHlg = mapBtnGroup.AddComponent<HorizontalLayoutGroup>();
            mHlg.spacing = 20;
            mHlg.childAlignment = TextAnchor.MiddleCenter;
            mHlg.childControlWidth = true;
            mHlg.childControlHeight = true;

            Button mapHosp = CreateButton("BtnHosp", mapBtnGroup.transform, "🏥 유령 병원\n(5분 생존)", 22, new Color(0.24f, 0.32f, 0.38f));
            Button mapOff = CreateButton("BtnOff", mapBtnGroup.transform, "🏢 지하 오피스\n(7분 생존)", 22, new Color(0.32f, 0.25f, 0.4f));
            Button mapHigh = CreateButton("BtnHigh", mapBtnGroup.transform, "🛣️ 도파민 고속도로\n(10분 생존)", 22, new Color(0.42f, 0.22f, 0.25f));

            // 2. 난이도 선택 헤더 & 버튼들
            CreateText("DiffHeader", panel.transform, "⚡ 출장 위험 등급 (난이도 설정)", 30, TextAnchor.MiddleCenter, new Vector2(0, 270), new Vector2(900, 45));

            GameObject diffBtnGroup = CreateUIObject("DiffBtnGroup", panel.transform);
            RectTransform dRt = diffBtnGroup.GetComponent<RectTransform>();
            dRt.anchorMin = new Vector2(0.5f, 0.5f);
            dRt.anchorMax = new Vector2(0.5f, 0.5f);
            dRt.sizeDelta = new Vector2(960, 80);
            dRt.anchoredPosition = new Vector2(0, 190);

            HorizontalLayoutGroup dHlg = diffBtnGroup.AddComponent<HorizontalLayoutGroup>();
            dHlg.spacing = 15;
            dHlg.childAlignment = TextAnchor.MiddleCenter;
            dHlg.childControlWidth = true;
            dHlg.childControlHeight = true;

            Button btnEasy = CreateButton("BtnEasy", diffBtnGroup.transform, "Easy (0.8x)", 22, new Color(0.25f, 0.55f, 0.35f));
            Button btnNorm = CreateButton("BtnNorm", diffBtnGroup.transform, "Normal (1.0x)", 22, new Color(0.25f, 0.45f, 0.75f));
            Button btnHard = CreateButton("BtnHard", diffBtnGroup.transform, "Hard (1.5x)", 22, new Color(0.75f, 0.45f, 0.2f));
            Button btnNight = CreateButton("BtnNight", diffBtnGroup.transform, "Nightmare (2.2x)", 22, new Color(0.75f, 0.15f, 0.25f));

            // 3. 브리핑 정보 창 (카드 형태)
            GameObject briefingObj = CreateUIObject("BriefingBox", panel.transform);
            RectTransform bRt = briefingObj.GetComponent<RectTransform>();
            bRt.anchorMin = new Vector2(0.5f, 0.5f);
            bRt.anchorMax = new Vector2(0.5f, 0.5f);
            bRt.sizeDelta = new Vector2(940, 360);
            bRt.anchoredPosition = new Vector2(0, -60);
            Image bImg = briefingObj.AddComponent<Image>();
            bImg.color = new Color(0.20f, 0.17f, 0.27f);

            Text mapTitle = CreateText("MapTitle", briefingObj.transform, "🏥 버려진 성모 유령병원", 28, TextAnchor.MiddleLeft, new Vector2(0, 120), new Vector2(880, 40));
            mapTitle.color = new Color(1f, 0.9f, 0.5f);
            mapTitle.fontStyle = FontStyle.Bold;

            Text mapDesc = CreateText("MapDesc", briefingObj.transform, "사고 현장 설명", 22, TextAnchor.UpperLeft, new Vector2(0, 40), new Vector2(880, 90));
            mapDesc.color = new Color(0.85f, 0.82f, 0.9f);

            Text diffSummary = CreateText("DiffSummary", briefingObj.transform, "난이도 계수 정보", 20, TextAnchor.UpperLeft, new Vector2(0, -50), new Vector2(880, 70));
            diffSummary.color = new Color(0.6f, 0.85f, 1f);

            Text rewardPreview = CreateText("RewardPreview", briefingObj.transform, "클리어 예상 보상", 24, TextAnchor.MiddleLeft, new Vector2(0, -125), new Vector2(880, 45));
            rewardPreview.color = new Color(0.4f, 1f, 0.6f);
            rewardPreview.fontStyle = FontStyle.Bold;

            // 4. 대형 출동 버튼
            GameObject startBtnObj = CreateUIObject("StartMissionButton", panel.transform);
            RectTransform sBtnRt = startBtnObj.GetComponent<RectTransform>();
            sBtnRt.anchorMin = new Vector2(0.5f, 0.5f);
            sBtnRt.anchorMax = new Vector2(0.5f, 0.5f);
            sBtnRt.sizeDelta = new Vector2(700, 100);
            sBtnRt.anchoredPosition = new Vector2(0, -350);
            Image sBtnImg = startBtnObj.AddComponent<Image>();
            sBtnImg.color = new Color(0.92f, 0.32f, 0.38f);
            Button startBtn = startBtnObj.AddComponent<Button>();

            Text startTxt = CreateText("StartText", startBtnObj.transform, "🚀 현장 출동 시작 (Start Mission)", 32, TextAnchor.MiddleCenter, Vector2.zero, new Vector2(680, 90));
            startTxt.color = Color.white;
            startTxt.fontStyle = FontStyle.Bold;

            SetPrivateField(ui, "mapHospitalBtn", mapHosp);
            SetPrivateField(ui, "mapOfficeBtn", mapOff);
            SetPrivateField(ui, "mapHighwayBtn", mapHigh);

            SetPrivateField(ui, "diffEasyBtn", btnEasy);
            SetPrivateField(ui, "diffNormalBtn", btnNorm);
            SetPrivateField(ui, "diffHardBtn", btnHard);
            SetPrivateField(ui, "diffNightmareBtn", btnNight);

            SetPrivateField(ui, "mapTitleText", mapTitle);
            SetPrivateField(ui, "mapDescText", mapDesc);
            SetPrivateField(ui, "difficultySummaryText", diffSummary);
            SetPrivateField(ui, "rewardPreviewText", rewardPreview);

            SetPrivateField(ui, "startMissionButton", startBtn);

            return panel;
        }

        // ==========================================
        // 프리팹 생성 메서드들
        // ==========================================
        private static GameObject CreateOrUpdateCharacterCardPrefab()
        {
            GameObject card = CreateUIObject("CharacterCard", null);
            RectTransform rt = card.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(460, 520);

            Image bg = card.AddComponent<Image>();
            bg.color = new Color(0.22f, 0.19f, 0.30f);

            Text icon = CreateText("Icon", card.transform, "🧑‍💼", 68, TextAnchor.MiddleCenter, new Vector2(0, 180), new Vector2(120, 100));
            Text name = CreateText("Name", card.transform, "김인턴", 28, TextAnchor.MiddleCenter, new Vector2(0, 100), new Vector2(400, 45));
            name.fontStyle = FontStyle.Bold;

            Text title = CreateText("Title", card.transform, "신입 수습사원", 20, TextAnchor.MiddleCenter, new Vector2(0, 60), new Vector2(400, 35));
            title.color = new Color(0.7f, 0.65f, 0.85f);

            Text stats = CreateText("Stats", card.transform, "체력: 100 | 이속: 5.0 | 공격력: x1.0", 18, TextAnchor.MiddleCenter, new Vector2(0, 10), new Vector2(420, 40));
            stats.color = new Color(0.6f, 0.9f, 1f);

            Text passive = CreateText("Passive", card.transform, "[특약 패시브]\n열정 넘치는 신입: 밸런스 균형", 18, TextAnchor.UpperCenter, new Vector2(0, -65), new Vector2(420, 90));
            passive.color = new Color(0.9f, 0.85f, 0.7f);

            Button btn = CreateButton("ActionButton", card.transform, "사원 선택", 22, new Color(0.85f, 0.4f, 0.5f));
            RectTransform btnRt = btn.GetComponent<RectTransform>();
            btnRt.sizeDelta = new Vector2(360, 65);
            btnRt.anchoredPosition = new Vector2(0, -190);

            string path = "Assets/Prefabs/CharacterCardPrefab.prefab";
            Directory.CreateDirectory("Assets/Prefabs");
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(card, path);
            Object.DestroyImmediate(card);
            return prefab;
        }

        private static GameObject CreateOrUpdateShopItemPrefab()
        {
            GameObject card = CreateUIObject("ShopItemCard", null);
            RectTransform rt = card.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(460, 220);

            Image bg = card.AddComponent<Image>();
            bg.color = new Color(0.22f, 0.19f, 0.30f);

            Text icon = CreateText("Icon", card.transform, "🩹", 52, TextAnchor.MiddleCenter, new Vector2(-160, 30), new Vector2(80, 80));
            Text name = CreateText("Name", card.transform, "응급 구급약", 24, TextAnchor.MiddleLeft, new Vector2(50, 60), new Vector2(300, 40));
            name.fontStyle = FontStyle.Bold;

            Text price = CreateText("Price", card.transform, "💰 150 G", 22, TextAnchor.MiddleLeft, new Vector2(50, 15), new Vector2(300, 35));
            price.color = new Color(1f, 0.85f, 0.3f);

            Text desc = CreateText("Desc", card.transform, "현장에서 체력 40 회복", 17, TextAnchor.UpperLeft, new Vector2(0, -35), new Vector2(420, 50));
            desc.color = new Color(0.85f, 0.82f, 0.9f);

            Button buyBtn = CreateButton("BuyButton", card.transform, "구매", 20, new Color(0.3f, 0.65f, 0.45f));
            RectTransform bRt = buyBtn.GetComponent<RectTransform>();
            bRt.sizeDelta = new Vector2(120, 50);
            bRt.anchoredPosition = new Vector2(150, 35);

            string path = "Assets/Prefabs/ShopItemPrefab.prefab";
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(card, path);
            Object.DestroyImmediate(card);
            return prefab;
        }

        private static GameObject CreateOrUpdateSellItemPrefab()
        {
            GameObject row = CreateUIObject("SellItemRow", null);
            RectTransform rt = row.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(280, 160);

            Image bg = row.AddComponent<Image>();
            bg.color = new Color(0.24f, 0.20f, 0.32f);

            Text info = CreateText("Info", row.transform, "🩹 응급 구급약", 20, TextAnchor.MiddleCenter, new Vector2(0, 30), new Vector2(260, 45));
            info.fontStyle = FontStyle.Bold;

            Button sellBtn = CreateButton("SellButton", row.transform, "환급 (+75G)", 18, new Color(0.75f, 0.45f, 0.3f));
            RectTransform bRt = sellBtn.GetComponent<RectTransform>();
            bRt.sizeDelta = new Vector2(220, 50);
            bRt.anchoredPosition = new Vector2(0, -35);

            string path = "Assets/Prefabs/SellItemPrefab.prefab";
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(row, path);
            Object.DestroyImmediate(row);
            return prefab;
        }

        private static GameObject CreateOrUpdateStorageItemPrefab()
        {
            GameObject row = CreateUIObject("StorageItemRow", null);
            RectTransform rt = row.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(440, 140);

            Image bg = row.AddComponent<Image>();
            bg.color = new Color(0.24f, 0.20f, 0.32f);

            Text title = CreateText("Title", row.transform, "🩹 응급 구급약", 22, TextAnchor.MiddleLeft, new Vector2(-60, 35), new Vector2(280, 40));
            title.fontStyle = FontStyle.Bold;

            Text desc = CreateText("Desc", row.transform, "체력 40 즉시 회복", 16, TextAnchor.UpperLeft, new Vector2(0, -10), new Vector2(400, 45));
            desc.color = new Color(0.85f, 0.82f, 0.9f);

            Button actionBtn = CreateButton("ActionButton", row.transform, "이동", 18, new Color(0.55f, 0.4f, 0.7f));
            RectTransform bRt = actionBtn.GetComponent<RectTransform>();
            bRt.sizeDelta = new Vector2(170, 45);
            bRt.anchoredPosition = new Vector2(120, 35);

            string path = "Assets/Prefabs/StorageItemPrefab.prefab";
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(row, path);
            Object.DestroyImmediate(row);
            return prefab;
        }

        // ==========================================
        // UI 헬퍼 메서드들
        // ==========================================
        private static Button CreateButton(string name, Transform parent, string label, int fontSize, Color btnColor)
        {
            GameObject obj = CreateUIObject(name, parent);
            RectTransform rt = obj.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(160, 60);

            Image img = obj.AddComponent<Image>();
            img.color = btnColor;

            Button btn = obj.AddComponent<Button>();

            Text txt = CreateText("Text", obj.transform, label, fontSize, TextAnchor.MiddleCenter, Vector2.zero, rt.sizeDelta);
            txt.color = Color.white;

            return btn;
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

        private static void LoadKoreanFont()
        {
            koreanFont = AssetDatabase.LoadAssetAtPath<Font>("Assets/Fonts/MalgunGothic.ttf");
            if (koreanFont == null)
            {
                koreanFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
                field.SetValue(target, value);
        }
    }
}
