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
			lightBox.DefaultScale = new Vector3(defaultScale, defaultScale);
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
					BaseExplorer behavior = collider.gameObject.GetComponent<BaseExplorer>();
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
			lightBox.IsDead = IsDead();
			if (IsDead()) {
				bool amNightmare = GlobalPlayerContainer.Instance.TeamSelection == GlobalPlayerContainer.NIGHTMARE;
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
			if (Time.time - damageTime < damageRecovery || IsOutOfHealth())
				return;
			this.currentSpeed = currentSpeed;
			damageTime = Time.time;
			currentHealth -= 1;
			currentState = MovementState.DAMAGED;
			DieIfAble();
		}

		private void DieIfAble() {
			if (currentHealth <= 0) {
				currentHealth = 0;
				currentState = MovementState.DYING;
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
    }
}
