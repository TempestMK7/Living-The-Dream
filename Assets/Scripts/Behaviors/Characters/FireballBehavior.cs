using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Tempest.Nightmare {

    public class FireballBehavior : Photon.PunBehaviour {

        public float startingSpeed = 20f;
        public float gravityFactor = 4f;
        public float explosionTriggerRadius = .5f;
        public float explosionRadius = 2f;
        public float explosionDuration = 0.5f;

        public LayerMask whatIsExplosionTrigger;
        public LayerMask whatTakesDamage;

        private Animator animator;

        private Vector3 currentSpeed;
        private float explosionTime;

        public void SetStartingDirection(Vector3 currentControllderState) {
            currentSpeed = currentControllderState;
            if (currentSpeed.magnitude == 0f) currentSpeed = new Vector3(0f, -20f);
            float ratio = startingSpeed / currentSpeed.magnitude;
            currentSpeed.x *= ratio;
            currentSpeed.y *= ratio;
        }

        // Use this for initialization
        void Awake() {
            animator = GetComponent<Animator>();
            if (currentSpeed == null) currentSpeed = new Vector3();
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
                BaseExplorerBehavior dreamer = trigger.gameObject.GetComponent<BaseExplorerBehavior>();
                if (dreamer != null && !dreamer.OutOfHealth()) {
                    dreamer.photonView.RPC("TakeDamage", PhotonTargets.All, currentSpeed);
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
