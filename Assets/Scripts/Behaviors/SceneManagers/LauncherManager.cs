using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Com.Tempest.Nightmare {

    public class LauncherManager : Photon.PunBehaviour {

        public PhotonLogLevel logLevel = PhotonLogLevel.Informational;
        public byte maxPlayersPerRoom = 5;
        public GameObject controlPanel;
        public GameObject progressLabel;
        public Text versionText;

        private bool isConnecting;
        
	    public void Start() {
            PhotonNetwork.logLevel = logLevel;
            PhotonNetwork.autoJoinLobby = false;
            PhotonNetwork.automaticallySyncScene = true;
            PhotonNetwork.autoCleanUpPlayerObjects = true;
            PhotonNetwork.sendRate = 20;
            PhotonNetwork.sendRateOnSerialize = 20;
            controlPanel.SetActive(true);
            progressLabel.SetActive(false);
            versionText.text = "Game Version: " + GlobalPlayerContainer.GAME_VERSION;
        }

        public void ConnectAsExplorer() {
            GlobalPlayerContainer.Instance.TeamSelection = GlobalPlayerContainer.EXPLORER;
            Connect();
        }

        public void ConnectAsNightmare() {
            GlobalPlayerContainer.Instance.TeamSelection = GlobalPlayerContainer.NIGHTMARE;
            Connect();
        }

        public void ConnectAsObserver() {
            GlobalPlayerContainer.Instance.TeamSelection = GlobalPlayerContainer.OBSERVER;
            Connect();
        }

        private void Connect() {
            isConnecting = true;
            controlPanel.SetActive(false);
            progressLabel.SetActive(true);
            if (!PhotonNetwork.connected) {
                PhotonNetwork.ConnectUsingSettings(GlobalPlayerContainer.GAME_VERSION);
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

