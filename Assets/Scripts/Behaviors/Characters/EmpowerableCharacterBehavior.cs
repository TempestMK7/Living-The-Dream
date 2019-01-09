using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using com.cygnusprojects.TalentTree;

namespace Com.Tempest.Nightmare {

	public abstract class EmpowerableCharacterBehavior : Photon.PunBehaviour {

		public float powerupDuration = 60f;

		private Dictionary<Powerup, float> powerupDictionary;
		protected TalentManagerBehavior talentManager;

		private int numUpgrades;

		protected int networkSightRange = 0;
		protected int networkShrineDuration = 0;
		protected int networkBonfireSpeed = 0;
		protected int networkUpgradeModifier = 0;
		protected int networkJumpHeight = 0;
		protected int networkMovementSpeed = 0;
		protected int networkReducedGravity = 0;
		protected int networkJetpackForce = 0;
		protected int networkResetDash = 0;

		public virtual void Awake() {
			powerupDictionary = new Dictionary<Powerup, float>();
			if (photonView.isMine) {
				talentManager = FindObjectOfType<TalentManagerBehavior>();
			}
		}

		[PunRPC]
		public void AddPowerup(Powerup p) {
			powerupDictionary[p] = Time.time;
			if (photonView.isMine) {
				GeneratedGameManager manager = FindObjectOfType<GeneratedGameManager>();
				if (manager != null) {
					manager.DisplayAlert("You have been granted " + p.ToString(), PlayerStateContainer.Instance.TeamSelection);
				} else {
					FindObjectOfType<DemoSceneManager>().DisplayAlert("You have been granted " + p.ToString(), PlayerStateContainer.Instance.TeamSelection);
				}
			}
		}

		public void AddRandomPowerup() {
			if (!photonView.isMine)
				return;
			Powerup[] possiblePowerups = GetUsablePowerups();
			photonView.RPC("AddPowerup", PhotonTargets.All, possiblePowerups[Random.Range(0, possiblePowerups.Length)]);
		}

		public bool HasPowerup(Powerup p) {
			float powerupExtension = networkShrineDuration * 5f;
			return powerupDictionary.ContainsKey(p) && Time.time - powerupDictionary[p] < powerupDuration + powerupExtension;
		}

		protected abstract Powerup[] GetUsablePowerups();

		public void AddUpgrade() {
			numUpgrades++;
			GeneratedGameManager manager = FindObjectOfType<GeneratedGameManager>();
			string message = PlayerStateContainer.Instance.TeamSelection == PlayerStateContainer.NIGHTMARE ? "Your attack has been upgraded." : "Your movement has been upgraded.";
			if (manager != null) {
				manager.DisplayAlert(message, PlayerStateContainer.Instance.TeamSelection);
			} else {
				FindObjectOfType<DemoSceneManager>().DisplayAlert(message, PlayerStateContainer.Instance.TeamSelection);
			}
		}
		
		// This should be used to call the ReceiveTalents RPC with this character's version of each talent in the talent tree.
		public abstract void SendTalentsToNetwork();

		[PunRPC]
		public void ReceiveTalents(int sightRange, int shrineDuration, int bonfireSpeed, int upgradeModifier, int jumpHeight, int movementSpeed, int reducedGravity, int jetpackForce, int resetDash) {
			networkSightRange = sightRange;
			networkShrineDuration = shrineDuration;
			networkBonfireSpeed = bonfireSpeed;
			networkUpgradeModifier = upgradeModifier;
			networkJumpHeight = jumpHeight;
			networkMovementSpeed = movementSpeed;
			networkReducedGravity = reducedGravity;
			networkJetpackForce = jetpackForce;
			networkResetDash = resetDash;
		}

		public int GetBonfireSpeed() {
			return networkBonfireSpeed;
		}

		public float GetNumUpgrades() {
			float upgradeModifier = 1.0f + (0.05f * networkUpgradeModifier);
			return numUpgrades * upgradeModifier;
		}

		public int GetUnmodifiedUpgrades() {
			return numUpgrades;
		}
	}
}
