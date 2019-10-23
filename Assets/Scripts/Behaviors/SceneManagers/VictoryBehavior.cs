using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Com.Tempest.Nightmare {

    public class VictoryBehavior : Photon.PunBehaviour {

        public Text victoryText;

        public Text rescueText;
        public Text objectiveText;
        public Text upgradeText;
        public Text totalText;

        public void Awake() {
            if (PlayerStateContainer.Instance.IsWinner) {
                victoryText.text = "Victory";
            } else {
                victoryText.text = "Defeat";
            }

            rescueText.text = "Rescue Embers: " + PlayerStateContainer.Instance.RescueEmbers;
            objectiveText.text = "Objective Embers: " + (int) PlayerStateContainer.Instance.ObjectiveEmbers;
            upgradeText.text = "Upgrade Embers: " + PlayerStateContainer.Instance.UpgradeEmbers;
            totalText.text = "Total Embers: " + PlayerStateContainer.Instance.TotalEmbers();

            if (PlayerStateContainer.Instance.TeamSelection == PlayerStateContainer.NIGHTMARE) {
                rescueText.enabled = false;
            }

            GlobalTalentContainer.GetInstance().UnspentEmbers += PlayerStateContainer.Instance.TotalEmbers();
            GlobalTalentContainer.SaveInstance();
        }

        public void LeaveRoom() {
            PhotonNetwork.LeaveRoom();
        }

        public override void OnLeftRoom() {
            SceneManager.LoadScene("LauncherScene");
        }
    }
}
