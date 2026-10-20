using System;
using UnityEngine;
using DopamineCleaner.Characters;
using DopamineCleaner.UI;

namespace DopamineCleaner.Core
{
    public class BreakroomManager : MonoBehaviour
    {
        public static BreakroomManager Instance { get; private set; }

        [Header("비밀 탕비실 스폰 설정")]
        [SerializeField] private float spawnTime = 140f; // 2분 20초 시점 스폰
        [SerializeField] private float interactionRadius = 2.4f;
        private bool hasSpawned = false;
        private bool hasInteracted = false;
        private GameObject breakroomStandObj;
        private TextMesh worldLabelText;

        public bool HasSpawned => hasSpawned;
        public bool HasInteracted => hasInteracted;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void Update()
        {
            if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing)
                return;

            float time = GameManager.Instance.SurvivalTime;

            if (!hasSpawned && time >= spawnTime)
            {
                SpawnBreakroomStand();
            }

            if (hasSpawned && !hasInteracted && breakroomStandObj != null && PlayerController.Instance != null)
            {
                float dist = Vector3.Distance(PlayerController.Instance.transform.position, breakroomStandObj.transform.position);
                if (dist <= interactionRadius)
                {
                    TriggerInteraction();
                }
            }
        }

        public void SpawnBreakroomStand()
        {
            if (hasSpawned) return;
            hasSpawned = true;

            Vector3 spawnPos = Vector3.zero;
            if (PlayerController.Instance != null)
            {
                spawnPos = PlayerController.Instance.transform.position + new Vector3(3.5f, 2.0f, 0f);
            }
            else
            {
                spawnPos = new Vector3(4f, 2f, 0f);
            }

            breakroomStandObj = new GameObject("BreakroomStand");
            breakroomStandObj.transform.position = spawnPos;

            // 스프라이트 렌더러
            SpriteRenderer sr = breakroomStandObj.AddComponent<SpriteRenderer>();
            Texture2D tex = CreateStandTexture();
            sr.sprite = Sprite.Create(tex, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 16);
            sr.sortingOrder = 12;

            // 부드러운 아우라
            CircleCollider2D col = breakroomStandObj.AddComponent<CircleCollider2D>();
            col.radius = interactionRadius;
            col.isTrigger = true;

            // 월드 텍스트 라벨
            GameObject labelObj = new GameObject("Label");
            labelObj.transform.SetParent(breakroomStandObj.transform, false);
            labelObj.transform.localPosition = new Vector3(0f, 1.4f, 0f);

            worldLabelText = labelObj.AddComponent<TextMesh>();
            worldLabelText.text = "☕ 비밀 탕비실 & 옥상 휴게실";
            worldLabelText.fontSize = 24;
            worldLabelText.characterSize = 0.12f;
            worldLabelText.anchor = TextAnchor.MiddleCenter;
            worldLabelText.alignment = TextAlignment.Center;
            worldLabelText.color = new Color(1f, 0.9f, 0.4f);
            worldLabelText.fontStyle = FontStyle.Bold;

            var mr = labelObj.GetComponent<MeshRenderer>();
            if (mr != null) mr.sortingOrder = 40;

            SoundManager.Instance?.Play(SoundEffect.GemCollect, 0.7f);
        }

        public void TriggerInteraction()
        {
            if (hasInteracted) return;
            hasInteracted = true;

            if (breakroomStandObj != null)
            {
                breakroomStandObj.SetActive(false);
            }

            if (BreakroomDilemmaModalUI.Instance != null)
            {
                BreakroomDilemmaModalUI.Instance.OpenDilemmaModal();
            }
        }

        private Texture2D CreateStandTexture()
        {
            Texture2D tex = new Texture2D(32, 32);
            tex.filterMode = FilterMode.Point;
            Color coffeeColor = new Color(0.45f, 0.28f, 0.18f);
            Color cupWhite = new Color(0.95f, 0.95f, 0.95f);
            Color goldAccent = new Color(1f, 0.85f, 0.3f);

            for (int y = 0; y < 32; y++)
            {
                for (int x = 0; x < 32; x++)
                {
                    if (x >= 8 && x <= 24 && y >= 6 && y <= 26)
                    {
                        if (y >= 24) tex.SetPixel(x, y, goldAccent); // 상단 황금 뚜껑
                        else if (x == 8 || x == 24 || y == 6) tex.SetPixel(x, y, coffeeColor); // 테두리
                        else tex.SetPixel(x, y, cupWhite); // 몸체
                    }
                    else
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                }
            }
            tex.Apply();
            return tex;
        }
    }
}
