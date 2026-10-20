using System;
using System.IO;
using UnityEngine;
using DopamineCleaner.Characters;
using DopamineCleaner.Meta;
using DopamineCleaner.Weapons;

namespace DopamineCleaner.Core
{
    /// <summary>
    /// 디톡스 종합 검진표를 인스타/디스코드 공유용 고해상도(1280x720) 네온 픽셀 인증서 카드로
    /// 즉각 렌더링하여 바탕화면 및 캡처 폴더에 PNG로 저장해주는 시스템.
    /// </summary>
    public static class DetoxCardExporter
    {
        public static string ExportDetoxReportCard()
        {
            try
            {
                int width = 1280;
                int height = 720;
                Texture2D card = new Texture2D(width, height, TextureFormat.RGBA32, false);
                card.filterMode = FilterMode.Point;

                // 1. 배경 채우기 (사이버 다크 퍼플 + 레트로 그리드 패턴)
                Color bgDark = new Color(0.08f, 0.06f, 0.12f, 1f);
                Color gridCol = new Color(0.14f, 0.11f, 0.22f, 1f);
                Color neonCyan = new Color(0.35f, 0.85f, 1f, 1f);
                Color neonGold = new Color(1f, 0.85f, 0.3f, 1f);
                Color neonPink = new Color(1f, 0.35f, 0.75f, 1f);

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        bool isGrid = (x % 32 == 0) || (y % 32 == 0);
                        card.SetPixel(x, y, isGrid ? gridCol : bgDark);
                    }
                }

                // 2. 외곽 네온 프레임 & 코너 브라켓
                DrawRectFrame(card, 20, 20, width - 40, height - 40, neonCyan, 3);
                DrawRectFrame(card, 26, 26, width - 52, height - 52, new Color(neonCyan.r, neonCyan.g, neonCyan.b, 0.4f), 1);

                // 코너 강조 브라켓
                DrawCornerBracket(card, 14, 14, 30, neonGold);
                DrawCornerBracket(card, width - 44, 14, 30, neonGold);
                DrawCornerBracket(card, 14, height - 44, 30, neonGold);
                DrawCornerBracket(card, width - 44, height - 44, 30, neonGold);

                // 3. 상단 헤더 배너 바
                FillRect(card, 40, height - 110, width - 80, 60, new Color(0.16f, 0.12f, 0.26f, 0.95f));
                DrawRectFrame(card, 40, height - 110, width - 80, 60, neonPink, 2);

                // 4. 중앙 정보 영역 분할 박스들
                // 좌측: 요원 프로필 & 생존 지표 (너비 580)
                FillRect(card, 50, 160, 560, 420, new Color(0.12f, 0.10f, 0.18f, 0.95f));
                DrawRectFrame(card, 50, 160, 560, 420, neonCyan, 2);

                // 우측: 무기 진화 트리 및 DPS 통계 (너비 580)
                FillRect(card, 650, 160, 580, 420, new Color(0.12f, 0.10f, 0.18f, 0.95f));
                DrawRectFrame(card, 650, 160, 580, 420, neonGold, 2);

                // 5. 하단 공인 인증 워터마크 바
                FillRect(card, 50, 50, width - 100, 80, new Color(0.14f, 0.11f, 0.22f, 0.95f));
                DrawRectFrame(card, 50, 50, width - 100, 80, new Color(0.3f, 0.8f, 0.6f, 0.9f), 2);

                // 6. DPS 바 그래프 시각화 (우측 영역 내부)
                if (WeaponManager.Instance != null)
                {
                    var stats = WeaponManager.Instance.GetDamageStatistics();
                    int startY = 520;
                    for (int i = 0; i < Mathf.Min(5, stats.Count); i++)
                    {
                        var ws = stats[i];
                        int barW = Mathf.RoundToInt((ws.percentage / 100f) * 440f);
                        FillRect(card, 680, startY - (i * 65), Mathf.Max(20, barW), 22, Color.Lerp(neonCyan, neonGold, ws.percentage / 100f));
                        DrawRectFrame(card, 680, startY - (i * 65), 440, 22, new Color(0.4f, 0.4f, 0.6f, 0.6f), 1);
                    }
                }

                // 7. S등급 / A등급 네온 도장 엠블럼 (중앙 스탬프)
                DrawStampBadge(card, 480, 320, neonGold);

                card.Apply();

                // 8. PNG 인코딩 및 파일 저장
                byte[] pngBytes = card.EncodeToPNG();
                UnityEngine.Object.Destroy(card);

                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string fileName = $"DopamineCleaner_DetoxReport_{timestamp}.png";

                // 기본 저장: 바탕화면
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string fullPath = Path.Combine(desktopPath, fileName);

                try
                {
                    File.WriteAllBytes(fullPath, pngBytes);
                }
                catch
                {
                    // 바탕화면 접근 실패 시 프로젝트 내부 Screenshots 폴더에 저장
                    string fallbackDir = Path.Combine(Application.dataPath, "..", "Screenshots");
                    if (!Directory.Exists(fallbackDir)) Directory.CreateDirectory(fallbackDir);
                    fullPath = Path.Combine(fallbackDir, fileName);
                    File.WriteAllBytes(fullPath, pngBytes);
                }

                Debug.Log($"📸 [검진표 카드 캡처 완료] 저장 경로: {fullPath}");
                return fullPath;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[DetoxCardExporter] 캡처 실패: {ex.Message}");
                return null;
            }
        }

        private static void FillRect(Texture2D tex, int x, int y, int w, int h, Color col)
        {
            for (int cy = y; cy < y + h && cy < tex.height; cy++)
            {
                for (int cx = x; cx < x + w && cx < tex.width; cx++)
                {
                    tex.SetPixel(cx, cy, col);
                }
            }
        }

        private static void DrawRectFrame(Texture2D tex, int x, int y, int w, int h, Color col, int thickness)
        {
            for (int t = 0; t < thickness; t++)
            {
                for (int cx = x + t; cx < x + w - t; cx++)
                {
                    tex.SetPixel(cx, y + t, col);
                    tex.SetPixel(cx, y + h - 1 - t, col);
                }
                for (int cy = y + t; cy < y + h - t; cy++)
                {
                    tex.SetPixel(x + t, cy, col);
                    tex.SetPixel(x + w - 1 - t, cy, col);
                }
            }
        }

        private static void DrawCornerBracket(Texture2D tex, int x, int y, int len, Color col)
        {
            for (int i = 0; i < len; i++)
            {
                tex.SetPixel(x + i, y, col);
                tex.SetPixel(x, y + i, col);
            }
        }

        private static void DrawStampBadge(Texture2D tex, int cx, int cy, Color col)
        {
            int r = 55;
            for (int dy = -r; dy <= r; dy++)
            {
                for (int dx = -r; dx <= r; dx++)
                {
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    if (dist >= r - 3 && dist <= r)
                    {
                        tex.SetPixel(cx + dx, cy + dy, col);
                    }
                    else if (dist >= r - 10 && dist <= r - 7)
                    {
                        tex.SetPixel(cx + dx, cy + dy, col * 0.8f);
                    }
                }
            }
        }
    }
}
