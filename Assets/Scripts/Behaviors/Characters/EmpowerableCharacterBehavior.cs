using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Tempest.Nightmare {

	public abstract class EmpowerableCharacterBehavior : Photon.PunBehaviour {

		public float powerupDuration = 60f;

		private Dictionary<Powerup, float> powerupDictionary;

		private int numUpgrades = 0;

		public virtual void Awake() {
			powerupDictionary = new Dictionary<Powerup, float>();
		}

		[PunRPC]
		public void AddPowerup(Powerup p) {
			powerupDictionary[p] = Time.time;
			if (photonView.isMine) {
				FindObjectOfType<GeneratedGameManager>().DisplayAlert("You have been granted " + p.ToString(), GlobalPlayerContainer.Instance.TeamSelection);
			}
		}

		public void AddRandomPowerup() {
			if (!photonView.isMine)
				return;
			Powerup[] possiblePowerups = GetUsablePowerups();
			photonView.RPC("AddPowerup", PhotonTargets.All, possiblePowerups[Random.Range(0, possiblePowerups.Length)]);
		}

		public bool HasPowerup(Powerup p) {
			return powerupDictionary.ContainsKey(p) && Time.time - powerupDictionary[p] < powerupDuration;
		}

		protected abstract Powerup[] GetUsablePowerups();

		[PunRPC]
		public void AddUpgrade(int numUpgrades) {
			numUpgrades += numUpgrades;
		}

		public int NumUpgrades() {
			return numUpgrades;
		}
	}
}
