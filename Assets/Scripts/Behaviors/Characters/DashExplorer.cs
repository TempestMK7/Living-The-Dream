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
                case MovementState.WALL_SLIDE_LEFT:
                case MovementState.WALL_SLIDE_RIGHT:
                    hasUsedDash = false;
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

        public override void ActionSecondaryPressed() {
            switch (currentState) {
                case MovementState.GROUNDED:
                case MovementState.WALL_SLIDE_LEFT:
                case MovementState.WALL_SLIDE_RIGHT:
                case MovementState.JUMPING:
                case MovementState.FALLING:
                case MovementState.WALL_JUMP:
                    if (!hasUsedDash) {
                        DashPhysics();
                        hasUsedDash = true;
                    } else if (HasPowerup(Powerup.THIRD_JUMP) && !hasUsedSecondDash) {
                        DashPhysics();
                        hasUsedSecondDash = true;
                    }
                    break;
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
