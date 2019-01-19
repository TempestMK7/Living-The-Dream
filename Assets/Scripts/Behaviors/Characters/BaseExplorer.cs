using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Com.Tempest.Nightmare {

    public abstract class BaseExplorer : PhysicsCharacter {

		// Player rule params.
		public int maxHealth = 3;
		public int maxLives = 3;
        
        // Health bar timer, time is in seconds.
		public float healthBarFadeDelay = 1f;
        public float deathRenderTime = 3f;

		public LayerMask whatIsBonfire;
		public LayerMask whatIsExplorer;
        
		// Light box params.
		public float defaultScale = 6f;
		public float activeScale = 40f;

		// Internal objects accessed by this behavior.
		protected LightBoxBehavior lightBox;
		private GameObject healthCanvas;
		private Image positiveHealthBar;
		private Renderer myRenderer;

        // Health values.
		private int currentHealth;
		private int currentLives;

        private float damageTime;

        public override void Awake() {
			base.Awake();
            // Handle character's light box.
			lightBox = GetComponentInChildren<LightBoxBehavior>();
			lightBox.IsMine = photonView.isMine;
			lightBox.IsActive = false;
			lightBox.IsDead = false;
			lightBox.DefaultScale = new Vector3(GetDefaultScale(), GetDefaultScale());
			lightBox.ActiveScale = new Vector3(activeScale, activeScale);

			// Setup internal components and initialize object variables.
			healthCanvas = transform.Find("DreamerCanvas").gameObject;
			positiveHealthBar = healthCanvas.transform.Find("PositiveHealth").GetComponent<Image>();
			myRenderer = GetComponent<Renderer>();

			// Initialize state values.
			currentHealth = maxHealth;
			currentLives = maxLives;
        }

        public override void Update() {
			base.Update();
			ResurrectIfAble();
			HandleLifeState();
			HandlePowerupState();
			DeleteSelfIfAble();
        }

        // Brings the player back to life if they are within range of a bonfire that has living players near it.
		private void ResurrectIfAble() {
			if (!photonView.isMine || !IsDead() || IsOutOfLives())
				return;
			bool ableToRes = false;
			BaseExplorer savior = null;	
			Collider2D[] bonfires = Physics2D.OverlapAreaAll(boxCollider.bounds.min, boxCollider.bounds.max, whatIsBonfire);
			foreach (Collider2D fireCollider in bonfires) {
				BonfireBehavior behavior = fireCollider.gameObject.GetComponent<BonfireBehavior>();
				if (behavior != null && behavior.IsLit()) {
					ableToRes = true;
				}
			}
			Collider2D[] players = Physics2D.OverlapAreaAll(boxCollider.bounds.min, boxCollider.bounds.max, whatIsExplorer);
			foreach (Collider2D collider in players) {
				BaseExplorer behavior = collider.gameObject.GetComponent<BaseExplorer>();
				if (behavior != null && !behavior.IsOutOfHealth()) {
					ableToRes = true;
					behavior.photonView.RPC("ReceiveRescueEmbers", PhotonTargets.All, 10);
					savior = behavior;
				}
			}
			if (ableToRes) {
				currentHealth = maxHealth;
				GeneratedGameManager behavior = FindObjectOfType<GeneratedGameManager>();
				behavior.photonView.RPC("DisplayAlert", PhotonTargets.Others, "An explorer has been saved!  His light shines once more.", PlayerStateContainer.EXPLORER);
				behavior.DisplayAlert("You have been saved!  Your light shines once more.", PlayerStateContainer.EXPLORER);
				PlayRelightSound();
				photonView.RPC("PlayRelightSound", PhotonTargets.Others);
			}
		}

        // Draws current health total, switches layers based on health totals, and hides player to other players if dead.
		private void HandleLifeState() {
			lightBox.IsDead = IsDead();
			if (IsDead()) {
				bool amNightmare = PlayerStateContainer.Instance.TeamSelection == PlayerStateContainer.NIGHTMARE;
				gameObject.layer = LayerMask.NameToLayer("Death");
				positiveHealthBar.fillAmount = 0f;
				healthCanvas.SetActive(!amNightmare);
				ToggleRenderers(!amNightmare);
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
			if (myRenderer.enabled != enabled) {
				myRenderer.enabled = enabled;
				Renderer[] childRenderers = gameObject.GetComponentsInChildren<Renderer>();
				foreach (Renderer childRenderer in childRenderers) {
					if (childRenderer.gameObject.GetComponent<LightBoxBehavior>() == null) {
						childRenderer.enabled = enabled;
					}
				}
			}
		}

		[PunRPC]
		public override void PlayJumpSound(bool doubleJump) {
			if (PlayerStateContainer.Instance.TeamSelection != PlayerStateContainer.NIGHTMARE || !IsOutOfHealth()) {
				base.PlayJumpSound(doubleJump);
			}
		}

		[PunRPC]
		public override void PlayDashSound() {
			if (PlayerStateContainer.Instance.TeamSelection != PlayerStateContainer.NIGHTMARE || !IsOutOfHealth()) {
				base.PlayDashSound();
			}
		}

        private void HandlePowerupState() {
			if (HasPowerup(Powerup.BETTER_VISION)) {
				lightBox.DefaultScale = new Vector3(GetDefaultScale() * 3f, GetDefaultScale() * 3f);
			} else {
				lightBox.DefaultScale = new Vector3(GetDefaultScale(), GetDefaultScale());
			}
		}

        #region HealthState

		public bool IsOutOfHealth() {
			return currentHealth <= 0;
		}

		// Returns whether or not the player is currently dead (out of health but still in the game).
		public bool IsDead() {
			return currentHealth <= 0 && Time.time - damageTime > deathRenderTime;
		}

		// Returns whether or not the player is out of the game (out of death time).
		public bool IsOutOfLives() {
			return currentLives <= 0;
		}

		#endregion HealthState

		public override void LightTogglePressed() {
			base.LightTogglePressed();
			if (!IsOutOfHealth()) {
				lightBox.IsActive = !lightBox.IsActive;
			}
		}

		#region DamageHandling

		// Called by a nightmare behavior when collision occurs.
		[PunRPC]
		public void TakeDamage(Vector3 currentSpeed) {
			if (currentState == MovementState.DAMAGED || currentState == MovementState.DYING || IsOutOfHealth()) return;
			damageTime = Time.time;
			if (!photonView.isMine) return;
			currentHealth -= 1;
			DamagePhysics(currentSpeed, IsOutOfHealth());
			DieIfAble();
			if (!IsOutOfHealth()) {
				PlayHitSound();
				photonView.RPC("PlayHitSound", PhotonTargets.Others);
			}
		}

		private void DieIfAble() {
			if (currentHealth <= 0) {
				currentHealth = 0;
				currentState = MovementState.DYING;
				currentLives--;
				if (photonView.isMine) {
					PlayDeathSound();
					photonView.RPC("PlayDeathSound", PhotonTargets.Others);
					GeneratedGameManager behavior = FindObjectOfType<GeneratedGameManager>();
					if (IsOutOfLives()) {
						behavior.DisplayAlert("Your light has gone out forever.  You can still spectate though.", PlayerStateContainer.EXPLORER);
						behavior.photonView.RPC("DisplayAlert", PhotonTargets.Others, "An explorer has fallen, his light is out forever.", PlayerStateContainer.EXPLORER);
					} else {
						behavior.DisplayAlert("Your light has gone out!  Go to a lit bonfire or another player to relight it.", PlayerStateContainer.EXPLORER);
						behavior.photonView.RPC("DisplayAlert", PhotonTargets.Others, "Someone's light has gone out!  Help them relight it by finding them.", PlayerStateContainer.EXPLORER);
					}
				}
			}
		}

		private void DeleteSelfIfAble() {
			if (photonView.isMine && IsOutOfLives() && Time.time - damageTime > deathRenderTime) {
				GeneratedGameManager gameManager = FindObjectOfType<GeneratedGameManager>();
				gameManager.Explorer = null;
				gameManager.ChangeMaskColor(0.5f);
				PhotonNetwork.Destroy(photonView);
			}
		}

		#endregion DamageHandling

		public override void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
			base.OnPhotonSerializeView(stream, info);
			if (stream.isWriting) {
				stream.SendNext(currentHealth);
				stream.SendNext(lightBox.IsActive);
			} else {
				currentHealth = (int)stream.ReceiveNext();
				lightBox.IsActive = (bool)stream.ReceiveNext();
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

		public float GetDefaultScale() {
			float defaultScaleModifier = (networkSightRange * 0.05f) + 1.0f;
			return defaultScale * defaultScaleModifier;
		}

		[PunRPC]
		public void ReceiveObjectiveEmbers(float embers) {
			if (!photonView.isMine) return;
			PlayerStateContainer.Instance.ObjectiveEmbers += embers;
		}

		[PunRPC]
		public void ReceiveRescueEmbers(int embers) {
			if (!photonView.isMine) return;
			PlayerStateContainer.Instance.ObjectiveEmbers += embers;
		}

		[PunRPC]
		public void ReceiveUpgradeEmbers(int embers) {
			if (!photonView.isMine)  return;
			PlayerStateContainer.Instance.UpgradeEmbers += embers;
		}
    }
}
