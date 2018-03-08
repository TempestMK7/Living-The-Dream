using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Tempest.Nightmare {
    
    public class NightmareBehavior : EmpowerableCharacterBehavior, IPunObservable, IControllable {

        public float maxSpeed = 10f;
        public float accelerationFactor = 0.5f;
        public float snapToMaxThresholdFactor = 0.1f;
        public float bounceThreshold = 1.0f;

        public float dashFactor = 2f;
        public float dashDuration = 0.1f;
        public float dashDamageDuration = 0.5f;
        public float dashCooldown = 1f;
        public float collisionDebounceTime = 1f;

        public float rayBoundShrinkage = 0.001f;
        public int numRays = 4;

        public LayerMask whatIsSolid;
        public LayerMask whatIsPlayer;

        public float lightBoxScale = 25f;

        private LightBoxBehavior lightBox;

        private BoxCollider2D boxCollider;
        private Animator animator;

        private Vector3 currentSpeed;
        private Vector3 currentControllerState;

        private bool facingRight;
        private float acceleration;
        private float snapToMaxThreshold;
        private float dashSpeed;
        private float dashStart;
        private float lastCollisionTime;

        // Use this for initialization
        public override void Awake() {
            base.Awake();

            lightBox = GetComponentInChildren<LightBoxBehavior>();
            lightBox.IsMine = photonView.isMine;
            lightBox.IsActive = true;
            lightBox.DefaultScale = new Vector3(lightBoxScale, lightBoxScale);
            lightBox.ActiveScale = new Vector3(lightBoxScale, lightBoxScale);

            boxCollider = GetComponent<BoxCollider2D>();
            animator = GetComponent<Animator>();
            animator.SetBool("IsAttacking", false);
            currentSpeed = new Vector3();
            currentControllerState = new Vector3();
            acceleration = accelerationFactor * maxSpeed;
            snapToMaxThreshold = maxSpeed * snapToMaxThresholdFactor;
            dashSpeed = maxSpeed * dashFactor;
            dashStart = 0f;
            facingRight = false;
        }
	
	    // Update is called once per frame
	    public void Update () {
            UpdateCurrentSpeed();
            MoveAsFarAsYouCan();
            animator.SetBool("IsAttacking", IsAttacking());
	    }

        private void UpdateCurrentSpeed() {
            if (photonView.isMine && Time.time - dashStart > dashDuration) {
                Vector3 newMax = new Vector3(maxSpeed * currentControllerState.x, maxSpeed * currentControllerState.y);
                if (newMax.magnitude > maxSpeed) {
                    newMax *= maxSpeed / newMax.magnitude;
                }
                // This is how far we are from that speed.
                Vector3 difference = newMax - currentSpeed;
                float usableAcceleratior = HasPowerup(Powerup.PERFECT_ACCELERATION) ? maxSpeed : acceleration;
                if (Mathf.Abs(difference.x) > snapToMaxThreshold) {
                    difference.x *= usableAcceleratior * Time.deltaTime;
                }
                if (Mathf.Abs(difference.y) > snapToMaxThreshold) {
                    difference.y *= usableAcceleratior * Time.deltaTime;
                }
                currentSpeed += difference;
            }
        }

        private void MoveAsFarAsYouCan() {
            // Calculate how far we're going.
            Vector3 distanceForFrame = currentSpeed * Time.deltaTime;
            bool goingRight = distanceForFrame.x > 0;
            bool goingUp = distanceForFrame.y > 0;

            // Declare everything we need for the ray casting process.
            Bounds currentBounds = boxCollider.bounds;
            currentBounds.Expand(rayBoundShrinkage * -1f);
            Vector3 topLeft = new Vector3(currentBounds.min.x, currentBounds.max.y);
            Vector3 bottomLeft = currentBounds.min;
            Vector3 bottomRight = new Vector3(currentBounds.max.x, currentBounds.min.y);
            bool hitX = false;
            bool hitY = false;

            // Use raycasts to decide if we hit anything horizontally.
            if (distanceForFrame.x != 0) {
                float rayInterval = (topLeft.y - bottomLeft.y) / (float)numRays;
                Vector3 rayOriginBase = currentSpeed.x > 0 ? bottomRight : bottomLeft;
                float rayOriginCorrection = currentSpeed.x > 0 ? rayBoundShrinkage : rayBoundShrinkage * -1f;
                for (int x = 0; x <= numRays; x++) {
                    Vector3 rayOrigin = new Vector3(rayOriginBase.x + rayOriginCorrection, rayOriginBase.y + rayInterval * (float)x);
                    RaycastHit2D rayCast = Physics2D.Raycast(rayOrigin, goingRight ? Vector3.right : Vector3.left, Mathf.Abs(distanceForFrame.x), whatIsSolid);
                    if (rayCast) {
                        hitX = true;
                        distanceForFrame.x = rayCast.point.x - rayOrigin.x;
                    }
                    if (distanceForFrame.x == 0f) break;
                }
            }
            if (hitX) {
                if (Mathf.Abs(currentSpeed.x) > maxSpeed * bounceThreshold) {
                    currentSpeed.x *= -1f;
                } else {
                    currentSpeed.x = 0;
                }
            }

            // Use raycasts to decide if we hit anything vertically.
            if (distanceForFrame.y != 0) {
                float rayInterval = (bottomRight.x - bottomLeft.x) / (float)numRays;
                Vector3 rayOriginBase = currentSpeed.y > 0 ? topLeft : bottomLeft;
                float rayOriginCorrection = currentSpeed.y > 0 ? rayBoundShrinkage : rayBoundShrinkage * -1f;
                for (int x = 0; x <= numRays; x++) {
                    Vector3 rayOrigin = new Vector3(rayOriginBase.x + rayInterval * (float)x, rayOriginBase.y + rayOriginCorrection);
                    RaycastHit2D rayCast = Physics2D.Raycast(rayOrigin, distanceForFrame.y > 0 ? Vector3.up : Vector3.down, Mathf.Abs(distanceForFrame.y), whatIsSolid);
                    if (rayCast) {
                        hitY = true;
                        distanceForFrame.y = rayCast.point.y - rayOrigin.y;
                    }
                    if (distanceForFrame.y == 0f) break;
                }
            }
            if (hitY) {
                if (Mathf.Abs(currentSpeed.y) > maxSpeed * bounceThreshold) {
                    currentSpeed.y *= -1f;
                } else {
                    currentSpeed.y = 0;
                }
            }

            // Actually move at long last.
            transform.position += distanceForFrame;

            // Decide whether or not to flip.
            goingRight = distanceForFrame.x > 0;
            if (distanceForFrame.x != 0 && goingRight != facingRight) {
                Flip();
            }
        }

        private void Flip() {
            facingRight = !facingRight;
            Vector3 currentScale = transform.localScale;
            currentScale.x *= -1;
            transform.localScale = currentScale;
        }

        public void InputsReceived(float horizontalScale, float verticalScale, bool grabHeld) {
            currentControllerState = new Vector3(horizontalScale, verticalScale);
        }

        public void ActionPressed() {
            float usableDashCooldown = HasPowerup(Powerup.HALF_ABILITY_COOLDOWN) ? dashCooldown / 2f : dashCooldown;
            if (Time.time - dashStart < usableDashCooldown || Time.time - lastCollisionTime < collisionDebounceTime) return;
            dashStart = Time.time;
            float angle = Mathf.Atan2(currentControllerState.y, currentControllerState.x);
            currentSpeed.x = Mathf.Cos(angle) * dashSpeed;
            currentSpeed.y = Mathf.Sin(angle) * dashSpeed;
        }

        public void ActionReleased() {

        }

        public void LightTogglePressed() {
            lightBox.IsActive = !lightBox.IsActive;
        }

        public bool IsAttacking() {
            return Time.time - dashStart < dashDamageDuration;
        }

        public void OnTriggerEnter2D(Collider2D other) {
            if (!photonView.isMine) return;
            BaseDreamerBehavior associatedBehavior = other.gameObject.GetComponent<BaseDreamerBehavior>();
            if (associatedBehavior == null || associatedBehavior.OutOfHealth()) return;
            if (IsAttacking() && Time.time - lastCollisionTime > collisionDebounceTime) {
                associatedBehavior.photonView.RPC("HandleCollision", PhotonTargets.All, currentSpeed);
                this.currentSpeed *= -1;
                lastCollisionTime = Time.time;
            }
        }

        public void OnTriggerStay2D(Collider2D other) {
            if (Time.time - lastCollisionTime > collisionDebounceTime) {
                OnTriggerEnter2D(other);
            }
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
            if (stream.isWriting) {
                stream.SendNext(transform.position);
                stream.SendNext(currentSpeed);
            } else {
                Vector3 networkPosition = (Vector3)stream.ReceiveNext();
                transform.position = (transform.position + networkPosition) / 2;
                currentSpeed = (Vector3)stream.ReceiveNext();
            }
        }

        protected override Powerup[] GetUsablePowerups() {
            return new Powerup[]{ Powerup.DREAMER_VISION, Powerup.PERFECT_ACCELERATION, Powerup.HALF_ABILITY_COOLDOWN };
        }
    }
}
