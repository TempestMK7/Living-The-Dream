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

        private AudioSource launchSource;
        private float fireballTime;

        public override void Awake() {
            base.Awake();
            launchSource = GetComponent<AudioSource>();
            launchSource.volume = ControlBindingContainer.GetInstance().effectVolume;
        }

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
            float usableAttackCooldown = HasPowerup(Powerup.HALF_ABILITY_COOLDOWN) ? fireballCooldown / 2f : fireballCooldown;
            usableAttackCooldown *= 1.0f - (networkCooldownReduction * 0.05f);
            if (Time.time - fireballTime < usableAttackCooldown) return;
            if (photonView.isMine) {
                fireballTime = Time.time;
                IceBallBehavior iceBall = PhotonNetwork.Instantiate(
                    fireballPrefab.name, new Vector3(transform.position.x, transform.position.y + 0.5f), Quaternion.identity, 0)
                    .GetComponent<IceBallBehavior>();
                    Vector3 direction = mouseDirection.magnitude == 0f ? currentControllerState : mouseDirection;
                iceBall.SetStartingDirection(direction, fireballSpeed + (fireballUpgradeSpeed * GetNumUpgrades()), networkProjectileGravity == 0);
                iceBall.CryoLauncherBehavior = this;
                launchSource.Play();
                photonView.RPC("PlayLaunchSound", PhotonTargets.Others);
            }
        }

        [PunRPC]
        public void PlayLaunchSound() {
            launchSource.Play();
        }

        // Override this to remove perfect acceleration powerup.
        protected override Powerup[] GetUsablePowerups() {
            return new Powerup[] { Powerup.BETTER_VISION, Powerup.DREAMER_VISION, Powerup.HALF_ABILITY_COOLDOWN };
        }

        protected override bool IsFlyer() {
            return true;
        }

        public override void SendTalentsToNetwork() {
            int sightRange = talentManager.GetTalentLevel(TalentManagerBehavior.CRYO_PREFIX + TalentManagerBehavior.SIGHT_RANGE);
            int chestDuration = talentManager.GetTalentLevel(TalentManagerBehavior.CRYO_PREFIX + TalentManagerBehavior.CHEST_DURATION);
            int cooldownReduction = talentManager.GetTalentLevel(TalentManagerBehavior.CRYO_PREFIX + TalentManagerBehavior.COOLDOWN_REDUCTION);
            int upgradeModifier = talentManager.GetTalentLevel(TalentManagerBehavior.CRYO_PREFIX + TalentManagerBehavior.UPGRADES);
            int accelerationModifier = talentManager.GetTalentLevel(TalentManagerBehavior.CRYO_PREFIX + TalentManagerBehavior.ACCELERATION);
            int jumpHeight = 0;
            int movementSpeed = talentManager.GetTalentLevel(TalentManagerBehavior.CRYO_PREFIX + TalentManagerBehavior.MOVEMENT_SPEED);
            int wallReflection = 0;
            int projectileGravity = talentManager.GetTalentLevel(TalentManagerBehavior.CANCEL_PROJECTILE_GRAVITY);
            int wallClimb = 0;
            photonView.RPC("ReceiveNightmareTalents", PhotonTargets.All, sightRange, chestDuration, cooldownReduction,
                    upgradeModifier, accelerationModifier, jumpHeight, movementSpeed, wallReflection, projectileGravity, wallClimb);
        }
    }
}
