using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DopamineCleaner.Core;
using DopamineCleaner.Weapons;
using DopamineCleaner.Enemies;
using DopamineCleaner.Meta;

namespace DopamineCleaner.UI
{
    public class EvolutionTreeModalUI : MonoBehaviour
    {
        private static EvolutionTreeModalUI instance;
        public static EvolutionTreeModalUI Instance
        {
            get
            {
                if (instance == null) instance = FindAnyObjectByType<EvolutionTreeModalUI>();
                return instance;
            }
            private set => instance = value;
        }

        public bool IsOpen => isOpen;

        private struct EvolutionRecipe
        {
            public string weaponName;
            public UpgradeType weaponType;
            public string passiveReq;
            public UpgradeType passiveType;
            public string evoName;
            public UpgradeType evoType;
            public string evoDesc;
        }

        private struct MonsterCodexEntry
        {
            public string name;
            public string category;
            public string spritePath;
            public string stars;
            public string description;
            public EnemyType type;
        }

        [Header("UI 컴포넌트")]
        private GameObject panelObj;
        private Transform listContent;
        private Transform monsterContent;
        private GameObject evoScrollObj;
        private GameObject monsterScrollObj;
        private Button tabEvoBtn;
        private Button tabMonsterBtn;
        private Text tabEvoTxt;
        private Text tabMonsterTxt;
        private bool isOpen = false;
        private int currentTab = 0; // 0: 진화 계통도, 1: 몬스터 도감

        private Font uiFont;

        private readonly List<EvolutionRecipe> recipes = new()
        {
            new EvolutionRecipe { weaponName = "클린 탄환", weaponType = UpgradeType.CleanBullet, passiveReq = "에스프레소 (공속 Lv.1+)", passiveType = UpgradeType.AttackSpeed, evoName = "기가 개틀링 소독기", evoType = UpgradeType.EvoGigaGatling, evoDesc = "8방향 고속 연사 탄막으로 전방을 융단 폭격합니다." },
            new EvolutionRecipe { weaponName = "가디언 위성", weaponType = UpgradeType.OrbitBarrier, passiveReq = "하트 비타민 (체력 Lv.1+)", passiveType = UpgradeType.MaxHP, evoName = "은하계 정화 결계", evoType = UpgradeType.EvoGalacticCleanse, evoDesc = "6개의 초거대 황금 위성이 접근하는 모든 잡생각을 갈아버립니다." },
            new EvolutionRecipe { weaponName = "디톡스 레이저", weaponType = UpgradeType.LaserBeam, passiveReq = "집중력 앰플 (공격력 Lv.1+)", passiveType = UpgradeType.DamageBoost, evoName = "프리즘 정화포", evoType = UpgradeType.EvoPrismCannon, evoDesc = "십자 4방향으로 화면을 관통하는 거대 프리즘 레이저를 발사합니다." },
            new EvolutionRecipe { weaponName = "방화벽 지뢰", weaponType = UpgradeType.FirewallMine, passiveReq = "달콤한 신발 (이속 Lv.1+)", passiveType = UpgradeType.MoveSpeed, evoName = "절대 보안 결계", evoType = UpgradeType.EvoSecurityMatrix, evoDesc = "발밑에 초대형 연쇄 폭발 지뢰를 맹렬히 투하합니다." },
            new EvolutionRecipe { weaponName = "쇼크 펄스", weaponType = UpgradeType.ShockAura, passiveReq = "에스프레소 (공속 Lv.1+)", passiveType = UpgradeType.AttackSpeed, evoName = "테슬라 뇌파 방전", evoType = UpgradeType.EvoTeslaDischarge, evoDesc = "화면 내 다수의 적에게 강력한 연쇄 번개 폭풍을 내리꽂습니다." },
            new EvolutionRecipe { weaponName = "디톡스 미사일", weaponType = UpgradeType.DetoxMissile, passiveReq = "집중력 앰플 (공격력 Lv.1+)", passiveType = UpgradeType.DamageBoost, evoName = "궤도 폭격 이지스", evoType = UpgradeType.EvoAegisBarrage, evoDesc = "위성 궤도에서 대량의 스마트 탄도 미사일이 화면 전체를 맹폭합니다." },
            new EvolutionRecipe { weaponName = "알고리즘 회오리", weaponType = UpgradeType.AlgorithmVortex, passiveReq = "달콤한 신발 (이속 Lv.1+)", passiveType = UpgradeType.MoveSpeed, evoName = "하이퍼 디톡스 타이푼", evoType = UpgradeType.EvoDetoxTyphoon, evoDesc = "황금빛 거대 회오리가 화면을 휩쓸며 몬스터를 영구 흡입 분쇄합니다." },
            new EvolutionRecipe { weaponName = "멸균 소독 안개", weaponType = UpgradeType.SanitizerMist, passiveReq = "도파민 자석 (자석 Lv.1+)", passiveType = UpgradeType.Magnet, evoName = "절대 멸균 플라즈마", evoType = UpgradeType.EvoPlasmaCloud, evoDesc = "플레이어 주위에 상시 유지되는 거대 청록색 플라즈마 영역을 생성합니다." },
            new EvolutionRecipe { weaponName = "사이버 낫", weaponType = UpgradeType.CyberScythe, passiveReq = "통찰의 안경 (치명타 Lv.1+)", passiveType = UpgradeType.CritChance, evoName = "절대 셧다운 엑스칼리버", evoType = UpgradeType.EvoShutdownBlade, evoDesc = "화면 전체를 360도로 베어 넘기며 약화된 일반 몬스터를 즉사시킵니다." }
        };

