using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Tempest.Nightmare {
    public class PortalBehavior : Photon.PunBehaviour, IPunObservable {

        public float requiredCharges = 30f;

        public float lightBoxScaleBase = 1f;
        public float lightBoxScaleUncharged = 1f;
        public float lightBoxScaleCharged = 3f;

        public LayerMask whatIsPlayer;

        private LightBoxBehavior lightBox;
        private CircleCollider2D circleCollider;
        private ParticleSystem splashSystem;
        private ParticleSystem portalSystem;

        private float currentCharges;
        private bool splashPlaying;
        private bool portalPlaying;

        void Awake() {
            lightBox = GetComponentInChildren<LightBoxBehavior>();
            lightBox.IsMine = true;
            lightBox.IsActive = false;
            lightBox.DefaultScale = new Vector3(lightBoxScaleUncharged, lightBoxScaleUncharged);
            lightBox.ActiveScale = new Vector3(lightBoxScaleCharged, lightBoxScaleCharged);

            circleCollider = GetComponent<CircleCollider2D>();
            splashSystem = transform.Find("SplashSystem").gameObject.GetComponent<ParticleSystem>();
            portalSystem = transform.Find("PortalSystem").gameObject.GetComponent<ParticleSystem>();

            currentCharges = 0f;
            splashPlaying = false;
            splashSystem.Stop();
            portalPlaying = false;
            portalSystem.Stop();
        }

        void Update() {
            HandlePlayerProximity();
            HandleLightBox();
            HandleParticleSystems();
        }

        private void HandlePlayerProximity() {
            if (photonView.isMine && currentCharges < requiredCharges) {
                Collider2D[] otherPlayers = Physics2D.OverlapCircleAll(transform.position, circleCollider.radius * (transform.localScale.x + transform.localScale.y) / 2, whatIsPlayer);
                float multiplier = 0;
                HashSet<BaseExplorer> playerSet = new HashSet<BaseExplorer>();
                foreach (Collider2D collider in otherPlayers) {
                    BaseExplorer behavior = collider.GetComponentInParent<BaseExplorer>();
                    if (behavior == null || playerSet.Contains(behavior)) continue;
                    playerSet.Add(behavior);
                    float explorerModifier = 1.0f + (behavior.GetBonfireSpeed() * 0.05f);
                    if (behavior.HasPowerup(Powerup.DOUBLE_OBJECTIVE_SPEED)) {
                        explorerModifier *= 2f;
                    }
                    float embers = Time.deltaTime * explorerModifier;
                    multiplier += explorerModifier;
                    behavior.photonView.RPC("ReceiveObjectiveEmbers", PhotonTargets.All, embers);
                }
                currentCharges += Time.deltaTime * multiplier;
                currentCharges = Mathf.Min(currentCharges, requiredCharges);
            }
        }

        private void HandleLightBox() {
            float completion = currentCharges / requiredCharges;
            float unlitScale = completion * lightBoxScaleUncharged;
            lightBox.DefaultScale = new Vector3(unlitScale + lightBoxScaleBase, unlitScale + lightBoxScaleBase);
            lightBox.IsMine = currentCharges > 0f;
            lightBox.IsActive = currentCharges >= requiredCharges;
        }

        private void HandleParticleSystems() {
            if (currentCharges <= 0f) {
                StopSplash();
                StopPortal();
            } else if (currentCharges < requiredCharges) {
                StartSplash();
                StopPortal();
            } else {
                StopSplash();
                StartPortal();
            }
        }

        private void StopSplash() {
            if (splashPlaying) {
                splashSystem.Stop();
                splashPlaying = false;
            }
        }

        private void StopPortal() {
            if (portalPlaying) {
                portalSystem.Stop();
                portalPlaying = false;
            }
        }

        private void StartSplash() {
            if (!splashPlaying) {
                splashSystem.Play();
                splashPlaying = true;
            }
        }

        private void StartPortal() {
            if (!portalPlaying) {
                portalSystem.Play();
                portalPlaying = true;
            }
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
