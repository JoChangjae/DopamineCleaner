using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DopamineCleaner.Characters;
using DopamineCleaner.Meta;

namespace DopamineCleaner.Town
{
    public class CharacterSelectUI : MonoBehaviour
    {
        [Header("캐릭터 카드 컨테이너")]
        [SerializeField] private Transform cardContainer;
        [SerializeField] private GameObject characterCardPrefab;

        private GameObject companionBar;

        public void RefreshUI()
        {
            if (cardContainer == null || characterCardPrefab == null || GameDataManager.Instance == null) return;

            foreach (Transform child in cardContainer)
            {
                Destroy(child.gameObject);
            }

            var allChars = CharacterDatabase.GetAll();
            var profile = GameDataManager.Instance.Profile;

            foreach (var charDef in allChars)
            {
                CreateCard(charDef, profile);
            }

            RefreshCompanionBar();
        }

        private void CreateCard(CharacterDefinition def, PlayerProfileData profile)
        {
            GameObject card = Instantiate(characterCardPrefab, cardContainer);

            Text iconText = card.transform.Find("Icon")?.GetComponent<Text>();
            Text nameText = card.transform.Find("Name")?.GetComponent<Text>();
            Text titleText = card.transform.Find("Title")?.GetComponent<Text>();
            Text statsText = card.transform.Find("Stats")?.GetComponent<Text>();
            Text passiveText = card.transform.Find("Passive")?.GetComponent<Text>();
            Button actionBtn = card.transform.Find("ActionButton")?.GetComponent<Button>();
            Text btnText = actionBtn?.GetComponentInChildren<Text>();

            bool isUnlocked = profile.unlockedCharacterIds.Contains(def.id);
            bool isCurrent = profile.selectedCharacterId == def.id;

            CharacterCareerData career = GameDataManager.Instance.GetCareerData(def.id);
            CareerRankDefinition currentRank = CareerDatabase.Get((CareerRank)career.rankLevel);
            CareerRankDefinition nextRank = CareerDatabase.GetNext((CareerRank)career.rankLevel);

            Image portraitImg = card.transform.Find("Portrait")?.GetComponent<Image>();
            if (portraitImg != null)
            {
                Sprite sp = Resources.Load<Sprite>($"Characters/{def.id}") ?? 
                           Resources.Load<Sprite>($"Characters/{def.id.Replace("char_", "char_")}") ??
                           AssetDatabaseLoadSprite($"Assets/Sprites/Characters/{def.id}.png");
                if (sp != null)
                {
                    portraitImg.sprite = sp;
                    portraitImg.gameObject.SetActive(true);
                    if (iconText != null) iconText.gameObject.SetActive(false);
                }
            }

            if (iconText != null && (portraitImg == null || !portraitImg.gameObject.activeSelf))
            {
                iconText.text = def.iconEmoji;
            }
            if (nameText != null) nameText.text = def.name;

            if (titleText != null)
            {
                titleText.text = $"{currentRank.badgeEmoji} {currentRank.rankTitle} ({def.jobTitle})";
                titleText.color = career.rankLevel switch
                {
                    2 => new Color(1f, 0.85f, 0.2f),   // 골드
                    1 => new Color(0.4f, 0.85f, 1f),   // 시안
                    _ => new Color(0.75f, 0.70f, 0.90f) // 연보라
                };
            }

            if (statsText != null)
            {
                float finalHp = def.baseMaxHP * (1f + currentRank.hpBonusPercent);
                float finalSpd = def.baseMoveSpeed * (1f + currentRank.spdBonusPercent);
                float finalDmg = def.baseDamageMult * (1f + currentRank.dmgBonusPercent);
                statsText.text = $"체력: {finalHp:F0}  |  이속: {finalSpd:F1}  |  공격력: x{finalDmg:F2}\n[정화 작전 성공: {career.clearCount}회]";
            }

            if (passiveText != null)
            {
                string rankPerk = career.rankLevel > 0 ? $"\n<color=#66FFAA>[직급 특전]</color> {currentRank.perksDesc}" : "";
                passiveText.text = $"[고유 특성]\n{def.passiveDesc}{rankPerk}";
            }

            // 승진 버튼 (카드의 ActionButton 위쪽에 배치 또는 검색)
            Transform promoteTr = card.transform.Find("PromoteButton");
            Button promoteBtn = promoteTr != null ? promoteTr.GetComponent<Button>() : null;
            if (promoteBtn == null && isUnlocked)
            {
                promoteBtn = CreateDynamicPromoteButton(card.transform);
            }

            if (promoteBtn != null)
            {
                Text pTxt = promoteBtn.GetComponentInChildren<Text>();
                promoteBtn.onClick.RemoveAllListeners();

                if (!isUnlocked)
                {
                    promoteBtn.gameObject.SetActive(false);
                }
                else if (nextRank != null)
                {
                    promoteBtn.gameObject.SetActive(true);
                    if (career.clearCount >= nextRank.requiredClears)
                    {
                        if (pTxt != null) pTxt.text = $"🌟 [특진 승진!] {nextRank.badgeEmoji} {nextRank.rankTitle}";
                        promoteBtn.interactable = true;
                        promoteBtn.GetComponent<Image>().color = new Color(0.9f, 0.65f, 0.15f); // 황금빛
                        promoteBtn.onClick.AddListener(() =>
                        {
                            Core.SoundManager.Instance?.Play(Core.SoundEffect.LevelUp, 0.9f);
                            GameDataManager.Instance.PromoteCharacter(def.id);
                            RefreshUI();
                        });
                    }
                    else
                    {
                        if (pTxt != null) pTxt.text = $"🔒 승진 조건: {career.clearCount}/{nextRank.requiredClears} 클리어";
                        promoteBtn.interactable = false;
                        promoteBtn.GetComponent<Image>().color = new Color(0.28f, 0.25f, 0.35f);
                    }
                }
                else
                {
                    promoteBtn.gameObject.SetActive(true);
                    promoteBtn.interactable = false;
                    if (pTxt != null) pTxt.text = $"👑 최고 직급 [{currentRank.rankTitle}] 달성";
                    promoteBtn.GetComponent<Image>().color = new Color(0.4f, 0.3f, 0.55f);
                }
            }

            if (actionBtn != null && btnText != null)
            {
                actionBtn.onClick.RemoveAllListeners();

                if (isCurrent)
                {
                    btnText.text = "✅ 출동 중인 요원";
                    actionBtn.interactable = false;
                }
                else if (isUnlocked)
                {
                    btnText.text = "요원 선택";
                    actionBtn.interactable = true;
                    actionBtn.onClick.AddListener(() =>
                    {
                        Core.SoundManager.Instance?.Play(Core.SoundEffect.Dash, 0.5f);
                        GameDataManager.Instance.SelectCharacter(def.id);
                        RefreshUI();
                    });
                }
                else
                {
                    string costStr = $"{def.unlockGoldCost}G";
                    if (def.unlockKarmaCost > 0) costStr += $" + {def.unlockKarmaCost}P";
                    btnText.text = $"🔓 영입 ({costStr})";

                    bool canAfford = profile.gold >= def.unlockGoldCost && profile.karmaPoints >= def.unlockKarmaCost;
                    actionBtn.interactable = canAfford;

                    actionBtn.onClick.AddListener(() =>
                    {
                        if (GameDataManager.Instance.UnlockCharacter(def.id))
                        {
                            Core.SoundManager.Instance?.Play(Core.SoundEffect.LevelUp, 0.8f);
                            RefreshUI();
                        }
                    });
                }
            }
        }

