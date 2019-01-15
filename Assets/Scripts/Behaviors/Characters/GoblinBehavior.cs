using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Tempest.Nightmare {

    public class GoblinBehavior : BaseNightmare {
        
        public float collisionDebounceTime = 1f;
        public float upgradeSpeedFactor = 0.2f;

        private bool hasUsedDash;
        private float lastCollisionTime;

        public override void Update() {
            base.Update();
            switch (currentState) {
                case MovementState.GROUNDED:
                case MovementState.WALL_SLIDE_LEFT:
                case MovementState.WALL_SLIDE_RIGHT:
                    hasUsedDash = false;
                    break;
            }
        }

        protected override void HandleAnimator() {
            base.HandleAnimator();
            animator.SetBool("IsAttacking", currentState == MovementState.DASHING);
        }

        public override void ActionPrimaryPressed() {
            base.ActionPrimaryPressed();
            switch (currentState) {
                case MovementState.GROUNDED:
                case MovementState.WALL_SLIDE_LEFT:
                case MovementState.WALL_SLIDE_RIGHT:
                    JumpPhysics(false);
                    break;
            }
        }

        public override void ActionSecondaryPressed(Vector3 mouseDirection) {
            base.ActionSecondaryPressed(mouseDirection);
            if (!hasUsedDash && DashPhysics(mouseDirection)) {
                hasUsedDash = true;
            }
        }

        public void OnTriggerEnter2D(Collider2D other) {
            if (!photonView.isMine) return;
            BaseExplorer associatedBehavior = other.gameObject.GetComponent<BaseExplorer>();
            if (associatedBehavior == null || associatedBehavior.IsOutOfHealth()) return;
            if (currentState == MovementState.DASHING && Time.time - lastCollisionTime > collisionDebounceTime) {
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

        protected override float MaxSpeed() {
            return base.MaxSpeed() + (upgradeSpeedFactor * GetNumUpgrades());
        }

        // Override this to remove perfect acceleration powerup.
        protected override Powerup[] GetUsablePowerups() {
            return new Powerup[] { Powerup.BETTER_VISION, Powerup.DREAMER_VISION };
        }

        protected override bool IsFlyer() {
            return false;
        }
        
        public override void SendTalentsToNetwork() {
            int sightRange = talentManager.GetTalentLevel(TalentManagerBehavior.GOBLIN_PREFIX + TalentManagerBehavior.SIGHT_RANGE);
            int chestDuration = talentManager.GetTalentLevel(TalentManagerBehavior.GOBLIN_PREFIX + TalentManagerBehavior.CHEST_DURATION);
            int cooldownReduction = talentManager.GetTalentLevel(TalentManagerBehavior.GOBLIN_PREFIX + TalentManagerBehavior.COOLDOWN_REDUCTION);
            int upgradeModifier = talentManager.GetTalentLevel(TalentManagerBehavior.GOBLIN_PREFIX + TalentManagerBehavior.UPGRADES);
            int accelerationModifier = 0;
            int jumpHeight = talentManager.GetTalentLevel(TalentManagerBehavior.GOBLIN_PREFIX + TalentManagerBehavior.JUMP_HEIGHT);
            int movementSpeed = talentManager.GetTalentLevel(TalentManagerBehavior.GOBLIN_PREFIX + TalentManagerBehavior.MOVEMENT_SPEED);
            int wallReflection = 0;
            int projectileGravity = 0;
            int wallClimb = talentManager.GetTalentLevel(TalentManagerBehavior.WALL_CLIMB);
            photonView.RPC("ReceiveNightmareTalents", PhotonTargets.All, sightRange, chestDuration, cooldownReduction,
                    upgradeModifier, accelerationModifier, jumpHeight, movementSpeed, wallReflection, projectileGravity, wallClimb);
        }
    }
}
