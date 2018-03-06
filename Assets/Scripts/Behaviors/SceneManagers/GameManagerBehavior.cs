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
        
        public GameObject nightmarePrefab;
        public GameObject dreamerPrefab;
        public GameObject bonfirePrefab;
        public Image notificationIconPrefab;
        
        public Canvas uiCanvas;
        public VerticalLayoutGroup notificationLayout;
        public GameObject alertTextPrefab;
        public Text bonfireText;
        public Text dreamerText;

        public CameraFilterPack_Vision_AuraDistortion distortionEffect;

        public int bonfiresAllowedIncomplete = 0;

        public NightmareBehavior Nightmare { get; set; }
        public DreamerBehavior Dreamer { get; set; }

        private int playersConnected;
        Image[] notificationImages;
        private List<BonfireBehavior> bonfires;
        private List<ShrineBehavior> shrines;
        private List<DreamerBehavior> dreamers;
        private List<NightmareBehavior> nightmares;

        public void Awake() {
            if (PhotonNetwork.isMasterClient) {
                PhotonNetwork.room.IsOpen = false;
            }
            notificationImages = new Image[0];
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
            HandleNotifications();
        }

        private void HandleBonfires() {
            if (bonfires == null) {
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
                    if (bonfire.IsLit()) {
                        firesLit++;
                    }
                }
                if (firesLit >= bonfires.Count - bonfiresAllowedIncomplete) {
                    EndTheGame(PunTeams.Team.red);
                }
                bonfireText.text = "Bonfires Remaining: " + (bonfires.Count - firesLit - bonfiresAllowedIncomplete);
            }
        }

        private void HandleShrines() {
            if (shrines == null) {
                HashSet<GameObject> shrineSet = PhotonNetwork.FindGameObjectsWithComponent(typeof(ShrineBehavior));
                if (shrineSet.Count != 0) {
                    shrines = new List<ShrineBehavior>();
                    foreach (GameObject go in shrineSet) {
                        shrines.Add(go.GetComponent<ShrineBehavior>());
                    }
                }
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
                int awakeDreamers = 0;
                foreach(DreamerBehavior dreamer in dreamers) {
                    if (!dreamer.IsDead()) {
                        awakeDreamers++;
                    }
                }
                if (awakeDreamers == 0) {
                    EndTheGame(PunTeams.Team.blue);
                }
                dreamerText.text = "Dreamers Awake: " + awakeDreamers + " / " + dreamers.Count;    
            }
        }

        private void HandleCameraFilter() {
            distortionEffect.enabled = Dreamer != null && Dreamer.IsDead();
        }

        private void HandleNotifications() {
            List<GameObject> objectsToDisplay = new List<GameObject>();
            if (Dreamer != null) {
                if (Dreamer.IsDead()) {
                    objectsToDisplay.AddRange(GetBonfiresInProgress());
                } else {
                    objectsToDisplay.AddRange(GetBonfiresWithDeadPlayers());
                }
                if (Dreamer.HasPowerup(Powerup.NIGHTMARE_VISION)) {
                    objectsToDisplay.AddRange(GetNightmareNotifications());
                }
            } else if (Nightmare != null) {
                if (Nightmare.HasPowerup(Powerup.DREAMER_VISION)) {
                    objectsToDisplay.AddRange(GetDreamerNotifications());
                }
            } else {
                objectsToDisplay.AddRange(GetBonfiresInProgress());
                objectsToDisplay.AddRange(GetBonfiresWithDeadPlayers());
            }
            objectsToDisplay.AddRange(GetRecentlyLitBonfires());
            objectsToDisplay.AddRange(GetRecentlyLitShrines());
            DrawNotifications(objectsToDisplay);
        }

        private List<GameObject> GetBonfiresInProgress() {
            List<GameObject> output = new List<GameObject>();
            if (bonfires == null) return output;
            foreach(BonfireBehavior behavior in bonfires) {
                if (behavior.PlayersNearby()) {
                    output.Add(behavior.gameObject);
                }
            }
            return output;
        }

        private List<GameObject> GetBonfiresWithDeadPlayers() {
            List<GameObject> output = new List<GameObject>();
            if (bonfires == null) return output;
            foreach (BonfireBehavior behavior in bonfires) {
                if (behavior.DeadPlayersNearby()) {
                    output.Add(behavior.gameObject);
                }
            }
            return output;
        }

        private List<GameObject> GetRecentlyLitBonfires() {
            List<GameObject> output = new List<GameObject>();
            if (bonfires == null) return output;
            foreach (BonfireBehavior behavior in bonfires) {
                if (behavior.ShowLitNotification()) {
                    output.Add(behavior.gameObject);
                }
            }
            return output;
        }

        private List<GameObject> GetRecentlyLitShrines() {
            List<GameObject> output = new List<GameObject>();
            if (shrines == null) return output;
            foreach (ShrineBehavior behavior in shrines) {
                if (behavior.ShowCaptureNotification()) {
                    output.Add(behavior.gameObject);
                }
            }
            return output;
        }

        private List<GameObject> GetDreamerNotifications() {
            List<GameObject> output = new List<GameObject>();
            if (dreamers == null || dreamers.Count == 0) return output;
            foreach (DreamerBehavior behavior in dreamers) {
                output.Add(behavior.gameObject);
            }
            return output;
        }

        private List<GameObject> GetNightmareNotifications() {
            List<GameObject> output = new List<GameObject>();
            if (nightmares == null || nightmares.Count == 0) return output;
            foreach (NightmareBehavior behavior in nightmares) {
                output.Add(behavior.gameObject);
            }
            return output;
        }

        private void DrawNotifications(List<GameObject> gameObjects) {
            if (gameObjects.Count != notificationImages.Length) {
                foreach (Image image in notificationImages) {
                    Destroy(image);
                }
                notificationImages = new Image[gameObjects.Count];
                for (int x = 0; x < notificationImages.Length; x++) {
                    notificationImages[x] = Instantiate<Image>(notificationIconPrefab, uiCanvas.transform, false);
                }
            }

            // Get camera bounds.
            Bounds cameraBounds = GetCameraBounds();

            // Get canvas bounds.
            RectTransform canvasRect = uiCanvas.GetComponent<RectTransform>();
            float canvasWidth = (canvasRect.rect.width / 2f) - 32f;
            float canvasHeight = (canvasRect.rect.height / 2f) - 32f;
            float canvasDiagonal = Mathf.Sqrt(Mathf.Pow(canvasWidth, 2f) + Mathf.Pow(canvasHeight, 2f));

            for (int x = 0; x < gameObjects.Count; x++) {
                GameObject notifier = gameObjects[x];
                Image notification = notificationImages[x];
                notification.enabled = !cameraBounds.Contains(notifier.transform.position);
                notification.sprite = notifier.GetComponent<SpriteRenderer>().sprite;
                Vector3 distance = notifier.transform.position - cameraBounds.center;
                float angle = Mathf.Atan2(distance.y, distance.x);
                Vector3 canvasOffset = new Vector3(Mathf.Cos(angle) * canvasDiagonal, Mathf.Sin(angle) * canvasDiagonal);
                canvasOffset.x = Mathf.Clamp(canvasOffset.x, canvasWidth * -1, canvasWidth);
                canvasOffset.y = Mathf.Clamp(canvasOffset.y, canvasHeight * -1, canvasHeight);
                notification.rectTransform.localPosition = canvasOffset;
            }
        }

        private Bounds GetCameraBounds() {
            Camera mainCamera = Camera.main;
            Vector3 min = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, mainCamera.transform.position.z * -1f));
            Vector3 max = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, mainCamera.transform.position.z * -1f));
            Vector3 cameraPosition = Camera.main.transform.position;
            Bounds cameraBounds = new Bounds();
            cameraBounds.SetMinMax(min, max);
            return cameraBounds;
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
            PunTeams.Team teamSelection = PhotonNetwork.player.GetTeam();
            switch (teamSelection) {
                case PunTeams.Team.blue:
                    Nightmare = PhotonNetwork.Instantiate(nightmarePrefab.name, new Vector3(0f, 4f), Quaternion.identity, 0).GetComponent<NightmareBehavior>();
                    Camera.main.transform.position = Nightmare.transform.position;
                    break;
                case PunTeams.Team.red:
                    Dreamer = PhotonNetwork.Instantiate(dreamerPrefab.name, new Vector3(-42f + (Random.Range(0, 8) * 12f), -38f), Quaternion.identity, 0).GetComponent<DreamerBehavior>();
                    Camera.main.transform.position = Dreamer.transform.position;
                    break;
                default:
                    break;
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
            GameObject textPrefab = Instantiate(alertTextPrefab);
            textPrefab.GetComponent<Text>().text = alertText;
            textPrefab.transform.SetParent(notificationLayout.transform);
        }
    }
}
