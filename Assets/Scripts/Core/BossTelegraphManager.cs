using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DopamineCleaner.Core
{
    public class BossTelegraphManager : MonoBehaviour
    {
        private static BossTelegraphManager instance;
        public static BossTelegraphManager Instance
        {
            get
            {
                if (instance == null) instance = FindAnyObjectByType<BossTelegraphManager>();
                return instance;
            }
            private set => instance = value;
        }

        [Header("보스 경고 UI 배너")]
        private GameObject warningBannerObj;
        private Text warningBannerText;
        private CanvasGroup bannerCanvasGroup;

        [Header("텔레그래프 풀")]
        private readonly List<GameObject> lineTelegraphs = new();
        private readonly List<GameObject> circleTelegraphs = new();
        private Sprite telegraphWhiteSprite;

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

            CreateTelegraphSprite();
            CreateWarningBanner();
        }

        private void Start()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnBossSpawned += ShowBossWarningBanner;
            }
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnBossSpawned -= ShowBossWarningBanner;
            }
        }

        private void CreateTelegraphSprite()
        {
            Texture2D tex = new Texture2D(8, 8, TextureFormat.RGBA32, false);
            Color[] colors = new Color[64];
            for (int i = 0; i < 64; i++) colors[i] = Color.white;
            tex.SetPixels(colors);
            tex.filterMode = FilterMode.Point;
            tex.Apply();

            telegraphWhiteSprite = Sprite.Create(tex, new Rect(0, 0, 8, 8), new Vector2(0.5f, 0.5f), 16f);
        }

        private void CreateWarningBanner()
        {
            Canvas canvas = FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            warningBannerObj = new GameObject("BossWarningBanner");
            warningBannerObj.transform.SetParent(canvas.transform, false);

            RectTransform rt = warningBannerObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 0.72f);
            rt.anchorMax = new Vector2(1f, 0.82f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            bannerCanvasGroup = warningBannerObj.AddComponent<CanvasGroup>();
            bannerCanvasGroup.alpha = 0f;

            Image bg = warningBannerObj.AddComponent<Image>();
            bg.color = new Color(0.12f, 0.05f, 0.1f, 0.85f);

            GameObject txtObj = new GameObject("WarningText");
            txtObj.transform.SetParent(warningBannerObj.transform, false);
            RectTransform trt = txtObj.AddComponent<RectTransform>();
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.offsetMin = Vector2.zero;
            trt.offsetMax = Vector2.zero;

            warningBannerText = txtObj.AddComponent<Text>();
            warningBannerText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            warningBannerText.fontSize = 32;
            warningBannerText.fontStyle = FontStyle.Bold;
            warningBannerText.alignment = TextAnchor.MiddleCenter;
            warningBannerText.color = new Color(1f, 0.25f, 0.35f);
            warningBannerText.text = "⚠️ [EMERGENCY WARNING] 도파민 거대 보스 출현! 비상 경보 발령! ⚠️";

            var outline = txtObj.AddComponent<Outline>();
            outline.effectColor = new Color(0.5f, 0f, 0.1f, 0.9f);
            outline.effectDistance = new Vector2(2f, -2f);

            warningBannerObj.SetActive(false);
        }

        public void ShowBossWarningBanner()
        {
            if (warningBannerObj != null && gameObject.activeInHierarchy)
            {
                StopAllCoroutines();
                StartCoroutine(WarningBannerRoutine());
            }

            SoundManager.Instance?.Play(SoundEffect.Fever, 1.0f);
        }

        private IEnumerator WarningBannerRoutine()
        {
            warningBannerObj.SetActive(true);

            // 3회 사이렌 점멸
            for (int cycle = 0; cycle < 3; cycle++)
            {
                // 페이드 인
                float t = 0f;
                while (t < 0.25f)
                {
                    t += Time.unscaledDeltaTime;
                    bannerCanvasGroup.alpha = Mathf.Lerp(0f, 1f, t / 0.25f);
                    yield return null;
                }

                yield return new WaitForSecondsRealtime(0.4f);

                // 페이드 아웃
                t = 0f;
                while (t < 0.2f)
                {
                    t += Time.unscaledDeltaTime;
                    bannerCanvasGroup.alpha = Mathf.Lerp(1f, 0.2f, t / 0.2f);
                    yield return null;
                }
            }

            bannerCanvasGroup.alpha = 0f;
            warningBannerObj.SetActive(false);
        }

        /// <summary>
        /// 돌진 궤적 텔레그래프 장판 표시 (startPos에서 endPos로 뻗어나가는 붉은 경고 직사각형)
        /// </summary>
        public GameObject SpawnLineTelegraph(Vector3 startPos, Vector3 targetPos, float width, float duration)
        {
            GameObject obj = GetLineTelegraph();
            obj.transform.position = (startPos + targetPos) * 0.5f;

            Vector3 dir = (targetPos - startPos);
            float length = dir.magnitude;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            obj.transform.rotation = Quaternion.Euler(0, 0, angle);
            obj.transform.localScale = new Vector3(length, width, 1f);

            obj.SetActive(true);
            StartCoroutine(FadeOutTelegraph(obj, duration));
            return obj;
        }

        /// <summary>
        /// 원형 충격파 텔레그래프 장판 표시 (centerPos 중심으로 붉은색 경고 원)
        /// </summary>
        public GameObject SpawnCircleTelegraph(Vector3 centerPos, float radius, float duration)
        {
            GameObject obj = GetCircleTelegraph();
            obj.transform.position = centerPos;
            obj.transform.rotation = Quaternion.identity;
            obj.transform.localScale = Vector3.one * (radius * 2f);

            obj.SetActive(true);
            StartCoroutine(FadeOutTelegraph(obj, duration));
            return obj;
        }

        private IEnumerator FadeOutTelegraph(GameObject obj, float duration)
        {
            SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
            float elapsed = 0f;
            Color c = sr.color;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / duration;
                // 시간에 따라 붉은색이 점점 진해지며 깜빡임
                float pulse = Mathf.PingPong(elapsed * 10f, 0.25f);
                c.a = Mathf.Lerp(0.25f, 0.75f, progress) + pulse;
                sr.color = c;
                yield return null;
            }

            obj.SetActive(false);
        }

        private GameObject GetLineTelegraph()
        {
            for (int i = 0; i < lineTelegraphs.Count; i++)
            {
                if (!lineTelegraphs[i].activeSelf)
                    return lineTelegraphs[i];
            }

            GameObject obj = new GameObject($"LineTelegraph_{lineTelegraphs.Count}");
            obj.transform.SetParent(transform);
            SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = telegraphWhiteSprite;
            sr.color = new Color(1f, 0.15f, 0.2f, 0.4f);
            sr.sortingOrder = 5; // 배경 바로 위, 몬스터 아래
            obj.SetActive(false);
            lineTelegraphs.Add(obj);
            return obj;
        }

        private GameObject GetCircleTelegraph()
        {
            for (int i = 0; i < circleTelegraphs.Count; i++)
            {
                if (!circleTelegraphs[i].activeSelf)
                    return circleTelegraphs[i];
            }

            GameObject obj = new GameObject($"CircleTelegraph_{circleTelegraphs.Count}");
            obj.transform.SetParent(transform);
            SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = telegraphWhiteSprite;
            sr.color = new Color(1f, 0.2f, 0.3f, 0.35f);
            sr.sortingOrder = 5;
            obj.SetActive(false);
            circleTelegraphs.Add(obj);
            return obj;
        }
    }
}
