using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Tempest.Nightmare {

    public class GhastBehavior : BaseNightmare {

        public float collisionDebounceTime = 1f;
        public float upgradeAccelerationFactor = 0.03f;
    
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
            return base.GetCurrentAcceleration() + (upgradeAccelerationFactor * GetNumUpgrades());
        }

        protected override bool IsFlyer() {
            return true;
        }
        
        public override void SendTalentsToNetwork() {
            int sightRange = talentManager.GetTalentLevel(TalentManagerBehavior.GHAST_PREFIX + TalentManagerBehavior.SIGHT_RANGE);
            int chestDuration = talentManager.GetTalentLevel(TalentManagerBehavior.GHAST_PREFIX + TalentManagerBehavior.CHEST_DURATION);
            int cooldownReduction = talentManager.GetTalentLevel(TalentManagerBehavior.GHAST_PREFIX + TalentManagerBehavior.COOLDOWN_REDUCTION);
            int upgradeModifier = talentManager.GetTalentLevel(TalentManagerBehavior.GHAST_PREFIX + TalentManagerBehavior.UPGRADES);
            int accelerationModifier = talentManager.GetTalentLevel(TalentManagerBehavior.GHAST_PREFIX + TalentManagerBehavior.ACCELERATION);
            int jumpHeight = 0;
            int movementSpeed = talentManager.GetTalentLevel(TalentManagerBehavior.GHAST_PREFIX + TalentManagerBehavior.MOVEMENT_SPEED);
            int wallReflection = talentManager.GetTalentLevel(TalentManagerBehavior.CANCEL_WALL_REFLECTION);
            int projectileGravity = 0;
            int wallClimb = 0;
            photonView.RPC("ReceiveNightmareTalents", PhotonTargets.All, sightRange, chestDuration, cooldownReduction,
                    upgradeModifier, accelerationModifier, jumpHeight, movementSpeed, wallReflection, projectileGravity, wallClimb);
        }
    }
}
