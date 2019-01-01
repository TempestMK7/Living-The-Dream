using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Com.Tempest.Nightmare {

    public abstract class BaseNightmare : PhysicsCharacter {
        
		public LayerMask whatIsPlayer;
		public float lightBoxScale = 20f;

		protected LightBoxBehavior lightBox;

        public override void Awake() {
            base.Awake();

			lightBox = GetComponentInChildren<LightBoxBehavior>();
			lightBox.IsMine = photonView.isMine;
			lightBox.IsActive = true;
			lightBox.DefaultScale = new Vector3(lightBoxScale, lightBoxScale);
			lightBox.ActiveScale = new Vector3(lightBoxScale, lightBoxScale);
        }

        public override void Update() {
            base.Update();
            
            HandlePowerupState();
        }

		protected virtual void HandlePowerupState() {
			if (HasPowerup(Powerup.BETTER_VISION)) {
				lightBox.DefaultScale = new Vector3(lightBoxScale * 2f, lightBoxScale * 2f);
			} else {
				lightBox.DefaultScale = new Vector3(lightBoxScale, lightBoxScale);
			}
			lightBox.ActiveScale = lightBox.DefaultScale;
		}

		protected override Powerup[] GetUsablePowerups() {
			return new Powerup[] {
				Powerup.BETTER_VISION,
				Powerup.DREAMER_VISION,
				Powerup.PERFECT_ACCELERATION,
				Powerup.HALF_ABILITY_COOLDOWN
			};
		}

		[PunRPC]
		public void ReceiveObjectiveEmbers(float embers) {
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
