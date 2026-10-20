using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DopamineCleaner.Characters;
using DopamineCleaner.Core;
using DopamineCleaner.Enemies;
using DopamineCleaner.Meta;

namespace DopamineCleaner.UI
{
    public class BreakroomDilemmaModalUI : MonoBehaviour
    {
        public static BreakroomDilemmaModalUI Instance { get; private set; }

        private GameObject modalPanel;
        private Transform cardsContainer;
        private Text titleText;
        private Text guideText;
        private bool isOpen = false;

        public bool IsOpen => isOpen;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            BuildModalUI();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void Start()
        {
            if (modalPanel != null) modalPanel.SetActive(false);
        }

        private void BuildModalUI()
        {
            modalPanel = new GameObject("BreakroomDilemmaModal");
            modalPanel.transform.SetParent(transform, false);

            RectTransform mRt = modalPanel.AddComponent<RectTransform>();
            mRt.anchorMin = Vector2.zero;
            mRt.anchorMax = Vector2.one;
            mRt.sizeDelta = Vector2.zero;

            // 반투명 딤 배경
            Image bg = modalPanel.AddComponent<Image>();
            bg.color = new Color(0.06f, 0.05f, 0.10f, 0.92f);

            // 중앙 카드 박스
            GameObject boxObj = new GameObject("DilemmaBox");
            boxObj.transform.SetParent(modalPanel.transform, false);
            RectTransform bRt = boxObj.AddComponent<RectTransform>();
            bRt.anchorMin = new Vector2(0.5f, 0.5f);
            bRt.anchorMax = new Vector2(0.5f, 0.5f);
            bRt.sizeDelta = new Vector2(1360, 680);

            Image bImg = boxObj.AddComponent<Image>();
            bImg.color = new Color(0.16f, 0.14f, 0.22f);

            // 상단 타이틀
            GameObject tObj = new GameObject("Title");
            tObj.transform.SetParent(boxObj.transform, false);
            RectTransform tRt = tObj.AddComponent<RectTransform>();
            tRt.anchorMin = new Vector2(0.5f, 1f);
            tRt.anchorMax = new Vector2(0.5f, 1f);
            tRt.sizeDelta = new Vector2(1200, 50);
            tRt.anchoredPosition = new Vector2(0, -45);
            titleText = tObj.AddComponent<Text>();
            titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            titleText.text = "☕ [비밀 탕비실 & 옥상 휴게실] 야간 딜레마 계약";
            titleText.fontSize = 28;
            titleText.fontStyle = FontStyle.Bold;
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.color = new Color(1f, 0.85f, 0.35f);

            // 서브 가이드
            GameObject gObj = new GameObject("Guide");
            gObj.transform.SetParent(boxObj.transform, false);
            RectTransform gRt = gObj.AddComponent<RectTransform>();
            gRt.anchorMin = new Vector2(0.5f, 1f);
            gRt.anchorMax = new Vector2(0.5f, 1f);
            gRt.sizeDelta = new Vector2(1200, 35);
            gRt.anchoredPosition = new Vector2(0, -90);
            guideText = gObj.AddComponent<Text>();
            guideText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            guideText.text = "남몰래 휴식을 취하며 즉각적인 보상과 위험한 대가 중 하나를 선택하십시오.";
            guideText.fontSize = 18;
            guideText.alignment = TextAnchor.MiddleCenter;
            guideText.color = new Color(0.8f, 0.85f, 0.95f);

            // 카드 컨테이너 (3열 가로 배치)
            GameObject cardsObj = new GameObject("CardsContainer");
            cardsObj.transform.SetParent(boxObj.transform, false);
            RectTransform cRt = cardsObj.AddComponent<RectTransform>();
            cRt.anchorMin = new Vector2(0.5f, 0.5f);
            cRt.anchorMax = new Vector2(0.5f, 0.5f);
            cRt.sizeDelta = new Vector2(1300, 480);
            cRt.anchoredPosition = new Vector2(0, -35);

            HorizontalLayoutGroup hlg = cardsObj.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 25;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;

            cardsContainer = cardsObj.transform;
        }

        public void OpenDilemmaModal()
        {
            if (isOpen) return;
            isOpen = true;

            Time.timeScale = 0f;
            SoundManager.Instance?.Play(SoundEffect.ButtonClick, 0.8f);

            if (cardsContainer == null)
            {
                BuildModalUI();
            }

            if (modalPanel != null) modalPanel.SetActive(true);

            // 기존 카드 정리
            if (cardsContainer != null)
            {
                for (int i = cardsContainer.childCount - 1; i >= 0; i--)
                {
                    var child = cardsContainer.GetChild(i).gameObject;
                    if (Application.isPlaying) Destroy(child);
                    else DestroyImmediate(child);
                }
            }

            var dilemmas = BreakroomDilemmaDatabase.GetRandomDilemmas(3);
            foreach (var opt in dilemmas)
            {
                CreateDilemmaCard(opt);
            }
        }

