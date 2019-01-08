using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Com.Tempest.Nightmare {

    public class JetpackExplorer : BaseExplorer {

        public float jetpackVelocityFactor = 2f;
        public float maxJetpackTime = 1f;
        public float jetpackTimeUpgradeMod = 0.1f;
        public float fallingJetpackForceFactor = 1.5f;
        public float aerialJetpackReloadFactor = 0.2f;

        private GameObject fuelBarCanvas;
        private Image positiveFuelImage;

        private float jetpackTimeRemaining;
        private bool jetpackOn;

        public override void Awake() {
            base.Awake();
            fuelBarCanvas = transform.Find("FuelCanvas").gameObject;
            positiveFuelImage = fuelBarCanvas.transform.Find("PositiveFuel").GetComponent<Image>();
        }

        public override void Update() {
            base.Update();
            HandleFuelBar();
        }

        protected override void HandleVerticalMovementGravityBound() {
            base.HandleVerticalMovementGravityBound();
            if (jetpackOn && currentState != MovementState.DAMAGED && currentState != MovementState.DYING) {
                float jetpackForce = (GetJetpackForce() * 0.4f) + 1.0f;
                if (currentSpeed.y <= 0f) {
                    currentSpeed.y += maxSpeed * gravityFactor * jetpackVelocityFactor * jetpackForce * Time.deltaTime * fallingJetpackForceFactor;
                } else {
                    currentSpeed.y += maxSpeed * gravityFactor * jetpackVelocityFactor * jetpackForce * Time.deltaTime;
                }
                currentSpeed.y = Mathf.Min(currentSpeed.y, maxSpeed * terminalVelocityFactor * jetpackForce);
                jetpackTimeRemaining -= HasPowerup(Powerup.THIRD_JUMP) ? Time.deltaTime : Time.deltaTime * 2f;
                if (jetpackTimeRemaining <= 0f) {
                    jetpackTimeRemaining = 0f;
                    jetpackOn = false;
                }
            } else if (currentState == MovementState.GROUNDED || currentState == MovementState.WALL_SLIDE_LEFT || currentState == MovementState.WALL_SLIDE_RIGHT) {
                jetpackTimeRemaining += Time.deltaTime;
            } else {
                jetpackTimeRemaining += Time.deltaTime * aerialJetpackReloadFactor;
            }
            jetpackTimeRemaining = Mathf.Min(jetpackTimeRemaining, UpgradedMaxJetpackTime());
        }

        private void HandleFuelBar() {
            positiveFuelImage.fillAmount = jetpackTimeRemaining / UpgradedMaxJetpackTime();
            fuelBarCanvas.SetActive(photonView.isMine && jetpackTimeRemaining != UpgradedMaxJetpackTime());
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
            jetpackOn = true;
        }

        public override void ActionSecondaryReleased() {
            base.ActionSecondaryReleased();
            jetpackOn = false;
        }

        private float UpgradedMaxJetpackTime() {
            return maxJetpackTime + (jetpackTimeUpgradeMod * (float) NumUpgrades);
        }

        protected override bool IsFlyer() {
            return false;
        }

		protected override int GetSightRange() {
            if (talentManager == null) return 0;
            return talentManager.GetTalentLevel(TalentManagerBehavior.JETPACK_PREFIX + TalentManagerBehavior.SIGHT_RANGE);
        }

		protected override int GetShrineDuration() {
            if (talentManager == null) return 0;
            return talentManager.GetTalentLevel(TalentManagerBehavior.JETPACK_PREFIX + TalentManagerBehavior.CHEST_DURATION);
        }

		protected override int GetBonfireSpeed() {
            if (talentManager == null) return 0;
            return talentManager.GetTalentLevel(TalentManagerBehavior.JETPACK_PREFIX + TalentManagerBehavior.BONFIRE_SPEED);
        }

		protected override int GetUpgradeModifier() {
            if (talentManager == null) return 0;
            return talentManager.GetTalentLevel(TalentManagerBehavior.JETPACK_PREFIX + TalentManagerBehavior.UPGRADES);
        }

		protected override int GetJumpHeight() {
            if (talentManager == null) return 0;
            return talentManager.GetTalentLevel(TalentManagerBehavior.JETPACK_PREFIX + TalentManagerBehavior.JUMP_HEIGHT);
        }

		protected override int GetMovementSpeed() {
            if (talentManager == null) return 0;
            return talentManager.GetTalentLevel(TalentManagerBehavior.JETPACK_PREFIX + TalentManagerBehavior.MOVEMENT_SPEED);
        }

		protected override int GetReducedGravity() {
            return 0;
        }

		protected override int GetJetpackForce() {
            if (talentManager == null) return 0;
            return talentManager.GetTalentLevel(TalentManagerBehavior.INCREASED_JETPACK_FORCE);
        }

		protected override int GetResetDashOnWallSlide() {
            return 0;
        }
    }
}
