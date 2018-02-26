using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Com.Tempest.Nightmare {

    public class GameManagerBehavior : Photon.PunBehaviour {
        
        public GameObject nightmarePrefab;
        public GameObject dreamerPrefab;
        public GameObject bonfirePrefab;
        public Image notificationIconPrefab;
        
        public Canvas uiCanvas;
        public Text bonfireText;
        public Text dreamerText;

        public NightmareBehavior Nightmare { get; set; }
        public DreamerBehavior Dreamer { get; set; }

        private int playersConnected;
        private List<BonfireBehavior> bonfires;
        Image[] bonfireNotifications;
        private List<DreamerBehavior> dreamers;
        private List<NightmareBehavior> nightmares;

        public void Awake() {
            if (PhotonNetwork.isMasterClient) {
                PhotonNetwork.room.IsOpen = false;
            }
            bonfireNotifications = new Image[0];
        }

        private void OnEnable() {
            SceneManager.sceneLoaded += LevelWasLoaded;
        }

        private void OnDisable() {
            SceneManager.sceneLoaded -= LevelWasLoaded;
        }

        public void Update() {
            HandleBonfires();
            HandlePlayers();
            HandleCanvasUI();
        }

        private void HandleBonfires() {
            if (bonfires == null) {
                // Try to build the bonfire list if we are the master client but do not have it.
                // This can happen if the user becomes the master client when the master client leaves the room.
                HashSet<GameObject> fireSet = PhotonNetwork.FindGameObjectsWithComponent(typeof(BonfireBehavior));
                if (fireSet.Count != 0) {
                    bonfires = new List<BonfireBehavior>();
                    foreach (GameObject go in fireSet) {
                        bonfires.Add(go.GetComponent<BonfireBehavior>());
                    }
                }
            }
            if (bonfires != null) {
                int firesLit = 0;
                foreach (BonfireBehavior bonfire in bonfires) {
                    if (bonfire.IsLit() == true) {
                        firesLit++;
                    }
                }
                if (firesLit == bonfires.Count) {
                    EndTheGame(PunTeams.Team.red);
                }
                bonfireText.text = "Bonfires Remaining: " + (bonfires.Count - firesLit);
            }
        }

        private void HandlePlayers() {
            HashSet<GameObject> dreamerSet = PhotonNetwork.FindGameObjectsWithComponent(typeof(DreamerBehavior));
            if ((dreamers == null && dreamerSet.Count != 0) || (dreamers != null && dreamerSet.Count != dreamers.Count)) {
                dreamers = new List<DreamerBehavior>();
                foreach (GameObject go in dreamerSet) {
                    dreamers.Add(go.GetComponent<DreamerBehavior>());
                }
            }
            HashSet<GameObject> nightmareSet = PhotonNetwork.FindGameObjectsWithComponent(typeof(NightmareBehavior));
            if ((nightmares == null && nightmareSet.Count != 0) || (nightmares != null && nightmareSet.Count != nightmares.Count)) {
                nightmares = new List<NightmareBehavior>();
                foreach (GameObject go in nightmareSet) {
                    nightmares.Add(go.GetComponent<NightmareBehavior>());
                }
            }
            if (dreamers != null) {
                int deadDreamers = 0;
                foreach(DreamerBehavior dreamer in dreamers) {
                    if (dreamer.IsDead() == true) {
                        deadDreamers++;
                    }
                }
                if (deadDreamers == dreamers.Count) {
                    EndTheGame(PunTeams.Team.blue);
                }
                dreamerText.text = "Dreamers Remaining: " + dreamers.Count;    
            }
        }

        private void HandleCanvasUI() {
            ShowBonfiresIfDead();
        }

        private void ShowBonfiresIfDead() {
            if (bonfires == null) return;
            if (Dreamer == null) return;

            // Build new array of images if necessary.
            if (bonfireNotifications.Length != bonfires.Count) {
                foreach (Image image in bonfireNotifications) {
                    Destroy(image.gameObject);
                }
                bonfireNotifications = new Image[bonfires.Count];
                for (int x = 0; x < bonfireNotifications.Length; x++) {
                    bonfireNotifications[x] = Instantiate<Image>(notificationIconPrefab, uiCanvas.transform, false);
                }
            }

            if (Dreamer.IsDead() == true) {
                // Get camera bounds.
                float vertExtent = Camera.main.orthographicSize;
                float horzExtent = vertExtent * Screen.width / Screen.height;
                Vector3 cameraPosition = Camera.main.transform.position;
                Bounds cameraBounds = new Bounds(new Vector3(cameraPosition.x, cameraPosition.y), new Vector3(horzExtent * 2f, vertExtent * 2f));

                // Get canvas bounds.
                RectTransform canvasRect = uiCanvas.GetComponent<RectTransform>();
                float canvasWidth = (canvasRect.rect.width / 2f) - 16f;
                float canvasHeight = (canvasRect.rect.height / 2f) - 16f;
                float canvasDiagonal = Mathf.Sqrt(Mathf.Pow(canvasWidth, 2f) + Mathf.Pow(canvasHeight, 2f));

                for (int x = 0; x < bonfires.Count; x++) {
                    BonfireBehavior behavior = bonfires[x];
                    Image fireImage = bonfireNotifications[x];
                    if (behavior.PlayersNearby() == true && !cameraBounds.Contains(behavior.transform.position)) {
                        fireImage.gameObject.SetActive(true);
                        fireImage.sprite = behavior.GetCurrentSprite();
                        Vector3 fireDistance = behavior.transform.position - cameraPosition;
                        float angle = Mathf.Atan2(fireDistance.y, fireDistance.x);
                        Vector3 canvasOffset = new Vector3(Mathf.Cos(angle) * canvasDiagonal, Mathf.Sin(angle) * canvasDiagonal);
                        canvasOffset.x = Mathf.Clamp(canvasOffset.x, canvasWidth * -1, canvasWidth);
                        canvasOffset.y = Mathf.Clamp(canvasOffset.y, canvasHeight * -1, canvasHeight);
                        fireImage.rectTransform.localPosition = canvasOffset;
                    } else {
                        fireImage.gameObject.SetActive(false);
                    }
                }
            } else {
                foreach (Image image in bonfireNotifications) {
                    image.gameObject.SetActive(false);
                }
            }
        }

        private void EndTheGame(PunTeams.Team winningTeam) {
            Debug.Log("Attempted to end game, winner is: " + winningTeam);
            LeaveRoom();
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
                InstantiateBonfires();
            }
        }

        public void InstantiateCharacter() {
            PunTeams.Team teamSelection = PhotonNetwork.player.GetTeam();
            switch (teamSelection) {
                case PunTeams.Team.blue:
                    Nightmare = PhotonNetwork.Instantiate(nightmarePrefab.name, new Vector3(1, 0), Quaternion.identity, 0).GetComponent<NightmareBehavior>();
                    break;
                case PunTeams.Team.red:
                    Dreamer = PhotonNetwork.Instantiate(dreamerPrefab.name, new Vector3(-3, 0), Quaternion.identity, 0).GetComponent<DreamerBehavior>();
                    break;
                default:
                    break;
            }
        }

        public void InstantiateBonfires() {
            bonfires = new List<BonfireBehavior> {
                PhotonNetwork.InstantiateSceneObject(bonfirePrefab.name, new Vector3(-40.52f, -14.68f), Quaternion.identity, 0, null).GetComponent<BonfireBehavior>(),
                PhotonNetwork.InstantiateSceneObject(bonfirePrefab.name, new Vector3(39.49f, -14.68f), Quaternion.identity, 0, null).GetComponent<BonfireBehavior>(),
                PhotonNetwork.InstantiateSceneObject(bonfirePrefab.name, new Vector3(39.49f, 21.32f), Quaternion.identity, 0, null).GetComponent<BonfireBehavior>(),
                PhotonNetwork.InstantiateSceneObject(bonfirePrefab.name, new Vector3(-40.52f, 21.32f), Quaternion.identity, 0, null).GetComponent<BonfireBehavior>()
            };
        }

        public override void OnLeftRoom() {
            SceneManager.LoadScene("LauncherScene");
        }

        public void LeaveRoom() {
            PhotonNetwork.LeaveRoom();
        }
    }
}
