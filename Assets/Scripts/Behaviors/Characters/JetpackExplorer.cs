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
            if (jetpackOn && (currentState == MovementState.JUMPING ||
                    currentState == MovementState.FALLING ||
                    currentState == MovementState.WALL_JUMP)) {
                if (currentSpeed.y <= 0f) {
                    currentSpeed.y += maxSpeed * gravityFactor * jetpackVelocityFactor * Time.deltaTime * fallingJetpackForceFactor;
                } else {
                    currentSpeed.y += maxSpeed * gravityFactor * jetpackVelocityFactor * Time.deltaTime;
                }
                currentSpeed.y = Mathf.Min(currentSpeed.y, maxSpeed * terminalVelocityFactor);
                jetpackTimeRemaining -= HasPowerup(Powerup.THIRD_JUMP) ? Time.deltaTime : Time.deltaTime * 2f;
                if (jetpackTimeRemaining <= 0f) {
                    jetpackTimeRemaining = 0f;
                    jetpackOn = false;
                }
            } else {
                jetpackTimeRemaining += Time.deltaTime;
                jetpackTimeRemaining = Mathf.Min(jetpackTimeRemaining, UpgradedMaxJetpackTime());
            }
        }

        private void HandleFuelBar() {
            positiveFuelImage.fillAmount = jetpackTimeRemaining / UpgradedMaxJetpackTime();
            fuelBarCanvas.SetActive(photonView.isMine && jetpackTimeRemaining != UpgradedMaxJetpackTime());
        }

        public override void ActionPressed() {
            base.ActionPressed();

            switch (currentState) {
                case MovementState.GROUNDED:
                case MovementState.WALL_SLIDE_LEFT:
                case MovementState.WALL_SLIDE_RIGHT:
                    JumpPhysics();
                    break;
                case MovementState.JUMPING:
                case MovementState.FALLING:
                case MovementState.WALL_JUMP:
                    jetpackOn = true;
                    break;
            }
        }

        public override void ActionReleased() {
            base.ActionReleased();
            jetpackOn = false;
        }

        private float UpgradedMaxJetpackTime() {
            return maxJetpackTime + (jetpackTimeUpgradeMod * (float) NumUpgrades);
        }

        protected override bool IsFlyer() {
            return false;
        }
    }
}
