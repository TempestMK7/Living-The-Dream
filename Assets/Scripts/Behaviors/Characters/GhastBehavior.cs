using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Tempest.Nightmare {

    public class GhastBehavior : BaseNightmare {

        public float dashCooldown = 1f;
        public float collisionDebounceTime = 1f;
        public float upgradeAccelerationFactor = 0.03f;
    
        private float dashStart;
        private float lastCollisionTime;

        protected override void HandleAnimator() {
            base.HandleAnimator();
            animator.SetBool("IsAttacking", IsAttacking());
        }

        public override void ActionPrimaryPressed() {
            base.ActionPrimaryPressed();
            if (Time.time - dashStart >= EffectiveDashCooldown()) {
                DashPhysics();
                dashStart = Time.time;
            }
        }

        private float EffectiveDashCooldown() {
            return HasPowerup(Powerup.HALF_ABILITY_COOLDOWN) ? dashCooldown / 2f : dashCooldown;
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
            return base.GetCurrentAcceleration() + (upgradeAccelerationFactor * (float) NumUpgrades);
        }

        protected override bool IsFlyer() {
            return true;
        }
    }
}
