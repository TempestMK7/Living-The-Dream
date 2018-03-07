using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Com.Tempest.Nightmare {

    public class LobbyManagerBehavior : Photon.PunBehaviour {

        public Text pingDisplay;
        public Button readyButton;
        public Dropdown dreamerSelect;
        public Dropdown nightmareSelect;
        public VerticalLayoutGroup playerListContent;
        public Text textPrefab;

        private float lastListRefresh;
        
        void Start() {
            if (PhotonNetwork.isMasterClient) {
                PhotonNetwork.room.IsOpen = true;
            }
            InitializePlayerStateWithPhoton();
            InitializePlayerSelections();
        }

        public void InitializePlayerStateWithPhoton() {
            PhotonPlayer player = PhotonNetwork.player;
            switch (GlobalPlayerContainer.Instance.TeamSelection) {
                case GlobalPlayerContainer.DREAMER:
                    player.SetTeam(PunTeams.Team.red);
                    break;
                case GlobalPlayerContainer.NIGHTMARE:
                    player.SetTeam(PunTeams.Team.blue);
                    break;
                default:
                    player.SetTeam(PunTeams.Team.none);
                    break;
            }
            ExitGames.Client.Photon.Hashtable properties = new ExitGames.Client.Photon.Hashtable();
            properties[GlobalPlayerContainer.IS_READY] = GlobalPlayerContainer.STATUS_NOT_READY;
            player.SetCustomProperties(properties);
        }

        public void InitializePlayerSelections() {
            GlobalPlayerContainer.Instance.DreamerSelection = GlobalPlayerContainer.DOUBLE_JUMP_DREAMER;
            GlobalPlayerContainer.Instance.NightmareSelection = GlobalPlayerContainer.GHAST;
        }

        private void Update() {
            pingDisplay.text = string.Format("Ping: {0:D4} ms", PhotonNetwork.GetPing());
            ResendPlayerInfoIfWrong();
            RefreshPlayerList();
        }

        public void ResendPlayerInfoIfWrong() {
            PhotonPlayer player = PhotonNetwork.player;
            int playerSelection;
            switch (player.GetTeam()) {
                case PunTeams.Team.red:
                    playerSelection = GlobalPlayerContainer.DREAMER;
                    break;
                case PunTeams.Team.blue:
                    playerSelection = GlobalPlayerContainer.NIGHTMARE;
                    break;
                default:
                    playerSelection = GlobalPlayerContainer.OBSERVER;
                    break;
            }
            if (playerSelection != GlobalPlayerContainer.Instance.TeamSelection) {
                InitializePlayerStateWithPhoton();
            }
        }

        public void RefreshPlayerList() {
            if (Time.time - lastListRefresh < 1f) return;
            PhotonPlayer[] playerList = PhotonNetwork.playerList;
            Text[] childrenTexts = playerListContent.GetComponentsInChildren<Text>();
            foreach (Text text in childrenTexts) {
                Destroy(text.gameObject);
            }
            bool allPlayersReady = true;
            foreach (PhotonPlayer player in playerList) {
                Text playerText = Instantiate(textPrefab) as Text;
                string readyStatus = player.CustomProperties[GlobalPlayerContainer.IS_READY].ToString();
                playerText.text = "(" + readyStatus + ") " + player.NickName;
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
                if (!GlobalPlayerContainer.STATUS_READY.Equals(readyStatus)) {
                    allPlayersReady = false;
                }
                playerText.gameObject.transform.SetParent(playerListContent.transform);
            }
            lastListRefresh = Time.time;
            if (allPlayersReady) {
                StartGame();
            }
        }

        private void StartGame() {
            if (PhotonNetwork.isMasterClient) {
                PhotonNetwork.LoadLevel("GameScene");
            }
        }

        public void ToggleReady() {
            string readyStatus = PhotonNetwork.player.CustomProperties[GlobalPlayerContainer.IS_READY].ToString();
            if (GlobalPlayerContainer.STATUS_READY.Equals(readyStatus)) {
                ExitGames.Client.Photon.Hashtable properties = new ExitGames.Client.Photon.Hashtable();
                properties[GlobalPlayerContainer.IS_READY] = GlobalPlayerContainer.STATUS_NOT_READY;
                PhotonNetwork.player.SetCustomProperties(properties);
            } else {
                ExitGames.Client.Photon.Hashtable properties = new ExitGames.Client.Photon.Hashtable();
                properties[GlobalPlayerContainer.IS_READY] = GlobalPlayerContainer.STATUS_READY;
                PhotonNetwork.player.SetCustomProperties(properties);
            }
        }

        public void OnCharacterSelectChanged() {
            Debug.Log("Selection was called.");
            int dreamerChoice = dreamerSelect.value;
            int nightmareChoice = nightmareSelect.value;
            GlobalPlayerContainer.Instance.DreamerSelection = dreamerChoice;
            GlobalPlayerContainer.Instance.NightmareSelection = nightmareChoice;
        }

        public void LeaveRoom() {
            PhotonNetwork.LeaveRoom();
        }

        public override void OnLeftRoom() {
            SceneManager.LoadScene("LauncherScene");
        }
    }
}
