using System.Collections.Generic;
using UnityEngine;
using DopamineCleaner.Enemies;

namespace DopamineCleaner.Weapons
{
    public class OrbitWeapon : WeaponBase
    {
        [Header("위성 프리팹 & 설정")]
        [SerializeField] private GameObject satellitePrefab;
        [SerializeField] private float orbitDistance = 2.2f;
        [SerializeField] private float rotationSpeed = 160f;
        [SerializeField] private float damagePerSecond = 45f;

        private readonly List<GameObject> satellites = new();
        private float currentAngle = 0f;

        private void Awake()
        {
            weaponName = "가디언 위성 (Orbit Barrier)";
            baseCooldown = 0.1f;
        }

        public override void Upgrade()
        {
            if (currentLevel < maxLevel)
            {
                currentLevel++;
                UpdateSatellites();
            }
        }

        public override void Evolve()
        {
            isEvolved = true;
            weaponName = "✨ 은하계 정화 결계 (Galactic Cleanse)";
            orbitDistance = 2.8f;
            rotationSpeed = 260f;
            damagePerSecond = 110f;
            UpdateSatellites();
        }

        protected override void Attack()
        {
            // 위성은 지속 회전 방식으로 동작하므로 Attack()에서는 물리 틱만 유지
        }

        protected override void Update()
        {
            base.Update();
            if (!IsActive) return;

            currentAngle += rotationSpeed * Time.deltaTime;
            if (currentAngle > 360f) currentAngle -= 360f;

            int count = satellites.Count;
            for (int i = 0; i < count; i++)
            {
                float ang = (currentAngle + (360f / count) * i) * Mathf.Deg2Rad;
                Vector3 offset = new Vector3(Mathf.Cos(ang), Mathf.Sin(ang), 0f) * orbitDistance;
                satellites[i].transform.position = transform.position + offset;
            }
        }

        private void UpdateSatellites()
        {
            int targetCount = isEvolved ? 6 : currentLevel;

            while (satellites.Count < targetCount)
            {
                GameObject sat = satellitePrefab != null
                    ? Instantiate(satellitePrefab, transform)
                    : CreateFallbackSatellite();

                satellites.Add(sat);
            }

            for (int i = 0; i < satellites.Count; i++)
            {
                satellites[i].SetActive(i < targetCount);
                if (isEvolved && satellites[i].TryGetComponent<SpriteRenderer>(out var sr))
                {
                    sr.color = new Color(1f, 0.82f, 0.4f); // 골든 은하계
                    satellites[i].transform.localScale = Vector3.one * 0.75f;
                }
            }
        }

        private GameObject CreateFallbackSatellite()
        {
            GameObject sat = new GameObject("Satellite");
            SpriteRenderer sr = sat.AddComponent<SpriteRenderer>();
            sr.color = new Color(1f, 0.46f, 0.56f);
            sat.transform.localScale = Vector3.one * 0.5f;

            CircleCollider2D col = sat.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.5f;

            OrbitSatelliteContact contact = sat.AddComponent<OrbitSatelliteContact>();
            contact.Initialize(this);

            return sat;
        }

        public void OnSatelliteHit(Enemy enemy)
        {
            if (enemy != null && enemy.IsAlive)
            {
                Vector2 pushDir = (enemy.transform.position - transform.position).normalized;
                enemy.TakeDamage(damagePerSecond * Time.deltaTime, pushDir, 0.15f);
            }
        }
    }

    public class OrbitSatelliteContact : MonoBehaviour
    {
        private OrbitWeapon parentWeapon;

        public void Initialize(OrbitWeapon weapon)
        {
            parentWeapon = weapon;
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (other.TryGetComponent<Enemy>(out var enemy))
            {
                parentWeapon?.OnSatelliteHit(enemy);
            }
        }
    }
}
