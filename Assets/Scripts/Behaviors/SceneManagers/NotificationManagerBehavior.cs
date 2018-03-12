using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Com.Tempest.Nightmare {

	public class NotificationManagerBehavior : MonoBehaviour {

		// Text holder and prefab for text notifications.
		public VerticalLayoutGroup notificationLayout;
		public GameObject alertTextPrefab;

		// Canvas and notification prefab for screen edge notifications.
		public Canvas uiCanvas;
		public Image notificationImagePrefab;

		public GameObject gameManagerObject;

		private GeneratedGameManager gameManagerBehavior;
		private Image[] notificationImages;

		// Use this for initialization
		void Awake() {
			gameManagerBehavior = gameManagerObject.GetComponent<GeneratedGameManager>();
			notificationImages = new Image[0];
		}

		// Update is called once per frame
		void Update() {
			HandleNotifications();
			HandleCameraFilter();
		}

		private void HandleNotifications() {
			List<GameObject> objectsToDisplay = new List<GameObject>();
			switch (GlobalPlayerContainer.Instance.TeamSelection) {
			case GlobalPlayerContainer.EXPLORER:
				BaseExplorerBehavior explorer = gameManagerBehavior.Explorer;
				if (explorer != null) {
					if (!explorer.IsDead()) {
						objectsToDisplay.AddRange(GetDeadExplorerNotifications());
					}
					if (explorer.HasPowerup(Powerup.NIGHTMARE_VISION)) {
						objectsToDisplay.AddRange(GetNightmareNotifications());
					}
				}
				break;
			case GlobalPlayerContainer.NIGHTMARE:
				BaseNightmareBehavior nightmare = gameManagerBehavior.Nightmare;
				if (nightmare != null) {
					if (nightmare.HasPowerup(Powerup.DREAMER_VISION)) {
						objectsToDisplay.AddRange(GetExplorerNotifications());
					}
				}
				break;
			default:
				break;
			}
			objectsToDisplay.AddRange(GetRecentlyLitBonfires());
			objectsToDisplay.AddRange(GetRecentlyLitShrines());
			DrawNotifications(objectsToDisplay);
		}

		private void DrawNotifications(List<GameObject> gameObjects) {
			if (gameObjects.Count != notificationImages.Length) {
				foreach (Image image in notificationImages) {
					Destroy(image);
				}
				notificationImages = new Image[gameObjects.Count];
				for (int x = 0; x < notificationImages.Length; x++) {
					notificationImages[x] = Instantiate<Image>(notificationImagePrefab, uiCanvas.transform, false);
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
				notification.sprite = notifier.GetComponent<SpriteRenderer>().sprite;
				Vector3 distance = notifier.transform.position - cameraBounds.center;
				float angle = Mathf.Atan2(distance.y, distance.x);
				Vector3 canvasOffset = new Vector3(Mathf.Cos(angle) * canvasDiagonal, Mathf.Sin(angle) * canvasDiagonal);
				canvasOffset.x = Mathf.Clamp(canvasOffset.x, canvasWidth * -1, canvasWidth);
				canvasOffset.y = Mathf.Clamp(canvasOffset.y, canvasHeight * -1, canvasHeight);
				notification.rectTransform.localPosition = canvasOffset;
				notification.enabled = !cameraBounds.Contains(notifier.transform.position);
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

		private List<GameObject> GetRecentlyLitShrines() {
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

		private List<GameObject> GetExplorerNotifications() {
			List<GameObject> output = new List<GameObject>();
			if (gameManagerBehavior.Explorers == null || gameManagerBehavior.Explorers.Count == 0)
				return output;
			foreach (BaseExplorerBehavior behavior in gameManagerBehavior.Explorers) {
				if (!behavior.IsDead()) {
					output.Add(behavior.gameObject);
				}
			}
			return output;
		}

		private List<GameObject> GetDeadExplorerNotifications() {
			List<GameObject> output = new List<GameObject>();
			if (gameManagerBehavior.Explorers == null || gameManagerBehavior.Explorers.Count == 0)
				return output;
			foreach (BaseExplorerBehavior behavior in gameManagerBehavior.Explorers) {
				if (behavior.IsDead()) {
					output.Add(behavior.gameObject);
				}
			}
			return output;
		}

		private List<GameObject> GetNightmareNotifications() {
			List<GameObject> output = new List<GameObject>();
			if (gameManagerBehavior.Nightmares == null || gameManagerBehavior.Nightmares.Count == 0)
				return output;
			foreach (BaseNightmareBehavior behavior in gameManagerBehavior.Nightmares) {
				output.Add(behavior.gameObject);
			}
			return output;
		}

		public void DisplayTextAlert(string alertText) {
			GameObject textPrefab = Instantiate(alertTextPrefab);
			textPrefab.GetComponent<Text>().text = alertText;
			textPrefab.transform.SetParent(notificationLayout.transform);
		}

		public void HandleCameraFilter() {
			bool showMyst = gameManagerBehavior.Explorer != null && gameManagerBehavior.Explorer.IsDead();
			Camera.main.GetComponent<CameraFilterPack_3D_Myst>().enabled = showMyst;
		}
	}
}
