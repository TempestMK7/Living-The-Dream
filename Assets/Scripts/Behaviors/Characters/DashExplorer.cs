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
    }
}
