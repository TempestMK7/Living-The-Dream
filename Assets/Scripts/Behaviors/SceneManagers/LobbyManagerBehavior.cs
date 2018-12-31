using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Com.Tempest.Nightmare {

	public class LobbyManagerBehavior : Photon.PunBehaviour {

		public Text pingDisplay;
		public Button readyButton;
		public Dropdown teamDropdown;
		public GameObject explorerPanel;
		public Dropdown explorerSelect;
		public GameObject nightmarePanel;
		public Dropdown nightmareSelect;
		public VerticalLayoutGroup playerListContent;
		public Text textPrefab;

		private float lastListRefresh;
		private int numExplorers;
		private int numNightmares;

		void Start() {
			if (PhotonNetwork.isMasterClient) {
				PhotonNetwork.room.IsOpen = true;
			}
			GlobalPlayerContainer.Instance.IsReady = GlobalPlayerContainer.STATUS_NOT_READY;
			InitializePlayerStateWithPhoton();
			InitializePlayerSelections();
			HandlePanels();
		}

		public void InitializePlayerStateWithPhoton() {
			PhotonPlayer player = PhotonNetwork.player;
			ExitGames.Client.Photon.Hashtable properties = new ExitGames.Client.Photon.Hashtable();
			teamDropdown.value = GlobalPlayerContainer.Instance.TeamSelection;
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
		}

		private void Update() {
			pingDisplay.text = string.Format("Ping: {0:D4} ms", PhotonNetwork.GetPing());
			ResendPlayerInfoIfWrong();
			RefreshPlayerList();
			UpdateRoomCounts();
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
			numExplorers = 0;
			numNightmares = 0;
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
					numNightmares++;
					break;
				case GlobalPlayerContainer.EXPLORER:
					playerText.text += ": Explorer";
					numExplorers++;
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

		private void UpdateRoomCounts() {
			if (PhotonNetwork.isMasterClient) {
				int needsExplorers = numExplorers < GlobalPlayerContainer.MAX_EXPLORERS ? 1 : 0;
				int needsNightmares = numNightmares < GlobalPlayerContainer.MAX_NIGHTMARES ? 1 : 0;
				PhotonNetwork.room.SetPropertiesListedInLobby(new string[]{ "C0", "C1"});
				PhotonNetwork.room.SetCustomProperties(new ExitGames.Client.Photon.Hashtable(){{"C0", needsExplorers}, {"C1", needsNightmares}});
			}
		}

		private void StartGame() {
			if (PhotonNetwork.isMasterClient) {
				PhotonNetwork.room.IsOpen = false;
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

		public void OnTeamSelectChanged() {
			int teamChoice = teamDropdown.value;
			if (teamChoice != GlobalPlayerContainer.Instance.TeamSelection) {
				if ((teamChoice == GlobalPlayerContainer.NIGHTMARE && numNightmares == GlobalPlayerContainer.MAX_NIGHTMARES) || 
						(teamChoice == GlobalPlayerContainer.EXPLORER && numExplorers == GlobalPlayerContainer.MAX_EXPLORERS)) {
					teamChoice = GlobalPlayerContainer.OBSERVER;
					GlobalPlayerContainer.Instance.TeamSelection = teamChoice;
					teamDropdown.value = teamChoice;
				} else {
					GlobalPlayerContainer.Instance.TeamSelection = teamChoice;
				}
				HandlePanels();
			}
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
