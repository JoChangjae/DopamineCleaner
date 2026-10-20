using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DopamineCleaner.Town
{
    /// <summary>
    /// 접속 시 보이는 본부(TownScene) 화면을 멋진 레트로 사이버펑크 도트 그래픽으로 수놓는 대기 연출 시스템.
    /// 절차적 픽셀 아트 야경 스카이라인, 신스웨이브 네온 그리드, 깜빡이는 안테나 비콘 및 부유 픽셀 입자를 생성합니다.
    /// </summary>
    public class TownPixelAtmosphere : MonoBehaviour
    {
        private RawImage skylineImage;
        private RawImage gridImage;
        private Texture2D skylineTex;
        private Texture2D gridTex;

        private float timer = 0f;
        private readonly List<PixelMote> motes = new();

        private class PixelMote
        {
            public RectTransform rect;
            public Image image;
            public Vector2 speed;
            public float baseAlpha;
            public float pulseSpeed;
        }

        private void Start()
        {
            SetupPixelBackdrop();
            SpawnFloatingPixelMotes(24);
        }

        private void SetupPixelBackdrop()
        {
            // Canvas 찾기
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null) canvas = FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            // 1. 도트 스카이라인 배경 생성
            GameObject skylineObj = new GameObject("PixelCitySkyline");
            skylineObj.transform.SetParent(canvas.transform, false);
            skylineObj.transform.SetSiblingIndex(1); // 배경 바로 위, UI 패널들 뒤

            RectTransform skyRt = skylineObj.AddComponent<RectTransform>();
            skyRt.anchorMin = new Vector2(0f, 0.25f);
            skyRt.anchorMax = new Vector2(1f, 1f);
            skyRt.offsetMin = Vector2.zero;
            skyRt.offsetMax = Vector2.zero;

            skylineImage = skylineObj.AddComponent<RawImage>();
            skylineTex = GenerateCitySkylineTexture(320, 160);
            skylineImage.texture = skylineTex;
            skylineImage.color = new Color(0.9f, 0.9f, 1f, 0.85f);

            // 2. 바닥 사이버 네온 그리드
            GameObject gridObj = new GameObject("PixelCyberGrid");
            gridObj.transform.SetParent(canvas.transform, false);
            gridObj.transform.SetSiblingIndex(2);

            RectTransform gridRt = gridObj.AddComponent<RectTransform>();
            gridRt.anchorMin = new Vector2(0f, 0f);
            gridRt.anchorMax = new Vector2(1f, 0.35f);
            gridRt.offsetMin = Vector2.zero;
            gridRt.offsetMax = Vector2.zero;

            gridImage = gridObj.AddComponent<RawImage>();
            gridTex = GenerateCyberGridTexture(256, 128);
            gridImage.texture = gridTex;
            gridImage.color = new Color(0.35f, 0.85f, 1f, 0.55f);

            // 3. 지평선 네온 발광 바 (Horizon Glow Line)
            GameObject horizonObj = new GameObject("HorizonGlow");
            horizonObj.transform.SetParent(canvas.transform, false);
            horizonObj.transform.SetSiblingIndex(3);

            RectTransform hzRt = horizonObj.AddComponent<RectTransform>();
            hzRt.anchorMin = new Vector2(0f, 0.35f);
            hzRt.anchorMax = new Vector2(1f, 0.35f);
            hzRt.sizeDelta = new Vector2(0, 4);
            hzRt.anchoredPosition = Vector2.zero;

            Image hzImg = horizonObj.AddComponent<Image>();
            hzImg.color = new Color(0.9f, 0.25f, 0.85f, 0.8f); // 핫 핑크 네온
        }

        private Texture2D GenerateCitySkylineTexture(int w, int h)
        {
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point; // 선명한 레트로 도트 그래픽 유지
            tex.wrapMode = TextureWrapMode.Clamp;

            Color skyTop = new Color(0.06f, 0.04f, 0.12f, 1f); // 깊은 밤하늘 보라
            Color skyBot = new Color(0.18f, 0.08f, 0.24f, 1f); // 네온 안개 스모그
            Color bldgSilhouetteFar = new Color(0.10f, 0.08f, 0.18f, 0.95f);
            Color bldgSilhouetteNear = new Color(0.07f, 0.05f, 0.13f, 1f);

            Color[] windowPalette = new Color[]
            {
                new Color(0.3f, 0.85f, 1f, 0.95f),  // 사이언 네온 창문
                new Color(1f, 0.85f, 0.3f, 0.95f),  // 앰버 옐로우 창문
                new Color(1f, 0.3f, 0.7f, 0.95f),   // 마젠타 네온
                new Color(0.4f, 0.95f, 0.6f, 0.95f), // 민트 그린
                new Color(0.15f, 0.12f, 0.25f, 0.5f) // 꺼진 창문
            };

            // 하늘 그라데이션
            for (int y = 0; y < h; y++)
            {
                float t = (float)y / h;
                Color col = Color.Lerp(skyBot, skyTop, t);
                for (int x = 0; x < w; x++)
                {
                    // 성운 및 밤하늘 미세 별빛 도트
                    if (y > h * 0.45f && ((x * 37 + y * 53) % 89 == 0))
                    {
                        col = new Color(0.9f, 0.85f, 1f, 0.8f);
                    }
                    else
                    {
                        col = Color.Lerp(skyBot, skyTop, t);
                    }
                    tex.SetPixel(x, y, col);
                }
            }

            // 원경 빌딩군 (Far Buildings)
            System.Random rand = new System.Random(1337);
            int curX = 0;
            while (curX < w)
            {
                int bWidth = rand.Next(12, 28);
                int bHeight = rand.Next(h / 3, (int)(h * 0.75f));
                for (int bx = curX; bx < Mathf.Min(w, curX + bWidth); bx++)
                {
                    for (int by = 0; by < bHeight; by++)
                    {
                        tex.SetPixel(bx, by, bldgSilhouetteFar);
                    }
                }
                curX += bWidth + rand.Next(2, 6);
            }

            // 근경 빌딩군 (Near Buildings with Windows & Antennas)
            rand = new System.Random(4242);
            curX = 0;
            while (curX < w)
            {
                int bWidth = rand.Next(16, 36);
                int bHeight = rand.Next(h / 4, (int)(h * 0.65f));

                // 옥상 안테나 도트
                int antennaX = curX + bWidth / 2;
                int antennaH = rand.Next(6, 18);
                for (int ay = bHeight; ay < Mathf.Min(h, bHeight + antennaH); ay++)
                {
                    tex.SetPixel(antennaX, ay, new Color(0.4f, 0.4f, 0.55f, 1f));
                }
                // 안테나 꼭대기 붉은 비콘 라이트
                if (bHeight + antennaH < h)
                {
                    tex.SetPixel(antennaX, bHeight + antennaH, new Color(1f, 0.2f, 0.3f, 1f));
                }

                for (int bx = curX; bx < Mathf.Min(w, curX + bWidth); bx++)
                {
                    for (int by = 0; by < bHeight; by++)
                    {
                        // 창문 도트 패턴 배치
                        bool isEdge = (bx == curX || bx == curX + bWidth - 1 || by == bHeight - 1);
                        if (!isEdge && (bx % 4 == 1 || bx % 4 == 2) && (by % 5 == 1 || by % 5 == 2))
                        {
                            int winColorIdx = rand.Next(0, windowPalette.Length);
                            tex.SetPixel(bx, by, windowPalette[winColorIdx]);
                        }
                        else
                        {
                            tex.SetPixel(bx, by, bldgSilhouetteNear);
                        }
                    }
                }

                curX += bWidth + rand.Next(1, 4);
            }

            tex.Apply();
            return tex;
        }

        private Texture2D GenerateCyberGridTexture(int w, int h)
        {
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            tex.wrapMode = TextureWrapMode.Repeat;

            Color baseBg = new Color(0.04f, 0.03f, 0.08f, 0.85f);
            Color gridLine = new Color(0.2f, 0.85f, 1f, 0.9f);
            Color horizonGlow = new Color(0.9f, 0.25f, 0.85f, 0.5f);

            for (int y = 0; y < h; y++)
            {
                float tY = (float)y / h;
                // 원근감 반영: 위로 갈수록 선 간격이 좁아짐
                bool isHorizontalLine = (y % Mathf.Max(3, Mathf.RoundToInt(Mathf.Pow(1.18f, y * 0.35f)))) == 0;

                for (int x = 0; x < w; x++)
                {
                    // 수직 원근 소실선 (가운데로 모여드는 레이저 라인)
                    float centerX = w * 0.5f;
                    float relX = (x - centerX) / (w * 0.5f);
                    float spreadX = relX * (1f - (tY * 0.7f));
                    int colIdx = Mathf.RoundToInt((spreadX + 1f) * 12f);
                    bool isVerticalLine = (colIdx % 2 == 0 && (x % 6 == 0));

                    if (isHorizontalLine || isVerticalLine)
                    {
                        Color lineCol = Color.Lerp(gridLine, horizonGlow, tY);
                        tex.SetPixel(x, y, lineCol);
                    }
                    else
                    {
                        tex.SetPixel(x, y, Color.Lerp(baseBg, horizonGlow * 0.3f, tY * 0.5f));
                    }
                }
            }

            tex.Apply();
            return tex;
        }

        private void SpawnFloatingPixelMotes(int count)
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null) canvas = FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            GameObject container = new GameObject("PixelMotesContainer");
            container.transform.SetParent(canvas.transform, false);
            container.transform.SetSiblingIndex(4); // 그리드 위

            Color[] moteColors = new Color[]
            {
                new Color(0.3f, 0.95f, 1f),  // 네온 사이언
                new Color(1f, 0.4f, 0.85f),  // 네온 핑크
                new Color(1f, 0.85f, 0.3f),  // 골드 앰버
                new Color(0.5f, 1f, 0.7f)    // 민트
            };

            for (int i = 0; i < count; i++)
            {
                GameObject moteObj = new GameObject($"Mote_{i}");
                moteObj.transform.SetParent(container.transform, false);

                RectTransform rt = moteObj.AddComponent<RectTransform>();
                int size = Random.Range(3, 7);
                rt.sizeDelta = new Vector2(size, size);
                rt.anchorMin = new Vector2(Random.value, Random.value * 0.7f);
                rt.anchorMax = rt.anchorMin;
                rt.anchoredPosition = Vector2.zero;

                Image img = moteObj.AddComponent<Image>();
                Color col = moteColors[Random.Range(0, moteColors.Length)];
                float alpha = Random.Range(0.4f, 0.9f);
                col.a = alpha;
                img.color = col;

                motes.Add(new PixelMote
                {
                    rect = rt,
                    image = img,
                    speed = new Vector2(Random.Range(-8f, 8f), Random.Range(14f, 32f)),
                    baseAlpha = alpha,
                    pulseSpeed = Random.Range(2f, 5f)
                });
            }
        }

        private void Update()
        {
            timer += Time.deltaTime;

            // 1. 그리드 바닥의 완만한 수평 스크롤
            if (gridImage != null)
            {
                gridImage.uvRect = new Rect((timer * 0.04f) % 1f, 0f, 1f, 1f);
            }

            // 2. 부유 픽셀 입자(Motes) 애니메이션
            foreach (var m in motes)
            {
                if (m.rect == null) continue;

                Vector2 pos = m.rect.anchoredPosition;
                pos += m.speed * Time.deltaTime;

                // 화면 상단 도달 시 다시 하단으로 리셋
                if (pos.y > 600f)
                {
                    pos.y = -400f;
                    pos.x = Random.Range(-900f, 900f);
                }
                m.rect.anchoredPosition = pos;

                // 알파 펄스 (깜빡임 효과)
                if (m.image != null)
                {
                    Color c = m.image.color;
                    c.a = m.baseAlpha * (0.6f + 0.4f * Mathf.Sin(timer * m.pulseSpeed));
                    m.image.color = c;
                }
            }
        }
    }
}
