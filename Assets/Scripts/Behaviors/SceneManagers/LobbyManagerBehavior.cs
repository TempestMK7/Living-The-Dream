using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Com.Tempest.Nightmare {

	public class LobbyManagerBehavior : Photon.PunBehaviour {

		public Text pingDisplay;
		public Button readyButton;
		public GameObject explorerPanel;
		public Dropdown explorerSelect;
		public GameObject nightmarePanel;
		public Dropdown nightmareSelect;
		public VerticalLayoutGroup playerListContent;
		public Text textPrefab;

		private float lastListRefresh;

		void Start() {
			if (PhotonNetwork.isMasterClient) {
				PhotonNetwork.room.IsOpen = true;
			}
			GlobalPlayerContainer.Instance.IsReady = 
                GlobalPlayerContainer.Instance.TeamSelection == GlobalPlayerContainer.OBSERVER ? 
                    GlobalPlayerContainer.STATUS_READY : GlobalPlayerContainer.STATUS_NOT_READY;
			InitializePlayerStateWithPhoton();
			InitializePlayerSelections();
			HandlePanels();
		}

		public void InitializePlayerStateWithPhoton() {
			PhotonPlayer player = PhotonNetwork.player;
			ExitGames.Client.Photon.Hashtable properties = new ExitGames.Client.Photon.Hashtable();
			properties[GlobalPlayerContainer.TEAM_SELECTION] = GlobalPlayerContainer.Instance.TeamSelection;
			properties[GlobalPlayerContainer.IS_READY] = GlobalPlayerContainer.Instance.IsReady;
			player.SetCustomProperties(properties);
		}

		public void InitializePlayerSelections() {
			GlobalPlayerContainer.Instance.ExplorerSelection = GlobalPlayerContainer.DOUBLE_JUMP_EXPLORER;
			GlobalPlayerContainer.Instance.NightmareSelection = GlobalPlayerContainer.GHAST;
		}

		public void HandlePanels() {
			explorerPanel.SetActive(GlobalPlayerContainer.Instance.TeamSelection == GlobalPlayerContainer.EXPLORER);
			nightmarePanel.SetActive(GlobalPlayerContainer.Instance.TeamSelection == GlobalPlayerContainer.NIGHTMARE);
			readyButton.gameObject.SetActive(GlobalPlayerContainer.Instance.TeamSelection != GlobalPlayerContainer.OBSERVER);
		}

		private void Update() {
			pingDisplay.text = string.Format("Ping: {0:D4} ms", PhotonNetwork.GetPing());
			ResendPlayerInfoIfWrong();
			RefreshPlayerList();
		}

		public void ResendPlayerInfoIfWrong() {
			PhotonPlayer player = PhotonNetwork.player;
			if ((int)player.CustomProperties[GlobalPlayerContainer.TEAM_SELECTION] != GlobalPlayerContainer.Instance.TeamSelection ||
			    	GlobalPlayerContainer.Instance.IsReady.Equals(player.CustomProperties[GlobalPlayerContainer.IS_READY])) {
				InitializePlayerStateWithPhoton();
			}
		}

		public void RefreshPlayerList() {
			if (Time.time - lastListRefresh < 1f)
				return;
			PhotonPlayer[] playerList = PhotonNetwork.playerList;
			Text[] childrenTexts = playerListContent.GetComponentsInChildren<Text>();
			foreach (Text text in childrenTexts) {
				Destroy(text.gameObject);
			}
			bool allPlayersReady = true;
			foreach (PhotonPlayer player in playerList) {
				Text playerText = Instantiate(textPrefab) as Text;
				if (!player.CustomProperties.ContainsKey(GlobalPlayerContainer.IS_READY) ||
				 		!player.CustomProperties.ContainsKey(GlobalPlayerContainer.TEAM_SELECTION)) {
					continue;
				}
				string readyStatus = player.CustomProperties[GlobalPlayerContainer.IS_READY].ToString();
				int teamSelection = (int)player.CustomProperties[GlobalPlayerContainer.TEAM_SELECTION];
				playerText.text = "(" + readyStatus + ") " + player.NickName;
				switch (teamSelection) {
				case GlobalPlayerContainer.NIGHTMARE:
					playerText.text += ": Nightmare";
					break;
				case GlobalPlayerContainer.EXPLORER:
					playerText.text += ": Explorer";
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
				PhotonNetwork.LoadLevel("GeneratedGameScene");
			}
		}

		public void ToggleReady() {
			if (GlobalPlayerContainer.STATUS_READY.Equals(GlobalPlayerContainer.Instance.IsReady)) {
				GlobalPlayerContainer.Instance.IsReady = GlobalPlayerContainer.STATUS_NOT_READY;
			} else {
				GlobalPlayerContainer.Instance.IsReady = GlobalPlayerContainer.STATUS_READY;
			}
			ExitGames.Client.Photon.Hashtable properties = new ExitGames.Client.Photon.Hashtable();
			properties[GlobalPlayerContainer.IS_READY] = GlobalPlayerContainer.Instance.IsReady;
			PhotonNetwork.player.SetCustomProperties(properties);
		}

		public void OnCharacterSelectChanged() {
			int explorerChoice = explorerSelect.value;
			int nightmareChoice = nightmareSelect.value;
			GlobalPlayerContainer.Instance.ExplorerSelection = explorerChoice;
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
