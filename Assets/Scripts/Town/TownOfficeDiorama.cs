using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DopamineCleaner.Meta;

namespace DopamineCleaner.Town
{
    /// <summary>
    /// 사옥 리모델링 상태에 따라 본부의 6대 복지 가구를 실제 픽셀 도트 아트로 실시간 시각화하는 디오라마 컴포넌트.
    /// 미배치(Lv.0) 시에는 청사진 실루엣으로, 업그레이드 시 생생한 네온 도트 그래픽과 부유 연출, MAX 단계 시 황금 후광을 표시합니다.
    /// </summary>
    public class TownOfficeDiorama : MonoBehaviour
    {
        [Header("가구 슬롯 컨테이너")]
        [SerializeField] private Transform slotContainer;

        private static Font koreanFont;
        private readonly List<DioramaSlotView> activeSlots = new();
        private static readonly Dictionary<string, Texture2D> cachedTextures = new();
        private float animTimer = 0f;

        private class DioramaSlotView
        {
            public string facilityId;
            public RectTransform rootRt;
            public RawImage iconImg;
            public Text nameText;
            public Text levelBadge;
            public Image glowBorder;
            public Vector2 basePos;
            public float phaseOffset;
        }

        private void Awake()
        {
            EnsureFont();
        }

        private static void EnsureFont()
        {
            if (koreanFont == null)
            {
#if UNITY_EDITOR
                koreanFont = UnityEditor.AssetDatabase.LoadAssetAtPath<Font>("Assets/Fonts/MalgunGothic.ttf");
#endif
                if (koreanFont == null)
                    koreanFont = Font.CreateDynamicFontFromOSFont(new string[] { "Malgun Gothic", "맑은 고딕", "Arial" }, 16);
            }
        }

        private void Start()
        {
            BuildDioramaSlots();
            RefreshDiorama();
        }

        private void Update()
        {
            if (activeSlots.Count == 0) return;
            animTimer += Time.deltaTime * 2.2f;

            for (int i = 0; i < activeSlots.Count; i++)
            {
                var slot = activeSlots[i];
                if (slot == null || slot.rootRt == null) continue;

                int lv = GameDataManager.Instance != null ? GameDataManager.Instance.GetFacilityLevel(slot.facilityId) : 0;
                if (lv > 0)
                {
                    // 활성 가구 미세 부유 애니메이션
                    float offsetY = Mathf.Sin(animTimer + slot.phaseOffset) * 3.5f;
                    slot.iconImg.rectTransform.anchoredPosition = new Vector2(0, offsetY);

                    // MAX 레벨 시 황금 네온 펄스
                    if (lv >= 5 && slot.glowBorder != null)
                    {
                        float alpha = 0.55f + Mathf.PingPong(animTimer * 1.5f, 0.45f);
                        slot.glowBorder.color = new Color(1f, 0.85f, 0.2f, alpha);
                    }
                }
                else
                {
                    slot.iconImg.rectTransform.anchoredPosition = Vector2.zero;
                    if (slot.glowBorder != null)
                    {
                        slot.glowBorder.color = new Color(0.2f, 0.3f, 0.4f, 0.25f);
                    }
                }
            }
        }

