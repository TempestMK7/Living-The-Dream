using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Tempest.Nightmare {
    public class PortalBehavior : Photon.PunBehaviour, IPunObservable, IPunCallbacks {

        public float requiredCharges = 30f;

        public float lightBoxScaleBase = 1f;
        public float lightBoxScaleUncharged = 1f;
        public float lightBoxScaleCharged = 3f;

        public float maxVolume = 0.5f;

        public LayerMask whatIsPlayer;

        public CircleCollider2D teleportCollider;
        public CircleCollider2D chargeCollider;

        public AudioSource chargingSource;
        public AudioSource chargedSource;
        public AudioSource teleportSource;

        private LightBoxBehavior lightBox;
        private ParticleSystem splashSystem;
        private ParticleSystem portalSystem;

        private float currentCharges;
        private bool splashPlaying;
        private bool portalPlaying;

        public int PortalIndex { get; set; }
        private int connectedPortalIndex;
        private PortalBehavior connectedPortal;

        void Awake() {
            lightBox = GetComponentInChildren<LightBoxBehavior>();
            lightBox.IsMine = true;
            lightBox.IsActive = false;
            lightBox.DefaultScale = new Vector3(lightBoxScaleUncharged, lightBoxScaleUncharged);
            lightBox.ActiveScale = new Vector3(lightBoxScaleCharged, lightBoxScaleCharged);

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
            HandleParticleSystems();
            HandleLightBox();
            HandleSounds();
            TeleportPlayersIfAble();
        }

        private void HandlePlayerProximity() {
            if (photonView.isMine && currentCharges < requiredCharges) {
                Collider2D[] otherPlayers = Physics2D.OverlapCircleAll(transform.position, chargeCollider.radius * (transform.localScale.x + transform.localScale.y) / 2, whatIsPlayer);
                if (otherPlayers.Length == 0) {
                    currentCharges -= Time.deltaTime;
                    currentCharges = Mathf.Max(currentCharges, 0f);
                } else {
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
                    if (currentCharges >= requiredCharges) {
                        photonView.RPC("PlayChargedSound", PhotonTargets.All);
                    }
                }
            }
        }

        private void TeleportPlayersIfAble() {
            PortalBehavior connectedPortal = GetConnectedPortal();
            if (!photonView.isMine || !IsCharged() || connectedPortal == null || !connectedPortal.IsCharged()) return;
            Vector3 position = transform.position;
            position.y += teleportCollider.offset.y;
            Collider2D[] players = Physics2D.OverlapCircleAll(position, teleportCollider.radius * (transform.localScale.x + transform.localScale.y) / 2, whatIsPlayer);
            foreach (Collider2D player in players) {
                BaseExplorer playerBehavior = player.gameObject.GetComponent<BaseExplorer>();
                if (playerBehavior == null) continue;
                playerBehavior.photonView.RPC("TeleportToPortal", PhotonTargets.All, connectedPortal.transform.position);
            }
        }

        private void HandleLightBox() {
            float completion = currentCharges / requiredCharges;
            float unlitScale = completion * lightBoxScaleUncharged;
            lightBox.DefaultScale = new Vector3(unlitScale + lightBoxScaleBase, unlitScale + lightBoxScaleBase);
            lightBox.IsMine = currentCharges > 0f;
            lightBox.IsActive = currentCharges >= requiredCharges;
        }

        private void HandleSounds() {
            if (currentCharges > 0 && currentCharges < requiredCharges) {
                chargingSource.volume = ControlBindingContainer.GetInstance().effectVolume * maxVolume;
            } else {
                chargingSource.volume = 0f;
            }
        }

        [PunRPC]
        public void PlayChargedSound() {
            chargedSource.volume = ControlBindingContainer.GetInstance().effectVolume;
            chargedSource.Play();
        }

        private void HandleParticleSystems() {
            if (currentCharges <= 0f) {
                StopSplash();
                StopPortal();
            } else if (currentCharges < requiredCharges) {
                StartSplash();
                StopPortal();
                float percentComplete = currentCharges / requiredCharges;
                int maxParticles = (int)(10f * percentComplete) + 1;
                ParticleSystem.MainModule module = splashSystem.main;
                module.maxParticles = maxParticles;
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

        [PunRPC]
        public void SetPortalIndex(int portalIndex, int connectedPortalIndex) {
            PortalIndex = portalIndex;
            this.connectedPortalIndex = connectedPortalIndex;
        }

        public PortalBehavior GetConnectedPortal() {
            if (connectedPortal != null) return connectedPortal;
            PortalBehavior[] otherPortals = FindObjectsOfType<PortalBehavior>();
            foreach (PortalBehavior portal in otherPortals) {
                if (portal.PortalIndex == connectedPortalIndex) {
                    connectedPortal = portal;
                }
            }
            return connectedPortal;
        }

        public bool IsCharged() {
            return currentCharges >= requiredCharges;
        }
    }
}
