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
		public Text embersText;
		public Text upgradesText;

		public Tilemap borderMap;
		public TileBase ruleTile;
	
		// Prefabs.
		public GameObject ghastPrefab;
		public GameObject cryoPrefab;
		public GameObject goblinPrefab;
		
		public GameObject doubleJumpPrefab;
		public GameObject jetpackPrefab;
		public GameObject dashPrefab;

		public GameObject bonfirePrefab;
		public GameObject shrinePrefab;
		public GameObject torchPrefab;
        public GameObject mirrorPrefab;
        public GameObject portalPrefab;

		public GameObject settingsPanel;
		public Button settingsButton;

        // Game parameters.
        public const int DEPTH = 3;
		public int bonfiresAllowedIncomplete = 0;
		public int levelWidth = 8;
		public int levelHeight = 8;
		public int bonfireFrequency = 4;
		public int bonfireOffset = 1;
        public int numChests = 4;
        public int numMirrors = 4;
	
		// Publicly accessible fields pertaining to game state.
		public BaseExplorer Explorer { get; set; }

		public BaseNightmare Nightmare { get; set; }

		public List<BonfireBehavior> Bonfires { get; set; }

		public List<ShrineBehavior> Shrines { get; set; }

        public List<MirrorBehavior> Mirrors { get; set; }

        public List<PortalBehavior> Portals { get; set; }

		public List<BaseExplorer> Explorers { get; set; }

		public List<BaseNightmare> Nightmares { get; set; }

        public bool ShowMirrorNotifications { get; set; }

		private bool gameStarted;
		private int playersConnected;
		private int levelsGenerated;
        private int[,,] levelGraph;
        private GameObject[,] levelChunks;

		#region Level Loading

		public void Awake() {
			if (PhotonNetwork.isMasterClient) {
				PhotonNetwork.room.IsOpen = false;
			}
			settingsPanel.gameObject.SetActive(false);
			gameStarted = false;
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
			StartGameIfAble();
		}

		private void StartGameIfAble() {
			if (PhotonNetwork.isMasterClient && !gameStarted && playersConnected >= PhotonNetwork.playerList.Length) {
				gameStarted = true;
				int[,,] levelGraph = GenerateLevelGraph(levelWidth, levelHeight, bonfireFrequency, bonfireOffset, numChests, numMirrors);
				photonView.RPC("GenerateLevel", PhotonTargets.All, levelWidth, levelHeight, TransformToOneDimension(levelGraph));
			}
		}

		private static int[,,] GenerateLevelGraph(int width, int height, int bonfire, int offset, int numChests, int numMirrors) {
			LevelGenerator generator = new LevelGenerator(width, height, bonfire, offset, numChests, numMirrors);
			return generator.SerializeLevelGraph();
		}

		private static int[] TransformToOneDimension(int[,,] array) {
			int[] output = new int[array.Length];
			Buffer.BlockCopy(array, 0, output, 0, array.Length * 4);
			return output;
		}

		private static int[,,] TransformToThreeDimension(int[] array, int depth, int width, int height) {
			int[,,] output = new int[depth, width, height];
			Buffer.BlockCopy(array, 0, output, 0, output.Length * 4);
			return output;
		}

		[PunRPC]
		public void GenerateLevel(int width, int height, int[] singleDimensionGraph) {
			levelGraph = TransformToThreeDimension(singleDimensionGraph, DEPTH, width, height);
			levelChunks = new GameObject[width, height];
			for (int x = 0; x < width; x++) {
				for (int y = 0; y < height; y++) {
					int roomType = levelGraph[LevelGenerator.CHUNK_INDEX_LAYER, x, y];
                    int roomRandomizedType = levelGraph[LevelGenerator.CHUNK_RANDOMIZED_TYPE_LAYER, x, y];
					Vector3 position = new Vector3(x * 16, y * 16);
					string path = LevelGenerator.ResourcePathForIndex(roomType, roomRandomizedType);
					try {
						levelChunks[x, y] = (GameObject)Instantiate(Resources.Load(path), position, Quaternion.identity);
					} catch (ArgumentException e) {
						Debug.Log("Attempted to instantiate illegal path: " + path);
						throw e;
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
				photonView.RPC("FindBonfireBehaviors", PhotonTargets.All);
				photonView.RPC("InstantiateCharacter", PhotonTargets.All);
			}
		}

		public void GenerateBonfires() {
            for (int x = 0; x < levelWidth; x++) {
                for (int y = 0; y < levelHeight; y++) {
                    int chunkObjectType = levelGraph[LevelGenerator.CHUNK_OBJECT_TYPE_LAYER, x, y];
                    GameObject levelChunk = levelChunks[x, y];
                    Transform fireHolder = levelChunk.transform.Find("BonfirePlaceholder");
                    if (fireHolder != null) {
                        if (chunkObjectType == LevelGenerator.GraphNode.BONFIRE) {
                            PhotonNetwork.InstantiateSceneObject(bonfirePrefab.name, fireHolder.position, Quaternion.identity, 0, null);
                        } else if (chunkObjectType == LevelGenerator.GraphNode.CHEST) {
                            PhotonNetwork.InstantiateSceneObject(shrinePrefab.name, fireHolder.position, Quaternion.identity, 0, null);
                        } else if (chunkObjectType == LevelGenerator.GraphNode.MIRROR) {
                            PhotonNetwork.InstantiateSceneObject(mirrorPrefab.name, fireHolder.position, Quaternion.identity, 0, null);
                        } else if (chunkObjectType == LevelGenerator.GraphNode.PORTAL) {
                            Vector3 position = fireHolder.position;
                            position.y -= 0.5f;
                            PhotonNetwork.InstantiateSceneObject(portalPrefab.name, position, Quaternion.identity, 0, null);
                        } else {
                            Vector3 position = fireHolder.position;
                            position.y -= 0.5f;
                            PhotonNetwork.InstantiateSceneObject(torchPrefab.name, position, Quaternion.identity, 0, null);
                        }
                    }
                }
            }
		}

		[PunRPC]
		public void FindBonfireBehaviors() {
			HashSet<GameObject> fireSet = PhotonNetwork.FindGameObjectsWithComponent(typeof(BonfireBehavior));
			if (fireSet.Count != 0) {
				Bonfires = new List<BonfireBehavior>();
				foreach (GameObject go in fireSet) {
					Bonfires.Add(go.GetComponent<BonfireBehavior>());
				}
			}
			HashSet<GameObject> shrineSet = PhotonNetwork.FindGameObjectsWithComponent(typeof(ShrineBehavior));
			if (shrineSet.Count != 0) {
				Shrines = new List<ShrineBehavior>();
				foreach (GameObject go in shrineSet) {
					Shrines.Add(go.GetComponent<ShrineBehavior>());
				}
			}
            HashSet<GameObject> mirrorSet = PhotonNetwork.FindGameObjectsWithComponent(typeof(MirrorBehavior));
            if (mirrorSet.Count != 0) {
                Mirrors = new List<MirrorBehavior>();
                foreach (GameObject go in mirrorSet) {
                    Mirrors.Add(go.GetComponent<MirrorBehavior>());
                }
            }
		}

		[PunRPC]
		public void InstantiateCharacter() {
			PlayerStateContainer playerContainer = PlayerStateContainer.Instance;
			if (playerContainer.TeamSelection == PlayerStateContainer.EXPLORER) {
				float spawnLocationX = (UnityEngine.Random.Range(1, levelChunks.GetLength(0) - 1) * 16) + 2;
				float spawnLocationY = (UnityEngine.Random.Range(levelChunks.GetLength(1) / 2, levelChunks.GetLength(1)) * 16) + 2;
				Vector3 spawnLocation = new Vector3(spawnLocationX, spawnLocationY);
				switch (playerContainer.ExplorerSelection) {
				case PlayerStateContainer.DOUBLE_JUMP_EXPLORER:
					Explorer = PhotonNetwork.Instantiate(doubleJumpPrefab.name, spawnLocation, Quaternion.identity, 0)
						.GetComponent<BaseExplorer>();
					break;
				case PlayerStateContainer.JETPACK_EXPLORER:
					Explorer = PhotonNetwork.Instantiate(jetpackPrefab.name, spawnLocation, Quaternion.identity, 0)
						.GetComponent<BaseExplorer>();
					break;
				case PlayerStateContainer.DASH_EXPLORER:
					Explorer = PhotonNetwork.Instantiate(dashPrefab.name, spawnLocation, Quaternion.identity, 0)
						.GetComponent<BaseExplorer>();
					break;
				}
				if (Explorer != null) {
					Explorer.SendTalentsToNetwork();
					Explorer.SendNameToNetwork();
					Camera.main.transform.position = Explorer.transform.position;
				}
				ChangeMaskColor(0f);
			} else if (playerContainer.TeamSelection == PlayerStateContainer.NIGHTMARE) {
				float spawnLocationX = (UnityEngine.Random.Range(1, levelChunks.GetLength(0) - 1) * 16) + 2;
				float spawnLocationY = (UnityEngine.Random.Range(0, levelChunks.GetLength(1) / 2) * 16) + 2;
				Vector3 spawnLocation = new Vector3(spawnLocationX, spawnLocationY);
				switch (playerContainer.NightmareSelection) {
					case PlayerStateContainer.GHAST:
						Nightmare = PhotonNetwork.Instantiate(ghastPrefab.name, spawnLocation, Quaternion.identity, 0)
							.GetComponent<BaseNightmare>();
						break;
					case PlayerStateContainer.CRYO:
						Nightmare = PhotonNetwork.Instantiate(cryoPrefab.name, spawnLocation, Quaternion.identity, 0)
							.GetComponent<BaseNightmare>();
						break;
					case PlayerStateContainer.GOBLIN:
						Nightmare = PhotonNetwork.Instantiate(goblinPrefab.name, spawnLocation, Quaternion.identity, 0)
							.GetComponent<BaseNightmare>();
						break;
				}
				if (Nightmare != null) {
					Nightmare.SendTalentsToNetwork();
					Nightmare.SendNameToNetwork();
					Camera.main.transform.position = Nightmare.gameObject.transform.position;
				}
				ChangeMaskColor(0f);
			} else {
				ChangeMaskColor(0.5f);
			}
		}

		#endregion

		#region Scene Object Handling

		public void Update() {
			StartGameIfAble();
			HandleBonfires();
			HandlePlayers();
			HandleUpgrades();
			HandleEmbers();
            HandleMirrors();
		}

		private void HandleBonfires() {
			if (Bonfires != null) {
				int firesLit = 0;
				foreach (BonfireBehavior bonfire in Bonfires) {
					if (bonfire.IsLit()) {
						firesLit++;
					}
				}
				if (firesLit >= Bonfires.Count - bonfiresAllowedIncomplete) {
					BeginEndingSequence(PlayerStateContainer.EXPLORER);
				}
				bonfireText.text = "Bonfires Remaining: " + (Bonfires.Count - firesLit - bonfiresAllowedIncomplete);
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
					BeginEndingSequence(PlayerStateContainer.NIGHTMARE);
				}
				dreamerText.text = "Explorers Alive: " + aliveExplorers + " / " + Explorers.Count;    
			}
		}

		private void HandleUpgrades() {
			if (Explorer != null) {
				upgradesText.text = "Upgrades: " + Explorer.GetUnmodifiedUpgrades();
			} else if (Nightmare != null) {
				upgradesText.text = "Upgrades: " + Nightmare.GetUnmodifiedUpgrades();
			}
		}

		private void HandleEmbers() {
			embersText.text = "Embers: " + PlayerStateContainer.Instance.TotalEmbers();
		}

        private void HandleMirrors() {
            if (Mirrors == null || Mirrors.Count == 0) return;
            bool explorersActive = false;
            bool nightmaresActive = false;
            foreach (MirrorBehavior mirror in Mirrors) {
                if (mirror.ExplorersActive()) explorersActive = true;
                if (mirror.NightmaresActive()) nightmaresActive = true;
            }
            ShowMirrorNotifications = (explorersActive && Explorer != null) || (nightmaresActive && Nightmare != null);
        }

		private void BeginEndingSequence(int winningTeam) {
			StartCoroutine(EndingSequence(winningTeam));
		}

		IEnumerator EndingSequence(int winningTeam) {
			yield return new WaitForSeconds(1f);
			EndTheGame(winningTeam);
		}

		private void EndTheGame(int winningTeam) {
			PlayerStateContainer.Instance.IsWinner = winningTeam == PlayerStateContainer.Instance.TeamSelection;
			if (PhotonNetwork.isMasterClient) {
				PhotonNetwork.LoadLevel("VictoryScene");
			}
		}

		public void ChangeMaskColor(float newValue) {
			maskCamera.backgroundColor = new Color(newValue, newValue, newValue);
		}

		#endregion

		#region Externally Called Methods

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
		public void AddUpgradeToCharacter(bool isNightmare) {
			if (!isNightmare && Explorer != null) {
				Explorer.AddUpgrade();
			} else if (isNightmare && Nightmare != null) {
				Nightmare.AddUpgrade();
			}
		}

		[PunRPC]
		public void DisplayAlert(string alertText, bool shortNotification, int targets) {
			if (PlayerStateContainer.Instance.TeamSelection == PlayerStateContainer.OBSERVER ||
			    targets == PlayerStateContainer.Instance.TeamSelection) {
				FindObjectOfType<NotificationManagerBehavior>().DisplayTextAlert(alertText, shortNotification);
			}
		}

		public void ToggleSettingsPanel() {
			bool isActive = settingsPanel.gameObject.GetActive();
			settingsPanel.gameObject.SetActive(!isActive);
			if (isActive) {
				settingsPanel.GetComponent<SettingsManager>().OnPanelClose();
				settingsButton.GetComponentInChildren<Text>().text = "Settings (Esc)";
			} else {
				settingsPanel.GetComponent<SettingsManager>().OnPanelLaunch(true);
				settingsButton.GetComponentInChildren<Text>().text = "Back";
			}
		}

		#endregion
	}
}