        private Button CreateDynamicPromoteButton(Transform parent)
        {
            GameObject pObj = new GameObject("PromoteButton");
            pObj.transform.SetParent(parent, false);
            RectTransform pRt = pObj.AddComponent<RectTransform>();
            pRt.sizeDelta = new Vector2(330, 48);
            pRt.anchoredPosition = new Vector2(0, -188);

            Image img = pObj.AddComponent<Image>();
            img.color = new Color(0.35f, 0.32f, 0.45f);
            Button btn = pObj.AddComponent<Button>();

            GameObject tObj = new GameObject("Text");
            tObj.transform.SetParent(pObj.transform, false);
            RectTransform tRt = tObj.AddComponent<RectTransform>();
            tRt.anchorMin = Vector2.zero;
            tRt.anchorMax = Vector2.one;
            tRt.sizeDelta = Vector2.zero;

            Text txt = tObj.AddComponent<Text>();
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.fontSize = 17;
            txt.fontStyle = FontStyle.Bold;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = Color.white;

            return btn;
        }

        private void RefreshCompanionBar()
        {
            if (companionBar == null)
            {
                companionBar = new GameObject("CompanionSelectorBar");
                companionBar.transform.SetParent(transform, false);

                RectTransform rt = companionBar.AddComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.5f, 0f);
                rt.anchorMax = new Vector2(0.5f, 0f);
                rt.pivot = new Vector2(0.5f, 0f);
                rt.sizeDelta = new Vector2(1520, 56);
                rt.anchoredPosition = new Vector2(0, 10);

                Image bg = companionBar.AddComponent<Image>();
                bg.color = new Color(0.14f, 0.11f, 0.22f, 0.95f);
                var outl = companionBar.AddComponent<Outline>();
                outl.effectColor = new Color(0.35f, 0.85f, 1f, 0.7f);
                outl.effectDistance = new Vector2(1f, -1f);

                HorizontalLayoutGroup hlg = companionBar.AddComponent<HorizontalLayoutGroup>();
                hlg.spacing = 14;
                hlg.childAlignment = TextAnchor.MiddleCenter;
                hlg.childControlWidth = false;
                hlg.childControlHeight = false;
            }