        private readonly List<MonsterCodexEntry> monsters = new()
        {
            new MonsterCodexEntry { name = "잡생각 먼지", category = "일반 잡념", spritePath = "Assets/Sprites/Enemies/enemy_dust.png", stars = "★☆☆☆☆", description = "도파민 중독의 가장 기초적인 잡생각. 개체는 약하지만 떼지어 몰려듭니다.", type = EnemyType.MindDust },
            new MonsterCodexEntry { name = "스마트폰 알림 유령", category = "비행 교란", spritePath = "Assets/Sprites/Enemies/enemy_ghost.png", stars = "★★☆☆☆", description = "끊임없이 울리는 SNS 푸시 알림의 원혼. 벽을 통과하며 빠른 속도로 접근합니다.", type = EnemyType.NotificationGhost },
            new MonsterCodexEntry { name = "미루기 나무늘보", category = "중장갑 탱커", spritePath = "Assets/Sprites/Enemies/enemy_sloth.png", stars = "★★★☆☆", description = "'내일부터 해야지'라는 게으름의 정수. 엄청난 맷집으로 플레이어의 길을 틀어막습니다.", type = EnemyType.HeavySloth },
            new MonsterCodexEntry { name = "쇼츠 중독 러너", category = "돌진 암살", spritePath = "Assets/Sprites/Enemies/enemy_shorts.png", stars = "★★★☆☆", description = "도파민 쇼츠를 빠르게 넘겨보는 손가락 속도로 지그재그 기습 돌진합니다.", type = EnemyType.ShortsRunner },
                        new MonsterCodexEntry { name = "스팸 악플 슈터", category = "원거리 사격", spritePath = "Assets/Sprites/Enemies/enemy_spam.png", stars = "★★★★☆", description = "화면 밖에서 독성 댓글 투사체를 조준 발사하여 정신력을 깎아먹습니다.", type = EnemyType.SpamShooter },
            new MonsterCodexEntry { name = "둠스크롤러 골렘", category = "심연 흡입", spritePath = "Assets/Sprites/Enemies/enemy_doomscroller.png", stars = "★★★★★", description = "스마트폰 알고리즘의 심연에서 서식하는 거대 골렘. 주변 시공간과 플레이어를 빨아들입니다.", type = EnemyType.DoomScroller },
            new MonsterCodexEntry { name = "불시 특별 감사관", category = "중간 보스", spritePath = "Assets/Sprites/Enemies/enemy_boss.png", stars = "★★★★★", description = "야근과 업무 압박의 화신. 체력이 감소하면 광폭화 360도 나선 탄막을 난사합니다.", type = EnemyType.AuditorKim }
        };

