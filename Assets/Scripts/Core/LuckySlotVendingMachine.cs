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
        [SerializeField] private float interactionRadius = 2.2f;
        [SerializeField] private int spinCost = 50;
        private bool isPlayerNearby = false;

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

        private bool isSpinning = false;
        private readonly string[] symbols = { "7️⃣", "💎", "🍒", "⭐", "💀" };

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            SetupVisualSprite();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void SetupVisualSprite()
        {
            var sr = GetComponent<SpriteRenderer>();
            if (sr == null) sr = gameObject.AddComponent<SpriteRenderer>();
            sr.sprite = CreateVendingSprite();
            transform.localScale = Vector3.one * 1.35f;

            var col = GetComponent<CircleCollider2D>();
            if (col == null) col = gameObject.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 1.0f;
        }

        private void Update()
        {
            if (PlayerController.Instance == null) return;

            float dist = Vector2.Distance(transform.position, PlayerController.Instance.transform.position);
            bool nearby = dist <= interactionRadius;

            if (nearby != isPlayerNearby)
            {
                isPlayerNearby = nearby;
                if (promptUI != null) promptUI.SetActive(isPlayerNearby && (slotModalUI == null || !slotModalUI.activeSelf));
            }

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

            if (spinButton != null) spinButton.onClick.AddListener(OnSpinClicked);
            if (closeButton != null) closeButton.onClick.AddListener(CloseSlotModal);

            if (contract1Btn != null) contract1Btn.onClick.AddListener(() => ApplyContract(1));
            if (contract2Btn != null) contract2Btn.onClick.AddListener(() => ApplyContract(2));

            if (promptUI != null) promptUI.SetActive(false);
            if (slotModalUI != null) slotModalUI.SetActive(false);
        }

        public void OpenSlotModal()
        {
            if (slotModalUI == null) return;
            slotModalUI.SetActive(true);
            if (promptUI != null) promptUI.SetActive(false);

            if (resultMessageText != null)
                resultMessageText.text = $"💰 50 골드로 레버를 당겨 도파민 잭팟을 노리세요!";

            RefreshContracts();
            SoundManager.Instance?.Play(SoundEffect.GemCollect, 0.6f);
        }

        public void CloseSlotModal()
        {
            if (slotModalUI != null) slotModalUI.SetActive(false);
            if (promptUI != null && isPlayerNearby) promptUI.SetActive(true);
        }

        private void OnSpinClicked()
        {
            if (isSpinning) return;

            int currentGold = GameManager.Instance != null ? GameManager.Instance.CurrentGold : 999;
            if (currentGold < spinCost)
            {
                if (resultMessageText != null)
                    resultMessageText.text = "⚠️ 골드가 부족합니다! (필요: 50G)";
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

            float spinDuration = 1.3f;
            float elapsed = 0f;

            while (elapsed < spinDuration)
            {
                elapsed += Time.deltaTime;
                if (reel1Text != null) reel1Text.text = symbols[Random.Range(0, symbols.Length)];
                if (reel2Text != null) reel2Text.text = symbols[Random.Range(0, symbols.Length)];
                if (reel3Text != null) reel3Text.text = symbols[Random.Range(0, symbols.Length)];

                SoundManager.Instance?.Play(SoundEffect.Shoot, 0.25f);
                yield return new WaitForSeconds(0.08f);
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
            if (s1 == "7️⃣" && s2 == "7️⃣" && s3 == "7️⃣")
            {
                // 대박 잭팟!
                if (resultMessageText != null)
                {
                    resultMessageText.text = "🎉 [초대박 잭팟!] 황금 상자 2개 + 150G + 피버 폭주!";
                    resultMessageText.color = new Color(1f, 0.85f, 0.2f);
                }

                ItemManager.Instance?.SpawnGem(transform.position + Vector3.up * 1.5f, GemType.Chest, 0);
                ItemManager.Instance?.SpawnGem(transform.position + Vector3.right * 1.5f, GemType.Chest, 0);
                GameManager.Instance?.AddBonusGold(150);
                FeverManager.Instance?.ActivateFever();
                AchievementManager.Instance?.ReportJackpot();
                SoundManager.Instance?.Play(SoundEffect.Fever, 0.9f);
            }
            else if (s1 == "💎" && s2 == "💎" && s3 == "💎")
            {
                if (resultMessageText != null)
                {
                    resultMessageText.text = "💎 [다이아몬드 대박!] 대량 경험치 + 자석 흡수!";
                    resultMessageText.color = new Color(0.3f, 0.9f, 1f);
                }

                PlayerController.Instance?.AddExp(250f);
                ItemManager.Instance?.TriggerMagnet();
                SoundManager.Instance?.Play(SoundEffect.LevelUp, 0.8f);
            }
            else if (s1 == "🍒" && s2 == "🍒" && s3 == "🍒")
            {
                if (resultMessageText != null)
                {
                    resultMessageText.text = "🍒 [달콤한 체리!] 체력 100% 완전 회복 + 멘탈 케이크!";
                    resultMessageText.color = new Color(1f, 0.3f, 0.6f);
                }

                PlayerController.Instance?.Heal(999f);
                ItemManager.Instance?.SpawnGem(transform.position + Vector3.up * 1.2f, GemType.Cake, 0);
                SoundManager.Instance?.Play(SoundEffect.LevelUp, 0.8f);
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
                    resultMessageText.text = "✨ [2개 일치!] 보너스 30G + 50 EXP 획득!";
                    resultMessageText.color = Color.white;
                }

                GameManager.Instance?.AddBonusGold(30);
                PlayerController.Instance?.AddExp(50f);
                SoundManager.Instance?.Play(SoundEffect.GemCollect, 0.5f);
            }
            else
            {
                if (resultMessageText != null)
                {
                    resultMessageText.text = "💨 [다음 기회에!] 위로금 15 EXP 획득.";
                    resultMessageText.color = new Color(0.8f, 0.8f, 0.8f);
                }

                PlayerController.Instance?.AddExp(15f);
            }
        }

        private void RefreshContracts()
        {
            if (contract1Txt != null) contract1Txt.text = "🩸 [피의 계약]\n받는 피해 +30%\n모든 공격력 +45%";
            if (contract2Txt != null) contract2Txt.text = "⚡ [마하 계약]\n최대 체력 -25\n이동/공격속도 +35%";
        }

        private void ApplyContract(int index)
        {
            var wm = Weapons.WeaponManager.Instance ?? FindFirstObjectByType<Weapons.WeaponManager>();

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
