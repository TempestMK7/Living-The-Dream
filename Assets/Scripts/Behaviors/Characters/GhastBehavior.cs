using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Tempest.Nightmare {

    public class GhastBehavior : BaseNightmare {

        public float collisionDebounceTime = 1f;
    
        private float dashStart;
        private float lastCollisionTime;

        protected override void HandleAnimator() {
            base.HandleAnimator();
            animator.SetBool("IsAttacking", IsAttacking());
        }

        public override void ActionSecondaryPressed(Vector3 mouseDirection) {
            base.ActionSecondaryPressed(mouseDirection);
            DashPhysics(mouseDirection);
        }

        protected override float DashCooldown() {
            return HasPowerup(Powerup.HALF_ABILITY_COOLDOWN) ? base.DashCooldown() / 2f : base.DashCooldown();
        }

        public bool IsAttacking() {
            return currentState == MovementState.DASHING;
        }

        public void OnTriggerEnter2D(Collider2D other) {
            if (!photonView.isMine) return;
            BaseExplorer associatedBehavior = other.gameObject.GetComponent<BaseExplorer>();
            if (associatedBehavior == null || associatedBehavior.IsOutOfHealth()) return;
            if (IsAttacking() && Time.time - lastCollisionTime > collisionDebounceTime) {
                associatedBehavior.photonView.RPC("TakeDamage", PhotonTargets.All, currentSpeed);
                this.currentSpeed *= -1;
                lastCollisionTime = Time.time;
                photonView.RPC("ReceiveObjectiveEmbers", PhotonTargets.All, 10f);
            }
        }

        public void OnTriggerStay2D(Collider2D other) {
            if (Time.time - lastCollisionTime > collisionDebounceTime) {
                OnTriggerEnter2D(other);
            }
        }

        protected override float GetCurrentAcceleration() {
            return base.GetCurrentAcceleration() * GetSigmoidUpgradeMultiplier(1f, 2.5f);
        }

        protected override bool IsFlyer() {
            return true;
        }

        protected override void LoadTalents() {
            talentRanks = GlobalTalentContainer.GetInstance().GhastTalents;
        }
    }
}
