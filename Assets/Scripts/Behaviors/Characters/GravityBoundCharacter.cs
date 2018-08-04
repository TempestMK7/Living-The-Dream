using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Com.Tempest.Nightmare {

    public abstract class GravityBoundCharacter : EmpowerableCharacterBehavior {

		// Recovery timers.  Values are in seconds.
		public float wallJumpRecovery = 0.2f;
		public float dashDuration = 0.5f;
		public float damageRecovery = 0.5f;
		public float deathAnimationTime = 3f;

		// Player movement params.
		public float maxSpeed = 7f;
		public float gravityFactor = 3f;
		public float terminalVelocityFactor = 2f;
		public float risingGravityBackoffFactor = 1f;
		public float dashFactor = 3f;
		public float jumpFactor = 1.8f;
		public float wallJumpFactor = 1.5f;
		public float wallSlideFactor = 0.3f;
		public float wallJumpControlFactor = 5f;
        public float jumpFactorUpgradeModifier = 0.1f;
		public float wallSpeedReflectionFactor = -0.75f;

		// Player hit calculation params.
		public float rayBoundShrinkage = 0.001f;
		public int numRays = 4;
		public LayerMask whatIsSolid;

		private BoxCollider2D boxCollider;
        
		protected MovementState currentState;
		protected Vector3 currentSpeed;
		private Vector3 currentOffset;
		private Vector3 currentControllerState;
		private bool actionHeld;
		private bool grabHeld;
		private float timerStart;

        protected void AwakePhysics() {
			boxCollider = GetComponent<BoxCollider2D>();
			currentSpeed = new Vector3();
			currentControllerState = new Vector3();
			currentOffset = new Vector3();
			currentState = MovementState.GROUNDED;
			actionHeld = false;
			grabHeld = false;
        }

        protected void UpdatePhysics() {
			HandleVerticalMovement();
			HandleHorizontalMovement();
			MoveAndUpdateState();
			UpdateStateFromTimers();
        }

		private void HandleVerticalMovement() {
			if (currentState == MovementState.DASHING) return;
			if (currentState == MovementState.JUMPING && !actionHeld) {
				currentState = MovementState.FALLING;
				currentSpeed.y = 0f;
			} else if (currentState == MovementState.JUMPING || currentState == MovementState.WALL_JUMP) {
				currentSpeed.y -= maxSpeed * gravityFactor * risingGravityBackoffFactor * Time.deltaTime;
			} else if (currentState == MovementState.WALL_SLIDE_LEFT || currentState == MovementState.WALL_SLIDE_RIGHT) {
				if (grabHeld && currentSpeed.y <= 0f) {
					currentSpeed.y = 0f;
				} else {
					currentSpeed.y = Mathf.Max(currentSpeed.y, maxSpeed * wallSlideFactor * -1f);
				}
			} else {
				float downHeldFactor = -1f;
				if (currentControllerState.y < -0.5f && currentState != MovementState.DAMAGED && currentState != MovementState.DYING) {
					downHeldFactor += currentControllerState.y;
				}
				currentSpeed.y -= maxSpeed * gravityFactor * downHeldFactor * Time.deltaTime;
			}
			// Clip to terminal velocity if necessary.
			currentSpeed.y = Mathf.Max(currentSpeed.y, maxSpeed * terminalVelocityFactor * -1f);
		}

		private void HandleHorizontalMovement() {
			switch (currentState) {
				case MovementState.DASHING:
					break;
				case MovementState.WALL_SLIDE_LEFT:
				case MovementState.WALL_SLIDE_RIGHT:
					currentSpeed.x = 0f;
					break;
				case MovementState.DAMAGED:
				case MovementState.DYING:
					currentSpeed.x -= currentSpeed.x * Time.deltaTime;
					break;
				case MovementState.WALL_JUMP:
					currentSpeed.x += currentControllerState.x * maxSpeed * Time.deltaTime * wallJumpControlFactor;
					currentSpeed.x = Mathf.Clamp(currentSpeed.x, maxSpeed * -1f, maxSpeed);
					break;
				default:
					currentSpeed.x = currentControllerState.x * maxSpeed;
					break;
			}
		}

		private void MoveAndUpdateState() {
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
				if (currentState == MovementState.DAMAGED || currentState == MovementState.DYING || currentState == MovementState.DASHING) {
					currentSpeed.x *= wallSpeedReflectionFactor;
					currentOffset.x *= wallSpeedReflectionFactor;
				} else {
					currentSpeed.x = 0f;
					currentOffset.x = 0f;
					currentSpeed.y = Mathf.Max(currentSpeed.y, maxSpeed * wallSlideFactor * -1f);
				}
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
				if (currentState == MovementState.DASHING) {
					currentSpeed.y *= wallSpeedReflectionFactor;
					currentOffset.y *= wallSpeedReflectionFactor;
				} else {
					currentSpeed.y = 0f;
					currentOffset.y = 0f;
				}
			}

			// If our horizontal and vertical ray casts did not find anything, there could still be an object to our corner.
			if (!hitX && !hitY && distanceForFrame.x != 0 && distanceForFrame.y != 0) {
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

		private void UpdateStateFromTimers() {
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

		protected void InputPhysics(float horizontalScale, float verticalScale, bool grabHeld) {
			currentControllerState = new Vector3(horizontalScale, verticalScale);
			this.grabHeld = grabHeld;
		}

		protected void ActionPhysics(bool held) {
			actionHeld = held;
		}

		protected void JumpPhysics() {
			switch (currentState) {
				case MovementState.GROUNDED:
				case MovementState.JUMPING:
				case MovementState.FALLING:
				case MovementState.WALL_JUMP:
					currentSpeed.y = maxSpeed * JumpFactor();
					currentState = MovementState.JUMPING;
					break;
				case MovementState.WALL_SLIDE_LEFT:
                	currentSpeed.y = Mathf.Sin(Mathf.PI / 4) * maxSpeed * WallJumpFactor();
                	currentSpeed.x = Mathf.Cos(Mathf.PI / 4) * maxSpeed * WallJumpFactor();
					timerStart = Time.time;
					currentState = MovementState.WALL_JUMP;
					break;
				case MovementState.WALL_SLIDE_RIGHT:
                	currentSpeed.y = Mathf.Sin(Mathf.PI * 3 / 4) * maxSpeed * WallJumpFactor();
                	currentSpeed.x = Mathf.Cos(Mathf.PI * 3 / 4) * maxSpeed * WallJumpFactor();
					timerStart = Time.time;
					currentState = MovementState.WALL_JUMP;
					break;
			}
		}

		protected void DashPhysics() {
			if (currentState == MovementState.DAMAGED || currentState == MovementState.DYING)  return;
			currentState = MovementState.DASHING;
			timerStart = Time.time;

            Vector3 direction = currentControllerState * maxSpeed / currentControllerState.magnitude;
            currentSpeed = direction * dashFactor;
		}

		protected void AttackPhysics() {
			currentSpeed *= -0.5f;
		}

		protected void DamagePhysics(Vector3 newSpeed, bool outOfHealth) {
			currentState = outOfHealth ? MovementState.DYING : MovementState.DAMAGED;
			currentSpeed = newSpeed;
		}

        private float JumpFactor() {
            return jumpFactor + ((float) NumUpgrades * jumpFactorUpgradeModifier);
        }

        private float WallJumpFactor() {
            return wallJumpFactor + ((float) NumUpgrades * jumpFactorUpgradeModifier);
        }

		protected void SerializePhysics(PhotonStream stream, PhotonMessageInfo info) {
			if (stream.isWriting) {
				stream.SendNext(currentState);
				stream.SendNext(transform.position);
				stream.SendNext(currentSpeed);
				stream.SendNext(actionHeld);
				stream.SendNext(grabHeld);
			} else {
				currentState = (MovementState)stream.ReceiveNext();
				Vector3 networkPosition = (Vector3)stream.ReceiveNext();
				currentSpeed = (Vector3)stream.ReceiveNext();
				actionHeld = (bool)stream.ReceiveNext();
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