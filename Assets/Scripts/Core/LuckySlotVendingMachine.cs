using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DopamineCleaner.Characters;
using DopamineCleaner.Enemies;
using DopamineCleaner.Items;
using DopamineCleaner.Meta;

namespace DopamineCleaner.Core
{
    public class LuckySlotVendingMachine : MonoBehaviour
    {
        public static LuckySlotVendingMachine Instance { get; private set; }

        [Header("자판기 상태")]
        [SerializeField] private float interactionRadius = 3.2f;
        [SerializeField] private int spinCost = 50;
        private bool isPlayerNearby = false;
        public bool IsModalOpen { get; private set; } = false;

        [Header("UI 연결")]
        [SerializeField] private GameObject promptUI;
        [SerializeField] private GameObject slotModalUI;
        [SerializeField] private Text reel1Text;
        [SerializeField] private Text reel2Text;
        [SerializeField] private Text reel3Text;
        [SerializeField] private Text resultMessageText;
        [SerializeField] private Button spinButton;
        [SerializeField] private Button closeButton;

        // 계약 버튼
        [SerializeField] private Button contract1Btn;
        [SerializeField] private Button contract2Btn;
        [SerializeField] private Text contract1Txt;
        [SerializeField] private Text contract2Txt;

        private TextMesh worldLabelText;
        private bool isSpinning = false;
        private readonly string[] symbols = { "7️⃣", "💎", "🍒", "⭐", "💀" };

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            SetupVisualSprite();
        }

        private void Start()
        {
            EnsureUIReferencesAndListeners();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public void SetupVisualSprite()
        {
            var sr = GetComponent<SpriteRenderer>();
            if (sr == null) sr = gameObject.AddComponent<SpriteRenderer>();
            sr.sprite = CreateVendingSprite();
            sr.sortingOrder = 5;
            transform.localScale = Vector3.one * 1.35f;

            var col = GetComponent<CircleCollider2D>();
            if (col == null) col = gameObject.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 1.2f;

            // 이미 라벨이 있다면 참조만 갱신
            Transform existingLabel = transform.Find("VendingWorldLabel");
            if (existingLabel != null)
            {
                worldLabelText = existingLabel.GetComponent<TextMesh>();
                return;
            }

            // 자판기 머리 위 월드 텍스트 라벨 (위치 인지 및 E키 안내)
            GameObject labelObj = new GameObject("VendingWorldLabel");
            labelObj.transform.SetParent(transform, false);
            labelObj.transform.localPosition = new Vector3(0, 1.4f, 0);

            worldLabelText = labelObj.AddComponent<TextMesh>();
            worldLabelText.text = "🎰 럭키 자판기\n[E] 상호작용";
            worldLabelText.fontSize = 28;
            worldLabelText.characterSize = 0.08f;
            worldLabelText.alignment = TextAlignment.Center;
            worldLabelText.anchor = TextAnchor.LowerCenter;
            worldLabelText.color = new Color(1f, 0.9f, 0.3f);
        }

        /// <summary>
        /// 씬 직렬화가 누락되었거나 런타임 진입 시 버튼 리스너 및 UI 참조를 안전하게 자동 바인딩합니다.
        /// </summary>
        public void EnsureUIReferencesAndListeners()
        {
            SetupVisualSprite();

            if (promptUI == null)
            {
                var p = GameObject.Find("SlotPrompt");
                if (p != null) promptUI = p;
            }

            if (slotModalUI == null)
            {
                var m = GameObject.Find("SlotModalPanel");
                if (m != null)
                {
                    slotModalUI = m;

                    if (reel1Text == null) reel1Text = m.transform.Find("ReelsGroup/Reel1")?.GetComponent<Text>();
                    if (reel2Text == null) reel2Text = m.transform.Find("ReelsGroup/Reel2")?.GetComponent<Text>();
                    if (reel3Text == null) reel3Text = m.transform.Find("ReelsGroup/Reel3")?.GetComponent<Text>();
                    if (resultMessageText == null) resultMessageText = m.transform.Find("ResText")?.GetComponent<Text>();

                    if (spinButton == null) spinButton = m.transform.Find("SpinBtn")?.GetComponent<Button>();
                    if (closeButton == null) closeButton = m.transform.Find("CloseBtn")?.GetComponent<Button>();

                    if (contract1Btn == null) contract1Btn = m.transform.Find("Contract1Btn")?.GetComponent<Button>();
                    if (contract2Btn == null) contract2Btn = m.transform.Find("Contract2Btn")?.GetComponent<Button>();
                    if (contract1Txt == null) contract1Txt = m.transform.Find("Contract1Btn/C1Txt")?.GetComponent<Text>();
                    if (contract2Txt == null) contract2Txt = m.transform.Find("Contract2Btn/C2Txt")?.GetComponent<Text>();
                }
            }

            // 버튼 클릭 이벤트 확실하게 등록
            if (spinButton != null)
            {
                spinButton.onClick.RemoveAllListeners();
                spinButton.onClick.AddListener(OnSpinClicked);
            }

            if (closeButton != null)
            {
                closeButton.onClick.RemoveAllListeners();
                closeButton.onClick.AddListener(CloseSlotModal);
            }

            if (contract1Btn != null)
            {
                contract1Btn.onClick.RemoveAllListeners();
                contract1Btn.onClick.AddListener(() => ApplyContract(1));
            }

            if (contract2Btn != null)
            {
                contract2Btn.onClick.RemoveAllListeners();
                contract2Btn.onClick.AddListener(() => ApplyContract(2));
            }

            if (promptUI != null && !IsModalOpen) promptUI.SetActive(false);
            if (slotModalUI != null && !IsModalOpen) slotModalUI.SetActive(false);
        }

        private void Update()
        {
            // 모달이 열려 있을 때: E키 또는 ESC키로 닫기 지원
            if (IsModalOpen)
            {
                if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Escape))
                {
                    CloseSlotModal();
                }
                return;
            }

