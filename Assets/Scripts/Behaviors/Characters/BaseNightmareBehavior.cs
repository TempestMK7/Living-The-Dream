using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Tempest.Nightmare {

    public abstract class BaseNightmareBehavior : EmpowerableCharacterBehavior, IPunObservable, IControllable {

        public float maxSpeed = 10f;
        public float accelerationFactor = 0.5f;
        public float snapToMaxThresholdFactor = 0.1f;
        public float bounceThreshold = 1.0f;

        public float rayBoundShrinkage = 0.001f;
        public int numRays = 4;

        public LayerMask whatIsSolid;
        public LayerMask whatIsPlayer;

        public float lightBoxScale = 25f;

        protected LightBoxBehavior lightBox;

        protected BoxCollider2D boxCollider;
        protected Animator animator;

        protected Vector3 currentSpeed;
        protected Vector3 currentControllerState;

        protected bool facingRight;
        protected float acceleration;
        protected float snapToMaxThreshold;

        // Use this for initialization
        public override void Awake() {
            base.Awake();

            lightBox = GetComponentInChildren<LightBoxBehavior>();
            lightBox.IsMine = photonView.isMine;
            lightBox.IsActive = false;
            lightBox.DefaultScale = new Vector3(lightBoxScale, lightBoxScale);
            lightBox.ActiveScale = new Vector3(lightBoxScale, lightBoxScale);

            boxCollider = GetComponent<BoxCollider2D>();
            animator = GetComponent<Animator>();
            animator.SetBool("IsAttacking", false);
            currentSpeed = new Vector3();
            currentControllerState = new Vector3();
            acceleration = accelerationFactor * maxSpeed;
            snapToMaxThreshold = maxSpeed * snapToMaxThresholdFactor;
            facingRight = false;
        }

        // Update is called once per frame
        public virtual void Update() {
            UpdateCurrentSpeed();
            MoveAsFarAsYouCan();
            HandlePowerupState();
        }

        protected virtual void UpdateCurrentSpeed() {
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

        private void HandlePowerupState() {
            if (HasPowerup(Powerup.BETTER_VISION)) {
                lightBox.DefaultScale = new Vector3(lightBoxScale * 2f, lightBoxScale * 2f);
            } else {
                lightBox.DefaultScale = new Vector3(lightBoxScale, lightBoxScale);
            }
        }

        public void InputsReceived(float horizontalScale, float verticalScale, bool grabHeld) {
            currentControllerState = new Vector3(horizontalScale, verticalScale);
        }

        public abstract void ActionPressed();

        public abstract void ActionReleased();

        public void LightTogglePressed() {
            lightBox.IsActive = !lightBox.IsActive;
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
            return new Powerup[] { Powerup.BETTER_VISION, Powerup.DREAMER_VISION, Powerup.PERFECT_ACCELERATION, Powerup.HALF_ABILITY_COOLDOWN };
        }
    }
}
