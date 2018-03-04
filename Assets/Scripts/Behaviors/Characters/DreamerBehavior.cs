using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Com.Tempest.Nightmare {
    
    public class DreamerBehavior : EmpowerableCharacterBehavior, IPunObservable, IControllable {

        // Player rule params.
        public int maxHealth = 3;
        public int maxDeathTime = 100;
        public float deathTimeLost = 30f;

        // Recovery timers.  Values are in seconds.
        public float jumpRecovery = 0.2f;
        public float wallJumpRecovery = 0.2f;
        public float nightmareCollisionRecovery = 0.5f;
        public float deathAnimationTime = 3f;
        public float healthBarFadeDelay = 5f;

        // Player movement params.
        public float maxSpeed = 6f;
        public float gravityFactor = 4f;
        public float terminalVelocityFactor = 2f;
        public float risingGravityBackoffFactor = 0.8f;
        public float jumpFactor = 1.8f;
        public float wallJumpFactor = 1.5f;
        public float wallSlideFactor = 0.3f;
        public float wallJumpControlFactor = 5f;

        // Player hit calculation params.
        public float rayBoundShrinkage = 0.001f;
        public int numRays = 4;
        public LayerMask whatIsSolid;
        public LayerMask whatIsBonfire;


        // Internal objects accessed by this behavior.
        private GameObject healthCanvas;
        private Image positiveHealthBar;
        private BoxCollider2D boxCollider;
        private Animator animator;
        private Renderer myRenderer;
        private Vector3 currentSpeed;
        private Vector3 currentControllerState;
        private Vector3 currentOffset;

        // Booleans used when deciding how to respond to collisions and controller inputs.
        private bool grabHeld;
        private bool grounded;
        private bool holdingWallLeft;
        private bool holdingWallRight;
        private bool usedSecondJump;

        // Health values.
        private int currentHealth;
        private float deathTimeRemaining;

        // Timer values, recorded in seconds.
        private float jumpTime;
        private float wallJumpTime;
        private float nightmareCollisionTime;
        private float deathEventTime;
        
        public override void Awake () {
            base.Awake();
            // Setup internal components and initialize object variables.
            healthCanvas = transform.Find("DreamerCanvas").gameObject;
            positiveHealthBar = healthCanvas.transform.Find("PositiveHealth").GetComponent<Image>();
            boxCollider = GetComponent<BoxCollider2D>();
            animator = GetComponent<Animator>();
            myRenderer = GetComponent<Renderer>();
            currentSpeed = new Vector3();
            currentControllerState = new Vector3();
            currentOffset = new Vector3();

            // Initialize state values.
            currentHealth = maxHealth;
            deathTimeRemaining = maxDeathTime;
        }

        // Update is called once per frame
        public override void Update() {
            base.Update();
            UpdateHorizontalMovement();
            UpdateVerticalMovement();
            MoveAsFarAsYouCan();
            HandleAnimator();
            ResurrectIfAble();
            HandleLifeState();
        }

        // Updates horizontal movement based on controller state.
        // Does nothing if this character belongs to another player.
        private void UpdateHorizontalMovement() {
            if (!photonView.isMine) return;
            if (Time.time - deathEventTime < deathAnimationTime) {
                currentSpeed.x -= currentSpeed.x * Time.deltaTime;
            } else if (Time.time - nightmareCollisionTime < nightmareCollisionRecovery) {
                currentSpeed += currentControllerState * maxSpeed * maxSpeed * 2f * Time.deltaTime;
            } else if (grabHeld && (holdingWallLeft || holdingWallRight)) {
                currentSpeed.x = 0;
            } else if (Time.time - wallJumpTime < wallJumpRecovery) {
                currentSpeed.x += currentControllerState.x * maxSpeed * Time.deltaTime * wallJumpControlFactor;
                if (currentSpeed.x > maxSpeed) currentSpeed.x = maxSpeed;
                else if (currentSpeed.x < maxSpeed * -1f) currentSpeed.x = maxSpeed * -1f;
            } else {
                currentSpeed.x = currentControllerState.x * maxSpeed;
            }
        }

        // Updates vertical movement based on gravity.  
        private void UpdateVerticalMovement() {
            // Add gravity.
            if (currentSpeed.y > maxSpeed * 0.1f) {
                currentSpeed.y += maxSpeed * -1f * gravityFactor * risingGravityBackoffFactor * Time.deltaTime;
            } else {
                currentSpeed.y += maxSpeed * -1f * gravityFactor * Time.deltaTime;
            }
            // Clip to terminal velocity if necessary.
            currentSpeed.y = Mathf.Max(currentSpeed.y, maxSpeed * terminalVelocityFactor * -1f);
        }

        // Moves the character based on current speed.
        // Uses raycasts to respect physics.
        private void MoveAsFarAsYouCan() {
            // Calculate how far we're going.
            Vector3 distanceForFrame = currentSpeed * Time.deltaTime;
            if (currentOffset.magnitude < 0.1f) {
                distanceForFrame += currentOffset;
                currentOffset = new Vector3();
            } else {
                distanceForFrame += currentOffset / 2f;
                currentOffset /= 2f;
            }
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
                holdingWallLeft = false;
                holdingWallRight = false;
                float rayInterval = (topLeft.y - bottomLeft.y) / (float)numRays;
                Vector3 rayOriginBase = currentSpeed.x > 0 ? bottomRight : bottomLeft;
                float rayOriginCorrection = currentSpeed.x > 0 ? rayBoundShrinkage : rayBoundShrinkage * -1f;
                for (int x = 0; x <= numRays; x++) {
                    Vector3 rayOrigin = new Vector3(rayOriginBase.x + rayOriginCorrection, rayOriginBase.y + rayInterval * (float)x);
                    RaycastHit2D rayCast = Physics2D.Raycast(rayOrigin, goingRight ? Vector3.right : Vector3.left, Mathf.Abs(distanceForFrame.x), whatIsSolid);
                    if (rayCast) {
                        hitX = true;
                        distanceForFrame.x = rayCast.point.x - rayOrigin.x;
                        if (currentSpeed.x > 0) {
                            holdingWallRight = true;
                        } else {
                            holdingWallLeft = true;
                        }
                    }
                    if (distanceForFrame.x == 0f) break;
                }
            }
            if (hitX) {
                if (Time.time - nightmareCollisionTime < nightmareCollisionRecovery || Time.time - deathEventTime < deathAnimationTime) {
                    holdingWallLeft = false;
                    holdingWallRight = false;
                    currentSpeed.x *= -1f;
                    currentOffset.x *= -1f;
                } else {
                    currentSpeed.x = 0f;
                    currentOffset.x = 0f;
                    if (currentSpeed.y < maxSpeed * wallSlideFactor * -1f) currentSpeed.y = maxSpeed * wallSlideFactor * -1f;
                }
            }

            if ((holdingWallLeft || holdingWallRight) && grabHeld) {
                if (currentSpeed.y < 0) {
                    currentSpeed.y = 0;
                    distanceForFrame.y = 0;
                }
            }

            // Use raycasts to decide if we hit anything vertically.
            if (distanceForFrame.y != 0) {
                grounded = false;
                float rayInterval = (bottomRight.x - bottomLeft.x) / (float)numRays;
                Vector3 rayOriginBase = currentSpeed.y > 0 ? topLeft : bottomLeft;
                float rayOriginCorrection = currentSpeed.y > 0 ? rayBoundShrinkage : rayBoundShrinkage * -1f;
                for (int x = 0; x <= numRays; x++) {
                    Vector3 rayOrigin = new Vector3(rayOriginBase.x + rayInterval * (float)x, rayOriginBase.y + rayOriginCorrection);
                    RaycastHit2D rayCast = Physics2D.Raycast(rayOrigin, distanceForFrame.y > 0 ? Vector3.up : Vector3.down, Mathf.Abs(distanceForFrame.y), whatIsSolid);
                    if (rayCast) {
                        hitY = true;
                        distanceForFrame.y = rayCast.point.y - rayOrigin.y;
                        if (currentSpeed.y < 0) {
                            grounded = true;
                            usedSecondJump = false;
                        }
                    }
                    if (distanceForFrame.y == 0f) break;
                }
            }
            if (hitY) {
                currentSpeed.y *= Time.time - nightmareCollisionTime < nightmareCollisionRecovery ? -1f : 0f;
                currentOffset.y *= Time.time - nightmareCollisionTime < nightmareCollisionRecovery ? -1f : 0f;
                grounded = grounded && Time.time - nightmareCollisionTime > nightmareCollisionRecovery;
            }

            // If our horizontal and vertical ray casts did not find anything, there could still be an object to our corner.
            if (!(hitY || hitX) && distanceForFrame.x != 0 && distanceForFrame.y != 0) {
                Vector3 rayOrigin = new Vector3(goingRight ? bottomRight.x : bottomLeft.x, goingUp ? topLeft.y : bottomLeft.y);
                RaycastHit2D rayCast = Physics2D.Raycast(rayOrigin, distanceForFrame, distanceForFrame.magnitude, whatIsSolid);
                if (rayCast) {
                    distanceForFrame.x = rayCast.point.x - rayOrigin.x;
                    distanceForFrame.y = rayCast.point.y - rayOrigin.y;
                }
            }

            // Actually move at long last.
            transform.position += distanceForFrame;
        }

        private void HandleAnimator() {
            animator.SetBool("IsGrounded", grounded);
            int speed = 0;
            if (currentSpeed.x < 0f) speed = -1;
            else if (currentSpeed.x > 0f) speed = 1;
            animator.SetInteger("HorizontalSpeed", speed);
            bool dyingAnimation = OutOfHealth() && !IsDead();
            animator.SetBool("DyingAnimation", dyingAnimation);
        }

        // Brings the player back to life if they are within range of a bonfire that has living players near it.
        private void ResurrectIfAble() {
            if (!photonView.isMine || !IsDead() || IsExiled()) return;
            Collider2D[] bonfires = Physics2D.OverlapAreaAll(boxCollider.bounds.min, boxCollider.bounds.max, whatIsBonfire);
            foreach (Collider2D fireCollider in bonfires) {
                BonfireBehavior behavior = fireCollider.gameObject.GetComponent<BonfireBehavior>();
                if (behavior == null) continue;
                if (behavior.PlayersNearby()) {
                    currentHealth = maxHealth;
                }
            }
        }

        // Draws current health total, switches layers based on health totals, and hides player to other players if dead.
        private void HandleLifeState() {
            if (photonView.isMine && IsExiled()) {
                FindObjectOfType<GameManagerBehavior>().Dreamer = null;
                PhotonNetwork.Destroy(photonView);
                return;
            }

            if (IsDead()) {
                deathTimeRemaining -= Time.deltaTime;
                positiveHealthBar.fillAmount = deathTimeRemaining / (float)maxDeathTime;
                gameObject.layer = LayerMask.NameToLayer("Death");
                ToggleRenderers(photonView.isMine);
                healthCanvas.SetActive(photonView.isMine);
            } else {
                positiveHealthBar.fillAmount = (float)currentHealth / (float)maxHealth;
                gameObject.layer = LayerMask.NameToLayer(OutOfHealth() ? "Death" : "Dreamer");
                ToggleRenderers(true);
                healthCanvas.SetActive(Time.time - nightmareCollisionTime < healthBarFadeDelay);
            }
        }

        // Toggles base renderer and health canvas if necessary.
        // Prevents multiple calls to change enabled state.
        private void ToggleRenderers(bool enabled) {
            if (myRenderer.enabled != enabled) myRenderer.enabled = enabled;
            foreach(SpriteRenderer childRenderer in GetComponentsInChildren<SpriteRenderer>()) {
                if (childRenderer.enabled != enabled) childRenderer.enabled = enabled;
            }
        }

        public bool OutOfHealth() {
            return currentHealth <= 0;
        }

        // Returns whether or not the player is currently dead (out of health but still in the game).
        public bool IsDead() {
            return currentHealth <= 0 && Time.time - deathEventTime > deathAnimationTime;
        }

        // Returns whether or not the player is out of the game (out of death time).
        public bool IsExiled() {
            return deathTimeRemaining <= 0;
        }

        // Called by the input manager with controller values.
        public void SendInputs(float horizontalScale, float verticalScale, bool grabHeld) {
            currentControllerState = new Vector3(horizontalScale, verticalScale);
            this.grabHeld = grabHeld;
        }

        // Called by the input manager when the jump action is pressed.
        public void SendAction() {
            // If we just jumped, got hit, or are in the death animation, ignore this action.
            if (Time.time - jumpTime < jumpRecovery ||
                Time.time - nightmareCollisionTime < nightmareCollisionRecovery ||
                Time.time - deathEventTime < deathAnimationTime) {
                return;
            }

            if (grounded) {
                currentSpeed.y = maxSpeed * jumpFactor;
                jumpTime = Time.time;
            } else if (holdingWallLeft) {
                currentSpeed.y = Mathf.Sin(Mathf.PI / 4) * maxSpeed * wallJumpFactor;
                currentSpeed.x = Mathf.Cos(Mathf.PI / 4) * maxSpeed * wallJumpFactor;
                jumpTime = Time.time;
                wallJumpTime = Time.time;
                holdingWallLeft = false;
            } else if (holdingWallRight) {
                currentSpeed.y = Mathf.Sin(Mathf.PI * 3 / 4) * maxSpeed * wallJumpFactor;
                currentSpeed.x = Mathf.Cos(Mathf.PI * 3 / 4) * maxSpeed * wallJumpFactor;
                jumpTime = Time.time;
                wallJumpTime = Time.time;
                holdingWallRight = false;
            } else if (!usedSecondJump) {
                currentSpeed.y = maxSpeed * jumpFactor * 0.9f;
                jumpTime = Time.time;
                usedSecondJump = true;
            }
        }

        // Called by a nightmare behavior when collision occurs.
        [PunRPC]
        public void HandleCollision(int nightmareId, int dreamerId, Vector3 currentSpeed) {
            this.currentSpeed = currentSpeed;
            nightmareCollisionTime = Time.time;
            currentHealth -= 1;
            if (currentHealth <= 0) {
                currentHealth = 0;
                deathEventTime = Time.time;
                deathTimeRemaining -= deathTimeLost;
            }
        }

        // Called by Photon whenever player state is synced across the network.
        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
            if (stream.isWriting) {
                stream.SendNext(transform.position);
                stream.SendNext(currentSpeed);
                stream.SendNext(grabHeld);
                stream.SendNext(currentHealth);
            } else {
                Vector3 networkPosition = (Vector3)stream.ReceiveNext();
                currentSpeed = (Vector3)stream.ReceiveNext();
                grabHeld = (bool)stream.ReceiveNext();
                currentHealth = (int)stream.ReceiveNext();

                currentOffset = networkPosition - transform.position;
                if (currentOffset.magnitude > 1f) {
                    currentOffset = new Vector3();
                    transform.position = networkPosition;
                }
            }
        }

        protected override Powerup[] GetUsablePowerups() {
            return new Powerup[] { Powerup.NIGHTMARE_VISION };
        }
    }
}
