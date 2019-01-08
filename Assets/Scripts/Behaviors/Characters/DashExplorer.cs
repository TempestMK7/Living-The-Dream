using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Com.Tempest.Nightmare {

    public class DashExplorer : BaseExplorer {

        public float upgradeDashFactor = 0.1f;

        private bool hasUsedDash;
        private bool hasUsedSecondDash;

        public override void Update() {
            base.Update();
            switch (currentState) {
                case MovementState.GROUNDED:
                    hasUsedDash = false;
                    hasUsedSecondDash = false;
                    break;
                case MovementState.WALL_SLIDE_LEFT:
                case MovementState.WALL_SLIDE_RIGHT:
                    if (GetResetDashOnWallSlide() != 0) {
                        hasUsedDash = false;
                    }
                    hasUsedSecondDash = false;
                    break;
            }
        }

        public override void ActionPrimaryPressed() {
            base.ActionPrimaryPressed();
            switch (currentState) {
                case MovementState.GROUNDED:
                case MovementState.WALL_SLIDE_LEFT:
                case MovementState.WALL_SLIDE_RIGHT:
                    JumpPhysics();
                    break;
            }
        }

        public override void ActionSecondaryPressed(Vector3 mouseDirection) {
            base.ActionSecondaryPressed(mouseDirection);
            if (!hasUsedDash && DashPhysics(mouseDirection)) {
                hasUsedDash = true;
            } else if (HasPowerup(Powerup.THIRD_JUMP) && !hasUsedSecondDash  && DashPhysics(mouseDirection)) {
                hasUsedSecondDash = true;
            }
        }

        protected override float DashFactor() {
            return base.DashFactor() + (upgradeDashFactor * NumUpgrades);
        }

        // Override this to remove perfect acceleration powerup.
        protected override Powerup[] GetUsablePowerups() {
            return new Powerup[] { Powerup.BETTER_VISION, Powerup.NIGHTMARE_VISION, Powerup.THIRD_JUMP, Powerup.DOUBLE_OBJECTIVE_SPEED };
        }

        protected override bool IsFlyer() {
            return false;
        }

		protected override int GetSightRange() {
            if (talentManager == null) return 0;
            return talentManager.GetTalentLevel(TalentManagerBehavior.DASH_PREFIX + TalentManagerBehavior.SIGHT_RANGE);
        }

		protected override int GetShrineDuration() {
            if (talentManager == null) return 0;
            return talentManager.GetTalentLevel(TalentManagerBehavior.DASH_PREFIX + TalentManagerBehavior.CHEST_DURATION);
        }

		protected override int GetBonfireSpeed() {
            if (talentManager == null) return 0;
            return talentManager.GetTalentLevel(TalentManagerBehavior.DASH_PREFIX + TalentManagerBehavior.BONFIRE_SPEED);
        }

		protected override int GetUpgradeModifier() {
            if (talentManager == null) return 0;
            return talentManager.GetTalentLevel(TalentManagerBehavior.DASH_PREFIX + TalentManagerBehavior.UPGRADES);
        }

		protected override int GetJumpHeight() {
            if (talentManager == null) return 0;
            return talentManager.GetTalentLevel(TalentManagerBehavior.DASH_PREFIX + TalentManagerBehavior.JUMP_HEIGHT);
        }

		protected override int GetMovementSpeed() {
            if (talentManager == null) return 0;
            return talentManager.GetTalentLevel(TalentManagerBehavior.DASH_PREFIX + TalentManagerBehavior.MOVEMENT_SPEED);
        }

		protected override int GetReducedGravity() {
            return 0;
        }

		protected override int GetJetpackForce() {
            return 0;
        }

		protected override int GetResetDashOnWallSlide() {
            if (talentManager == null) return 0;
            return talentManager.GetTalentLevel(TalentManagerBehavior.RESET_DASH);
        }
    }
}
