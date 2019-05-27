using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Coffee.UIExtensions;

namespace Com.Tempest.Nightmare {

	public class NotificationManagerBehavior : MonoBehaviour {

        private const int IMAGES_PER_GAME_OBJECT = 7;

		// Text holder and prefab for text notifications.
		public VerticalLayoutGroup notificationLayout;
		public GameObject alertTextPrefab;

		// Canvas and notification prefab for screen edge notifications.
		public Canvas uiCanvas;
		public Image notificationImagePrefab;
        public GameObject progressParticlePrefab;
        public GameObject chargedParticlePrefab;

		public GameObject gameManagerObject;

		private GeneratedGameManager gameManagerBehavior;
		private List<Image[]> notificationImages;
        private List<GameObject[]> notificationParticles;

		// Use this for initialization
		void Awake() {
			if (gameManagerObject != null) {
				gameManagerBehavior = gameManagerObject.GetComponent<GeneratedGameManager>();
			}
			notificationImages = new List<Image[]>();
            for (int x = 0; x < 20; x++) {
                Image[] images = new Image[IMAGES_PER_GAME_OBJECT];
                for (int y = 0; y < IMAGES_PER_GAME_OBJECT; y++) {
                    images[y] = Instantiate<Image>(notificationImagePrefab, uiCanvas.transform, false);
                }
                notificationImages.Add(images);
            }
            notificationParticles = new List<GameObject[]>();
            for (int x = 0; x < 4; x++) {
                GameObject[] particles = new GameObject[2];
                particles[0] = Instantiate(progressParticlePrefab, uiCanvas.transform, false);
                particles[1] = Instantiate(chargedParticlePrefab, uiCanvas.transform, false);
                notificationParticles.Add(particles);
            }
		}

		// Update is called once per frame
		void Update() {
			HandleNotifications();
			HandleCameraFilter();
		}

		private void HandleNotifications() {
			if (gameManagerBehavior == null) return;
			List<GameObject> objectsToDisplay = new List<GameObject>();
            List<PortalBehavior> particlesToDisplay = new List<PortalBehavior>();
			switch (PlayerStateContainer.Instance.TeamSelection) {
			    case PlayerStateContainer.EXPLORER:
				    BaseExplorer explorer = gameManagerBehavior.Explorer;
				    if (explorer != null) {
				    	if (!explorer.IsDead()) {
				    		objectsToDisplay.AddRange(GetDeadExplorerNotifications());
				    	} else {
				    		objectsToDisplay.AddRange(GetLiveExplorerNotifications());
				    		objectsToDisplay.AddRange(GetAllLitBonfires());
				    	}

                        if (gameManagerBehavior.ShowMirrorNotifications) {
                            objectsToDisplay.AddRange(GetAllUnlitBonfires());
                            objectsToDisplay.AddRange(GetNightmareNotifications());
                            if (!explorer.IsDead()) {
                                objectsToDisplay.AddRange(GetLiveExplorerNotifications());
                            }

                            int chestLocatorRank = explorer.GetChestLocatorRank();
                            if (chestLocatorRank >= 1) {
                                objectsToDisplay.AddRange(GetClosedChests());
                            }
                            if (chestLocatorRank >= 2) {
                                objectsToDisplay.AddRange(GetOpenedChests());
                            }
                        } else if (explorer.HasPowerup(Powerup.NIGHTMARE_VISION)) {
					    	objectsToDisplay.AddRange(GetNightmareNotifications());
					    }

                        int portalNotificationRank = explorer.GetPortalNotificationRank();
                        if (portalNotificationRank >= 1) {
                            particlesToDisplay.AddRange(GetRecentlyChargedPortals());
                        }
                        if (portalNotificationRank >= 2) {
                            particlesToDisplay.AddRange(GetInProgressPortals());
                        }
				    }
				    break;
			    case PlayerStateContainer.NIGHTMARE:
			    	BaseNightmare nightmare = gameManagerBehavior.Nightmare;
			    	if (nightmare != null) {
                        if (gameManagerBehavior.ShowMirrorNotifications) {
                            objectsToDisplay.AddRange(GetNightmareNotifications());
                            objectsToDisplay.AddRange(GetLiveExplorerNotifications());
                        } else if (nightmare.HasPowerup(Powerup.DREAMER_VISION)) {
			    			objectsToDisplay.AddRange(GetLiveExplorerNotifications());
			    		}

                        int chestLocatorRank = nightmare.GetChestLocatorRank();
                        if (chestLocatorRank >= 1) {
                            objectsToDisplay.AddRange(GetClosedChests());
                        }
                        if (chestLocatorRank >= 2) {
                            objectsToDisplay.AddRange(GetOpenedChests());
                        }
                    }
			    	break;
			    default:
			    	break;
			}
			objectsToDisplay.AddRange(GetRecentlyLitBonfires());
			objectsToDisplay.AddRange(GetRecentlyOpenedChests());
			DrawNotifications(objectsToDisplay);
            DrawParticles(particlesToDisplay);
		}

