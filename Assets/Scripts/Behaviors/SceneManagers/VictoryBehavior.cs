﻿using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Com.Tempest.Nightmare {

    public class VictoryBehavior : Photon.PunBehaviour {

        public Text victoryText;

        // Use this for initialization
        void Start() {
            if (GlobalPlayerContainer.Instance.IsWinner) {
                victoryText.text = "Victory";
            } else {
                victoryText.text = "Defeat";
            }
        }

        public void LeaveRoom() {
            PhotonNetwork.LeaveRoom();
        }

        public override void OnLeftRoom() {
            SceneManager.LoadScene("LauncherScene");
        }
    }
}
