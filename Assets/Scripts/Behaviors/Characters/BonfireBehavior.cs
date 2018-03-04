using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Com.Tempest.Nightmare {
    
    public class BonfireBehavior : Photon.PunBehaviour, IPunObservable {

        public float requiredCharges = 30f;
        public float litNotificationDuration = 5f;
        public Sprite unlitSprite;
        public Sprite partLitSprite;
        public Sprite litSprite;
        public LayerMask whatIsPlayer;

        private GameObject progressCanvas;
        private Image positiveProgressBar;
        private SpriteRenderer spriteRenderer;
        private CircleCollider2D circleCollider;
        private float currentCharges;
        private float timeLit;
        private bool playersNearby;

        // Use this for initialization
        void Awake () {
            progressCanvas = transform.Find("BonfireCanvas").gameObject;
            positiveProgressBar = progressCanvas.transform.Find("PositiveProgress").GetComponent<Image>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            circleCollider = GetComponent<CircleCollider2D>();
            currentCharges = 0f;
	    }
	
	    // Update is called once per frame
	    void Update () {
            HandlePlayerEvents();
            HandleSprite();
            HandleProgressBar();
        }

        private void HandlePlayerEvents() {
            Collider2D[] otherPlayers = Physics2D.OverlapCircleAll(transform.position, circleCollider.radius * (transform.localScale.x + transform.localScale.y) / 2, whatIsPlayer);
            playersNearby = otherPlayers.Length != 0;
            if (photonView.isMine && currentCharges < requiredCharges) {
                if (playersNearby == false) {
                    currentCharges -= Time.deltaTime;
                    currentCharges = Mathf.Max(currentCharges, 0f);
                } else {
                    currentCharges += Time.deltaTime * otherPlayers.Length;
                    currentCharges = Mathf.Min(currentCharges, requiredCharges);
                }
                if (currentCharges >= requiredCharges) {
                    currentCharges = requiredCharges;
                    timeLit = Time.time;
                }
            }
        }

        private void HandleSprite() {
            if (IsLit()) {
                spriteRenderer.sprite = litSprite;
            } else if (playersNearby) {
                spriteRenderer.sprite = partLitSprite;
            } else {
                spriteRenderer.sprite = unlitSprite;
            }
        }

        private void HandleProgressBar() {
            if (IsLit() || currentCharges == 0f) {
                progressCanvas.SetActive(false);
            } else {
                progressCanvas.SetActive(true);
                positiveProgressBar.fillAmount = currentCharges / requiredCharges;
            }
        }

        public Sprite GetCurrentSprite() {
            return spriteRenderer.sprite;
        }

        public bool IsLit() {
            return currentCharges >= requiredCharges;
        }

        public bool ShowLitNotification() {
            return Time.time - timeLit < litNotificationDuration;
        }

        public bool PlayersNearby() {
            return playersNearby;
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
            if (stream.isWriting) {
                stream.SendNext(currentCharges);
                stream.SendNext(timeLit);
            } else {
                currentCharges = (float)stream.ReceiveNext();
                timeLit = (float)stream.ReceiveNext();
            }
        }
    }
}

    
