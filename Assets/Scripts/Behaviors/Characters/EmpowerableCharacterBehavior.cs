using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Com.Tempest.Nightmare {

	public abstract class EmpowerableCharacterBehavior : Photon.PunBehaviour {

		public float powerupDuration = 60f;

		private Dictionary<Powerup, float> powerupDictionary;
        private int numUpgrades;
        private bool hasTouchedFirstBonfire;

        protected string playerName;
        protected Dictionary<TalentEnum, int> talentRanks;

        public virtual void Awake() {
			powerupDictionary = new Dictionary<Powerup, float>();
            if (photonView.isMine) {
                LoadTalents();
            } else {
                talentRanks = new Dictionary<TalentEnum, int>();
            }
		}

        protected abstract void LoadTalents();

		[PunRPC]
		public void AddPowerup(Powerup p) {
			powerupDictionary[p] = Time.time;
			if (photonView.isMine) {
				GeneratedGameManager manager = FindObjectOfType<GeneratedGameManager>();
				if (manager != null) {
					manager.DisplayAlert("You have been granted " + p.ToString(), true, PlayerStateContainer.Instance.TeamSelection);
				} else {
					FindObjectOfType<DemoSceneManager>().DisplayAlert("You have been granted " + p.ToString(), true, PlayerStateContainer.Instance.TeamSelection);
				}
			}
		}

		public void AddRandomPowerup() {
			if (!photonView.isMine)
				return;
			Powerup[] possiblePowerups = GetUsablePowerups();
			photonView.RPC("AddPowerup", PhotonTargets.All, possiblePowerups[UnityEngine.Random.Range(0, possiblePowerups.Length)]);
		}

		public bool HasPowerup(Powerup p) {
			float powerupExtension = talentRanks[TalentEnum.CHEST_DURATION] * 5f;
			return powerupDictionary.ContainsKey(p) && Time.time - powerupDictionary[p] < powerupDuration + powerupExtension;
		}

		protected abstract Powerup[] GetUsablePowerups();

		public void AddUpgrade() {
			numUpgrades++;
			GeneratedGameManager manager = FindObjectOfType<GeneratedGameManager>();
			string message = PlayerStateContainer.Instance.TeamSelection == PlayerStateContainer.NIGHTMARE ? "Your attack has been upgraded." : "Your movement has been upgraded.";
			if (manager != null) {
				manager.DisplayAlert(message, true, PlayerStateContainer.Instance.TeamSelection);
			} else {
				FindObjectOfType<DemoSceneManager>().DisplayAlert(message, true, PlayerStateContainer.Instance.TeamSelection);
			}
		}
		
		// This should be used to call the ReceiveTalents RPC with this character's version of each talent in the talent tree.
		public void SendTalentsToNetwork() {
            foreach (TalentEnum talent in Enum.GetValues(typeof(TalentEnum))) {
                photonView.RPC("ReceiveTalent", PhotonTargets.All, talent, talentRanks[talent]);
            }
        }

		[PunRPC]
		public void ReceiveTalent(TalentEnum talent, int rank) {
            talentRanks[talent] = rank;
		}

		public int GetBonfireSpeed() {
			return talentRanks[TalentEnum.BONFIRE_SPEED];
		}

        public int GetFirstBonfireRank() {
            return talentRanks[TalentEnum.FIRST_BONFIRE_SPEED];
        }

        public int GetChestLocatorRank() {
            return talentRanks[TalentEnum.CHEST_LOCATOR];
        }

        public int GetMirrorActivationRank() {
            return talentRanks[TalentEnum.MIRROR_ACTIVATION];
        }

        public int GetMirrorFadeRank() {
            return talentRanks[TalentEnum.MIRROR_FADE_DELAY];
        }

        public int GetPortalNotificationRank() {
            return talentRanks[TalentEnum.PORTAL_NOTIFICATIONS];
        }

		public float GetNumUpgrades() {
			float upgradeModifier = 1.0f + (0.05f * talentRanks[TalentEnum.UPGRADE_EFFECTIVENESS]);
			return numUpgrades * upgradeModifier;
		}

		// Returns the modifier that should be used when factoring in upgrades.
		// Output will be between min and max values.
		public float GetSigmoidUpgradeMultiplier(float minValue, float maxValue) {
			float upgrades = GetNumUpgrades() / 10f;
			float sigmoid = ((1 / (1 + (Mathf.Exp(upgrades * -1f)))) - 0.5f) * 2f;
			float range = maxValue - minValue;
			return minValue + (range * sigmoid);
		}

		public int GetUnmodifiedUpgrades() {
			return numUpgrades;
		}

		public void SendNameToNetwork() {
			string name = PhotonNetwork.playerName;
			photonView.RPC("ReceiveName", PhotonTargets.Others, name);
			ReceiveName(name);
		}

		[PunRPC]
		public void ReceiveName(string name) {
			playerName = name;
			transform.Find("NameCanvas").transform.Find("NameText").GetComponent<Text>().text = name;
		}
	}
}
