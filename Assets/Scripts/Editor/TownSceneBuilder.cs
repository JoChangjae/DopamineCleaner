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

        [MenuItem("Dopamine Cleaner/1. 🏰 타운 본부 씬 구축 (Build HQ Scene)")]
        public static void BuildTownScene()
        {
            LoadKoreanFont();

            // 1. 프리팹 생성/갱신
            GameObject charCardPrefab = CreateOrUpdateCharacterCardPrefab();
            GameObject shopItemPrefab = CreateOrUpdateShopItemPrefab();
            GameObject sellItemPrefab = CreateOrUpdateSellItemPrefab();
            GameObject storageItemPrefab = CreateOrUpdateStorageItemPrefab();
            GameObject researchCardPrefab = CreateOrUpdateResearchCardPrefab();

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

            // 4. GameDataManager & MutationRouletteManager 생성
            GameObject gdmObj = new GameObject("[GameDataManager]");
            gdmObj.AddComponent<GameDataManager>();
            gdmObj.AddComponent<MutationRouletteManager>();

            // 5. 캔버스 및 UI 생성
            GameObject canvasObj = new GameObject("TownCanvas");
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

            // TownManager & Pixel Atmosphere
            TownManager townMgr = canvasObj.AddComponent<TownManager>();
            canvasObj.AddComponent<TownPixelAtmosphere>();
            canvasObj.AddComponent<CyberTerminalBootUI>();
            canvasObj.AddComponent<UI.SettingsModalUI>();
            canvasObj.AddComponent<UI.EvolutionTreeModalUI>();

            // 배경 패널
            GameObject bgObj = CreateUIObject("TownBackground", canvasObj.transform);
            RectTransform bgRt = bgObj.GetComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.sizeDelta = Vector2.zero;
            Image bgImg = bgObj.AddComponent<Image>();
            bgImg.color = new Color(0.12f, 0.10f, 0.16f);

            // 상단 헤더 (본부명 및 재화/요원증)
            GameObject headerObj = CreateUIObject("HeaderBar", canvasObj.transform);
            RectTransform hRt = headerObj.GetComponent<RectTransform>();
            hRt.anchorMin = new Vector2(0f, 1f);
            hRt.anchorMax = new Vector2(1f, 1f);
            hRt.pivot = new Vector2(0.5f, 1f);
            hRt.sizeDelta = new Vector2(-60, 90);
            hRt.anchoredPosition = new Vector2(0, -15);

            Image hBg = headerObj.AddComponent<Image>();
            hBg.color = new Color(0.18f, 0.15f, 0.24f);

            Text corpTitle = CreateText("CorpTitle", headerObj.transform, "🧠 도파민 클리너 본부 (HQ)", 28, TextAnchor.MiddleLeft, Vector2.zero, new Vector2(500, 40));
            RectTransform ctRt = corpTitle.GetComponent<RectTransform>();
            ctRt.anchorMin = new Vector2(0f, 0.5f);
            ctRt.anchorMax = new Vector2(0f, 0.5f);
            ctRt.pivot = new Vector2(0f, 0.5f);
            ctRt.anchoredPosition = new Vector2(30, 18);

            Text activeCharTxt = CreateText("ActiveCharText", headerObj.transform, "요원증: 🧑‍💼 김인턴 (수습 클리너)", 19, TextAnchor.MiddleLeft, Vector2.zero, new Vector2(550, 35));
            activeCharTxt.color = new Color(0.85f, 0.8f, 0.95f);
            RectTransform acRt = activeCharTxt.GetComponent<RectTransform>();
            acRt.anchorMin = new Vector2(0f, 0.5f);
            acRt.anchorMax = new Vector2(0f, 0.5f);
            acRt.pivot = new Vector2(0f, 0.5f);
            acRt.anchoredPosition = new Vector2(30, -18);

            // 상단 환경설정 버튼 (헤더 중앙)
            Button settingsBtn = CreateButton("SettingsButton", headerObj.transform, "⚙️ 환경설정", 16, new Color(0.20f, 0.35f, 0.45f));
            RectTransform sRt = settingsBtn.GetComponent<RectTransform>();
            sRt.sizeDelta = new Vector2(160, 48);
            sRt.anchoredPosition = new Vector2(90, 0);

            // 상단 정화 도감 버튼
            Button codexBtn = CreateButton("CodexButton", headerObj.transform, "📜 정화 도감", 16, new Color(0.38f, 0.28f, 0.65f));
            RectTransform cdRt = codexBtn.GetComponent<RectTransform>();
            cdRt.sizeDelta = new Vector2(160, 48);
            cdRt.anchoredPosition = new Vector2(265, 0);
            codexBtn.onClick.AddListener(() => UI.EvolutionTreeModalUI.Instance?.OpenCodex(0));

            // 상단 펫 훈련소 버튼
            Button petBtn = CreateButton("PetButton", headerObj.transform, "🐾 동행 펫", 16, new Color(0.25f, 0.55f, 0.42f));
            RectTransform petRt = petBtn.GetComponent<RectTransform>();
            petRt.sizeDelta = new Vector2(140, 48);
            petRt.anchoredPosition = new Vector2(420, 0);
            petBtn.onClick.AddListener(() => Town.PetManagementModalUI.Instance?.ToggleModal());

            // 상단 창모드 / 전체화면 토글 버튼 (헤더 중앙 우측)
            Button winModeBtn = CreateButton("WindowModeButton", headerObj.transform, "🖥️ 화면 모드 (F11)", 16, new Color(0.24f, 0.28f, 0.38f));
            RectTransform wRt = winModeBtn.GetComponent<RectTransform>();
            wRt.anchorMin = new Vector2(1f, 0.5f);
            wRt.anchorMax = new Vector2(1f, 0.5f);
            wRt.pivot = new Vector2(1f, 0.5f);
            wRt.sizeDelta = new Vector2(210, 46);
            wRt.anchoredPosition = new Vector2(-460, 0);
            Text winModeTxt = winModeBtn.GetComponentInChildren<Text>();

            Text goldTxt = CreateText("GoldText", headerObj.transform, "💰 보유 골드: 600 G", 21, TextAnchor.MiddleRight, Vector2.zero, new Vector2(400, 38));
            goldTxt.color = new Color(1f, 0.85f, 0.3f);
            RectTransform gRt = goldTxt.GetComponent<RectTransform>();
            gRt.anchorMin = new Vector2(1f, 0.5f);
            gRt.anchorMax = new Vector2(1f, 0.5f);
            gRt.pivot = new Vector2(1f, 0.5f);
            gRt.anchoredPosition = new Vector2(-30, 18);

            Text karmaTxt = CreateText("KarmaText", headerObj.transform, "✨ 디톡스 포인트: 1 P", 21, TextAnchor.MiddleRight, Vector2.zero, new Vector2(400, 34));
            karmaTxt.color = new Color(0.6f, 0.85f, 1f);
            RectTransform kRt = karmaTxt.GetComponent<RectTransform>();
            kRt.anchorMin = new Vector2(1f, 0.5f);
            kRt.anchorMax = new Vector2(1f, 0.5f);
            kRt.pivot = new Vector2(1f, 0.5f);
            kRt.anchoredPosition = new Vector2(-30, -18);

            SetPrivateField(townMgr, "goldText", goldTxt);
            SetPrivateField(townMgr, "karmaText", karmaTxt);
            SetPrivateField(townMgr, "activeCharacterText", activeCharTxt);
            SetPrivateField(townMgr, "windowModeBtn", winModeBtn);
            SetPrivateField(townMgr, "windowModeBtnText", winModeTxt);
            SetPrivateField(townMgr, "settingsBtn", settingsBtn);

            // 상단 탭 네비게이션 버튼 (5개)
            GameObject tabNavObj = CreateUIObject("TabNavBar", canvasObj.transform);
            RectTransform tabRt = tabNavObj.GetComponent<RectTransform>();
            tabRt.anchorMin = new Vector2(0.5f, 1f);
            tabRt.anchorMax = new Vector2(0.5f, 1f);
            tabRt.pivot = new Vector2(0.5f, 1f);
            tabRt.sizeDelta = new Vector2(1500, 52);
            tabRt.anchoredPosition = new Vector2(0, -115);

            HorizontalLayoutGroup tabHlg = tabNavObj.AddComponent<HorizontalLayoutGroup>();
            tabHlg.spacing = 15;
            tabHlg.childAlignment = TextAnchor.MiddleCenter;
            tabHlg.childControlWidth = true;
            tabHlg.childControlHeight = true;

            Button tabChar = CreateButton("TabChar", tabNavObj.transform, "🧑‍💼 요원실", 17, new Color(0.24f, 0.28f, 0.42f));
            Button tabShop = CreateButton("TabShop", tabNavObj.transform, "🛒 보급소", 17, new Color(0.24f, 0.28f, 0.42f));
            Button tabStorage = CreateButton("TabStorage", tabNavObj.transform, "🗄️ 창고", 17, new Color(0.24f, 0.28f, 0.42f));
            Button tabResearch = CreateButton("TabResearch", tabNavObj.transform, "🔬 연구소", 17, new Color(0.24f, 0.28f, 0.42f));
            Button tabAchieve = CreateButton("TabAchieve", tabNavObj.transform, "🏆 도감/실적", 17, new Color(0.24f, 0.28f, 0.42f));
            Button tabFame = CreateButton("TabHallOfFame", tabNavObj.transform, "👑 명예의 전당", 17, new Color(0.38f, 0.32f, 0.15f));
            Button tabRemodel = CreateButton("TabRemodel", tabNavObj.transform, "🛋️ 사옥 리모델링", 17, new Color(0.24f, 0.42f, 0.35f));
            Button tabDungeon = CreateButton("TabDungeon", tabNavObj.transform, "🚀 작전 출동처", 17, new Color(0.85f, 0.35f, 0.38f));

            SetPrivateField(townMgr, "tabCharBtn", tabChar);
            SetPrivateField(townMgr, "tabShopBtn", tabShop);
            SetPrivateField(townMgr, "tabStorageBtn", tabStorage);
            SetPrivateField(townMgr, "tabResearchBtn", tabResearch);
            SetPrivateField(townMgr, "tabAchievementBtn", tabAchieve);
            SetPrivateField(townMgr, "tabHallOfFameBtn", tabFame);
            SetPrivateField(townMgr, "tabRemodelBtn", tabRemodel);
            SetPrivateField(townMgr, "tabDungeonBtn", tabDungeon);

            // 메인 콘텐츠 영역 (중앙)
            GameObject contentArea = CreateUIObject("ContentArea", canvasObj.transform);
            RectTransform contentRt = contentArea.GetComponent<RectTransform>();
            contentRt.anchorMin = new Vector2(0f, 0f);
            contentRt.anchorMax = new Vector2(1f, 1f);
            contentRt.offsetMin = new Vector2(30, 20);
            contentRt.offsetMax = new Vector2(-30, -175);

            // 1. 캐릭터 선택 패널
            GameObject charPanel = CreateCharacterSelectPanel(contentArea.transform, charCardPrefab);
            SetPrivateField(townMgr, "characterPanel", charPanel);

            // 2. 상점 패널
            GameObject shopPanel = CreateShopPanel(contentArea.transform, shopItemPrefab, sellItemPrefab);
            SetPrivateField(townMgr, "shopPanel", shopPanel);
            shopPanel.SetActive(false);

            // 3. 창고 패널
            GameObject storagePanel = CreateStoragePanel(contentArea.transform, storageItemPrefab);
            SetPrivateField(townMgr, "storagePanel", storagePanel);
            storagePanel.SetActive(false);

            // 4. 연구소 패널
            GameObject resPanel = CreateResearchPanel(contentArea.transform, researchCardPrefab);
            SetPrivateField(townMgr, "researchPanel", resPanel);
            resPanel.SetActive(false);

            // 5. 업적/도감 패널
            GameObject achievePanel = CreateAchievementPanel(contentArea.transform);
            SetPrivateField(townMgr, "achievementPanel", achievePanel);
            achievePanel.SetActive(false);

            // 6. 명예의 전당 패널
            GameObject famePanel = CreateHallOfFamePanel(contentArea.transform);
            SetPrivateField(townMgr, "hallOfFamePanel", famePanel);
            famePanel.SetActive(false);

            // 7. 정화 구역 출동처 패널
            GameObject dungeonPanel = CreateDungeonSelectPanel(contentArea.transform);
            SetPrivateField(townMgr, "dungeonPanel", dungeonPanel);
            dungeonPanel.SetActive(false);

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

            EditorUtility.DisplayDialog("도파민 클리너 (Dopamine Cleaner)", "🧠 'TownScene.unity'(도파민 본부 씬) 구축이 성공적으로 완료되었습니다!\n상단 [▶ 플레이] 버튼을 누르면 요원 선택/보급소/창고/출동 시스템을 이용할 수 있습니다.", "확인");
        }

                private static void CreatePetManagementModal(Transform canvasTransform)
        {
            GameObject modalObj = CreateUIObject("PetManagementModal", canvasTransform);
            RectTransform mRt = modalObj.GetComponent<RectTransform>();
            mRt.anchorMin = Vector2.zero;
            mRt.anchorMax = Vector2.one;
            mRt.sizeDelta = Vector2.zero;

            Image mBg = modalObj.AddComponent<Image>();
            mBg.color = new Color(0.06f, 0.08f, 0.12f, 0.94f);

            var petUI = modalObj.AddComponent<Town.PetManagementModalUI>();

            // 헤더
            CreateText("Header", modalObj.transform, "🐾 동행 펫 육성 & 출격 관리소", 32, TextAnchor.MiddleCenter, new Vector2(0, 360), new Vector2(800, 45));

            Text goldTxt = CreateText("GoldStatus", modalObj.transform, "💰 보유 골드: 0 G", 20, TextAnchor.MiddleCenter, new Vector2(0, 315), new Vector2(600, 35));
            goldTxt.color = new Color(1f, 0.85f, 0.3f);

            // 닫기 버튼
            Button closeBtn = CreateButton("CloseBtn", modalObj.transform, "✖ 닫기 (ESC)", 18, new Color(0.7f, 0.2f, 0.2f));
            RectTransform cRt = closeBtn.GetComponent<RectTransform>();
            cRt.anchorMin = new Vector2(1f, 1f);
            cRt.anchorMax = new Vector2(1f, 1f);
            cRt.pivot = new Vector2(1f, 1f);
            cRt.sizeDelta = new Vector2(140, 46);
            cRt.anchoredPosition = new Vector2(-40, -40);

            // 카드 컨테이너
            GameObject container = CreateUIObject("PetCardContainer", modalObj.transform);
            RectTransform cntRt = container.GetComponent<RectTransform>();
            cntRt.anchorMin = new Vector2(0.5f, 0.5f);
            cntRt.anchorMax = new Vector2(0.5f, 0.5f);
            cntRt.sizeDelta = new Vector2(1250, 460);
            cntRt.anchoredPosition = new Vector2(0, -30);

            HorizontalLayoutGroup hlg = container.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 25;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;

            SetPrivateField(petUI, "modalPanel", modalObj);
            SetPrivateField(petUI, "cardContainer", container.transform);
            SetPrivateField(petUI, "closeButton", closeBtn);
            SetPrivateField(petUI, "goldStatusText", goldTxt);

            modalObj.SetActive(false);
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
            cRt.sizeDelta = new Vector2(1650, 640);
            cRt.anchoredPosition = new Vector2(0, 35);

            GridLayoutGroup glg = container.AddComponent<GridLayoutGroup>();
            glg.cellSize = new Vector2(520, 310);
            glg.spacing = new Vector2(25, 15);
            glg.childAlignment = TextAnchor.MiddleCenter;
            glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            glg.constraintCount = 3;

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

            // 상품 목록 (구매 가능 품목)
            CreateText("ShopTitle", panel.transform, "🛒 디톡스 보급소 (구매 가능 품목)", 26, TextAnchor.MiddleLeft, new Vector2(0, 375), new Vector2(1650, 35));

            GameObject shopCont = CreateUIObject("ShopItemContainer", panel.transform);
            RectTransform sRt = shopCont.GetComponent<RectTransform>();
            sRt.anchorMin = new Vector2(0.5f, 0.5f);
            sRt.anchorMax = new Vector2(0.5f, 0.5f);
            sRt.sizeDelta = new Vector2(1650, 420);
            sRt.anchoredPosition = new Vector2(0, 140);

            GridLayoutGroup sGlg = shopCont.AddComponent<GridLayoutGroup>();
            sGlg.cellSize = new Vector2(380, 190);
            sGlg.spacing = new Vector2(30, 20);
            sGlg.childAlignment = TextAnchor.MiddleCenter;

            // 판매 목록 (소지품 환급)
            CreateText("SellTitle", panel.transform, "💰 소지품 매각 환급 (가방 아이템)", 24, TextAnchor.MiddleLeft, new Vector2(0, -115), new Vector2(1650, 35));

            GameObject sellCont = CreateUIObject("SellItemContainer", panel.transform);
            RectTransform sellRt = sellCont.GetComponent<RectTransform>();
            sellRt.anchorMin = new Vector2(0.5f, 0.5f);
            sellRt.anchorMax = new Vector2(0.5f, 0.5f);
            sellRt.sizeDelta = new Vector2(1650, 160);
            sellRt.anchoredPosition = new Vector2(0, -235);

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

            // 좌측: 출동 가방 (최대 6칸)
            Text invCap = CreateText("InvCap", panel.transform, "🎒 출동 가방 (0 / 6)", 26, TextAnchor.MiddleCenter, new Vector2(-510, 375), new Vector2(580, 40));
            GameObject invCont = CreateUIObject("InvContainer", panel.transform);
            RectTransform iRt = invCont.GetComponent<RectTransform>();
            iRt.anchorMin = new Vector2(0.5f, 0.5f);
            iRt.anchorMax = new Vector2(0.5f, 0.5f);
            iRt.sizeDelta = new Vector2(580, 680);
            iRt.anchoredPosition = new Vector2(-510, -20);

            GridLayoutGroup iGlg = invCont.AddComponent<GridLayoutGroup>();
            iGlg.cellSize = new Vector2(270, 190);
            iGlg.spacing = new Vector2(20, 25);
            iGlg.childAlignment = TextAnchor.UpperCenter;

            // 중앙: 이동 안내
            Text midHint = CreateText("MidHint", panel.transform, "⇄\n아이템 클릭 시\n가방 ↔ 창고\n즉시 이동", 20, TextAnchor.MiddleCenter, new Vector2(0, -20), new Vector2(180, 160));
            midHint.color = new Color(0.75f, 0.70f, 0.90f);

            // 우측: 본부 창고 (최대 24칸)
            Text stashCap = CreateText("StashCap", panel.transform, "🗄️ 본부 창고 (0 / 24)", 26, TextAnchor.MiddleCenter, new Vector2(450, 375), new Vector2(750, 40));
            GameObject stashCont = CreateUIObject("StashContainer", panel.transform);
            RectTransform sRt = stashCont.GetComponent<RectTransform>();
            sRt.anchorMin = new Vector2(0.5f, 0.5f);
            sRt.anchorMax = new Vector2(0.5f, 0.5f);
            sRt.sizeDelta = new Vector2(750, 680);
            sRt.anchoredPosition = new Vector2(450, -20);

            GridLayoutGroup sGlg = stashCont.AddComponent<GridLayoutGroup>();
            sGlg.cellSize = new Vector2(230, 150);
            sGlg.spacing = new Vector2(15, 15);
            sGlg.childAlignment = TextAnchor.UpperCenter;

            SetPrivateField(ui, "inventoryCapacityText", invCap);
            SetPrivateField(ui, "inventoryContainer", invCont.transform);
            SetPrivateField(ui, "storageItemPrefab", itemPrefab);
            SetPrivateField(ui, "stashCapacityText", stashCap);
            SetPrivateField(ui, "stashContainer", stashCont.transform);

            return panel;
        }

        private static GameObject CreateResearchPanel(Transform parent, GameObject itemPrefab)
        {
            GameObject panel = CreateUIObject("ResearchPanel", parent);
            RectTransform rt = panel.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;

            ResearchUI ui = panel.AddComponent<ResearchUI>();

            CreateText("ResTitle", panel.transform, "🔬 본부 연구소 (영구 능력치 강화)", 26, TextAnchor.MiddleLeft, new Vector2(-280, 375), new Vector2(1000, 35));

            Button refundBtn = CreateButton("ResetRefundBtn", panel.transform, "🔄 연구 100% 전액 환급 초기화", 18, new Color(0.75f, 0.25f, 0.28f));
            RectTransform rRt = refundBtn.GetComponent<RectTransform>();
            rRt.sizeDelta = new Vector2(340, 48);
            rRt.anchoredPosition = new Vector2(650, 375);
            SetPrivateField(ui, "refundAllButton", refundBtn);

            GameObject cont = CreateUIObject("ResearchContainer", panel.transform);
            RectTransform cRt = cont.GetComponent<RectTransform>();
            cRt.anchorMin = new Vector2(0.5f, 0.5f);
            cRt.anchorMax = new Vector2(0.5f, 0.5f);
            cRt.sizeDelta = new Vector2(1650, 680);
            cRt.anchoredPosition = new Vector2(0, -10);

            GridLayoutGroup glg = cont.AddComponent<GridLayoutGroup>();
            glg.cellSize = new Vector2(380, 260);
            glg.spacing = new Vector2(35, 30);
            glg.childAlignment = TextAnchor.UpperCenter;

            SetPrivateField(ui, "researchContainer", cont.transform);
            SetPrivateField(ui, "researchCardPrefab", itemPrefab);

            return panel;
        }

                private static GameObject CreateOfficeRemodelPanel(Transform parent)
        {
            GameObject panel = CreateUIObject("OfficeRemodelPanel", parent);
            RectTransform rt = panel.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;

            var ui = panel.AddComponent<Town.OfficeRemodelUI>();

            // 1. 상단 상태 및 복지 등급 바
            Text headerTxt = CreateText("OfficeHeader", panel.transform, "🏢 낡은 지하 임시 본부 (복지 총합 Lv.0)  |  💰 보유 골드: 0 G", 23, TextAnchor.MiddleCenter, new Vector2(0, 390), new Vector2(1500, 32));
            headerTxt.color = new Color(1f, 0.9f, 0.5f);
            headerTxt.fontStyle = FontStyle.Bold;

            // 2. 종합 버프 요약 바
            Text buffSummaryTxt = CreateText("BuffSummary", panel.transform, "현재 적용된 사옥 복지 버프 없음", 16, TextAnchor.MiddleCenter, new Vector2(0, 358), new Vector2(1500, 26));
            buffSummaryTxt.color = new Color(0.85f, 0.9f, 1f);

            // 3. 방치형 복지 기금 보관소 바 (Vault Bar)
            GameObject vaultBar = CreateUIObject("WelfareVaultBar", panel.transform);
            RectTransform vRt = vaultBar.GetComponent<RectTransform>();
            vRt.anchorMin = new Vector2(0.5f, 0.5f);
            vRt.anchorMax = new Vector2(0.5f, 0.5f);
            vRt.sizeDelta = new Vector2(1500, 50);
            vRt.anchoredPosition = new Vector2(0, 305);
            Image vBg = vaultBar.AddComponent<Image>();
            vBg.color = new Color(0.14f, 0.16f, 0.22f, 0.95f);

            Text vaultTxt = CreateText("VaultInfo", vaultBar.transform, "🎁 [사내 복지 기금] 미수령 기금: 0 G", 16, TextAnchor.MiddleLeft, new Vector2(25, 0), new Vector2(980, 40));
            vaultTxt.color = Color.white;

            Button claimBtn = CreateButton("ClaimBtn", vaultBar.transform, "📦 복지 기금 적립 대기 중", 15, new Color(0.2f, 0.6f, 0.45f));
            RectTransform cbRt = claimBtn.GetComponent<RectTransform>();
            cbRt.anchorMin = new Vector2(1f, 0.5f);
            cbRt.anchorMax = new Vector2(1f, 0.5f);
            cbRt.pivot = new Vector2(1f, 0.5f);
            cbRt.sizeDelta = new Vector2(340, 38);
            cbRt.anchoredPosition = new Vector2(-15, 0);
            Text claimBtnTxt = claimBtn.GetComponentInChildren<Text>();

            // 4. 복지 세트 시너지 바
            GameObject synergyBar = CreateUIObject("SynergyBar", panel.transform);
            RectTransform sRt = synergyBar.GetComponent<RectTransform>();
            sRt.anchorMin = new Vector2(0.5f, 0.5f);
            sRt.anchorMax = new Vector2(0.5f, 0.5f);
            sRt.sizeDelta = new Vector2(1500, 44);
            sRt.anchoredPosition = new Vector2(0, 246);

            HorizontalLayoutGroup sHlg = synergyBar.AddComponent<HorizontalLayoutGroup>();
            sHlg.spacing = 20;
            sHlg.childAlignment = TextAnchor.MiddleCenter;
            sHlg.childControlWidth = true;
            sHlg.childControlHeight = true;

            // 5. 사옥 디오라마 쇼케이스 (HQ Diorama Showcase)
            GameObject dioramaObj = CreateUIObject("OfficeDiorama", panel.transform);
            RectTransform dRt = dioramaObj.GetComponent<RectTransform>();
            dRt.anchorMin = new Vector2(0.5f, 0.5f);
            dRt.anchorMax = new Vector2(0.5f, 0.5f);
            dRt.sizeDelta = new Vector2(1500, 125);
            dRt.anchoredPosition = new Vector2(0, 150);

            var diorama = dioramaObj.AddComponent<Town.TownOfficeDiorama>();

            // 6. 가구 카드 그리드 컨테이너
            GameObject container = CreateUIObject("FacilityGrid", panel.transform);
            RectTransform cntRt = container.GetComponent<RectTransform>();
            cntRt.anchorMin = new Vector2(0.5f, 0.5f);
            cntRt.anchorMax = new Vector2(0.5f, 0.5f);
            cntRt.sizeDelta = new Vector2(1540, 470);
            cntRt.anchoredPosition = new Vector2(0, -170);

            GridLayoutGroup glg = container.AddComponent<GridLayoutGroup>();
            glg.cellSize = new Vector2(490, 225);
            glg.spacing = new Vector2(25, 16);
            glg.childAlignment = TextAnchor.MiddleCenter;
            glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            glg.constraintCount = 3;

            SetPrivateField(ui, "headerStatusText", headerTxt);
            SetPrivateField(ui, "buffSummaryText", buffSummaryTxt);
            SetPrivateField(ui, "welfareFundInfoText", vaultTxt);
            SetPrivateField(ui, "welfareClaimBtn", claimBtn);
            SetPrivateField(ui, "welfareClaimBtnText", claimBtnTxt);
            SetPrivateField(ui, "synergyContainer", synergyBar.transform);
            SetPrivateField(ui, "officeDiorama", diorama);
            SetPrivateField(ui, "cardContainer", container.transform);

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
            CreateText("MapHeader", panel.transform, "📍 정화 작전 구역 선택", 28, TextAnchor.MiddleCenter, new Vector2(0, 350), new Vector2(900, 35));

            GameObject mapBtnGroup = CreateUIObject("MapBtnGroup", panel.transform);
            RectTransform mRt = mapBtnGroup.GetComponent<RectTransform>();
            mRt.anchorMin = new Vector2(0.5f, 0.5f);
            mRt.anchorMax = new Vector2(0.5f, 0.5f);
            mRt.sizeDelta = new Vector2(1300, 75);
            mRt.anchoredPosition = new Vector2(0, 280);

            HorizontalLayoutGroup mHlg = mapBtnGroup.AddComponent<HorizontalLayoutGroup>();
            mHlg.spacing = 25;
            mHlg.childAlignment = TextAnchor.MiddleCenter;
            mHlg.childControlWidth = true;
            mHlg.childControlHeight = true;

            Button mapHosp = CreateButton("BtnHosp", mapBtnGroup.transform, "🏥 격리 병동\n(5분 생존)", 20, new Color(0.24f, 0.32f, 0.38f));
            Button mapOff = CreateButton("BtnOff", mapBtnGroup.transform, "🏢 번아웃 오피스\n(7분 생존)", 20, new Color(0.32f, 0.25f, 0.4f));
            Button mapHigh = CreateButton("BtnHigh", mapBtnGroup.transform, "🛣️ 숏폼 고속도로\n(10분 생존)", 20, new Color(0.42f, 0.22f, 0.25f));
            Button mapAbyss = CreateButton("BtnAbyss", mapBtnGroup.transform, "📱 알고리즘 심연\n(12분 생존)", 20, new Color(0.36f, 0.18f, 0.48f));
            Button mapBossRush = CreateButton("BtnBossRush", mapBtnGroup.transform, "👑 보스 러시\n(15분 무한)", 20, new Color(0.55f, 0.15f, 0.2f));

            // 2. 난이도 선택 헤더 & 버튼들
            CreateText("DiffHeader", panel.transform, "⚡ 도파민 오염 등급 (난이도 설정)", 25, TextAnchor.MiddleCenter, new Vector2(0, 215), new Vector2(900, 30));

            GameObject diffBtnGroup = CreateUIObject("DiffBtnGroup", panel.transform);
            RectTransform dRt = diffBtnGroup.GetComponent<RectTransform>();
            dRt.anchorMin = new Vector2(0.5f, 0.5f);
            dRt.anchorMax = new Vector2(0.5f, 0.5f);
            dRt.sizeDelta = new Vector2(1300, 52);
            dRt.anchoredPosition = new Vector2(0, 160);

            HorizontalLayoutGroup dHlg = diffBtnGroup.AddComponent<HorizontalLayoutGroup>();
            dHlg.spacing = 20;
            dHlg.childAlignment = TextAnchor.MiddleCenter;
            dHlg.childControlWidth = true;
            dHlg.childControlHeight = true;

            Button btnEasy = CreateButton("BtnEasy", diffBtnGroup.transform, "Easy (0.8x)", 19, new Color(0.25f, 0.55f, 0.35f));
            Button btnNorm = CreateButton("BtnNorm", diffBtnGroup.transform, "Normal (1.0x)", 19, new Color(0.25f, 0.45f, 0.75f));
            Button btnHard = CreateButton("BtnHard", diffBtnGroup.transform, "Hard (1.5x)", 19, new Color(0.75f, 0.45f, 0.2f));
            Button btnNight = CreateButton("BtnNight", diffBtnGroup.transform, "Nightmare (2.2x)", 19, new Color(0.75f, 0.15f, 0.25f));

            // 3. 브리핑 정보 창 (카드 형태: 난이도 버튼과 16px 이상 여백 확보)
            GameObject briefingObj = CreateUIObject("BriefingBox", panel.transform);
            RectTransform bRt = briefingObj.GetComponent<RectTransform>();
            bRt.anchorMin = new Vector2(0.5f, 0.5f);
            bRt.anchorMax = new Vector2(0.5f, 0.5f);
            bRt.sizeDelta = new Vector2(1300, 185);
            bRt.anchoredPosition = new Vector2(0, 25);
            Image bImg = briefingObj.AddComponent<Image>();
            bImg.color = new Color(0.20f, 0.17f, 0.27f);

            GameObject previewImgObj = CreateUIObject("MapPreview", briefingObj.transform);
            RectTransform pImgRt = previewImgObj.GetComponent<RectTransform>();
            pImgRt.sizeDelta = new Vector2(145, 145);
            pImgRt.anchoredPosition = new Vector2(-530, 0);
            Image mapPrevImg = previewImgObj.AddComponent<Image>();
            mapPrevImg.preserveAspect = false;

            Text mapTitle = CreateText("MapTitle", briefingObj.transform, "🏥 도파민 격리 병동", 26, TextAnchor.MiddleLeft, new Vector2(90, 56), new Vector2(980, 32));
            mapTitle.color = new Color(1f, 0.9f, 0.5f);
            mapTitle.fontStyle = FontStyle.Bold;

            Text mapDesc = CreateText("MapDesc", briefingObj.transform, "정화 작전 구역 설명", 18, TextAnchor.UpperLeft, new Vector2(90, 20), new Vector2(980, 34));
            mapDesc.color = new Color(0.85f, 0.82f, 0.9f);

            Text diffSummary = CreateText("DiffSummary", briefingObj.transform, "난이도 계수 정보", 18, TextAnchor.MiddleLeft, new Vector2(90, -18), new Vector2(980, 28));
            diffSummary.color = new Color(0.6f, 0.85f, 1f);

            Text rewardPreview = CreateText("RewardPreview", briefingObj.transform, "클리어 예상 보상", 22, TextAnchor.MiddleLeft, new Vector2(90, -54), new Vector2(980, 32));
            rewardPreview.color = new Color(0.4f, 1f, 0.6f);
            rewardPreview.fontStyle = FontStyle.Bold;

            // 4. 블랙기업 근로 조약 영역
            GameObject pactBtnGroup = CreateUIObject("PactBtnGroup", panel.transform);
            RectTransform pRt = pactBtnGroup.GetComponent<RectTransform>();
            pRt.anchorMin = new Vector2(0.5f, 0.5f);
            pRt.anchorMax = new Vector2(0.5f, 0.5f);
            pRt.sizeDelta = new Vector2(1300, 46);
            pRt.anchoredPosition = new Vector2(0, -105);

            HorizontalLayoutGroup pHlg = pactBtnGroup.AddComponent<HorizontalLayoutGroup>();
            pHlg.spacing = 15;
            pHlg.childAlignment = TextAnchor.MiddleCenter;
            pHlg.childControlWidth = true;
            pHlg.childControlHeight = true;

            Button pactOverwork = CreateButton("PactOverwork", pactBtnGroup.transform, "⏰ 주말 특근 (+25%G)", 17, new Color(0.22f, 0.20f, 0.28f));
            Button pactAi = CreateButton("PactAI", pactBtnGroup.transform, "🤖 AI 압박 (+25%P)", 17, new Color(0.22f, 0.20f, 0.28f));
            Button pactNotif = CreateButton("PactNotif", pactBtnGroup.transform, "📨 스팸 폭탄 (+25%Exp)", 17, new Color(0.22f, 0.20f, 0.28f));
            Button pactBlackout = CreateButton("PactBlackout", pactBtnGroup.transform, "🔌 블랙아웃 (+35%G)", 17, new Color(0.22f, 0.20f, 0.28f));

            Text pactSummary = CreateText("PactSummary", panel.transform, "📜 [근로 조약]: 미체결", 17, TextAnchor.MiddleCenter, new Vector2(0, -146), new Vector2(1300, 24));
            pactSummary.color = new Color(0.7f, 0.75f, 0.85f);

            // 5. 돌연변이 룰렛 영역
            Button spinBtn = CreateButton("MutationSpinBtn", panel.transform, "🎰 돌연변이 룰렛 추첨", 18, new Color(0.48f, 0.28f, 0.65f));
            RectTransform spRt = spinBtn.GetComponent<RectTransform>();
            spRt.anchorMin = new Vector2(0.5f, 0.5f);
            spRt.anchorMax = new Vector2(0.5f, 0.5f);
            spRt.sizeDelta = new Vector2(340, 42);
            spRt.anchoredPosition = new Vector2(-320, -195);

            Text mutStatus = CreateText("MutationStatus", panel.transform, "🎰 [출격 돌연변이]: 🛡️ 표준 모드 (특이 변이 없음)", 17, TextAnchor.MiddleLeft, new Vector2(180, -195), new Vector2(620, 38));
            mutStatus.color = new Color(0.8f, 0.85f, 1f);

            // 6. 대형 출동 버튼
            GameObject startBtnObj = CreateUIObject("StartMissionButton", panel.transform);
            RectTransform sBtnRt = startBtnObj.GetComponent<RectTransform>();
            sBtnRt.anchorMin = new Vector2(0.5f, 0.5f);
            sBtnRt.anchorMax = new Vector2(0.5f, 0.5f);
            sBtnRt.sizeDelta = new Vector2(650, 66);
            sBtnRt.anchoredPosition = new Vector2(0, -275);
            Image sBtnImg = startBtnObj.AddComponent<Image>();
            sBtnImg.color = new Color(0.92f, 0.32f, 0.38f);
            Button startBtn = startBtnObj.AddComponent<Button>();

            Text startTxt = CreateText("StartText", startBtnObj.transform, "🚀 정화 작전 출동 (Start Cleanse)", 26, TextAnchor.MiddleCenter, Vector2.zero, new Vector2(630, 66));
            startTxt.color = Color.white;
            startTxt.fontStyle = FontStyle.Bold;

            SetPrivateField(ui, "mapHospitalBtn", mapHosp);
            SetPrivateField(ui, "mapOfficeBtn", mapOff);
            SetPrivateField(ui, "mapHighwayBtn", mapHigh);
            SetPrivateField(ui, "mapAbyssBtn", mapAbyss);
            SetPrivateField(ui, "mapBossRushBtn", mapBossRush);

            SetPrivateField(ui, "diffEasyBtn", btnEasy);
            SetPrivateField(ui, "diffNormalBtn", btnNorm);
            SetPrivateField(ui, "diffHardBtn", btnHard);
            SetPrivateField(ui, "diffNightmareBtn", btnNight);

            SetPrivateField(ui, "mapPreviewImage", mapPrevImg);
            SetPrivateField(ui, "mapTitleText", mapTitle);
            SetPrivateField(ui, "mapDescText", mapDesc);
            SetPrivateField(ui, "difficultySummaryText", diffSummary);
            SetPrivateField(ui, "rewardPreviewText", rewardPreview);

            SetPrivateField(ui, "pactOverworkBtn", pactOverwork);
            SetPrivateField(ui, "pactAiPressureBtn", pactAi);
            SetPrivateField(ui, "pactNotificationBtn", pactNotif);
            SetPrivateField(ui, "pactBlackoutBtn", pactBlackout);
            SetPrivateField(ui, "pactSummaryText", pactSummary);

            SetPrivateField(ui, "mutationSpinButton", spinBtn);
            SetPrivateField(ui, "mutationStatusText", mutStatus);
            SetPrivateField(ui, "startMissionButton", startBtn);

            return panel;
        }

        private static GameObject CreateHallOfFamePanel(Transform parent)
        {
            GameObject panel = CreateUIObject("HallOfFamePanel", parent);
            RectTransform rt = panel.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;

            HallOfFameUI ui = panel.AddComponent<HallOfFameUI>();

            CreateText("Header", panel.transform, "👑 디톡스 최고 명예의 전당 (Top 10 Cleaner Records)", 28, TextAnchor.MiddleCenter, new Vector2(0, 340), new Vector2(900, 40));
            CreateText("SubGuide", panel.transform, "역대 정화 요원들의 생존 시간과 잡생각 소독 실적 순위표입니다.", 18, TextAnchor.MiddleCenter, new Vector2(0, 300), new Vector2(900, 30));

            // 스크롤뷰
            GameObject scrollObj = CreateUIObject("ScrollView", panel.transform);
            RectTransform scrt = scrollObj.GetComponent<RectTransform>();
            scrt.anchorMin = new Vector2(0.5f, 0.5f);
            scrt.anchorMax = new Vector2(0.5f, 0.5f);
            scrt.anchoredPosition = new Vector2(0, -20);
            scrt.sizeDelta = new Vector2(1360, 560);

            ScrollRect sr = scrollObj.AddComponent<ScrollRect>();
            sr.horizontal = false;
            sr.vertical = true;

            Image maskImg = scrollObj.AddComponent<Image>();
            maskImg.color = new Color(0.1f, 0.08f, 0.15f, 0.6f);
            Mask mask = scrollObj.AddComponent<Mask>();
            mask.showMaskGraphic = true;

            GameObject contentObj = CreateUIObject("Content", scrollObj.transform);
            RectTransform crt = contentObj.GetComponent<RectTransform>();
            crt.anchorMin = new Vector2(0f, 1f);
            crt.anchorMax = new Vector2(1f, 1f);
            crt.pivot = new Vector2(0.5f, 1f);
            crt.anchoredPosition = Vector2.zero;

            VerticalLayoutGroup vlg = contentObj.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 8;
            vlg.padding = new RectOffset(15, 15, 15, 15);
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;

            ContentSizeFitter csf = contentObj.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            sr.content = crt;

            SetPrivateField(ui, "recordListContainer", contentObj.transform);
            return panel;
        }

        // ==========================================
        // 프리팹 생성 메서드들
        // ==========================================
        private static GameObject CreateOrUpdateCharacterCardPrefab()
        {
            GameObject card = CreateUIObject("CharacterCard", null);
            RectTransform rt = card.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(520, 310);

            Image bg = card.AddComponent<Image>();
            bg.color = new Color(0.20f, 0.17f, 0.28f);

            Text icon = CreateText("Icon", card.transform, "🧑‍💼", 54, TextAnchor.MiddleCenter, new Vector2(-190, 45), new Vector2(100, 100));
            Text name = CreateText("Name", card.transform, "김인턴", 23, TextAnchor.MiddleLeft, new Vector2(40, 75), new Vector2(340, 35));
            name.fontStyle = FontStyle.Bold;

            Text title = CreateText("Title", card.transform, "수습 클리너", 17, TextAnchor.MiddleLeft, new Vector2(40, 45), new Vector2(340, 28));
            title.color = new Color(0.75f, 0.70f, 0.90f);

            Text stats = CreateText("Stats", card.transform, "체력: 100 | 이속: 5.0 | 공격력: x1.0", 14, TextAnchor.MiddleLeft, new Vector2(40, 15), new Vector2(340, 30));
            stats.color = new Color(0.6f, 0.9f, 1f);

            Text passive = CreateText("Passive", card.transform, "[고유 특성]\n열정 넘치는 신입: 밸런스 균형", 14, TextAnchor.UpperLeft, new Vector2(0, -35), new Vector2(480, 50));
            passive.color = new Color(0.95f, 0.88f, 0.72f);

            Button btn = CreateButton("ActionButton", card.transform, "요원 선택", 18, new Color(0.85f, 0.4f, 0.5f));
            RectTransform btnRt = btn.GetComponent<RectTransform>();
            btnRt.sizeDelta = new Vector2(380, 45);
            btnRt.anchoredPosition = new Vector2(0, -95);

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
            rt.sizeDelta = new Vector2(380, 190);

            Image bg = card.AddComponent<Image>();
            bg.color = new Color(0.20f, 0.17f, 0.28f);

            Text icon = CreateText("Icon", card.transform, "🩹", 50, TextAnchor.MiddleCenter, new Vector2(-135, 45), new Vector2(64, 64));

            GameObject imgObj = CreateUIObject("ItemImage", card.transform);
            RectTransform iRt = imgObj.GetComponent<RectTransform>();
            iRt.sizeDelta = new Vector2(64, 64);
            iRt.anchoredPosition = new Vector2(-135, 45);
            Image itemImg = imgObj.AddComponent<Image>();
            itemImg.preserveAspect = true;
            imgObj.SetActive(false);

            Text name = CreateText("Name", card.transform, "응급 구급약", 20, TextAnchor.MiddleLeft, new Vector2(20, 58), new Vector2(240, 32));
            name.fontStyle = FontStyle.Bold;

            Text price = CreateText("Price", card.transform, "💰 150 G", 18, TextAnchor.MiddleLeft, new Vector2(20, 25), new Vector2(240, 26));
            price.color = new Color(1f, 0.85f, 0.3f);

            Text desc = CreateText("Desc", card.transform, "현장에서 체력 40 회복", 15, TextAnchor.UpperLeft, new Vector2(0, -14), new Vector2(340, 42));
            desc.color = new Color(0.85f, 0.82f, 0.9f);

            Button buyBtn = CreateButton("BuyButton", card.transform, "🛒 구매하기", 17, new Color(0.25f, 0.58f, 0.40f));
            RectTransform bRt = buyBtn.GetComponent<RectTransform>();
            bRt.sizeDelta = new Vector2(340, 40);
            bRt.anchoredPosition = new Vector2(0, -62);

            string path = "Assets/Prefabs/ShopItemPrefab.prefab";
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(card, path);
            Object.DestroyImmediate(card);
            return prefab;
        }

        private static GameObject CreateOrUpdateSellItemPrefab()
        {
            GameObject row = CreateUIObject("SellItemRow", null);
            RectTransform rt = row.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(245, 140);

            Image bg = row.AddComponent<Image>();
            bg.color = new Color(0.22f, 0.18f, 0.30f);

            Text info = CreateText("Info", row.transform, "🩹 응급 구급약", 18, TextAnchor.MiddleCenter, new Vector2(0, 25), new Vector2(230, 40));
            info.fontStyle = FontStyle.Bold;

            Button sellBtn = CreateButton("SellButton", row.transform, "환급 (+75G)", 17, new Color(0.75f, 0.45f, 0.3f));
            RectTransform bRt = sellBtn.GetComponent<RectTransform>();
            bRt.sizeDelta = new Vector2(200, 45);
            bRt.anchoredPosition = new Vector2(0, -30);

            string path = "Assets/Prefabs/SellItemPrefab.prefab";
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(row, path);
            Object.DestroyImmediate(row);
            return prefab;
        }

        private static GameObject CreateOrUpdateStorageItemPrefab()
        {
            GameObject row = CreateUIObject("StorageItemRow", null);
            RectTransform rt = row.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(230, 150);

            Image bg = row.AddComponent<Image>();
            bg.color = new Color(0.22f, 0.18f, 0.30f);

            Text title = CreateText("Title", row.transform, "🩹 응급 구급약", 18, TextAnchor.MiddleCenter, new Vector2(0, 35), new Vector2(210, 35));
            title.fontStyle = FontStyle.Bold;

            Text desc = CreateText("Desc", row.transform, "체력 40 회복", 15, TextAnchor.MiddleCenter, new Vector2(0, 0), new Vector2(210, 30));
            desc.color = new Color(0.85f, 0.82f, 0.9f);

            Button actionBtn = CreateButton("ActionButton", row.transform, "이동", 17, new Color(0.55f, 0.4f, 0.7f));
            RectTransform bRt = actionBtn.GetComponent<RectTransform>();
            bRt.sizeDelta = new Vector2(160, 40);
            bRt.anchoredPosition = new Vector2(0, -40);

            string path = "Assets/Prefabs/StorageItemPrefab.prefab";
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(row, path);
            Object.DestroyImmediate(row);
            return prefab;
        }

        private static GameObject CreateOrUpdateResearchCardPrefab()
        {
            GameObject card = CreateUIObject("ResearchCard", null);
            RectTransform rt = card.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(380, 260);

            Image bg = card.AddComponent<Image>();
            bg.color = new Color(0.20f, 0.17f, 0.28f);

            Text icon = CreateText("Icon", card.transform, "💥", 48, TextAnchor.MiddleCenter, new Vector2(-125, 65), new Vector2(70, 70));
            Text title = CreateText("Title", card.transform, "뇌파 집중 (공격력)", 20, TextAnchor.MiddleLeft, new Vector2(35, 80), new Vector2(230, 35));
            title.fontStyle = FontStyle.Bold;

            Text lvlText = CreateText("LevelText", card.transform, "Lv.0 / 5", 18, TextAnchor.MiddleLeft, new Vector2(35, 45), new Vector2(230, 25));
            lvlText.color = new Color(0.6f, 0.9f, 1f);

            Text desc = CreateText("Desc", card.transform, "모든 무기의 기본 공격력이 상승합니다. (레벨당 +10%)", 15, TextAnchor.UpperLeft, new Vector2(0, -10), new Vector2(330, 70));
            desc.color = new Color(0.85f, 0.82f, 0.9f);

            Button btn = CreateButton("UpgradeButton", card.transform, "연구 강화 (250G + 1P)", 18, new Color(0.85f, 0.45f, 0.25f));
            RectTransform btnRt = btn.GetComponent<RectTransform>();
            btnRt.sizeDelta = new Vector2(330, 50);
            btnRt.anchoredPosition = new Vector2(0, -85);

            string path = "Assets/Prefabs/ResearchCardPrefab.prefab";
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(card, path);
            Object.DestroyImmediate(card);
            return prefab;
        }

        private static GameObject CreateAchievementPanel(Transform parent)
        {
            GameObject panel = CreateUIObject("AchievementPanel", parent);
            RectTransform rt = panel.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;

            AchievementUI ui = panel.AddComponent<AchievementUI>();

            // 상단 진행도 요약 헤더
            Text progressTxt = CreateText("ProgressSummaryText", panel.transform, "🏆 디톡스 실적 달성 현황: 0 / 12 (0%)", 22, TextAnchor.MiddleCenter, new Vector2(0, 310), new Vector2(900, 40));
            progressTxt.color = new Color(1f, 0.88f, 0.35f);
            SetPrivateField(ui, "progressSummaryText", progressTxt);

            // 카드 스크롤 컨테이너
            GameObject scrollObj = CreateUIObject("ScrollArea", panel.transform);
            RectTransform sRt = scrollObj.GetComponent<RectTransform>();
            sRt.anchorMin = new Vector2(0.5f, 0.5f);
            sRt.anchorMax = new Vector2(0.5f, 0.5f);
            sRt.sizeDelta = new Vector2(1400, 580);
            sRt.anchoredPosition = new Vector2(0, -25);

            VerticalLayoutGroup vlg = scrollObj.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 10;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;

            SetPrivateField(ui, "cardContainer", scrollObj.transform);

            return panel;
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
