using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Com.Tempest.Nightmare {

    public class TalentSceneManager : MonoBehaviour {

        public GameObject explorerPanel;
        public GameObject nightmarePanel;

        public Button purchaseButton;
        public Text unspentEmberText;

        public Text talentNameText;
        public Text talentDescriptionText;
        public Text talentCurrentLevelText;
        public Text talentNextLevelText;

        private bool showingExplorers;
        private ExplorerEnum currentExplorer;
        private NightmareEnum currentNightmare;
        private TalentEnum selectedTalent;

        private Dictionary<TalentEnum, int> currentTalentRanks;
        private Dictionary<TalentEnum, TalentButton> sceneButtons;

        void Awake() {
            showingExplorers = true;
            currentExplorer = ExplorerEnum.DOUBLE_JUMP;
            currentNightmare = NightmareEnum.GHAST;
            sceneButtons = new Dictionary<TalentEnum, TalentButton>();
            foreach (TalentButton button in FindObjectsOfType<TalentButton>()) {
                sceneButtons[button.selectedTalent] = button;
            }
            ReloadTalents();
            selectedTalent = TalentEnum.SIGHT_RANGE;
            HandleTalentDisplayPanel();
        }

        #region UI Handling.

        private void ReloadTalents() {
            if (showingExplorers) {
                switch (currentExplorer) {
                    case ExplorerEnum.DOUBLE_JUMP:
                        currentTalentRanks = GlobalTalentContainer.GetInstance().DoubleJumpTalents;
                        break;
                    case ExplorerEnum.JETPACK:
                        currentTalentRanks = GlobalTalentContainer.GetInstance().JetpackTalents;
                        break;
                    case ExplorerEnum.DASH:
                        currentTalentRanks = GlobalTalentContainer.GetInstance().DashTalents;
                        break;
                }
            } else {
                switch (currentNightmare) {
                    case NightmareEnum.GHAST:
                        currentTalentRanks = GlobalTalentContainer.GetInstance().GhastTalents;
                        break;
                    case NightmareEnum.CRYO:
                        currentTalentRanks = GlobalTalentContainer.GetInstance().CryoTalents;
                        break;
                    case NightmareEnum.GOBLIN:
                        currentTalentRanks = GlobalTalentContainer.GetInstance().GoblinTalents;
                        break;
                }
            }
            explorerPanel.SetActive(showingExplorers);
            foreach (TalentEnum talent in Enum.GetValues(typeof(TalentEnum))) {
                if (!sceneButtons.ContainsKey(talent)) continue;
                TalentInfoContainer.DescriptionContainer description = TalentInfoContainer.infoDictionary[talent];
                TalentButton button = sceneButtons[talent];
                button.SetRankLabel(currentTalentRanks[talent], description.NumRanks);
            }
            unspentEmberText.text = "Unspent Embers: " + GlobalTalentContainer.GetInstance().UnspentEmbers;
        }

        private void HandleTalentDisplayPanel() {
            int currentRank = currentTalentRanks[selectedTalent];
            TalentInfoContainer.DescriptionContainer description = TalentInfoContainer.infoDictionary[selectedTalent];

            talentNameText.text = description.Name;
            talentDescriptionText.text = description.Description;

            if (currentRank == 0) {
                talentCurrentLevelText.text = "Current Level:\nThis talent has not been purchased.";
            } else {
                talentCurrentLevelText.text = "Current Level:\n" + description.LevelDescriptions[currentRank - 1];
            }

            if (currentRank == description.NumRanks) {
                talentNextLevelText.text = "Next Level:\nThis talent is already at the maximum level.";
                purchaseButton.gameObject.SetActive(false);
            } else {
                talentNextLevelText.text = "Next Level:\n" + description.LevelDescriptions[currentRank];
                purchaseButton.gameObject.SetActive(true);
                int cost = (currentRank + 1) * description.BaseCost;
                purchaseButton.GetComponentInChildren<Text>().text = "Purchase (" + cost + ")";
            }
        }

        #endregion

        #region Button Handling.

        public void OnTalentClick(TalentEnum talentButtonEnum) {
            selectedTalent = talentButtonEnum;
            HandleTalentDisplayPanel();
        }
        
        public void OnTalentPurchase() {
            TalentInfoContainer.DescriptionContainer description = TalentInfoContainer.infoDictionary[selectedTalent];
            int currentRank = currentTalentRanks[selectedTalent];
            int cost = (currentRank + 1) * description.BaseCost;
            if (currentRank < description.NumRanks && cost <= GlobalTalentContainer.GetInstance().UnspentEmbers) {
                currentTalentRanks[selectedTalent]++;
                GlobalTalentContainer.GetInstance().UnspentEmbers -= cost;
                ReloadTalents();
                HandleTalentDisplayPanel();
            }
        }

        public void OnApplyClicked() {
            GlobalTalentContainer.SaveInstance();
            SceneManager.LoadScene("LauncherScene");
        }

        public void OnCancelClicked() {
            GlobalTalentContainer.ForceReload();
            SceneManager.LoadScene("LauncherScene");
        }

        #endregion

        #region Character selection button callbacks.

        public void OnDoubleJumpClick() {
            showingExplorers = true;
            currentExplorer = ExplorerEnum.DOUBLE_JUMP;
            ReloadTalents();
        }

        public void OnJetpackClick() {
            showingExplorers = true;
            currentExplorer = ExplorerEnum.JETPACK;
            ReloadTalents();
        }

        public void OnDashClick() {
            showingExplorers = true;
            currentExplorer = ExplorerEnum.DASH;
            ReloadTalents();
        }

        public void OnGhastClick() {
            showingExplorers = false;
            currentNightmare = NightmareEnum.GHAST;
            ReloadTalents();
        }

        public void OnCryoClick() {
            showingExplorers = false;
            currentNightmare = NightmareEnum.CRYO;
            ReloadTalents();
        }

        public void OnGoblinClick() {
            showingExplorers = false;
            currentNightmare = NightmareEnum.GOBLIN;
            ReloadTalents();
        }

        #endregion
    }
}
