using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Tempest.Nightmare {

    public class DoubleJumpExplorer : BaseExplorerBehavior {

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
            if (Time.time - jumpTime < jumpRecovery ||
                Time.time - damageTime < damageRecovery ||
                Time.time - deathEventTime < deathAnimationTime) {
                return;
            }

            if (grounded) {
                currentSpeed.y = maxSpeed * jumpFactor;
                jumpTime = Time.time;
            } else if (holdingWallLeft) {
                currentSpeed.y = Mathf.Sin(Mathf.PI / 4) * maxSpeed * wallJumpFactor;
                currentSpeed.x = Mathf.Cos(Mathf.PI / 4) * maxSpeed * wallJumpFactor;
                jumpTime = Time.time;
                wallJumpTime = Time.time;
                holdingWallLeft = false;
            } else if (holdingWallRight) {
                currentSpeed.y = Mathf.Sin(Mathf.PI * 3 / 4) * maxSpeed * wallJumpFactor;
                currentSpeed.x = Mathf.Cos(Mathf.PI * 3 / 4) * maxSpeed * wallJumpFactor;
                jumpTime = Time.time;
                wallJumpTime = Time.time;
                holdingWallRight = false;
            } else if (!usedSecondJump) {
                currentSpeed.y = maxSpeed * jumpFactor * 0.9f;
                jumpTime = Time.time;
                usedSecondJump = true;
            } else if (!usedThirdJump && HasPowerup(Powerup.THIRD_JUMP)) {
                currentSpeed.y = maxSpeed * jumpFactor * 0.9f;
                jumpTime = Time.time;
                usedThirdJump = true;
            }
        }

        public override void ActionReleased() {
            // ignored callback.
        }
    }
}
