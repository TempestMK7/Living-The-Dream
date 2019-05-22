using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Com.Tempest.Nightmare {

    public class DashExplorer : BaseExplorer {

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
                    if (networkResetDash != 0) {
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
                    JumpPhysics(false);
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
            return base.DashFactor() * GetSigmoidUpgradeMultiplier(1f, 2f);
        }

        // Override this to remove perfect acceleration powerup.
        protected override Powerup[] GetUsablePowerups() {
            return new Powerup[] { Powerup.BETTER_VISION, Powerup.NIGHTMARE_VISION, Powerup.THIRD_JUMP, Powerup.DOUBLE_OBJECTIVE_SPEED };
        }

        protected override bool IsFlyer() {
            return false;
        }

        public override void SendTalentsToNetwork() {
            int sightRange = talentManager.GetTalentLevel(TalentManagerBehavior.DASH_PREFIX + TalentManagerBehavior.SIGHT_RANGE);
            int chestDuration = talentManager.GetTalentLevel(TalentManagerBehavior.DASH_PREFIX + TalentManagerBehavior.CHEST_DURATION);
            int bonfireSpeed = talentManager.GetTalentLevel(TalentManagerBehavior.DASH_PREFIX + TalentManagerBehavior.BONFIRE_SPEED);
            int upgradeModifier = talentManager.GetTalentLevel(TalentManagerBehavior.DASH_PREFIX + TalentManagerBehavior.UPGRADES);
            int jumpHeight = talentManager.GetTalentLevel(TalentManagerBehavior.DASH_PREFIX + TalentManagerBehavior.JUMP_HEIGHT);
            int movementSpeed = talentManager.GetTalentLevel(TalentManagerBehavior.DASH_PREFIX + TalentManagerBehavior.MOVEMENT_SPEED);
            int reducedGravity = 0;
            int jetpackForce = 0;
            int resetDash = talentManager.GetTalentLevel(TalentManagerBehavior.RESET_DASH);
            photonView.RPC("ReceiveExplorerTalents", PhotonTargets.All, sightRange, chestDuration, bonfireSpeed, upgradeModifier, jumpHeight, movementSpeed, reducedGravity, jetpackForce, resetDash);
        }
    }
}
