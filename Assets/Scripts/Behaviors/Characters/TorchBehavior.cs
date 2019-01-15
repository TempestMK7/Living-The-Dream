using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Com.Tempest.Nightmare {

    public class TorchBehavior : Photon.PunBehaviour {

        public float respawnTimer = 60f;
        public float lightBoxScale = 1f;

        public LayerMask whatIsNightmare;
        public LayerMask whatIsExplorer;

        public AudioSource soundSource;

        private Animator animator;
        private CircleCollider2D circleCollider;
        private LightBoxBehavior lightBox;

        private float timeTaken;

        void Awake() {
            lightBox = GetComponentInChildren<LightBoxBehavior>();
            lightBox.IsMine = false;
            lightBox.IsActive = true;
            lightBox.DefaultScale = new Vector3(lightBoxScale, lightBoxScale);
            lightBox.ActiveScale = new Vector3(lightBoxScale, lightBoxScale);

            soundSource.volume = ControlBindingContainer.GetInstance().effectVolume * 0.4f;
            animator = GetComponent<Animator>();
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
				foreach (Collider2D nightmare in nightmares) {
					nightmare.gameObject.GetComponent<BaseNightmare>().photonView.RPC("ReceiveUpgradeEmbers", PhotonTargets.All, 2);
				}
                return;
            }
            Collider2D[] explorers = Physics2D.OverlapCircleAll(transform.position, circleCollider.radius * (transform.localScale.x + transform.localScale.y) / 2, whatIsExplorer);
            if (explorers.Length != 0) {
                photonView.RPC("NotifyTaken", PhotonTargets.All, false);
				foreach (Collider2D explorer in explorers) {
					explorer.gameObject.GetComponent<BaseExplorer>().photonView.RPC("ReceiveUpgradeEmbers", PhotonTargets.All, 2);
				}
            }
        }

        [PunRPC]
        public void NotifyTaken(bool nightmaresWon) {
            timeTaken = Time.time;
            if (!photonView.isMine) 
                return;
            GeneratedGameManager behavior = FindObjectOfType<GeneratedGameManager>();
            if (behavior != null) {
                behavior.photonView.RPC("AddUpgradeToCharacter", PhotonTargets.All, nightmaresWon);
            } else {
                DemoSceneManager demoBehavior = FindObjectOfType<DemoSceneManager>();
                if (demoBehavior != null) {
                    demoBehavior.photonView.RPC("AddUpgradeToCharacter", PhotonTargets.All, nightmaresWon);
                }
            }
            photonView.RPC("PlaySound", PhotonTargets.All, nightmaresWon);
        }

        [PunRPC]
        public void PlaySound(bool nightmaresWon) {
            if (nightmaresWon && PlayerStateContainer.Instance.TeamSelection == PlayerStateContainer.NIGHTMARE) {
                soundSource.Play();
            } else if (!nightmaresWon && PlayerStateContainer.Instance.TeamSelection == PlayerStateContainer.EXPLORER) {
                soundSource.Play();
            } else if (PlayerStateContainer.Instance.TeamSelection == PlayerStateContainer.OBSERVER) {
                soundSource.Play();
            }
        }

        private void HandleSprite() {
            animator.SetBool("IsLit", IsLit());
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
