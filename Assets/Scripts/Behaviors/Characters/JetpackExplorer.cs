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
                float jetpackForce = (networkJetpackForce * 0.4f) + 1.0f;
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
                    JumpPhysics(false);
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
            return maxJetpackTime + (jetpackTimeUpgradeMod * GetNumUpgrades());
        }

        protected override bool IsFlyer() {
            return false;
        }

        public override void SendTalentsToNetwork() {
            int sightRange = talentManager.GetTalentLevel(TalentManagerBehavior.JETPACK_PREFIX + TalentManagerBehavior.SIGHT_RANGE);
            int chestDuration = talentManager.GetTalentLevel(TalentManagerBehavior.JETPACK_PREFIX + TalentManagerBehavior.CHEST_DURATION);
            int bonfireSpeed = talentManager.GetTalentLevel(TalentManagerBehavior.JETPACK_PREFIX + TalentManagerBehavior.BONFIRE_SPEED);
            int upgradeModifier = talentManager.GetTalentLevel(TalentManagerBehavior.JETPACK_PREFIX + TalentManagerBehavior.UPGRADES);
            int jumpHeight = talentManager.GetTalentLevel(TalentManagerBehavior.JETPACK_PREFIX + TalentManagerBehavior.JUMP_HEIGHT);
            int movementSpeed = talentManager.GetTalentLevel(TalentManagerBehavior.JETPACK_PREFIX + TalentManagerBehavior.MOVEMENT_SPEED);
            int reducedGravity = 0;
            int jetpackForce = talentManager.GetTalentLevel(TalentManagerBehavior.INCREASED_JETPACK_FORCE);
            int resetDash = 0;
            photonView.RPC("ReceiveExplorerTalents", PhotonTargets.All, sightRange, chestDuration, bonfireSpeed, upgradeModifier, jumpHeight, movementSpeed, reducedGravity, jetpackForce, resetDash);
        }
    }
}
