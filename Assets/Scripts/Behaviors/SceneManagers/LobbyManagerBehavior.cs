using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Com.Tempest.Nightmare {

    public class LobbyManagerBehavior : Photon.PunBehaviour {

        public Text pingDisplay;
        public Button startGameButton;
        public Dropdown typeSelect;
        public VerticalLayoutGroup playerListContent;
        public Text textPrefab;

        private float lastListRefresh;

        // Use this for initialization
        void Start() {
            if (PhotonNetwork.isMasterClient) {
                PhotonNetwork.room.IsOpen = true;
            }
        }

        private void Update() {
            pingDisplay.text = string.Format("Ping: {0:D4} ms", PhotonNetwork.GetPing());
            RefreshPlayerList();
        }

        public void StartGame() {
            if (PhotonNetwork.isMasterClient) {
                PhotonNetwork.LoadLevel("GameScene");
            }
        }

        public void OnSelectionChanged() {
            int typeSelection = typeSelect.value;
            switch (typeSelection) {
                case 0:
                    PhotonNetwork.player.SetTeam(PunTeams.Team.none);
                    break;
                case 1:
                    PhotonNetwork.player.SetTeam(PunTeams.Team.blue);
                    break;
                case 2:
                    PhotonNetwork.player.SetTeam(PunTeams.Team.red);
                    break;
            }
            lastListRefresh = 0f;
            GlobalPlayerContainer.Instance.PlayerTeam = PhotonNetwork.player.GetTeam();
        }

        public void LeaveRoom() {
            PhotonNetwork.LeaveRoom();
        }

        public void HandleMasterClientStuff() {
            if (startGameButton != null) {
                startGameButton.enabled = PhotonNetwork.isMasterClient;
            }
        }

        public void RefreshPlayerList() {
            if (Time.time - lastListRefresh < 1f) return;
            OnSelectionChanged();
            PhotonPlayer[] playerList = PhotonNetwork.playerList;
            Text[] childrenTexts = playerListContent.GetComponentsInChildren<Text>();
            foreach (Text text in childrenTexts) {
                Destroy(text.gameObject);
            }
            foreach (PhotonPlayer player in playerList) {
                Text playerText = Instantiate(textPrefab) as Text;
                playerText.text = player.NickName;
                switch (player.GetTeam()) {
                    case PunTeams.Team.blue:
                        playerText.text += ": Nightmare";
                        break;
                    case PunTeams.Team.red:
                        playerText.text += ": Dreamer";
                        break;
                    default:
                        playerText.text += ": Observer";
                        break;
                }
                if (player.IsMasterClient) {
                    playerText.text += " (Host)";
                }
                playerText.gameObject.transform.SetParent(playerListContent.transform);
            }
            lastListRefresh = Time.time;
        }

        public override void OnLeftRoom() {
            SceneManager.LoadScene("LauncherScene");
        }

        private void OnPlayerConnected(NetworkPlayer player) {
            RefreshPlayerList();
            HandleMasterClientStuff();
        }

        private void OnPlayerDisconnected(NetworkPlayer player) {
            RefreshPlayerList();
            HandleMasterClientStuff();
        }
    }
}
