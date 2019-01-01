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
			PlayerStateContainer.Instance.IsReady = PlayerStateContainer.STATUS_NOT_READY;
			InitializePlayerStateWithPhoton();
			InitializePlayerSelections();
			HandlePanels();
		}

		public void InitializePlayerStateWithPhoton() {
			PhotonPlayer player = PhotonNetwork.player;
			ExitGames.Client.Photon.Hashtable properties = new ExitGames.Client.Photon.Hashtable();
			teamDropdown.value = PlayerStateContainer.Instance.TeamSelection;
			properties[PlayerStateContainer.TEAM_SELECTION] = PlayerStateContainer.Instance.TeamSelection;
			properties[PlayerStateContainer.IS_READY] = PlayerStateContainer.Instance.IsReady;
			player.SetCustomProperties(properties);
		}

		public void InitializePlayerSelections() {
			PlayerStateContainer.Instance.ExplorerSelection = PlayerStateContainer.DOUBLE_JUMP_EXPLORER;
			PlayerStateContainer.Instance.NightmareSelection = PlayerStateContainer.GHAST;
		}

		public void HandlePanels() {
			explorerPanel.SetActive(PlayerStateContainer.Instance.TeamSelection == PlayerStateContainer.EXPLORER);
			nightmarePanel.SetActive(PlayerStateContainer.Instance.TeamSelection == PlayerStateContainer.NIGHTMARE);
		}

		private void Update() {
			pingDisplay.text = string.Format("Ping: {0:D4} ms", PhotonNetwork.GetPing());
			ResendPlayerInfoIfWrong();
			RefreshPlayerList();
			UpdateRoomCounts();
		}

		public void ResendPlayerInfoIfWrong() {
			PhotonPlayer player = PhotonNetwork.player;
			if ((int)player.CustomProperties[PlayerStateContainer.TEAM_SELECTION] != PlayerStateContainer.Instance.TeamSelection ||
			    	PlayerStateContainer.Instance.IsReady.Equals(player.CustomProperties[PlayerStateContainer.IS_READY])) {
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
				if (!player.CustomProperties.ContainsKey(PlayerStateContainer.IS_READY) ||
				 		!player.CustomProperties.ContainsKey(PlayerStateContainer.TEAM_SELECTION)) {
					continue;
				}
				string readyStatus = player.CustomProperties[PlayerStateContainer.IS_READY].ToString();
				int teamSelection = (int)player.CustomProperties[PlayerStateContainer.TEAM_SELECTION];
				playerText.text = "(" + readyStatus + ") " + player.NickName;
				switch (teamSelection) {
				case PlayerStateContainer.NIGHTMARE:
					playerText.text += ": Nightmare";
					numNightmares++;
					break;
				case PlayerStateContainer.EXPLORER:
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
				if (!PlayerStateContainer.STATUS_READY.Equals(readyStatus)) {
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
				int needsExplorers = numExplorers < Constants.MAX_EXPLORERS ? 1 : 0;
				int needsNightmares = numNightmares < Constants.MAX_NIGHTMARES ? 1 : 0;
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
			if (PlayerStateContainer.STATUS_READY.Equals(PlayerStateContainer.Instance.IsReady)) {
				PlayerStateContainer.Instance.IsReady = PlayerStateContainer.STATUS_NOT_READY;
			} else {
				PlayerStateContainer.Instance.IsReady = PlayerStateContainer.STATUS_READY;
			}
			ExitGames.Client.Photon.Hashtable properties = new ExitGames.Client.Photon.Hashtable();
			properties[PlayerStateContainer.IS_READY] = PlayerStateContainer.Instance.IsReady;
			PhotonNetwork.player.SetCustomProperties(properties);
		}

		public void OnTeamSelectChanged() {
			int teamChoice = teamDropdown.value;
			if (teamChoice != PlayerStateContainer.Instance.TeamSelection) {
				if ((teamChoice == PlayerStateContainer.NIGHTMARE && numNightmares == Constants.MAX_NIGHTMARES) || 
						(teamChoice == PlayerStateContainer.EXPLORER && numExplorers == Constants.MAX_EXPLORERS)) {
					teamChoice = PlayerStateContainer.OBSERVER;
					PlayerStateContainer.Instance.TeamSelection = teamChoice;
					teamDropdown.value = teamChoice;
				} else {
					PlayerStateContainer.Instance.TeamSelection = teamChoice;
				}
				HandlePanels();
			}
		}

		public void OnCharacterSelectChanged() {
			int explorerChoice = explorerSelect.value;
			int nightmareChoice = nightmareSelect.value;
			PlayerStateContainer.Instance.ExplorerSelection = explorerChoice;
			PlayerStateContainer.Instance.NightmareSelection = nightmareChoice;
		}

		public void LeaveRoom() {
			PhotonNetwork.LeaveRoom();
		}

		public override void OnLeftRoom() {
			SceneManager.LoadScene("LauncherScene");
		}
	}
}
