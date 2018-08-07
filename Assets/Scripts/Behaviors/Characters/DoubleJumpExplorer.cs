using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Tempest.Nightmare {

    public class DoubleJumpExplorer : BaseExplorer {

        private bool usedSecondJump;
        private bool usedThirdJump;

        public override void Update() {
            base.Update();
            if (currentState == MovementState.GROUNDED) {
                usedSecondJump = false;
                usedThirdJump = false;
            } else if (currentState == MovementState.WALL_SLIDE_LEFT || currentState == MovementState.WALL_SLIDE_RIGHT) {
                usedThirdJump = false;
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
                case MovementState.JUMPING:
                case MovementState.FALLING:
                case MovementState.WALL_JUMP:
                    if (!usedSecondJump) {
                        JumpPhysics();
                        usedSecondJump = true;
                    } else if (!usedThirdJump && HasPowerup(Powerup.THIRD_JUMP)) {
                        JumpPhysics();
                        usedThirdJump = true;
                    }
                    break;
            }
        }

        protected override float JumpFactor() {
            return base.JumpFactor() + ((float) NumUpgrades * jumpFactorUpgradeModifier);
        }

        protected override float WallJumpFactor() {
            return base.WallJumpFactor() + ((float) NumUpgrades * jumpFactorUpgradeModifier);
        }

        protected override bool IsFlyer() {
            return false;
        }
    }
}
