using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DopamineCleaner.Meta;
using DopamineCleaner.Core;

namespace DopamineCleaner.Town
{
    public class HallOfFameUI : MonoBehaviour
    {
        [Header("UI 컨테이너")]
        [SerializeField] private Transform recordListContainer;
        [SerializeField] private Button refreshButton;

        private void Start()
        {
            if (refreshButton != null)
            {
                refreshButton.onClick.AddListener(RefreshLeaderboard);
            }

            RefreshLeaderboard();
        }

        public void RefreshLeaderboard()
        {
            if (recordListContainer == null) return;

            // 기존 슬롯 청소
            for (int i = recordListContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(recordListContainer.GetChild(i).gameObject);
            }

            List<CombatRecord> records = GameDataManager.Instance?.Profile?.combatRecords ?? new List<CombatRecord>();

            // 가상 기본 기록 추가 (기록이 적을 경우 튜토리얼 기본 랭커 데이터 채움)
            List<CombatRecord> allRecords = new List<CombatRecord>(records);
            if (allRecords.Count < 5)
            {
                allRecords.Add(new CombatRecord { characterId = "char_intern", survivalSeconds = 300, kills = 420, goldEarned = 1500, karmaEarned = 2, isCleared = true, timestamp = "2026-10-18 14:00" });
                allRecords.Add(new CombatRecord { characterId = "char_director", survivalSeconds = 420, kills = 680, goldEarned = 2800, karmaEarned = 4, isCleared = true, timestamp = "2026-10-18 19:30" });
                allRecords.Add(new CombatRecord { characterId = "char_intern", survivalSeconds = 240, kills = 310, goldEarned = 980, karmaEarned = 1, isCleared = false, timestamp = "2026-10-19 10:15" });
            }

            // 점수 기준 내림차순 정렬 (생존 시간 * 2 + 킬수 * 3 + 골드 / 10 + 클리어보너스 1000)
            allRecords.Sort((a, b) =>
            {
                int scoreA = a.survivalSeconds * 2 + a.kills * 3 + a.goldEarned / 10 + (a.isCleared ? 1000 : 0);
                int scoreB = b.survivalSeconds * 2 + b.kills * 3 + b.goldEarned / 10 + (b.isCleared ? 1000 : 0);
                return scoreB.CompareTo(scoreA);
            });

            int displayCount = Mathf.Min(10, allRecords.Count);
            for (int rank = 1; rank <= displayCount; rank++)
            {
                CombatRecord r = allRecords[rank - 1];
                CreateRankCard(rank, r);
            }
        }

        private void CreateRankCard(int rank, CombatRecord r)
        {
            GameObject card = new GameObject($"RankCard_{rank}");
            card.transform.SetParent(recordListContainer, false);

            RectTransform rt = card.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(1300, 58);

            Image bg = card.AddComponent<Image>();

            // 1, 2, 3위 특별 배경색
            if (rank == 1) bg.color = new Color(0.35f, 0.28f, 0.12f, 0.95f); // 황금 1위
            else if (rank == 2) bg.color = new Color(0.24f, 0.28f, 0.35f, 0.95f); // 은빛 2위
            else if (rank == 3) bg.color = new Color(0.32f, 0.22f, 0.18f, 0.95f); // 동빛 3위
            else bg.color = new Color(0.14f, 0.12f, 0.19f, 0.85f); // 일반

            string rankBadge = rank switch
            {
                1 => "👑 <b>1위</b>",
                2 => "🥈 <b>2위</b>",
                3 => "🥉 <b>3위</b>",
                _ => $"#{rank:D2}"
            };

            string charName = r.characterId == "char_director" ? "👔 윤이사 (총괄)" : "🧑‍💻 인턴 (도파민 요원)";
            string resultBadge = r.isCleared ? "<color=#4DFFA0>【정화 성공】</color>" : "<color=#FF6666>【작전 중단】</color>";
            int sec = r.survivalSeconds;

            GameObject txtObj = new GameObject("Text");
            txtObj.transform.SetParent(card.transform, false);
            RectTransform trt = txtObj.AddComponent<RectTransform>();
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.offsetMin = new Vector2(25, 0);
            trt.offsetMax = new Vector2(-25, 0);

            Text txt = txtObj.AddComponent<Text>();
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.fontSize = 20;
            txt.alignment = TextAnchor.MiddleLeft;
            txt.color = (rank == 1) ? new Color(1f, 0.9f, 0.4f) : Color.white;

            txt.text = $"{rankBadge,-12}  |  {charName,-18}  |  ⏱️ {sec / 60:D2}:{sec % 60:D2}  |  👾 {r.kills} Kills  |  💰 +{r.goldEarned:N0} G  |  {resultBadge}  |  <size=15><color=#A0A0B5>{r.timestamp}</color></size>";
        }
    }
}
