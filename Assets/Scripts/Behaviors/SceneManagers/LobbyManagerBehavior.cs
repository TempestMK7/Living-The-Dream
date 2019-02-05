using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Com.Tempest.Nightmare {

	public class LobbyManagerBehavior : Photon.PunBehaviour {

		public GameObject talentManager;

		public Text pingDisplay;
		public Button readyButton;
		public VerticalLayoutGroup playerListContent;
		public Text textPrefab;

		public GameObject explorerPanel;
		public GameObject nightmarePanel;
		public Text explorerSelectionText;
		public Text nightmareSelectionText;
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

		private float lastListRefresh;
		private int numExplorers;
		private int numNightmares;

		public void Awake() {
			talentBehavior = talentManager.GetComponent<TalentManagerBehavior>();
			if (PhotonNetwork.isMasterClient) {
				PhotonNetwork.room.IsOpen = true;
			}
			PlayerStateContainer.Instance.IsReady = PlayerStateContainer.STATUS_NOT_READY;
			PlayerStateContainer.Instance.ExplorerSelection = PlayerStateContainer.DOUBLE_JUMP_EXPLORER;
			PlayerStateContainer.Instance.NightmareSelection = PlayerStateContainer.GHAST;
			InitializePlayerStateWithPhoton();
			HandlePanels();
		}

		public void InitializePlayerStateWithPhoton() {
			PhotonPlayer player = PhotonNetwork.player;
			ExitGames.Client.Photon.Hashtable properties = new ExitGames.Client.Photon.Hashtable();
			properties[PlayerStateContainer.TEAM_SELECTION] = PlayerStateContainer.Instance.TeamSelection;
			properties[PlayerStateContainer.IS_READY] = PlayerStateContainer.Instance.IsReady;
			player.SetCustomProperties(properties);
		}

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
			HandlePanels();
		}

		public void SelectNightmare() {
			if (numNightmares == Constants.MAX_NIGHTMARES) return;
			PlayerStateContainer.Instance.TeamSelection = PlayerStateContainer.NIGHTMARE;
			HandlePanels();
		}

		public void SelectObserver() {
			PlayerStateContainer.Instance.TeamSelection = PlayerStateContainer.OBSERVER;
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

		private void Update() {
			pingDisplay.text = string.Format("Ping: {0:D4} ms", PhotonNetwork.GetPing());
			readyButton.GetComponentInChildren<Text>().text = PlayerStateContainer.Instance.IsReady.Equals(PlayerStateContainer.STATUS_READY) ? "Unready" : "Ready";
			ResendPlayerInfoIfWrong();
			RefreshPlayerList();
			UpdateRoomCounts();
		}

		public void ResendPlayerInfoIfWrong() {
			PhotonPlayer player = PhotonNetwork.player;
			if ((int)player.CustomProperties[PlayerStateContainer.TEAM_SELECTION] != PlayerStateContainer.Instance.TeamSelection ||
			    	!PlayerStateContainer.Instance.IsReady.Equals(player.CustomProperties[PlayerStateContainer.IS_READY])) {
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
				if (!PlayerStateContainer.STATUS_READY.Equals(readyStatus)) {
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
				if (player.IsMasterClient) {
					playerText.text += " (MC)";
				}
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
				if (player.IsMasterClient) {
					playerText.text += " (MC)";
				}
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
				if (player.IsMasterClient) {
					playerText.text += " (MC)";
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
			ExitGames.Client.Photon.Hashtable properties = new ExitGames.Client.Photon.Hashtable();
			properties[PlayerStateContainer.IS_READY] = PlayerStateContainer.Instance.IsReady;
			PhotonNetwork.player.SetCustomProperties(properties);
		}

		public void LeaveRoom() {
			PhotonNetwork.LeaveRoom();
		}

		public override void OnLeftRoom() {
			SceneManager.LoadScene("LauncherScene");
		}
	}
}
