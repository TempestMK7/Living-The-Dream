using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Tilemaps;

namespace Com.Tempest.Nightmare {

	public class GeneratedGameManager : Photon.PunBehaviour {
	
		public Camera maskCamera;
	
		// UI objects.
		public Text bonfireText;
		public Text dreamerText;

		public Tilemap borderMap;
		public TileBase ruleTile;
	
		// Prefabs.
		public GameObject ghastPrefab;
		public GameObject cryoPrefab;
		public GameObject doubleJumpPrefab;
		public GameObject jetpackPrefab;
		public GameObject lightBoxPrefab;

		public GameObject levelChunk01;
	
		// Game parameters.
		public int bonfiresAllowedIncomplete = 0;
		public int levelWidth = 8;
		public int levelHeight = 8;
	
		// Publicly accessible fields pertaining to game state.
		public BaseExplorerBehavior Explorer { get; set; }

		public BaseNightmareBehavior Nightmare { get; set; }

		public List<BonfireBehavior> Bonfires { get; set; }

		public List<ShrineBehavior> Shrines { get; set; }

		public List<BaseExplorerBehavior> Explorers { get; set; }

		public List<BaseNightmareBehavior> Nightmares { get; set; }

		private int playersConnected;
		private int levelsGenerated;

		public void Awake() {
			if (PhotonNetwork.isMasterClient) {
				PhotonNetwork.room.IsOpen = false;
			}
		}

		private void OnEnable() {
			SceneManager.sceneLoaded += LevelWasLoaded;
		}

		private void OnDisable() {
			SceneManager.sceneLoaded -= LevelWasLoaded;
		}

		private void LevelWasLoaded(Scene scene, LoadSceneMode mode) {
			photonView.RPC("NotifyMasterClientConnected", PhotonTargets.MasterClient);
		}

		[PunRPC]
		public void NotifyMasterClientConnected() {
			if (!PhotonNetwork.isMasterClient)
				return;
			playersConnected++;
			if (PhotonNetwork.playerList.Length == playersConnected) {
				photonView.RPC("GenerateLevel", PhotonTargets.All, levelWidth, levelHeight);
			}
		}

		[PunRPC]
		public void GenerateLevel(int levelWidth, int levelHeight) {
			GameObject[,] levelChunks = new GameObject[levelWidth, levelHeight];
			for (int x = 0; x < levelWidth; x++) {
				for (int y = 0; y < levelHeight; y++) {
					levelChunks[x, y] = Instantiate(levelChunk01, new Vector3(x * 16f, y * 16f), Quaternion.identity);
				}
			}
			GenerateBorder(levelWidth, levelHeight);
			photonView.RPC("NotifyLevelGenerated", PhotonTargets.MasterClient);
		}

		private void GenerateBorder(int levelWidth, int levelHeight) {
			int totalWidth = (levelWidth * 16) + 6; 
			int totalHeight = (levelHeight * 16) + 6; 
			GenerateHorizontalBorder(totalWidth, -3);
			GenerateHorizontalBorder(totalWidth, -2);
			GenerateHorizontalBorder(totalWidth, totalHeight - 4);
			GenerateHorizontalBorder(totalWidth, totalHeight - 5);
			GenerateVerticalBorder(-3, totalHeight);
			GenerateVerticalBorder(-2, totalHeight);
			GenerateVerticalBorder(totalWidth - 4, totalHeight);
			GenerateVerticalBorder(totalWidth - 5, totalHeight);
		}

		private void GenerateHorizontalBorder(int totalWidth, int rowIndex) {
			Vector3Int position = new Vector3Int(0, rowIndex, 0);
			for (int x = 0; x < totalWidth; x++) {
				position.x = x - 3;
				borderMap.SetTile(position, ruleTile);
			}
		}

		private void GenerateVerticalBorder(int rowIndex, int totalHeight) {
			Vector3Int position = new Vector3Int(rowIndex, 0, 0);
			for (int y = 0; y < totalHeight; y++) {
				position.y = y - 3;
				borderMap.SetTile(position, ruleTile);
			}
		}

		[PunRPC]
		public void NotifyLevelGenerated() {
			if (!PhotonNetwork.isMasterClient)
				return;
			levelsGenerated++;
			if (PhotonNetwork.playerList.Length == levelsGenerated) {
				photonView.RPC("InstantiateCharacter", PhotonTargets.All);
			}
		}

		[PunRPC]
		public void InstantiateCharacter() {
			GlobalPlayerContainer playerContainer = GlobalPlayerContainer.Instance;
			if (playerContainer.TeamSelection == GlobalPlayerContainer.EXPLORER) {
				switch (playerContainer.ExplorerSelection) {
				case GlobalPlayerContainer.DOUBLE_JUMP_EXPLORER:
					Explorer = PhotonNetwork.Instantiate(doubleJumpPrefab.name, new Vector3(0, 0), Quaternion.identity, 0)
						.GetComponent<BaseExplorerBehavior>();
					break;
				case GlobalPlayerContainer.JETPACK_EXPLORER:
					Explorer = PhotonNetwork.Instantiate(jetpackPrefab.name, new Vector3(0, 0), Quaternion.identity, 0)
						.GetComponent<BaseExplorerBehavior>();
					break;
				}
				if (Explorer != null) {
					Camera.main.transform.position = Explorer.transform.position;
				}
				ChangeMaskColor(0f);
			} else if (playerContainer.TeamSelection == GlobalPlayerContainer.NIGHTMARE) {
				switch (playerContainer.NightmareSelection) {
				case GlobalPlayerContainer.GHAST:
					Nightmare = PhotonNetwork.Instantiate(ghastPrefab.name, new Vector3(0f, 0f), Quaternion.identity, 0)
						.GetComponent<BaseNightmareBehavior>();
					break;
				case GlobalPlayerContainer.CRYO:
					Nightmare = PhotonNetwork.Instantiate(cryoPrefab.name, new Vector3(0f, 0f), Quaternion.identity, 0)
						.GetComponent<BaseNightmareBehavior>();
					break;
				}
				if (Nightmare != null) {
					Camera.main.transform.position = Nightmare.gameObject.transform.position;
				}
				ChangeMaskColor(0f);
			} else {
				ChangeMaskColor(0.5f);
			}
		}

