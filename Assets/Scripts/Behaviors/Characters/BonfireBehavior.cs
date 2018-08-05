using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Com.Tempest.Nightmare {

    public class BonfireBehavior : Photon.PunBehaviour, IPunObservable {

        public float requiredCharges = 30f;
        public float litNotificationDuration = 5f;

        public float lightBoxScaleBase = 1f;
        public float lightBoxScaleUnlit = 4f;
        public float lightBoxScaleLit = 10f;

        public Sprite unlitSprite;
        public Sprite partLitSprite;
        public Sprite litSprite;

        public LayerMask whatIsPlayer;
        public LayerMask whatIsDeadPlayer;

        private LightBoxBehavior lightBox;
        private GameObject progressCanvas;
        private Image positiveProgressBar;
        private SpriteRenderer spriteRenderer;
        private CircleCollider2D circleCollider;
        private float currentCharges;
        private float timeLit;
        private bool playersNearby;
        private bool deadPlayersNearby;

        // Use this for initialization
        void Awake() {
            lightBox = GetComponentInChildren<LightBoxBehavior>();
            lightBox.IsMine = true;
            lightBox.IsActive = false;
            lightBox.DefaultScale = new Vector3(lightBoxScaleUnlit, lightBoxScaleUnlit);
            lightBox.ActiveScale = new Vector3(lightBoxScaleLit, lightBoxScaleLit);

            progressCanvas = transform.Find("BonfireCanvas").gameObject;
            positiveProgressBar = progressCanvas.transform.Find("PositiveProgress").GetComponent<Image>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            circleCollider = GetComponent<CircleCollider2D>();
            currentCharges = 0f;
            timeLit = 0f;
        }

        // Update is called once per frame
        void Update() {
            HandlePlayerEvents();
            HandleSprite();
            HandleProgressBar();
            HandleLightBox();
        }

        private void HandlePlayerEvents() {
            Collider2D[] otherPlayers = Physics2D.OverlapCircleAll(transform.position, circleCollider.radius * (transform.localScale.x + transform.localScale.y) / 2, whatIsPlayer);
            if (photonView.isMine && currentCharges < requiredCharges) {
                if (otherPlayers.Length == 0) {
                    currentCharges -= Time.deltaTime;
                    currentCharges = Mathf.Max(currentCharges, 0f);
                } else {
                    float multiplier = otherPlayers.Length;
                    foreach (Collider2D collider in otherPlayers) {
                        BaseExplorer behavior = collider.GetComponentInParent<BaseExplorer>();
                        if (behavior != null && behavior.HasPowerup(Powerup.DOUBLE_OBJECTIVE_SPEED)) {
                            multiplier += 1f;
                        }
                    }
                    currentCharges += Time.deltaTime * multiplier;
                    if (currentCharges >= requiredCharges) {
                        currentCharges = requiredCharges;
                        photonView.RPC("NotifyLit", PhotonTargets.All);
                    }
                }
            }
        }

        private void HandleSprite() {
            if (IsLit()) {
                spriteRenderer.sprite = litSprite;
            } else if (currentCharges != 0) {
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

        private void HandleLightBox() {
            float completion = currentCharges / requiredCharges;
            float unlitScale = completion * lightBoxScaleUnlit;
            lightBox.DefaultScale = new Vector3(unlitScale + lightBoxScaleBase, unlitScale + lightBoxScaleBase);
            lightBox.IsMine = currentCharges > 0f;
            lightBox.IsActive = currentCharges >= requiredCharges;
        }

        [PunRPC]
        public void NotifyLit() {
            currentCharges = requiredCharges;
            timeLit = Time.time;
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

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
            if (stream.isWriting) {
                stream.SendNext(currentCharges);
            } else {
                currentCharges = (float)stream.ReceiveNext();
            }
        }
    }
}
