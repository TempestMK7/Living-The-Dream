using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Com.Tempest.Nightmare {

    [RequireComponent(typeof(InputField))]
    public class PlayerNameInputScript : MonoBehaviour {

        private static string playerNameKey = "PlayerName";

        // Use this for initialization
        void Start() {
            string defaultName = "";
            InputField inputField = GetComponent<InputField>();
            if (inputField != null && PlayerPrefs.HasKey(playerNameKey)) {
                defaultName = PlayerPrefs.GetString(playerNameKey);
                inputField.text = defaultName;
            }
            PhotonNetwork.playerName = defaultName;
        }

        public void SetPlayerName(string value) {
            if (value != "") {
                PhotonNetwork.playerName = value;
                PlayerPrefs.SetString(playerNameKey, value);
            }
        }
    }
}
