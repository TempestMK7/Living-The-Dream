using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Tempest.Nightmare {

    public class GhastBehavior : BaseNightmare {

        public float attackDuration = 0.3f;
        public float attackCooldown = 0.7f;
        public float attackRadius = 0.1f;
        public float attackHorizontalLaunchFactor = 10f;
        public float attackVerticalLaunchFactor = 10f;
        public int attackDamage = 20;
        public float attackFreezeTime = 0.2f;
        public float attackStunTime = 0.5f;

        public GameObject attackPrefab;

        public float collisionDebounceTime = 1f;
        public int dashDamage = 40;
        public float dashFreezeTime = 0.2f;
        public float dashStunTime = 0.6f;

        private float lastAttackTime;
        private float lastCollisionTime;
        private GhastAttackBehavior currentAttack;

        protected override void HandleAnimator() {
            base.HandleAnimator();
            animator.SetBool("IsAttacking", IsAttacking());
        }

        public override void ActionPrimaryPressed(Vector3 mouseDirection) {
            base.ActionPrimaryPressed(mouseDirection);
            if (IsAttacking()) return;
            if (Time.time - lastAttackTime < attackCooldown) return;
            lastAttackTime = Time.time;
            bool attackingRight;
            if (mouseDirection.magnitude != 0f) {
                attackingRight = mouseDirection.x >= 0;
            } else {
                attackingRight = currentControllerState.x >= 0;
            }
            Vector3 attackAngle = new Vector3(attackingRight ? 0.8f : -0.8f, 0);
            Vector3 attackLaunch = new Vector3(attackingRight ? attackHorizontalLaunchFactor : attackHorizontalLaunchFactor * -1f, attackVerticalLaunchFactor);
            Vector3 attackPosition = transform.position + attackAngle;
            currentAttack = PhotonNetwork.Instantiate(attackPrefab.name, attackPosition, Quaternion.identity, 0)
                .GetComponent<GhastAttackBehavior>();
            currentAttack.Initialize(attackDuration, attackAngle, attackRadius, attackLaunch, attackDamage, attackFreezeTime, attackStunTime);
            currentAttack.photonView.RPC("Initialize", PhotonTargets.Others, attackDuration, attackAngle, attackRadius, attackLaunch, attackDamage, attackFreezeTime, attackStunTime);
            currentAttack.ParentGhast = this;
        }

        public override void ActionSecondaryPressed(Vector3 mouseDirection) {
            base.ActionSecondaryPressed(mouseDirection);
            if (IsAttacking()) return;
            DashPhysics(mouseDirection);
        }

        protected override float DashCooldown() {
            return HasPowerup(Powerup.HALF_ABILITY_COOLDOWN) ? base.DashCooldown() / 2f : base.DashCooldown();
        }

        public bool IsAttacking() {
            return currentState == MovementState.DASHING || Time.time - lastAttackTime < attackDuration;
        }

        public void OnTriggerEnter2D(Collider2D other) {
            if (!photonView.isMine) return;
            BaseExplorer associatedBehavior = other.gameObject.GetComponent<BaseExplorer>();
            if (associatedBehavior == null || associatedBehavior.ImmuneToDamage()) return;
            if (currentState == MovementState.DASHING && Time.time - lastCollisionTime > collisionDebounceTime) {
                associatedBehavior.photonView.RPC("OnDamageTaken", PhotonTargets.All, associatedBehavior.transform.position, currentSpeed, dashDamage, dashFreezeTime, dashStunTime);
                this.currentSpeed *= 0.8f;
                lastCollisionTime = Time.time;
                photonView.RPC("ReceiveObjectiveEmbers", PhotonTargets.All, 10f);
            }
        }

        public void OnTriggerStay2D(Collider2D other) {
            if (Time.time - lastCollisionTime > collisionDebounceTime) {
                OnTriggerEnter2D(other);
            }
        }

        protected override float GetCurrentAcceleration() {
            return base.GetCurrentAcceleration() * GetSigmoidUpgradeMultiplier(1f, 2.5f);
        }

        protected override bool IsFlyer() {
            return true;
        }

        protected override void LoadTalents() {
            talentRanks = GlobalTalentContainer.GetInstance().GhastTalents;
        }
    }
}
