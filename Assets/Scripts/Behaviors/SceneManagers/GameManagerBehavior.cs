using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Com.Tempest.Nightmare {

    public class GameManagerBehavior : Photon.PunBehaviour {

        public const int DREAMER = 0;
        public const int NIGHTMARE = 1;
        public const int ALL = 2;

        public Camera maskCamera;

        // UI objects.
        public Text bonfireText;
        public Text dreamerText;
        
        // Prefabs.
        public GameObject ghastPrefab;
        public GameObject doubleJumpPrefab;
        public GameObject jetpackPrefab;
        public GameObject lightBoxPrefab;

        // Camera filter when dreamer is dead.
        public CameraFilterPack_Vision_AuraDistortion distortionEffect;

        // Game parameters.
        public int bonfiresAllowedIncomplete = 0;

        // Publicly accessible fields pertaining to game state.
        public BaseNightmareBehavior Nightmare { get; set; }
        public BaseDreamerBehavior Dreamer { get; set; }

        public List<BonfireBehavior> Bonfires { get; set; }
        public List<ShrineBehavior> Shrines { get; set; }
        public List<BaseDreamerBehavior> Dreamers { get; set; }
        public List<BaseNightmareBehavior> Nightmares { get; set; }

        private int playersConnected;

        public void Start() {
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
            HandleCameraFilter();
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
                    EndTheGame(PunTeams.Team.red);
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
            HashSet<GameObject> dreamerSet = PhotonNetwork.FindGameObjectsWithComponent(typeof(BaseDreamerBehavior));
            if ((Dreamers == null && dreamerSet.Count != 0) || (Dreamers != null && dreamerSet.Count != Dreamers.Count)) {
                Dreamers = new List<BaseDreamerBehavior>();
                foreach (GameObject go in dreamerSet) {
                    Dreamers.Add(go.GetComponent<BaseDreamerBehavior>());
                }
            }
            HashSet<GameObject> nightmareSet = PhotonNetwork.FindGameObjectsWithComponent(typeof(BaseNightmareBehavior));
            if ((Nightmares == null && nightmareSet.Count != 0) || (Nightmares != null && nightmareSet.Count != Nightmares.Count)) {
                Nightmares = new List<BaseNightmareBehavior>();
                foreach (GameObject go in nightmareSet) {
                    Nightmares.Add(go.GetComponent<BaseNightmareBehavior>());
                }
            }
            if (Dreamers != null) {
                int awakeDreamers = 0;
                foreach(BaseDreamerBehavior dreamer in Dreamers) {
                    if (!dreamer.IsDead()) {
                        awakeDreamers++;
                    }
                }
                if (awakeDreamers == 0) {
                    EndTheGame(PunTeams.Team.blue);
                }
                dreamerText.text = "Dreamers Awake: " + awakeDreamers + " / " + Dreamers.Count;    
            }
        }

        private void HandleCameraFilter() {
            distortionEffect.enabled = Dreamer != null && Dreamer.IsDead();
        }

        private void EndTheGame(PunTeams.Team winningTeam) {
            GlobalPlayerContainer.Instance.IsWinner = winningTeam == PhotonNetwork.player.GetTeam();
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
            if (playerContainer.TeamSelection == GlobalPlayerContainer.DREAMER) {
                switch (playerContainer.DreamerSelection) {
                    case GlobalPlayerContainer.DOUBLE_JUMP_DREAMER:
                        Dreamer = PhotonNetwork.Instantiate(doubleJumpPrefab.name, new Vector3(-42f + (Random.Range(0, 8) * 12f), -38f), Quaternion.identity, 0)
                            .GetComponent<BaseDreamerBehavior>();
                        break;
                    case GlobalPlayerContainer.JETPACK_DREAMER:
                        Dreamer = PhotonNetwork.Instantiate(jetpackPrefab.name, new Vector3(-42f + (Random.Range(0, 8) * 12f), -38f), Quaternion.identity, 0)
                            .GetComponent<BaseDreamerBehavior>();
                        break;
                }
                if (Dreamer != null) {
                    Camera.main.transform.position = Dreamer.transform.position;
                }
                maskCamera.backgroundColor = new Color(.01f, .01f, .02f);
            } else if (playerContainer.TeamSelection == GlobalPlayerContainer.NIGHTMARE) {
                switch (playerContainer.NightmareSelection) {
                    case GlobalPlayerContainer.GHAST:
                        Nightmare = PhotonNetwork.Instantiate(ghastPrefab.name, new Vector3(0f, 4f), Quaternion.identity, 0).GetComponent<BaseNightmareBehavior>();
                        break;
                }
                if (Nightmare != null) {
                    Camera.main.transform.position = Nightmare.gameObject.transform.position;
                }
                maskCamera.backgroundColor = new Color(.03f, .03f, .05f);
            } else {
                maskCamera.backgroundColor = new Color(.5f, .5f, .55f);
            }
        }

        public void LeaveRoom() {
            PhotonNetwork.LeaveRoom();
        }

        public override void OnLeftRoom() {
            SceneManager.LoadScene("LauncherScene");
        }
        
        public IControllable GetControllableCharacter() {
            if (Nightmare != null) return Nightmare;
            if (Dreamer != null) return Dreamer;
            return null;
        }

        [PunRPC]
        public void AddPowerupToCharacter(bool dreamer) {
            if (dreamer && Dreamer != null) {
                Dreamer.AddRandomPowerup();
            } else if (!dreamer && Nightmare != null) {
                Nightmare.AddRandomPowerup();
            }
        }

        [PunRPC]
        public void DisplayAlert(string alertText, int targets) {
            if (targets == DREAMER && Dreamer == null) return;
            if (targets == NIGHTMARE && Nightmare == null) return;
            FindObjectOfType<NotificationManagerBehavior>().DisplayTextAlert(alertText);
        }
    }
}
