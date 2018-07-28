using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Tempest.Nightmare {

    public class GhastBehavior : BaseNightmareBehavior {

        public float dashFactor = 2f;
        public float dashDuration = 0.1f;
        public float dashDamageDuration = 0.5f;
        public float dashCooldown = 1f;
        public float collisionDebounceTime = 1f;
        public float upgradeAccelerationFactor = 0.03f;
    
        private float dashSpeed;
        private float dashStart;
        private float lastCollisionTime;

        public override void Awake() {
            base.Awake();
            lightBox.IsActive = true;
            dashSpeed = maxSpeed * dashFactor;
        }

        protected override void UpdateCurrentSpeed() {
            if (photonView.isMine && Time.time - dashStart > dashDuration) {
                base.UpdateCurrentSpeed();
            }
        }

        protected override void HandleAnimator() {
            animator.SetBool("IsAttacking", IsAttacking());
        }

        public override void ActionPressed() {
            float usableDashCooldown = HasPowerup(Powerup.HALF_ABILITY_COOLDOWN) ? dashCooldown / 2f : dashCooldown;
            if (Time.time - dashStart < usableDashCooldown || Time.time - lastCollisionTime < collisionDebounceTime) return;
            dashStart = Time.time;
            float angle = Mathf.Atan2(currentControllerState.y, currentControllerState.x);
            currentSpeed.x = Mathf.Cos(angle) * dashSpeed;
            currentSpeed.y = Mathf.Sin(angle) * dashSpeed;
        }

        public override void ActionReleased() {

        }

        public bool IsAttacking() {
            return Time.time - dashStart < dashDamageDuration;
        }

        public void OnTriggerEnter2D(Collider2D other) {
            if (!photonView.isMine) return;
            BaseExplorerBehavior associatedBehavior = other.gameObject.GetComponent<BaseExplorerBehavior>();
            if (associatedBehavior == null || associatedBehavior.IsOutOfHealth()) return;
            if (IsAttacking() && Time.time - lastCollisionTime > collisionDebounceTime) {
                associatedBehavior.photonView.RPC("TakeDamage", PhotonTargets.All, currentSpeed);
                this.currentSpeed *= -1;
                lastCollisionTime = Time.time;
            }
        }

        public void OnTriggerStay2D(Collider2D other) {
            if (Time.time - lastCollisionTime > collisionDebounceTime) {
                OnTriggerEnter2D(other);
            }
        }

        protected override float GetCurrentAcceleration() {
            return baseAcceleration + (upgradeAccelerationFactor * (float) NumUpgrades());
        }
    }
}
