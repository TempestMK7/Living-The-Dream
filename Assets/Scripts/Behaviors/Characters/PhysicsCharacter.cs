using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Com.Tempest.Nightmare {

    public abstract class PhysicsCharacter : EmpowerableCharacterBehavior, IPunObservable, IControllable {

        // Recovery timers.  Values are in seconds.
		public float dashDuration = 0.5f;
        public float dashCooldown = 1f;
        public float parryDuration = 0.5f;
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
        public float groundAccelerationFactor = 6f;
        public float aerialAccelerationFactor = 4f;
        public float directionalInfluenceFactor = 2f;

		// Physics hit calculation params.
		public float rayBoundShrinkage = 0.001f;
		public int numRays = 4;
		public LayerMask whatIsSolid;

		public AudioSource jumpSource;
		public AudioSource doubleJumpSource;
		public AudioSource dashSource;

		public AudioSource hitSource;
		public AudioSource deathSource;
		public AudioSource relightSource;

        // Self initialized variables.
		protected BoxCollider2D boxCollider;
		protected Animator animator;
		protected GameObject nameCanvas;
		protected MovementState currentState;
		protected Vector3 currentSpeed;
		protected Vector3 currentControllerState;
		private Vector3 currentOffset;

        // State timers.
        private bool awaitingTimer;
        private float stateTimerChange;
        private float freezeTime;
        private float stunTime;

        private Vector3 damageSpeed;

		private float lastVolume = -1f;

        // Self initialized flyer variables.
		private float baseAcceleration;
		private float snapToMaxThreshold;
        private bool facingRight;

        // Self initialized gravity bound variables.
		private bool actionPrimaryHeld;
		private bool actionSecondaryHeld;
		protected bool grabHeld;

        public bool ControlsFrozen { get; set; }

        protected abstract bool IsFlyer();

        // Called by the system when created.
        public override void Awake() {
            base.Awake();
			boxCollider = GetComponent<BoxCollider2D>();
			animator = GetComponent<Animator>();
			nameCanvas = transform.Find("NameCanvas").gameObject;
			currentSpeed = new Vector3();
			currentControllerState = new Vector3();
			currentOffset = new Vector3();
			currentState = MovementState.GROUNDED;
			baseAcceleration = accelerationFactor * maxSpeed;
			snapToMaxThreshold = maxSpeed * snapToMaxThresholdFactor;
            awaitingTimer = false;
            stateTimerChange = 0f;
			facingRight = false;
			actionPrimaryHeld = false;
			actionSecondaryHeld = false;
			grabHeld = false;
            ControlsFrozen = false;
        }

        // Called by the system once per frame.
        public virtual void Update() {
            if (ControlsFrozen) {
                currentControllerState = new Vector3();
            }
            if (IsFlyer()) {
                UpdateCurrentSpeedFlyer();
                MoveAndUpdateStateFlyer();
            } else {
			    HandleVerticalMovementGravityBound();
			    HandleHorizontalMovementGravityBound();
			    MoveAndUpdateStateGravityBound();
            }
			ResetVolume();
			UpdateStateFromTimers();
			HandleAnimator();
        }

		#region Gravity-bound physics handling.

		protected virtual void HandleVerticalMovementGravityBound() {
            switch (currentState) {
                case MovementState.DASHING:
                    return;
                case MovementState.PARRYING:
                case MovementState.HIT_FREEZE:
                    currentSpeed.y = 0f;
                    return;
                case MovementState.JUMPING:
                    currentSpeed.y -= MaxSpeed() * gravityFactor * risingGravityBackoffFactor * Time.deltaTime;
                    break;
                case MovementState.WALL_SLIDE_LEFT:
                case MovementState.WALL_SLIDE_RIGHT:
                    if (grabHeld && currentSpeed.y <= 0f) {
                        currentSpeed.y = 0f;
                    } else {
                        float wallControlFactor = currentControllerState.y < -0.5f ? terminalVelocityFactor : 1f;
                        currentSpeed.y -= MaxSpeed() * gravityFactor * Time.deltaTime * wallControlFactor;
                        currentSpeed.y = Mathf.Max(currentSpeed.y, MaxSpeed() * wallSlideFactor * wallControlFactor * -1f);
                    }
                    break;
                case MovementState.HIT_STUN:
                    currentSpeed.y += MaxSpeed() * (gravityFactor + (currentControllerState.y * directionalInfluenceFactor)) * Time.deltaTime * -1f;
                    break;
                case MovementState.RAG_DOLL:
                    currentSpeed.y += MaxSpeed() * gravityFactor * Time.deltaTime * -1f;
                    break;
                default:
                    if (currentControllerState.y <= -0.5f) {
                        float fastFallTalent = talentRanks[TalentEnum.FASTER_FALL_SPEED];
                        if (fastFallTalent < 3) {
                            currentSpeed.y += MaxSpeed() * gravityFactor * Time.deltaTime * (-2f - (fastFallTalent * 0.5f));
                        } else {
                            currentSpeed.y = MaxSpeed() * terminalVelocityFactor * -1f;
                        }
                    } else {
                        currentSpeed.y += MaxSpeed() * gravityFactor * Time.deltaTime * -1f;
                    }
                    break;
            }

			// Clip to terminal velocity if necessary.
			currentSpeed.y = Mathf.Clamp(currentSpeed.y, MaxSpeed() * terminalVelocityFactor * -1f, MaxSpeed() * JumpFactor());
		}

		private void HandleHorizontalMovementGravityBound() {
			switch (currentState) {
				case MovementState.DASHING:
                    break;
                case MovementState.PARRYING:
                case MovementState.HIT_FREEZE:
                    currentSpeed.x = 0f;
                    break;
                case MovementState.HIT_STUN:
                    currentSpeed.x += currentControllerState.x * directionalInfluenceFactor * MaxSpeed() * Time.deltaTime;
                    break;
                case MovementState.RAG_DOLL:
                    // Constantly decelerate in rag doll.
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
                case MovementState.GROUNDED:
                    float horizontalAxis = Mathf.Clamp(currentControllerState.x, -1f, 1f);
                    float intendedSpeed = horizontalAxis * MaxSpeed();
                    float difference = intendedSpeed - currentSpeed.x;
                    if (Mathf.Abs(difference) > snapToMaxThreshold) {
                        difference *= groundAccelerationFactor * Time.deltaTime;
                    }
                    currentSpeed.x += difference;
                    break;
                case MovementState.FALLING:
                case MovementState.JUMPING:
                    horizontalAxis = Mathf.Clamp(currentControllerState.x, -1f, 1f);
                    intendedSpeed = horizontalAxis * MaxSpeed();
                    difference = intendedSpeed - currentSpeed.x;
                    if (Mathf.Abs(difference) > snapToMaxThreshold) {
                        difference *= aerialAccelerationFactor * Time.deltaTime;
                    }
                    currentSpeed.x += difference;
                    break;
			}
            currentSpeed.x = Mathf.Clamp(currentSpeed.x, MaxSpeed() * -1f, MaxSpeed());
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
						if (currentState == MovementState.JUMPING || currentState == MovementState.FALLING) {
							currentState = goingRight ? MovementState.WALL_SLIDE_RIGHT : MovementState.WALL_SLIDE_LEFT;
						}
					}
					if (distanceForFrame.x == 0f)
						break;
				}
			}

			// If we hit anything horizontally, reflect or stop x axis movement.
			if (hitX) {
				if (currentState == MovementState.HIT_STUN || currentState == MovementState.RAG_DOLL) {
					currentSpeed.x *= wallSpeedReflectionFactor;
					currentOffset.x *= wallSpeedReflectionFactor;
				} else if (currentState == MovementState.DASHING) {
					if (wallReflection) {
						currentSpeed.x *= wallSpeedReflectionFactor;
						currentOffset.x *= wallSpeedReflectionFactor;
					} else {
						float magnitude = Mathf.Abs(currentSpeed.magnitude);
						currentSpeed.x = 0f;
						currentOffset.x = 0f;
						currentSpeed.y = goingUp ? magnitude : magnitude * -1f;
					}
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
						if (!goingUp && currentState != MovementState.HIT_STUN && currentState != MovementState.RAG_DOLL && currentState != MovementState.DASHING) {
							currentState = MovementState.GROUNDED;
						}
					}
					if (distanceForFrame.y == 0f)
						break;
				}
			}
			if (hitY) {
				if (currentState == MovementState.HIT_STUN || currentState == MovementState.RAG_DOLL) {
					currentSpeed.y *= wallSpeedReflectionFactor;
					currentOffset.y *= wallSpeedReflectionFactor;
				} else if (currentState == MovementState.DASHING) {
					if (wallReflection) {
						currentSpeed.y *= wallSpeedReflectionFactor;
						currentOffset.y *= wallSpeedReflectionFactor;
					} else {
						float magnitude = Mathf.Abs(currentSpeed.magnitude);
						currentSpeed.y = 0f;
						currentOffset.y = 0f;
						currentSpeed.x = goingRight ? magnitude : magnitude * -1f;
					}
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
            if (currentState == MovementState.HIT_STUN || currentState == MovementState.RAG_DOLL) {
                currentSpeed *= 0.9f;
                return;
            }

			Vector3 newMax = new Vector3(MaxSpeed() * currentControllerState.x, MaxSpeed() * currentControllerState.y);
			if (newMax.magnitude > MaxSpeed()) {
				newMax *= MaxSpeed() / newMax.magnitude;
			}
			if (currentSpeed.magnitude > MaxSpeed()) {
				currentSpeed *= MaxSpeed() / currentSpeed.magnitude;
			}
			// This is how far we are from that speed.
			Vector3 difference = newMax - currentSpeed;
			float usableAcceleratior = HasPowerup(Powerup.PERFECT_ACCELERATION) ? MaxSpeed() : GetCurrentAcceleration();
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
				if (currentState == MovementState.DASHING) {
					if (wallReflection) {
						currentSpeed.x *= wallSpeedReflectionFactor;
						currentOffset.x *= wallSpeedReflectionFactor;
					} else {
						float magnitude = Mathf.Abs(currentSpeed.magnitude);
						currentSpeed.x = 0f;
						currentOffset.x = 0f;
						currentSpeed.y = goingUp ? magnitude : magnitude * -1f;
					}
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
					if (wallReflection) {
						currentSpeed.y *= wallSpeedReflectionFactor;
						currentOffset.y *= wallSpeedReflectionFactor;
					} else {
						float magnitude = Mathf.Abs(currentSpeed.magnitude);
						currentSpeed.y = 0f;
						currentOffset.y = 0f;
						currentSpeed.x = goingRight ? magnitude : magnitude * -1f;
					}
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
			Vector3 nameScale = nameCanvas.transform.localScale;
			nameScale.x *= -1;
			nameCanvas.transform.localScale = nameScale;
		}

		#endregion

		private void ResetVolume() {
			float volume = ControlBindingContainer.GetInstance().effectVolume;
			if (volume != lastVolume) {
				jumpSource.volume = volume * 0.15f;
				doubleJumpSource.volume = volume * 0.15f;
				dashSource.volume = volume * 0.4f;
				hitSource.volume = volume * 0.6f;
				deathSource.volume = volume * 0.5f;
				relightSource.volume = volume * 0.5f;
				lastVolume = volume;
			}
		}

		private void UpdateStateFromTimers() {
            if (awaitingTimer) {
                switch (currentState) {
                    case MovementState.DASHING:
                        if (Time.time >= stateTimerChange + dashDuration) {
                            awaitingTimer = false;
                            // Note that we do not clear the stateTimerChange variable here because we still need it for the dash cooldown.
                            // We clear it everywhere else to avoid extending the dash cooldown out of hit stun.
                            currentState = MovementState.FALLING;
                        }
                        break;
                    case MovementState.PARRYING:
                        if (Time.time >= stateTimerChange + parryDuration) {
                            awaitingTimer = false;
                            stateTimerChange = 0f;
                            currentState = MovementState.FALLING;
                        }
                        break;
                    case MovementState.HIT_FREEZE:
                        if (Time.time >= stateTimerChange + freezeTime) {
                            currentState = OutOfHealth() ? MovementState.RAG_DOLL : MovementState.HIT_STUN;
                            currentSpeed = damageSpeed;
                        }
                        break;
                    case MovementState.HIT_STUN:
                        if (Time.time >= stateTimerChange + freezeTime + stunTime) {
                            awaitingTimer = false;
                            stateTimerChange = 0f;
                            currentState = MovementState.FALLING;
                        }
                        break;
                    case MovementState.RAG_DOLL:
                        if (Time.time >= stateTimerChange + freezeTime + deathAnimationTime) {
                            awaitingTimer = false;
                            stateTimerChange = 0f;
                            currentState = MovementState.FALLING;
                        }
                        break;
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
			animator.SetBool("DyingAnimation", currentState == MovementState.RAG_DOLL);
		}

		public virtual void InputsReceived(float horizontalScale, float verticalScale, bool grabHeld) {
            if (!ControlsFrozen) {
                currentControllerState = new Vector3(horizontalScale, verticalScale);
                this.grabHeld = grabHeld;
            }
		}

		public virtual void ActionPrimaryPressed(Vector3 mouseDirection) {
            if (!ControlsFrozen) actionPrimaryHeld = true;
		}

		public virtual void ActionPrimaryReleased() {
			if (!ControlsFrozen) actionPrimaryHeld = false;
		}

		public virtual void ActionSecondaryPressed(Vector3 mouseDirection) {
            if (!ControlsFrozen) actionSecondaryHeld = true;
		}

		public virtual void ActionSecondaryReleased() {
            if (!ControlsFrozen) actionSecondaryHeld = false;
		}

		public virtual void LightTogglePressed() {
			// ignored callback.
		}

		#region Control callbacks.

		// These callbacks are not used by every character, so they should be called by children of this class when needed.

		protected void JumpPhysics(bool doubleJump) {
			switch (currentState) {
				case MovementState.GROUNDED:
				case MovementState.JUMPING:
				case MovementState.FALLING:
					currentSpeed.y = MaxSpeed() * JumpFactor();
					currentState = MovementState.JUMPING;
					break;
				case MovementState.WALL_SLIDE_LEFT:
                	currentSpeed.y = Mathf.Sin(Mathf.PI / 4) * MaxSpeed() * WallJumpFactor();
                	currentSpeed.x = Mathf.Cos(Mathf.PI / 4) * MaxSpeed() * WallJumpFactor();
					currentState = MovementState.JUMPING;
					break;
				case MovementState.WALL_SLIDE_RIGHT:
                	currentSpeed.y = Mathf.Sin(Mathf.PI * 3 / 4) * MaxSpeed() * WallJumpFactor();
                	currentSpeed.x = Mathf.Cos(Mathf.PI * 3 / 4) * MaxSpeed() * WallJumpFactor();
					currentState = MovementState.JUMPING;
					break;
			}
			if (photonView.isMine) {
				PlayJumpSound(doubleJump);
				photonView.RPC("PlayJumpSound", PhotonTargets.Others, doubleJump);
			}
		}

		[PunRPC]
		public virtual void PlayJumpSound(bool doubleJump) {
			if (doubleJump) {
				doubleJumpSource.Play();
			} else {
				jumpSource.Play();
			}
		}

		protected bool DashPhysics(Vector3 mouseDirection) {
            // Cannot dash out of certain states.
			if (currentState == MovementState.PARRYING ||
                currentState == MovementState.HIT_FREEZE || 
                currentState == MovementState.HIT_STUN ||
                currentState == MovementState.RAG_DOLL ||
                currentState == MovementState.DASHING)  {
				return false;
			}

            // Cannot dash if on cooldown.
            if (Time.time < stateTimerChange + DashCooldown()) {
                return false;
            }

			currentState = MovementState.DASHING;
            awaitingTimer = true;
            stateTimerChange = Time.time;

			if (mouseDirection.magnitude != 0f) {
				currentSpeed = mouseDirection * MaxSpeed() * DashFactor() / mouseDirection.magnitude;
			} else if (currentControllerState.magnitude != 0f) {
            	currentSpeed = currentControllerState * MaxSpeed() * DashFactor() / currentControllerState.magnitude;
			} else {
				currentSpeed = new Vector3(0, -1f, 0) * MaxSpeed() * DashFactor();
			}

			if (photonView.isMine) {
				PlayDashSound();
				photonView.RPC("PlayDashSound", PhotonTargets.Others);
			}
			return true;
		}

		[PunRPC]
		public virtual void PlayDashSound() {
			dashSource.Play();
		}

		protected void AttackPhysics() {
			currentSpeed *= -0.5f;
		}

		protected void TakeDamage(Vector3 hitPosition, Vector3 hitSpeed, int damage, float freezeTime, float stunTime) {
            if (currentState == MovementState.HIT_FREEZE || currentState == MovementState.RAG_DOLL || OutOfHealth()) return;
            // transform.position = hitPosition;
            damageSpeed = hitSpeed;
            if (photonView.isMine) {
                SubtractHealth(damage);
            }
            this.freezeTime = freezeTime;
            this.stunTime = stunTime;

            currentState = MovementState.HIT_FREEZE;
            awaitingTimer = true;
            stateTimerChange = Time.time;
		}

        protected abstract void SubtractHealth(int health);

        public abstract bool OutOfHealth();

		[PunRPC]
		public void PlayHitSound() {
			hitSource.Play();
		}

		[PunRPC]
		public void PlayDeathSound() {
			deathSource.Play();
		}

		[PunRPC]
		public void PlayRelightSound() {
			relightSource.Play();
		}

		#endregion

		#region Overridable variable accessors.

		// Some characters have conditional changes to these values, such as upgrades of powerups 
		// that modify the values temporarily.  Overriding these will change how the numbers are 
		// used in the base physics calculations.

		protected virtual float MaxSpeed() {
            float talentModifier = talentRanks[TalentEnum.MOVEMENT_SPEED] == 3 ? 0.1f : talentRanks[TalentEnum.MOVEMENT_SPEED] * 0.03f;
			float speedModifier = 1.0f + talentModifier;
			return maxSpeed * speedModifier;
		}

        protected virtual float JumpFactor() {
			float modifier = 1.0f + (0.05f * talentRanks[TalentEnum.JUMP_HEIGHT_OR_ACCELERATION]);
            return jumpFactor * modifier;
        }

        protected virtual float WallJumpFactor() {
			float modifier = 1.0f + (0.05f * talentRanks[TalentEnum.JUMP_HEIGHT_OR_ACCELERATION]);
            return wallJumpFactor * modifier;
        }

		protected virtual float DashFactor() {
			return dashFactor;
		}

		protected virtual float GetCurrentAcceleration() {
			float talentModifier = 1.0f + (talentRanks[TalentEnum.JUMP_HEIGHT_OR_ACCELERATION] * 0.05f);
			return baseAcceleration * talentModifier;
		}

		protected virtual float DashCooldown() {
			float talentModifier = 1.0f - (talentRanks[TalentEnum.ATTACK_COOLDOWN] * 0.05f);
			return dashCooldown * talentModifier;
		}

		#endregion

		public virtual void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
			if (stream.isWriting) {
				stream.SendNext(currentState);
				stream.SendNext(awaitingTimer);
				stream.SendNext(Time.time - stateTimerChange);
				stream.SendNext(transform.position);
				stream.SendNext(currentSpeed);
				stream.SendNext(currentControllerState);
				stream.SendNext(actionPrimaryHeld);
				stream.SendNext(actionSecondaryHeld);
				stream.SendNext(grabHeld);
			} else {
				currentState = (MovementState)stream.ReceiveNext();
				awaitingTimer = (bool)stream.ReceiveNext();
				stateTimerChange = Time.time - (float)stream.ReceiveNext();
				Vector3 networkPosition = (Vector3)stream.ReceiveNext();
				currentSpeed = (Vector3)stream.ReceiveNext();
				currentControllerState = (Vector3)stream.ReceiveNext();
				actionPrimaryHeld = (bool)stream.ReceiveNext();
				actionSecondaryHeld = (bool)stream.ReceiveNext();
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
