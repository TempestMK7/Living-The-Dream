﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Com.Tempest.Nightmare {

    public class JetpackExplorer : BaseExplorer {

        public float jetpackVelocityFactor = 2f;
        public float maxJetpackTime = 1f;
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
            if (jetpackOn && currentState != MovementState.HIT_FREEZE && currentState != MovementState.HIT_STUN && currentState != MovementState.RAG_DOLL) {
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
            } else if (currentState == MovementState.WALL_SLIDE_LEFT || currentState == MovementState.WALL_SLIDE_RIGHT) {
                float wallResetRank = GetTalentRank(TalentEnum.RESET_MOVEMENT_ON_WALL_SLIDE);
                float wallSlideMultiplier = 1f + wallResetRank;
                jetpackTimeRemaining += Time.deltaTime * GetSigmoidUpgradeMultiplier(1f, 2f) * wallSlideMultiplier;
            } else if (currentState == MovementState.GROUNDED) {
                jetpackTimeRemaining += Time.deltaTime * GetSigmoidUpgradeMultiplier(1f, 2f);
            } else {
                jetpackTimeRemaining += Time.deltaTime * GetSigmoidUpgradeMultiplier(1f, 2f) * aerialJetpackReloadFactor;
            }
            jetpackTimeRemaining = Mathf.Min(jetpackTimeRemaining, UpgradedMaxJetpackTime());
        }

        private void HandleFuelBar() {
            positiveFuelImage.fillAmount = jetpackTimeRemaining / UpgradedMaxJetpackTime();
            fuelBarCanvas.SetActive(photonView.isMine && jetpackTimeRemaining != UpgradedMaxJetpackTime());
        }

        public override void ActionPrimaryPressed(Vector3 mouseDirection) {
            base.ActionPrimaryPressed(mouseDirection);
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
            return maxJetpackTime * GetSigmoidUpgradeMultiplier(1f, 2f);
        }

        protected override bool IsFlyer() {
            return false;
        }

        protected override void LoadTalents() {
            talentRanks = GlobalTalentContainer.GetInstance().JetpackTalents;
        }
    }
}
