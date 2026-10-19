using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DopamineCleaner.Core;
using DopamineCleaner.Weapons;

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

        private struct EvolutionRecipe
        {
            public string weaponName;
            public string weaponIcon;
            public string passiveReq;
            public string evoName;
            public string evoIcon;
            public string evoDesc;
            public UpgradeType evoType;
        }

        [Header("UI 컴포넌트")]
        private GameObject panelObj;
        private Transform listContent;
        private bool isOpen = false;

        private readonly List<EvolutionRecipe> recipes = new()
        {
            new EvolutionRecipe { weaponName = "클린 탄환", weaponIcon = "💧", passiveReq = "공격력 증폭 (Lv.1+)", evoName = "기가 개틀링 소독기", evoIcon = "🔫", evoDesc = "원형으로 맹렬한 탄환 폭풍을 난사합니다.", evoType = UpgradeType.EvoGigaGatling },
            new EvolutionRecipe { weaponName = "가디언 위성", weaponIcon = "🛡️", passiveReq = "최대 체력 증강 (Lv.1+)", evoName = "은하계 정화 결계", evoIcon = "🪐", evoDesc = "6개의 초거대 황금 위성이 접근하는 모든 적을 갈아버립니다.", evoType = UpgradeType.EvoGalacticCleanse },
            new EvolutionRecipe { weaponName = "디톡스 레이저", weaponIcon = "🔦", passiveReq = "자석 반경 확대 (Lv.1+)", evoName = "프리즘 정화포", evoIcon = "🌈", evoDesc = "십자 4방향으로 화면을 관통하는 거대 프리즘 레이저를 발사합니다.", evoType = UpgradeType.EvoPrismCannon },
            new EvolutionRecipe { weaponName = "방화벽 지뢰", weaponIcon = "💣", passiveReq = "이동속도 증가 (Lv.1+)", evoName = "절대 보안 결계", evoIcon = "💥", evoDesc = "발밑에 초대형 연쇄 폭발 지뢰를 맹렬히 투하합니다.", evoType = UpgradeType.EvoSecurityMatrix },
            new EvolutionRecipe { weaponName = "쇼크 펄스", weaponIcon = "⚡", passiveReq = "공격속도 증가 (Lv.1+)", evoName = "테슬라 뇌파 방전", evoIcon = "⚡", evoDesc = "화면 내 다수의 적에게 강력한 연쇄 번개 폭풍을 내리꽂습니다.", evoType = UpgradeType.EvoTeslaDischarge },
            new EvolutionRecipe { weaponName = "디톡스 미사일", weaponIcon = "🚀", passiveReq = "공속 또는 공격력 (Lv.1+)", evoName = "궤도 폭격 이지스", evoIcon = "🚀", evoDesc = "위성 궤도에서 대량의 스마트 탄도 미사일이 화면 전체를 맹폭합니다.", evoType = UpgradeType.EvoAegisBarrage },
            new EvolutionRecipe { weaponName = "디톡스 볼텍스", weaponIcon = "🌪️", passiveReq = "이동속도 증가 (Lv.1+)", evoName = "하이퍼 타이푼", evoIcon = "🌪️", evoDesc = "황금빛 거대 회오리가 화면을 휩쓸며 몬스터를 영구 흡입 분쇄합니다.", evoType = UpgradeType.EvoDetoxTyphoon },
            new EvolutionRecipe { weaponName = "나노 미스트", weaponIcon = "🧪", passiveReq = "자석 반경 확대 (Lv.1+)", evoName = "절대 멸균 플라즈마", evoIcon = "🧪", evoDesc = "플레이어 주위에 상시 유지되는 거대 청록색 플라즈마 영역을 생성합니다.", evoType = UpgradeType.EvoPlasmaCloud },
            new EvolutionRecipe { weaponName = "사이버 낫", weaponIcon = "🪓", passiveReq = "체력 또는 치명타 (Lv.1+)", evoName = "절대 셧다운 엑스칼리버", evoIcon = "🪓", evoDesc = "화면 전체를 360도로 베어 넘기며 약화된 일반 몬스터를 즉사시킵니다.", evoType = UpgradeType.EvoShutdownBlade }
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

            BuildModalUI();
        }

        private void Update()
        {
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
                RefreshRecipeCards();
                SoundManager.Instance?.Play(SoundEffect.ButtonClick, 0.7f);
            }
            else
            {
                Time.timeScale = 1f;
                SoundManager.Instance?.Play(SoundEffect.ButtonClick, 0.7f);
            }
        }

        private void BuildModalUI()
        {
            Canvas canvas = FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            panelObj = new GameObject("EvolutionTreeModal");
            panelObj.transform.SetParent(canvas.transform, false);

            RectTransform prt = panelObj.AddComponent<RectTransform>();
            prt.anchorMin = Vector2.zero;
            prt.anchorMax = Vector2.one;
            prt.offsetMin = Vector2.zero;
            prt.offsetMax = Vector2.zero;

            Image dim = panelObj.AddComponent<Image>();
            dim.color = new Color(0.04f, 0.03f, 0.07f, 0.88f); // 딤드 어두운 배경

            // 메인 프레임 창
            GameObject frame = new GameObject("Frame");
            frame.transform.SetParent(panelObj.transform, false);
            RectTransform frt = frame.AddComponent<RectTransform>();
            frt.anchorMin = new Vector2(0.5f, 0.5f);
            frt.anchorMax = new Vector2(0.5f, 0.5f);
            frt.sizeDelta = new Vector2(1100, 750);

            Image fBg = frame.AddComponent<Image>();
            fBg.color = new Color(0.12f, 0.10f, 0.18f, 0.98f);
            var outline = frame.AddComponent<Outline>();
            outline.effectColor = new Color(0.4f, 0.7f, 1f, 0.8f);
            outline.effectDistance = new Vector2(3f, -3f);

            // 헤더 타이틀
            GameObject header = new GameObject("HeaderTitle");
            header.transform.SetParent(frame.transform, false);
            RectTransform hrt = header.AddComponent<RectTransform>();
            hrt.anchorMin = new Vector2(0.5f, 1f);
            hrt.anchorMax = new Vector2(0.5f, 1f);
            hrt.anchoredPosition = new Vector2(0, -45);
            hrt.sizeDelta = new Vector2(1000, 50);

            Text hTxt = header.AddComponent<Text>();
            hTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            hTxt.fontSize = 28;
            hTxt.fontStyle = FontStyle.Bold;
            hTxt.alignment = TextAnchor.MiddleCenter;
            hTxt.color = new Color(1f, 0.88f, 0.35f);
            hTxt.text = "⚡ 실시간 무기 진화 계통도 (Evolution Tree) ⚡";

            // 서브 힌트
            GameObject sub = new GameObject("SubGuide");
            sub.transform.SetParent(frame.transform, false);
            RectTransform srt = sub.AddComponent<RectTransform>();
            srt.anchorMin = new Vector2(0.5f, 1f);
            srt.anchorMax = new Vector2(0.5f, 1f);
            srt.anchoredPosition = new Vector2(0, -85);
            srt.sizeDelta = new Vector2(1000, 30);

            Text sTxt = sub.AddComponent<Text>();
            sTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            sTxt.fontSize = 17;
            sTxt.alignment = TextAnchor.MiddleCenter;
            sTxt.color = new Color(0.75f, 0.8f, 0.9f);
            sTxt.text = "주 무기 Lv.5 달성 + 필요 보조 패시브 1레벨 이상 획득 시 진화 카드가 등장합니다. [Tab 키로 닫기]";

            // 닫기 버튼
            GameObject closeBtnObj = new GameObject("CloseButton");
            closeBtnObj.transform.SetParent(frame.transform, false);
            RectTransform cbrt = closeBtnObj.AddComponent<RectTransform>();
            cbrt.anchorMin = new Vector2(1f, 1f);
            cbrt.anchorMax = new Vector2(1f, 1f);
            cbrt.anchoredPosition = new Vector2(-40, -40);
            cbrt.sizeDelta = new Vector2(50, 50);

            Image cbImg = closeBtnObj.AddComponent<Image>();
            cbImg.color = new Color(0.85f, 0.25f, 0.35f);
            Button cBtn = closeBtnObj.AddComponent<Button>();
            cBtn.onClick.AddListener(ToggleModal);

            GameObject cbTxtObj = new GameObject("CloseText");
            cbTxtObj.transform.SetParent(closeBtnObj.transform, false);
            Text cbTxt = cbTxtObj.AddComponent<Text>();
            cbTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            cbTxt.fontSize = 24;
            cbTxt.fontStyle = FontStyle.Bold;
            cbTxt.alignment = TextAnchor.MiddleCenter;
            cbTxt.color = Color.white;
            cbTxt.text = "X";
            RectTransform cbtrt = cbTxtObj.GetComponent<RectTransform>();
            cbtrt.anchorMin = Vector2.zero;
            cbtrt.anchorMax = Vector2.one;
            cbtrt.offsetMin = Vector2.zero;
            cbtrt.offsetMax = Vector2.zero;

            // 스크롤 뷰 컨테이너
            GameObject scrollObj = new GameObject("ScrollView");
            scrollObj.transform.SetParent(frame.transform, false);
            RectTransform scrt = scrollObj.AddComponent<RectTransform>();
            scrt.anchorMin = new Vector2(0.5f, 0.5f);
            scrt.anchorMax = new Vector2(0.5f, 0.5f);
            scrt.anchoredPosition = new Vector2(0, -35);
            scrt.sizeDelta = new Vector2(1040, 560);

            ScrollRect sr = scrollObj.AddComponent<ScrollRect>();
            sr.horizontal = false;
            sr.vertical = true;

            Image maskImg = scrollObj.AddComponent<Image>();
            maskImg.color = new Color(0, 0, 0, 0.2f);
            Mask mask = scrollObj.AddComponent<Mask>();
            mask.showMaskGraphic = false;

            GameObject contentObj = new GameObject("Content");
            contentObj.transform.SetParent(scrollObj.transform, false);
            RectTransform crt = contentObj.AddComponent<RectTransform>();
            crt.anchorMin = new Vector2(0f, 1f);
            crt.anchorMax = new Vector2(1f, 1f);
            crt.pivot = new Vector2(0.5f, 1f);
            crt.anchoredPosition = Vector2.zero;
            crt.sizeDelta = new Vector2(0, 1100);

            VerticalLayoutGroup vlg = contentObj.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 10;
            vlg.padding = new RectOffset(10, 10, 10, 10);
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;

            ContentSizeFitter csf = contentObj.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            sr.content = crt;
            listContent = contentObj.transform;

            panelObj.SetActive(false);
        }

        private void RefreshRecipeCards()
        {
            if (listContent == null) return;

            // 기존 카드 정리
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
                crt.sizeDelta = new Vector2(1000, 85);

                Image bg = card.AddComponent<Image>();

                // 현재 플레이어의 무기 상태 확인
                bool hasWeapon = false;
                int weaponLv = 0;
                bool isEvolved = false;

                if (wm != null)
                {
                    GetWeaponStatus(wm, r.weaponName, out hasWeapon, out weaponLv, out isEvolved);
                }

                // 상태에 따른 비주얼 테두리 및 색상
                if (isEvolved)
                {
                    bg.color = new Color(0.25f, 0.38f, 0.25f, 0.95f); // 에메랄드 완료
                }
                else if (hasWeapon && weaponLv >= 5)
                {
                    bg.color = new Color(0.38f, 0.32f, 0.15f, 0.95f); // 골드 준비 완료
                }
                else if (hasWeapon)
                {
                    bg.color = new Color(0.18f, 0.20f, 0.28f, 0.95f); // 보유 중
                }
                else
                {
                    bg.color = new Color(0.13f, 0.12f, 0.16f, 0.75f); // 미획득
                }

                GameObject txtObj = new GameObject("RecipeText");
                txtObj.transform.SetParent(card.transform, false);
                RectTransform trt = txtObj.AddComponent<RectTransform>();
                trt.anchorMin = Vector2.zero;
                trt.anchorMax = Vector2.one;
                trt.offsetMin = new Vector2(20, 5);
                trt.offsetMax = new Vector2(-20, -5);

                Text txt = txtObj.AddComponent<Text>();
                txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                txt.fontSize = 20;
                txt.alignment = TextAnchor.MiddleLeft;

                string statusBadge;
                if (isEvolved)
                    statusBadge = "<color=#66FF99>★ [전설 진화 완료]</color>";
                else if (hasWeapon && weaponLv >= 5)
                    statusBadge = "<color=#FFE600>⚡ [진화 레시피 완성 - 다음 레벨업 대기]</color>";
                else if (hasWeapon)
                    statusBadge = $"<color=#66CCFF>🔹 [보유 중: Lv.{weaponLv}/5]</color>";
                else
                    statusBadge = "<color=#888888>🔒 [미획득]</color>";

                txt.text = $"{r.weaponIcon} <b>{r.weaponName}</b> (Lv.5)  +  📦 <b>{r.passiveReq}</b>  ➔  {r.evoIcon} <color=#FFD700><b>{r.evoName}</b></color>\n" +
                           $"    <size=15><color=#D0D0E0>{r.evoDesc}</color></size>   |   {statusBadge}";
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
            else if (name.Contains("볼텍스")) target = wm.VortexWeapon;
            else if (name.Contains("미스트")) target = wm.MistWeapon;
            else if (name.Contains("낫")) target = wm.ScytheWeapon;

            if (target != null)
            {
                hasWeapon = target.Level > 0;
                level = target.Level;
                isEvolved = target.IsEvolved;
            }
        }
    }
}
