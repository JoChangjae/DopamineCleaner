using System.Collections;
using UnityEngine;
using DopamineCleaner.Core;
using DopamineCleaner.Characters;
using DopamineCleaner.Items;

namespace DopamineCleaner.Enemies
{
    public enum EnemyType
    {
        MindDust,          // 잡생각 먼지 (약하지만 떼지어 몰려옴)
        HeavySloth,        // 쏟아지는 졸음 (탱커형)
        NotificationGhost, // 알림 유령 (스마트폰 알림, 고속 이동)
        DopamineBoss       // 도파민 거대 보스
    }

    [RequireComponent(typeof(Collider2D))]
    public class Enemy : MonoBehaviour
    {
        [Header("몬스터 스탯")]
        [SerializeField] private EnemyType enemyType = EnemyType.MindDust;
        [SerializeField] private float maxHP = 15f;
        [SerializeField] private float currentHP;
        [SerializeField] private float moveSpeed = 2.5f;
        [SerializeField] private float damage = 5f;
        [SerializeField] private int baseExp = 1;

        [Header("컴포넌트")]
        private SpriteRenderer spriteRenderer;
        private Color originalColor;
        private Coroutine flashCoroutine;

        public EnemyType Type => enemyType;
        public float HP => currentHP;
        public float MaxHP => maxHP;
        public float Damage => damage;
        public bool IsAlive => currentHP > 0f;

        private void Awake()
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            if (spriteRenderer != null)
                originalColor = spriteRenderer.color;
        }

        public void Initialize(EnemyType type, int stage, float survivalSec)
        {
            enemyType = type;
            float stageMult = 1.0f + (stage - 1) * 0.32f;

            switch (type)
            {
                case EnemyType.MindDust:
                    maxHP = (12f + survivalSec * 0.15f) * stageMult;
                    moveSpeed = Random.Range(2.4f, 3.0f);
                    damage = 5f + (stage - 1) * 2f;
                    baseExp = 1;
                    SetVisualColor(new Color(0.85f, 0.78f, 0.92f)); // 연보라
                    transform.localScale = Vector3.one * 0.9f;
                    break;

                case EnemyType.HeavySloth:
                    maxHP = (28f + survivalSec * 0.28f) * stageMult;
                    moveSpeed = Random.Range(1.6f, 2.0f);
                    damage = 10f + (stage - 1) * 3f;
                    baseExp = 3;
                    SetVisualColor(new Color(0.75f, 0.88f, 0.92f)); // 포근한 하늘빛
                    transform.localScale = Vector3.one * 1.3f;
                    break;

                case EnemyType.NotificationGhost:
                    maxHP = (45f + survivalSec * 0.40f) * stageMult;
                    moveSpeed = Random.Range(3.4f, 4.2f);
                    damage = 14f + (stage - 1) * 4f;
                    baseExp = 8;
                    SetVisualColor(new Color(1.0f, 0.76f, 0.82f)); // 핑크
                    transform.localScale = Vector3.one * 1.1f;
                    break;

                case EnemyType.DopamineBoss:
                    maxHP = (380f + (stage - 1) * 260f) * stageMult;
                    moveSpeed = 1.8f + Mathf.Min(0.6f, (stage - 1) * 0.12f);
                    damage = 22f + (stage - 1) * 6f;
                    baseExp = 35;
                    SetVisualColor(GetBossStageColor(stage));
                    transform.localScale = Vector3.one * 2.8f;
                    break;
            }

            if (Meta.GameDataManager.Instance != null)
            {
                var diff = Meta.DungeonDatabase.GetDifficultyMultiplier(Meta.GameDataManager.Instance.SelectedDifficulty);
                maxHP *= diff.enemyHpMult;
                damage *= diff.enemyDmgMult;
                moveSpeed *= diff.enemySpeedMult;
            }

            currentHP = maxHP;
            gameObject.SetActive(true);
        }

