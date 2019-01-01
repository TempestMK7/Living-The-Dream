using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Com.Tempest.Nightmare {

    public abstract class PhysicsCharacter : EmpowerableCharacterBehavior, IPunObservable, IControllable {

        // Recovery timers.  Values are in seconds.
		public float wallJumpRecovery = 0.2f;
		public float dashDuration = 0.5f;
		public float damageRecovery = 1f;
		public float deathAnimationTime = 3f;

		// General movement params.
		public float maxSpeed = 7f;
		public float dashFactor = 3f;
		public bool wallReflection = true;
		public float wallSpeedReflectionFactor = -0.75f;

        // Flyer movement params.
		public float accelerationFactor = 0.5f;
		public float snapToMaxThresholdFactor = 0.1f;

        // Gravity bound movement params.
		public float gravityFactor = 3f;
		public float terminalVelocityFactor = 3f;
		public float risingGravityBackoffFactor = 0.9f;
		public float jumpFactor = 1.5f;
		public float wallJumpFactor = 1.5f;
		public float wallSlideFactor = 0.5f;
		public float wallJumpControlFactor = 6f;

		// Physics hit calculation params.
		public float rayBoundShrinkage = 0.001f;
		public int numRays = 4;
		public LayerMask whatIsSolid;

        // Self initialized variables.
		protected BoxCollider2D boxCollider;
		protected Animator animator;
		protected MovementState currentState;
		protected Vector3 currentSpeed;
		protected Vector3 currentControllerState;
		private Vector3 currentOffset;
		private float timerStart;

        // Self initialized flyer variables.
		private float baseAcceleration;
		private float snapToMaxThreshold;
        private bool facingRight;

        // Self initialized gravity bound variables.
		private bool actionPrimaryHeld;
		private bool actionSecondaryHeld;
		private bool grabHeld;

        protected abstract bool IsFlyer();

        // Called by the system when created.
        public override void Awake() {
            base.Awake();
			boxCollider = GetComponent<BoxCollider2D>();
			animator = GetComponent<Animator>();
			currentSpeed = new Vector3();
			currentControllerState = new Vector3();
			currentOffset = new Vector3();
			currentState = MovementState.GROUNDED;
			baseAcceleration = accelerationFactor * maxSpeed;
			snapToMaxThreshold = maxSpeed * snapToMaxThresholdFactor;
			facingRight = false;
			actionPrimaryHeld = false;
			grabHeld = false;
        }

        // Called by the system once per frame.
        public virtual void Update() {
            if (IsFlyer()) {
                UpdateCurrentSpeedFlyer();
                MoveAndUpdateStateFlyer();
            } else {
			    HandleVerticalMovementGravityBound();
			    HandleHorizontalMovementGravityBound();
			    MoveAndUpdateStateGravityBound();
            }
			UpdateStateFromTimers();
			HandleAnimator();
        }

		#region Gravity-bound physics handling.

		protected virtual void HandleVerticalMovementGravityBound() {
			if (currentState == MovementState.DASHING) return;
			if (currentState == MovementState.JUMPING || currentState == MovementState.WALL_JUMP) {
				currentSpeed.y -= MaxSpeed() * gravityFactor * risingGravityBackoffFactor * Time.deltaTime;
			} else if (currentState == MovementState.WALL_SLIDE_LEFT || currentState == MovementState.WALL_SLIDE_RIGHT) {
				if (grabHeld && currentSpeed.y <= 0f) {
					currentSpeed.y = 0f;
				} else {
					float wallControlFactor = currentControllerState.y < -0.5f ? terminalVelocityFactor : 1f;
					currentSpeed.y -= MaxSpeed() * gravityFactor * Time.deltaTime * wallControlFactor;
					currentSpeed.y = Mathf.Max(currentSpeed.y, MaxSpeed() * wallSlideFactor * wallControlFactor * -1f);
				}
			} else {
				float downHeldFactor = -1f;
				if (currentControllerState.y < -0.5f && currentState != MovementState.DAMAGED && currentState != MovementState.DYING) {
					downHeldFactor += currentControllerState.y;
				}
				currentSpeed.y += MaxSpeed() * gravityFactor * downHeldFactor * Time.deltaTime;
			}
			// Clip to terminal velocity if necessary.
			currentSpeed.y = Mathf.Clamp(currentSpeed.y, MaxSpeed() * terminalVelocityFactor * -1f, MaxSpeed() * JumpFactor());
		}

		private void HandleHorizontalMovementGravityBound() {
			switch (currentState) {
				case MovementState.DASHING:
					break;
				case MovementState.DAMAGED:
				case MovementState.DYING:
					currentSpeed.x -= currentSpeed.x * Time.deltaTime;
					break;
				case MovementState.WALL_SLIDE_LEFT:
					if (currentControllerState.x > 0.5f && !grabHeld) {
						currentSpeed.x = currentControllerState.x * MaxSpeed();
						currentState = MovementState.FALLING;
					} else {
						currentSpeed.x = -0.01f;
					}
					break;
				case MovementState.WALL_SLIDE_RIGHT:
					if (currentControllerState.x < -0.5f && !grabHeld) {
						currentSpeed.x = currentControllerState.x * MaxSpeed();
						currentState = MovementState.FALLING;
					} else {
						currentSpeed.x = 0.01f;
					}
					break;
				case MovementState.WALL_JUMP:
					currentSpeed.x += currentControllerState.x * MaxSpeed() * Time.deltaTime * wallJumpControlFactor;
					currentSpeed.x = Mathf.Clamp(currentSpeed.x, MaxSpeed() * -1f, MaxSpeed());
					break;
				default:
					currentSpeed.x = currentControllerState.x * MaxSpeed();
					break;
			}
		}

		private void MoveAndUpdateStateGravityBound() {
			// Calculate how far we're going.
			Vector3 distanceForFrame = currentSpeed * Time.deltaTime;
			if (currentOffset.magnitude < 0.1f) {
				distanceForFrame += currentOffset;
				currentOffset = new Vector3();
			} else {
				distanceForFrame += currentOffset / 4f;
				currentOffset -= currentOffset / 4f;
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
				float rayInterval = (topLeft.y - bottomLeft.y) / (float)numRays;
				Vector3 rayOriginBase = currentSpeed.x > 0 ? bottomRight : bottomLeft;
				float rayOriginCorrection = currentSpeed.x > 0 ? rayBoundShrinkage : rayBoundShrinkage * -1f;
				for (int x = 0; x <= numRays; x++) {
					Vector3 rayOrigin = new Vector3(rayOriginBase.x + rayOriginCorrection, rayOriginBase.y + rayInterval * (float)x);
					RaycastHit2D rayCast = Physics2D.Raycast(rayOrigin, goingRight ? Vector3.right : Vector3.left, Mathf.Abs(distanceForFrame.x), whatIsSolid);
					if (rayCast) {
						hitX = true;
						distanceForFrame.x = rayCast.point.x - rayOrigin.x;
						if (currentState != MovementState.DAMAGED && 
								currentState != MovementState.DYING && 
								currentState != MovementState.GROUNDED && 
								currentState != MovementState.DASHING) {
							currentState = goingRight ? MovementState.WALL_SLIDE_RIGHT : MovementState.WALL_SLIDE_LEFT;
						}
					}
					if (distanceForFrame.x == 0f)
						break;
				}
			}

			// If we hit anything horizontally, reflect or stop x axis movement.
			if (hitX) {
				if (currentState == MovementState.DAMAGED || currentState == MovementState.DYING || (currentState == MovementState.DASHING && wallReflection)) {
					currentSpeed.x *= wallSpeedReflectionFactor;
					currentOffset.x *= wallSpeedReflectionFactor;
				} else {
					currentSpeed.x = 0f;
					currentOffset.x = 0f;
				}
			} else if (currentState == MovementState.WALL_SLIDE_LEFT || currentState == MovementState.WALL_SLIDE_RIGHT) {
				currentState = MovementState.FALLING;
			}

			// If we grabbed a wall and are holding grab, 0 out y movement.
			if ((currentState == MovementState.WALL_SLIDE_LEFT || currentState == MovementState.WALL_SLIDE_RIGHT) && grabHeld) {
				if (currentSpeed.y < 0) {
					currentSpeed.y = 0;
					distanceForFrame.y = 0;
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
						if (!goingUp && currentState != MovementState.DAMAGED && currentState != MovementState.DYING) {
							currentState = MovementState.GROUNDED;
						}
					}
					if (distanceForFrame.y == 0f)
						break;
				}
			}
			if (hitY) {
				if ((currentState == MovementState.DAMAGED || currentState == MovementState.DYING ||
						(currentState == MovementState.DASHING && wallReflection)) && 
						Mathf.Abs(currentSpeed.y) > MaxSpeed()) {
					currentSpeed.y *= wallSpeedReflectionFactor;
					currentOffset.y *= wallSpeedReflectionFactor;
				} else {
					currentSpeed.y = 0f;
					currentOffset.y = 0f;
				}
			} else if (currentState == MovementState.GROUNDED) {
				currentState = MovementState.FALLING;
			}

			// If our horizontal and vertical ray casts did not find anything, there could still be an object to our corner.
			if (!hitX && !hitY && distanceForFrame.x != 0 && distanceForFrame.y != 0) {
				Vector3 rayOrigin = new Vector3(goingRight ? bottomRight.x : bottomLeft.x, goingUp ? topLeft.y : bottomLeft.y);
				rayOrigin.x += goingRight ? rayBoundShrinkage : rayBoundShrinkage * -1f;
				rayOrigin.y += goingUp ? rayBoundShrinkage : rayBoundShrinkage * -1f;
				RaycastHit2D rayCast = Physics2D.Raycast(rayOrigin, distanceForFrame, distanceForFrame.magnitude, whatIsSolid);
				if (rayCast) {
					distanceForFrame.x = rayCast.point.x - rayOrigin.x;
					distanceForFrame.y = rayCast.point.y - rayOrigin.y;
					currentSpeed.x = 0f;
					currentOffset.x = 0f;
					currentState = goingRight ? MovementState.WALL_SLIDE_RIGHT : MovementState.WALL_SLIDE_LEFT;
				}
			}

			// Actually move at long last.
			transform.position += distanceForFrame;
		}

		#endregion

		#region Flying physics handling.

        private void UpdateCurrentSpeedFlyer() {
            // Ignore controls if damaged, dying, or dashing.
            if (currentState == MovementState.DASHING) return;
            if (currentState == MovementState.DAMAGED || currentState == MovementState.DYING) {
                currentSpeed *= 0.9f;
                return;
            }

			Vector3 newMax = new Vector3(maxSpeed * currentControllerState.x, maxSpeed * currentControllerState.y);
			if (newMax.magnitude > maxSpeed) {
				newMax *= maxSpeed / newMax.magnitude;
			}
			if (currentSpeed.magnitude > maxSpeed) {
				currentSpeed *= maxSpeed / currentSpeed.magnitude;
			}
			// This is how far we are from that speed.
			Vector3 difference = newMax - currentSpeed;
			float usableAcceleratior = HasPowerup(Powerup.PERFECT_ACCELERATION) ? maxSpeed : GetCurrentAcceleration();
			if (Mathf.Abs(difference.x) > snapToMaxThreshold) {
				difference.x *= usableAcceleratior * Time.deltaTime;
			}
			if (Mathf.Abs(difference.y) > snapToMaxThreshold) {
				difference.y *= usableAcceleratior * Time.deltaTime;
			}
			currentSpeed += difference;
        }

        private void MoveAndUpdateStateFlyer() {
			// Calculate how far we're going.
			Vector3 distanceForFrame = currentSpeed * Time.deltaTime;
			if (currentOffset.magnitude < 0.1f) {
				distanceForFrame += currentOffset;
				currentOffset = new Vector3();
			} else {
				distanceForFrame += currentOffset / 4f;
				currentOffset -= currentOffset / 4f;
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
					if (distanceForFrame.x == 0f)
						break;
				}
			}
			if (hitX) {
				if (Mathf.Abs(currentSpeed.x) > maxSpeed) {
					currentSpeed.x *= wallSpeedReflectionFactor;
					currentOffset.x *= wallSpeedReflectionFactor;
				} else {
					currentSpeed.x = 0f;
					currentOffset.x = 0f;
				}
			}

			// Use raycasts to decide if we hit anything vertically.
			if (distanceForFrame.y != 0) {
				float rayInterval = (bottomRight.x - bottomLeft.x) / (float)numRays;
				Vector3 rayOriginBase = goingUp ? topLeft : bottomLeft;
				float rayOriginCorrection = goingUp ? rayBoundShrinkage : rayBoundShrinkage * -1f;
				for (int x = 0; x <= numRays; x++) {
					Vector3 rayOrigin = new Vector3(rayOriginBase.x + rayInterval * (float)x, rayOriginBase.y + rayOriginCorrection);
					RaycastHit2D rayCast = Physics2D.Raycast(rayOrigin, goingUp ? Vector3.up : Vector3.down, Mathf.Abs(distanceForFrame.y), whatIsSolid);
					if (rayCast) {
						hitY = true;
						distanceForFrame.y = rayCast.point.y - rayOrigin.y;
					}
					if (distanceForFrame.y == 0f)
						break;
				}
			}
			if (hitY) {
				if (currentState == MovementState.DASHING) {
					currentSpeed.y *= wallSpeedReflectionFactor;
					currentOffset.y *= wallSpeedReflectionFactor;
				} else {
					currentSpeed.y = 0;
					currentOffset.y = 0f;
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

		protected virtual void Flip() {
			facingRight = !facingRight;
			Vector3 currentScale = transform.localScale;
			currentScale.x *= -1;
			transform.localScale = currentScale;
		}

		#endregion

		private void UpdateStateFromTimers() {
			if (photonView.isMine) {
				if (currentState == MovementState.DAMAGED && Time.time - timerStart > damageRecovery) {
					currentState = MovementState.FALLING;
				} else if (currentState == MovementState.DYING && Time.time - timerStart > deathAnimationTime) {
					currentState = MovementState.FALLING;
				} else if (currentState == MovementState.WALL_JUMP && Time.time - timerStart > wallJumpRecovery) {
					currentState = MovementState.JUMPING;
				} else if (currentState == MovementState.DASHING && Time.time - timerStart > dashDuration) {
					currentState = MovementState.FALLING;
				}
			}
			if (currentState == MovementState.JUMPING && (currentSpeed.y <= 0 || !actionPrimaryHeld)) {
				currentState = MovementState.FALLING;
			}
		}

		protected virtual void HandleAnimator() {
			animator.SetBool("IsGrounded", currentState == MovementState.GROUNDED);
			int speed = 0;
			if (currentSpeed.x < 0f) speed = -1;
			else if (currentSpeed.x > 0f) speed = 1;
			animator.SetInteger("HorizontalSpeed", speed);
			animator.SetBool("DyingAnimation", currentState == MovementState.DYING);
		}

		public virtual void InputsReceived(float horizontalScale, float verticalScale, bool grabHeld) {
			currentControllerState = new Vector3(horizontalScale, verticalScale);
			this.grabHeld = grabHeld;
		}

		public virtual void ActionPrimaryPressed() {
			actionPrimaryHeld = true;
		}

		public virtual void ActionPrimaryReleased() {
			actionPrimaryHeld = false;
		}

		public virtual void ActionSecondaryPressed() {
			actionSecondaryHeld = true;
		}

		public virtual void ActionSecondaryReleased() {
			actionSecondaryHeld = false;
		}

		public virtual void LightTogglePressed() {
			// ignored callback.
		}

		#region Control callbacks.

		// These callbacks are not used by every character, so they should be called by children of this class when needed.

		protected void JumpPhysics() {
			switch (currentState) {
				case MovementState.GROUNDED:
				case MovementState.JUMPING:
				case MovementState.FALLING:
				case MovementState.WALL_JUMP:
					currentSpeed.y = MaxSpeed() * JumpFactor();
					currentState = MovementState.JUMPING;
					break;
				case MovementState.WALL_SLIDE_LEFT:
                	currentSpeed.y = Mathf.Sin(Mathf.PI / 4) * MaxSpeed() * WallJumpFactor();
                	currentSpeed.x = Mathf.Cos(Mathf.PI / 4) * MaxSpeed() * WallJumpFactor();
					timerStart = Time.time;
					currentState = MovementState.WALL_JUMP;
					break;
				case MovementState.WALL_SLIDE_RIGHT:
                	currentSpeed.y = Mathf.Sin(Mathf.PI * 3 / 4) * MaxSpeed() * WallJumpFactor();
                	currentSpeed.x = Mathf.Cos(Mathf.PI * 3 / 4) * MaxSpeed() * WallJumpFactor();
					timerStart = Time.time;
					currentState = MovementState.WALL_JUMP;
					break;
			}
		}

		protected void DashPhysics() {
			if (currentState == MovementState.DAMAGED || currentState == MovementState.DYING)  return;
			currentState = MovementState.DASHING;
			timerStart = Time.time;

			if (currentControllerState.magnitude == 0f) {
				currentSpeed = new Vector3(0, -1f, 0) * MaxSpeed() * DashFactor();
			} else {
            	Vector3 direction = currentControllerState * MaxSpeed() / currentControllerState.magnitude;
            	currentSpeed = direction * DashFactor();
			}
		}

		protected void AttackPhysics() {
			currentSpeed *= -0.5f;
		}

		protected void DamagePhysics(Vector3 newSpeed, bool outOfHealth) {
			currentState = outOfHealth ? MovementState.DYING : MovementState.DAMAGED;
			timerStart = Time.time;
			currentSpeed = newSpeed;
		}

		#endregion

		#region Overridable variable accessors.

		// Some characters have conditional changes to these values, such as upgrades of powerups 
		// that modify the values temporarily.  Overriding these will change how the numbers are 
		// used in the base physics calculations.

		protected virtual float MaxSpeed() {
			return maxSpeed;
		}

        protected virtual float JumpFactor() {
            return jumpFactor;
        }

        protected virtual float WallJumpFactor() {
            return wallJumpFactor;
        }

		protected virtual float DashFactor() {
			return dashFactor;
		}

		protected virtual float GetCurrentAcceleration() {
			return baseAcceleration;
		}

		#endregion

		public virtual void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
			if (stream.isWriting) {
				stream.SendNext(currentState);
				stream.SendNext(transform.position);
				stream.SendNext(currentSpeed);
				stream.SendNext(currentControllerState);
				stream.SendNext(actionPrimaryHeld);
				stream.SendNext(grabHeld);
			} else {
				currentState = (MovementState)stream.ReceiveNext();
				Vector3 networkPosition = (Vector3)stream.ReceiveNext();
				currentSpeed = (Vector3)stream.ReceiveNext();
				currentControllerState = (Vector3)stream.ReceiveNext();
				actionPrimaryHeld = (bool)stream.ReceiveNext();
				grabHeld = (bool)stream.ReceiveNext();

				currentOffset = networkPosition - transform.position;
				if (currentOffset.magnitude > 3f) {
					currentOffset = new Vector3();
					transform.position = networkPosition;
				}
			}
		}
    }
}
