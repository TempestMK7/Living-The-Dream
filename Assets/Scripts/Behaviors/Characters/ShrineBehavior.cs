using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Com.Tempest.Nightmare {

	public class ShrineBehavior : Photon.PunBehaviour, IPunObservable {

		public float requiredCharges = 10f;
		public float captureNotificationDuration = 5f;
		public float cooldownTime = 180f;

		public LayerMask whatIsNightmare;
		public LayerMask whatIsDreamer;

		private GameObject progressCanvas;
		private Image positiveProgressBar;
		private SpriteRenderer spriteRenderer;
		private CircleCollider2D circleCollider;

		private float dreamerCharges;
		private float nightmareCharges;
		private float timeLit;

		// Use this for initialization
		void Awake() {
			progressCanvas = transform.Find("ShrineCanvas").gameObject;
			positiveProgressBar = progressCanvas.transform.Find("PositiveProgress").GetComponent<Image>();
			spriteRenderer = GetComponent<SpriteRenderer>();
			circleCollider = GetComponent<CircleCollider2D>();
			dreamerCharges = 0f;
			nightmareCharges = 0f;
			timeLit = 0f;
		}

		// Update is called once per frame
		void Update() {
			HandleDreamerProximity();
			HandleNightmareProximity();
			ResetIfAppropriate();
			HandleProgressBar();
		}

		private void HandleDreamerProximity() {
			if (IsLit() || !photonView.isMine)
				return;
			Collider2D[] dreamers = Physics2D.OverlapCircleAll(transform.position, circleCollider.radius * (transform.localScale.x + transform.localScale.y) / 2, whatIsDreamer);
			if (dreamers.Length == 0) {
				dreamerCharges -= Time.deltaTime;
				dreamerCharges = Mathf.Max(dreamerCharges, 0f);
			} else {
				dreamerCharges += Time.deltaTime * dreamers.Length;
				dreamerCharges = Mathf.Min(dreamerCharges, requiredCharges);
			}
			if (dreamerCharges >= requiredCharges) {
				dreamerCharges = requiredCharges;
				photonView.RPC("NotifyLit", PhotonTargets.All, true);
				foreach (Collider2D dreamer in dreamers) {
					dreamer.gameObject.GetComponent<BaseExplorer>().photonView.RPC("ReceiveUpgradeEmbers", PhotonTargets.All, 10);
				}
			}
		}

		private void HandleNightmareProximity() {
			if (IsLit() || !photonView.isMine)
				return;
			Collider2D[] nightmares = Physics2D.OverlapCircleAll(transform.position, circleCollider.radius * (transform.localScale.x + transform.localScale.y) / 2, whatIsNightmare);
			if (nightmares.Length == 0) {
				nightmareCharges -= Time.deltaTime;
				nightmareCharges = Mathf.Max(nightmareCharges, 0f);
			} else {
				nightmareCharges += Time.deltaTime * nightmares.Length;
				nightmareCharges = Mathf.Min(nightmareCharges, requiredCharges);
			}
			if (nightmareCharges >= requiredCharges) {
				nightmareCharges = requiredCharges;
				photonView.RPC("NotifyLit", PhotonTargets.All, false);
				foreach (Collider2D nightmare in nightmares) {
					nightmare.gameObject.GetComponent<BaseNightmare>().photonView.RPC("ReceiveUpgradeEmbers", PhotonTargets.All, 10);
				}
			}
		}

		[PunRPC]
		public void NotifyLit(bool dreamersWon) {
			if (dreamersWon)
				dreamerCharges = requiredCharges;
			else
				nightmareCharges = requiredCharges;
			timeLit = Time.time;
			AwardPowerups(dreamersWon);
		}

		private void AwardPowerups(bool dreamersWon) {
			if (!photonView.isMine)
				return;
			GeneratedGameManager behavior = FindObjectOfType<GeneratedGameManager>();
			if (behavior != null) {
				behavior.photonView.RPC("AddPowerupToCharacter", PhotonTargets.All, dreamersWon);
			} else {
				DemoSceneManager demoBehavior = FindObjectOfType<DemoSceneManager>();
				if (demoBehavior != null) {
					demoBehavior.photonView.RPC("AddPowerupToCharacter", PhotonTargets.All, dreamersWon);
				}
			}
		}

		private void ResetIfAppropriate() {
			if (!IsLit())
				return;
			if (Time.time - timeLit > cooldownTime) {
				dreamerCharges = 0f;
				nightmareCharges = 0f;
				timeLit = 0f;
			}
		}

		private void HandleProgressBar() {
			if (dreamerCharges == 0f && nightmareCharges == 0f) {
				progressCanvas.SetActive(false);
			} else {
				progressCanvas.SetActive(true);
				if (IsLit()) {
					positiveProgressBar.fillAmount = (cooldownTime - (Time.time - timeLit)) / cooldownTime;
				} else {
					positiveProgressBar.fillAmount = Mathf.Max(dreamerCharges, nightmareCharges) / requiredCharges;
				}
			}
		}

		public bool IsLit() {
			return dreamerCharges >= requiredCharges || nightmareCharges >= requiredCharges;
		}

		public bool ShowCaptureNotification() {
			return Time.time - timeLit < captureNotificationDuration;
		}

		public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
			if (stream.isWriting) {
				stream.SendNext(dreamerCharges);
				stream.SendNext(nightmareCharges);
			} else {
				dreamerCharges = (float)stream.ReceiveNext();
				nightmareCharges = (float)stream.ReceiveNext();
			}
		}
	}
}
