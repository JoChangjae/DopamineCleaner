using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DopamineCleaner.Core
{
    public class DamageText : MonoBehaviour
    {
        private TextMesh textMesh;
        private MeshRenderer meshRenderer;

        private void Awake()
        {
            textMesh = gameObject.AddComponent<TextMesh>();
            meshRenderer = GetComponent<MeshRenderer>();

            textMesh.fontSize = 24;
            textMesh.characterSize = 0.15f;
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;
            textMesh.fontStyle = FontStyle.Bold;

            if (meshRenderer != null)
            {
                meshRenderer.sortingOrder = 50; // 최상단 노출
            }
        }

        public void Show(float damage, Vector2 position, bool isCritical)
        {
            transform.position = position + new Vector2(Random.Range(-0.25f, 0.25f), Random.Range(0.2f, 0.5f));
            gameObject.SetActive(true);

            int dmgInt = Mathf.RoundToInt(damage);
            textMesh.text = isCritical ? $"{dmgInt}!" : $"{dmgInt}";
            textMesh.color = isCritical ? new Color(1f, 0.35f, 0.15f, 1f) : new Color(1f, 0.95f, 0.5f, 1f);
            transform.localScale = isCritical ? Vector3.one * 1.35f : Vector3.one;

            StopAllCoroutines();
            StartCoroutine(AnimateRoutine());
        }

        private IEnumerator AnimateRoutine()
        {
            float elapsed = 0f;
            float duration = 0.55f;
            Vector3 startPos = transform.position;
            Vector3 targetPos = startPos + new Vector3(Random.Range(-0.2f, 0.2f), 0.75f, 0f);

            Color initialColor = textMesh.color;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                transform.position = Vector3.Lerp(startPos, targetPos, Mathf.Sin(t * Mathf.PI * 0.5f));

                Color c = initialColor;
                c.a = Mathf.Lerp(1f, 0f, t * t);
                textMesh.color = c;

                yield return null;
            }

            gameObject.SetActive(false);
        }
    }

    public class DamageTextManager : MonoBehaviour
    {
        public static DamageTextManager Instance { get; private set; }

        private readonly Queue<DamageText> pool = new();
        private const int InitialPoolSize = 30;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            for (int i = 0; i < InitialPoolSize; i++)
            {
                CreateNewInstance();
            }
        }

        private DamageText CreateNewInstance()
        {
            GameObject obj = new GameObject("DamageText");
            obj.transform.SetParent(transform);
            DamageText dt = obj.AddComponent<DamageText>();
            obj.SetActive(false);
            pool.Enqueue(dt);
            return dt;
        }

        public void SpawnDamageText(float damage, Vector2 position, bool isCritical = false)
        {
            DamageText dt = null;
            while (pool.Count > 0)
            {
                var candidate = pool.Dequeue();
                if (candidate != null)
                {
                    dt = candidate;
                    break;
                }
            }

            if (dt == null)
            {
                dt = CreateNewInstance();
                pool.Dequeue();
            }

            dt.Show(damage, position, isCritical);
            pool.Enqueue(dt);
        }
    }
}