        public void BuildDioramaSlots()
        {
            EnsureFont();
            Transform container = slotContainer != null ? slotContainer : transform;

            // 기존 슬롯 청소
            for (int i = container.childCount - 1; i >= 0; i--)
            {
                Destroy(container.GetChild(i).gameObject);
            }
            activeSlots.Clear();

            var allFac = OfficeFacilityDatabase.GetAll();
            float slotWidth = 230f;
            float totalWidth = allFac.Count * slotWidth;
            float startX = -totalWidth * 0.5f + slotWidth * 0.5f;

            for (int i = 0; i < allFac.Count; i++)
            {
                var fac = allFac[i];
                GameObject slotObj = new GameObject($"Slot_{fac.id}");
                slotObj.transform.SetParent(container, false);

                RectTransform sRt = slotObj.AddComponent<RectTransform>();
                sRt.sizeDelta = new Vector2(215f, 115f);
                Vector2 pos = new Vector2(startX + i * slotWidth, 0);
                sRt.anchoredPosition = pos;

                // 배경 카드
                Image bg = slotObj.AddComponent<Image>();
                bg.color = new Color(0.12f, 0.13f, 0.18f, 0.95f);

                // 테두리 글로우
                GameObject borderObj = new GameObject("GlowBorder");
                borderObj.transform.SetParent(slotObj.transform, false);
                RectTransform bRt = borderObj.AddComponent<RectTransform>();
                bRt.anchorMin = Vector2.zero;
                bRt.anchorMax = Vector2.one;
                bRt.sizeDelta = new Vector2(4, 4);
                Image borderImg = borderObj.AddComponent<Image>();
                borderImg.color = new Color(0.3f, 0.4f, 0.55f, 0.4f);
                borderObj.transform.SetAsFirstSibling();

                // 가구 도트 아이콘 (RawImage)
                GameObject iconObj = new GameObject("PixelIcon");
                iconObj.transform.SetParent(slotObj.transform, false);
                RectTransform iRt = iconObj.AddComponent<RectTransform>();
                iRt.sizeDelta = new Vector2(54f, 54f);
                iRt.anchoredPosition = new Vector2(0, 10f);

                RawImage rImg = iconObj.AddComponent<RawImage>();
                rImg.texture = GetOrCreateFurnitureTexture(fac.id);

                // 이름 텍스트
                GameObject nameObj = new GameObject("NameText");
                nameObj.transform.SetParent(slotObj.transform, false);
                RectTransform nRt = nameObj.AddComponent<RectTransform>();
                nRt.sizeDelta = new Vector2(200f, 22f);
                nRt.anchoredPosition = new Vector2(0, -28f);
                Text nTxt = nameObj.AddComponent<Text>();
                nTxt.font = koreanFont;
                nTxt.fontSize = 13;
                nTxt.alignment = TextAnchor.MiddleCenter;
                nTxt.color = Color.white;
                nTxt.text = fac.name;

                // 레벨 배지 텍스트
                GameObject badgeObj = new GameObject("LevelBadge");
                badgeObj.transform.SetParent(slotObj.transform, false);
                RectTransform lRt = badgeObj.AddComponent<RectTransform>();
                lRt.sizeDelta = new Vector2(180f, 18f);
                lRt.anchoredPosition = new Vector2(0, -46f);
                Text lTxt = badgeObj.AddComponent<Text>();
                lTxt.font = koreanFont;
                lTxt.fontSize = 12;
                lTxt.alignment = TextAnchor.MiddleCenter;
                lTxt.fontStyle = FontStyle.Bold;

                var slotView = new DioramaSlotView
                {
                    facilityId = fac.id,
                    rootRt = sRt,
                    iconImg = rImg,
                    nameText = nTxt,
                    levelBadge = lTxt,
                    glowBorder = borderImg,
                    basePos = pos,
                    phaseOffset = i * 0.85f
                };
                activeSlots.Add(slotView);
            }
        }

        public void RefreshDiorama()
        {
            EnsureFont();
            if (activeSlots.Count == 0)
            {
                BuildDioramaSlots();
            }

            foreach (var slot in activeSlots)
            {
                if (slot == null) continue;
                int lv = GameDataManager.Instance != null ? GameDataManager.Instance.GetFacilityLevel(slot.facilityId) : 0;
                var fac = OfficeFacilityDatabase.Get(slot.facilityId);

                if (lv <= 0)
                {
                    // 미설치 청사진 상태
                    slot.iconImg.color = new Color(0.25f, 0.45f, 0.75f, 0.35f);
                    slot.nameText.color = new Color(0.6f, 0.65f, 0.75f, 0.7f);
                    slot.levelBadge.text = "<color=#64748B>🔒 미배치</color>";
                    if (slot.glowBorder != null)
                        slot.glowBorder.color = new Color(0.2f, 0.25f, 0.35f, 0.3f);
                }
                else if (lv >= 5)
                {
                    // MAX 명품 가구
                    slot.iconImg.color = Color.white;
                    slot.nameText.color = new Color(1f, 0.95f, 0.6f);
                    slot.levelBadge.text = "<color=#FCD34D>★ MAX 명품 복지</color>";
                    if (slot.glowBorder != null)
                        slot.glowBorder.color = new Color(1f, 0.85f, 0.2f, 0.85f);
                }
                else
                {
                    // Lv.1~4 가동 상태
                    slot.iconImg.color = Color.white;
                    slot.nameText.color = Color.white;
                    slot.levelBadge.text = $"<color=#6EE7B7>Lv.{lv}</color> <color=#94A3B8>({fac.category})</color>";
                    if (slot.glowBorder != null)
                        slot.glowBorder.color = new Color(0.25f, 0.65f, 0.85f, 0.5f);
                }
            }
        }

