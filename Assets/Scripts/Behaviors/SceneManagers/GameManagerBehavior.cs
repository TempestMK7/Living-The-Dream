using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Com.Tempest.Nightmare {

    public class GameManagerBehavior : Photon.PunBehaviour {

        public Camera maskCamera;

        // UI objects.
        public Text bonfireText;
        public Text dreamerText;
        
        // Prefabs.
        public GameObject ghastPrefab;
        public GameObject cryoPrefab;
        public GameObject doubleJumpPrefab;
        public GameObject jetpackPrefab;
        public GameObject lightBoxPrefab;

        // Game parameters.
        public int bonfiresAllowedIncomplete = 0;

        // Publicly accessible fields pertaining to game state.
        public BaseExplorerBehavior Explorer { get; set; }
        public BaseNightmareBehavior Nightmare { get; set; }

        public List<BonfireBehavior> Bonfires { get; set; }
        public List<ShrineBehavior> Shrines { get; set; }
        public List<BaseExplorerBehavior> Explorers { get; set; }
        public List<BaseNightmareBehavior> Nightmares { get; set; }

        private int playersConnected;

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
                foreach(BaseExplorerBehavior dreamer in Explorers) {
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

        private void LevelWasLoaded(Scene scene, LoadSceneMode mode) {
            photonView.RPC("NotifyMasterClientConnected", PhotonTargets.MasterClient);
            // If we are not the master client, it is safe to load our character now.
            if (!PhotonNetwork.isMasterClient) {
                InstantiateCharacter();
            }
        }

        [PunRPC]
        public void NotifyMasterClientConnected() {
            if (!PhotonNetwork.isMasterClient) return;
            playersConnected++;
            if (PhotonNetwork.playerList.Length == playersConnected) {
                InstantiateCharacter();
            }
        }

        public void InstantiateCharacter() {
            GlobalPlayerContainer playerContainer = GlobalPlayerContainer.Instance;
            if (playerContainer.TeamSelection == GlobalPlayerContainer.EXPLORER) {
                switch (playerContainer.ExplorerSelection) {
                    case GlobalPlayerContainer.DOUBLE_JUMP_EXPLORER:
                        Explorer = PhotonNetwork.Instantiate(doubleJumpPrefab.name, new Vector3(-42f + (Random.Range(0, 8) * 12f), -38f), Quaternion.identity, 0)
                            .GetComponent<BaseExplorerBehavior>();
                        break;
                    case GlobalPlayerContainer.JETPACK_EXPLORER:
                        Explorer = PhotonNetwork.Instantiate(jetpackPrefab.name, new Vector3(-42f + (Random.Range(0, 8) * 12f), -38f), Quaternion.identity, 0)
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
                        Nightmare = PhotonNetwork.Instantiate(ghastPrefab.name, new Vector3(0f, 4f), Quaternion.identity, 0).GetComponent<BaseNightmareBehavior>();
                        break;
                    case GlobalPlayerContainer.CRYO:
                        Nightmare = PhotonNetwork.Instantiate(cryoPrefab.name, new Vector3(0f, 4f), Quaternion.identity, 0).GetComponent<BaseNightmareBehavior>();
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
            if (Nightmare != null) return Nightmare;
            if (Explorer != null) return Explorer;
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
