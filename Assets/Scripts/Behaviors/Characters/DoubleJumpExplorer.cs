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
                    JumpPhysics(false);
                    break;
                case MovementState.JUMPING:
                case MovementState.FALLING:
                case MovementState.WALL_JUMP:
                    if (!usedSecondJump) {
                        JumpPhysics(true);
                        usedSecondJump = true;
                    } else if (!usedThirdJump && HasPowerup(Powerup.THIRD_JUMP)) {
                        JumpPhysics(true);
                        usedThirdJump = true;
                    }
                    break;
            }
        }

        protected override float JumpFactor() {
            return base.JumpFactor() * GetSigmoidUpgradeMultiplier(1f, 1.5f);
        }

        protected override float WallJumpFactor() {
            return base.WallJumpFactor() * GetSigmoidUpgradeMultiplier(1f, 1.5f);
        }

        protected override bool IsFlyer() {
            return false;
        }

        public override void SendTalentsToNetwork() {
            int sightRange = talentManager.GetTalentLevel(TalentManagerBehavior.DOUBLE_JUMP_PREFIX + TalentManagerBehavior.SIGHT_RANGE);
            int chestDuration = talentManager.GetTalentLevel(TalentManagerBehavior.DOUBLE_JUMP_PREFIX + TalentManagerBehavior.CHEST_DURATION);
            int bonfireSpeed = talentManager.GetTalentLevel(TalentManagerBehavior.DOUBLE_JUMP_PREFIX + TalentManagerBehavior.BONFIRE_SPEED);
            int upgradeModifier = talentManager.GetTalentLevel(TalentManagerBehavior.DOUBLE_JUMP_PREFIX + TalentManagerBehavior.UPGRADES);
            int jumpHeight = talentManager.GetTalentLevel(TalentManagerBehavior.DOUBLE_JUMP_PREFIX + TalentManagerBehavior.JUMP_HEIGHT);
            int movementSpeed = talentManager.GetTalentLevel(TalentManagerBehavior.DOUBLE_JUMP_PREFIX + TalentManagerBehavior.MOVEMENT_SPEED);
            int reducedGravity = talentManager.GetTalentLevel(TalentManagerBehavior.REDUCED_GRAVITY);
            int jetpackForce = 0;
            int resetDash = 0;
            photonView.RPC("ReceiveExplorerTalents", PhotonTargets.All, sightRange, chestDuration, bonfireSpeed, upgradeModifier, jumpHeight, movementSpeed, reducedGravity, jetpackForce, resetDash);
        }
    }
}