            foreach (Transform child in companionBar.transform)
            {
                Destroy(child.gameObject);
            }

            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var profile = GameDataManager.Instance.Profile;

            // 라벨 텍스트
            GameObject lblObj = new GameObject("CompLabel");
            lblObj.transform.SetParent(companionBar.transform, false);
            RectTransform lRt = lblObj.AddComponent<RectTransform>();
            lRt.sizeDelta = new Vector2(230, 46);
            Text lTxt = lblObj.AddComponent<Text>();
            lTxt.text = "🐾 [전술 동반자 드론/펫]:";
            lTxt.fontSize = 16;
            lTxt.alignment = TextAnchor.MiddleCenter;
            lTxt.color = new Color(1f, 0.85f, 0.3f);
            lTxt.font = font;

            var companions = CompanionDatabase.GetAll();
            foreach (var comp in companions)
            {
                GameObject btnObj = new GameObject($"Comp_{comp.id}");
                btnObj.transform.SetParent(companionBar.transform, false);
                RectTransform bRt = btnObj.AddComponent<RectTransform>();
                bRt.sizeDelta = new Vector2(285, 46);

                Image img = btnObj.AddComponent<Image>();
                bool isEquipped = profile.equippedCompanionId == comp.id;
                bool isUnlocked = profile.unlockedCompanionIds != null && profile.unlockedCompanionIds.Contains(comp.id);

                img.color = isEquipped ? new Color(0.25f, 0.55f, 0.4f) : new Color(0.20f, 0.18f, 0.28f);

                Button btn = btnObj.AddComponent<Button>();
                var bOut = btnObj.AddComponent<Outline>();
                bOut.effectColor = isEquipped ? new Color(0.4f, 1f, 0.6f) : new Color(0f, 0f, 0f, 0.6f);
                bOut.effectDistance = new Vector2(1.5f, -1.5f);

                GameObject txtObj = new GameObject("Text");
                txtObj.transform.SetParent(btnObj.transform, false);
                RectTransform tRt = txtObj.AddComponent<RectTransform>();
                tRt.anchorMin = Vector2.zero;
                tRt.anchorMax = Vector2.one;
                tRt.sizeDelta = Vector2.zero;

                Text t = txtObj.AddComponent<Text>();
                string status = isEquipped ? "✅ [동행 중]" : (isUnlocked ? "동행 선택" : "500G 영입");
                t.text = $"{comp.icon} {comp.name} {status}";
                t.fontSize = 14;
                t.alignment = TextAnchor.MiddleCenter;
                t.color = Color.white;
                t.font = font;

                btn.onClick.AddListener(() =>
                {
                    if (isUnlocked)
                    {
                        Core.SoundManager.Instance?.Play(Core.SoundEffect.Dash, 0.5f);
                        GameDataManager.Instance.SelectCompanion(comp.id);
                        RefreshUI();
                    }
                    else
                    {
                        if (GameDataManager.Instance.UnlockCompanion(comp.id, 500))
                        {
                            Core.SoundManager.Instance?.Play(Core.SoundEffect.LevelUp, 0.7f);
                            RefreshUI();
                        }
                    }
                });
            }
        }

        private Sprite AssetDatabaseLoadSprite(string path)
        {
#if UNITY_EDITOR
            return UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(path);
#else
            return null;
#endif
        }
    }
}
