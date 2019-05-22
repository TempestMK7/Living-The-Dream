using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Com.Tempest.Nightmare {

	public class LobbyManagerBehavior : Photon.PunBehaviour {

		public float countDownDuration = 30f;
		public float countDownLockedDuration = 5f;

		public GameObject talentManager;

		public Text pingDisplay;
		public Text countdownDisplay;
		public Button readyButton;
		public VerticalLayoutGroup playerListContent;
		public Text textPrefab;
		public GameObject lobbySynchronizerPrefab;

		public GameObject explorerPanel;
		public GameObject nightmarePanel;
		public Text explorerSelectionText;
		public Text nightmareSelectionText;

		public Button nightmareButton;
		public Button explorerButton;

		public Button doubleJumpButton;
		public Button jetpackButton;
		public Button dashButton;
		public Button ghastButton;
		public Button cryoButton;
		public Button goblinButton;

		public Image nightmareImage;
		public Image explorerHairImage;
		public Image explorerChestImage;
		public Image explorerEyeImage;

		public Sprite ghastSprite;
		public Sprite cryoSprite;
		public Sprite goblinSprite;

		public Sprite doubleJumpHair;
		public Sprite doubleJumpChest;
		public Sprite doubleJumpEye;

		public Sprite jetpackHair;
		public Sprite jetpackChest;
		public Sprite jetpackEye;

		public Sprite dashHair;
		public Sprite dashChest;
		public Sprite dashEye;

		private TalentManagerBehavior talentBehavior;
		private LobbySynchronizerBehavior synchronizerBehavior;

		private float lastListRefresh;
		private int numExplorers;
		private int numNightmares;
		private bool allPlayersReady;
		private float countDownStartTime;
		private bool countingDown;

		#region Intitialization.

		public void Awake() {
			talentBehavior = talentManager.GetComponent<TalentManagerBehavior>();
			if (PhotonNetwork.isMasterClient) {
				PhotonNetwork.room.IsOpen = true;
			}
			PlayerStateContainer.Instance.IsReady = PlayerStateContainer.STATUS_NOT_READY;
			PlayerStateContainer.Instance.ExplorerSelection = PlayerStateContainer.DOUBLE_JUMP_EXPLORER;
			PlayerStateContainer.Instance.NightmareSelection = PlayerStateContainer.GHAST;
			InitializePlayerStateWithPhoton();
			CreateSynchronizer();
			RefreshPlayerInformation(false);
			HandlePanels();
		}

		public void InitializePlayerStateWithPhoton() {
			PhotonPlayer player = PhotonNetwork.player;
			ExitGames.Client.Photon.Hashtable properties = new ExitGames.Client.Photon.Hashtable();
			properties[PlayerStateContainer.TEAM_SELECTION] = PlayerStateContainer.Instance.TeamSelection;
			properties[PlayerStateContainer.IS_READY] = PlayerStateContainer.Instance.IsReady;
			player.SetCustomProperties(properties);
		}

		public void CreateSynchronizer() {
			synchronizerBehavior = PhotonNetwork.Instantiate(lobbySynchronizerPrefab.name, new Vector3(), Quaternion.identity, 0)
					.GetComponent<LobbySynchronizerBehavior>();
		}

		#endregion

		#region Button Callbacks.

		public void HandlePanels() {
			explorerPanel.SetActive(PlayerStateContainer.Instance.TeamSelection == PlayerStateContainer.EXPLORER);
			nightmarePanel.SetActive(PlayerStateContainer.Instance.TeamSelection == PlayerStateContainer.NIGHTMARE);

			doubleJumpButton.gameObject.SetActive(talentBehavior.GetTalentLevel("Double Jump Explorer") != 0);
			jetpackButton.gameObject.SetActive(talentBehavior.GetTalentLevel("Jetpack Explorer") != 0);
			dashButton.gameObject.SetActive(talentBehavior.GetTalentLevel("Dash Explorer") != 0);

			switch (PlayerStateContainer.Instance.ExplorerSelection) {
				case PlayerStateContainer.DOUBLE_JUMP_EXPLORER:
					explorerSelectionText.text = "Double Jump";
					explorerHairImage.sprite = doubleJumpHair;
					explorerChestImage.sprite = doubleJumpChest;
					explorerEyeImage.sprite = doubleJumpEye;
					break;
				case PlayerStateContainer.JETPACK_EXPLORER:
					explorerSelectionText.text = "Jetpack";
					explorerHairImage.sprite = jetpackHair;
					explorerChestImage.sprite = jetpackChest;
					explorerEyeImage.sprite = jetpackEye;
					break;
				case PlayerStateContainer.DASH_EXPLORER:
					explorerSelectionText.text = "Dash";
					explorerHairImage.sprite = dashHair;
					explorerChestImage.sprite = dashChest;
					explorerEyeImage.sprite = dashEye;
					break;
			}

			ghastButton.gameObject.SetActive(talentBehavior.GetTalentLevel("Ghast Nightmare") != 0);
			cryoButton.gameObject.SetActive(talentBehavior.GetTalentLevel("Cryo Nightmare") != 0);
			goblinButton.gameObject.SetActive(talentBehavior.GetTalentLevel("Goblin Nightmare") != 0);

			switch (PlayerStateContainer.Instance.NightmareSelection) {
				case PlayerStateContainer.GHAST:
					nightmareSelectionText.text = "Ghast";
					nightmareImage.sprite = ghastSprite;
					break;
				case PlayerStateContainer.CRYO:
					nightmareSelectionText.text = "Cryo";
					nightmareImage.sprite = cryoSprite;
					break;
				case PlayerStateContainer.GOBLIN:
					nightmareSelectionText.text = "Goblin";
					nightmareImage.sprite = goblinSprite;
					break;
			}
		}

		public void SelectExplorer() {
			if (numExplorers == Constants.MAX_EXPLORERS) return;
			PlayerStateContainer.Instance.TeamSelection = PlayerStateContainer.EXPLORER;
			ResendPlayerInfoIfWrong();
			synchronizerBehavior.NotifyTeamChange();
			HandlePanels();
		}

		public void SelectNightmare() {
			if (numNightmares == Constants.MAX_NIGHTMARES) return;
			PlayerStateContainer.Instance.TeamSelection = PlayerStateContainer.NIGHTMARE;
			ResendPlayerInfoIfWrong();
			synchronizerBehavior.NotifyTeamChange();
			HandlePanels();
		}

		public void SelectObserver() {
			PlayerStateContainer.Instance.TeamSelection = PlayerStateContainer.OBSERVER;
			ResendPlayerInfoIfWrong();
			synchronizerBehavior.NotifyTeamChange();
			HandlePanels();
		}

		public void SelectDoubleJump() {
			PlayerStateContainer.Instance.ExplorerSelection = PlayerStateContainer.DOUBLE_JUMP_EXPLORER;
			HandlePanels();
		}

		public void SelectJetpack() {
			PlayerStateContainer.Instance.ExplorerSelection = PlayerStateContainer.JETPACK_EXPLORER;
			HandlePanels();
		}

		public void SelectDash() {
			PlayerStateContainer.Instance.ExplorerSelection = PlayerStateContainer.DASH_EXPLORER;
			HandlePanels();
		}

		public void SelectGhast() {
			PlayerStateContainer.Instance.NightmareSelection = PlayerStateContainer.GHAST;
			HandlePanels();
		}

		public void SelectCryo() {
			PlayerStateContainer.Instance.NightmareSelection = PlayerStateContainer.CRYO;
			HandlePanels();
		}

		public void SelectGoblin() {
			PlayerStateContainer.Instance.NightmareSelection = PlayerStateContainer.GOBLIN;
			HandlePanels();
		}

		#endregion

		#region Room Constraints And Player Updates.

		public void Update() {
			pingDisplay.text = string.Format("Ping: {0:D4} ms", PhotonNetwork.GetPing());
			readyButton.GetComponentInChildren<Text>().text = PlayerStateContainer.Instance.IsReady.Equals(PlayerStateContainer.STATUS_READY) ? "Unready" : "Ready";
			ResendPlayerInfoIfWrong();
			RefreshPlayerInformation(true);
			HandleCountDown();
		}

		public void ResendPlayerInfoIfWrong() {
			PhotonPlayer player = PhotonNetwork.player;
			if ((int)player.CustomProperties[PlayerStateContainer.TEAM_SELECTION] != PlayerStateContainer.Instance.TeamSelection ||
			    	!PlayerStateContainer.Instance.IsReady.Equals(player.CustomProperties[PlayerStateContainer.IS_READY])) {
				InitializePlayerStateWithPhoton();
			}
		}

		public void RefreshPlayerInformation(bool checkTimer) {
			if (checkTimer && Time.time - lastListRefresh < 1f)
				return;
			CheckPlayerCounts();
			UpdateRoomConstraints();
			lastListRefresh = Time.time;
		}

		public void CheckPlayerCounts() {
			PhotonPlayer[] playerList = PhotonNetwork.playerList;
			Text[] childrenTexts = playerListContent.GetComponentsInChildren<Text>();
			foreach (Text text in childrenTexts) {
				Destroy(text.gameObject);
			}
			allPlayersReady = true;
			numExplorers = 0;
			numNightmares = 0;

			ArrayList explorers = new ArrayList();
			ArrayList nightmares = new ArrayList();
			ArrayList observers = new ArrayList();
			foreach (PhotonPlayer player in playerList) {
				if (!player.CustomProperties.ContainsKey(PlayerStateContainer.IS_READY) ||
				 		!player.CustomProperties.ContainsKey(PlayerStateContainer.TEAM_SELECTION)) {
					continue;
				}
				int teamSelection = (int)player.CustomProperties[PlayerStateContainer.TEAM_SELECTION];
				switch (teamSelection) {
					case PlayerStateContainer.NIGHTMARE:
						nightmares.Add(player);
						numNightmares++;
						break;
					case PlayerStateContainer.EXPLORER:
						explorers.Add(player);
						numExplorers++;
						break;
					case PlayerStateContainer.OBSERVER:
						observers.Add(player);
						break;
				}
				string readyStatus = player.CustomProperties[PlayerStateContainer.IS_READY].ToString();
				// If the player is an observer, ignore their ready status unless the room is private.
				// Private rooms should wait for all players to be ready, public rooms should not.
				if ((teamSelection != PlayerStateContainer.OBSERVER || !PhotonNetwork.room.IsVisible) &&
						!PlayerStateContainer.STATUS_READY.Equals(readyStatus)) {
					allPlayersReady = false;
				}
			}

			Text nightmareLabel = Instantiate(textPrefab) as Text;
			nightmareLabel.fontSize += 2;
			nightmareLabel.text = "Nightmares (" + nightmares.Count + " / " + Constants.MAX_NIGHTMARES + ")\n";
			nightmareLabel.gameObject.transform.SetParent(playerListContent.transform);
			foreach (PhotonPlayer player in nightmares) {
				Text playerText = Instantiate(textPrefab) as Text;
				string readyStatus = player.CustomProperties[PlayerStateContainer.IS_READY].ToString();
				int teamSelection = (int)player.CustomProperties[PlayerStateContainer.TEAM_SELECTION];
				playerText.text = "(" + readyStatus + ") " + player.NickName;
				playerText.gameObject.transform.SetParent(playerListContent.transform);
			}

			Text explorerLabel = Instantiate(textPrefab) as Text;
			explorerLabel.fontSize += 2;
			explorerLabel.text = "\nExplorers (" + explorers.Count + " / " + Constants.MAX_EXPLORERS + ")\n";
			explorerLabel.gameObject.transform.SetParent(playerListContent.transform);
			foreach (PhotonPlayer player in explorers) {
				Text playerText = Instantiate(textPrefab) as Text;
				string readyStatus = player.CustomProperties[PlayerStateContainer.IS_READY].ToString();
				int teamSelection = (int)player.CustomProperties[PlayerStateContainer.TEAM_SELECTION];
				playerText.text = "(" + readyStatus + ") " + player.NickName;
				playerText.gameObject.transform.SetParent(playerListContent.transform);
			}

			Text observerLabel = Instantiate(textPrefab) as Text;
			observerLabel.fontSize += 2;
			observerLabel.text = "\nObservers (" + observers.Count + ")\n";
			observerLabel.gameObject.transform.SetParent(playerListContent.transform);
			foreach (PhotonPlayer player in observers) {
				Text playerText = Instantiate(textPrefab) as Text;
				string readyStatus = player.CustomProperties[PlayerStateContainer.IS_READY].ToString();
				int teamSelection = (int)player.CustomProperties[PlayerStateContainer.TEAM_SELECTION];
				playerText.text = "(" + readyStatus + ") " + player.NickName;
				playerText.gameObject.transform.SetParent(playerListContent.transform);
			}

			if (numNightmares > Constants.MAX_NIGHTMARES && PlayerStateContainer.Instance.TeamSelection == PlayerStateContainer.NIGHTMARE) {
				BootLastNightmare();
			} else if (numExplorers > Constants.MAX_EXPLORERS && PlayerStateContainer.Instance.TeamSelection == PlayerStateContainer.EXPLORER) {
				BootLastExplorer();
			}
			
			if ((!PhotonNetwork.room.IsVisible && allPlayersReady) || (numExplorers == Constants.MAX_EXPLORERS && numNightmares == Constants.MAX_NIGHTMARES)) {
				StartCountDown();
			} else {
				StopCountDown();
			}
		}

		public void ResetSelection() {
			PlayerStateContainer.Instance.IsReady = PlayerStateContainer.STATUS_NOT_READY;
			SelectObserver();
		}

		private void BootLastExplorer() {
			if (!PhotonNetwork.isMasterClient) return;
			PhotonPlayer[] playerList = PhotonNetwork.playerList;
			for (int i = playerList.Length - 1; i >= 0; i--) {
				PhotonPlayer player = playerList[i];
				if (!player.CustomProperties.ContainsKey(PlayerStateContainer.IS_READY) ||
				 		!player.CustomProperties.ContainsKey(PlayerStateContainer.TEAM_SELECTION)) {
					continue;
				}
				int teamSelection = (int)player.CustomProperties[PlayerStateContainer.TEAM_SELECTION];
				if (teamSelection == PlayerStateContainer.EXPLORER) {
					synchronizerBehavior.ResetTeamSelectionForPlayer(player.UserId);
					return;
				}
			}
		}

		private void BootLastNightmare() {
			if (!PhotonNetwork.isMasterClient) return;
			PhotonPlayer[] playerList = PhotonNetwork.playerList;
			for (int i = playerList.Length - 1; i >= 0; i--) {
				PhotonPlayer player = playerList[i];
				if (!player.CustomProperties.ContainsKey(PlayerStateContainer.IS_READY) ||
				 		!player.CustomProperties.ContainsKey(PlayerStateContainer.TEAM_SELECTION)) {
					continue;
				}
				int teamSelection = (int)player.CustomProperties[PlayerStateContainer.TEAM_SELECTION];
				if (teamSelection == PlayerStateContainer.NIGHTMARE) {
					synchronizerBehavior.ResetTeamSelectionForPlayer(player.UserId);
					return;
				}
			}
		}

		private void UpdateRoomConstraints() {
			if (PhotonNetwork.isMasterClient) {
				int needsExplorers = numExplorers < Constants.MAX_EXPLORERS ? 1 : 0;
				int needsNightmares = numNightmares < Constants.MAX_NIGHTMARES ? 1 : 0;
				PhotonNetwork.room.SetPropertiesListedInLobby(new string[]{ "C0", "C1"});
				PhotonNetwork.room.SetCustomProperties(new ExitGames.Client.Photon.Hashtable(){{"C0", needsExplorers}, {"C1", needsNightmares}});
			}

			if (numNightmares >= Constants.MAX_NIGHTMARES) {
				nightmareButton.GetComponentInChildren<Text>().text = "Play Nightmare (Full)";
			} else {
				nightmareButton.GetComponentInChildren<Text>().text = "Play Nightmare";
			}

			if (numExplorers >= Constants.MAX_EXPLORERS) {
				explorerButton.GetComponentInChildren<Text>().text = "Play Explorer (Full)";
			} else {
				explorerButton.GetComponentInChildren<Text>().text = "Play Explorer";
			}
		}

		private void StartCountDown() {
			if (!countingDown) {
				countingDown = true;
				countDownStartTime = Time.time;
			}
		}

		private void StopCountDown() {
			countingDown = false;
			countDownStartTime = 0f;
		}

		private void HandleCountDown() {
			if (!countingDown) {
				countdownDisplay.text = "Waiting For Players";
			} else {
				float countDownElapsed = Time.time - countDownStartTime;
				float countDownRemaining = (allPlayersReady ? countDownLockedDuration : countDownDuration) - countDownElapsed;
				countDownRemaining = Mathf.Max(countDownRemaining, 0f);
				if (countDownRemaining == 0f) {
					countdownDisplay.text = "Loading Game...";
					StartGame();
				} else {
					countdownDisplay.text = "Starting in " + Mathf.RoundToInt(countDownRemaining + 0.5f);
				}
			}
		}

		private void StartGame() {
			if (PhotonNetwork.isMasterClient) {
				photonView.RPC("StopMusic", PhotonTargets.All);
				PhotonNetwork.room.IsOpen = false;
				PhotonNetwork.LoadLevel("GeneratedGameScene");
			}
		}

		[PunRPC]
		public void StopMusic() {
			FindObjectOfType<LobbyMusicBehavior>().StopMusic();
		}

		public void ToggleReady() {
			if (PlayerStateContainer.STATUS_READY.Equals(PlayerStateContainer.Instance.IsReady)) {
				PlayerStateContainer.Instance.IsReady = PlayerStateContainer.STATUS_NOT_READY;
			} else {
				PlayerStateContainer.Instance.IsReady = PlayerStateContainer.STATUS_READY;
			}
			ResendPlayerInfoIfWrong();
			synchronizerBehavior.NotifyReadyChange();
		}

		public void LeaveRoom() {
			PhotonNetwork.LeaveRoom();
		}

		public override void OnLeftRoom() {
			SceneManager.LoadScene("LauncherScene");
		}
		
		public override void OnPhotonPlayerConnected(PhotonPlayer player) {
			base.OnPhotonPlayerConnected(player);
			RefreshPlayerInformation(false);
		}

		public override void OnPhotonPlayerDisconnected(PhotonPlayer otherPlayer) {
			base.OnPhotonPlayerDisconnected(otherPlayer);
			RefreshPlayerInformation(false);
		}

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
            
        }

		#endregion
	}
}
