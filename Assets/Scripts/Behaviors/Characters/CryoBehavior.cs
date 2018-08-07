using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Tempest.Nightmare {

    public class CryoBehavior : BaseNightmare {

        public float fireballSpeed = 20f;
        public float fireballUpgradeSpeed = 2f;
        public float fireballAttackAnimation = 0.5f;
        public float fireballCooldown = 1f;

        public GameObject fireballPrefab;

        private float fireballTime;

        protected override void Flip() {
            // Do nothing here to prevent the base behavior from manually flipping our sprite.
        }

        protected override void HandleAnimator() {
            base.HandleAnimator();
            animator.SetBool("IsAttacking", IsAttacking());
        }

        public bool IsAttacking() {
            return Time.time - fireballTime < fireballAttackAnimation;
        }

        public override void ActionPrimaryPressed() {
            base.ActionPrimaryPressed();
            float usableAttackCooldown = HasPowerup(Powerup.HALF_ABILITY_COOLDOWN) ? fireballCooldown / 2f : fireballCooldown;
            if (Time.time - fireballTime < usableAttackCooldown) return;
            fireballTime = Time.time;
            LaunchIceBall();
        }

        public void LaunchIceBall() {
            if (photonView.isMine) {
                IceBallBehavior iceBall = PhotonNetwork.Instantiate(
                    fireballPrefab.name, new Vector3(transform.position.x, transform.position.y + 0.5f), Quaternion.identity, 0)
                    .GetComponent<IceBallBehavior>();
                iceBall.SetStartingDirection(currentControllerState, fireballSpeed + (fireballUpgradeSpeed * (float) NumUpgrades));
            }
        }

        // Override this to remove perfect acceleration powerup.
        protected override Powerup[] GetUsablePowerups() {
            return new Powerup[] { Powerup.BETTER_VISION, Powerup.DREAMER_VISION, Powerup.HALF_ABILITY_COOLDOWN };
        }

        protected override bool IsFlyer() {
            return true;
        }
    }
}
