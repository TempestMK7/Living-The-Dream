using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Com.Tempest.Nightmare {

    public class TorchBehavior : Photon.PunBehaviour {

        public float respawnTimer = 60f;
        public float lightBoxScale = 1f;

        public Sprite unlitSprite;
        public Sprite litSprite;

        public LayerMask whatIsNightmare;
        public LayerMask whatIsExplorer;

        private SpriteRenderer spriteRenderer;
        private CircleCollider2D circleCollider;
        private LightBoxBehavior lightBox;

        private float timeTaken;

        void Awake() {
            lightBox = GetComponentInChildren<LightBoxBehavior>();
            lightBox.IsMine = false;
            lightBox.IsActive = true;
            lightBox.DefaultScale = new Vector3(lightBoxScale, lightBoxScale);
            lightBox.ActiveScale = new Vector3(lightBoxScale, lightBoxScale);

            spriteRenderer = GetComponent<SpriteRenderer>();
            circleCollider = GetComponent<CircleCollider2D>();
            timeTaken = respawnTimer * -1f;
        }

        void Update() {
            HandlePlayerProximity();
            HandleSprite();
            HandleLightBox();
        }

        private void HandlePlayerProximity() {
            if (!IsLit() || !photonView.isMine) 
                return;
            Collider2D[] nightmares = Physics2D.OverlapCircleAll(transform.position, circleCollider.radius * (transform.localScale.x + transform.localScale.y) / 2, whatIsNightmare);
            if (nightmares.Length != 0) {
                photonView.RPC("NotifyTaken", PhotonTargets.All, true);
                return;
            }
            Collider2D[] explorers = Physics2D.OverlapCircleAll(transform.position, circleCollider.radius * (transform.localScale.x + transform.localScale.y) / 2, whatIsExplorer);
            if (explorers.Length != 0) {
                photonView.RPC("NotifyTaken", PhotonTargets.All, false);
            }
        }

        [PunRPC]
        public void NotifyTaken(bool nightmaresWon) {
            timeTaken = Time.time;
            if (!photonView.isMine) 
                return;
            GeneratedGameManager behavior = FindObjectOfType<GeneratedGameManager>();
            if (behavior == null)
                return;
            behavior.photonView.RPC("AddUpgradeToCharacter", PhotonTargets.All, nightmaresWon);
        }

        private void HandleSprite() {
            if (IsLit()) {
                spriteRenderer.sprite = litSprite;
            } else {
                spriteRenderer.sprite = unlitSprite;
            }
        }

        private void HandleLightBox() {
            lightBox.IsActive = IsLit();
        }

        private bool IsLit() {
            return Time.time - timeTaken > respawnTimer;
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
            // ignored callback.
		}
    }
}
