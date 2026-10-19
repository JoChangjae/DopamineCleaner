using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DopamineCleaner.Characters;
using DopamineCleaner.Core;
using DopamineCleaner.Enemies;
using DopamineCleaner.Items;

namespace DopamineCleaner.UI
{
    public class MinimapRadar : MonoBehaviour
    {
        public static MinimapRadar Instance { get; private set; }

        [Header("미니맵 레이더 설정")]
        [SerializeField] private RectTransform radarRect;
        [SerializeField] private float radarWorldRadius = 45f;
        [SerializeField] private RectTransform playerDot;
        public float RadarWorldRadius => radarWorldRadius;

        [Header("동적 포인터 인디케이터 (나침반)")]
        [SerializeField] private RectTransform pointerContainer;
        [SerializeField] private GameObject pointerPrefab;

        private readonly List<RectTransform> activePointers = new();
        private Camera mainCam;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            mainCam = Camera.main;
        }

        private void LateUpdate()
        {
            if (PlayerController.Instance == null) return;
            if (mainCam == null) mainCam = Camera.main;

            UpdatePointers();
        }

        private void UpdatePointers()
        {
            if (pointerContainer == null || mainCam == null) return;

            Vector3 playerPos = PlayerController.Instance.transform.position;

            // 추적 대상 수집 (1. 멘탈 쉼터, 2. 보스/엘리트, 3. 황금 상자)
            List<(Vector3 worldPos, string icon, Color color, string label)> targets = new();

            // 1. 멘탈 쉼터
            if (MentalSanctuaryManager.Instance != null && MentalSanctuaryManager.Instance.ActiveSanctuary != null)
            {
                targets.Add((MentalSanctuaryManager.Instance.ActiveSanctuary.transform.position, "⛲", new Color(0.2f, 1f, 0.9f), "멘탈 쉼터"));
            }

            // 2. 보스 몬스터
            var enemies = FindObjectsByType<Enemy>();
            foreach (var e in enemies)
            {
                if (e != null && e.gameObject.activeInHierarchy && e.Type == EnemyType.DopamineBoss)
                {
                    targets.Add((e.transform.position, "👑", new Color(1f, 0.25f, 0.25f), "도파민 보스"));
                }
            }

            // 3. 황금 보물상자
            var gems = FindObjectsByType<ExpGem>();
            foreach (var g in gems)
            {
                if (g != null && g.gameObject.activeInHierarchy && g.Type == GemType.Chest)
                {
                    targets.Add((g.transform.position, "🎁", new Color(1f, 0.85f, 0.2f), "황금 상자"));
                }
            }

            // 필요한 포인터 수 동적 조절
            while (activePointers.Count < targets.Count)
            {
                GameObject pObj = CreatePointerObject();
                activePointers.Add(pObj.GetComponent<RectTransform>());
            }

            for (int i = 0; i < activePointers.Count; i++)
            {
                if (i < targets.Count)
                {
                    var (worldPos, icon, color, label) = targets[i];
                    UpdateSinglePointer(activePointers[i], playerPos, worldPos, icon, color, label);
                }
                else
                {
                    activePointers[i].gameObject.SetActive(false);
                }
            }
        }

        private void UpdateSinglePointer(RectTransform pointer, Vector3 playerPos, Vector3 targetPos, string icon, Color color, string label)
        {
            float dist = Vector2.Distance(playerPos, targetPos);
            Vector3 viewportPos = mainCam.WorldToViewportPoint(targetPos);

            // 화면 내에 있으면 나침반 화살표 숨김
            if (viewportPos.x >= 0.08f && viewportPos.x <= 0.92f &&
                viewportPos.y >= 0.08f && viewportPos.y <= 0.92f)
            {
                pointer.gameObject.SetActive(false);
                return;
            }

            pointer.gameObject.SetActive(true);

            // 화면 가장자리 클램핑
            Vector2 screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
            Vector3 screenTarget = mainCam.WorldToScreenPoint(targetPos);
            Vector2 dir = ((Vector2)screenTarget - screenCenter).normalized;

            float margin = 50f;
            float clampedX = Mathf.Clamp(screenCenter.x + dir.x * (Screen.width * 0.45f), margin, Screen.width - margin);
            float clampedY = Mathf.Clamp(screenCenter.y + dir.y * (Screen.height * 0.45f), margin, Screen.height - margin);

            pointer.position = new Vector3(clampedX, clampedY, 0);

            // 텍스트 업데이트
            var text = pointer.GetComponentInChildren<Text>();
            if (text != null)
            {
                text.text = $"{icon} {dist:F0}m";
                text.color = color;
            }
        }

        private GameObject CreatePointerObject()
        {
            GameObject obj = new GameObject("Pointer");
            obj.transform.SetParent(pointerContainer, false);

            var rt = obj.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(100, 36);

            var img = obj.AddComponent<Image>();
            img.color = new Color(0.1f, 0.12f, 0.18f, 0.85f);

            GameObject textObj = new GameObject("Label");
            textObj.transform.SetParent(obj.transform, false);
            var text = textObj.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 14;
            text.fontStyle = FontStyle.Bold;
            text.alignment = TextAnchor.MiddleCenter;
            text.rectTransform.anchorMin = Vector2.zero;
            text.rectTransform.anchorMax = Vector2.one;
            text.rectTransform.sizeDelta = Vector2.zero;

            return obj;
        }
    }
}