        private void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing)
                return;

            if (PlayerController.Instance == null || !IsAlive) return;

            // 플레이어 추적 이동
            Vector3 playerPos = PlayerController.Instance.transform.position;
            Vector3 dir = (playerPos - transform.position).normalized;
            transform.position += dir * (moveSpeed * Time.deltaTime);

            // 스프라이트 반전
            if (spriteRenderer != null && dir.x != 0)
            {
                spriteRenderer.flipX = dir.x < 0;
            }
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (other.CompareTag("Player") && IsAlive)
            {
                PlayerController.Instance?.TakeDamage(damage);
            }
        }

        public void TakeDamage(float amount, Vector2 knockbackDir, float knockbackForce = 0f)
        {
            if (!IsAlive) return;

            currentHP -= amount;

            // 넉백 적용
            if (knockbackForce > 0f)
            {
                transform.position += (Vector3)(knockbackDir.normalized * knockbackForce);
            }

            // 피격 플래시
            if (flashCoroutine != null) StopCoroutine(flashCoroutine);
            flashCoroutine = StartCoroutine(FlashRoutine());

            if (currentHP <= 0f)
            {
                Die();
            }
        }

        private IEnumerator FlashRoutine()
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.color = Color.white;
                yield return new WaitForSeconds(0.08f);
                spriteRenderer.color = originalColor;
            }
        }

        private void Die()
        {
            currentHP = 0f;
            bool isBoss = (enemyType == EnemyType.DopamineBoss);

            // 킬 카운트 등록
            GameManager.Instance?.RegisterKill(isBoss);

            // 아이템 및 경험치 드랍
            DropLoot(isBoss);

            gameObject.SetActive(false);
        }

        private void DropLoot(bool isBoss)
        {
            if (isBoss)
            {
                // 보스 격파 시 확정 황금상자 & 대량 EXP 드랍
                ItemManager.Instance?.SpawnGem(transform.position, GemType.Chest, 0);
                ItemManager.Instance?.SpawnGem(transform.position + Vector3.left * 1.2f, GemType.Cake, 0);
                ItemManager.Instance?.SpawnGem(transform.position + Vector3.right * 1.2f, GemType.Magnet, 0);
                ItemManager.Instance?.SpawnGem(transform.position + Vector3.up * 1.0f, GemType.Pink, 35);
            }
            else
            {
                // 일반 적 드랍
                GemType gType = GemType.Green;
                if (enemyType == EnemyType.HeavySloth) gType = GemType.Blue;
                else if (enemyType == EnemyType.NotificationGhost) gType = GemType.Pink;

                ItemManager.Instance?.SpawnGem(transform.position, gType, baseExp);

                // 무작위 특수 아이템 드랍 (3% 케이크, 2.5% 폭탄, 0.5% 보물상자)
                int roll = Random.Range(0, 1000);
                if (roll < 30)
                    ItemManager.Instance?.SpawnGem(transform.position + Vector3.up * 0.4f, GemType.Cake, 0);
                else if (roll < 55)
                    ItemManager.Instance?.SpawnGem(transform.position + Vector3.up * 0.4f, GemType.Bomb, 0);
                else if (roll < 60)
                    ItemManager.Instance?.SpawnGem(transform.position + Vector3.up * 0.4f, GemType.Chest, 0);
            }
        }

        private void SetVisualColor(Color c)
        {
            originalColor = c;
            if (spriteRenderer != null)
                spriteRenderer.color = c;
        }

        private Color GetBossStageColor(int stage)
        {
            return stage switch
            {
                1 => new Color(0.48f, 0.49f, 0.86f), // 1스테이지: 라벤더 퍼플
                2 => new Color(0.23f, 0.53f, 1.0f),  // 2스테이지: 오션 블루
                3 => new Color(1.0f, 0.0f, 0.43f),   // 3스테이지: 네온 마젠타
                4 => new Color(1.0f, 0.35f, 0.37f),  // 4스테이지: 분노의 코랄 레드
                _ => new Color(0.51f, 0.22f, 0.93f)  // 5스테이지+: 다크 사이버 퍼플
            };
        }
    }
}