        private void Awake()
        {
            if (instance == null || instance == this)
            {
                instance = this;
            }
            else
            {
                if (Application.isPlaying) Destroy(gameObject);
                else DestroyImmediate(gameObject);
                return;
            }

            LoadFont();
            BuildModalUI();
        }

        private void LoadFont()
        {
            uiFont = Font.CreateDynamicFontFromOSFont(new string[] { "Malgun Gothic", "맑은 고딕", "Arial" }, 18);
            if (uiFont == null)
            {
                uiFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }
        }

        private void Update()
        {
            if (isOpen && Input.GetKeyDown(KeyCode.Escape))
            {
                CloseModal();
                return;
            }

            // Tab 키로 인게임에서 진화 족보 모달 열기/닫기
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                if (GameManager.Instance != null &&
                    (GameManager.Instance.CurrentState == GameState.GameOver || GameManager.Instance.CurrentState == GameState.LevelUp))
                {
                    return; // 게임오버나 레벨업 중에는 토글 불가
                }

                ToggleModal();
            }
        }

        public void ToggleModal()
        {
            if (panelObj == null) return;

            isOpen = !isOpen;
            panelObj.SetActive(isOpen);

            if (isOpen)
            {
                Time.timeScale = 0f;
                SwitchTab(currentTab);
                SoundManager.Instance?.Play(SoundEffect.ButtonClick, 0.7f);
            }
            else
            {
                Time.timeScale = 1f;
                SoundManager.Instance?.Play(SoundEffect.ButtonClick, 0.7f);
            }
        }

        public void OpenCodex(int tab = 0)
        {
            if (panelObj == null) BuildModalUI();
            isOpen = true;
            panelObj.SetActive(true);
            Time.timeScale = 0f;
            SwitchTab(tab);
            SoundManager.Instance?.Play(SoundEffect.ButtonClick, 0.7f);
        }

        public void CloseModal()
        {
            if (panelObj != null) panelObj.SetActive(false);
            isOpen = false;
            Time.timeScale = 1f;
        }

        private void SwitchTab(int tabIndex)
        {
            currentTab = tabIndex;
            bool isEvo = tabIndex == 0;

            if (evoScrollObj != null) evoScrollObj.SetActive(isEvo);
            if (monsterScrollObj != null) monsterScrollObj.SetActive(!isEvo);

            Color activeBg = new Color(0.35f, 0.28f, 0.65f, 1f);
            Color inactiveBg = new Color(0.18f, 0.16f, 0.26f, 0.95f);

            if (tabEvoBtn != null) tabEvoBtn.GetComponent<Image>().color = isEvo ? activeBg : inactiveBg;
            if (tabMonsterBtn != null) tabMonsterBtn.GetComponent<Image>().color = !isEvo ? activeBg : inactiveBg;

            if (isEvo)
            {
                RefreshRecipeCards();
            }
            else
            {
                RefreshMonsterCards();
            }
        }