		public void Update() {
			HandleBonfires();
			HandleShrines();
			HandlePlayers();
		}

		private void HandleBonfires() {
			if (Bonfires == null) {
				HashSet<GameObject> fireSet = PhotonNetwork.FindGameObjectsWithComponent(typeof(BonfireBehavior));
				if (fireSet.Count != 0) {
					Bonfires = new List<BonfireBehavior>();
					foreach (GameObject go in fireSet) {
						Bonfires.Add(go.GetComponent<BonfireBehavior>());
					}
				}
			}
			if (Bonfires != null) {
				int firesLit = 0;
				foreach (BonfireBehavior bonfire in Bonfires) {
					if (bonfire.IsLit()) {
						firesLit++;
					}
				}
				if (firesLit >= Bonfires.Count - bonfiresAllowedIncomplete) {
					EndTheGame(GlobalPlayerContainer.EXPLORER);
				}
				bonfireText.text = "Bonfires Remaining: " + (Bonfires.Count - firesLit - bonfiresAllowedIncomplete);
			}
		}

		private void HandleShrines() {
			if (Shrines == null) {
				HashSet<GameObject> shrineSet = PhotonNetwork.FindGameObjectsWithComponent(typeof(ShrineBehavior));
				if (shrineSet.Count != 0) {
					Shrines = new List<ShrineBehavior>();
					foreach (GameObject go in shrineSet) {
						Shrines.Add(go.GetComponent<ShrineBehavior>());
					}
				}
			}
		}

		private void HandlePlayers() {
			HashSet<GameObject> dreamerSet = PhotonNetwork.FindGameObjectsWithComponent(typeof(BaseExplorerBehavior));
			if ((Explorers == null && dreamerSet.Count != 0) || (Explorers != null && dreamerSet.Count != Explorers.Count)) {
				Explorers = new List<BaseExplorerBehavior>();
				foreach (GameObject go in dreamerSet) {
					Explorers.Add(go.GetComponent<BaseExplorerBehavior>());
				}
			}
			HashSet<GameObject> nightmareSet = PhotonNetwork.FindGameObjectsWithComponent(typeof(BaseNightmareBehavior));
			if ((Nightmares == null && nightmareSet.Count != 0) || (Nightmares != null && nightmareSet.Count != Nightmares.Count)) {
				Nightmares = new List<BaseNightmareBehavior>();
				foreach (GameObject go in nightmareSet) {
					Nightmares.Add(go.GetComponent<BaseNightmareBehavior>());
				}
			}
			if (Explorers != null) {
				int awakeDreamers = 0;
				foreach (BaseExplorerBehavior dreamer in Explorers) {
					if (!dreamer.IsDead()) {
						awakeDreamers++;
					}
				}
				if (awakeDreamers == 0) {
					EndTheGame(GlobalPlayerContainer.NIGHTMARE);
				}
				dreamerText.text = "Dreamers Awake: " + awakeDreamers + " / " + Explorers.Count;    
			}
		}

		private void EndTheGame(int winningTeam) {
			GlobalPlayerContainer.Instance.IsWinner = winningTeam == GlobalPlayerContainer.Instance.TeamSelection;
			if (PhotonNetwork.isMasterClient) {
				PhotonNetwork.LoadLevel("VictoryScene");
			}
		}

		public void ChangeMaskColor(float newValue) {
			maskCamera.backgroundColor = new Color(newValue, newValue, newValue);
		}

		public void LeaveRoom() {
			PhotonNetwork.LeaveRoom();
		}

		public override void OnLeftRoom() {
			SceneManager.LoadScene("LauncherScene");
		}

		public IControllable GetControllableCharacter() {
			if (Nightmare != null)
				return Nightmare;
			if (Explorer != null)
				return Explorer;
			return null;
		}

		[PunRPC]
		public void AddPowerupToCharacter(bool explorer) {
			if (explorer && Explorer != null) {
				Explorer.AddRandomPowerup();
			} else if (!explorer && Nightmare != null) {
				Nightmare.AddRandomPowerup();
			}
		}

		[PunRPC]
		public void DisplayAlert(string alertText, int targets) {
			if (GlobalPlayerContainer.Instance.TeamSelection == GlobalPlayerContainer.OBSERVER ||
			    targets == GlobalPlayerContainer.Instance.TeamSelection) {
				FindObjectOfType<NotificationManagerBehavior>().DisplayTextAlert(alertText);
			}
		}
	}
}
