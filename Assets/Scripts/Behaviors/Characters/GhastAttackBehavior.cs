using System.Collections.Generic;
using UnityEngine;

namespace Com.Tempest.Nightmare {

    public class GhastAttackBehavior : Photon.PunBehaviour {

        public LayerMask whatTakesDamage;

        private float attackDuration;
        private Vector3 attackAngle;
        private float attackRadius;
        private Vector3 launchVector;
        private int damage;
        private float freezeTime;
        private float stunTime;

        private float creationTime;
        private List<BaseExplorer> playersHit;

        public GhastBehavior ParentGhast { get; set; }

        [PunRPC]
        public void Initialize(float attackDuration, Vector3 attackAngle, float attackRadius, Vector3 launchVector, int damage, float freezeTime, float stunTime) {
            this.attackDuration = attackDuration;
            this.attackAngle = attackAngle;
            this.attackRadius = attackRadius;
            this.launchVector = launchVector;
            this.damage = damage;
            this.freezeTime = freezeTime;
            this.stunTime = stunTime;
            creationTime = Time.time;
        }

        public void Awake() {
            playersHit = new List<BaseExplorer>();
            if (ParentGhast == null) FindParent();
            UpdatePosition();
        }

        void Update() {
            if (ParentGhast == null) FindParent();
            UpdatePosition();
            if (!photonView.isMine) return;
            CheckForExplorers();
            DestroyIfExpired();
        }

        private void UpdatePosition() {
            if (ParentGhast == null) return;
            Vector3 parentPosition = ParentGhast.transform.position;
            parentPosition.y += 0.5f;
            transform.position = parentPosition + attackAngle;
        }

        private void FindParent() {
            int ownerId = photonView.ownerId;
            foreach (GhastBehavior behavior in FindObjectsOfType<GhastBehavior>()) {
                if (behavior.photonView.ownerId == ownerId) {
                    ParentGhast = behavior;
                    return;
                }
            }
        }

        private void CheckForExplorers() {
            Collider2D[] triggers = Physics2D.OverlapCircleAll(transform.position, attackRadius, whatTakesDamage);
            foreach (Collider2D trigger in triggers) {
                BaseExplorer explorer = trigger.gameObject.GetComponent<BaseExplorer>();
                if (explorer != null && !explorer.OutOfHealth() && !playersHit.Contains(explorer)) {
                    playersHit.Add(explorer);
                    explorer.photonView.RPC("OnDamageTaken", PhotonTargets.All, explorer.transform.position, launchVector, damage, freezeTime, stunTime);
                    ParentGhast.photonView.RPC("ReceiveObjectiveEmbers", PhotonTargets.All, 10f);
                }
            }
        }

        private void DestroyIfExpired() {
            if (photonView.isMine && Time.time - creationTime > attackDuration) {
                PhotonNetwork.Destroy(photonView);
            }
        }
    }
}