        private void BuildModalUI()
        {
            Canvas canvas = GetComponentInParent<Canvas>() ?? FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            panelObj = new GameObject("EvolutionTreeModal");
            panelObj.transform.SetParent(canvas.transform, false);

            RectTransform prt = panelObj.AddComponent<RectTransform>();
            prt.anchorMin = Vector2.zero;
            prt.anchorMax = Vector2.one;
            prt.offsetMin = Vector2.zero;
            prt.offsetMax = Vector2.zero;

            Image dim = panelObj.AddComponent<Image>();
            dim.color = new Color(0.04f, 0.03f, 0.08f, 0.92f); // 딥 다크 배경

            // 메인 프레임 창
            GameObject frame = new GameObject("Frame");
            frame.transform.SetParent(panelObj.transform, false);
            RectTransform frt = frame.AddComponent<RectTransform>();
            frt.anchorMin = new Vector2(0.5f, 0.5f);
            frt.anchorMax = new Vector2(0.5f, 0.5f);
            frt.sizeDelta = new Vector2(1160, 780);

            Image fBg = frame.AddComponent<Image>();
            fBg.color = new Color(0.10f, 0.09f, 0.16f, 0.98f);
            var outline = frame.AddComponent<Outline>();
            outline.effectColor = new Color(0.45f, 0.35f, 0.85f, 0.9f);
            outline.effectDistance = new Vector2(3f, -3f);

            // 헤더 타이틀
            GameObject header = new GameObject("HeaderTitle");
            header.transform.SetParent(frame.transform, false);
            RectTransform hrt = header.AddComponent<RectTransform>();
            hrt.anchorMin = new Vector2(0.5f, 1f);
            hrt.anchorMax = new Vector2(0.5f, 1f);
            hrt.anchoredPosition = new Vector2(0, -42);
            hrt.sizeDelta = new Vector2(1000, 46);

            Text hTxt = header.AddComponent<Text>();
            hTxt.font = uiFont;
            hTxt.fontSize = 28;
            hTxt.fontStyle = FontStyle.Bold;
            hTxt.alignment = TextAnchor.MiddleCenter;
            hTxt.color = new Color(1f, 0.88f, 0.35f);
            hTxt.text = "📜 도파민 정화 아카이브 (Dopamine Codex) 📜";

            // 탭 내비게이션 바 (진화 계통도 / 몬스터 도감)
            GameObject tabNav = new GameObject("TabNav");
            tabNav.transform.SetParent(frame.transform, false);
            RectTransform tnRt = tabNav.AddComponent<RectTransform>();
            tnRt.anchorMin = new Vector2(0.5f, 1f);
            tnRt.anchorMax = new Vector2(0.5f, 1f);
            tnRt.anchoredPosition = new Vector2(0, -90);
            tnRt.sizeDelta = new Vector2(800, 44);

            tabEvoBtn = CreateTabButton("TabEvo", tabNav.transform, "⚔️ 무기 진화 계통도 (Evolution Tree)", new Vector2(-205, 0), () => SwitchTab(0));
            tabMonsterBtn = CreateTabButton("TabMonster", tabNav.transform, "👾 몬스터 정화 도감 (Bestiary)", new Vector2(205, 0), () => SwitchTab(1));

            // 닫기 버튼
            GameObject closeBtnObj = new GameObject("CloseButton");
            closeBtnObj.transform.SetParent(frame.transform, false);
            RectTransform cbrt = closeBtnObj.AddComponent<RectTransform>();
            cbrt.anchorMin = new Vector2(1f, 1f);
            cbrt.anchorMax = new Vector2(1f, 1f);
            cbrt.anchoredPosition = new Vector2(-36, -36);
            cbrt.sizeDelta = new Vector2(46, 46);

            Image cbImg = closeBtnObj.AddComponent<Image>();
            cbImg.color = new Color(0.85f, 0.25f, 0.35f);
            Button cBtn = closeBtnObj.AddComponent<Button>();
            cBtn.onClick.AddListener(CloseModal);

            GameObject cbTxtObj = new GameObject("CloseText");
            cbTxtObj.transform.SetParent(closeBtnObj.transform, false);
            Text cbTxt = cbTxtObj.AddComponent<Text>();
            cbTxt.font = uiFont;
            cbTxt.fontSize = 22;
            cbTxt.fontStyle = FontStyle.Bold;
            cbTxt.alignment = TextAnchor.MiddleCenter;
            cbTxt.color = Color.white;
            cbTxt.text = "✕";
            RectTransform cbtrt = cbTxtObj.GetComponent<RectTransform>();
            cbtrt.anchorMin = Vector2.zero;
            cbtrt.anchorMax = Vector2.one;
            cbtrt.offsetMin = Vector2.zero;
            cbtrt.offsetMax = Vector2.zero;

            // 1. 진화 족보 스크롤 뷰
            evoScrollObj = CreateScrollView("EvoScrollView", frame.transform, out listContent);

            // 2. 몬스터 도감 스크롤 뷰
            monsterScrollObj = CreateScrollView("MonsterScrollView", frame.transform, out monsterContent);

            // 하단 조작 안내 힌트
            GameObject bottomGuide = new GameObject("BottomGuide");
            bottomGuide.transform.SetParent(frame.transform, false);
            RectTransform bgRt = bottomGuide.AddComponent<RectTransform>();
            bgRt.anchorMin = new Vector2(0.5f, 0f);
            bgRt.anchorMax = new Vector2(0.5f, 0f);
            bgRt.anchoredPosition = new Vector2(0, 25);
            bgRt.sizeDelta = new Vector2(1000, 30);

            Text bgTxt = bottomGuide.AddComponent<Text>();
            bgTxt.font = uiFont;
            bgTxt.fontSize = 15;
            bgTxt.alignment = TextAnchor.MiddleCenter;
            bgTxt.color = new Color(0.65f, 0.70f, 0.85f);
            bgTxt.text = "💡 [Tab] 키 또는 상단 [✕] 버튼을 눌러 창을 닫을 수 있습니다.";

            panelObj.SetActive(false);
        }

