using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Com.Tempest.Nightmare {

    public class TalentSceneManager : MonoBehaviour {

        public GameObject explorerPanel;
        public GameObject nightmarePanel;

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

        public void OnTalentClick(TalentEnum talentButtonEnum) {
            selectedTalent = talentButtonEnum;
            HandleTalentDisplayPanel();
        }

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
        }

        private void HandleTalentDisplayPanel() {
            int currentRank = currentTalentRanks[selectedTalent];
            TalentInfoDictionaryContainer.TalentDescriptions description = TalentInfoDictionaryContainer.infoDictionary[selectedTalent];

            talentNameText.text = description.Name;
            talentDescriptionText.text = description.Description;

            if (currentRank == 0) {
                talentCurrentLevelText.text = "Current Level:\nThis talent has not been purchased.";
            } else {
                talentCurrentLevelText.text = "Current Level:\n" + description.LevelDescriptions[currentRank - 1];
            }

            if (currentRank == description.NumRanks) {
                talentNextLevelText.text = "Next Level:\nThis talent is already at the maximum level.";
            } else {
                talentNextLevelText.text = "Next Level:\n" + description.LevelDescriptions[currentRank];
            }
        }
    }
}
