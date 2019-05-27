using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Com.Tempest.Nightmare {
    public class MirrorBehavior : Photon.PunBehaviour, IPunObservable {

        public float requiredCharges = 5f;

        public LayerMask whatIsNightmare;
        public LayerMask whatIsExplorer;

        private GameObject progressCanvas;
        private Image positiveProgressBar;
        private CircleCollider2D circleCollider;

        private float explorerCharges;
        private float nightmareCharges;

        void Awake() {
            progressCanvas = transform.Find("MirrorCanvas").gameObject;
            positiveProgressBar = progressCanvas.transform.Find("PositiveProgress").GetComponent<Image>();
            circleCollider = GetComponent<CircleCollider2D>();
            explorerCharges = 0f;
            nightmareCharges = 0f;
        }

        void Update() {
            HandleExplorerProximity();
            HandleNightmareProximity();
            HandleProgressBar();
        }

        private void HandleExplorerProximity() {
            if (!photonView.isMine) return;
            Collider2D[] explorers = Physics2D.OverlapCircleAll(transform.position, circleCollider.radius * (transform.localScale.x + transform.localScale.y) / 2, whatIsExplorer);
            if (explorers.Length == 0) {
                explorerCharges -= Time.deltaTime;
                explorerCharges = Mathf.Max(explorerCharges, 0f);
            } else {
                float modifier = 0f;
                HashSet<BaseExplorer> playerSet = new HashSet<BaseExplorer>();
                foreach (Collider2D collider in explorers) {
                    BaseExplorer behavior = collider.GetComponentInParent<BaseExplorer>();
                    if (behavior == null || playerSet.Contains(behavior)) continue;
                    playerSet.Add(behavior);
                    float explorerModifier = 1f;
                    int mirrorActivationRank = behavior.GetTalentRank(TalentEnum.MIRROR_ACTIVATION);
                    if (behavior.HasPowerup(Powerup.DOUBLE_OBJECTIVE_SPEED)) {
                        explorerModifier *= 2f;
                    }

                    switch (mirrorActivationRank) {
                        case 1:
                            explorerModifier *= 1.25f;
                            break;
                        case 2:
                            explorerModifier *= 2.5f;
                            break;
                        case 3:
                            explorerCharges = requiredCharges;
                            break;
                    }
                    modifier += explorerModifier;
                }
                explorerCharges += Time.deltaTime * modifier;
                explorerCharges = Mathf.Min(explorerCharges, requiredCharges);
            }
        }

        private void HandleNightmareProximity() {
            if (!photonView.isMine) return;
            Collider2D[] nightmares = Physics2D.OverlapCircleAll(transform.position, circleCollider.radius * (transform.localScale.x + transform.localScale.y) / 2, whatIsNightmare);
            if (nightmares.Length == 0) {
                nightmareCharges -= Time.deltaTime;
                nightmareCharges = Mathf.Max(nightmareCharges, 0f);
            } else {
                float modifier = 0f;
                HashSet<BaseNightmare> playerSet = new HashSet<BaseNightmare>();
                foreach (Collider2D collider in nightmares) {
                    BaseNightmare behavior = collider.GetComponentInParent<BaseNightmare>();
                    if (behavior == null || playerSet.Contains(behavior)) continue;
                    playerSet.Add(behavior);
                    float nightmareModifier = 1f;
                    if (behavior.HasPowerup(Powerup.DOUBLE_OBJECTIVE_SPEED)) {
                        nightmareModifier *= 2f;
                    }
                    modifier += nightmareModifier;
                }
                nightmareCharges += Time.deltaTime * modifier;
                nightmareCharges = Mathf.Min(nightmareCharges, requiredCharges);
            }
        }

        private void HandleProgressBar() {
            if (explorerCharges == 0f && nightmareCharges == 0f) {
                progressCanvas.SetActive(false);
            } else {
                progressCanvas.SetActive(true);
                positiveProgressBar.fillAmount = Mathf.Max(explorerCharges, nightmareCharges) / requiredCharges;
            }
        }

        public bool ExplorersActive() {
            return explorerCharges >= requiredCharges;
        }

        public bool NightmaresActive() {
            return nightmareCharges >= requiredCharges;
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
            if (stream.isWriting) {
                stream.SendNext(explorerCharges);
                stream.SendNext(nightmareCharges);
            } else {
                explorerCharges = (float)stream.ReceiveNext();
                nightmareCharges = (float)stream.ReceiveNext();
            }
        }
    }
}