        private Button CreateTabButton(string name, Transform parent, string label, Vector2 pos, UnityEngine.Events.UnityAction action)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            RectTransform rt = obj.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(390, 42);
            rt.anchoredPosition = pos;

            Image img = obj.AddComponent<Image>();
            img.color = new Color(0.22f, 0.20f, 0.32f);

            var outline = obj.AddComponent<Outline>();
            outline.effectColor = new Color(0.40f, 0.35f, 0.60f, 0.8f);
            outline.effectDistance = new Vector2(1, -1);

            Button btn = obj.AddComponent<Button>();
            btn.onClick.AddListener(action);

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(obj.transform, false);
            RectTransform trt = textObj.AddComponent<RectTransform>();
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.offsetMin = Vector2.zero;
            trt.offsetMax = Vector2.zero;

            Text txt = textObj.AddComponent<Text>();
            txt.font = uiFont;
            txt.fontSize = 17;
            txt.fontStyle = FontStyle.Bold;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = Color.white;
            txt.text = label;

            return btn;
        }

        private GameObject CreateScrollView(string name, Transform parent, out Transform content)
        {
            GameObject scrollObj = new GameObject(name);
            scrollObj.transform.SetParent(parent, false);
            RectTransform scrt = scrollObj.AddComponent<RectTransform>();
            scrt.anchorMin = new Vector2(0.5f, 0.5f);
            scrt.anchorMax = new Vector2(0.5f, 0.5f);
            scrt.anchoredPosition = new Vector2(0, -38);
            scrt.sizeDelta = new Vector2(1100, 580);

            ScrollRect sr = scrollObj.AddComponent<ScrollRect>();
            sr.horizontal = false;
            sr.vertical = true;

            Image maskImg = scrollObj.AddComponent<Image>();
            maskImg.color = new Color(0, 0, 0, 0.15f);
            Mask mask = scrollObj.AddComponent<Mask>();
            mask.showMaskGraphic = false;

            GameObject contentObj = new GameObject("Content");
            contentObj.transform.SetParent(scrollObj.transform, false);
            RectTransform crt = contentObj.AddComponent<RectTransform>();
            crt.anchorMin = new Vector2(0f, 1f);
            crt.anchorMax = new Vector2(1f, 1f);
            crt.pivot = new Vector2(0.5f, 1f);
            crt.anchoredPosition = Vector2.zero;
            crt.sizeDelta = new Vector2(0, 1000);

            VerticalLayoutGroup vlg = contentObj.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 10;
            vlg.padding = new RectOffset(15, 15, 12, 12);
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;

            ContentSizeFitter csf = contentObj.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            sr.content = crt;
            content = contentObj.transform;
            return scrollObj;
        }

        private void RefreshRecipeCards()
        {
            if (listContent == null) return;

            for (int i = listContent.childCount - 1; i >= 0; i--)
            {
                Destroy(listContent.GetChild(i).gameObject);
            }

            WeaponManager wm = WeaponManager.Instance;

            foreach (var r in recipes)
            {
                GameObject card = new GameObject($"Card_{r.weaponName}");
                card.transform.SetParent(listContent, false);

                RectTransform crt = card.AddComponent<RectTransform>();
                crt.sizeDelta = new Vector2(1070, 96);

                Image bg = card.AddComponent<Image>();

                bool hasWeapon = false;
                int weaponLv = 0;
                bool isEvolved = false;
                bool hasPassive = false;

                if (wm != null)
                {
                    GetWeaponStatus(wm, r.weaponName, out hasWeapon, out weaponLv, out isEvolved);
                    hasPassive = CheckPassiveStatus(wm, r.passiveType);
                }

                // 상태에 따른 비주얼 테두리 및 색상
                var cardOutline = card.AddComponent<Outline>();
                if (isEvolved)
                {
                    bg.color = new Color(0.12f, 0.28f, 0.20f, 0.95f); // 에메랄드 완료
                    cardOutline.effectColor = new Color(0.0f, 0.9f, 0.45f, 0.9f);
                    cardOutline.effectDistance = new Vector2(2, -2);
                }
                else if (hasWeapon && weaponLv >= 5 && hasPassive)
                {
                    bg.color = new Color(0.32f, 0.26f, 0.10f, 0.95f); // 골드 준비 완료
                    cardOutline.effectColor = new Color(1.0f, 0.85f, 0.2f, 1f);
                    cardOutline.effectDistance = new Vector2(2, -2);
                }
                else if (hasWeapon || hasPassive)
                {
                    bg.color = new Color(0.15f, 0.15f, 0.25f, 0.95f); // 파랑 일부 보유
                    cardOutline.effectColor = new Color(0.35f, 0.50f, 0.85f, 0.7f);
                    cardOutline.effectDistance = new Vector2(1, -1);
                }
                else
                {
                    bg.color = new Color(0.11f, 0.10f, 0.16f, 0.85f); // 미획득
                    cardOutline.effectColor = new Color(0.25f, 0.22f, 0.35f, 0.5f);
                    cardOutline.effectDistance = new Vector2(1, -1);
                }

                // 도트 아이콘 3종 배치 컨테이너
                CreateRecipeRowUI(card.transform, r, hasWeapon, weaponLv, hasPassive, isEvolved);
            }
        }

        private void CreateRecipeRowUI(Transform parent, EvolutionRecipe r, bool hasWeapon, int weaponLv, bool hasPassive, bool isEvolved)
        {
            // 1. 무기 도트 아이콘
            Sprite wSprite = UpgradeCardUI.GetUpgradeSprite(r.weaponType);
            CreateIconSlot(parent, wSprite, new Vector2(-460, 0), hasWeapon ? new Color(0.20f, 0.35f, 0.60f) : new Color(0.15f, 0.14f, 0.20f));

            // 2. 무기 이름 & 레벨
            string wColor = hasWeapon ? "#60A5FA" : "#94A3B8";
            string wLvStr = hasWeapon ? $"(Lv.{weaponLv}/5)" : "(미보유)";
            CreateTextObject(parent, $"<color={wColor}><b>{r.weaponName}</b></color>\n<size=13><color=#94A3B8>{wLvStr}</color></size>", 17, new Vector2(-360, 0), new Vector2(150, 48), TextAnchor.MiddleLeft);

            // 3. 더하기 기호
            CreateTextObject(parent, "+", 24, new Vector2(-260, 0), new Vector2(40, 40), TextAnchor.MiddleCenter);

            // 4. 패시브 도트 아이콘
            Sprite pSprite = UpgradeCardUI.GetUpgradeSprite(r.passiveType);
            CreateIconSlot(parent, pSprite, new Vector2(-200, 0), hasPassive ? new Color(0.10f, 0.38f, 0.35f) : new Color(0.15f, 0.14f, 0.20f));

            // 5. 패시브 조건 이름
            string pColor = hasPassive ? "#34D399" : "#94A3B8";
            string pTag = hasPassive ? "✅보유중" : "❌미보유";
            CreateTextObject(parent, $"<color={pColor}><b>{r.passiveReq}</b></color>\n<size=13><color=#94A3B8>[{pTag}]</color></size>", 16, new Vector2(-100, 0), new Vector2(160, 48), TextAnchor.MiddleLeft);

            // 6. 진화 화살표
            CreateTextObject(parent, "➔", 24, new Vector2(10, 0), new Vector2(40, 40), TextAnchor.MiddleCenter);

            // 7. 진화 무기 도트 아이콘 (황금 플레이트)
            Sprite eSprite = UpgradeCardUI.GetUpgradeSprite(r.evoType);
            CreateIconSlot(parent, eSprite, new Vector2(70, 0), isEvolved ? new Color(0.45f, 0.35f, 0.10f) : new Color(0.20f, 0.18f, 0.28f));

            // 8. 진화 무기 이름 & 설명 & 상태 뱃지
            string statusBadge;
            if (isEvolved)
                statusBadge = "<color=#10B981>👑 <b>[초월 진화 완료]</b></color>";
            else if (hasWeapon && weaponLv >= 5 && hasPassive)
                statusBadge = "<color=#F59E0B>⚡ <b>[진화 조건 완료! 레벨업 대기]</b></color>";
            else if (hasWeapon)
                statusBadge = $"<color=#38BDF8>🔹 [진화 진행중 {weaponLv}/5]</color>";
            else
                statusBadge = "<color=#64748B>🔒 [미개방]</color>";

            string descBlock = $"<color=#FBBF24><b>{r.evoName}</b></color>  |  {statusBadge}\n" +
                               $"<size=13><color=#CBD5E1>{r.evoDesc}</color></size>";
            CreateTextObject(parent, descBlock, 16, new Vector2(300, 0), new Vector2(400, 56), TextAnchor.MiddleLeft);
        }

        private void CreateIconSlot(Transform parent, Sprite sprite, Vector2 pos, Color bgColor)
        {
            GameObject slot = new GameObject("IconSlot");
            slot.transform.SetParent(parent, false);
            RectTransform srt = slot.AddComponent<RectTransform>();
            srt.anchoredPosition = pos;
            srt.sizeDelta = new Vector2(52, 52);

            Image bg = slot.AddComponent<Image>();
            bg.color = bgColor;

            var outl = slot.AddComponent<Outline>();
            outl.effectColor = new Color(0.40f, 0.35f, 0.55f, 0.6f);
            outl.effectDistance = new Vector2(1, -1);

            if (sprite != null)
            {
                GameObject imgObj = new GameObject("Icon");
                imgObj.transform.SetParent(slot.transform, false);
                RectTransform irt = imgObj.AddComponent<RectTransform>();
                irt.sizeDelta = new Vector2(44, 44);
                Image img = imgObj.AddComponent<Image>();
                img.sprite = sprite;
                img.preserveAspect = true;
            }
        }

        private void CreateTextObject(Transform parent, string text, int fontSize, Vector2 pos, Vector2 size, TextAnchor align)
        {
            GameObject obj = new GameObject("Text");
            obj.transform.SetParent(parent, false);
            RectTransform rt = obj.AddComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            Text t = obj.AddComponent<Text>();
            t.font = uiFont;
            t.fontSize = fontSize;
            t.alignment = align;
            t.supportRichText = true;
            t.text = text;
            t.color = Color.white;
            t.lineSpacing = 1.2f;
        }

        private void RefreshMonsterCards()
        {
            if (monsterContent == null) return;

            for (int i = monsterContent.childCount - 1; i >= 0; i--)
            {
                Destroy(monsterContent.GetChild(i).gameObject);
            }

            foreach (var m in monsters)
            {
                GameObject card = new GameObject($"MonsterCard_{m.name}");
                card.transform.SetParent(monsterContent, false);

                RectTransform crt = card.AddComponent<RectTransform>();
                crt.sizeDelta = new Vector2(1070, 96);

                Image bg = card.AddComponent<Image>();
                bg.color = new Color(0.12f, 0.11f, 0.18f, 0.95f);

                var outl = card.AddComponent<Outline>();
                outl.effectColor = new Color(0.35f, 0.25f, 0.50f, 0.7f);
                outl.effectDistance = new Vector2(1, -1);

                // 몬스터 스프라이트 로드
                Sprite mSprite = null;
#if UNITY_EDITOR
                mSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(m.spritePath);
#endif
                CreateIconSlot(card.transform, mSprite, new Vector2(-460, 0), new Vector2(64, 64), new Color(0.20f, 0.16f, 0.28f));

                // 몬스터 정보 블록
                string starColor = m.stars.Contains("★★★★") ? "#EF4444" : "#F59E0B";
                string titleBlock = $"<color=#F8FAFC><b>{m.name}</b></color>  <size=13><color=#94A3B8>[{m.category}]</color></size>  <color={starColor}><b>{m.stars}</b></color>\n" +
                                   $"<size=14><color=#CBD5E1>{m.description}</color></size>";
                CreateTextObject(card.transform, titleBlock, 17, new Vector2(-100, 0), new Vector2(620, 56), TextAnchor.MiddleLeft);

                // 정화 현황 배지
                int killCount = GameManager.Instance != null ? GameManager.Instance.TotalKills : 0;
                string statusText = $"<color=#38BDF8><b>디톡스 정화 대상</b></color>\n<size=13><color=#A7F3D0>위험도: {m.stars}</color></size>";
                CreateTextObject(card.transform, statusText, 15, new Vector2(400, 0), new Vector2(200, 50), TextAnchor.MiddleCenter);
            }
        }

        private void CreateIconSlot(Transform parent, Sprite sprite, Vector2 pos, Vector2 size, Color bgColor)
        {
            GameObject slot = new GameObject("MonsterIconSlot");
            slot.transform.SetParent(parent, false);
            RectTransform srt = slot.AddComponent<RectTransform>();
            srt.anchoredPosition = pos;
            srt.sizeDelta = size;

            Image bg = slot.AddComponent<Image>();
            bg.color = bgColor;

            var outl = slot.AddComponent<Outline>();
            outl.effectColor = new Color(0.45f, 0.35f, 0.65f, 0.7f);
            outl.effectDistance = new Vector2(1, -1);

            if (sprite != null)
            {
                GameObject imgObj = new GameObject("Icon");
                imgObj.transform.SetParent(slot.transform, false);
                RectTransform irt = imgObj.AddComponent<RectTransform>();
                irt.sizeDelta = size * 0.85f;
                Image img = imgObj.AddComponent<Image>();
                img.sprite = sprite;
                img.preserveAspect = true;
            }
        }

        private void GetWeaponStatus(WeaponManager wm, string name, out bool hasWeapon, out int level, out bool isEvolved)
        {
            hasWeapon = false;
            level = 0;
            isEvolved = false;

            WeaponBase target = null;
            if (name.Contains("탄환")) target = wm.BulletWeapon;
            else if (name.Contains("위성")) target = wm.OrbitWeapon;
            else if (name.Contains("레이저")) target = wm.LaserWeapon;
            else if (name.Contains("지뢰")) target = wm.MineWeapon;
            else if (name.Contains("쇼크")) target = wm.ShockWeapon;
            else if (name.Contains("미사일")) target = wm.MissileWeapon;
            else if (name.Contains("회오리")) target = wm.VortexWeapon;
            else if (name.Contains("안개")) target = wm.MistWeapon;
            else if (name.Contains("낫")) target = wm.ScytheWeapon;

            if (target != null)
            {
                hasWeapon = target.Level > 0;
                level = target.Level;
                isEvolved = target.IsEvolved;
            }
        }

        private bool CheckPassiveStatus(WeaponManager wm, UpgradeType pType)
        {
            switch (pType)
            {
                case UpgradeType.AttackSpeed: return wm.AttackSpeedLevel >= 1;
                case UpgradeType.MaxHP: return wm.MaxHPLevel >= 1;
                case UpgradeType.DamageBoost: return wm.DamageBoostLevel >= 1;
                case UpgradeType.MoveSpeed: return wm.MoveSpeedLevel >= 1;
                case UpgradeType.Magnet: return wm.MagnetLevel >= 1;
                case UpgradeType.CritChance: return wm.CritChanceLevel >= 1;
                default: return false;
            }
        }
    }
}