		private void DrawNotifications(List<GameObject> gameObjects) {
			if (gameObjects.Count > notificationImages.Count) {
                for (int x = 0; x < gameObjects.Count - notificationImages.Count; x++) {
                    Image[] images = new Image[IMAGES_PER_GAME_OBJECT];
                    for (int y = 0; y < IMAGES_PER_GAME_OBJECT; y++) {
                        images[y] = Instantiate<Image>(notificationImagePrefab, uiCanvas.transform, false);
                    }
                    notificationImages.Add(images);
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
                // The notificationImages list can be incorrectly sized if the list creation takes longer than 1 frame.
                // This should not be able to happen any more, but I added this check here just in case.
                if (x >= notificationImages.Count) continue;
                Image notification = notificationImages[x][0];
                SpriteRenderer spriteRenderer = notifier.GetComponent<SpriteRenderer>();
				Vector3 distance = notifier.transform.position - cameraBounds.center;
				float angle = Mathf.Atan2(distance.y, distance.x);
				Vector3 canvasOffset = new Vector3(Mathf.Cos(angle) * canvasDiagonal, Mathf.Sin(angle) * canvasDiagonal);
				canvasOffset.x = Mathf.Clamp(canvasOffset.x, canvasWidth * -1, canvasWidth);
				canvasOffset.y = Mathf.Clamp(canvasOffset.y, canvasHeight * -1, canvasHeight);

                if (!spriteRenderer.enabled) {
                    notification.enabled = false;
                } else {
                    notification.sprite = spriteRenderer.sprite;
				    notification.rectTransform.localPosition = canvasOffset;
				    notification.enabled = !cameraBounds.Contains(notifier.transform.position);
                }
				
				SpriteRenderer[] childRenderers = notifier.GetComponentsInChildren<SpriteRenderer>();
				for (int y = 1; y < IMAGES_PER_GAME_OBJECT; y++) {
				    int childIndex = y - 1;
				    Image childNotification = notificationImages[x][y];
				    if (childRenderers.Length > childIndex) {
				        bool isLightBox = childRenderers[childIndex].gameObject.GetComponent<LightBoxBehavior>() != null;
				        childNotification.sprite = childRenderers[childIndex].sprite;
				        childNotification.rectTransform.localPosition = canvasOffset;
				        childNotification.enabled = !isLightBox && !cameraBounds.Contains(notifier.transform.position) && childRenderers[childIndex].enabled;
				    } else {
				        childNotification.enabled = false;
				    }
				}    
			}
            for (int x = gameObjects.Count; x < notificationImages.Count; x++) {
                if (x >= notificationImages.Count) continue;
                for (int y = 0; y < IMAGES_PER_GAME_OBJECT; y++) {
                    notificationImages[x][y].enabled = false;
                }
            }
		}

        private void DrawParticles(List<PortalBehavior> portals) {
            if (portals.Count > notificationParticles.Count) {
                for (int x = 0; x < portals.Count - notificationParticles.Count; x++) {
                    GameObject[] particles = new GameObject[2];
                    particles[0] = Instantiate(progressParticlePrefab, uiCanvas.transform, false);
                    particles[1] = Instantiate(chargedParticlePrefab, uiCanvas.transform, false);
                    notificationParticles.Add(particles);
                }
            }

            // Get camera bounds.
            Bounds cameraBounds = GetCameraBounds();

            // Get canvas bounds.
            RectTransform canvasRect = uiCanvas.GetComponent<RectTransform>();
            float canvasWidth = (canvasRect.rect.width / 2f) - 32f;
            float canvasHeight = (canvasRect.rect.height / 2f) - 32f;
            float canvasDiagonal = Mathf.Sqrt(Mathf.Pow(canvasWidth, 2f) + Mathf.Pow(canvasHeight, 2f));

            for (int x = 0; x < portals.Count; x++) {
                if (x >= notificationParticles.Count) continue;
                PortalBehavior notifier = portals[x];
                GameObject progressNotification = notificationParticles[x][0];
                GameObject chargedNotification = notificationParticles[x][1];
                if (portals[x].IsCharged()) {
                    StopNotification(progressNotification);
                    PlayNotification(chargedNotification);
                } else if (portals[x].IsInProgress()) {
                    StopNotification(chargedNotification);
                    PlayNotification(progressNotification);
                    ParticleSystem.MainModule module = progressNotification.GetComponent<ParticleSystem>().main;
                    module.maxParticles = portals[x].GetMaxParticles();
                }

                Vector3 distance = notifier.transform.position - cameraBounds.center;
                float angle = Mathf.Atan2(distance.y, distance.x);
                Vector3 canvasOffset = new Vector3(Mathf.Cos(angle) * canvasDiagonal, Mathf.Sin(angle) * canvasDiagonal);
                canvasOffset.x = Mathf.Clamp(canvasOffset.x, canvasWidth * -1, canvasWidth);
                canvasOffset.y = Mathf.Clamp(canvasOffset.y, canvasHeight * -1, canvasHeight);

                progressNotification.GetComponent<UIParticle>().rectTransform.localPosition = canvasOffset;
                chargedNotification.GetComponent<UIParticle>().rectTransform.localPosition = canvasOffset;

                if (cameraBounds.Contains(notifier.transform.position)) {
                    StopNotification(progressNotification);
                    StopNotification(chargedNotification);
                }
            }

            for (int x = portals.Count; x < notificationParticles.Count; x++) {
                if (x >= notificationParticles.Count) continue;
                StopNotification(notificationParticles[x][0]);
                StopNotification(notificationParticles[x][1]);
            }
        }

        private void StopNotification(GameObject notification) {
            ParticleSystem system = notification.GetComponent<ParticleSystem>();
            if (system != null && system.isPlaying) {
                system.Stop();
            }
        }

        private void PlayNotification(GameObject notification) {
            ParticleSystem system = notification.GetComponent<ParticleSystem>();
            if (system != null && system.isStopped) {
                system.Play();
            }
        }

        private Bounds GetCameraBounds() {
			Camera mainCamera = Camera.main;
			Vector3 min = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, mainCamera.transform.position.z * -1f));
			min.z = -1f;
			Vector3 max = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, mainCamera.transform.position.z * -1f));
			max.z = 1f;
			Vector3 cameraPosition = Camera.main.transform.position;
			Bounds cameraBounds = new Bounds();
			cameraBounds.SetMinMax(min, max);
			return cameraBounds;
		}

		private List<GameObject> GetRecentlyLitBonfires() {
			List<GameObject> output = new List<GameObject>();
			if (gameManagerBehavior.Bonfires == null)
				return output;
			foreach (BonfireBehavior behavior in gameManagerBehavior.Bonfires) {
				if (behavior.ShowLitNotification()) {
					output.Add(behavior.gameObject);
				}
			}
			return output;
		}

		private List<GameObject> GetAllLitBonfires() {
			List<GameObject> output = new List<GameObject>();
			if (gameManagerBehavior.Bonfires == null)
				return output;
			foreach (BonfireBehavior behavior in gameManagerBehavior.Bonfires) {
				if (behavior.IsLit()) {
					output.Add(behavior.gameObject);
				}
			}
			return output;
		}

        private List<GameObject> GetAllUnlitBonfires() {
            List<GameObject> output = new List<GameObject>();
            if (gameManagerBehavior.Bonfires == null)
                return output;
            foreach (BonfireBehavior behavior in gameManagerBehavior.Bonfires) {
                if (!behavior.IsLit()) {
                    output.Add(behavior.gameObject);
                }
            }
            return output;
        }

        private List<GameObject> GetClosedChests() {
            List<GameObject> output = new List<GameObject>();
            if (gameManagerBehavior.Shrines == null) return output;
            foreach (ShrineBehavior behavior in gameManagerBehavior.Shrines) {
                if (behavior.chestClosed) output.Add(behavior.gameObject);
            }
            return output;
        }

        private List<GameObject> GetOpenedChests() {
            List<GameObject> output = new List<GameObject>();
            if (gameManagerBehavior.Shrines == null) return output;
            foreach (ShrineBehavior behavior in gameManagerBehavior.Shrines) {
                if (behavior.chestClosed) output.Add(behavior.gameObject);
            }
            return output;
        }

        private List<GameObject> GetRecentlyOpenedChests() {
			List<GameObject> output = new List<GameObject>();
			if (gameManagerBehavior.Shrines == null)
				return output;
			foreach (ShrineBehavior behavior in gameManagerBehavior.Shrines) {
				if (behavior.ShowCaptureNotification()) {
					output.Add(behavior.gameObject);
				}
			}
			return output;
        }

        private List<PortalBehavior> GetRecentlyChargedPortals() {
            List<PortalBehavior> output = new List<PortalBehavior>();
            if (gameManagerBehavior.Portals == null) return output;
            foreach (PortalBehavior behavior in gameManagerBehavior.Portals) {
                if (behavior.ShowChargedNotification()) output.Add(behavior);
            }
            return output;
        }

        private List<PortalBehavior> GetInProgressPortals() {
            List<PortalBehavior> output = new List<PortalBehavior>();
            if (gameManagerBehavior.Portals == null) return output;
            foreach (PortalBehavior behavior in gameManagerBehavior.Portals) {
                if (behavior.IsInProgress()) output.Add(behavior);
            }
            return output;
        }

        private List<GameObject> GetDeadExplorerNotifications() {
			List<GameObject> output = new List<GameObject>();
			if (gameManagerBehavior.Explorers == null || gameManagerBehavior.Explorers.Count == 0)
				return output;
			foreach (BaseExplorer behavior in gameManagerBehavior.Explorers) {
				if (behavior.IsDead()) {
					output.Add(behavior.gameObject);
				}
			}
			return output;
		}

		private List<GameObject> GetLiveExplorerNotifications() {
			List<GameObject> output = new List<GameObject>();
			if (gameManagerBehavior.Explorers == null || gameManagerBehavior.Explorers.Count == 0)
				return output;
			foreach (BaseExplorer behavior in gameManagerBehavior.Explorers) {
				if (!behavior.IsDead()) {
					output.Add(behavior.gameObject);
				}
			}
			return output;
		}

		private List<GameObject> GetNightmareNotifications() {
			List<GameObject> output = new List<GameObject>();
			if (gameManagerBehavior.Nightmares == null || gameManagerBehavior.Nightmares.Count == 0)
				return output;
			foreach (BaseNightmare behavior in gameManagerBehavior.Nightmares) {
				output.Add(behavior.gameObject);
			}
			return output;
		}

		public void DisplayTextAlert(string alertText, bool shortNotification) {
			GameObject textPrefab = Instantiate(alertTextPrefab);
			textPrefab.GetComponent<Text>().text = alertText;
			textPrefab.GetComponent<AlertTextBehavior>().IsShortNotification = shortNotification;
			textPrefab.transform.SetParent(notificationLayout.transform);
		}

		public void HandleCameraFilter() {
			bool showMyst = false;
			if (gameManagerBehavior != null) {
				showMyst = gameManagerBehavior.Explorer != null && gameManagerBehavior.Explorer.IsDead();
			}
			Camera.main.GetComponent<CameraFilterPack_3D_Myst>().enabled = showMyst;
		}
	}
}
