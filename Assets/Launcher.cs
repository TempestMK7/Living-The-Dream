using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Tempest.Nightmare {

    public class Launcher : Photon.PunBehaviour {

        public PhotonLogLevel logLevel = PhotonLogLevel.Informational;
        [Tooltip("The maximum number of players per room.")]
        public byte maxPlayersPerRoom = 5;
        [Tooltip("The parent of the connect button and name input field.")]
        public GameObject controlPanel;
        [Tooltip("The label that shows 'Connecting...' while connection is being established.")]
        public GameObject progressLabel;

        private string gameVersion = "0.01";
        private bool isConnecting;
        
	    public void Start() {
            PhotonNetwork.logLevel = logLevel;
            PhotonNetwork.autoJoinLobby = false;
            PhotonNetwork.automaticallySyncScene = true;
            PhotonNetwork.autoCleanUpPlayerObjects = true;
            PhotonNetwork.sendRate = 60;
            PhotonNetwork.sendRateOnSerialize = 60;
            controlPanel.SetActive(true);
            progressLabel.SetActive(false);
	    }

        public void Connect() {
            isConnecting = true;
            controlPanel.SetActive(false);
            progressLabel.SetActive(true);
            if (!PhotonNetwork.connected) {
                PhotonNetwork.ConnectUsingSettings(gameVersion);
            } else {
                PhotonNetwork.JoinRandomRoom();
            }
        }

        public override void OnConnectedToMaster() {
            if (isConnecting) {
                Debug.Log("Successfully connected to master. Joining random room.");
                PhotonNetwork.JoinRandomRoom();
            }
        }

        public override void OnDisconnectedFromPhoton() {
            Debug.Log("Disconnected from Photon.");
            controlPanel.SetActive(true);
            progressLabel.SetActive(false);
        }

        public override void OnPhotonRandomJoinFailed(object[] codeAndMsg) {
            Debug.Log("Random join failed. Creating new room.");
            PhotonNetwork.CreateRoom(null, new RoomOptions() { MaxPlayers = maxPlayersPerRoom }, null);
        }

        public override void OnJoinedRoom() {
            Debug.Log("This client is now in a room.");
            if (PhotonNetwork.room.PlayerCount == 1) {
                PhotonNetwork.LoadLevel("LobbyScene");
            }
        }
    }
}

