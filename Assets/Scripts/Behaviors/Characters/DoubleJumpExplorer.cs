using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Tempest.Nightmare {

    public class DoubleJumpExplorer : BaseExplorerBehavior {

        public float jumpFactorUpgradeModifier = 0.1f;
        private bool usedSecondJump;
        private bool usedThirdJump;

        public override void BecameGrounded() {
            usedSecondJump = false;
            usedThirdJump = false;
        }

        public override void GrabbedWall(bool grabbedLeft) {
            usedThirdJump = false;
        }

        public override void InputsReceived(float horizontalScale, float verticalScale, bool grabHeld) {
            currentControllerState = new Vector3(horizontalScale, verticalScale);
            this.grabHeld = grabHeld;
        }

        public override void ActionPressed() {
            // If we just jumped, got hit, or are in the death animation, ignore this action.
            if (Time.time - damageTime < damageRecovery ||
                Time.time - deathEventTime < deathAnimationTime) {
                return;
            }

            if (grounded) {
                currentSpeed.y = maxSpeed * JumpFactor();
                jumpTime = Time.time;
            } else if (holdingWallLeft) {
                currentSpeed.y = Mathf.Sin(Mathf.PI / 4) * maxSpeed * WallJumpFactor();
                currentSpeed.x = Mathf.Cos(Mathf.PI / 4) * maxSpeed * WallJumpFactor();
                jumpTime = Time.time;
                wallJumpTime = Time.time;
                holdingWallLeft = false;
            } else if (holdingWallRight) {
                currentSpeed.y = Mathf.Sin(Mathf.PI * 3 / 4) * maxSpeed * WallJumpFactor();
                currentSpeed.x = Mathf.Cos(Mathf.PI * 3 / 4) * maxSpeed * WallJumpFactor();
                jumpTime = Time.time;
                wallJumpTime = Time.time;
                holdingWallRight = false;
            } else if (!usedSecondJump) {
                currentSpeed.y = maxSpeed * JumpFactor() * 0.9f;
                jumpTime = Time.time;
                usedSecondJump = true;
            } else if (!usedThirdJump && HasPowerup(Powerup.THIRD_JUMP)) {
                currentSpeed.y = maxSpeed * JumpFactor() * 0.9f;
                jumpTime = Time.time;
                usedThirdJump = true;
            }
        }

        public override void ActionReleased() {
            // ignored callback.
        }

        private float JumpFactor() {
            return jumpFactor + ((float) NumUpgrades * jumpFactorUpgradeModifier);
        }

        private float WallJumpFactor() {
            return wallJumpFactor + ((float) NumUpgrades * jumpFactorUpgradeModifier);
        }
    }
}
