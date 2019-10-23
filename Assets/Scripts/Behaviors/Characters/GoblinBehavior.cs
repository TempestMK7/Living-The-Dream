using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Tempest.Nightmare {

    public class GoblinBehavior : BaseNightmare {
        
        public float collisionDebounceTime = 1f;

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

        protected override float DashFactor() {
            return base.DashFactor() * GetSigmoidUpgradeMultiplier(1f, 2f);
        }

        // Override this to remove perfect acceleration powerup.
        protected override Powerup[] GetUsablePowerups() {
            return new Powerup[] { Powerup.BETTER_VISION, Powerup.DREAMER_VISION };
        }

        protected override bool IsFlyer() {
            return false;
        }

        protected override void LoadTalents() {
            talentRanks = GlobalTalentContainer.GetInstance().GoblinTalents;
        }
    }
}
