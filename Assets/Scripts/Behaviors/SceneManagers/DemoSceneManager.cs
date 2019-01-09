using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Tilemaps;

namespace Com.Tempest.Nightmare {

    public class DemoSceneManager : Photon.PunBehaviour {
	
		public Camera maskCamera;

        public GameObject doubleJumpPrefab;
        public GameObject bonfirePrefab;
        public GameObject shrinePrefab;
        public GameObject torchPrefab;

        public Text upgradesText;

        public GameObject objectivePanel;
        public GameObject controlPanel;

        private GameObject[,] levelChunks;

        private BaseExplorer explorer;

        public void Awake() {
            ClosePanels();
            if (PhotonNetwork.connected) {
                CreateInvisibleRoom();
            } else {
                PhotonNetwork.ConnectUsingSettings(Constants.GAME_VERSION);
            }
        }

        public override void OnConnectedToMaster() {
            CreateInvisibleRoom();
        }

        private void CreateInvisibleRoom() {
            RoomOptions options = new RoomOptions();
            options.IsOpen = true;
            options.IsVisible = true;
            options.MaxPlayers = 1;
            PhotonNetwork.CreateRoom(null, options, TypedLobby.Default);
        }

        public override void OnJoinedRoom() {
            InstantiateChunks();
            GenerateObjects();
            CreateCharacter();
            ChangeMaskColor(0);
        }

        private void InstantiateChunks() {
            levelChunks = new GameObject[2, 2];
            levelChunks[0, 0] = (GameObject)Instantiate(Resources.Load("LevelChunks/LevelChunk10"), new Vector3(0, 0), Quaternion.identity);
            levelChunks[1, 0] = (GameObject)Instantiate(Resources.Load("LevelChunks/ShrineChunks/LevelChunk8"), new Vector3(16, 0), Quaternion.identity);
            levelChunks[0, 1] = (GameObject)Instantiate(Resources.Load("LevelChunks/TorchChunks/LevelChunk7"), new Vector3(0, 16), Quaternion.identity);
            levelChunks[1, 1] = (GameObject)Instantiate(Resources.Load("LevelChunks/BonfireChunks/LevelChunk5"), new Vector3(16, 16), Quaternion.identity);
        }

        private void GenerateObjects() {
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

        private void CreateCharacter() {
            PlayerStateContainer container = PlayerStateContainer.Instance;
            container.TeamSelection = PlayerStateContainer.EXPLORER;
            container.ExplorerSelection = PlayerStateContainer.DOUBLE_JUMP_EXPLORER;
            explorer = PhotonNetwork.Instantiate(doubleJumpPrefab.name, new Vector3(2, 2), Quaternion.identity, 0).GetComponent<BaseExplorer>();
            explorer.SendTalentsToNetwork();
            Camera.main.transform.position = explorer.transform.position;
        }

		private void ChangeMaskColor(float newValue) {
			maskCamera.backgroundColor = new Color(newValue, newValue, newValue);
		}

        public void Update() {
            if (explorer != null) upgradesText.text = "Upgrades: " + explorer.GetUnmodifiedUpgrades();
        }

        public void QuitDemo() {
			PhotonNetwork.LeaveRoom();
        }

		public override void OnLeftRoom() {
			SceneManager.LoadScene("LauncherScene");
		}

        public void ClosePanels() {
            objectivePanel.SetActive(false);
            controlPanel.SetActive(false);
        }

        public void OpenObjectivesPanel() {
            objectivePanel.SetActive(true);
            controlPanel.SetActive(false);
        }

        public void OpenControlsPanel() {
            controlPanel.SetActive(true);
            objectivePanel.SetActive(false);
        }

        public IControllable GetControllableCharacter() {
            return explorer;
        }

		[PunRPC]
		public void AddPowerupToCharacter(bool explorer) {
            this.explorer.AddRandomPowerup();
		}

		[PunRPC]
		public void AddUpgradeToCharacter(bool nightmaresWon) {
            explorer.AddUpgrade();
		}

		[PunRPC]
		public void DisplayAlert(string alertText, int targets) {
			if (PlayerStateContainer.Instance.TeamSelection == PlayerStateContainer.OBSERVER ||
			    targets == PlayerStateContainer.Instance.TeamSelection) {
				FindObjectOfType<NotificationManagerBehavior>().DisplayTextAlert(alertText);
			}
		}
    }
}
