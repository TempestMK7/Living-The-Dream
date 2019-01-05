using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Tempest.Nightmare {

    public class IceBallBehavior : Photon.PunBehaviour {

        public float gravityFactor = 4f;
        public float explosionTriggerRadius = .5f;
        public float explosionRadius = 2f;
        public float explosionDuration = 0.5f;
        public float lightBoxScale = 1f;

        public LayerMask whatIsExplosionTrigger;
        public LayerMask whatTakesDamage;

        private Animator animator;
        private LightBoxBehavior lightBox;

        private Vector3 currentSpeed;
        private float explosionTime;

        private List<BaseExplorer> playersHit;

        public CryoBehavior CryoLauncherBehavior { get; set; }

        public void SetStartingDirection(Vector3 currentControllerState, float startingSpeed) {
            currentSpeed = currentControllerState;
            if (currentSpeed.magnitude == 0f) currentSpeed = new Vector3(0f, -20f);
            float ratio = startingSpeed / currentSpeed.magnitude;
            currentSpeed.x *= ratio;
            currentSpeed.y *= ratio;
        }

        // Use this for initialization
        void Awake() {
            animator = GetComponent<Animator>();
            playersHit = new List<BaseExplorer>();

            lightBox = GetComponentInChildren<LightBoxBehavior>();
            lightBox.IsMine = false;
            lightBox.IsActive = true;
            lightBox.DefaultScale = new Vector3(lightBoxScale, lightBoxScale);
            lightBox.ActiveScale = new Vector3(lightBoxScale, lightBoxScale);
        }
        
        void Update() {
            UpdatePosition();
            ExplodeIfAble();
            DamageIfExploding();
            DestroyIfExpired();
        }

        private void UpdatePosition() {
            if (IsExploding()) {
                currentSpeed = new Vector3();
                return;
            }
            currentSpeed.y -= gravityFactor * Time.deltaTime;
            transform.position += currentSpeed * Time.deltaTime;
        }

        private void ExplodeIfAble() {
            if (explosionTime != 0f) return;
            Collider2D[] triggers = Physics2D.OverlapCircleAll(transform.position, explosionTriggerRadius, whatIsExplosionTrigger);
            if (triggers.Length != 0) {
                explosionTime = Time.time;
                animator.SetBool("IsExploding", true);
            }
        }

        private void DamageIfExploding() {
            if (!photonView.isMine || !IsExploding()) return;
            Collider2D[] triggers = Physics2D.OverlapCircleAll(transform.position, explosionRadius, whatTakesDamage);
            foreach (Collider2D trigger in triggers) {
                BaseExplorer explorer = trigger.gameObject.GetComponent<BaseExplorer>();
                if (explorer != null && !explorer.IsOutOfHealth() && !playersHit.Contains(explorer)) {
                    playersHit.Add(explorer);
                    explorer.photonView.RPC("TakeDamage", PhotonTargets.All, currentSpeed);
                    CryoLauncherBehavior.photonView.RPC("ReceiveObjectiveEmbers", PhotonTargets.All, 10f);
                }
            }
        }

        private void DestroyIfExpired() {
            if (photonView.isMine && Time.time - explosionTime > explosionDuration && explosionTime != 0f) {
                PhotonNetwork.Destroy(photonView);
            }
        }

        public bool IsExploding() {
            return Time.time - explosionTime < explosionDuration;
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
            if (stream.isWriting) {
                stream.SendNext(transform.position);
                stream.SendNext(currentSpeed);
            } else {
                transform.position = (Vector3)stream.ReceiveNext();
                currentSpeed = (Vector3)stream.ReceiveNext();
            }
        }
    }
}
