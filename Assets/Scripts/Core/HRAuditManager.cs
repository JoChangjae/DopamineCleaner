using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DopamineCleaner.Characters;
using DopamineCleaner.Enemies;

namespace DopamineCleaner.Core
{
    public class HRAuditManager : MonoBehaviour
    {
        public static HRAuditManager Instance { get; private set; }

        [Header("사내 특별 감사 발동 시점")]
        [SerializeField] private float triggerTime = 195f; // 3분 15초 시점
        private bool hasTriggered = false;
        private bool isAuditorDefeated = false;
        private Enemy activeAuditor = null;

        [Header("감사 경보 UI")]
        [SerializeField] private GameObject auditBanner;
        [SerializeField] private Text auditText;

        public bool HasTriggered => hasTriggered;
        public bool IsAuditorDefeated => isAuditorDefeated;
        public Enemy ActiveAuditor => activeAuditor;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void Start()
        {
            if (auditBanner == null)
            {
                CreateAuditBannerUI();
            }
            if (auditBanner != null) auditBanner.SetActive(false);
        }

        private void Update()
        {
            if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing)
                return;

            float time = GameManager.Instance.SurvivalTime;
            if (!hasTriggered && time >= triggerTime)
            {
                TriggerAuditBoss();
            }
        }

        public void TriggerAuditBoss()
        {
            if (hasTriggered) return;
            hasTriggered = true;

            SoundManager.Instance?.Play(SoundEffect.Warning, 0.9f);
            ShowBanner("🚨 [긴급 경보] 본사 특별 감사팀 불시 점검 시작!\n💼 [본사 감사팀장] 김감사 부장이 들이닥쳤습니다!", new Color(1f, 0.2f, 0.25f), 4.5f);

            // 김감사 부장 스폰
            SpawnAuditorEnemy();
        }

        private void SpawnAuditorEnemy()
        {
            Vector3 spawnPos = Vector3.zero;
            if (PlayerController.Instance != null)
            {
                float angle = UnityEngine.Random.Range(0f, 360f) * Mathf.Deg2Rad;
                spawnPos = PlayerController.Instance.transform.position + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * 11f;
            }
            else
            {
                spawnPos = new Vector3(8f, 6f, 0f);
            }

            var spawner = EnemySpawner.Instance;
            if (spawner != null)
            {
                activeAuditor = spawner.GetComponentInChildren<Enemy>(true);
            }

            if (activeAuditor == null)
            {
                GameObject enemyObj = new GameObject("AuditorKim_Boss");
                activeAuditor = enemyObj.AddComponent<Enemy>();
            }

            activeAuditor.transform.position = spawnPos;
            int stage = GameManager.Instance != null ? GameManager.Instance.CurrentStage : 1;
            float time = GameManager.Instance != null ? GameManager.Instance.SurvivalTime : 180f;

            activeAuditor.Initialize(EnemyType.AuditorKim, stage, time);
        }

        public void OnAuditorDefeated()
        {
            isAuditorDefeated = true;
            SoundManager.Instance?.Play(SoundEffect.LevelUp, 0.95f);
            ShowBanner("🎉 [사내 특별 감사 종결] 전 항목 무혐의 통과!\n황금 퇴직금 상자 (+500G / 체력 완충 / 공격력 +30%)를 획득하세요!", new Color(1f, 0.85f, 0.3f), 5.0f);
        }

        private void CreateAuditBannerUI()
        {
            var canvas = FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            auditBanner = new GameObject("AuditBanner");
            auditBanner.transform.SetParent(canvas.transform, false);

            RectTransform rt = auditBanner.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.80f);
            rt.anchorMax = new Vector2(0.5f, 0.80f);
            rt.sizeDelta = new Vector2(920, 95);
            rt.anchoredPosition = Vector2.zero;

            Image bg = auditBanner.AddComponent<Image>();
            bg.color = new Color(0.12f, 0.08f, 0.16f, 0.92f);

            Outline ol = auditBanner.AddComponent<Outline>();
            ol.effectColor = new Color(1f, 0.25f, 0.35f, 0.8f);
            ol.effectDistance = new Vector2(2f, -2f);

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(auditBanner.transform, false);
            RectTransform trt = textObj.AddComponent<RectTransform>();
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.sizeDelta = Vector2.zero;

            auditText = textObj.AddComponent<Text>();
            auditText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            auditText.fontSize = 20;
            auditText.fontStyle = FontStyle.Bold;
            auditText.alignment = TextAnchor.MiddleCenter;
            auditText.color = Color.white;
        }

        private void ShowBanner(string message, Color textColor, float duration)
        {
            if (auditBanner == null) CreateAuditBannerUI();
            if (auditBanner == null || auditText == null) return;

            auditText.text = message;
            auditText.color = textColor;
            auditBanner.SetActive(true);

            StopAllCoroutines();
            StartCoroutine(HideBannerAfterDelay(duration));
        }

        private IEnumerator HideBannerAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (auditBanner != null) auditBanner.SetActive(false);
        }
    }
}
