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

        public override void ActionSecondaryPressed(Vector3 mouseDirection) {
            base.ActionSecondaryPressed(mouseDirection);
            LaunchIceBall(mouseDirection);
        }

        public void LaunchIceBall(Vector3 mouseDirection) {
            if (photonView.isMine) {    
                float usableAttackCooldown = HasPowerup(Powerup.HALF_ABILITY_COOLDOWN) ? fireballCooldown / 2f : fireballCooldown;
                if (Time.time - fireballTime < usableAttackCooldown) return;
                fireballTime = Time.time;
                IceBallBehavior iceBall = PhotonNetwork.Instantiate(
                    fireballPrefab.name, new Vector3(transform.position.x, transform.position.y + 0.5f), Quaternion.identity, 0)
                    .GetComponent<IceBallBehavior>();
                    Vector3 direction = mouseDirection.magnitude == 0f ? currentControllerState : mouseDirection;
                iceBall.SetStartingDirection(direction, fireballSpeed + (fireballUpgradeSpeed * (float) NumUpgrades));
                iceBall.CryoLauncherBehavior = this;
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
