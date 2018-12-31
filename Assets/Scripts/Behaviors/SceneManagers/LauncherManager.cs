using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Com.Tempest.Nightmare {

    public class LauncherManager : Photon.PunBehaviour {

        public PhotonLogLevel logLevel = PhotonLogLevel.Informational;
        public GameObject startPanel;
        public GameObject connectPanel;
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
            startPanel.SetActive(true);
            connectPanel.SetActive(false);
            progressLabel.SetActive(false);
            versionText.text = "Game Version: " + GlobalPlayerContainer.GAME_VERSION;
        }

        public void OpenConnectPanel() {
            startPanel.SetActive(false);
            connectPanel.SetActive(true);
        }

        public void CloseConnectPanel() {
            startPanel.SetActive(true);
            connectPanel.SetActive(false);
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
            connectPanel.SetActive(false);
            progressLabel.SetActive(true);
            if (!PhotonNetwork.connected) {
                PhotonNetwork.ConnectUsingSettings(GlobalPlayerContainer.GAME_VERSION);
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
            PhotonNetwork.JoinLobby(new TypedLobby(GlobalPlayerContainer.LOBBY_NAME, LobbyType.SqlLobby));
        }

        public override void OnJoinedLobby() {
            if (GlobalPlayerContainer.Instance.TeamSelection == GlobalPlayerContainer.EXPLORER) {
                string filter = "C0 = 1";
                PhotonNetwork.JoinRandomRoom(null, 0, MatchmakingMode.FillRoom, new TypedLobby(GlobalPlayerContainer.LOBBY_NAME, LobbyType.SqlLobby), filter);
            } else if (GlobalPlayerContainer.Instance.TeamSelection == GlobalPlayerContainer.NIGHTMARE) {
                string filter = "C1 = 1";
                PhotonNetwork.JoinRandomRoom(null, 0, MatchmakingMode.FillRoom, new TypedLobby(GlobalPlayerContainer.LOBBY_NAME, LobbyType.SqlLobby), filter);
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
            PhotonNetwork.CreateRoom(null, options, new TypedLobby(GlobalPlayerContainer.LOBBY_NAME, LobbyType.SqlLobby));
        }

        public override void OnJoinedRoom() {
            Debug.Log("This client is now in a room.");
            if (PhotonNetwork.room.PlayerCount == 1) {
                PhotonNetwork.LoadLevel("LobbyScene");
            }
        }
    }
}