        private void CreateDilemmaCard(BreakroomDilemmaOption opt)
        {
            GameObject card = new GameObject($"Card_{opt.type}");
            card.transform.SetParent(cardsContainer, false);

            Image cImg = card.AddComponent<Image>();
            cImg.color = new Color(0.22f, 0.19f, 0.30f);

            // 아이콘
            CreateCardText(card.transform, opt.iconEmoji, 55, FontStyle.Normal, new Color(1f, 0.9f, 0.5f),
                new Vector2(0, 160), new Vector2(360, 65));

            // 제목
            CreateCardText(card.transform, opt.title, 20, FontStyle.Bold, Color.white,
                new Vector2(0, 105), new Vector2(370, 45));

            // 혜택 설명 (초록)
            CreateCardText(card.transform, $"✨ [혜택]\n{opt.benefitDesc}", 16, FontStyle.Normal, new Color(0.45f, 1f, 0.65f),
                new Vector2(0, 30), new Vector2(360, 80));

            // 페널티 설명 (주황/빨강)
            CreateCardText(card.transform, $"⚠️ [대가 및 리스크]\n{opt.penaltyDesc}", 16, FontStyle.Normal, new Color(1f, 0.5f, 0.45f),
                new Vector2(0, -60), new Vector2(360, 80));

            // 선택 버튼
            GameObject btnObj = new GameObject("SelectButton");
            btnObj.transform.SetParent(card.transform, false);
            RectTransform bRt = btnObj.AddComponent<RectTransform>();
            bRt.anchorMin = new Vector2(0.5f, 0.5f);
            bRt.anchorMax = new Vector2(0.5f, 0.5f);
            bRt.sizeDelta = new Vector2(330, 52);
            bRt.anchoredPosition = new Vector2(0, -170);

            Image btnImg = btnObj.AddComponent<Image>();
            btnImg.color = new Color(0.85f, 0.38f, 0.42f);
            Button btn = btnObj.AddComponent<Button>();

            Text btnTxt = CreateCardText(btnObj.transform, opt.buttonLabel, 18, FontStyle.Bold, Color.white,
                Vector2.zero, new Vector2(320, 50));

            btn.onClick.AddListener(() =>
            {
                ApplyDilemmaChoice(opt);
                CloseModal();
            });
        }

        private Text CreateCardText(Transform parent, string content, int fontSize, FontStyle style, Color color,
                                    Vector2 pos, Vector2 size)
        {
            GameObject tObj = new GameObject("Text");
            tObj.transform.SetParent(parent, false);
            RectTransform rt = tObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size;
            rt.anchoredPosition = pos;

            Text txt = tObj.AddComponent<Text>();
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.fontSize = fontSize;
            txt.fontStyle = style;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = color;
            txt.text = content;
            return txt;
        }

        public void ApplyDilemmaChoice(BreakroomDilemmaOption opt)
        {
            SoundManager.Instance?.Play(SoundEffect.Fever, 0.85f);

            switch (opt.type)
            {
                case BreakroomOptionType.TripleEspresso:
                    StartCoroutine(TripleEspressoRoutine());
                    DamageTextManager.Instance?.SpawnDamageText(0, PlayerController.Instance.transform.position + Vector3.up * 1f, true);
                    break;

                case BreakroomOptionType.RooftopPowerNap:
                    if (PlayerController.Instance != null)
                    {
                        PlayerController.Instance.Heal(9999f);
                    }
                    if (EnemySpawner.Instance != null)
                    {
                        EnemySpawner.Instance.SpawnSpecialWave(6);
                    }
                    break;

                case BreakroomOptionType.CorporateCardSwipe:
                    GameManager.Instance?.AddBonusGold(350);
                    if (PlayerController.Instance != null)
                    {
                        PlayerController.Instance.CooldownRateMultiplier *= 1.25f; // 쿨타임 -20%
                        PlayerController.Instance.TakeDamage(20f); // 최대 체력 감소 느낌의 즉시 타격
                    }
                    break;

                case BreakroomOptionType.AnonymousBlindLeak:
                    // 화면 내 일반 몬스터 제거 및 젬 수거
                    var enemies = FindObjectsByType<Enemy>();
                    foreach (var e in enemies)
                    {
                        if (e != null && e.gameObject.activeInHierarchy && e.Type != EnemyType.DopamineBoss)
                        {
                            e.TakeDamage(999f);
                        }
                    }
                    Items.ItemManager.Instance?.TriggerMagnet();

                    // 엘리트 2마리 스폰
                    if (EnemySpawner.Instance != null)
                    {
                        EnemySpawner.Instance.SpawnSpecialWave(2);
                    }
                    break;
            }
        }

        private IEnumerator TripleEspressoRoutine()
        {
            if (PlayerController.Instance != null)
            {
                PlayerController.Instance.CooldownRateMultiplier *= 1.40f;
            }

            yield return new WaitForSeconds(60f);

            if (PlayerController.Instance != null)
            {
                PlayerController.Instance.CooldownRateMultiplier /= 1.40f;
                PlayerController.Instance.CooldownRateMultiplier *= 0.80f; // 카페인 방전 (10초간 속도 감속)
            }

            yield return new WaitForSeconds(10f);

            if (PlayerController.Instance != null)
            {
                PlayerController.Instance.CooldownRateMultiplier /= 0.80f; // 정상 복구
            }
        }

        public void CloseModal()
        {
            isOpen = false;
            if (modalPanel != null) modalPanel.SetActive(false);
            Time.timeScale = 1f;
        }
    }
}
