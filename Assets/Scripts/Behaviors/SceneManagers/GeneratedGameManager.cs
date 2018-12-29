using System;
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
		public Text upgradesText;

		public Tilemap borderMap;
		public TileBase ruleTile;
	
		// Prefabs.
		public GameObject ghastPrefab;
		public GameObject cryoPrefab;
		public GameObject goblinPrefab;
		
		public GameObject doubleJumpPrefab;
		public GameObject jetpackPrefab;

		public GameObject bonfirePrefab;
		public GameObject shrinePrefab;
		public GameObject torchPrefab;
	
		// Game parameters.
		public int bonfiresAllowedIncomplete = 0;
		public int levelWidth = 8;
		public int levelHeight = 8;
		public int bonfireFrequency = 4;
		public float torchProbability = 0.3f;
	
		// Publicly accessible fields pertaining to game state.
		public BaseExplorer Explorer { get; set; }

		public BaseNightmare Nightmare { get; set; }

		public List<BonfireBehavior> Bonfires { get; set; }

		public List<ShrineBehavior> Shrines { get; set; }

		public List<BaseExplorer> Explorers { get; set; }

		public List<BaseNightmare> Nightmares { get; set; }

		private int playersConnected;
		private int levelsGenerated;
		private GameObject[,] levelChunks;

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
				int[,] levelGraph = GenerateLevelGraph(levelWidth, levelHeight, bonfireFrequency, torchProbability);
				photonView.RPC("GenerateLevel", PhotonTargets.All, levelWidth, levelHeight, TransformToOneDimension(levelGraph));
			}
		}

		private static int[,] GenerateLevelGraph(int width, int height, int bonfire, float torchProbability) {
			LevelGenerator generator = new LevelGenerator(width, height, bonfire, torchProbability);
			return generator.SerializeLevelGraph();
		}

		private static int[] TransformToOneDimension(int[,] array) {
			int[] output = new int[array.Length];
			Buffer.BlockCopy(array, 0, output, 0, array.Length * 4);
			return output;
		}

		private static int[,] TransformToTwoDimension(int[] array, int width, int height) {
			int[,] output = new int[width, height];
			Buffer.BlockCopy(array, 0, output, 0, output.Length * 4);
			return output;
		}

		[PunRPC]
		public void GenerateLevel(int width, int height, int[] singleDimensionGraph) {
			int[,] levelGraph = TransformToTwoDimension(singleDimensionGraph, width, height);
			levelChunks = new GameObject[width, height];
			for (int x = 0; x < width; x++) {
				for (int y = 0; y < height; y++) {
					int roomType = levelGraph[x, y];
					Vector3 position = new Vector3(x * 16, y * 16);
					if ((x == 0 || x == width - 1) && (y == 0 || y == height - 1)) {
						levelChunks[x, y] = (GameObject)Instantiate(Resources.Load("LevelChunks/ShrineChunks/LevelChunk" + roomType), position, Quaternion.identity);
					} else if (roomType < 0) {
						roomType *= -1;
						levelChunks[x, y] = (GameObject)Instantiate(Resources.Load("LevelChunks/BonfireChunks/LevelChunk" + roomType), position, Quaternion.identity);
					} else if (roomType >= 100) {
						roomType -= 100;
						levelChunks[x, y] = (GameObject)Instantiate(Resources.Load("LevelChunks/TorchChunks/LevelChunk" + roomType), position, Quaternion.identity);
					} else {
						levelChunks[x, y] = (GameObject)Instantiate(Resources.Load("LevelChunks/LevelChunk" + roomType), position, Quaternion.identity);
					}
				}
			}
			photonView.RPC("NotifyLevelGenerated", PhotonTargets.MasterClient);
		}

		[PunRPC]
		public void NotifyLevelGenerated() {
			if (!PhotonNetwork.isMasterClient)
				return;
			levelsGenerated++;
			if (PhotonNetwork.playerList.Length == levelsGenerated) {
				GenerateBonfires();
				photonView.RPC("InstantiateCharacter", PhotonTargets.All);
			}
		}

		public void GenerateBonfires() {
			foreach (GameObject chunk in levelChunks) {
				Transform fireHolder = chunk.transform.Find("BonfirePlaceholder");
				if (fireHolder != null) {
					PhotonNetwork.Instantiate(bonfirePrefab.name, fireHolder.position, Quaternion.identity, 0);
				}
				Transform shrineHolder = chunk.transform.Find("ShrinePlaceholder");
				if (shrineHolder != null) {
					PhotonNetwork.Instantiate(shrinePrefab.name, shrineHolder.position, Quaternion.identity, 0);
				}
				Transform torchHolder = chunk.transform.Find("TorchPlaceholder");
				if (torchHolder != null) {
					PhotonNetwork.Instantiate(torchPrefab.name, torchHolder.position, Quaternion.identity, 0);
				}
			}
		}

		[PunRPC]
		public void InstantiateCharacter() {
			GlobalPlayerContainer playerContainer = GlobalPlayerContainer.Instance;
			if (playerContainer.TeamSelection == GlobalPlayerContainer.EXPLORER) {
				float spawnLocationX = (UnityEngine.Random.Range(1, levelChunks.GetLength(0) - 1) * 16) + 2;
				float spawnLocationY = (UnityEngine.Random.Range(levelChunks.GetLength(1) / 2, levelChunks.GetLength(1)) * 16) + 2;
				Vector3 spawnLocation = new Vector3(spawnLocationX, spawnLocationY);
				switch (playerContainer.ExplorerSelection) {
				case GlobalPlayerContainer.DOUBLE_JUMP_EXPLORER:
					Explorer = PhotonNetwork.Instantiate(doubleJumpPrefab.name, spawnLocation, Quaternion.identity, 0)
						.GetComponent<BaseExplorer>();
					break;
				case GlobalPlayerContainer.JETPACK_EXPLORER:
					Explorer = PhotonNetwork.Instantiate(jetpackPrefab.name, spawnLocation, Quaternion.identity, 0)
						.GetComponent<BaseExplorer>();
					break;
				}
				if (Explorer != null) {
					Camera.main.transform.position = Explorer.transform.position;
				}
				ChangeMaskColor(0f);
			} else if (playerContainer.TeamSelection == GlobalPlayerContainer.NIGHTMARE) {
				float spawnLocationX = (UnityEngine.Random.Range(1, levelChunks.GetLength(0) - 1) * 16) + 2;
				float spawnLocationY = (UnityEngine.Random.Range(0, levelChunks.GetLength(1) / 2) * 16) + 2;
				Vector3 spawnLocation = new Vector3(spawnLocationX, spawnLocationY);
				switch (playerContainer.NightmareSelection) {
					case GlobalPlayerContainer.GHAST:
						Nightmare = PhotonNetwork.Instantiate(ghastPrefab.name, spawnLocation, Quaternion.identity, 0)
							.GetComponent<BaseNightmare>();
						break;
					case GlobalPlayerContainer.CRYO:
						Nightmare = PhotonNetwork.Instantiate(cryoPrefab.name, spawnLocation, Quaternion.identity, 0)
							.GetComponent<BaseNightmare>();
						break;
					case GlobalPlayerContainer.GOBLIN:
						Nightmare = PhotonNetwork.Instantiate(goblinPrefab.name, spawnLocation, Quaternion.identity, 0)
							.GetComponent<BaseNightmare>();
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
			HandleUpgrades();
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
					BeginEndingSequence(GlobalPlayerContainer.EXPLORER);
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
			HashSet<GameObject> explorerSet = PhotonNetwork.FindGameObjectsWithComponent(typeof(BaseExplorer));
			if ((Explorers == null && explorerSet.Count != 0) || (Explorers != null && explorerSet.Count != Explorers.Count)) {
				Explorers = new List<BaseExplorer>();
				foreach (GameObject go in explorerSet) {
					Explorers.Add(go.GetComponent<BaseExplorer>());
				}
			}
			HashSet<GameObject> nightmareSet = PhotonNetwork.FindGameObjectsWithComponent(typeof(BaseNightmare));
			if ((Nightmares == null && nightmareSet.Count != 0) || (Nightmares != null && nightmareSet.Count != Nightmares.Count)) {
				Nightmares = new List<BaseNightmare>();
				foreach (GameObject go in nightmareSet) {
					Nightmares.Add(go.GetComponent<BaseNightmare>());
				}
			}
			if (Explorers != null) {
				int aliveExplorers = 0;
				foreach (BaseExplorer explorer in Explorers) {
					if (!explorer.IsDead()) {
						aliveExplorers++;
					}
				}
				if (aliveExplorers == 0) {
					BeginEndingSequence(GlobalPlayerContainer.NIGHTMARE);
				}
				dreamerText.text = "Explorers Alive: " + aliveExplorers + " / " + Explorers.Count;    
			}
		}

		private void HandleUpgrades() {
			if (Explorer != null) {
				upgradesText.text = "Upgrades: " + Explorer.NumUpgrades;
			} else if (Nightmare != null) {
				upgradesText.text = "Upgrades: " + Nightmare.NumUpgrades;
			}
		}

		private void BeginEndingSequence(int winningTeam) {
			StartCoroutine(EndingSequence(winningTeam));
		}

		IEnumerator EndingSequence(int winningTeam) {
			yield return new WaitForSeconds(1f);
			EndTheGame(winningTeam);
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
		public void AddUpgradeToCharacter(bool nightmaresWon) {
			if (!nightmaresWon && Explorer != null) {
				Explorer.AddUpgrade();
			} else if (nightmaresWon && Nightmare != null) {
				Nightmare.AddUpgrade();
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