            if (PlayerController.Instance == null) return;

            float dist = Vector2.Distance(transform.position, PlayerController.Instance.transform.position);
            bool nearby = dist <= interactionRadius;

            if (nearby != isPlayerNearby)
            {
                isPlayerNearby = nearby;
                if (promptUI != null)
                {
                    promptUI.SetActive(isPlayerNearby && !IsModalOpen);
                }

                if (worldLabelText != null)
                {
                    worldLabelText.color = isPlayerNearby ? new Color(1f, 0.35f, 0.75f) : new Color(1f, 0.9f, 0.3f);
                }
            }

            // 자판기 근처에서 E키 입력 시 모달 열기
            if (isPlayerNearby && Input.GetKeyDown(KeyCode.E))
            {
                OpenSlotModal();
            }
        }

        public void BindUI(GameObject prompt, GameObject modal, Text r1, Text r2, Text r3, Text result, Button spin, Button close,
                           Button c1Btn = null, Button c2Btn = null, Text c1Txt = null, Text c2Txt = null)
        {
            promptUI = prompt;
            slotModalUI = modal;
            reel1Text = r1;
            reel2Text = r2;
            reel3Text = r3;
            resultMessageText = result;
            spinButton = spin;
            closeButton = close;
            contract1Btn = c1Btn;
            contract2Btn = c2Btn;
            contract1Txt = c1Txt;
            contract2Txt = c2Txt;

            EnsureUIReferencesAndListeners();
        }

        public void OpenSlotModal()
        {
            EnsureUIReferencesAndListeners();

            if (slotModalUI == null) return;

            IsModalOpen = true;
            slotModalUI.SetActive(true);
            if (promptUI != null) promptUI.SetActive(false);

            if (resultMessageText != null)
                resultMessageText.text = $"💰 50 골드로 레버를 당겨 도파민 잭팟을 노리세요!";

            RefreshContracts();
            SoundManager.Instance?.Play(SoundEffect.GemCollect, 0.6f);

            // 슬롯 이용 중 몬스터 공격 방지 및 안전한 플레이를 위해 일시정지
            Time.timeScale = 0f;
        }

        public void CloseSlotModal()
        {
            if (isSpinning) return; // 릴 회전 중에는 닫기 방지

            IsModalOpen = false;
            if (slotModalUI != null) slotModalUI.SetActive(false);
            if (promptUI != null && isPlayerNearby) promptUI.SetActive(true);

            // 게임 플레이 상태인 경우 시간 정상 재개
            if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Playing)
            {
                Time.timeScale = 1f;
            }

            SoundManager.Instance?.Play(SoundEffect.Dash, 0.5f);
        }

        private void OnSpinClicked()
        {
            if (isSpinning) return;

            int currentGold = GameManager.Instance != null ? GameManager.Instance.CurrentGold : 999;
            if (currentGold < spinCost)
            {
                if (resultMessageText != null)
                {
                    resultMessageText.text = "⚠️ 골드가 부족합니다! (필요: 50G)";
                    resultMessageText.color = new Color(1f, 0.3f, 0.3f);
                }
                SoundManager.Instance?.Play(SoundEffect.Hit, 0.6f);
                return;
            }

            // 골드 차감
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddBonusGold(-spinCost);
            }

            StartCoroutine(SpinRoutine());
        }

        private IEnumerator SpinRoutine()
        {
            isSpinning = true;
            if (spinButton != null) spinButton.interactable = false;

            // 12회 빠르고 경쾌하게 회전 (총 약 0.96초)
            int totalTicks = 12;
            for (int t = 0; t < totalTicks; t++)
            {
                if (reel1Text != null) reel1Text.text = symbols[Random.Range(0, symbols.Length)];
                if (reel2Text != null) reel2Text.text = symbols[Random.Range(0, symbols.Length)];
                if (reel3Text != null) reel3Text.text = symbols[Random.Range(0, symbols.Length)];

                SoundManager.Instance?.Play(SoundEffect.Shoot, 0.25f);
                yield return new WaitForSecondsRealtime(0.08f);
            }

            // 결과 추첨
            int roll = Random.Range(0, 100);
            string s1, s2, s3;

            if (roll < 12) // 12% 잭팟 7-7-7
            {
                s1 = s2 = s3 = "7️⃣";
            }
            else if (roll < 26) // 14% 다이아몬드
            {
                s1 = s2 = s3 = "💎";
            }
            else if (roll < 45) // 19% 체리
            {
                s1 = s2 = s3 = "🍒";
            }
            else if (roll < 58) // 13% 해골 꽝
            {
                s1 = s2 = s3 = "💀";
            }
            else // 2개 일치 또는 불일치
            {
                s1 = symbols[Random.Range(0, symbols.Length)];
                s2 = s1;
                s3 = symbols[Random.Range(0, symbols.Length)];
            }

            if (reel1Text != null) reel1Text.text = s1;
            if (reel2Text != null) reel2Text.text = s2;
            if (reel3Text != null) reel3Text.text = s3;

            EvaluateOutcome(s1, s2, s3);

            isSpinning = false;
            if (spinButton != null) spinButton.interactable = true;
        }

        private void EvaluateOutcome(string s1, string s2, string s3)
        {
            Vector3 dropPos = transform.position + new Vector3(0f, -0.9f, 0f);

            if (s1 == "7️⃣" && s2 == "7️⃣" && s3 == "7️⃣")
            {
                // 대박 잭팟!
                if (resultMessageText != null)
                {
                    resultMessageText.text = "🎉 [초대박 잭팟!] 황금 상자 2개 & 메가 보석 배출!";
                    resultMessageText.color = new Color(1f, 0.85f, 0.2f);
                }

                ItemManager.Instance?.SpawnGem(dropPos + new Vector3(-0.6f, -0.2f, 0f), GemType.Chest, 0);
                ItemManager.Instance?.SpawnGem(dropPos + new Vector3(0.6f, -0.2f, 0f), GemType.Chest, 0);
                ItemManager.Instance?.SpawnGem(dropPos + new Vector3(0f, -0.4f, 0f), GemType.MegaPink, 35);
                GameManager.Instance?.AddBonusGold(150);
                FeverManager.Instance?.ActivateFever();
                AchievementManager.Instance?.ReportJackpot();
                SoundManager.Instance?.Play(SoundEffect.Fever, 0.9f);
                PixelParticleManager.Instance?.SpawnDeathPop(dropPos, new Color(1f, 0.85f, 0.2f), 18);
            }
            else if (s1 == "💎" && s2 == "💎" && s3 == "💎")
            {
                if (resultMessageText != null)
                {
                    resultMessageText.text = "💎 [다이아몬드 대박!] 대량 보석 배출 & 자석 흡수!";
                    resultMessageText.color = new Color(0.3f, 0.9f, 1f);
                }

                ItemManager.Instance?.SpawnGem(dropPos + new Vector3(-0.5f, -0.2f, 0f), GemType.MegaPink, 30);
                ItemManager.Instance?.SpawnGem(dropPos + new Vector3(0.5f, -0.2f, 0f), GemType.MegaPink, 30);
                ItemManager.Instance?.SpawnGem(dropPos, GemType.Blue, 10);
                ItemManager.Instance?.TriggerMagnet();
                SoundManager.Instance?.Play(SoundEffect.LevelUp, 0.8f);
                PixelParticleManager.Instance?.SpawnDeathPop(dropPos, new Color(0.3f, 0.9f, 1f), 16);
            }
            else if (s1 == "🍒" && s2 == "🍒" && s3 == "🍒")
            {
                if (resultMessageText != null)
                {
                    resultMessageText.text = "🍒 [달콤한 체리!] 멘탈 케이크 & 회복 보석 배출!";
                    resultMessageText.color = new Color(1f, 0.3f, 0.6f);
                }

                PlayerController.Instance?.Heal(999f);
                ItemManager.Instance?.SpawnGem(dropPos, GemType.Cake, 0);
                ItemManager.Instance?.SpawnGem(dropPos + new Vector3(0.5f, -0.2f, 0f), GemType.Blue, 10);
                SoundManager.Instance?.Play(SoundEffect.LevelUp, 0.8f);
                PixelParticleManager.Instance?.SpawnDeathPop(dropPos, new Color(1f, 0.3f, 0.6f), 16);
            }
            else if (s1 == "💀" && s2 == "💀" && s3 == "💀")
            {
                if (resultMessageText != null)
                {
                    resultMessageText.text = "💀 [도파민 과부하 꽝!] 스팸 슈터 몬스터가 출현합니다!";
                    resultMessageText.color = new Color(0.9f, 0.2f, 0.2f);
                }

                EnemySpawner.Instance?.SpawnSpecialWave(5);
                SoundManager.Instance?.Play(SoundEffect.Hit, 0.8f);
            }
            else if (s1 == s2 || s2 == s3 || s1 == s3)
            {
                if (resultMessageText != null)
                {
                    resultMessageText.text = "✨ [2개 일치!] 배출구로 보석 2개 & 30G 배출!";
                    resultMessageText.color = Color.white;
                }

                GameManager.Instance?.AddBonusGold(30);
                ItemManager.Instance?.SpawnGem(dropPos + new Vector3(-0.4f, -0.2f, 0f), GemType.Blue, 6);
                ItemManager.Instance?.SpawnGem(dropPos + new Vector3(0.4f, -0.2f, 0f), GemType.Green, 3);
                SoundManager.Instance?.Play(SoundEffect.GemCollect, 0.6f);
            }
            else
            {
                if (resultMessageText != null)
                {
                    resultMessageText.text = "💨 [위로상] 배출구로 위로 보석 1개가 떨어졌습니다!";
                    resultMessageText.color = new Color(0.85f, 0.85f, 0.85f);
                }

                ItemManager.Instance?.SpawnGem(dropPos, GemType.Green, 3);
                SoundManager.Instance?.Play(SoundEffect.GemCollect, 0.4f);
            }
        }

        private void RefreshContracts()
        {
            if (contract1Txt != null) contract1Txt.text = "🩸 [피의 계약]\n받는 피해 +30%\n모든 공격력 +45%";
            if (contract2Txt != null) contract2Txt.text = "⚡ [마하 계약]\n최대 체력 -25\n이동/공격속도 +35%";
        }

        private void ApplyContract(int index)
        {
            var wm = Weapons.WeaponManager.Instance ?? FindAnyObjectByType<Weapons.WeaponManager>();

            if (index == 1)
            {
                // 피의 계약
                wm?.ApplyUpgrade(Weapons.UpgradeType.DamageBoost);
                wm?.ApplyUpgrade(Weapons.UpgradeType.DamageBoost);
                if (resultMessageText != null)
                    resultMessageText.text = "🩸 피의 계약 체결 완료! 공격력이 24% 폭증했습니다.";
            }
            else
            {
                // 마하 계약
                wm?.ApplyUpgrade(Weapons.UpgradeType.MoveSpeed);
                wm?.ApplyUpgrade(Weapons.UpgradeType.AttackSpeed);
                if (resultMessageText != null)
                    resultMessageText.text = "⚡ 마하 계약 체결 완료! 기동력과 공격주기가 대폭 향상되었습니다.";
            }

            if (contract1Btn != null) contract1Btn.interactable = false;
            if (contract2Btn != null) contract2Btn.interactable = false;
            SoundManager.Instance?.Play(SoundEffect.LevelUp, 0.7f);
        }

        private static Sprite CreateVendingSprite()
        {
            int w = 32, h = 48;
            Texture2D tex = new Texture2D(w, h);
            tex.filterMode = FilterMode.Point;
            Color body = new Color(0.12f, 0.16f, 0.28f);
            Color screen = new Color(0.2f, 0.85f, 1f);
            Color neon = new Color(1f, 0.3f, 0.7f);

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    if (x < 2 || x >= w - 2 || y < 2 || y >= h - 2)
                        tex.SetPixel(x, y, neon);
                    else if (y >= 20 && y <= 38 && x >= 4 && x <= w - 5)
                        tex.SetPixel(x, y, screen);
                    else
                        tex.SetPixel(x, y, body);
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 32);
        }
    }
}
