using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Com.Tempest.Nightmare {

    public abstract class FlyingCharacter : EmpowerableCharacterBehavior {

		public float dashDuration = 0.5f;
		public float damageRecovery = 0.5f;
		public float deathAnimationTime = 3f;

		public float maxSpeed = 10f;
		public float accelerationFactor = 0.5f;
		public float snapToMaxThresholdFactor = 0.1f;
		public float wallSpeedReflectionFactor = -0.75f;
		public float dashFactor = 3f;

		public float rayBoundShrinkage = 0.001f;
		public int numRays = 4;
        
		public LayerMask whatIsSolid;
		public LayerMask whatIsPlayer;
		private BoxCollider2D boxCollider;

        protected MovementState currentState;
		private Vector3 currentOffset;
		protected Vector3 currentSpeed;
		protected Vector3 currentControllerState;
		protected bool facingRight;
		private float baseAcceleration;
		private float snapToMaxThreshold;
        private float timerStart;

        protected void AwakePhysics() {
			boxCollider = GetComponent<BoxCollider2D>();
            currentState = MovementState.FALLING;
			currentOffset = new Vector3();
			currentSpeed = new Vector3();
			currentControllerState = new Vector3();
			baseAcceleration = accelerationFactor * maxSpeed;
			snapToMaxThreshold = maxSpeed * snapToMaxThresholdFactor;
			facingRight = false;
            timerStart = 0f;
        }

        protected void UpdatePhysics() {
            UpdateCurrentSpeed();
            MoveAndUpdateState();
            UpdateStateFromTimers();
        }

        private void UpdateCurrentSpeed() {
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
				Vector3 rayOriginBase = currentSpeed.y > 0 ? topLeft : bottomLeft;
				float rayOriginCorrection = currentSpeed.y > 0 ? rayBoundShrinkage : rayBoundShrinkage * -1f;
				for (int x = 0; x <= numRays; x++) {
					Vector3 rayOrigin = new Vector3(rayOriginBase.x + rayInterval * (float)x, rayOriginBase.y + rayOriginCorrection);
					RaycastHit2D rayCast = Physics2D.Raycast(rayOrigin, distanceForFrame.y > 0 ? Vector3.up : Vector3.down, Mathf.Abs(distanceForFrame.y), whatIsSolid);
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

        private void UpdateStateFromTimers() {
			if (currentState == MovementState.DAMAGED && Time.time - timerStart > damageRecovery) {
				currentState = MovementState.FALLING;
			} else if (currentState == MovementState.DYING && Time.time - timerStart > deathAnimationTime) {
				currentState = MovementState.FALLING;
			} else if (currentState == MovementState.DASHING && Time.time - timerStart > dashDuration) {
				currentState = MovementState.FALLING;
			}
        }

		protected void InputPhysics(float horizontalScale, float verticalScale) {
			currentControllerState = new Vector3(horizontalScale, verticalScale);
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

		protected virtual float GetCurrentAcceleration() {
			return baseAcceleration;
		}
        
		protected void SerializePhysics(PhotonStream stream, PhotonMessageInfo info) {
			if (stream.isWriting) {
				stream.SendNext(currentState);
				stream.SendNext(transform.position);
				stream.SendNext(currentSpeed);
			} else {
				currentState = (MovementState)stream.ReceiveNext();
				Vector3 networkPosition = (Vector3)stream.ReceiveNext();
				currentSpeed = (Vector3)stream.ReceiveNext();

				currentOffset = networkPosition - transform.position;
				if (currentOffset.magnitude > 3f) {
					currentOffset = new Vector3();
					transform.position = networkPosition;
				}
			}
        }
    }
}
