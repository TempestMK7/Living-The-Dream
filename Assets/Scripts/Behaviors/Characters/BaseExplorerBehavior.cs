using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Com.Tempest.Nightmare {

	public abstract class BaseExplorerBehavior : EmpowerableCharacterBehavior, IControllable, IPunObservable {

		#region SerializedVariables

		// Player rule params.
		public int maxHealth = 3;
		public int maxLives = 3;

		// Recovery timers.  Values are in seconds.
		public float jumpRecovery = 0.2f;
		public float wallJumpRecovery = 0.2f;
		public float damageRecovery = 0.5f;
		public float deathAnimationTime = 3f;
		public float healthBarFadeDelay = 1f;

		// Player movement params.
		public float maxSpeed = 7f;
		public float gravityFactor = 3f;
		public float terminalVelocityFactor = 2f;
		public float risingGravityBackoffFactor = 1f;
		public float jumpFactor = 1.8f;
		public float wallJumpFactor = 1.5f;
		public float wallSlideFactor = 0.3f;
		public float wallJumpControlFactor = 5f;

		// Player hit calculation params.
		public float rayBoundShrinkage = 0.001f;
		public int numRays = 4;
		public LayerMask whatIsSolid;
		public LayerMask whatIsBonfire;
		public LayerMask whatIsExplorer;

		// Light box params.
		public float defaultScale = 6f;
		public float activeScale = 40f;

		#endregion

		#region InternalVariables

		// Internal objects accessed by this behavior.
		protected LightBoxBehavior lightBox;
		private GameObject healthCanvas;
		private Image positiveHealthBar;
		private BoxCollider2D boxCollider;
		private Animator animator;
		private Renderer myRenderer;
		private Vector3 currentOffset;

		protected Vector3 currentSpeed;
		protected Vector3 currentControllerState;

		// Booleans used when deciding how to respond to collisions and controller inputs.
		protected bool grabHeld;
		protected bool grounded;
		protected bool holdingWallLeft;
		protected bool holdingWallRight;

		// Health values.
		private int currentHealth;
		private int currentLives;

		// Timer values, recorded in seconds.
		protected float jumpTime;
		protected float wallJumpTime;
		protected float damageTime;
		protected float deathEventTime;

		#endregion

		public override void Awake() {
			base.Awake();

			// Handle character's light box.
			lightBox = GetComponentInChildren<LightBoxBehavior>();
			lightBox.IsMine = photonView.isMine;
			lightBox.IsActive = false;
			lightBox.DefaultScale = new Vector3(defaultScale, defaultScale);
			lightBox.ActiveScale = new Vector3(activeScale, activeScale);

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
			currentLives = maxLives;
		}

		#region Update

		// Update is called once per frame
		public virtual void Update() {
			UpdateHorizontalMovement();
			UpdateVerticalMovement();
			MoveAsFarAsYouCan();
			HandleAnimator();
			ResurrectIfAble();
			HandleLifeState();
			HandlePowerupState();
		}

		// Updates horizontal movement based on controller state.
		// Does nothing if this character belongs to another player.
		private void UpdateHorizontalMovement() {
			if (!photonView.isMine)
				return;
			if (Time.time - deathEventTime < deathAnimationTime) {
				currentSpeed.x -= currentSpeed.x * Time.deltaTime;
			} else if (Time.time - damageTime < damageRecovery) {
				currentSpeed += currentControllerState * maxSpeed * maxSpeed * 2f * Time.deltaTime;
			} else if (grabHeld && (holdingWallLeft || holdingWallRight)) {
				currentSpeed.x = 0;
			} else if (Time.time - wallJumpTime < wallJumpRecovery) {
				currentSpeed.x += currentControllerState.x * maxSpeed * Time.deltaTime * wallJumpControlFactor;
				if (currentSpeed.x > maxSpeed)
					currentSpeed.x = maxSpeed;
				else if (currentSpeed.x < maxSpeed * -1f)
					currentSpeed.x = maxSpeed * -1f;
			} else {
				currentSpeed.x = currentControllerState.x * maxSpeed;
			}
		}

		// Updates vertical movement based on gravity.
		protected virtual void UpdateVerticalMovement() {
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
							GrabbedWall(false);
						} else {
							holdingWallLeft = true;
							GrabbedWall(true);
						}
					}
					if (distanceForFrame.x == 0f)
						break;
				}
			}
			if (hitX) {
				if (Time.time - damageTime < damageRecovery || Time.time - deathEventTime < deathAnimationTime) {
					holdingWallLeft = false;
					holdingWallRight = false;
					currentSpeed.x *= -1f;
					currentOffset.x *= -1f;
				} else {
					currentSpeed.x = 0f;
					currentOffset.x = 0f;
					if (currentSpeed.y < maxSpeed * wallSlideFactor * -1f)
						currentSpeed.y = maxSpeed * wallSlideFactor * -1f;
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
							BecameGrounded();
						}
					}
					if (distanceForFrame.y == 0f)
						break;
				}
			}
			if (hitY) {
				currentSpeed.y *= Time.time - damageTime < damageRecovery ? -1f : 0f;
				currentOffset.y *= Time.time - damageTime < damageRecovery ? -1f : 0f;
				grounded = grounded && Time.time - damageTime > damageRecovery;
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
			if (currentSpeed.x < 0f)
				speed = -1;
			else if (currentSpeed.x > 0f)
				speed = 1;
			animator.SetInteger("HorizontalSpeed", speed);
			bool dyingAnimation = IsOutOfHealth() && !IsDead();
			animator.SetBool("DyingAnimation", dyingAnimation);
		}

		// Brings the player back to life if they are within range of a bonfire that has living players near it.
		private void ResurrectIfAble() {
			if (!photonView.isMine || !IsDead() || IsOutOfLives())
				return;
			Collider2D[] bonfires = Physics2D.OverlapAreaAll(boxCollider.bounds.min, boxCollider.bounds.max, whatIsBonfire);
			foreach (Collider2D fireCollider in bonfires) {
				BonfireBehavior behavior = fireCollider.gameObject.GetComponent<BonfireBehavior>();
				if (behavior == null)
					continue;
				if (behavior.IsLit()) {
					currentHealth = maxHealth;
				}
			}
			if (IsDead()) {
				Collider2D[] players = Physics2D.OverlapAreaAll(boxCollider.bounds.min, boxCollider.bounds.max, whatIsExplorer);
				foreach (Collider2D collider in players) {
					BaseExplorerBehavior behavior = collider.gameObject.GetComponent<BaseExplorerBehavior>();
					if (behavior == null)
						continue;
					if (!behavior.IsOutOfHealth()) {
						currentHealth = maxHealth;
					}
				}
			}
		}

		// Draws current health total, switches layers based on health totals, and hides player to other players if dead.
		private void HandleLifeState() {
			if (IsDead()) {
				gameObject.layer = LayerMask.NameToLayer("Death");
				positiveHealthBar.fillAmount = 0f;
				bool showPlayer = GlobalPlayerContainer.Instance.TeamSelection != GlobalPlayerContainer.NIGHTMARE;
				healthCanvas.SetActive(showPlayer);
				ToggleRenderers(showPlayer);
				lightBox.IsActive = false;
			} else {
				gameObject.layer = LayerMask.NameToLayer(IsOutOfHealth() ? "Death" : "Explorer");
				positiveHealthBar.fillAmount = (float)currentHealth / (float)maxHealth;
				healthCanvas.SetActive(Time.time - damageTime < healthBarFadeDelay);
				ToggleRenderers(true);
			}
		}

		// Toggles base renderer and health canvas if necessary.
		// Prevents multiple calls to change enabled state.
		private void ToggleRenderers(bool enabled) {
			if (myRenderer.enabled != enabled)
				myRenderer.enabled = enabled;
		}

		private void HandlePowerupState() {
			if (HasPowerup(Powerup.BETTER_VISION)) {
				lightBox.DefaultScale = new Vector3(defaultScale * 3f, defaultScale * 3f);
			} else {
				lightBox.DefaultScale = new Vector3(defaultScale, defaultScale);
			}
		}

		// Called internally to let sub classes know what our state is.
		public abstract void BecameGrounded();

		public abstract void GrabbedWall(bool grabbedLeft);

		#endregion Update

		#region HealthState

		public bool IsOutOfHealth() {
			return currentHealth <= 0;
		}

		// Returns whether or not the player is currently dead (out of health but still in the game).
		public bool IsDead() {
			return currentHealth <= 0 && Time.time - deathEventTime > deathAnimationTime;
		}

		// Returns whether or not the player is out of the game (out of death time).
		public bool IsOutOfLives() {
			return currentLives <= 0;
		}

		#endregion HealthState

		#region InputHandling

		// Called by the input manager to move our character.
		public abstract void InputsReceived(float horizontalScale, float verticalScale, bool grabHeld);

		public abstract void ActionPressed();

		public abstract void ActionReleased();

		public void LightTogglePressed() {
			if (!IsOutOfHealth()) {
				lightBox.IsActive = !lightBox.IsActive;
			}
		}

		#endregion InputHandling

		#region DamageHandling

		// Called by a nightmare behavior when collision occurs.
		[PunRPC]
		public void TakeDamage(Vector3 currentSpeed) {
			if (Time.time - damageTime < damageRecovery || IsOutOfHealth())
				return;
			this.currentSpeed = currentSpeed;
			damageTime = Time.time;
			currentHealth -= 1;
			DieIfAble();
		}

		private void DieIfAble() {
			if (currentHealth <= 0) {
				currentHealth = 0;
				deathEventTime = Time.time;
				currentLives--;
				if (photonView.isMine) {
					GeneratedGameManager behavior = FindObjectOfType<GeneratedGameManager>();
					if (IsOutOfLives()) {
						behavior.DisplayAlert("Your light has gone out forever.  You can still spectate though.", GlobalPlayerContainer.EXPLORER);
						behavior.photonView.RPC("DisplayAlert", PhotonTargets.Others, "An explorer has fallen, his light is out forever.", GlobalPlayerContainer.EXPLORER);
						DeleteSelf();
					} else {
						behavior.DisplayAlert("Your light has gone out!  Go to a lit bonfire or another player to relight it.", GlobalPlayerContainer.EXPLORER);
						behavior.photonView.RPC("DisplayAlert", PhotonTargets.Others, "Someone's light has gone out!  Help them relight it by finding them.", GlobalPlayerContainer.EXPLORER);
					}
				}
			}
		}

		private void DeleteSelf() {
			if (photonView.isMine && IsOutOfLives()) {
				GeneratedGameManager gameManager = FindObjectOfType<GeneratedGameManager>();
				gameManager.Explorer = null;
				gameManager.ChangeMaskColor(0.5f);
				PhotonNetwork.Destroy(photonView);
				return;
			}
		}

		#endregion DamageHandling

		// Called by Photon whenever player state is synced across the network.
		public virtual void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
			if (stream.isWriting) {
				stream.SendNext(transform.position);
				stream.SendNext(currentSpeed);
				stream.SendNext(grabHeld);
				stream.SendNext(currentHealth);
				stream.SendNext(lightBox.IsActive);
			} else {
				Vector3 networkPosition = (Vector3)stream.ReceiveNext();
				currentSpeed = (Vector3)stream.ReceiveNext();
				grabHeld = (bool)stream.ReceiveNext();
				currentHealth = (int)stream.ReceiveNext();
				lightBox.IsActive = (bool)stream.ReceiveNext();

				currentOffset = networkPosition - transform.position;
				if (currentOffset.magnitude > 3f) {
					currentOffset = new Vector3();
					transform.position = networkPosition;
				}
			}
		}

		// Called within EmpowerableCharacterBehavior to determine which powerups this character is eligible for.
		protected override Powerup[] GetUsablePowerups() {
			return new Powerup[] {
				Powerup.BETTER_VISION,
				Powerup.NIGHTMARE_VISION,
				Powerup.THIRD_JUMP,
				Powerup.DOUBLE_OBJECTIVE_SPEED
			};
		}
	}
}