        // ==========================================
        // 절차적 픽셀 도트 텍스처 생성기 (32x32)
        // ==========================================
        private static Texture2D GetOrCreateFurnitureTexture(string id)
        {
            if (cachedTextures.TryGetValue(id, out var existing) && existing != null)
                return existing;

            Texture2D tex = GenerateFurnitureDotArt(id);
            cachedTextures[id] = tex;
            return tex;
        }

        private static Texture2D GenerateFurnitureDotArt(string id)
        {
            const int S = 32;
            Texture2D tex = new Texture2D(S, S, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            tex.wrapMode = TextureWrapMode.Clamp;

            Color empty = Color.clear;
            Color[] pixels = new Color[S * S];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = empty;

            Action<int, int, Color> setP = (x, y, c) =>
            {
                if (x >= 0 && x < S && y >= 0 && y < S)
                    pixels[y * S + x] = c;
            };

            Action<int, int, int, int, Color> fillR = (x, y, w, h, c) =>
            {
                for (int py = y; py < y + h; py++)
                {
                    for (int px = x; px < x + w; px++)
                    {
                        setP(px, py, c);
                    }
                }
            };

            switch (id)
            {
                case "fac_massage_chair":
                    // 최고급 무중력 안마 의자 (사이버 퍼플 & 인체공학 가죽)
                    fillR(8, 6, 16, 20, new Color(0.18f, 0.12f, 0.28f)); // 본체 등받이
                    fillR(10, 8, 12, 16, new Color(0.38f, 0.24f, 0.58f)); // 시트 쿠션
                    fillR(6, 4, 20, 6, new Color(0.12f, 0.10f, 0.18f));  // 베이스 다리
                    fillR(4, 10, 5, 12, new Color(0.24f, 0.18f, 0.36f)); // 좌 팔걸이
                    fillR(23, 10, 5, 12, new Color(0.24f, 0.18f, 0.36f)); // 우 팔걸이
                    fillR(11, 22, 10, 6, new Color(0.5f, 0.3f, 0.75f));  // 헤드레스트
                    // 네온 안마 롤러 펄스 라인
                    fillR(12, 12, 8, 2, new Color(0.4f, 0.9f, 1f));
                    fillR(12, 16, 8, 2, new Color(0.9f, 0.4f, 1f));
                    break;

                case "fac_espresso_bar":
                    // 네온 바리스타 에스프레소 바
                    fillR(4, 4, 24, 10, new Color(0.28f, 0.20f, 0.16f)); // 원목 카운터
                    fillR(7, 14, 18, 12, new Color(0.45f, 0.48f, 0.55f)); // 실버 머신
                    fillR(9, 16, 14, 8, new Color(0.15f, 0.18f, 0.24f));  // 머신 인터페이스
                    fillR(11, 12, 4, 3, new Color(0.85f, 0.65f, 0.3f));   // 추출 포터필터
                    fillR(17, 12, 4, 3, new Color(0.85f, 0.65f, 0.3f));
                    fillR(10, 6, 5, 5, new Color(0.95f, 0.95f, 0.95f));  // 커피 잔
                    fillR(11, 8, 3, 2, new Color(0.35f, 0.18f, 0.08f));  // 크레마 에스프레소
                    // 따뜻한 증기 픽셀
                    setP(12, 13, new Color(0.9f, 0.9f, 1f, 0.8f));
                    setP(13, 15, new Color(0.9f, 0.9f, 1f, 0.6f));
                    setP(11, 17, new Color(0.9f, 0.9f, 1f, 0.4f));
                    break;

                case "fac_arcade_machine":
                    // 레트로 네온 아케이드 오락기
                    fillR(8, 4, 16, 24, new Color(0.14f, 0.12f, 0.24f)); // 캐비닛
                    fillR(10, 14, 12, 10, new Color(0.08f, 0.25f, 0.4f)); // CRT 화면
                    // 8비트 인베이더 도트 on 화면
                    fillR(14, 18, 4, 3, new Color(0.3f, 0.95f, 0.5f));
                    setP(13, 17, new Color(0.3f, 0.95f, 0.5f));
                    setP(18, 17, new Color(0.3f, 0.95f, 0.5f));
                    // 조작부 & 조이스틱
                    fillR(9, 10, 14, 4, new Color(0.2f, 0.18f, 0.3f));
                    fillR(12, 11, 2, 3, new Color(0.95f, 0.2f, 0.3f));   // 빨간 레버
                    setP(17, 11, new Color(1f, 0.85f, 0.2f));             // 노랑 버튼
                    setP(19, 11, new Color(0.2f, 0.85f, 1f));             // 파랑 버튼
                    // 상단 네온 간판
                    fillR(9, 24, 14, 3, new Color(1f, 0.25f, 0.6f));
                    break;

                case "fac_standing_desk":
                    // 스마트 전동 모션 데스크
                    fillR(6, 4, 4, 14, new Color(0.3f, 0.32f, 0.38f));   // 좌 모터 다리
                    fillR(22, 4, 4, 14, new Color(0.3f, 0.32f, 0.38f));  // 우 모터 다리
                    fillR(4, 16, 24, 3, new Color(0.85f, 0.75f, 0.62f)); // 자작나무 상판
                    // 듀얼 와이드 커브드 모니터
                    fillR(8, 19, 16, 9, new Color(0.1f, 0.15f, 0.25f));
                    fillR(9, 20, 14, 7, new Color(0.12f, 0.4f, 0.65f));
                    // 코드 라인
                    fillR(10, 24, 7, 1, new Color(0.4f, 0.9f, 0.5f));
                    fillR(10, 22, 10, 1, new Color(0.9f, 0.6f, 0.3f));
                    // 키보드 & 마우스
                    fillR(11, 17, 7, 2, new Color(0.2f, 0.2f, 0.25f));
                    setP(20, 17, new Color(0.9f, 0.3f, 0.3f));
                    break;

                case "fac_sleep_capsule":
                    // 고압 산소 회복 수면 캡슐 (바이오 사이언스)
                    fillR(6, 6, 20, 18, new Color(0.15f, 0.2f, 0.32f)); // 외골격
                    fillR(8, 8, 16, 14, new Color(0.1f, 0.35f, 0.55f)); // 내부 챔버
                    fillR(9, 10, 14, 10, new Color(0.25f, 0.75f, 0.9f, 0.75f)); // 사이언 블루 글래스
                    fillR(5, 4, 22, 4, new Color(0.1f, 0.12f, 0.18f));  // 받침대
                    // 바이탈 LED 게이지
                    setP(10, 20, Color.green);
                    setP(12, 20, Color.green);
                    setP(14, 20, Color.cyan);
                    setP(16, 20, Color.cyan);
                    // 산소 방울
                    setP(12, 13, new Color(1f, 1f, 1f, 0.8f));
                    setP(17, 15, new Color(1f, 1f, 1f, 0.8f));
                    break;

                case "fac_neon_aquarium":
                    // 홀로그램 네온 수족관
                    fillR(5, 5, 22, 20, new Color(0.1f, 0.25f, 0.45f)); // 수조 프레임
                    fillR(6, 6, 20, 18, new Color(0.08f, 0.18f, 0.35f)); // 심해 블루 수중
                    fillR(4, 4, 24, 3, new Color(0.15f, 0.15f, 0.2f));   // 스탠드 베이스
                    fillR(4, 23, 24, 3, new Color(0.15f, 0.15f, 0.2f));  // 상단 후드
                    // 해초 픽셀
                    fillR(8, 7, 2, 6, new Color(0.2f, 0.85f, 0.4f));
                    fillR(21, 7, 2, 8, new Color(0.15f, 0.75f, 0.35f));
                    // 네온 물고기들
                    fillR(11, 16, 4, 2, new Color(1f, 0.3f, 0.85f));    // 핑크 네온 피쉬
                    setP(15, 17, new Color(1f, 0.3f, 0.85f));
                    fillR(16, 11, 3, 2, new Color(0.2f, 0.95f, 1f));     // 사이언 테트라
                    setP(15, 11, new Color(0.2f, 0.95f, 1f));
                    // 기포
                    setP(12, 19, new Color(0.8f, 0.95f, 1f, 0.7f));
                    setP(19, 18, new Color(0.8f, 0.95f, 1f, 0.7f));
                    break;
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return tex;
        }
    }
}
