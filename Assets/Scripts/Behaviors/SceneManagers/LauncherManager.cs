﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Com.Tempest.Nightmare {

    public class LauncherManager : Photon.PunBehaviour {

        public PhotonLogLevel logLevel = PhotonLogLevel.ErrorsOnly;
        public GameObject startPanel;
        public GameObject connectPanel;
        public GameObject progressionPanel;

        public GameObject progressLabel;
        public Text versionText;
        public Text unspentEmberText;

        private bool isConnecting;
        
	    public void Awake() {
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;
            PhotonNetwork.offlineMode = false;

            PhotonNetwork.logLevel = logLevel;
            PhotonNetwork.autoJoinLobby = false;
            PhotonNetwork.automaticallySyncScene = true;
            PhotonNetwork.autoCleanUpPlayerObjects = true;
            PhotonNetwork.sendRate = 30;
            PhotonNetwork.sendRateOnSerialize = 30;
            OpenStartPanel();
            progressLabel.SetActive(false);
            versionText.text = "Game Version: " + Constants.GAME_VERSION;
            PlayerStateContainer.ResetInstance();
            AccountStateContainer.getInstance();
        }

        public void ExitGame() {
            Application.Quit();
        }

        public void OpenStartPanel() {
            startPanel.SetActive(true);
            connectPanel.SetActive(false);
            progressionPanel.SetActive(false);
        }

        public void OpenConnectPanel() {
            startPanel.SetActive(false);
            connectPanel.SetActive(true);
            progressionPanel.SetActive(false);
        }

        public void OpenProgressionPanel() {
            startPanel.SetActive(false);
            connectPanel.SetActive(false);
            progressionPanel.SetActive(true);
            unspentEmberText.text = "Unspent Embers: " + AccountStateContainer.getInstance().unspentEmbers;
        }

        public void CloseAllPanels() {
            startPanel.SetActive(false);
            connectPanel.SetActive(false);
            progressionPanel.SetActive(false);
        }

        public void LaunchDemoScene() {
            SceneManager.LoadScene("DemoScene");
        }

        public void ConnectAsExplorer() {
            PlayerStateContainer.Instance.TeamSelection = PlayerStateContainer.EXPLORER;
            Connect();
        }

        public void ConnectAsNightmare() {
            PlayerStateContainer.Instance.TeamSelection = PlayerStateContainer.NIGHTMARE;
            Connect();
        }

        public void ConnectAsObserver() {
            PlayerStateContainer.Instance.TeamSelection = PlayerStateContainer.OBSERVER;
            Connect();
        }

        private void Connect() {
            isConnecting = true;
            CloseAllPanels();
            progressLabel.SetActive(true);
            if (!PhotonNetwork.connected) {
                PhotonNetwork.ConnectUsingSettings(Constants.GAME_VERSION);
            } else {
                JoinLobby();
            }
        }

        public override void OnConnectedToMaster() {
            if (isConnecting) {
                Debug.Log("Successfully connected to master. Joining random room.");
                JoinLobby();
            }
        }

        private void JoinLobby() {
            PhotonNetwork.JoinLobby(new TypedLobby(Constants.LOBBY_NAME, LobbyType.SqlLobby));
        }

        public override void OnJoinedLobby() {
            if (PlayerStateContainer.Instance.TeamSelection == PlayerStateContainer.EXPLORER) {
                string filter = "C0 = 1";
                PhotonNetwork.JoinRandomRoom(null, 0, MatchmakingMode.FillRoom, new TypedLobby(Constants.LOBBY_NAME, LobbyType.SqlLobby), filter);
            } else if (PlayerStateContainer.Instance.TeamSelection == PlayerStateContainer.NIGHTMARE) {
                string filter = "C1 = 1";
                PhotonNetwork.JoinRandomRoom(null, 0, MatchmakingMode.FillRoom, new TypedLobby(Constants.LOBBY_NAME, LobbyType.SqlLobby), filter);
            } else {
                PhotonNetwork.JoinRandomRoom();
            }
        }

        public override void OnDisconnectedFromPhoton() {
            Debug.Log("Disconnected from Photon.");
            connectPanel.SetActive(true);
            progressLabel.SetActive(false);
        }

        public override void OnPhotonRandomJoinFailed(object[] codeAndMsg) {
            Debug.Log("Random join failed. Creating new room.");
            RoomOptions options = new RoomOptions();
            options.IsOpen = true;
            options.IsVisible = true;
            options.MaxPlayers = 0;
            options.CustomRoomPropertiesForLobby = new string[]{ "C0", "C1" };
            options.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable(){{ "C0", 1 }, { "C1", 1 }};
            PhotonNetwork.CreateRoom(null, options, new TypedLobby(Constants.LOBBY_NAME, LobbyType.SqlLobby));
        }

        public override void OnJoinedRoom() {
            Debug.Log("This client is now in a room.");
            if (PhotonNetwork.room.PlayerCount == 1) {
                PhotonNetwork.LoadLevel("LobbyScene");
            }
        }
    }
}
